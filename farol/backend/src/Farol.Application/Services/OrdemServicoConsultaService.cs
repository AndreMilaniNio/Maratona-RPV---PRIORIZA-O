using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Mapping;
using Farol.Application.Prioritization;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace Farol.Application.Services;

/// <summary>Leituras do painel operacional: fila, indicadores, detalhe e explicação da prioridade.</summary>
public class OrdemServicoConsultaService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    EscopoMunicipio escopo,
    DuplicidadeService duplicidades)
{
    private static readonly StatusOrdemServico[] Encerrados = [StatusOrdemServico.Concluida, StatusOrdemServico.Cancelada];
    private static readonly string[] RiscosReais =
    [
        Criterios.Seguranca.RiscoChoque, Criterios.Seguranca.CaboEnergizado, Criterios.Seguranca.Incendio,
        Criterios.Seguranca.EstruturaQueda, Criterios.Seguranca.RiscoCirculacao,
    ];

    private sealed record LinhaChave(
        Guid Id, int MunicipioId, int NivelPrecedencia, int? Rank, TipoManutencao TipoManutencao, int Pontuacao,
        StatusOrdemServico Status, DateTimeOffset? PrazoTriagem, DateTimeOffset? PrazoDespacho, DateTimeOffset? PrazoInicio,
        DateTimeOffset? PrazoConclusao, DateTimeOffset AbertaEm, string Numero);

    public async Task<FilaDto> FilaAsync(FiltroFila f, CancellationToken ct = default)
    {
        var municipios = await escopo.ResolverAsync(f.MunicipioId, ct);
        var agora = relogio.Agora;

        // Posições: toda a fila pendente das cidades em escopo, independentemente dos filtros.
        var pendentes = await Chaves(Escopo(db.OrdensServico.AsNoTracking(), municipios).Where(o => !Encerrados.Contains(o.Status)))
            .ToListAsync(ct);
        var posicoes = PoliticaOrdenacao.Posicoes(pendentes.Select(ParaChave));

        var filtradas = await Chaves(Filtrar(Escopo(db.OrdensServico.AsNoTracking(), municipios), f, agora)).ToListAsync(ct);
        var ordenadas = Ordenar(filtradas, f.OrdenarPor, posicoes).ToList();

        var tamanho = Math.Clamp(f.TamanhoPagina, 1, 200);
        var pagina = Math.Max(1, f.Pagina);
        var ids = ordenadas.Skip((pagina - 1) * tamanho).Take(tamanho).Select(x => x.Id).ToList();
        var itens = await ItensAsync(ids, posicoes, agora, ct);

        var demonstrativa = await VersaoDemonstrativaEmVigorAsync(municipios, ct);
        return new FilaDto(ids.Select(id => itens[id]).ToList(), ordenadas.Count, pagina, tamanho, demonstrativa, agora);
    }

    public async Task<IndicadoresDto> IndicadoresAsync(int? municipioId, CancellationToken ct = default)
    {
        var municipios = await escopo.ResolverAsync(municipioId, ct);
        var agora = relogio.Agora;

        var abertas = await Escopo(db.OrdensServico.AsNoTracking(), municipios)
            .Where(o => !Encerrados.Contains(o.Status))
            .Select(o => new
            {
                o.MunicipioId, o.Status, Critica = o.Prioridade != null && o.Prioridade.Critica,
                o.PrazoTriagem, o.PrazoDespacho, o.PrazoInicio, o.PrazoConclusao,
            })
            .ToListAsync(ct);

        var idsEscopo = municipios?.ToArray();
        var equipes = await db.Equipes.AsNoTracking()
            .Where(e => e.Ativa && (idsEscopo == null || idsEscopo.Contains(e.MunicipioBaseId)))
            .Select(e => new { e.MunicipioBaseId, e.Status, e.Capacidade, Ativos = db.Despachos.Count(d => d.EquipeId == e.Id && d.Ativo) })
            .ToListAsync(ct);

        var nomes = await db.Municipios.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Nome, ct);

        bool Vencida(StatusOrdemServico s, DateTimeOffset? t, DateTimeOffset? d, DateTimeOffset? i, DateTimeOffset? c) =>
            Proximo(s, t, d, i, c) is { } p && p <= agora;

        var cidades = abertas.Select(a => a.MunicipioId).Concat(equipes.Select(e => e.MunicipioBaseId)).Distinct().OrderBy(id => nomes[id]).ToList();
        var porCidade = cidades.Select(id =>
        {
            var os = abertas.Where(a => a.MunicipioId == id).ToList();
            var eq = equipes.Where(e => e.MunicipioBaseId == id).ToList();
            return new IndicadoresCidadeDto(id, nomes[id],
                os.Count,
                os.Count(o => o.Critica),
                os.Count(o => MaquinaEstadosOrdemServico.Despachaveis.Contains(o.Status)),
                os.Count(o => MaquinaEstadosOrdemServico.ComEquipe.Contains(o.Status)),
                os.Count(o => Vencida(o.Status, o.PrazoTriagem, o.PrazoDespacho, o.PrazoInicio, o.PrazoConclusao)),
                eq.Count(e => e.Status == StatusEquipe.Disponivel && e.Ativos < e.Capacidade),
                eq.Count(e => e.Status is StatusEquipe.ACaminho or StatusEquipe.DeslocamentoOutraAtividade),
                eq.Count(e => e.Status == StatusEquipe.EmAtendimento));
        }).ToList();

        return new IndicadoresDto(
            porCidade.Sum(c => c.Abertas), porCidade.Sum(c => c.Criticas), porCidade.Sum(c => c.AguardandoDespacho),
            porCidade.Sum(c => c.EmAtendimento), porCidade.Sum(c => c.Vencidas), porCidade.Sum(c => c.EquipesDisponiveis),
            porCidade.Sum(c => c.EquipesDeslocamento), porCidade.Sum(c => c.EquipesExecutando),
            porCidade, agora);
    }

    public async Task<OrdemServicoDetalheDto> DetalheAsync(Guid id, CancellationToken ct = default)
    {
        var os = await db.OrdensServico.AsNoTracking()
            .Include(o => o.Municipio).Include(o => o.TipoOcorrencia)
            .Include(o => o.Subestacao).Include(o => o.Conjunto)
            .Include(o => o.CondicoesSeguranca)
            .Include(o => o.Classes).ThenInclude(c => c.ClasseCliente)
            .Include(o => o.RecursosNecessarios).ThenInclude(r => r.Recurso)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, ct) ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", id, ct);

        var agora = relogio.Agora;
        var solicitacoes = await db.Solicitacoes.AsNoTracking().Include(s => s.Ucs).Include(s => s.Municipio)
            .Where(s => s.OrdemServicoId == id).OrderBy(s => s.RegistradaEm).ToListAsync(ct);
        var autores = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.Nome, ct);
        SolicitacaoDto Sol(Solicitacao s) => new(s.Id, s.Numero, s.MunicipioId, s.Municipio!.Nome, s.Canal, s.Origem,
            s.ProtocoloExterno, s.RegistradaEm, autores.GetValueOrDefault(s.RegistradaPorId, "—"), s.Descricao, s.UcNaoInformada,
            s.MotivoUcNaoInformada, s.Ucs.Select(u => new SolicitacaoUcDto(u.Numero, u.ValidadaNoCadastro)).ToList(),
            os.Id, os.Numero, s.DadosInformados);

        var original = solicitacoes.FirstOrDefault(s => s.Id == os.SolicitacaoId) ?? solicitacoes.First();
        var numerosUc = solicitacoes.SelectMany(s => s.Ucs).Select(u => u.Numero).Distinct().ToList();
        var ucs = await db.UnidadesConsumidoras.AsNoTracking().Include(u => u.ClasseCliente)
            .Where(u => numerosUc.Contains(u.Numero)).ToDictionaryAsync(u => u.Numero, ct);
        var ucsDto = numerosUc.Select(n => ucs.TryGetValue(n, out var u)
            ? new UcDetalheDto(n, true, Mascarar(u.ClienteNome), u.ClasseCliente!.Nome, u.Situacao.ToString(),
                $"{u.Logradouro}, {u.NumeroImovel} — {u.Bairro}", u.Demonstrativo)
            : new UcDetalheDto(n, false, null, null, null, null, false)).ToList();

        string? localidadeNome = os.TransformadorLocalidade is null ? null
            : await db.Localidades.Where(l => l.Codigo == os.TransformadorLocalidade).Select(l => l.Nome).FirstOrDefaultAsync(ct);

        var classificacao = await ExplicacaoAsync(os, ct);
        var prioridade = os.PrioridadeId is { } pid ? await db.Prioridades.AsNoTracking().FirstAsync(p => p.Id == pid, ct) : null;
        var demonstrativa = await db.VersoesPontuacao.Where(v => v.Id == os.VersaoPontuacaoId).Select(v => v.Demonstrativa).FirstAsync(ct);

        var despachos = await DespachosAsync(id, autores, ct);
        var historico = await db.HistoricosOs.AsNoTracking().Where(h => h.OrdemServicoId == id).OrderByDescending(h => h.OcorridoEm).ThenByDescending(h => h.Id)
            .Select(h => new HistoricoDto(h.OcorridoEm, h.Tipo, h.Descricao, h.UsuarioNome, h.StatusAnterior, h.StatusNovo, h.Justificativa))
            .ToListAsync(ct);
        var comentarios = await db.Comentarios.AsNoTracking().Where(c => c.OrdemServicoId == id).OrderBy(c => c.CriadoEm)
            .Select(c => new ComentarioDto(c.Id, c.UsuarioNome, c.Texto, c.CriadoEm)).ToListAsync(ct);
        var anexos = await db.Anexos.AsNoTracking().Where(a => a.OrdemServicoId == id).OrderBy(a => a.EnviadoEm)
            .Select(a => new AnexoDto(a.Id, a.NomeArquivo, a.TipoConteudo, a.Tamanho, a.EnviadoEm)).ToListAsync(ct);

        var posicao = await PosicaoAsync(os, ct);
        var sugestoes = MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status) ? [] :
            await duplicidades.SugerirAsync(new CriterioDuplicidade(os.MunicipioId, os.Latitude, os.Longitude,
                os.TransformadorNumero, numerosUc, os.ConjuntoId, os.TipoOcorrenciaId, os.Id), ct);

        return new OrdemServicoDetalheDto(
            os.Id, os.Numero, posicao, os.Status, os.TipoManutencao, os.TipoOcorrenciaId, os.TipoOcorrencia!.Nome,
            os.MunicipioId, os.Municipio!.Nome, os.AbertaEm, os.AtualizadaEm, os.EncerradaEm, os.MotivoCancelamento,
            Sol(original), solicitacoes.Where(s => s.Id != original.Id).Select(Sol).ToList(), ucsDto,
            os.Logradouro, os.NumeroEndereco, os.Bairro, os.Cep, os.EnderecoCompleto, os.PontoReferencia,
            os.ObservacoesLocalizacao, os.LocalizacaoPendente,
            os.Latitude is { } lat && os.Longitude is { } lng ? new CoordenadaDto(lat, lng, os.OrigemCoordenada, os.PrecisaoMetros, os.CoordenadaRegistradaEm) : null,
            new RedeDto(os.SubestacaoId, os.Subestacao?.Codigo, os.Subestacao?.Nome, os.Subestacao?.Local, os.ConjuntoId,
                os.Conjunto?.Numero, os.TransformadorNumero, os.TransformadorLocalidade, localidadeNome, os.TransformadorLocal,
                os.OrigemRede, os.Trecho, os.TrechoConfirmado, os.EquipamentoDescricao, os.IdentificadorEquipamento, os.ChaveEletrica),
            new ImpactoDto(os.PessoasAfetadas, os.QuantidadePessoas, os.UcsAfetadas, os.QuantidadeUcs, os.ServicoEssencial,
                os.SituacaoCliente, os.CondicoesSeguranca.Select(c => c.Codigo).ToList(), os.CondicaoFornecimento,
                os.Redundancia, os.FonteReserva, os.EquipeEspecializada, os.EquipamentoAfetado, os.Abrangencia, os.NivelRede,
                os.QuantidadeEquipamentos, os.DuracaoEstimadaMin, os.DataLimite,
                os.RecursosNecessarios.Select(r => r.Recurso!.Nome).ToList(),
                os.Classes.Select(c => c.ClasseCliente!.Nome).ToList()),
            classificacao,
            os.PrioridadeManualId is not null,
            os.JustificativaManual,
            new PrazosDto(os.PrazoTriagem, os.PrazoDespacho, os.PrazoInicio, os.PrazoRestabelecimento, os.PrazoConclusao,
                ProximoPrazo(os.Status, os.PrazoTriagem, os.PrazoDespacho, os.PrazoInicio, os.PrazoConclusao, agora),
                prioridade?.Calendario.ToString() ?? "-", prioridade?.Demonstrativa ?? true),
            despachos.FirstOrDefault(d => d.Ativo),
            despachos,
            historico,
            comentarios,
            anexos,
            sugestoes,
            await AcoesAsync(os, despachos.Any(d => d.Ativo), ct));
    }

    /// <summary>Explicação do resultado atual, com a versão de pontuação usada.</summary>
    public async Task<ClassificacaoDto?> ExplicacaoAsync(OrdemServico os, CancellationToken ct = default)
    {
        if (os.ResultadoAtualId is not { } rid) return null;
        var resultado = await db.ResultadosPrioridade.AsNoTracking()
            .Include(r => r.Itens).Include(r => r.VersaoPontuacao).ThenInclude(v => v!.Municipio)
            .FirstAsync(r => r.Id == rid, ct);
        var prioridades = await db.Prioridades.AsNoTracking().ToDictionaryAsync(p => p.Id, ct);
        return MapeamentoClassificacao.DeResultado(resultado, prioridades, MapeamentoClassificacao.Versao(resultado.VersaoPontuacao!));
    }

    public async Task<object> PrioridadeAsync(Guid id, CancellationToken ct = default)
    {
        var os = await db.OrdensServico.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct)
                 ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(os.MunicipioId, "OrdemServico", id, ct);
        var atual = await ExplicacaoAsync(os, ct);
        var historico = await db.ResultadosPrioridade.AsNoTracking()
            .Where(r => r.OrdemServicoId == id).OrderByDescending(r => r.CalculadoEm).ThenByDescending(r => r.Id)
            .Select(r => new
            {
                r.Id, r.CalculadoEm, Motivo = r.Motivo.ToString(), r.Pontuacao, r.NivelPrecedencia,
                Prioridade = r.Prioridade!.Nome, PrioridadeCodigo = r.Prioridade.Codigo, Versao = r.VersaoPontuacao!.Numero,
                VersaoMunicipioId = r.VersaoPontuacao.MunicipioId, r.MotivoPrincipal, r.Justificativa,
            })
            .ToListAsync(ct);
        return new { atual, politicaOrdenacao = PoliticaOrdenacao.Descricao, historico };
    }

    public async Task<List<HistoricoDto>> HistoricoAsync(Guid id, CancellationToken ct = default)
    {
        var municipio = await db.OrdensServico.Where(o => o.Id == id).Select(o => (int?)o.MunicipioId).FirstOrDefaultAsync(ct)
                        ?? throw new NaoEncontradoException("OS não encontrada.");
        await escopo.ExigirAsync(municipio, "OrdemServico", id, ct);
        return await db.HistoricosOs.AsNoTracking().Where(h => h.OrdemServicoId == id)
            .OrderByDescending(h => h.OcorridoEm).ThenByDescending(h => h.Id)
            .Select(h => new HistoricoDto(h.OcorridoEm, h.Tipo, h.Descricao, h.UsuarioNome, h.StatusAnterior, h.StatusNovo, h.Justificativa))
            .ToListAsync(ct);
    }

    private async Task<List<DespachoDto>> DespachosAsync(Guid osId, Dictionary<Guid, string> autores, CancellationToken ct)
    {
        var lista = await db.Despachos.AsNoTracking().Include(d => d.Equipe)
            .Where(d => d.OrdemServicoId == osId).OrderByDescending(d => d.DesignadoEm).ToListAsync(ct);
        return lista.Select(d => new DespachoDto(d.Id, new EquipeResumoDto(d.EquipeId, d.Equipe!.Codigo, d.Equipe.Nome),
            d.Ativo, autores.GetValueOrDefault(d.DesignadoPorId, "—"), d.DesignadoEm, d.AceitoEm, d.IniciadoEm, d.EncerradoEm,
            d.MotivoEncerramento, d.DistanciaKm, d.TempoEstimadoMin, d.OrigemEstimativa, d.ApoioIntermunicipal, d.Excecao,
            d.Justificativa)).ToList();
    }

    private async Task<int?> PosicaoAsync(OrdemServico os, CancellationToken ct)
    {
        if (MaquinaEstadosOrdemServico.Encerrados.Contains(os.Status)) return null;
        var chaves = await Chaves(db.OrdensServico.AsNoTracking()
            .Where(o => o.MunicipioId == os.MunicipioId && !Encerrados.Contains(o.Status))).ToListAsync(ct);
        return PoliticaOrdenacao.Posicoes(chaves.Select(ParaChave)).GetValueOrDefault(os.Id);
    }

    private async Task<List<string>> AcoesAsync(OrdemServico os, bool temDespachoAtivo, CancellationToken ct)
    {
        var acoes = new List<string>();
        var s = os.Status;
        var equipeDoUsuario = await usuario.EquipeIdAsync(ct);
        var ehDaEquipe = equipeDoUsuario is { } eq && await db.Despachos.AnyAsync(d => d.OrdemServicoId == os.Id && d.Ativo && d.EquipeId == eq, ct);
        var podeOperar = usuario.Tem(Permissoes.DespachoDesignar) || ehDaEquipe;

        if (usuario.Tem(Permissoes.DespachoDesignar) && MaquinaEstadosOrdemServico.Despachaveis.Contains(s) && !temDespachoAtivo) acoes.Add("DESPACHAR");
        if (podeOperar && MaquinaEstadosOrdemServico.PorDespacho.PodeAceitar(s)) acoes.Add("ACEITE");
        if (podeOperar && MaquinaEstadosOrdemServico.PorDespacho.PodeIniciar(s)) acoes.Add("INICIAR");
        if (podeOperar && MaquinaEstadosOrdemServico.PorDespacho.PodeConcluir(s)) acoes.Add("CONCLUIR");
        if (usuario.Tem(Permissoes.DespachoDesignar) && MaquinaEstadosOrdemServico.PorDespacho.PodeRemoverEquipe(s)) acoes.Add("REMOVER_EQUIPE");
        if (usuario.Tem(Permissoes.OsStatus))
        {
            foreach (var destino in MaquinaEstadosOrdemServico.Manuais.GetValueOrDefault(s, []))
            {
                if (MaquinaEstadosOrdemServico.EhReabertura(s, destino) && !usuario.Tem(Permissoes.OsReabrir)) continue;
                if (destino == StatusOrdemServico.Cancelada && !usuario.Tem(Permissoes.OsEncerrar)) continue;
                acoes.Add($"STATUS:{destino}");
            }
        }
        if (!MaquinaEstadosOrdemServico.Encerrados.Contains(s))
        {
            if (usuario.Tem(Permissoes.OsReclassificar)) acoes.Add("RECLASSIFICAR");
            if (usuario.Tem(Permissoes.OsAtualizarFatos)) acoes.Add("ATUALIZAR_FATOS");
            if (usuario.Tem(Permissoes.OsTrocarMunicipio)) acoes.Add("TROCAR_MUNICIPIO");
            if (usuario.Tem(Permissoes.OsUnificar)) acoes.Add("UNIFICAR");
        }
        acoes.Add("COMENTAR");
        return acoes;
    }

    private async Task<Dictionary<Guid, FilaItemDto>> ItensAsync(List<Guid> ids, Dictionary<Guid, int> posicoes, DateTimeOffset agora, CancellationToken ct)
    {
        var linhas = await db.OrdensServico.AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .Select(o => new
            {
                o.Id, o.Numero, o.Prioridade, Manual = o.PrioridadeManualId != null, o.Pontuacao, o.NivelPrecedencia,
                o.TipoManutencao, Tipo = o.TipoOcorrencia!.Nome, o.EnderecoCompleto, o.Logradouro, o.Bairro, o.Trecho,
                o.MunicipioId, Municipio = o.Municipio!.Nome,
                Sub = o.Subestacao != null ? o.Subestacao.Codigo : null, Conj = o.Conjunto != null ? o.Conjunto.Numero : null,
                o.TransformadorNumero, o.PessoasAfetadas, o.UcsAfetadas, o.ServicoEssencial, o.SituacaoCliente,
                Condicoes = o.CondicoesSeguranca.Select(c => c.Codigo).ToList(),
                Classes = o.Classes.Select(c => c.ClasseCliente!.Nome).ToList(),
                o.AbertaEm, o.Status, o.PrazoTriagem, o.PrazoDespacho, o.PrazoInicio, o.PrazoConclusao,
                o.LocalizacaoPendente, TemCoordenadas = o.Latitude != null,
                Demonstrativa = db.VersoesPontuacao.Where(v => v.Id == o.VersaoPontuacaoId).Select(v => v.Demonstrativa).First(),
                Equipe = o.Despachos.Where(d => d.Ativo).Select(d => new EquipeResumoDto(d.EquipeId, d.Equipe!.Codigo, d.Equipe.Nome)).FirstOrDefault(),
            })
            .AsSplitQuery()
            .ToListAsync(ct);

        var ucs = (await db.Solicitacoes.AsNoTracking()
                .Where(s => ids.Contains(s.OrdemServicoId))
                .Select(s => new { s.OrdemServicoId, s.UcNaoInformada, Ucs = s.Ucs.Select(u => u.Numero).ToList() })
                .ToListAsync(ct))
            .GroupBy(x => x.OrdemServicoId)
            .ToDictionary(g => g.Key, g => (Ucs: g.SelectMany(x => x.Ucs).Distinct().ToList(), NaoInformada: g.All(x => x.UcNaoInformada)));

        return linhas.ToDictionary(o => o.Id, o =>
        {
            var u = ucs.GetValueOrDefault(o.Id, ([], true));
            return new FilaItemDto(
                posicoes.TryGetValue(o.Id, out var p) ? p : null, o.Id, o.Numero,
                o.Prioridade is null ? null : MapeamentoClassificacao.Resumo(o.Prioridade), o.Manual, o.Pontuacao,
                o.NivelPrecedencia, o.TipoManutencao, o.Tipo, o.EnderecoCompleto ?? o.Logradouro, o.Bairro, o.Trecho,
                o.MunicipioId, o.Municipio, u.Ucs, u.Ucs.Count == 0, o.Classes, o.Sub, o.Conj, o.TransformadorNumero,
                o.PessoasAfetadas, o.UcsAfetadas, o.Condicoes.Any(RiscosReais.Contains), o.Condicoes, o.ServicoEssencial,
                o.SituacaoCliente, o.AbertaEm,
                ProximoPrazo(o.Status, o.PrazoTriagem, o.PrazoDespacho, o.PrazoInicio, o.PrazoConclusao, agora),
                o.Status, o.Equipe, o.LocalizacaoPendente, o.TemCoordenadas, o.Demonstrativa);
        });
    }

    private IQueryable<OrdemServico> Filtrar(IQueryable<OrdemServico> q, FiltroFila f, DateTimeOffset agora)
    {
        if (!f.IncluirEncerradas && (f.Status is null || f.Status.Count == 0)) q = q.Where(o => !Encerrados.Contains(o.Status));
        if (f.Status is { Count: > 0 } status) q = q.Where(o => status.Contains(o.Status));
        if (f.PrioridadeIds is { Count: > 0 } prioridades) q = q.Where(o => o.PrioridadeId != null && prioridades.Contains(o.PrioridadeId.Value));
        if (f.TipoManutencao is { } tm) q = q.Where(o => o.TipoManutencao == tm);
        if (f.TipoOcorrenciaId is { } to) q = q.Where(o => o.TipoOcorrenciaId == to);
        if (!string.IsNullOrWhiteSpace(f.Bairro)) q = q.Where(o => o.Bairro != null && EF.Functions.Like(o.Bairro.ToLower(), "%" + f.Bairro.Trim().ToLower() + "%"));
        if (!string.IsNullOrWhiteSpace(f.Logradouro)) q = q.Where(o => o.Logradouro != null && EF.Functions.Like(o.Logradouro.ToLower(), "%" + f.Logradouro.Trim().ToLower() + "%"));
        if (!string.IsNullOrWhiteSpace(f.Trecho)) q = q.Where(o => o.Trecho != null && EF.Functions.Like(o.Trecho.ToLower(), "%" + f.Trecho.Trim().ToLower() + "%"));
        if (!string.IsNullOrWhiteSpace(f.FaixaPessoas)) q = q.Where(o => o.PessoasAfetadas == f.FaixaPessoas);
        if (!string.IsNullOrWhiteSpace(f.FaixaUcs)) q = q.Where(o => o.UcsAfetadas == f.FaixaUcs);
        if (f.Risco == "QUALQUER") q = q.Where(o => o.CondicoesSeguranca.Any(c => RiscosReais.Contains(c.Codigo)));
        else if (!string.IsNullOrWhiteSpace(f.Risco)) q = q.Where(o => o.CondicoesSeguranca.Any(c => c.Codigo == f.Risco));
        if (f.ServicoEssencial == "QUALQUER") q = q.Where(o => o.ServicoEssencial != Criterios.Essencial.NaoIdentificado);
        else if (!string.IsNullOrWhiteSpace(f.ServicoEssencial)) q = q.Where(o => o.ServicoEssencial == f.ServicoEssencial);
        if (!string.IsNullOrWhiteSpace(f.Equipamento)) q = q.Where(o => o.EquipamentoAfetado == f.Equipamento);
        if (f.EquipeId is { } eq) q = q.Where(o => o.Despachos.Any(d => d.Ativo && d.EquipeId == eq));
        if (f.AbertaDe is { } de) q = q.Where(o => o.AbertaEm >= de);
        if (f.AbertaAte is { } ate) q = q.Where(o => o.AbertaEm <= ate);
        if (f.SubestacaoId is { } sub) q = q.Where(o => o.SubestacaoId == sub);
        if (f.ConjuntoId is { } conj) q = q.Where(o => o.ConjuntoId == conj);
        if (!string.IsNullOrWhiteSpace(f.Transformador)) q = q.Where(o => o.TransformadorNumero != null && o.TransformadorNumero.StartsWith(f.Transformador.Trim()));
        if (f.ClasseId is { } cl) q = q.Where(o => o.Classes.Any(c => c.ClasseClienteId == cl));
        if (!string.IsNullOrWhiteSpace(f.SituacaoCliente)) q = q.Where(o => o.SituacaoCliente == f.SituacaoCliente);
        if (!string.IsNullOrWhiteSpace(f.Uc))
        {
            var uc = f.Uc.Trim();
            q = q.Where(o => db.Solicitacoes.Any(s => s.OrdemServicoId == o.Id && s.Ucs.Any(u => u.Numero == uc)));
        }
        if (!string.IsNullOrWhiteSpace(f.Busca))
        {
            var b = "%" + f.Busca.Trim().ToLower() + "%";
            q = q.Where(o => EF.Functions.Like(o.Numero.ToLower(), b) || (o.EnderecoCompleto != null && EF.Functions.Like(o.EnderecoCompleto.ToLower(), b))
                             || EF.Functions.Like(o.TipoOcorrencia!.Nome.ToLower(), b));
        }
        if (f.Prazo is "vencido" or "proximo")
        {
            var limite = f.Prazo == "vencido" ? agora : agora.AddHours(2);
            q = q.Where(o =>
                ((o.Status == StatusOrdemServico.Aberta || o.Status == StatusOrdemServico.EmTriagem) && o.PrazoTriagem <= limite) ||
                ((o.Status == StatusOrdemServico.AguardandoDespacho || o.Status == StatusOrdemServico.Suspensa) && o.PrazoDespacho <= limite) ||
                ((o.Status == StatusOrdemServico.EquipeDesignada || o.Status == StatusOrdemServico.EquipeACaminho) && o.PrazoInicio <= limite) ||
                ((o.Status == StatusOrdemServico.EmExecucao || o.Status == StatusOrdemServico.AguardandoRecurso) && o.PrazoConclusao <= limite));
            if (f.Prazo == "proximo")
                q = q.Where(o =>
                    ((o.Status == StatusOrdemServico.Aberta || o.Status == StatusOrdemServico.EmTriagem) && o.PrazoTriagem > agora) ||
                    ((o.Status == StatusOrdemServico.AguardandoDespacho || o.Status == StatusOrdemServico.Suspensa) && o.PrazoDespacho > agora) ||
                    ((o.Status == StatusOrdemServico.EquipeDesignada || o.Status == StatusOrdemServico.EquipeACaminho) && o.PrazoInicio > agora) ||
                    ((o.Status == StatusOrdemServico.EmExecucao || o.Status == StatusOrdemServico.AguardandoRecurso) && o.PrazoConclusao > agora));
        }
        return q;
    }

    private static IEnumerable<LinhaChave> Ordenar(List<LinhaChave> linhas, string? ordenarPor, Dictionary<Guid, int> posicoes)
    {
        var campo = (ordenarPor ?? "fila").TrimStart('-');
        var desc = ordenarPor?.StartsWith('-') == true;
        IEnumerable<LinhaChave> Dir<TK>(Func<LinhaChave, TK> chave) => desc ? linhas.OrderByDescending(chave) : linhas.OrderBy(chave);

        return campo switch
        {
            "abertura" => Dir(l => l.AbertaEm),
            "pontuacao" => Dir(l => l.Pontuacao),
            "numero" => Dir(l => l.Numero),
            "prazo" => Dir(l => Proximo(l.Status, l.PrazoTriagem, l.PrazoDespacho, l.PrazoInicio, l.PrazoConclusao) ?? DateTimeOffset.MaxValue),
            "municipio" => Dir(l => l.MunicipioId),
            // Fila: encerradas por último; entre cidades, por posição e depois pela política.
            _ => linhas.OrderBy(l => posicoes.ContainsKey(l.Id) ? 0 : 1)
                .ThenBy(l => posicoes.GetValueOrDefault(l.Id, int.MaxValue))
                .ThenBy(l => ParaChave(l), PoliticaOrdenacao.Instancia),
        };
    }

    private static IQueryable<OrdemServico> Escopo(IQueryable<OrdemServico> q, IReadOnlySet<int>? municipios) =>
        municipios is null ? q : Restringir(q, municipios.ToArray());

    private static IQueryable<OrdemServico> Restringir(IQueryable<OrdemServico> q, int[] ids) => q.Where(o => ids.Contains(o.MunicipioId));

    private static IQueryable<LinhaChave> Chaves(IQueryable<OrdemServico> q) =>
        q.Select(o => new LinhaChave(o.Id, o.MunicipioId, o.NivelPrecedencia, o.Prioridade != null ? o.Prioridade.Rank : (int?)null,
            o.TipoManutencao, o.Pontuacao, o.Status, o.PrazoTriagem, o.PrazoDespacho, o.PrazoInicio, o.PrazoConclusao, o.AbertaEm, o.Numero));

    private static ChaveFila ParaChave(LinhaChave l) => new(l.Id, l.MunicipioId, l.NivelPrecedencia, l.Rank ?? 99, l.TipoManutencao,
        l.Pontuacao, Proximo(l.Status, l.PrazoTriagem, l.PrazoDespacho, l.PrazoInicio, l.PrazoConclusao), l.AbertaEm, l.Numero);

    public static DateTimeOffset? Proximo(StatusOrdemServico s, DateTimeOffset? triagem, DateTimeOffset? despacho, DateTimeOffset? inicio, DateTimeOffset? conclusao) =>
        MaquinaEstadosOrdemServico.PrazoCorrente(s) switch
        {
            TipoPrazo.Triagem => triagem,
            TipoPrazo.Despacho => despacho,
            TipoPrazo.Inicio => inicio,
            TipoPrazo.Conclusao => conclusao,
            _ => null,
        };

    public static ProximoPrazoDto? ProximoPrazo(StatusOrdemServico s, DateTimeOffset? triagem, DateTimeOffset? despacho, DateTimeOffset? inicio, DateTimeOffset? conclusao, DateTimeOffset agora)
    {
        var tipo = MaquinaEstadosOrdemServico.PrazoCorrente(s);
        var limite = Proximo(s, triagem, despacho, inicio, conclusao);
        return tipo is null || limite is null ? null
            : new ProximoPrazoDto(tipo.Value, limite.Value, limite <= agora, Math.Round((limite.Value - agora).TotalMinutes, 1));
    }

    private async Task<bool> VersaoDemonstrativaEmVigorAsync(IReadOnlySet<int>? municipios, CancellationToken ct)
    {
        // Existe ao menos uma versão não demonstrativa publicada que valha para o recorte?
        var agora = relogio.Agora;
        var oficiais = await db.VersoesPontuacao.AsNoTracking()
            .Where(v => !v.Demonstrativa && v.Status == StatusVersaoPontuacao.Publicada && v.VigenciaInicio <= agora)
            .Select(v => v.MunicipioId).ToListAsync(ct);
        if (oficiais.Contains(null)) return false;
        return municipios is null || !municipios.All(m => oficiais.Contains(m));
    }

    public static string Mascarar(string nome) =>
        string.Join(' ', nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Length <= 1 ? p : p[0] + new string('*', p.Length - 1)));
}
