using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Farol.Application.Services;

/// <summary>Cadastro e consulta de equipes (seção 11), recortado por cidade.</summary>
public class EquipeService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    IRelogio relogio,
    INotificadorFila notificador,
    EscopoMunicipio escopo,
    Auditor auditor)
{
    public async Task<List<Equipe>> ListarEntidadesAsync(CancellationToken ct = default) =>
        await db.Equipes.AsNoTracking()
            .Include(e => e.MunicipioBase)
            .Include(e => e.MunicipiosAdicionais).ThenInclude(m => m.Municipio)
            .Include(e => e.Integrantes)
            .Include(e => e.Qualificacoes).ThenInclude(q => q.Qualificacao)
            .Include(e => e.Recursos).ThenInclude(r => r.Recurso)
            .AsSplitQuery()
            .OrderBy(e => e.Codigo)
            .ToListAsync(ct);

    public async Task<List<EquipeDto>> ListarAsync(int? municipioId, bool somenteDisponiveis, CancellationToken ct = default)
    {
        var municipios = await escopo.ResolverAsync(municipioId, ct);
        var todas = await ListarEntidadesAsync(ct);
        var visiveis = todas.Where(e => municipios is null || municipios.Contains(e.MunicipioBaseId)
                                        || e.MunicipiosAdicionais.Any(m => municipios.Contains(m.MunicipioId))).ToList();
        var atribuicoes = await AtribuicoesAsync(visiveis.Select(e => e.Id).ToList(), ct);
        var dtos = visiveis.Select(e => Mapear(e, atribuicoes.GetValueOrDefault(e.Id, []).Count, atribuicoes.GetValueOrDefault(e.Id, []))).ToList();
        return somenteDisponiveis ? dtos.Where(d => d.Disponivel).ToList() : dtos;
    }

    public async Task<EquipeDto> ObterAsync(int id, CancellationToken ct = default)
    {
        var e = (await ListarEntidadesAsync(ct)).FirstOrDefault(x => x.Id == id) ?? throw new NaoEncontradoException("Equipe não encontrada.");
        await ExigirEscopoAsync(e, ct);
        var atribuicoes = await AtribuicoesAsync([id], ct);
        var lista = atribuicoes.GetValueOrDefault(id, []);
        return Mapear(e, lista.Count, lista);
    }

    public async Task<List<object>> HistoricoLocalizacaoAsync(int id, CancellationToken ct = default)
    {
        await ObterAsync(id, ct);
        return await db.LocalizacoesEquipe.AsNoTracking().Where(l => l.EquipeId == id).OrderByDescending(l => l.RegistradaEm).Take(50)
            .Select(l => (object)new { l.Latitude, l.Longitude, Origem = l.Origem.ToString(), l.RegistradaEm }).ToListAsync(ct);
    }

    /// <summary>Atualiza a posição conhecida. Sem GPS integrado, a origem fica registrada como "cadastrada".</summary>
    public async Task AtualizarLocalizacaoAsync(int id, LocalizacaoEquipeRequest r, CancellationToken ct = default)
    {
        if (!new Coordenadas(r.Latitude, r.Longitude).DentroDosLimites) throw new RegraNegocioException("Coordenadas fora dos intervalos válidos.");
        var e = await db.Equipes.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NaoEncontradoException("Equipe não encontrada.");
        await ExigirEscopoAsync(e, ct);
        if (!usuario.Tem(Permissoes.EquipesAdministrar) && !usuario.Tem(Permissoes.DespachoDesignar) && await usuario.EquipeIdAsync(ct) != id)
            throw new AcessoNegadoException("Você não pode atualizar a localização desta equipe.");

        var agora = relogio.Agora;
        e.Latitude = r.Latitude;
        e.Longitude = r.Longitude;
        e.LocalizacaoAtualizadaEm = agora;
        e.OrigemLocalizacao = r.Origem;
        db.LocalizacoesEquipe.Add(new LocalizacaoEquipe
        {
            EquipeId = id, Latitude = r.Latitude, Longitude = r.Longitude, Origem = r.Origem, RegistradaEm = agora, RegistradaPorId = usuario.Id,
        });
        await db.SaveChangesAsync(ct);
        await notificador.FilaAlteradaAsync(e.MunicipioBaseId, "equipe-localizacao", ct);
    }

    public async Task AlterarStatusAsync(int id, StatusEquipeRequest r, CancellationToken ct = default)
    {
        var e = await db.Equipes.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NaoEncontradoException("Equipe não encontrada.");
        await ExigirEscopoAsync(e, ct);
        var ativos = await db.Despachos.CountAsync(d => d.EquipeId == id && d.Ativo, ct);
        if (ativos > 0 && r.Status == StatusEquipe.Disponivel && e.Status is StatusEquipe.ACaminho or StatusEquipe.EmAtendimento)
            throw new ConflitoException("A equipe tem despacho ativo; o status segue o ciclo do despacho.");
        if (r.Status is StatusEquipe.Indisponivel or StatusEquipe.EmPausa && string.IsNullOrWhiteSpace(r.Justificativa))
            throw new RegraNegocioException("Informe a justificativa.");
        var anterior = e.Status;
        e.Status = r.Status;
        auditor.Registrar(AcoesAuditoria.AlterarStatus, "Equipe", id, e.MunicipioBaseId, new { status = anterior }, new { status = r.Status }, r.Justificativa);
        await db.SaveChangesAsync(ct);
        await notificador.FilaAlteradaAsync(e.MunicipioBaseId, "equipe-status", ct);
    }

    public async Task<EquipeDto> SalvarAsync(int? id, EquipeSalvarRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Codigo) || string.IsNullOrWhiteSpace(r.Nome)) throw new RegraNegocioException("Informe código e nome.");
        if (r.Capacidade is < 1 or > 10) throw new RegraNegocioException("Capacidade deve estar entre 1 e 10.");
        if (r.Integrantes.Count == 0) throw new RegraNegocioException("A equipe precisa de ao menos um integrante.");
        await escopo.ExigirAsync(r.MunicipioBaseId, "Equipe", id?.ToString() ?? r.Codigo, ct);
        if (await db.Equipes.AnyAsync(e => e.Codigo == r.Codigo.Trim() && e.Id != id, ct)) throw new ConflitoException("Já existe equipe com este código.");

        Equipe e;
        if (id is { } existente)
        {
            e = await db.Equipes.Include(x => x.MunicipiosAdicionais).Include(x => x.Integrantes).Include(x => x.Qualificacoes).Include(x => x.Recursos)
                    .FirstOrDefaultAsync(x => x.Id == existente, ct) ?? throw new NaoEncontradoException("Equipe não encontrada.");
            e.MunicipiosAdicionais.Clear(); e.Integrantes.Clear(); e.Qualificacoes.Clear(); e.Recursos.Clear();
        }
        else
        {
            e = new Equipe { Codigo = r.Codigo.Trim(), Nome = r.Nome.Trim() };
            db.Equipes.Add(e);
        }
        e.Codigo = r.Codigo.Trim();
        e.Nome = r.Nome.Trim();
        e.MunicipioBaseId = r.MunicipioBaseId;
        e.Status = r.Status;
        e.Capacidade = r.Capacidade;
        e.Ativa = r.Ativa;
        e.MunicipiosAdicionais.AddRange(r.MunicipiosAdicionaisIds.Where(m => m != r.MunicipioBaseId).Distinct().Select(m => new EquipeMunicipio { MunicipioId = m }));
        e.Integrantes.AddRange(r.Integrantes.Select(i => new Integrante { Nome = i.Nome.Trim(), Matricula = i.Matricula, Funcao = i.Funcao }));
        e.Qualificacoes.AddRange(r.QualificacoesIds.Distinct().Select(q => new EquipeQualificacao { QualificacaoId = q }));
        e.Recursos.AddRange(r.RecursosIds.Distinct().Select(x => new EquipeRecurso { RecursoId = x }));
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Equipe", e.Codigo, r.MunicipioBaseId, novos: r);
        await db.SaveChangesAsync(ct);
        return await ObterAsync(e.Id, ct);
    }

    private async Task ExigirEscopoAsync(Equipe e, CancellationToken ct)
    {
        var permitidos = await usuario.MunicipiosPermitidosAsync(ct);
        if (permitidos is null || permitidos.Contains(e.MunicipioBaseId)) return;
        var adicionais = await db.Equipes.Where(x => x.Id == e.Id).SelectMany(x => x.MunicipiosAdicionais.Select(m => m.MunicipioId)).ToListAsync(ct);
        if (adicionais.Any(permitidos.Contains)) return;
        await escopo.ExigirAsync(e.MunicipioBaseId, "Equipe", e.Id, ct);
    }

    private async Task<Dictionary<int, List<OsAtribuidaDto>>> AtribuicoesAsync(List<int> ids, CancellationToken ct) =>
        (await db.Despachos.AsNoTracking().Where(d => d.Ativo && ids.Contains(d.EquipeId))
            .Select(d => new { d.EquipeId, Dto = new OsAtribuidaDto(d.OrdemServicoId, d.OrdemServico!.Numero, d.OrdemServico.Status, d.DesignadoEm) })
            .ToListAsync(ct))
        .GroupBy(x => x.EquipeId).ToDictionary(g => g.Key, g => g.Select(x => x.Dto).ToList());

    public EquipeDto Mapear(Equipe e, int despachosAtivos, List<OsAtribuidaDto> atribuidas)
    {
        var (disponivel, _) = DespachoService.Disponibilidade(e, despachosAtivos);
        return new EquipeDto(e.Id, e.Codigo, e.Nome, e.MunicipioBaseId, e.MunicipioBase?.Nome ?? "",
            e.MunicipiosAdicionais.Select(m => new ItemCodigoDto(m.MunicipioId, m.Municipio?.Prefixo ?? "", m.Municipio?.Nome ?? "")).ToList(),
            e.Status, e.Capacidade, despachosAtivos, disponivel,
            usuario.Tem(Permissoes.LocalizacaoConsultar) ? e.Latitude : null,
            usuario.Tem(Permissoes.LocalizacaoConsultar) ? e.Longitude : null,
            e.LocalizacaoAtualizadaEm, e.OrigemLocalizacao,
            e.Integrantes.OrderBy(i => i.Nome).Select(i => new IntegranteDto(i.Id, i.Nome, i.Matricula, i.Funcao)).ToList(),
            e.Qualificacoes.Select(q => new ItemCodigoDto(q.QualificacaoId, q.Qualificacao?.Codigo ?? "", q.Qualificacao?.Nome ?? "")).OrderBy(q => q.Nome).ToList(),
            e.Recursos.Select(r => new ItemCodigoDto(r.RecursoId, r.Recurso?.Codigo ?? "", r.Recurso?.Nome ?? "")).OrderBy(r => r.Nome).ToList(),
            atribuidas, e.Ativa, e.Demonstrativa);
    }
}
