using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Infrastructure.Persistence.Context;
using Farol.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.Infrastructure.Persistence.Seed;

/// <summary>
/// Inicialização do banco: migrações, perfis, estrutura de referência e conjunto demonstrativo.
/// O administrador inicial só é criado a partir de FAROL_ADMIN_EMAIL/FAROL_ADMIN_SENHA — nunca com senha fixa.
/// </summary>
public class Semeador(
    FarolDbContext db,
    RoleManager<IdentityRole<Guid>> perfis,
    UserManager<Usuario> usuarios,
    IConfiguration configuracao,
    IOptions<FarolOptions> opcoes,
    IServiceProvider servicos,
    ILogger<Semeador> logger)
{
    public async Task ExecutarAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        foreach (var perfil in Perfis.Todos)
            if (!await perfis.RoleExistsAsync(perfil))
                await perfis.CreateAsync(new IdentityRole<Guid>(perfil));

        await CriarOperadorUnicoAsync(ct);

        if (!await db.Criterios.AnyAsync(ct))
        {
            await SemearReferenciaAsync(ct);
            logger.LogInformation("Estrutura de referência e pontuação demonstrativa carregadas.");
        }

        await CriarAdministradorInicialAsync(ct);

        if (opcoes.Value.Demo.Habilitado && !await db.Municipios.AnyAsync(ct))
        {
            var demo = ActivatorUtilities.CreateInstance<SemeadorDemonstrativo>(servicos);
            await demo.ExecutarAsync(ct);
        }
    }

    private async Task SemearReferenciaAsync(CancellationToken ct)
    {
        var agora = DateTimeOffset.UtcNow;

        var quals = DadosReferencia.Qualificacoes.Select(q => new Qualificacao { Codigo = q.Codigo, Nome = q.Nome }).ToList();
        db.Qualificacoes.AddRange(quals);
        db.Recursos.AddRange(DadosReferencia.Recursos.Select(r => new Recurso { Codigo = r.Codigo, Nome = r.Nome }));
        await db.SaveChangesAsync(ct);

        var ordem = 0;
        foreach (var t in DadosReferencia.Tipos)
        {
            db.TiposOcorrencia.Add(new TipoOcorrencia
            {
                Codigo = t.Codigo, Nome = t.Nome, TipoManutencaoSugerido = t.Sugerido, Ordem = ++ordem,
                Qualificacoes = t.Qualificacoes.Select(q => new TipoOcorrenciaQualificacao { QualificacaoId = quals.First(x => x.Codigo == q).Id }).ToList(),
            });
        }
        ordem = 0;
        foreach (var c in DadosReferencia.Classes)
            db.ClassesCliente.Add(new ClasseCliente { Codigo = c.Codigo, Nome = c.Nome, Essencial = c.Essencial, Ordem = ++ordem });

        var prioridades = DadosReferencia.Prioridades.Select(p => new Prioridade
        {
            Codigo = p.Codigo, Nome = p.Nome, Descricao = p.Descricao, Rank = p.Rank, Cor = p.Cor, Critica = p.Critica,
            PrazoTriagemMin = p.Triagem, PrazoDespachoMin = p.Despacho, PrazoInicioMin = p.Inicio, PrazoRestabelecimentoMin = p.Restabelecimento,
            PrazoConclusaoMin = p.Conclusao, Calendario = p.Calendario, UnidadePrazo = p.Calendario == CalendarioPrazo.Util ? "HORAS" : "MINUTOS",
            TratamentoCritico = p.Critica ? "Acionar supervisor de plantão e seguir o procedimento de emergência vigente." : null,
            Escalonamento = "Ao vencer o prazo de despacho, notificar o supervisor da cidade.",
            VigenciaInicio = agora, Demonstrativa = true,
        }).ToList();
        db.Prioridades.AddRange(prioridades);
        db.Feriados.AddRange(DadosReferencia.FeriadosNacionais(agora.Year - 1, 3));
        await db.SaveChangesAsync(ct);

        // Critérios fixos com opções; tipos e classes viram opções dos critérios correspondentes.
        ordem = 0;
        var pontosDemo = new Dictionary<OpcaoCriterio, (int Pontos, bool Requer)>();
        var criterios = new List<Criterio>();
        foreach (var s in DadosReferencia.Criterios)
        {
            var criterio = new Criterio
            {
                Codigo = s.Codigo, Nome = s.Nome, Descricao = s.Descricao, RegraAplicacao = s.Regra, Tipo = TipoCriterio.Fixo,
                Agregacao = s.Agregacao, MultiplaEscolha = s.Multipla, Ordem = ++ordem, CriadoEm = agora, AlteradoEm = agora,
            };
            var k = 0;
            IEnumerable<DadosReferencia.OpcaoSemente> sementes = s.Codigo switch
            {
                C.TipoOcorrencia => DadosReferencia.Tipos.Select(t => new DadosReferencia.OpcaoSemente(t.Codigo, t.Nome, t.PontosDemo)),
                C.ClasseCliente => DadosReferencia.Classes.Select(c => new DadosReferencia.OpcaoSemente(c.Codigo, c.Nome, c.PontosDemo))
                    .Append(new DadosReferencia.OpcaoSemente(C.ClasseNaoIdentificada, "Não identificada", DadosReferencia.PontosClasseNaoIdentificada, true, true)),
                _ => s.Opcoes,
            };
            foreach (var o in sementes)
            {
                var opcao = new OpcaoCriterio { Codigo = o.Codigo, Rotulo = o.Rotulo, Ordem = ++k, RepresentaDesconhecido = o.Desconhecido };
                criterio.Opcoes.Add(opcao);
                pontosDemo[opcao] = (o.PontosDemo, o.RequerConfirmacao);
            }
            criterios.Add(criterio);
        }
        db.Criterios.AddRange(criterios);
        await db.SaveChangesAsync(ct);

        foreach (var r in DadosReferencia.Regras)
        {
            db.RegrasPrecedencia.Add(new RegraPrecedencia
            {
                Codigo = r.Codigo, Nome = r.Nome, Descricao = r.Descricao, Condicoes = r.Condicoes.ToList(), NivelPrecedencia = r.Nivel,
                PrioridadeMinimaId = prioridades.First(p => p.Codigo == r.PrioridadeMinima).Id, Demonstrativa = true, AlteradaEm = agora,
            });
        }

        // Versão 1 global: demonstrativa, publicada, vigente desde já. O aviso "não oficial" acompanha enquanto valer.
        db.VersoesPontuacao.Add(new VersaoPontuacao
        {
            Numero = 1, Status = StatusVersaoPontuacao.Publicada, Demonstrativa = true, VigenciaInicio = agora.AddDays(-30),
            Aplicacao = AplicacaoVersao.SomenteNovas, Justificativa = "Conjunto demonstrativo inicial — não oficial.",
            AutorNome = "Sistema (demonstrativo)", PublicadaPorNome = "Sistema (demonstrativo)", PublicadaEm = agora, CriadaEm = agora, AlteradaEm = agora,
            Criterios = criterios.Select(c => new VersaoCriterio { CriterioId = c.Id, Habilitado = true }).ToList(),
            Pontos = pontosDemo.Select(p => new PontoOpcao
            {
                OpcaoId = p.Key.Id, Pontos = p.Value.Pontos, RequerConfirmacao = p.Value.Requer, Origem = OrigemPontos.Demonstrativo,
                AlteradoPorNome = "Sistema (demonstrativo)", AlteradoEm = agora,
            }).ToList(),
            Faixas = DadosReferencia.Prioridades.Select(p => new FaixaPrioridade
            {
                PrioridadeId = prioridades.First(x => x.Codigo == p.Codigo).Id, Minimo = p.Minimo, Maximo = p.Maximo,
            }).ToList(),
        });
        await db.SaveChangesAsync(ct);
    }

    private async Task CriarAdministradorInicialAsync(CancellationToken ct)
    {
        var email = configuracao["FAROL_ADMIN_EMAIL"];
        var senha = configuracao["FAROL_ADMIN_SENHA"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha)) return;
        if (await usuarios.FindByEmailAsync(email) is not null) return;

        var admin = new Usuario { UserName = email, Email = email, EmailConfirmed = true, Nome = "Administrador" };
        var r = await usuarios.CreateAsync(admin, senha);
        if (!r.Succeeded)
        {
            logger.LogError("Administrador inicial não criado: {Erros}", string.Join("; ", r.Errors.Select(e => e.Description)));
            return;
        }
        await usuarios.AddToRoleAsync(admin, Perfis.Administrador);
        logger.LogInformation("Administrador inicial criado para {Email}. Troque a senha após o primeiro acesso.", email);
    }

    /// <summary>Cria a identidade técnica usada pelo modo local sem tela de login.</summary>
    private async Task CriarOperadorUnicoAsync(CancellationToken ct)
    {
        if (!opcoes.Value.ModoOperadorUnico || await usuarios.FindByIdAsync(UsuarioAtual.OperadorUnicoId.ToString()) is not null) return;

        var operador = new Usuario
        {
            Id = UsuarioAtual.OperadorUnicoId,
            UserName = "operador.local@farol",
            Email = "operador.local@farol",
            EmailConfirmed = true,
            Nome = "Operador do sistema",
            Ativo = true,
        };
        var resultado = await usuarios.CreateAsync(operador, "OperadorLocalSemLogin2026!");
        if (!resultado.Succeeded)
        {
            logger.LogError("Operador único não criado: {Erros}", string.Join("; ", resultado.Errors.Select(e => e.Description)));
            return;
        }
        await usuarios.AddToRoleAsync(operador, Perfis.Administrador);
    }
}
