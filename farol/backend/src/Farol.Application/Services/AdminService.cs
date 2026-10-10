using System.Text.RegularExpressions;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.Rules;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Services;

/// <summary>Cadastros administráveis: municípios, rede, classes, tipos, prioridades e prazos, regras da camada A, critérios.</summary>
public partial class AdminService(IAppDbContext db, IUsuarioAtual usuario, IRelogio relogio, Auditor auditor, IOptions<FarolOptions> opcoes)
{
    // ---------- Municípios ----------
    public async Task<MunicipioDto> SalvarMunicipioAsync(int? id, MunicipioSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(!string.IsNullOrWhiteSpace(r.Nome) && r.Uf?.Length == 2, "Informe nome e UF (2 letras).");
        Exigir(Regex.IsMatch(r.Prefixo ?? "", "^[A-Z0-9]{2,6}$"), "Prefixo: 2 a 6 letras maiúsculas ou dígitos.");
        Exigir(r.CepUnico is null || Regex.IsMatch(r.CepUnico, @"^\d{8}$"), "CEP único deve ter 8 dígitos.");
        if (await db.Municipios.AnyAsync(m => m.Prefixo == r.Prefixo && m.Id != id, ct)) throw new ConflitoException("Prefixo já usado por outro município.");
        var m = id is { } i ? await db.Municipios.FindAsync([i], ct) ?? throw new NaoEncontradoException("Município não encontrado.") : null;
        var anterior = m is null ? null : CadastroRedeService.Mapear(m);
        if (m is null) db.Municipios.Add(m = new Municipio { Nome = r.Nome, Uf = r.Uf!, Prefixo = r.Prefixo! });
        m.Nome = r.Nome.Trim(); m.Uf = r.Uf!.ToUpperInvariant(); m.CodigoIbge = r.CodigoIbge; m.Prefixo = r.Prefixo!;
        m.Latitude = r.Latitude; m.Longitude = r.Longitude; m.RaioKm = r.RaioKm; m.CepUnico = r.CepUnico; m.Ativo = r.Ativo;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Municipio", m.Nome, id, anterior, r);
        await db.SaveChangesAsync(ct);
        return CadastroRedeService.Mapear(m);
    }

    /// <summary>Município com OS vinculada não é excluído, só inativado (seção 3A.1).</summary>
    public async Task ExcluirMunicipioAsync(int id, CancellationToken ct = default)
    {
        var m = await db.Municipios.FindAsync([id], ct) ?? throw new NaoEncontradoException("Município não encontrado.");
        if (await db.OrdensServico.AnyAsync(o => o.MunicipioId == id, ct) || await db.Solicitacoes.AnyAsync(s => s.MunicipioId == id, ct))
            throw new ConflitoException("Município com OS vinculada não pode ser excluído; inative-o.");
        if (await db.Subestacoes.AnyAsync(s => s.MunicipioId == id, ct) || await db.Equipes.AnyAsync(e => e.MunicipioBaseId == id, ct))
            throw new ConflitoException("Município com cadastros de rede ou equipes vinculados não pode ser excluído; inative-o.");
        db.Municipios.Remove(m);
        auditor.Registrar(AcoesAuditoria.Excluir, "Municipio", id, id, anteriores: CadastroRedeService.Mapear(m));
        await db.SaveChangesAsync(ct);
    }

    // ---------- Rede ----------
    public async Task<int> SalvarLocalidadeAsync(int? id, LocalidadeSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(Regex.IsMatch(r.Codigo ?? "", @"^\d{3}$"), "O código da localidade tem exatamente 3 dígitos.");
        Exigir(!string.IsNullOrWhiteSpace(r.Nome), "Informe o nome.");
        if (await db.Localidades.AnyAsync(l => l.Codigo == r.Codigo && l.Id != id, ct)) throw new ConflitoException("Código de localidade já cadastrado.");
        var l = id is { } i ? await db.Localidades.FindAsync([i], ct) ?? throw new NaoEncontradoException("Localidade não encontrada.") : null;
        if (l is null) db.Localidades.Add(l = new Localidade { Codigo = r.Codigo!, Nome = r.Nome });
        l.Codigo = r.Codigo!; l.Nome = r.Nome.Trim(); l.MunicipioId = r.MunicipioId;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Localidade", r.Codigo!, r.MunicipioId, novos: r);
        await db.SaveChangesAsync(ct);
        return l.Id;
    }

    public async Task<int> SalvarSubestacaoAsync(int? id, SubestacaoSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(!string.IsNullOrWhiteSpace(r.Codigo) && !string.IsNullOrWhiteSpace(r.Nome), "Informe código e nome da subestação.");
        CoordenadaValida(r.Latitude, r.Longitude);
        if (await db.Subestacoes.AnyAsync(s => s.Codigo == r.Codigo && s.MunicipioId == r.MunicipioId && s.Id != id, ct))
            throw new ConflitoException("Código de subestação já cadastrado no município.");
        var s = id is { } i ? await db.Subestacoes.FindAsync([i], ct) ?? throw new NaoEncontradoException("Subestação não encontrada.") : null;
        if (s is null) db.Subestacoes.Add(s = new Subestacao { Codigo = r.Codigo, Nome = r.Nome });
        s.Codigo = r.Codigo.Trim(); s.Nome = r.Nome.Trim(); s.Local = r.Local; s.MunicipioId = r.MunicipioId; s.Latitude = r.Latitude; s.Longitude = r.Longitude;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Subestacao", r.Codigo, r.MunicipioId, novos: r);
        await db.SaveChangesAsync(ct);
        return s.Id;
    }

    public async Task<int> SalvarConjuntoAsync(int? id, ConjuntoSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(!string.IsNullOrWhiteSpace(r.Numero), "Informe o número do conjunto.");
        var sub = await db.Subestacoes.FindAsync([r.SubestacaoId], ct) ?? throw new RegraNegocioException("Subestação inexistente.");
        if (await db.Conjuntos.AnyAsync(c => c.SubestacaoId == r.SubestacaoId && c.Numero == r.Numero && c.Id != id, ct))
            throw new ConflitoException("Conjunto já cadastrado nesta subestação.");
        var c = id is { } i ? await db.Conjuntos.FindAsync([i], ct) ?? throw new NaoEncontradoException("Conjunto não encontrado.") : null;
        if (c is null) db.Conjuntos.Add(c = new ConjuntoEletrico { Numero = r.Numero });
        c.Numero = r.Numero.Trim(); c.SubestacaoId = r.SubestacaoId;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "ConjuntoEletrico", r.Numero, sub.MunicipioId, novos: r);
        await db.SaveChangesAsync(ct);
        return c.Id;
    }

    public async Task<int> SalvarTransformadorAsync(int? id, TransformadorSalvarRequest r, CancellationToken ct = default)
    {
        if (!NumeroTransformador.TentarCriar(r.Numero, opcoes.Value.Transformador.TamanhoMaximo, out var n, out var erro))
            throw new RegraNegocioException(erro!);
        CoordenadaValida(r.Latitude, r.Longitude);
        if (r.ConjuntoId is { } cid && !await db.Conjuntos.AnyAsync(c => c.Id == cid && c.SubestacaoId == r.SubestacaoId, ct))
            throw new RegraNegocioException("O conjunto não pertence à subestação informada.");
        if (await db.Transformadores.AnyAsync(t => t.CodigoLocalidade == n!.CodigoLocalidade && t.NumeroLocal == n.NumeroLocal && t.Id != id, ct))
            throw new ConflitoException("Já existe transformador com esta localidade e número local.");
        var t = id is { } i ? await db.Transformadores.FindAsync([i], ct) ?? throw new NaoEncontradoException("Transformador não encontrado.") : null;
        if (t is null) db.Transformadores.Add(t = new Transformador { NumeroCompleto = n!.Completo, CodigoLocalidade = n.CodigoLocalidade, NumeroLocal = n.NumeroLocal });
        t.NumeroCompleto = n!.Completo; t.CodigoLocalidade = n.CodigoLocalidade; t.NumeroLocal = n.NumeroLocal;
        t.LocalidadeId = await db.Localidades.Where(l => l.Codigo == n.CodigoLocalidade).Select(l => (int?)l.Id).FirstOrDefaultAsync(ct);
        t.MunicipioId = r.MunicipioId; t.SubestacaoId = r.SubestacaoId; t.ConjuntoId = r.ConjuntoId; t.Latitude = r.Latitude; t.Longitude = r.Longitude;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Transformador", n.Completo, r.MunicipioId, novos: r);
        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    // ---------- Classes e tipos (cada um vira opção do critério correspondente) ----------
    public async Task<ClasseClienteDto> SalvarClasseAsync(int? id, ClasseSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? ""), "Código: letras maiúsculas, dígitos e _.");
        Exigir(!string.IsNullOrWhiteSpace(r.Nome), "Informe o nome.");
        if (await db.ClassesCliente.AnyAsync(c => c.Codigo == r.Codigo && c.Id != id, ct)) throw new ConflitoException("Código de classe já cadastrado.");
        var c = id is { } i ? await db.ClassesCliente.FindAsync([i], ct) ?? throw new NaoEncontradoException("Classe não encontrada.") : null;
        var codigoAnterior = c?.Codigo;
        if (c is null) db.ClassesCliente.Add(c = new ClasseCliente { Codigo = r.Codigo!, Nome = r.Nome });
        c.Codigo = r.Codigo!; c.Nome = r.Nome.Trim(); c.Essencial = r.Essencial; c.Ordem = r.Ordem; c.Ativo = r.Ativo;
        await SincronizarOpcaoAsync(Criterios.ClasseCliente, codigoAnterior, c.Codigo, c.Nome, c.Ordem, c.Ativo, ct);
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "ClasseCliente", c.Codigo, novos: r);
        await db.SaveChangesAsync(ct);
        return new ClasseClienteDto(c.Id, c.Codigo, c.Nome, c.Essencial, c.Ordem, c.Ativo);
    }

    public async Task<int> SalvarTipoOcorrenciaAsync(int? id, TipoOcorrenciaSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? ""), "Código: letras maiúsculas, dígitos e _.");
        Exigir(!string.IsNullOrWhiteSpace(r.Nome), "Informe o nome.");
        if (await db.TiposOcorrencia.AnyAsync(t => t.Codigo == r.Codigo && t.Id != id, ct)) throw new ConflitoException("Código já cadastrado.");
        var t = id is { } i ? await db.TiposOcorrencia.Include(x => x.Qualificacoes).FirstOrDefaultAsync(x => x.Id == i, ct)
                              ?? throw new NaoEncontradoException("Tipo não encontrado.") : null;
        var codigoAnterior = t?.Codigo;
        if (t is null) db.TiposOcorrencia.Add(t = new TipoOcorrencia { Codigo = r.Codigo!, Nome = r.Nome });
        t.Codigo = r.Codigo!; t.Nome = r.Nome.Trim(); t.TipoManutencaoSugerido = r.TipoManutencaoSugerido; t.Ordem = r.Ordem; t.Ativo = r.Ativo;
        t.Qualificacoes.Clear();
        t.Qualificacoes.AddRange(r.QualificacoesIds.Distinct().Select(q => new TipoOcorrenciaQualificacao { QualificacaoId = q }));
        await SincronizarOpcaoAsync(Criterios.TipoOcorrencia, codigoAnterior, t.Codigo, t.Nome, t.Ordem, t.Ativo, ct);
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "TipoOcorrencia", t.Codigo, novos: r);
        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    /// <summary>
    /// Cadastrar uma classe ou um tipo cria a opção correspondente no critério. Ela nasce sem pontos:
    /// o Usuário Chave precisa defini-los antes da próxima publicação.
    /// </summary>
    private async Task SincronizarOpcaoAsync(string criterioCodigo, string? codigoAnterior, string codigo, string rotulo, int ordem, bool ativa, CancellationToken ct)
    {
        var criterio = await db.Criterios.Include(c => c.Opcoes).FirstAsync(c => c.Codigo == criterioCodigo, ct);
        var opcao = criterio.Opcoes.FirstOrDefault(o => o.Codigo == (codigoAnterior ?? codigo));
        if (opcao is null) criterio.Opcoes.Add(opcao = new OpcaoCriterio { Codigo = codigo, Rotulo = rotulo });
        opcao.Codigo = codigo; opcao.Rotulo = rotulo; opcao.Ordem = ordem; opcao.Ativa = ativa;
        criterio.AlteradoEm = relogio.Agora;
        criterio.AlteradoPorId = usuario.Id;
    }

    public async Task<ItemCodigoDto> SalvarQualificacaoAsync(int? id, ItemSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? "") && !string.IsNullOrWhiteSpace(r.Nome), "Informe código (MAIÚSCULAS_E_DIGITOS) e nome.");
        var q = id is { } i ? await db.Qualificacoes.FindAsync([i], ct) ?? throw new NaoEncontradoException("Qualificação não encontrada.") : null;
        if (q is null) db.Qualificacoes.Add(q = new Qualificacao { Codigo = r.Codigo, Nome = r.Nome });
        q.Codigo = r.Codigo; q.Nome = r.Nome.Trim();
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Qualificacao", r.Codigo, novos: r);
        await db.SaveChangesAsync(ct);
        return new ItemCodigoDto(q.Id, q.Codigo, q.Nome);
    }

    public async Task<ItemCodigoDto> SalvarRecursoAsync(int? id, ItemSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? "") && !string.IsNullOrWhiteSpace(r.Nome), "Informe código (MAIÚSCULAS_E_DIGITOS) e nome.");
        var q = id is { } i ? await db.Recursos.FindAsync([i], ct) ?? throw new NaoEncontradoException("Recurso não encontrado.") : null;
        if (q is null) db.Recursos.Add(q = new Recurso { Codigo = r.Codigo, Nome = r.Nome });
        q.Codigo = r.Codigo; q.Nome = r.Nome.Trim();
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Recurso", r.Codigo, novos: r);
        await db.SaveChangesAsync(ct);
        return new ItemCodigoDto(q.Id, q.Codigo, q.Nome);
    }

    // ---------- Códigos de prioridade e prazos (seção 8) ----------
    public async Task<PrioridadeDto> SalvarPrioridadeAsync(int? id, PrioridadeSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? "") && !string.IsNullOrWhiteSpace(r.Nome), "Informe código e nome.");
        Exigir(r.Rank >= 1, "O rank começa em 1 (mais urgente).");
        Exigir(Regex.IsMatch(r.Cor ?? "", "^#[0-9A-Fa-f]{6}$"), "Cor no formato #RRGGBB.");
        Exigir(new[] { r.PrazoTriagemMin, r.PrazoDespachoMin, r.PrazoInicioMin, r.PrazoConclusaoMin }.All(x => x > 0) && r.PrazoRestabelecimentoMin is null or > 0,
            "Prazos devem ser positivos.");
        Exigir(r.PrazoTriagemMin <= r.PrazoDespachoMin && r.PrazoDespachoMin <= r.PrazoInicioMin && r.PrazoInicioMin <= r.PrazoConclusaoMin,
            "Os prazos devem ser crescentes: triagem ≤ despacho ≤ início ≤ conclusão.");
        if (await db.Prioridades.AnyAsync(p => p.Codigo == r.Codigo && p.Id != id, ct)) throw new ConflitoException("Código de prioridade já cadastrado.");
        var p = id is { } i ? await db.Prioridades.FindAsync([i], ct) ?? throw new NaoEncontradoException("Prioridade não encontrada.") : null;
        var anterior = p is null ? null : CadastroRedeService.MapearPrioridade(p);
        if (p is null) db.Prioridades.Add(p = new Prioridade { Codigo = r.Codigo!, Nome = r.Nome, Cor = r.Cor!, VigenciaInicio = relogio.Agora });
        p.Codigo = r.Codigo!; p.Nome = r.Nome.Trim(); p.Descricao = r.Descricao; p.Rank = r.Rank; p.Cor = r.Cor!; p.Critica = r.Critica;
        p.PrazoTriagemMin = r.PrazoTriagemMin; p.PrazoDespachoMin = r.PrazoDespachoMin; p.PrazoInicioMin = r.PrazoInicioMin;
        p.PrazoRestabelecimentoMin = r.PrazoRestabelecimentoMin; p.PrazoConclusaoMin = r.PrazoConclusaoMin; p.UnidadePrazo = r.UnidadePrazo;
        p.Calendario = r.Calendario; p.ConsideraFeriados = r.ConsideraFeriados; p.TratamentoCritico = r.TratamentoCritico; p.Escalonamento = r.Escalonamento;
        p.VigenciaInicio = r.VigenciaInicio ?? p.VigenciaInicio; p.VigenciaFim = r.VigenciaFim; p.Ativo = r.Ativo;
        p.Demonstrativa = false;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Prioridade", p.Codigo, anteriores: anterior, novos: r);
        await db.SaveChangesAsync(ct);
        return CadastroRedeService.MapearPrioridade(p);
    }

    // ---------- Regras de precedência (camada A) ----------
    public async Task<List<RegraPrecedenciaDto>> RegrasAsync(CancellationToken ct = default)
    {
        var criterios = await db.Criterios.AsNoTracking().Include(c => c.Opcoes).ToDictionaryAsync(c => c.Codigo, ct);
        return (await db.RegrasPrecedencia.AsNoTracking().Include(r => r.PrioridadeMinima).OrderByDescending(r => r.NivelPrecedencia).ThenBy(r => r.Codigo).ToListAsync(ct))
            .Select(r => PontuacaoService.MapearRegra(r, criterios)).ToList();
    }

    /// <summary>Alterar uma regra exige justificativa e registra quem aprovou (o administrador que grava).</summary>
    public async Task<RegraPrecedenciaDto> SalvarRegraAsync(int? id, RegraSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? "") && !string.IsNullOrWhiteSpace(r.Nome), "Informe código e nome.");
        Exigir(!string.IsNullOrWhiteSpace(r.Justificativa), "Alterações na camada A exigem justificativa.");
        Exigir(r.Condicoes.Count > 0, "A regra precisa de ao menos uma condição.");
        Exigir(r.NivelPrecedencia is >= 1 and <= 10, "Nível de precedência entre 1 e 10.");
        var criterios = await db.Criterios.AsNoTracking().Include(c => c.Opcoes).ToDictionaryAsync(c => c.Codigo, ct);
        foreach (var c in r.Condicoes)
        {
            Exigir(criterios.TryGetValue(c.Criterio, out var cr), $"Critério {c.Criterio} inexistente.");
            Exigir(c.Opcoes.Length > 0 && c.Opcoes.All(o => criterios[c.Criterio].Opcoes.Any(x => x.Codigo == o)), $"Opções inválidas em {c.Criterio}.");
        }
        Exigir(await db.Prioridades.AnyAsync(p => p.Id == r.PrioridadeMinimaId, ct), "Prioridade mínima inexistente.");
        if (await db.RegrasPrecedencia.AnyAsync(x => x.Codigo == r.Codigo && x.Id != id, ct)) throw new ConflitoException("Código de regra já cadastrado.");

        var regra = id is { } i ? await db.RegrasPrecedencia.FindAsync([i], ct) ?? throw new NaoEncontradoException("Regra não encontrada.") : null;
        var anterior = regra is null ? null : new { regra.Condicoes, regra.NivelPrecedencia, regra.PrioridadeMinimaId, regra.Ativa, regra.Versao };
        if (regra is null) db.RegrasPrecedencia.Add(regra = new RegraPrecedencia { Codigo = r.Codigo!, Nome = r.Nome, Versao = 0 });
        regra.Codigo = r.Codigo!; regra.Nome = r.Nome.Trim(); regra.Descricao = r.Descricao; regra.Condicoes = r.Condicoes;
        regra.NivelPrecedencia = r.NivelPrecedencia; regra.PrioridadeMinimaId = r.PrioridadeMinimaId; regra.Ativa = r.Ativa;
        regra.Versao += 1; regra.Demonstrativa = false; regra.AprovadaPorId = usuario.Id; regra.AprovadaEm = relogio.Agora; regra.AlteradaEm = relogio.Agora;
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "RegraPrecedencia", regra.Codigo, anteriores: anterior, novos: r, justificativa: r.Justificativa);
        await db.SaveChangesAsync(ct);
        await db.Entry(regra).Reference(x => x.PrioridadeMinima).LoadAsync(ct);
        return PontuacaoService.MapearRegra(regra, criterios);
    }

    // ---------- Critérios (a estrutura é do administrador; os pontos, do Usuário Chave) ----------
    public async Task<List<CriterioAdminDto>> CriteriosAsync(CancellationToken ct = default) =>
        (await db.Criterios.AsNoTracking().Include(c => c.Opcoes).Include(c => c.Municipios).OrderBy(c => c.Ordem).ToListAsync(ct))
        .Select(MapearCriterio).ToList();

    public async Task<CriterioAdminDto> SalvarCriterioAsync(int? id, CriterioSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(CodigoValido().IsMatch(r.Codigo ?? "") && !string.IsNullOrWhiteSpace(r.Nome), "Informe código e nome.");
        var criterio = id is { } i ? await db.Criterios.Include(c => c.Opcoes).Include(c => c.Municipios).FirstOrDefaultAsync(c => c.Id == i, ct)
                                     ?? throw new NaoEncontradoException("Critério não encontrado.") : null;
        if (await db.Criterios.AnyAsync(c => c.Codigo == r.Codigo && c.Id != id, ct)) throw new ConflitoException("Código de critério já cadastrado.");
        var agora = relogio.Agora;

        if (criterio is { Tipo: TipoCriterio.Fixo })
        {
            // Critério fixo: só descrição, regra de aplicação, agregação, escopo e ativação mudam; as opções seguem o código.
            criterio.Descricao = r.Descricao; criterio.RegraAplicacao = r.RegraAplicacao; criterio.Agregacao = r.Agregacao; criterio.Ativo = r.Ativo;
        }
        else
        {
            Exigir(r.Opcoes.Count >= 2, "Critério personalizado precisa de ao menos duas opções.");
            Exigir(r.Opcoes.Any(o => o.RepresentaDesconhecido), "Inclua uma opção que represente \"não informado / desconhecido\".");
            Exigir(r.Opcoes.All(o => CodigoValido().IsMatch(o.Codigo) && !string.IsNullOrWhiteSpace(o.Rotulo)), "Opções: código e rótulo obrigatórios.");
            Exigir(r.Opcoes.Select(o => o.Codigo).Distinct().Count() == r.Opcoes.Count, "Códigos de opção repetidos.");
            if (criterio is null)
            {
                var ordem = (await db.Criterios.MaxAsync(c => (int?)c.Ordem, ct) ?? 0) + 1;
                db.Criterios.Add(criterio = new Criterio { Codigo = r.Codigo!, Nome = r.Nome, Tipo = TipoCriterio.Personalizado, Ordem = ordem, CriadoEm = agora, CriadoPorId = usuario.Id });
            }
            criterio.Codigo = r.Codigo!; criterio.Descricao = r.Descricao; criterio.RegraAplicacao = r.RegraAplicacao ?? "Respondido pelo atendente no formulário.";
            criterio.Agregacao = r.Agregacao; criterio.MultiplaEscolha = r.MultiplaEscolha; criterio.Ativo = r.Ativo;
            for (var k = 0; k < r.Opcoes.Count; k++)
            {
                var o = r.Opcoes[k];
                var existente = criterio.Opcoes.FirstOrDefault(x => x.Codigo == o.Codigo);
                if (existente is null) criterio.Opcoes.Add(existente = new OpcaoCriterio { Codigo = o.Codigo, Rotulo = o.Rotulo });
                existente.Rotulo = o.Rotulo.Trim(); existente.Ordem = k; existente.RepresentaDesconhecido = o.RepresentaDesconhecido; existente.Ativa = true;
            }
            foreach (var removida in criterio.Opcoes.Where(x => r.Opcoes.All(o => o.Codigo != x.Codigo))) removida.Ativa = false;
        }
        criterio.Nome = r.Nome.Trim();
        criterio.AlteradoEm = agora;
        criterio.AlteradoPorId = usuario.Id;
        criterio.Municipios.Clear();
        criterio.Municipios.AddRange(r.MunicipiosIds.Distinct().Select(m => new CriterioMunicipio { MunicipioId = m }));
        auditor.Registrar(id is null ? AcoesAuditoria.Criar : AcoesAuditoria.Alterar, "Criterio", r.Codigo!, novos: r);
        await db.SaveChangesAsync(ct);
        return MapearCriterio(criterio);
    }

    private static CriterioAdminDto MapearCriterio(Criterio c) =>
        new(c.Id, c.Codigo, c.Nome, c.Descricao, c.RegraAplicacao, c.Tipo, c.Agregacao, c.MultiplaEscolha, c.Ordem, c.Ativo,
            c.Opcoes.Where(o => o.Ativa).OrderBy(o => o.Ordem).Select(o => new OpcaoFormularioDto(o.Id, o.Codigo, o.Rotulo, o.RepresentaDesconhecido)).ToList(),
            c.Municipios.Select(m => m.MunicipioId).ToList(), c.CriadoEm, c.AlteradoEm);

    // ---------- Feriados ----------
    public async Task<List<FeriadoDto>> FeriadosAsync(CancellationToken ct = default) =>
        await db.Feriados.AsNoTracking().OrderBy(f => f.Data).Select(f => new FeriadoDto(f.Id, f.Data, f.Nome, f.MunicipioId)).ToListAsync(ct);

    public async Task<FeriadoDto> SalvarFeriadoAsync(FeriadoSalvarRequest r, CancellationToken ct = default)
    {
        Exigir(!string.IsNullOrWhiteSpace(r.Nome), "Informe o nome do feriado.");
        var f = new Feriado { Data = r.Data, Nome = r.Nome.Trim(), MunicipioId = r.MunicipioId };
        db.Feriados.Add(f);
        auditor.Registrar(AcoesAuditoria.Criar, "Feriado", r.Data.ToString("yyyy-MM-dd"), r.MunicipioId, novos: r);
        await db.SaveChangesAsync(ct);
        return new FeriadoDto(f.Id, f.Data, f.Nome, f.MunicipioId);
    }

    public async Task ExcluirFeriadoAsync(int id, CancellationToken ct = default)
    {
        var f = await db.Feriados.FindAsync([id], ct) ?? throw new NaoEncontradoException("Feriado não encontrado.");
        db.Feriados.Remove(f);
        auditor.Registrar(AcoesAuditoria.Excluir, "Feriado", id, f.MunicipioId, anteriores: new { f.Data, f.Nome });
        await db.SaveChangesAsync(ct);
    }

    // ---------- Auditoria ----------
    public async Task<PaginaDto<AuditoriaDto>> AuditoriaAsync(FiltroAuditoria f, CancellationToken ct = default)
    {
        var q = db.Auditorias.AsNoTracking().AsQueryable();
        var permitidos = (await usuario.MunicipiosPermitidosAsync(ct))?.ToArray();
        if (permitidos is not null) q = q.Where(a => a.MunicipioId == null || permitidos.Contains(a.MunicipioId.Value));
        if (!string.IsNullOrWhiteSpace(f.Entidade)) q = q.Where(a => a.Entidade == f.Entidade);
        if (!string.IsNullOrWhiteSpace(f.EntidadeId)) q = q.Where(a => a.EntidadeId == f.EntidadeId);
        if (!string.IsNullOrWhiteSpace(f.Acao)) q = q.Where(a => a.Acao == f.Acao);
        if (f.UsuarioId is { } u) q = q.Where(a => a.UsuarioId == u);
        if (f.MunicipioId is { } m) q = q.Where(a => a.MunicipioId == m);
        if (f.De is { } de) q = q.Where(a => a.OcorridoEm >= de);
        if (f.Ate is { } ate) q = q.Where(a => a.OcorridoEm <= ate);
        var total = await q.CountAsync(ct);
        var tamanho = Math.Clamp(f.TamanhoPagina, 1, 200);
        var itens = await q.OrderByDescending(a => a.Id).Skip((Math.Max(1, f.Pagina) - 1) * tamanho).Take(tamanho)
            .Select(a => new AuditoriaDto(a.Id, a.OcorridoEm, a.UsuarioNome, a.Acao, a.Entidade, a.EntidadeId, a.MunicipioId, a.ValoresAnteriores, a.ValoresNovos, a.Justificativa))
            .ToListAsync(ct);
        return new PaginaDto<AuditoriaDto>(itens, total, f.Pagina, tamanho);
    }

    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao) throw new RegraNegocioException(mensagem);
    }

    private static void CoordenadaValida(double? lat, double? lng)
    {
        if (lat.HasValue != lng.HasValue) throw new RegraNegocioException("Informe latitude e longitude juntas.");
        if (lat is { } a && lng is { } b && !new Coordenadas(a, b).DentroDosLimites) throw new RegraNegocioException("Coordenadas fora dos intervalos válidos.");
    }

    [GeneratedRegex("^[A-Z0-9_]{2,40}$")]
    private static partial Regex CodigoValido();
}
