using System.Text.RegularExpressions;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using Farol.Domain.Exceptions;
using Farol.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.Application.Services;

/// <summary>Consultas de apoio ao atendimento: rede, UC, CEP, coordenadas e catálogos.</summary>
public class CadastroRedeService(
    IAppDbContext db,
    IUsuarioAtual usuario,
    Auditor auditor,
    IProvedorCep provedorCep,
    IProvedorRoteamento roteamento,
    IOptions<FarolOptions> opcoes)
{
    private readonly FarolOptions _opcoes = opcoes.Value;

    public async Task<List<MunicipioDto>> MunicipiosAsync(bool somenteDoUsuario, CancellationToken ct = default)
    {
        var permitidos = somenteDoUsuario ? await usuario.MunicipiosPermitidosAsync(ct) : null;
        var lista = await db.Municipios.AsNoTracking().OrderBy(m => m.Nome).ToListAsync(ct);
        return lista.Where(m => permitidos is null || permitidos.Contains(m.Id)).Select(Mapear).ToList();
    }

    public static MunicipioDto Mapear(Municipio m) =>
        new(m.Id, m.Nome, m.Uf, m.CodigoIbge, m.Prefixo, m.Latitude, m.Longitude, m.RaioKm, m.CepUnico, m.Ativo, m.Demonstrativo);

    public async Task<List<SubestacaoDto>> SubestacoesAsync(int municipioId, CancellationToken ct = default) =>
        await db.Subestacoes.AsNoTracking().Where(s => s.MunicipioId == municipioId).OrderBy(s => s.Codigo)
            .Select(s => new SubestacaoDto(s.Id, s.Codigo, s.Nome, s.Local, s.MunicipioId, s.Latitude, s.Longitude, s.Conjuntos.Count, s.Demonstrativo))
            .ToListAsync(ct);

    public async Task<List<ConjuntoDto>> ConjuntosAsync(int subestacaoId, CancellationToken ct = default) =>
        await db.Conjuntos.AsNoTracking().Where(c => c.SubestacaoId == subestacaoId).OrderBy(c => c.Numero)
            .Select(c => new ConjuntoDto(c.Id, c.SubestacaoId, c.Numero, c.Demonstrativo)).ToListAsync(ct);

    public async Task<List<LocalidadeDto>> LocalidadesAsync(int? municipioId, CancellationToken ct = default) =>
        await db.Localidades.AsNoTracking().Where(l => municipioId == null || l.MunicipioId == municipioId).OrderBy(l => l.Codigo)
            .Select(l => new LocalidadeDto(l.Id, l.Codigo, l.Nome, l.MunicipioId, l.Municipio!.Nome, l.Demonstrativo)).ToListAsync(ct);

    public async Task<PaginaDto<TransformadorDto>> TransformadoresAsync(int? municipioId, string? numero, int pagina, int tamanho, CancellationToken ct = default)
    {
        var q = db.Transformadores.AsNoTracking().Where(t => municipioId == null || t.MunicipioId == municipioId);
        if (!string.IsNullOrWhiteSpace(numero)) q = q.Where(t => t.NumeroCompleto.StartsWith(numero.Trim()));
        var total = await q.CountAsync(ct);
        tamanho = Math.Clamp(tamanho, 1, 200);
        var itens = await Projetar(q.OrderBy(t => t.NumeroCompleto).Skip((Math.Max(1, pagina) - 1) * tamanho).Take(tamanho)).ToListAsync(ct);
        return new PaginaDto<TransformadorDto>(itens, total, pagina, tamanho);
    }

    private IQueryable<TransformadorDto> Projetar(IQueryable<Transformador> q) =>
        q.Select(t => new TransformadorDto(t.Id, t.NumeroCompleto, t.CodigoLocalidade, t.NumeroLocal,
            t.Localidade != null ? t.Localidade.Nome : null, t.SubestacaoId, t.Subestacao != null ? t.Subestacao.Codigo : null,
            t.ConjuntoId, t.Conjunto != null ? t.Conjunto.Numero : null, t.MunicipioId,
            db.UnidadesConsumidoras.Count(u => u.TransformadorId == t.Id), t.Demonstrativo));

    /// <summary>Interpretação do número: 3 dígitos de localidade + número local. Nunca converte para número.</summary>
    public async Task<InterpretacaoTransformadorDto> InterpretarTransformadorAsync(string? numero, int? municipioId, CancellationToken ct = default)
    {
        if (!NumeroTransformador.TentarCriar(numero, _opcoes.Transformador.TamanhoMaximo, out var n, out var erro))
            return new InterpretacaoTransformadorDto(false, erro, numero, null, null, null, false, null, []);

        var alertas = new List<string>();
        var localidade = await db.Localidades.AsNoTracking().Where(l => l.Codigo == n!.CodigoLocalidade)
            .Select(l => new LocalidadeDto(l.Id, l.Codigo, l.Nome, l.MunicipioId, l.Municipio!.Nome, l.Demonstrativo)).FirstOrDefaultAsync(ct);
        if (localidade is null) alertas.Add("Localidade não cadastrada, confirmar.");
        else if (municipioId is { } m && localidade.MunicipioId != m) alertas.Add($"A localidade {localidade.Codigo} pertence a {localidade.Municipio}.");

        var transformador = await Projetar(db.Transformadores.AsNoTracking()
            .Where(t => t.CodigoLocalidade == n!.CodigoLocalidade && t.NumeroLocal == n.NumeroLocal)).FirstOrDefaultAsync(ct);
        if (transformador is not null) alertas.Add("Circuito e conjunto identificados pelo cadastro, a confirmar.");
        else alertas.Add("Transformador não encontrado no cadastro da rede: será registrado como informado, sem confirmação.");

        return new InterpretacaoTransformadorDto(true, null, n!.Completo, n.CodigoLocalidade, n.NumeroLocal, localidade, localidade is not null, transformador, alertas);
    }

    /// <summary>Busca de UC (seção 4.2.1): só os dados necessários, nome mascarado, consulta auditada (LGPD).</summary>
    public async Task<UnidadeConsumidoraDto> UnidadeConsumidoraAsync(string numero, CancellationToken ct = default)
    {
        var valor = (numero ?? "").Trim();
        if (!Regex.IsMatch(valor, _opcoes.Uc.Formato)) throw new RegraNegocioException("Formato de UC inválido.");

        var uc = await db.UnidadesConsumidoras.AsNoTracking()
            .Include(u => u.ClasseCliente).Include(u => u.Municipio)
            .Include(u => u.Transformador).ThenInclude(t => t!.Subestacao)
            .Include(u => u.Transformador).ThenInclude(t => t!.Conjunto)
            .FirstOrDefaultAsync(u => u.Numero == valor, ct);

        await auditor.RegistrarAgoraAsync(AcoesAuditoria.ConsultaUc, "UnidadeConsumidora", valor, uc?.MunicipioId,
            uc is null ? "UC não encontrada no cadastro." : null, ct);
        if (uc is null) throw new NaoEncontradoException("UC não encontrada no cadastro. Ela pode ser registrada como \"não validada\".");

        var permitidos = await usuario.MunicipiosPermitidosAsync(ct);
        if (permitidos is not null && !permitidos.Contains(uc.MunicipioId))
            throw new AcessoNegadoException("A UC pertence a um município ao qual você não tem acesso.");

        var t = uc.Transformador;
        return new UnidadeConsumidoraDto(uc.Numero, OrdemServicoConsultaService.Mascarar(uc.ClienteNome), uc.ClasseClienteId, uc.ClasseCliente!.Nome,
            uc.Situacao == SituacaoUnidade.Ligado ? "LIGADO" : "DESLIGADO", uc.Logradouro, uc.NumeroImovel, uc.Bairro, uc.Cep,
            uc.MunicipioId, uc.Municipio!.Nome, t?.NumeroCompleto, t?.SubestacaoId, t?.Subestacao?.Codigo, t?.ConjuntoId, t?.Conjunto?.Numero,
            uc.Latitude, uc.Longitude, uc.Demonstrativo);
    }

    public async Task<CepDto> CepAsync(string cep, CancellationToken ct = default)
    {
        var digitos = new string((cep ?? "").Where(char.IsAsciiDigit).ToArray());
        if (digitos.Length != 8) throw new RegraNegocioException("CEP deve ter 8 dígitos.");
        var r = await provedorCep.ConsultarAsync(digitos, ct);
        var unico = r.MunicipioId is { } mid && await db.Municipios.AnyAsync(m => m.Id == mid && m.CepUnico == digitos, ct);
        var aviso = !r.Encontrado ? "CEP não encontrado ou serviço indisponível: preencha o endereço manualmente."
            : unico ? "Município com CEP único: o CEP não identifica a rua. Preencha a rua manualmente." : null;
        return new CepDto(r.Encontrado, $"{digitos[..5]}-{digitos[5..]}", unico ? null : r.Logradouro, unico ? null : r.Bairro,
            r.MunicipioId, r.MunicipioNome, unico, r.Fonte, aviso);
    }

    public async Task<CoordenadasInterpretadasDto> InterpretarCoordenadasAsync(InterpretarCoordenadasRequest r, CancellationToken ct = default)
    {
        if (!InterpretadorCoordenadas.TentarInterpretar(r.Texto, out var c))
            return new CoordenadasInterpretadasDto(false, null, null, [], "Não foi possível encontrar um par de coordenadas no texto.");
        if (!c.DentroDosLimites)
            return new CoordenadasInterpretadasDto(false, c.Latitude, c.Longitude, [], "Latitude deve estar entre -90 e 90 e longitude entre -180 e 180.");
        var municipio = r.MunicipioId is { } m ? await db.Municipios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == m, ct) : null;
        return new CoordenadasInterpretadasDto(true, Math.Round(c.Latitude, 6), Math.Round(c.Longitude, 6),
            MontadorOrdemServico.AlertasCoordenada(c, municipio), null);
    }

    public async Task<CatalogoDto> CatalogoAsync(int? municipioId = null, CancellationToken ct = default)
    {
        var criterios = await db.Criterios.AsNoTracking().Include(c => c.Opcoes).Include(c => c.Municipios)
            .Where(c => c.Ativo && (c.Municipios.Count == 0 || municipioId == null || c.Municipios.Any(m => m.MunicipioId == municipioId)))
            .OrderBy(c => c.Ordem).ToListAsync(ct);
        CriterioFormularioDto Criterio(Criterio c) => new(c.Id, c.Codigo, c.Nome, c.Descricao, c.MultiplaEscolha,
            c.Opcoes.Where(o => o.Ativa).OrderBy(o => o.Ordem).Select(o => new OpcaoFormularioDto(o.Id, o.Codigo, o.Rotulo, o.RepresentaDesconhecido)).ToList());

        return new CatalogoDto(
            await MunicipiosAsync(true, ct),
            await TiposOcorrenciaAsync(true, ct),
            await db.ClassesCliente.AsNoTracking().Where(c => c.Ativo).OrderBy(c => c.Ordem)
                .Select(c => new ClasseClienteDto(c.Id, c.Codigo, c.Nome, c.Essencial, c.Ordem, c.Ativo)).ToListAsync(ct),
            await db.Recursos.AsNoTracking().OrderBy(r => r.Nome).Select(r => new ItemCodigoDto(r.Id, r.Codigo, r.Nome)).ToListAsync(ct),
            await db.Qualificacoes.AsNoTracking().OrderBy(r => r.Nome).Select(r => new ItemCodigoDto(r.Id, r.Codigo, r.Nome)).ToListAsync(ct),
            (await db.Prioridades.AsNoTracking().Where(p => p.Ativo).OrderBy(p => p.Rank).ToListAsync(ct)).Select(MapearPrioridade).ToList(),
            criterios.Where(c => c.Tipo == TipoCriterio.Fixo).Select(Criterio).ToList(),
            criterios.Where(c => c.Tipo == TipoCriterio.Personalizado).Select(Criterio).ToList(),
            new Dictionary<string, string>
            {
                ["LIGADO"] = _opcoes.Rotulos.Ligado, ["DESLIGADO"] = _opcoes.Rotulos.Desligado, ["NAO_INFORMADA"] = _opcoes.Rotulos.NaoInformada,
            },
            new ConfiguracaoFormularioDto(_opcoes.Uc.Formato, _opcoes.Transformador.TamanhoMaximo, _opcoes.FusoHorario, _opcoes.Demo.Habilitado, roteamento.Disponivel));
    }

    public async Task<List<TipoOcorrenciaDto>> TiposOcorrenciaAsync(bool somenteAtivos, CancellationToken ct = default) =>
        (await db.TiposOcorrencia.AsNoTracking().Include(t => t.Qualificacoes).ThenInclude(q => q.Qualificacao)
            .Where(t => !somenteAtivos || t.Ativo).OrderBy(t => t.Ordem).ToListAsync(ct))
        .Select(t => new TipoOcorrenciaDto(t.Id, t.Codigo, t.Nome, t.TipoManutencaoSugerido, t.Ordem, t.Ativo,
            t.Qualificacoes.Select(q => new ItemCodigoDto(q.QualificacaoId, q.Qualificacao!.Codigo, q.Qualificacao.Nome)).ToList()))
        .ToList();

    public static PrioridadeDto MapearPrioridade(Prioridade p) =>
        new(p.Id, p.Codigo, p.Nome, p.Descricao, p.Rank, p.Cor, p.Critica, p.PrazoTriagemMin, p.PrazoDespachoMin, p.PrazoInicioMin,
            p.PrazoRestabelecimentoMin, p.PrazoConclusaoMin, p.UnidadePrazo, p.Calendario, p.ConsideraFeriados, p.TratamentoCritico,
            p.Escalonamento, p.VigenciaInicio, p.VigenciaFim, p.Ativo, p.Demonstrativa);
}
