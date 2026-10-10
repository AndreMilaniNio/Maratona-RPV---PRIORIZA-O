using System.Globalization;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Farol.Application.Services;

/// <summary>
/// Recomendação e designação de equipes (seção 5.5) e ciclo de vida do despacho (seção 11).
/// Compatibilidade e segurança vêm antes da distância; a corrida pela mesma equipe perde com 409 (Key decision 6).
/// </summary>
public class DespachoService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    INotificadorFila notificador,
    EscopoMunicipio escopo,
    Auditor auditor,
    IProvedorRoteamento roteamento,
    EquipeService equipes)
{
    public const string QualificacaoEspecializada = "EQUIPE_ESPECIALIZADA";
    private const int MaximoRotasPorConsulta = 6;

    public async Task<CandidatasDto> CandidatasAsync(Guid osId, bool incluirApoio, CancellationToken ct = default)
    {
        var os = await OsAsync(osId, ct);
        var (quals, recursos) = await RequisitosAsync(os, ct);
        var nomesQuals = await db.Qualificacoes.Where(q => quals.Contains(q.Id)).ToDictionaryAsync(q => q.Id, q => q.Nome, ct);
        var nomesRecursos = await db.Recursos.Where(r => recursos.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.Nome, ct);

        var lista = await equipes.ListarEntidadesAsync(ct);
        var hoje = DateOnly.FromDateTime(relogio.Agora.UtcDateTime);
        var destino = os.Latitude is { } la && os.Longitude is { } lo ? new Coordenadas(la, lo) : (Coordenadas?)null;
        var ativos = await AtivosPorEquipeAsync(ct);

        var candidatas = new List<(Equipe Equipe, EquipeCandidataDto Dto)>();
        foreach (var e in lista.Where(e => e.Ativa))
        {
            var mesmaCidade = e.MunicipioBaseId == os.MunicipioId || e.MunicipiosAdicionais.Any(m => m.MunicipioId == os.MunicipioId);
            if (!mesmaCidade && !incluirApoio) continue;

            var temQuals = e.Qualificacoes.Where(q => q.ValidaAte is null || q.ValidaAte >= hoje).Select(q => q.QualificacaoId).ToHashSet();
            var temRecursos = e.Recursos.Select(r => r.RecursoId).ToHashSet();
            var faltando = quals.Where(q => !temQuals.Contains(q)).Select(q => $"Qualificação: {nomesQuals[q]}")
                .Concat(recursos.Where(r => !temRecursos.Contains(r)).Select(r => $"Recurso: {nomesRecursos[r]}"))
                .ToList();

            var emUso = ativos.GetValueOrDefault(e.Id);
            var (disponivel, motivo) = Disponibilidade(e, emUso);

            double? distancia = null;
            if (destino is { } d && e.Latitude is { } elat && e.Longitude is { } elng)
                distancia = Math.Round(new Coordenadas(elat, elng).DistanciaKm(d), 2);

            var dto = new EquipeCandidataDto(equipes.Mapear(e, emUso, []), faltando.Count == 0, faltando, disponivel, motivo,
                !mesmaCidade, distancia, null, distancia is null ? "SEM_LOCALIZACAO" : "LINHA_RETA", false, string.Empty);
            candidatas.Add((e, dto));
        }

        // Rotas só para as melhores candidatas compatíveis e disponíveis (custo do serviço externo).
        if (roteamento.Disponivel && destino is { } alvo)
        {
            foreach (var (e, dto) in candidatas.Where(c => c.Dto.Compativel && c.Dto.Disponivel && c.Dto.DistanciaKm is not null)
                         .OrderBy(c => c.Dto.DistanciaKm).Take(MaximoRotasPorConsulta).ToList())
            {
                var rota = await roteamento.CalcularAsync(e.Latitude!.Value, e.Longitude!.Value, alvo.Latitude, alvo.Longitude, ct);
                if (rota is null) continue;
                var i = candidatas.FindIndex(c => c.Equipe.Id == e.Id);
                candidatas[i] = (e, dto with { DistanciaKm = Math.Round(rota.DistanciaKm, 2), TempoEstimadoMin = Math.Round(rota.TempoMin, 1), OrigemEstimativa = "ROTEAMENTO" });
            }
        }

        var ordenadas = candidatas.Select(c => c.Dto)
            .OrderBy(c => c.Compativel && c.Disponivel ? 0 : c.Compativel ? 1 : 2)
            .ThenBy(c => c.ApoioIntermunicipal ? 1 : 0)
            .ThenBy(c => c.TempoEstimadoMin ?? double.MaxValue)
            .ThenBy(c => c.DistanciaKm ?? double.MaxValue)
            .ThenBy(c => c.Equipe.Codigo, StringComparer.Ordinal)
            .ToList();

        var recomendada = ordenadas.FirstOrDefault(c => c.Compativel && c.Disponivel && !c.ApoioIntermunicipal)
                          ?? ordenadas.FirstOrDefault(c => c.Compativel && c.Disponivel);
        var resultado = ordenadas.Select(c => c with
        {
            Recomendada = ReferenceEquals(c, recomendada),
            Justificativa = Justificar(c, ReferenceEquals(c, recomendada)),
        }).ToList();

        return new CandidatasDto(os.Id, os.Numero, os.Latitude, os.Longitude,
            nomesQuals.Values.Order().ToList(), nomesRecursos.Values.Order().ToList(), roteamento.Disponivel, resultado);
    }

    public async Task<DesignacaoDto> DesignarAsync(Guid osId, DesignarRequest r, CancellationToken ct = default)
    {
        var agora = relogio.Agora;
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Bloqueia OS e equipe: duas designações simultâneas serializam aqui e a segunda revalida tudo.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM ordens_servico WHERE id = {osId} FOR UPDATE", ct);
        var os = await db.OrdensServico.Where(o => o.Id == osId)
                     .Include(o => o.RecursosNecessarios).FirstOrDefaultAsync(ct)
                 ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", osId, ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM equipes WHERE id = {r.EquipeId} FOR UPDATE", ct);
        var equipe = await db.Equipes.Where(e => e.Id == r.EquipeId)
                         .Include(e => e.MunicipiosAdicionais).Include(e => e.Qualificacoes).Include(e => e.Recursos)
                         .FirstOrDefaultAsync(ct)
                     ?? throw new NaoEncontradoException("Equipe não encontrada.");

        if (!MaquinaEstadosOrdemServico.Despachaveis.Contains(os.Status))
            throw new ConflitoException($"A OS não está apta ao despacho (status {os.Status}).");
        if (await db.Despachos.AnyAsync(d => d.OrdemServicoId == os.Id && d.Ativo, ct))
            throw new ConflitoException("A OS já possui um despacho ativo.");

        var mesmaCidade = equipe.MunicipioBaseId == os.MunicipioId || equipe.MunicipiosAdicionais.Any(m => m.MunicipioId == os.MunicipioId);
        if (!mesmaCidade)
        {
            if (!r.ApoioIntermunicipal)
                throw new RegraNegocioException("Equipe de outro município: confirme o apoio intermunicipal.");
            if (string.IsNullOrWhiteSpace(r.Justificativa))
                throw new RegraNegocioException("O apoio intermunicipal exige justificativa.");
        }

        var (quals, recursos) = await RequisitosAsync(os, ct);
        var hoje = DateOnly.FromDateTime(agora.UtcDateTime);
        var compativel = quals.All(q => equipe.Qualificacoes.Any(x => x.QualificacaoId == q && (x.ValidaAte is null || x.ValidaAte >= hoje)))
                         && recursos.All(rc => equipe.Recursos.Any(x => x.RecursoId == rc));
        var emUso = await db.Despachos.CountAsync(d => d.EquipeId == equipe.Id && d.Ativo, ct);
        var (disponivel, motivo) = Disponibilidade(equipe, emUso);

        // Capacidade é invariante (Key decision 6): nem exceção autorizada a ultrapassa.
        if (emUso >= equipe.Capacidade)
            throw new ConflitoException("A equipe deixou de estar disponível: capacidade de atendimento esgotada.");
        if (!compativel || !disponivel)
        {
            if (!r.Excecao)
                throw new ConflitoException(!compativel
                    ? "A equipe não é compatível com os requisitos da OS."
                    : $"A equipe deixou de estar disponível ({motivo}).");
            if (!usuario.Tem(Permissoes.DespachoExcecao))
                throw new AcessoNegadoException("Designar equipe incompatível ou indisponível exige exceção autorizada por supervisor.");
            if (string.IsNullOrWhiteSpace(r.Justificativa))
                throw new RegraNegocioException("A exceção exige justificativa.");
        }

        double? distancia = null, tempo = null;
        string? origem = null;
        if (os.Latitude is { } la && os.Longitude is { } lo && equipe.Latitude is { } ela && equipe.Longitude is { } elo)
        {
            var rota = roteamento.Disponivel ? await roteamento.CalcularAsync(ela, elo, la, lo, ct) : null;
            distancia = Math.Round(rota?.DistanciaKm ?? new Coordenadas(ela, elo).DistanciaKm(new Coordenadas(la, lo)), 2);
            tempo = rota is null ? null : Math.Round(rota.TempoMin, 1);
            origem = rota is null ? "LINHA_RETA" : "ROTEAMENTO";
        }

        var despacho = new Despacho
        {
            Id = Guid.NewGuid(), OrdemServicoId = os.Id, EquipeId = equipe.Id, Ativo = true, DesignadoPorId = usuario.Id,
            DesignadoEm = agora, EquipeLatitude = equipe.Latitude, EquipeLongitude = equipe.Longitude,
            EquipeLocalizacaoEm = equipe.LocalizacaoAtualizadaEm, DistanciaKm = distancia, TempoEstimadoMin = tempo,
            OrigemEstimativa = origem, ApoioIntermunicipal = !mesmaCidade, Excecao = !compativel || !disponivel,
            Justificativa = string.IsNullOrWhiteSpace(r.Justificativa) ? null : r.Justificativa.Trim(),
        };
        db.Despachos.Add(despacho);

        var statusAnterior = os.Status;
        os.Status = StatusOrdemServico.EquipeDesignada;
        os.AtualizadaEm = agora;
        db.HistoricosOs.Add(Hist(os, "DESPACHO", $"Equipe {equipe.Codigo} — {equipe.Nome} designada" +
            (despacho.ApoioIntermunicipal ? " (apoio intermunicipal)" : "") + (despacho.Excecao ? " (exceção autorizada)" : "") + ".",
            statusAnterior, os.Status, despacho.Justificativa));
        auditor.Registrar(AcoesAuditoria.Despachar, "OrdemServico", os.Id, os.MunicipioId, new { status = statusAnterior },
            new { equipe = equipe.Codigo, despacho.ApoioIntermunicipal, despacho.Excecao, despacho.DistanciaKm }, despacho.Justificativa);

        try
        {
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflitoException("A OS foi despachada por outra pessoa neste instante.");
        }

        await notificador.FilaAlteradaAsync(os.MunicipioId, "despacho", ct);
        return new DesignacaoDto(despacho.Id, os.Id, os.Numero, new EquipeResumoDto(equipe.Id, equipe.Codigo, equipe.Nome),
            await EntregaAsync(os, ct));
    }

    public async Task AceiteAsync(Guid despachoId, EventoDespachoRequest r, CancellationToken ct = default) =>
        await TransitarAsync(despachoId, "ACEITE", MaquinaEstadosOrdemServico.PorDespacho.PodeAceitar,
            StatusOrdemServico.EquipeACaminho, StatusEquipe.ACaminho, d => d.AceitoEm = relogio.Agora, r.Observacao, ct);

    public async Task InicioAsync(Guid despachoId, EventoDespachoRequest r, CancellationToken ct = default) =>
        await TransitarAsync(despachoId, "INICIO", MaquinaEstadosOrdemServico.PorDespacho.PodeIniciar,
            StatusOrdemServico.EmExecucao, StatusEquipe.EmAtendimento, d => d.IniciadoEm = relogio.Agora, r.Observacao, ct);

    public async Task ConclusaoAsync(Guid despachoId, EventoDespachoRequest r, CancellationToken ct = default) =>
        await TransitarAsync(despachoId, "CONCLUSAO", MaquinaEstadosOrdemServico.PorDespacho.PodeConcluir,
            StatusOrdemServico.Concluida, null, d =>
            {
                d.Ativo = false;
                d.EncerradoEm = relogio.Agora;
                d.MotivoEncerramento = "Serviço concluído";
                d.ObservacaoConclusao = r.Observacao;
            }, r.Observacao, ct);

    /// <summary>Remove a equipe (troca ou desistência): o despacho fica no histórico e a OS volta à fila.</summary>
    public async Task EncerrarAsync(Guid despachoId, EventoDespachoRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Motivo)) throw new RegraNegocioException("Informe o motivo da remoção da equipe.");
        if (!usuario.Tem(Permissoes.DespachoDesignar)) throw new AcessoNegadoException("Somente o despacho pode remover a equipe.");
        await TransitarAsync(despachoId, "REMOCAO", MaquinaEstadosOrdemServico.PorDespacho.PodeRemoverEquipe,
            StatusOrdemServico.AguardandoDespacho, null, d =>
            {
                d.Ativo = false;
                d.EncerradoEm = relogio.Agora;
                d.MotivoEncerramento = r.Motivo!.Trim();
            }, r.Motivo, ct);
    }

    private async Task TransitarAsync(
        Guid despachoId, string evento, Func<StatusOrdemServico, bool> permitido, StatusOrdemServico novoStatus,
        StatusEquipe? statusEquipe, Action<Despacho> aplicar, string? observacao, CancellationToken ct)
    {
        var agora = relogio.Agora;
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var despacho = await db.Despachos.FirstOrDefaultAsync(d => d.Id == despachoId, ct) ?? throw new NaoEncontradoException("Despacho não encontrado.");
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM ordens_servico WHERE id = {despacho.OrdemServicoId} FOR UPDATE", ct);
        var os = await db.OrdensServico.FirstAsync(o => o.Id == despacho.OrdemServicoId, ct);
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", os.Id, ct);

        var equipeDoUsuario = await usuario.EquipeIdAsync(ct);
        if (!usuario.Tem(Permissoes.DespachoDesignar) && !(usuario.Tem(Permissoes.OsStatus) && equipeDoUsuario == despacho.EquipeId))
            throw new AcessoNegadoException("Você não pode alterar este despacho.");
        if (!despacho.Ativo) throw new ConflitoException("Este despacho não está mais ativo.");
        if (!permitido(os.Status)) throw new ConflitoException($"Ação não permitida com a OS em {os.Status}.");

        var equipe = await db.Equipes.FirstAsync(e => e.Id == despacho.EquipeId, ct);
        aplicar(despacho);
        var anterior = os.Status;
        os.Status = novoStatus;
        os.AtualizadaEm = agora;
        if (novoStatus == StatusOrdemServico.Concluida) os.EncerradaEm = agora;

        if (statusEquipe is { } se) equipe.Status = se;
        else
        {
            var outrosAtivos = await db.Despachos.AnyAsync(d => d.EquipeId == equipe.Id && d.Ativo && d.Id != despacho.Id, ct);
            if (!outrosAtivos && equipe.Status is StatusEquipe.ACaminho or StatusEquipe.EmAtendimento) equipe.Status = StatusEquipe.Disponivel;
        }

        var texto = evento switch
        {
            "ACEITE" => $"Equipe {equipe.Codigo} aceitou a OS e está a caminho.",
            "INICIO" => $"Equipe {equipe.Codigo} iniciou a execução.",
            "CONCLUSAO" => $"Serviço concluído pela equipe {equipe.Codigo}.",
            _ => $"Equipe {equipe.Codigo} removida da OS.",
        };
        db.HistoricosOs.Add(Hist(os, evento == "REMOCAO" ? "DESPACHO" : "EXECUCAO", texto, anterior, novoStatus, observacao));
        auditor.Registrar(evento switch
        {
            "CONCLUSAO" => AcoesAuditoria.Concluir,
            "REMOCAO" => AcoesAuditoria.EncerrarDespacho,
            _ => AcoesAuditoria.AlterarStatus,
        }, "OrdemServico", os.Id, os.MunicipioId, new { status = anterior }, new { status = novoStatus, equipe = equipe.Codigo }, observacao);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        await notificador.FilaAlteradaAsync(os.MunicipioId, evento.ToLowerInvariant(), ct);
    }

    /// <summary>O que a equipe recebe no despacho: coordenadas, endereço, CEP, UC e rede (seção 5.5).</summary>
    public async Task<EntregaEquipeDto> EntregaAsync(OrdemServico os, CancellationToken ct = default)
    {
        var ucs = await db.Solicitacoes.Where(s => s.OrdemServicoId == os.Id).SelectMany(s => s.Ucs.Select(u => u.Numero)).Distinct().ToListAsync(ct);
        var sub = os.SubestacaoId is { } sid ? await db.Subestacoes.Where(s => s.Id == sid).Select(s => s.Codigo + " — " + s.Nome).FirstOrDefaultAsync(ct) : null;
        var conj = os.ConjuntoId is { } cid ? await db.Conjuntos.Where(c => c.Id == cid).Select(c => c.Numero).FirstOrDefaultAsync(ct) : null;
        var cep = os.Cep is { Length: 8 } c8 ? $"{c8[..5]}-{c8[5..]}" : os.Cep;

        if (os.Latitude is { } lat && os.Longitude is { } lng)
        {
            var par = string.Create(CultureInfo.InvariantCulture, $"{lat:0.######},{lng:0.######}");
            var aproximada = os.OrigemCoordenada == OrigemCoordenada.Geocodificada;
            return new EntregaEquipeDto(lat, lng, par, aproximada,
                aproximada ? "Coordenadas geocodificadas a partir do endereço: localização aproximada." : null,
                os.EnderecoCompleto, cep, ucs, sub, conj, os.TransformadorNumero,
                string.Create(CultureInfo.InvariantCulture, $"https://www.openstreetmap.org/?mlat={lat:0.######}&mlon={lng:0.######}#map=18/{lat:0.######}/{lng:0.######}"),
                $"geo:{par}?q={par}");
        }
        return new EntregaEquipeDto(null, null, null, true,
            "A OS não possui coordenadas: a localização é aproximada e depende de confirmação no local.",
            os.EnderecoCompleto, cep, ucs, sub, conj, os.TransformadorNumero, null, null);
    }

    private async Task<OrdemServico> OsAsync(Guid id, CancellationToken ct)
    {
        var os = await db.OrdensServico.AsNoTracking().Include(o => o.RecursosNecessarios).FirstOrDefaultAsync(o => o.Id == id, ct)
                 ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", id, ct);
        return os;
    }

    /// <summary>Qualificações do tipo de ocorrência (+ especializada quando exigida) e recursos pedidos na OS.</summary>
    private async Task<(List<int> Qualificacoes, List<int> Recursos)> RequisitosAsync(OrdemServico os, CancellationToken ct)
    {
        var quals = await db.TiposOcorrencia.Where(t => t.Id == os.TipoOcorrenciaId)
            .SelectMany(t => t.Qualificacoes.Select(q => q.QualificacaoId)).ToListAsync(ct);
        if (os.EquipeEspecializada == Criterios.Confirmacao.Sim)
        {
            var esp = await db.Qualificacoes.Where(q => q.Codigo == QualificacaoEspecializada).Select(q => (int?)q.Id).FirstOrDefaultAsync(ct);
            if (esp is { } id && !quals.Contains(id)) quals.Add(id);
        }
        return (quals, os.RecursosNecessarios.Select(r => r.RecursoId).ToList());
    }

    private async Task<Dictionary<int, int>> AtivosPorEquipeAsync(CancellationToken ct) =>
        await db.Despachos.Where(d => d.Ativo).GroupBy(d => d.EquipeId).Select(g => new { g.Key, Qtd = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Qtd, ct);

    public static (bool Disponivel, string? Motivo) Disponibilidade(Equipe e, int despachosAtivos)
    {
        if (!e.Ativa) return (false, "equipe inativa");
        if (despachosAtivos >= e.Capacidade) return (false, $"capacidade esgotada ({despachosAtivos}/{e.Capacidade})");
        return e.Status switch
        {
            StatusEquipe.Disponivel => (true, null),
            StatusEquipe.Indisponivel => (false, "indisponível"),
            StatusEquipe.EmPausa => (false, "em pausa"),
            StatusEquipe.DeslocamentoOutraAtividade => (false, "em deslocamento para outra atividade"),
            StatusEquipe.ACaminho => (false, "a caminho de outra OS"),
            StatusEquipe.EmAtendimento => (false, "em atendimento"),
            _ => (false, e.Status.ToString()),
        };
    }

    private static string Justificar(EquipeCandidataDto c, bool recomendada)
    {
        if (!c.Compativel) return "Não habilitada para o serviço: " + string.Join("; ", c.Faltando) + ".";
        if (!c.Disponivel) return $"Compatível, mas indisponível ({c.MotivoIndisponivel}).";
        var deslocamento = c.TempoEstimadoMin is { } t ? $"{t:0} min estimados por rota ({c.DistanciaKm:0.0} km)"
            : c.DistanciaKm is { } km ? $"{km:0.0} km em linha reta — sem estimativa de tempo"
            : "sem localização conhecida da equipe";
        var prefixo = recomendada ? "Recomendada: compatível, disponível" : "Compatível e disponível";
        return c.ApoioIntermunicipal
            ? $"{prefixo}; apoio intermunicipal (exige justificativa); {deslocamento}."
            : $"{prefixo}, do município da OS; {deslocamento}.";
    }

    private HistoricoOs Hist(OrdemServico os, string tipo, string descricao, StatusOrdemServico de, StatusOrdemServico para, string? justificativa) =>
        new()
        {
            OrdemServicoId = os.Id, OcorridoEm = relogio.Agora, UsuarioId = usuario.Id, UsuarioNome = usuario.Nome, Tipo = tipo,
            Descricao = descricao, StatusAnterior = de.ToString(), StatusNovo = para.ToString(), Justificativa = justificativa,
        };
}
