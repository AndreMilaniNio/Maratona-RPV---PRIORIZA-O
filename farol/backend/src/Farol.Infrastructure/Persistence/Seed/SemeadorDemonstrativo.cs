using System.Globalization;
using System.Reflection;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Prioritization;
using Farol.Application.Services;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.ValueObjects;
using Farol.Infrastructure.Authentication;
using Farol.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.Infrastructure.Persistence.Seed;

/// <summary>
/// Conjunto DEMONSTRATIVO derivado da base fictícia "Experimento_Dados_Ficticios - BASE CLIENTES 1.xlsx"
/// (200 UCs da empresa EMP_TESTE). Nenhum dado real: municípios, CEPs, subestações, equipes e pessoas são fictícios.
/// Mapeamento: longitude ≥ -42,585 → Cerro Anil; &lt; -42,675 → Vale Turquesa; demais → Lumiara.
/// Bairro → localidade de 3 dígitos (1xx, 2xx, 3xx por cidade); TRxxxx → {localidade}xxxx; CIRCUITO → subestação.
/// </summary>
public class SemeadorDemonstrativo(
    FarolDbContext db,
    UserManager<Usuario> usuarios,
    IServiceProvider servicos,
    IOptions<FarolOptions> opcoes,
    ILogger<SemeadorDemonstrativo> logger)
{
    public const string RecursoCsv = "Farol.Infrastructure.Seed.base_clientes_ficticia.csv";

    private sealed record Linha(string Uc, string Nome, string Rua, string Casa, string Bairro, string Classe, bool Ativo,
        string Circuito, string Conjunto, string Trafo, string Medidor, string Fases, double Lat, double Lng);

    private sealed record CidadeDemo(string Nome, string Prefixo, string CodigoCep, int CentenaLocalidade, string? CepUnico);

    private static readonly CidadeDemo Lumiara = new("Lumiara", "LUM", "001", 100, null);
    private static readonly CidadeDemo CerroAnil = new("Cerro Anil", "CAN", "002", 200, "00200000");
    private static readonly CidadeDemo ValeTurquesa = new("Vale Turquesa", "VTQ", "003", 300, null);

    private static readonly string[] Bairros = ["Centro", "Bela Vista", "Jardim Europa", "Industrial", "Sao Jose"];

    private static CidadeDemo CidadeDe(Linha l) => l.Lng >= -42.585 ? CerroAnil : l.Lng < -42.675 ? ValeTurquesa : Lumiara;

    private static string NomeBairro(string b) => b == "Sao Jose" ? "São José" : b;

    public async Task ExecutarAsync(CancellationToken ct = default)
    {
        var linhas = LerBase();
        var agora = DateTimeOffset.UtcNow;

        // Municípios: centro e raio calculados a partir das UCs de cada um.
        var municipios = new Dictionary<CidadeDemo, Municipio>();
        foreach (var grupo in linhas.GroupBy(CidadeDe))
        {
            var lat = grupo.Average(l => l.Lat);
            var lng = grupo.Average(l => l.Lng);
            var centro = new Coordenadas(lat, lng);
            var raio = grupo.Max(l => new Coordenadas(l.Lat, l.Lng).DistanciaKm(centro)) + 1.5;
            var m = new Municipio
            {
                Nome = grupo.Key.Nome, Uf = "MG", Prefixo = grupo.Key.Prefixo, Latitude = Math.Round(lat, 6), Longitude = Math.Round(lng, 6),
                RaioKm = Math.Round(raio, 1), CepUnico = grupo.Key.CepUnico, Demonstrativo = true,
            };
            municipios[grupo.Key] = m;
            db.Municipios.Add(m);
        }
        await db.SaveChangesAsync(ct);

        // Localidades (bairro → código de 3 dígitos por cidade)
        var localidades = new Dictionary<(CidadeDemo, string), Localidade>();
        foreach (var chave in linhas.Select(l => (CidadeDe(l), l.Bairro)).Distinct())
        {
            var codigo = (chave.Item1.CentenaLocalidade + Array.IndexOf(Bairros, chave.Bairro) + 1).ToString("000", CultureInfo.InvariantCulture);
            var l = new Localidade { Codigo = codigo, Nome = $"{NomeBairro(chave.Bairro)} ({chave.Item1.Nome})", MunicipioId = municipios[chave.Item1].Id, Demonstrativo = true };
            localidades[chave] = l;
            db.Localidades.Add(l);
        }

        // Subestações (circuitos) e conjuntos
        var subestacoes = new Dictionary<string, Subestacao>();
        foreach (var g in linhas.GroupBy(l => l.Circuito))
        {
            var cidade = g.GroupBy(CidadeDe).OrderByDescending(x => x.Count()).First().Key;
            var bairro = g.GroupBy(l => l.Bairro).OrderByDescending(x => x.Count()).First().Key;
            var s = new Subestacao
            {
                Codigo = g.Key, Nome = $"SE {g.Key} (fictícia)", Local = $"{NomeBairro(bairro)}, {cidade.Nome}", MunicipioId = municipios[cidade].Id,
                Latitude = Math.Round(g.Average(l => l.Lat) + 0.0015, 6), Longitude = Math.Round(g.Average(l => l.Lng) + 0.0015, 6), Demonstrativo = true,
                Conjuntos = g.Select(l => l.Conjunto).Distinct().Order().Select(c => new ConjuntoEletrico { Numero = c, Demonstrativo = true }).ToList(),
            };
            subestacoes[g.Key] = s;
            db.Subestacoes.Add(s);
        }

        // CEPs fictícios: prefixo "00" nunca é um CEP real. Cerro Anil tem CEP único.
        var ceps = new Dictionary<(CidadeDemo, string, string), string>();
        foreach (var l in linhas)
        {
            var cidade = CidadeDe(l);
            var chave = (cidade, l.Bairro, l.Rua);
            if (ceps.ContainsKey(chave)) continue;
            var cep = cidade.CepUnico ?? $"{cidade.CodigoCep}{Array.IndexOf(Bairros, l.Bairro) + 1}{int.Parse(l.Rua.Replace("Rua ", ""), CultureInfo.InvariantCulture):0000}";
            ceps[chave] = cep;
            if (cidade.CepUnico is null && !ceps.Where(x => x.Key != chave).Any(x => x.Value == cep))
                db.Ceps.Add(new CepCadastrado { Cep = cep, Logradouro = l.Rua, Bairro = NomeBairro(l.Bairro), MunicipioId = municipios[cidade].Id, Demonstrativo = true });
        }
        await db.SaveChangesAsync(ct);

        // Transformadores e UCs
        var classes = await db.ClassesCliente.ToDictionaryAsync(c => c.Codigo, ct);
        var transformadores = new Dictionary<string, Transformador>();
        foreach (var l in linhas)
        {
            var cidade = CidadeDe(l);
            var localidade = localidades[(cidade, l.Bairro)];
            var numero = localidade.Codigo + l.Trafo.Replace("TR", "");
            if (!transformadores.TryGetValue(numero, out var t))
            {
                var sub = subestacoes[l.Circuito];
                t = new Transformador
                {
                    NumeroCompleto = numero, CodigoLocalidade = localidade.Codigo, NumeroLocal = numero[3..], LocalidadeId = localidade.Id,
                    SubestacaoId = sub.Id, ConjuntoId = sub.Conjuntos.First(c => c.Numero == l.Conjunto).Id, MunicipioId = municipios[cidade].Id,
                    Latitude = l.Lat, Longitude = l.Lng, Demonstrativo = true,
                };
                transformadores[numero] = t;
                db.Transformadores.Add(t);
            }
            db.UnidadesConsumidoras.Add(new UnidadeConsumidora
            {
                Numero = l.Uc, ClienteNome = l.Nome, ClasseCliente = classes[l.Classe.ToUpperInvariant()],
                Situacao = l.Ativo ? SituacaoUnidade.Ligado : SituacaoUnidade.Desligado, Logradouro = l.Rua, NumeroImovel = l.Casa,
                Bairro = NomeBairro(l.Bairro), Cep = ceps[(cidade, l.Bairro, l.Rua)], MunicipioId = municipios[cidade].Id, Transformador = t,
                Latitude = l.Lat, Longitude = l.Lng, Medidor = l.Medidor, Fases = l.Fases, Demonstrativo = true,
            });
        }
        await db.SaveChangesAsync(ct);

        // UCs complementares de classes Essencial e Poder público (a base não as possui).
        var complementares = new (string Uc, string Nome, string Classe, string BaseUc, double DLat, double DLng)[]
        {
            ("1000001", "Hospital Municipal Fictício de Lumiara", "ESSENCIAL", linhas.First(l => CidadeDe(l) == Lumiara && l.Bairro == "Centro").Uc, 0.0007, 0.0006),
            ("1000002", "UPA Fictícia Bela Vista", "ESSENCIAL", linhas.First(l => CidadeDe(l) == Lumiara && l.Bairro == "Bela Vista").Uc, -0.0006, 0.0005),
            ("1000003", "Prefeitura Fictícia de Lumiara", "PODER_PUBLICO", linhas.Where(l => CidadeDe(l) == Lumiara && l.Bairro == "Centro").Skip(1).First().Uc, 0.0004, -0.0006),
            ("1000004", "Base Fictícia do Corpo de Bombeiros", "ESSENCIAL", linhas.First(l => CidadeDe(l) == Lumiara && l.Bairro == "Industrial").Uc, 0.0005, 0.0004),
            ("1000005", "Posto de Saúde Fictício de Cerro Anil", "ESSENCIAL", linhas.First(l => CidadeDe(l) == CerroAnil).Uc, 0.0006, 0.0006),
            ("1000006", "Estação Fictícia de Tratamento de Água", "ESSENCIAL", linhas.First(l => CidadeDe(l) == ValeTurquesa).Uc, -0.0007, 0.0004),
        };
        foreach (var c in complementares)
        {
            var baseUc = await db.UnidadesConsumidoras.AsNoTracking().FirstAsync(u => u.Numero == c.BaseUc, ct);
            db.UnidadesConsumidoras.Add(new UnidadeConsumidora
            {
                Numero = c.Uc, ClienteNome = c.Nome, ClasseClienteId = classes[c.Classe].Id, Situacao = SituacaoUnidade.Ligado,
                Logradouro = baseUc.Logradouro, NumeroImovel = "S/N", Bairro = baseUc.Bairro, Cep = baseUc.Cep, MunicipioId = baseUc.MunicipioId,
                TransformadorId = baseUc.TransformadorId, Latitude = Math.Round(baseUc.Latitude!.Value + c.DLat, 6),
                Longitude = Math.Round(baseUc.Longitude!.Value + c.DLng, 6), Demonstrativo = true,
            });
        }
        await db.SaveChangesAsync(ct);

        var equipes = await SemearEquipesAsync(municipios, agora, ct);
        var usuariosDemo = await SemearUsuariosAsync(municipios, equipes, ct);
        await SemearOrdensAsync(municipios, usuariosDemo, equipes, ct);

        logger.LogInformation("Conjunto demonstrativo carregado: {Ucs} UCs, {Trafos} transformadores, {Subs} subestações em 3 municípios fictícios.",
            linhas.Count + complementares.Length, transformadores.Count, subestacoes.Count);
    }

    private async Task<Dictionary<string, Equipe>> SemearEquipesAsync(Dictionary<CidadeDemo, Municipio> m, DateTimeOffset agora, CancellationToken ct)
    {
        var quals = await db.Qualificacoes.ToDictionaryAsync(q => q.Codigo, ct);
        var recursos = await db.Recursos.ToDictionaryAsync(r => r.Codigo, ct);

        Equipe Nova(string codigo, string nome, CidadeDemo cidade, double dLat, double dLng, StatusEquipe status, string[] q, string[] r, string[] pessoas, CidadeDemo[]? adicionais = null) =>
            new()
            {
                Codigo = codigo, Nome = nome, MunicipioBaseId = m[cidade].Id, Status = status, Capacidade = 1,
                Latitude = Math.Round(m[cidade].Latitude!.Value + dLat, 6), Longitude = Math.Round(m[cidade].Longitude!.Value + dLng, 6),
                LocalizacaoAtualizadaEm = agora.AddMinutes(-12), OrigemLocalizacao = OrigemLocalizacaoEquipe.Demonstrativa, Demonstrativa = true,
                Qualificacoes = q.Select(x => new EquipeQualificacao { QualificacaoId = quals[x].Id }).ToList(),
                Recursos = r.Select(x => new EquipeRecurso { RecursoId = recursos[x].Id }).ToList(),
                Integrantes = pessoas.Select((p, i) => new Integrante { Nome = p, Matricula = $"DEMO-{codigo}-{i + 1}", Funcao = i == 0 ? "Encarregado" : "Eletricista" }).ToList(),
                MunicipiosAdicionais = (adicionais ?? []).Select(a => new EquipeMunicipio { MunicipioId = m[a].Id }).ToList(),
            };

        var lista = new[]
        {
            Nova("LUM-01", "Linha Viva Alfa", Lumiara, 0.006, -0.004, StatusEquipe.Disponivel, ["NR10", "SEP", "TRABALHO_ALTURA", "LINHA_VIVA"], ["CAMINHAO_CESTO", "KIT_ATERRAMENTO"], ["Carlos Oliveira", "Rafael Mendes"]),
            Nova("LUM-02", "Poda e Rede Beta", Lumiara, -0.008, 0.005, StatusEquipe.Disponivel, ["NR10", "PODA", "TRABALHO_ALTURA"], ["MOTOSSERRA", "CAMINHAO_CESTO"], ["Juliana Costa", "Lucas Ferreira"]),
            Nova("LUM-03", "Emergência Gama", Lumiara, 0.002, 0.009, StatusEquipe.Disponivel, ["NR10", "SEP", "TRABALHO_ALTURA"], ["KIT_ATERRAMENTO", "GERADOR"], ["Pedro Santos", "Beatriz Rocha"]),
            Nova("LUM-04", "Especializada Delta", Lumiara, -0.003, -0.011, StatusEquipe.Disponivel, ["NR10", "SEP", "TRABALHO_ALTURA", "EQUIPE_ESPECIALIZADA", "OPERACAO_MANOBRA"], ["GUINDAUTO", "TRANSFORMADOR_RESERVA", "CAMINHAO_CESTO"], ["Maria Souza", "Thiago Barros", "Camila Duarte"]),
            Nova("CAN-01", "Cerro Anil Plantão", CerroAnil, 0.003, 0.002, StatusEquipe.Disponivel, ["NR10", "SEP", "TRABALHO_ALTURA"], ["CAMINHAO_CESTO"], ["Ana Lima", "Gustavo Prado"]),
            Nova("CAN-02", "Cerro Anil Apoio", CerroAnil, -0.004, -0.003, StatusEquipe.EmPausa, ["NR10", "SEP"], ["KIT_ATERRAMENTO"], ["Joao Silva", "Renata Alves"]),
            Nova("VTQ-01", "Vale Turquesa Norte", ValeTurquesa, 0.010, 0.006, StatusEquipe.Disponivel, ["NR10", "SEP", "TRABALHO_ALTURA", "PODA"], ["CAMINHAO_CESTO", "MOTOSSERRA"], ["Felipe Nunes", "Larissa Melo"]),
            Nova("VTQ-02", "Vale Turquesa Sul", ValeTurquesa, -0.012, 0.004, StatusEquipe.Disponivel, ["NR10", "SEP", "OPERACAO_MANOBRA"], ["GERADOR", "KIT_ATERRAMENTO"], ["Diego Ramos", "Patrícia Lopes"], [Lumiara]),
        };
        db.Equipes.AddRange(lista);
        await db.SaveChangesAsync(ct);
        foreach (var e in lista)
            db.LocalizacoesEquipe.Add(new LocalizacaoEquipe { EquipeId = e.Id, Latitude = e.Latitude!.Value, Longitude = e.Longitude!.Value, Origem = OrigemLocalizacaoEquipe.Demonstrativa, RegistradaEm = e.LocalizacaoAtualizadaEm!.Value });
        await db.SaveChangesAsync(ct);
        return lista.ToDictionary(e => e.Codigo);
    }

    private async Task<Dictionary<string, Usuario>> SemearUsuariosAsync(Dictionary<CidadeDemo, Municipio> m, Dictionary<string, Equipe> equipes, CancellationToken ct)
    {
        var senha = opcoes.Value.Demo.SenhaUsuarios;
        var resultado = new Dictionary<string, Usuario>();
        if (string.IsNullOrWhiteSpace(senha))
        {
            logger.LogWarning("Farol:Demo:SenhaUsuarios não definida: usuários demonstrativos não foram criados.");
            return resultado;
        }

        var todas = m.Values.Select(x => x.Id).ToArray();
        var definicoes = new (string Email, string Nome, string Perfil, int[] Cidades, string? Equipe)[]
        {
            ("admin@farol.demo", "Administrador Demo", Perfis.Administrador, todas, null),
            ("supervisor@farol.demo", "Sofia Supervisora (demo)", Perfis.Supervisor, todas, null),
            ("despachante@farol.demo", "Diego Despachante (demo)", Perfis.Despachante, [m[Lumiara].Id, m[CerroAnil].Id], null),
            ("atendente@farol.demo", "Alice Atendente (demo)", Perfis.Atendente, [m[Lumiara].Id, m[CerroAnil].Id], null),
            ("atendente.vtq@farol.demo", "Vitor Atendente VTQ (demo)", Perfis.Atendente, [m[ValeTurquesa].Id], null),
            ("despachante.vtq@farol.demo", "Valéria Despachante VTQ (demo)", Perfis.Despachante, [m[ValeTurquesa].Id], null),
            ("chave@farol.demo", "Carla Usuária Chave (demo)", Perfis.UsuarioChave, todas, null),
            ("chave2@farol.demo", "Caio Usuário Chave substituto (demo)", Perfis.UsuarioChave, todas, null),
            ("equipe.lum03@farol.demo", "Equipe LUM-03 (demo)", Perfis.EquipeCampo, [m[Lumiara].Id], "LUM-03"),
        };
        foreach (var d in definicoes)
        {
            var u = new Usuario
            {
                UserName = d.Email, Email = d.Email, EmailConfirmed = true, Nome = d.Nome, Demonstrativo = true,
                MunicipioPreferidoId = d.Cidades.Length == 1 ? d.Cidades[0] : m[Lumiara].Id,
                EquipeId = d.Equipe is null ? null : equipes[d.Equipe].Id,
                Municipios = d.Cidades.Select(c => new UsuarioMunicipio { MunicipioId = c }).ToList(),
            };
            var r = await usuarios.CreateAsync(u, senha);
            if (!r.Succeeded) throw new InvalidOperationException($"Usuário demo {d.Email}: {string.Join("; ", r.Errors.Select(e => e.Description))}");
            await usuarios.AddToRoleAsync(u, d.Perfil);
            resultado[d.Email] = u;
        }
        return resultado;
    }

    /// <summary>
    /// Atualiza somente a conta administrativa demonstrativa em bancos já
    /// existentes, sem apagar dados para alterar a credencial de apresentação.
    /// </summary>
    public async Task GarantirSenhaAdministradorAsync(CancellationToken ct)
    {
        var senha = opcoes.Value.Demo.SenhaUsuarios;
        if (string.IsNullOrWhiteSpace(senha)) return;

        var admin = await usuarios.FindByEmailAsync("admin@farol.demo");
        if (admin is null || !admin.Demonstrativo) return;

        var desbloqueio = await usuarios.SetLockoutEndDateAsync(admin, null);
        var falhas = await usuarios.ResetAccessFailedCountAsync(admin);
        if (!desbloqueio.Succeeded || !falhas.Succeeded)
        {
            var erros = desbloqueio.Errors.Concat(falhas.Errors).Select(e => e.Description);
            logger.LogWarning("Bloqueio do administrador demonstrativo não foi removido: {Erros}", string.Join("; ", erros));
        }

        var token = await usuarios.GeneratePasswordResetTokenAsync(admin);
        var resultado = await usuarios.ResetPasswordAsync(admin, token, senha);
        if (!resultado.Succeeded)
            logger.LogWarning("Senha do administrador demonstrativo não foi atualizada: {Erros}", string.Join("; ", resultado.Errors.Select(e => e.Description)));
    }

    /// <summary>Cenários da seção 21, criados pelos mesmos serviços da produção.</summary>
    private async Task SemearOrdensAsync(Dictionary<CidadeDemo, Municipio> m, Dictionary<string, Usuario> u, Dictionary<string, Equipe> equipes, CancellationToken ct)
    {
        if (!u.TryGetValue("supervisor@farol.demo", out var supervisor)) return;

        using var escopo = servicos.CreateScope();
        var sp = escopo.ServiceProvider;
        sp.GetRequiredService<UsuarioAtual>().Personificar(supervisor.Id, supervisor.Nome, Perfis.Supervisor);
        var solicitacoes = sp.GetRequiredService<SolicitacaoService>();
        var despacho = sp.GetRequiredService<DespachoService>();
        var ctx = sp.GetRequiredService<FarolDbContext>();
        var classificacao = sp.GetRequiredService<ServicoClassificacao>();

        var tipos = await ctx.TiposOcorrencia.ToDictionaryAsync(t => t.Codigo, t => t.Id, ct);
        var classes = await ctx.ClassesCliente.ToDictionaryAsync(c => c.Codigo, c => c.Id, ct);

        async Task<UnidadeConsumidora> Uc(string numero) => await ctx.UnidadesConsumidoras.AsNoTracking()
            .Include(x => x.Transformador).FirstAsync(x => x.Numero == numero, ct);
        async Task<UnidadeConsumidora> UcDe(CidadeDemo cidade, string classe, int pular = 0) => await ctx.UnidadesConsumidoras.AsNoTracking()
            .Include(x => x.Transformador).Where(x => x.MunicipioId == m[cidade].Id && x.ClasseCliente!.Codigo == classe && x.Numero.Length == 6)
            .OrderBy(x => x.Numero).Skip(pular).FirstAsync(ct);

        NovaSolicitacaoRequest Base(UnidadeConsumidora uc, string tipo, string descricao, ImpactoInput impacto, TipoManutencao manutencao = TipoManutencao.Corretiva,
            bool comCoordenadas = true, string canal = "Telefone") => new()
        {
            ChaveIdempotencia = Guid.NewGuid(), MunicipioId = uc.MunicipioId, Canal = Enum.Parse<CanalEntrada>(canal), Origem = "Cenário demonstrativo",
            Ucs = [uc.Numero], ClassesIds = [uc.ClasseClienteId], TipoManutencao = manutencao, TipoOcorrenciaId = tipos[tipo], Descricao = descricao,
            Localizacao = new LocalizacaoInput
            {
                Logradouro = uc.Logradouro, Numero = uc.NumeroImovel, Bairro = uc.Bairro, Cep = uc.Cep,
                Latitude = comCoordenadas ? uc.Latitude : null, Longitude = comCoordenadas ? uc.Longitude : null,
                OrigemCoordenada = comCoordenadas ? OrigemCoordenada.Informada : null, PrecisaoMetros = comCoordenadas ? 15 : null,
            },
            Rede = new RedeInput { SubestacaoId = uc.Transformador?.SubestacaoId, ConjuntoId = uc.Transformador?.ConjuntoId, TransformadorNumero = uc.Transformador?.NumeroCompleto },
            Impacto = impacto with { SituacaoCliente = uc.Situacao == SituacaoUnidade.Ligado ? C.Situacao.Ligado : C.Situacao.Desligado },
        };

        var hospital = await Uc("1000001");
        await solicitacoes.CriarAsync(Base(hospital, "TRANSFORMADOR_DANIFICADO",
            "Hospital fictício informa queda total de energia após estrondo no transformador em frente à entrada principal. Gerador não está funcionando.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De101A500, UcsAfetadas = C.Faixa.De11A100, ServicoEssencial = C.Essencial.Hospital,
                CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional], CondicaoFornecimento = C.Fornecimento.Total, FonteReserva = C.Confirmacao.Nao,
                Redundancia = C.Confirmacao.Nao, EquipeEspecializada = C.Confirmacao.Sim, EquipamentoAfetado = C.Equipamento.Transformador,
                Abrangencia = C.Abrange.Bairro, NivelRede = C.Rede.Transformador,
            }), ct);

        var caboUc = await UcDe(Lumiara, "RESIDENCIAL", 2);
        await solicitacoes.CriarAsync(Base(caboUc, "CABO_ROMPIDO",
            "Morador relata cabo caído sobre a calçada soltando faíscas; pedestres desviando pela rua.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De11A100, UcsAfetadas = C.Faixa.De11A100, CondicoesSeguranca = [C.Seguranca.CaboEnergizado, C.Seguranca.RiscoCirculacao],
                CondicaoFornecimento = C.Fornecimento.Parcial, EquipamentoAfetado = C.Equipamento.Cabo, Abrangencia = C.Abrange.RuaTrecho, NivelRede = C.Rede.Transformador,
            }, canal: "Presencial"), ct);

        var residencial = await UcDe(Lumiara, "RESIDENCIAL", 5);
        var r3 = await solicitacoes.CriarAsync(Base(residencial, "INTERRUPCAO_TOTAL",
            "Cliente informa que está sem energia em casa desde a manhã; vizinhos têm energia.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.Ate10, QuantidadePessoas = 4, UcsAfetadas = C.Faixa.Ate10, QuantidadeUcs = 1,
                CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional], CondicaoFornecimento = C.Fornecimento.Total, Abrangencia = C.Abrange.PontoUnico,
                NivelRede = C.Rede.RamalUc, EquipeEspecializada = C.Confirmacao.Nao,
            }), ct);

        var galho = await UcDe(Lumiara, "COMERCIAL", 1);
        var r4 = await solicitacoes.CriarAsync(Base(galho, "GALHO_NA_REDE",
            "Galho grande apoiado na rede após o vento da tarde; parte da rua sem energia.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De11A100, UcsAfetadas = C.Faixa.De11A100, CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional],
                CondicaoFornecimento = C.Fornecimento.Parcial, DuracaoEstimadaMin = 90, EquipamentoAfetado = C.Equipamento.Cabo,
                Abrangencia = C.Abrange.RuaTrecho, NivelRede = C.Rede.Transformador,
            }), ct);

        var prev = await UcDe(Lumiara, "INDUSTRIAL", 0);
        await solicitacoes.CriarAsync(Base(prev, "INSPECAO_PREVENTIVA",
            "Inspeção termográfica programada no conjunto que atende o distrito industrial.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.Ate10, UcsAfetadas = C.Faixa.Ate10, CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional],
                CondicaoFornecimento = C.Fornecimento.SemInterrupcao, Redundancia = C.Confirmacao.Sim, EquipamentoAfetado = C.Equipamento.Outro,
                Abrangencia = C.Abrange.PontoUnico, NivelRede = C.Rede.Conjunto, EquipeEspecializada = C.Confirmacao.Nao, DataLimite = DateTimeOffset.UtcNow.AddDays(5),
            }, TipoManutencao.Preventiva, canal: "SistemaInterno"), ct);

        var prev2 = await UcDe(Lumiara, "RURAL", 0);
        await solicitacoes.CriarAsync(Base(prev2, "MANUTENCAO_PROGRAMADA",
            "Troca preventiva de para-raios com prazo regulatório vencendo amanhã cedo.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.Ate10, UcsAfetadas = C.Faixa.Ate10, CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional],
                CondicaoFornecimento = C.Fornecimento.SemInterrupcao, EquipamentoAfetado = C.Equipamento.Outro, Abrangencia = C.Abrange.PontoUnico,
                NivelRede = C.Rede.Transformador, EquipeEspecializada = C.Confirmacao.Nao, DataLimite = DateTimeOffset.UtcNow.AddHours(14),
            }, TipoManutencao.Preventiva, canal: "SistemaInterno"), ct);

        // Localização desconhecida: solicitante na rua, sem UC, sem coordenadas — só o CEP.
        await solicitacoes.CriarAsync(new NovaSolicitacaoRequest
        {
            ChaveIdempotencia = Guid.NewGuid(), MunicipioId = m[Lumiara].Id, Canal = CanalEntrada.Telefone, Origem = "Cenário demonstrativo",
            UcNaoInformada = true, MotivoUcNaoInformada = "Solicitante de passagem, fora da própria residência.",
            TipoManutencao = TipoManutencao.Corretiva, TipoOcorrenciaId = tipos["FALHA_EQUIPAMENTO"],
            Descricao = "Pessoa de passagem relata zumbido forte em equipamento no alto de um poste, sem saber o endereço exato.",
            Localizacao = new LocalizacaoInput { Cep = hospital.Cep, PontoReferencia = "Perto de uma praça com coreto" },
            Impacto = new ImpactoInput { CondicoesSeguranca = [C.Seguranca.Desconhecida] },
        }, ct);

        var vencida = await UcDe(Lumiara, "COMERCIAL", 4);
        var r8 = await solicitacoes.CriarAsync(Base(vencida, "INTERRUPCAO_PARCIAL",
            "Comércio relata oscilação e falta de uma fase desde o início da manhã.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De101A500, UcsAfetadas = C.Faixa.De11A100, CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional],
                CondicaoFornecimento = C.Fornecimento.Parcial, EquipamentoAfetado = C.Equipamento.NaoIdentificado, Abrangencia = C.Abrange.PontoUnico,
                NivelRede = C.Rede.RamalUc,
            }), ct);

        var poste = await UcDe(CerroAnil, "RURAL", 0);
        await solicitacoes.CriarAsync(Base(poste, "POSTE_DANIFICADO",
            "Caminhão bateu em poste na estrada vicinal; poste inclinado com risco de queda.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De11A100, UcsAfetadas = C.Faixa.De11A100, CondicoesSeguranca = [C.Seguranca.EstruturaQueda, C.Seguranca.RiscoCirculacao],
                CondicaoFornecimento = C.Fornecimento.Desconhecida, EquipamentoAfetado = C.Equipamento.Poste, Abrangencia = C.Abrange.RuaTrecho,
            }), ct);

        var posto = await Uc("1000005");
        await solicitacoes.CriarAsync(Base(posto, "INTERRUPCAO_TOTAL",
            "Posto de saúde fictício sem energia; geladeira de vacinas com gerador funcionando.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De101A500, UcsAfetadas = C.Faixa.De101A500, ServicoEssencial = C.Essencial.Hospital,
                CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional], CondicaoFornecimento = C.Fornecimento.Total, FonteReserva = C.Confirmacao.Sim,
                EquipamentoAfetado = C.Equipamento.NaoIdentificado, Abrangencia = C.Abrange.Bairro, NivelRede = C.Rede.Conjunto,
            }), ct);

        var curto = await UcDe(ValeTurquesa, "INDUSTRIAL", 0);
        await solicitacoes.CriarAsync(Base(curto, "CURTO_INCENDIO",
            "Funcionários de indústria fictícia relatam fumaça e chamas na cruzeta do poste em frente ao portão.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.De11A100, UcsAfetadas = C.Faixa.Ate10, CondicoesSeguranca = [C.Seguranca.Incendio],
                CondicaoFornecimento = C.Fornecimento.Total, EquipamentoAfetado = C.Equipamento.Chave, Abrangencia = C.Abrange.PontoUnico, NivelRede = C.Rede.Transformador,
            }), ct);

        var agua = await Uc("1000006");
        await solicitacoes.CriarAsync(Base(agua, "INTERRUPCAO_PARCIAL",
            "Estação fictícia de tratamento de água operando com uma bomba por falta de fase.",
            new ImpactoInput
            {
                PessoasAfetadas = C.Faixa.MilOuMais, UcsAfetadas = C.Faixa.Ate10, ServicoEssencial = C.Essencial.InfraestruturaCritica,
                CondicoesSeguranca = [C.Seguranca.SemRiscoAdicional], CondicaoFornecimento = C.Fornecimento.Parcial, FonteReserva = C.Confirmacao.Desconhecida,
                EquipamentoAfetado = C.Equipamento.NaoIdentificado, Abrangencia = C.Abrange.MultiplosBairros, NivelRede = C.Rede.Conjunto,
            }), ct);

        // OS vencida: a abertura recua 9 horas e os prazos são recalculados.
        var osVencida = await ctx.OrdensServico.Include(o => o.CondicoesSeguranca).Include(o => o.Classes).Include(o => o.RecursosNecessarios)
            .Include(o => o.RespostasPersonalizadas).FirstAsync(o => o.Id == r8.OsId, ct);
        osVencida.AbertaEm = osVencida.AbertaEm.AddHours(-9);
        await ctx.Solicitacoes.Where(s => s.Id == r8.SolicitacaoId).ExecuteUpdateAsync(s => s.SetProperty(x => x.RegistradaEm, osVencida.AbertaEm), ct);
        await classificacao.AplicarPrazosAsync(osVencida, ct);
        await ctx.SaveChangesAsync(ct);
        await classificacao.ReclassificarAsync(osVencida, MotivoClassificacao.Tempo, somenteSeMudou: true, ct: ct);

        // Ciclo de despacho em andamento para mostrar equipes ocupadas.
        var d3 = await despacho.DesignarAsync(r3.OsId, new DesignarRequest(equipes["LUM-03"].Id, null, false, false), ct);
        await despacho.AceiteAsync(d3.DespachoId, new EventoDespachoRequest("Equipe a caminho (demonstrativo).", null), ct);
        var d4 = await despacho.DesignarAsync(r4.OsId, new DesignarRequest(equipes["LUM-02"].Id, null, false, false), ct);
        await despacho.AceiteAsync(d4.DespachoId, new EventoDespachoRequest(null, null), ct);
        await despacho.InicioAsync(d4.DespachoId, new EventoDespachoRequest("Poda iniciada (demonstrativo).", null), ct);
    }

    private static List<Linha> LerBase()
    {
        using var stream = typeof(SemeadorDemonstrativo).Assembly.GetManifestResourceStream(RecursoCsv)
                           ?? throw new InvalidOperationException($"Recurso {RecursoCsv} não encontrado.");
        using var leitor = new StreamReader(stream);
        var linhas = new List<Linha>();
        leitor.ReadLine();
        while (leitor.ReadLine() is { } texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) continue;
            var c = texto.Split(';');
            linhas.Add(new Linha(c[0], c[1], c[2], c[3], c[4], c[5], c[6] == "ATIVO", c[7], c[8], c[9], c[10], c[11],
                double.Parse(c[12], CultureInfo.InvariantCulture), double.Parse(c[13], CultureInfo.InvariantCulture)));
        }
        return linhas;
    }
}
