using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farol.API.Controllers;

[ApiController]
[Route("api/solicitacoes")]
[Authorize]
public class SolicitacoesController(SolicitacaoService servico) : ControllerBase
{
    /// <summary>Registra a solicitação e gera a OS (seção 4.8) em uma transação.</summary>
    [HttpPost]
    [Authorize(Policy = Permissoes.SolicitacaoRegistrar)]
    public async Task<ActionResult<NovaSolicitacaoResponse>> Criar(NovaSolicitacaoRequest r, CancellationToken ct)
    {
        var resposta = await servico.CriarAsync(r, ct);
        return CreatedAtAction(nameof(Obter), new { id = resposta.SolicitacaoId }, resposta);
    }

    /// <summary>Prévia da classificação — calculada pelo mesmo motor, nada é gravado.</summary>
    [HttpPost("previa")]
    [Authorize(Policy = Permissoes.SolicitacaoRegistrar)]
    public Task<ClassificacaoDto> Previa(NovaSolicitacaoRequest r, CancellationToken ct) => servico.PreviaAsync(r, ct);

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissoes.OsConsultar)]
    public Task<SolicitacaoDto> Obter(Guid id, CancellationToken ct) => servico.ObterAsync(id, ct);
}

[ApiController]
[Route("api/ordens-servico")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class OrdensServicoController(
    OrdemServicoConsultaService consultas, OrdemServicoComandoService comandos, DespachoService despachos,
    DuplicidadeService duplicidades, EscopoMunicipio escopo) : ControllerBase
{
    /// <summary>Fila operacional com posição calculada por cidade, filtros e paginação no servidor.</summary>
    [HttpGet]
    public Task<FilaDto> Fila([FromQuery] FiltroFila filtro, CancellationToken ct) => consultas.FilaAsync(filtro, ct);

    [HttpGet("indicadores")]
    public Task<IndicadoresDto> Indicadores([FromQuery] int? municipioId, CancellationToken ct) => consultas.IndicadoresAsync(municipioId, ct);

    [HttpGet("possiveis-duplicidades")]
    public async Task<List<DuplicidadeDto>> Duplicidades([FromQuery] int municipioId, [FromQuery] double? latitude, [FromQuery] double? longitude,
        [FromQuery] string? transformador, [FromQuery] string[]? uc, [FromQuery] int? conjuntoId, [FromQuery] int? tipoOcorrenciaId, CancellationToken ct)
    {
        await escopo.ExigirAsync(municipioId, "Municipio", municipioId, ct);
        return await duplicidades.SugerirAsync(new CriterioDuplicidade(municipioId, latitude, longitude, transformador, uc ?? [], conjuntoId, tipoOcorrenciaId), ct);
    }

    [HttpGet("{id:guid}")]
    public Task<OrdemServicoDetalheDto> Detalhe(Guid id, CancellationToken ct) => consultas.DetalheAsync(id, ct);

    /// <summary>Explicação da prioridade: pontos por critério, regras de precedência, versão e histórico de cálculos.</summary>
    [HttpGet("{id:guid}/prioridade")]
    public Task<object> Prioridade(Guid id, CancellationToken ct) => consultas.PrioridadeAsync(id, ct);

    [HttpGet("{id:guid}/historico")]
    public Task<List<HistoricoDto>> Historico(Guid id, CancellationToken ct) => consultas.HistoricoAsync(id, ct);

    /// <summary>Revisão manual da prioridade, com justificativa obrigatória.</summary>
    [HttpPost("{id:guid}/reclassificar")]
    [Authorize(Policy = Permissoes.OsReclassificar)]
    public async Task<IActionResult> Reclassificar(Guid id, ReclassificarRequest r, CancellationToken ct)
    {
        await comandos.ReclassificarAsync(id, r, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Permissoes.OsStatus)]
    public async Task<IActionResult> Status(Guid id, AlterarStatusRequest r, CancellationToken ct)
    {
        await comandos.AlterarStatusAsync(id, r, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/fatos")]
    [Authorize(Policy = Permissoes.OsAtualizarFatos)]
    public async Task<IActionResult> Fatos(Guid id, AtualizarFatosRequest r, [FromServices] MontadorOrdemServico montador, CancellationToken ct)
    {
        await comandos.AtualizarFatosAsync(id, r, montador, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/municipio")]
    [Authorize(Policy = Permissoes.OsTrocarMunicipio)]
    public async Task<IActionResult> Municipio(Guid id, TrocarMunicipioRequest r, CancellationToken ct)
    {
        await comandos.TrocarMunicipioAsync(id, r, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unificar")]
    [Authorize(Policy = Permissoes.OsUnificar)]
    public async Task<IActionResult> Unificar(Guid id, UnificarRequest r, CancellationToken ct)
    {
        await comandos.UnificarAsync(id, r, ct);
        return NoContent();
    }

    /// <summary>Equipes candidatas: compatibilidade e segurança antes da distância.</summary>
    [HttpGet("{id:guid}/equipes-candidatas")]
    [Authorize(Policy = Permissoes.DespachoDesignar)]
    public Task<CandidatasDto> Candidatas(Guid id, [FromQuery] bool incluirApoio, CancellationToken ct) => despachos.CandidatasAsync(id, incluirApoio, ct);

    [HttpPost("{id:guid}/despachos")]
    [Authorize(Policy = Permissoes.DespachoDesignar)]
    public async Task<ActionResult<DesignacaoDto>> Despachar(Guid id, DesignarRequest r, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await despachos.DesignarAsync(id, r, ct));

    [HttpPost("{id:guid}/comentarios")]
    public Task<ComentarioDto> Comentar(Guid id, ComentarioRequest r, CancellationToken ct) => comandos.ComentarAsync(id, r, ct);

    [HttpPost("{id:guid}/anexos")]
    [RequestSizeLimit(OrdemServicoComandoService.TamanhoMaximoAnexo + 64 * 1024)]
    public async Task<AnexoDto> Anexar(Guid id, IFormFile arquivo, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);
        return await comandos.AnexarAsync(id, arquivo.FileName, arquivo.ContentType, ms.ToArray(), ct);
    }

    [HttpGet("{id:guid}/anexos/{anexoId:guid}")]
    public async Task<IActionResult> Baixar(Guid id, Guid anexoId, CancellationToken ct)
    {
        var a = await comandos.BaixarAnexoAsync(id, anexoId, ct);
        return File(a.Conteudo, a.TipoConteudo, a.NomeArquivo);
    }
}

[ApiController]
[Route("api/despachos")]
[Authorize(Policy = Permissoes.OsStatus)]
public class DespachosController(DespachoService servico) : ControllerBase
{
    [HttpPost("{id:guid}/aceite")]
    public async Task<IActionResult> Aceite(Guid id, EventoDespachoRequest r, CancellationToken ct) { await servico.AceiteAsync(id, r, ct); return NoContent(); }

    [HttpPost("{id:guid}/inicio")]
    public async Task<IActionResult> Inicio(Guid id, EventoDespachoRequest r, CancellationToken ct) { await servico.InicioAsync(id, r, ct); return NoContent(); }

    [HttpPost("{id:guid}/conclusao")]
    public async Task<IActionResult> Conclusao(Guid id, EventoDespachoRequest r, CancellationToken ct) { await servico.ConclusaoAsync(id, r, ct); return NoContent(); }

    /// <summary>Remove a equipe da OS (troca ou desistência), preservando o histórico.</summary>
    [HttpPost("{id:guid}/encerrar")]
    public async Task<IActionResult> Encerrar(Guid id, EventoDespachoRequest r, CancellationToken ct) { await servico.EncerrarAsync(id, r, ct); return NoContent(); }
}

[ApiController]
[Route("api/equipes")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class EquipesController(EquipeService servico) : ControllerBase
{
    [HttpGet]
    public Task<List<EquipeDto>> Listar([FromQuery] int? municipioId, CancellationToken ct) => servico.ListarAsync(municipioId, false, ct);

    [HttpGet("disponiveis")]
    public Task<List<EquipeDto>> Disponiveis([FromQuery] int? municipioId, CancellationToken ct) => servico.ListarAsync(municipioId, true, ct);

    [HttpGet("{id:int}")]
    public Task<EquipeDto> Obter(int id, CancellationToken ct) => servico.ObterAsync(id, ct);

    [HttpGet("{id:int}/localizacao")]
    [Authorize(Policy = Permissoes.LocalizacaoConsultar)]
    public Task<List<object>> Localizacao(int id, CancellationToken ct) => servico.HistoricoLocalizacaoAsync(id, ct);

    /// <summary>Registra a posição conhecida (manual ou de integração GPS futura).</summary>
    [HttpPut("{id:int}/localizacao")]
    public async Task<IActionResult> AtualizarLocalizacao(int id, LocalizacaoEquipeRequest r, CancellationToken ct)
    {
        await servico.AtualizarLocalizacaoAsync(id, r, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = Permissoes.DespachoDesignar)]
    public async Task<IActionResult> Status(int id, StatusEquipeRequest r, CancellationToken ct)
    {
        await servico.AlterarStatusAsync(id, r, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/mapa")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class MapaController(MapaService servico) : ControllerBase
{
    [HttpGet("operacoes")]
    public Task<MapaDto> Operacoes([FromQuery] int? municipioId, CancellationToken ct) => servico.OperacoesAsync(municipioId, ct);

    [HttpGet("rota")]
    public Task<RotaDto> Rota([FromQuery] double deLat, [FromQuery] double deLng, [FromQuery] double paraLat, [FromQuery] double paraLng, CancellationToken ct) =>
        servico.RotaAsync(deLat, deLng, paraLat, paraLng, ct);
}
