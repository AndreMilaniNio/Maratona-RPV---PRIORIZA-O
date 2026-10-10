using Farol.Application.Common;
using Farol.Application.DTOs;
using Farol.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farol.API.Controllers;

/// <summary>Consultas de rede, UC, CEP e coordenadas usadas pelo formulário de abertura.</summary>
[ApiController]
[Route("api")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class CadastrosController(CadastroRedeService servico) : ControllerBase
{
    [HttpGet("catalogo")]
    public Task<CatalogoDto> Catalogo(CancellationToken ct) => servico.CatalogoAsync(ct);

    [HttpGet("municipios")]
    public Task<List<MunicipioDto>> Municipios([FromQuery] bool todos, CancellationToken ct) => servico.MunicipiosAsync(!todos, ct);

    [HttpGet("municipios/{id:int}/subestacoes")]
    public Task<List<SubestacaoDto>> Subestacoes(int id, CancellationToken ct) => servico.SubestacoesAsync(id, ct);

    [HttpGet("subestacoes/{id:int}/conjuntos")]
    public Task<List<ConjuntoDto>> Conjuntos(int id, CancellationToken ct) => servico.ConjuntosAsync(id, ct);

    [HttpGet("localidades")]
    public Task<List<LocalidadeDto>> Localidades([FromQuery] int? municipioId, CancellationToken ct) => servico.LocalidadesAsync(municipioId, ct);

    [HttpGet("transformadores")]
    public Task<PaginaDto<TransformadorDto>> Transformadores([FromQuery] int? municipioId, [FromQuery] string? numero,
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50, CancellationToken ct = default) =>
        servico.TransformadoresAsync(municipioId, numero, pagina, tamanhoPagina, ct);

    /// <summary>Interpreta o número: 3 dígitos de localidade + número local; sugere circuito e conjunto do cadastro.</summary>
    [HttpGet("transformadores/interpretar")]
    public Task<InterpretacaoTransformadorDto> Interpretar([FromQuery] string numero, [FromQuery] int? municipioId, CancellationToken ct) =>
        servico.InterpretarTransformadorAsync(numero, municipioId, ct);

    /// <summary>Busca de UC — consulta auditada, nome mascarado.</summary>
    [HttpGet("unidades-consumidoras")]
    public Task<UnidadeConsumidoraDto> Uc([FromQuery] string uc, CancellationToken ct) => servico.UnidadeConsumidoraAsync(uc, ct);

    [HttpGet("geocodificacao/cep/{cep}")]
    public Task<CepDto> Cep(string cep, CancellationToken ct) => servico.CepAsync(cep, ct);

    /// <summary>Aceita par decimal, link de mapa ou graus/minutos/segundos.</summary>
    [HttpPost("geocodificacao/coordenadas/interpretar")]
    public Task<CoordenadasInterpretadasDto> Coordenadas(InterpretarCoordenadasRequest r, CancellationToken ct) => servico.InterpretarCoordenadasAsync(r, ct);

    [HttpGet("tipos-ocorrencia")]
    public Task<List<TipoOcorrenciaDto>> Tipos([FromQuery] bool todos, CancellationToken ct) => servico.TiposOcorrenciaAsync(!todos, ct);
}

/// <summary>Configurações vigentes em modo leitura para qualquer perfil operacional.</summary>
[ApiController]
[Route("api/configuracoes")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class ConfiguracoesController(AdminService admin, CadastroRedeService cadastros) : ControllerBase
{
    [HttpGet("prioridades")]
    public async Task<List<PrioridadeDto>> Prioridades(CancellationToken ct) => (await cadastros.CatalogoAsync(ct)).Prioridades;

    [HttpGet("regras")]
    public Task<List<RegraPrecedenciaDto>> Regras(CancellationToken ct) => admin.RegrasAsync(ct);

    [HttpGet("criterios")]
    public Task<List<CriterioAdminDto>> Criterios(CancellationToken ct) => admin.CriteriosAsync(ct);
}

/// <summary>Tela do Usuário Chave (seção 7A).</summary>
[ApiController]
[Route("api/configuracoes/pontuacao")]
[Authorize(Policy = Permissoes.OsConsultar)]
public class PontuacaoController(PontuacaoService servico) : ControllerBase
{
    /// <summary>Versão vigente, rascunho e versão aguardando aprovação do escopo (global ou cidade).</summary>
    [HttpGet]
    public Task<ConfiguracaoPontuacaoDto> Obter([FromQuery] int? municipioId, CancellationToken ct) => servico.ObterAsync(municipioId, ct);

    [HttpPost("rascunho")]
    [Authorize(Policy = Permissoes.PontuacaoEditar)]
    public Task<VersaoDetalheDto> CriarRascunho([FromQuery] int? municipioId, CancellationToken ct) => servico.CriarRascunhoAsync(municipioId, null, ct);

    /// <summary>Grava o rascunho. Gravação concorrente com token antigo recebe 409.</summary>
    [HttpPut("rascunho")]
    [Authorize(Policy = Permissoes.PontuacaoEditar)]
    public Task<VersaoDetalheDto> SalvarRascunho(SalvarRascunhoRequest r, CancellationToken ct) => servico.SalvarRascunhoAsync(r, ct);

    [HttpDelete("rascunho/{versaoId:int}")]
    [Authorize(Policy = Permissoes.PontuacaoEditar)]
    public async Task<IActionResult> Descartar(int versaoId, CancellationToken ct)
    {
        await servico.DescartarRascunhoAsync(versaoId, ct);
        return NoContent();
    }

    /// <summary>Simulador: mesmo motor da produção, com o rascunho ou uma versão escolhida.</summary>
    [HttpPost("simular")]
    public Task<ClassificacaoDto> Simular(SimularRequest r, CancellationToken ct) => servico.SimularAsync(r, ct);

    [HttpGet("impacto")]
    [Authorize(Policy = Permissoes.PontuacaoEditar)]
    public Task<ImpactoVersaoDto> Impacto([FromQuery] int versaoId, CancellationToken ct) => servico.ImpactoAsync(versaoId, ct);

    [HttpPost("publicar")]
    [Authorize(Policy = Permissoes.PontuacaoPublicar)]
    public Task<VersaoDetalheDto> Publicar(PublicarRequest r, CancellationToken ct) => servico.PublicarAsync(r, ct);

    [HttpPost("versoes/{id:int}/aprovar")]
    [Authorize(Policy = Permissoes.PontuacaoAprovar)]
    public Task<VersaoDetalheDto> Aprovar(int id, CancellationToken ct) => servico.AprovarAsync(id, ct);

    [HttpPost("versoes/{id:int}/rejeitar")]
    [Authorize(Policy = Permissoes.PontuacaoAprovar)]
    public Task<VersaoDetalheDto> Rejeitar(int id, [FromQuery] string? motivo, CancellationToken ct) => servico.RejeitarAsync(id, motivo, ct);

    [HttpGet("versoes")]
    public Task<List<VersaoListaDto>> Versoes([FromQuery] int? municipioId, CancellationToken ct) => servico.VersoesAsync(municipioId, ct);

    [HttpGet("versoes/{id:int}")]
    public Task<VersaoDetalheDto> Versao(int id, CancellationToken ct) => servico.DetalheAsync(id, ct);

    [HttpGet("versoes/comparar")]
    public Task<ComparacaoDto> Comparar([FromQuery] int a, [FromQuery] int b, CancellationToken ct) => servico.CompararAsync(a, b, ct);

    /// <summary>Restaurar cria um novo rascunho com o conteúdo da versão; o histórico fica intacto.</summary>
    [HttpPost("versoes/{id:int}/restaurar")]
    [Authorize(Policy = Permissoes.PontuacaoEditar)]
    public async Task<ActionResult<VersaoDetalheDto>> Restaurar(int id, [FromQuery] int? municipioId, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servico.CriarRascunhoAsync(municipioId, id, ct));
}
