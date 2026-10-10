using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record OpcaoPontosDto(
    int OpcaoId, string Codigo, string Rotulo, bool RepresentaDesconhecido, int? Pontos, bool RequerConfirmacao,
    OrigemPontos? Origem, string? AlteradoPor, DateTimeOffset? AlteradoEm);

public record CriterioPontosDto(
    int CriterioId, string Codigo, string Nome, string? Descricao, string? RegraAplicacao, TipoCriterio Tipo,
    AgregacaoCriterio Agregacao, bool MultiplaEscolha, bool Habilitado, int? MinimoCriterio, int? MaximoCriterio,
    List<OpcaoPontosDto> Opcoes);

public record FaixaDto(int PrioridadeId, string Codigo, string Nome, string Cor, int Rank, int? Minimo, int? Maximo);

public record VersaoDetalheDto(
    int Id, int Numero, StatusVersaoPontuacao Status, bool Demonstrativa, int? MunicipioId, string? MunicipioNome,
    DateTimeOffset? VigenciaInicio, AplicacaoVersao? Aplicacao, string? Justificativa, string? Autor, DateTimeOffset CriadaEm,
    DateTimeOffset AlteradaEm, string? PublicadaPor, DateTimeOffset? PublicadaEm, string? AprovadaPor, DateTimeOffset? AprovadaEm,
    string Token, List<CriterioPontosDto> Criterios, List<FaixaDto> Faixas, int MinimoPossivel, int MaximoPossivel,
    List<string> Pendencias, List<string> Alertas);

public record RegraPrecedenciaDto(
    int Id, string Codigo, string Nome, string? Descricao, List<CondicaoDto> Condicoes, int NivelPrecedencia,
    int PrioridadeMinimaId, string PrioridadeMinima, bool Ativa, bool Demonstrativa, int Versao, DateTimeOffset? AprovadaEm);

public record CondicaoDto(string Criterio, string CriterioNome, List<string> Opcoes, List<string> OpcoesRotulos);

public record ConfiguracaoPontuacaoDto(
    int? MunicipioId,
    string Escopo,
    VersaoDetalheDto Vigente,
    bool VigenteHerdadaDaGlobal,
    VersaoDetalheDto? Rascunho,
    VersaoDetalheDto? AguardandoAprovacao,
    List<RegraPrecedenciaDto> RegrasPrecedencia,
    bool ExigeAprovacao,
    int PontosMaximosPorOpcao,
    bool PodeEditar,
    bool PodePublicar,
    bool PodeAprovar);

public record PontoSalvarDto(int OpcaoId, int? Pontos, bool RequerConfirmacao);
public record CriterioSalvarDto(int CriterioId, bool Habilitado);
public record FaixaSalvarDto(int PrioridadeId, int Minimo, int? Maximo);

public record SalvarRascunhoRequest(int VersaoId, string Token, List<CriterioSalvarDto> Criterios, List<PontoSalvarDto> Pontos, List<FaixaSalvarDto> Faixas);

public record PublicarRequest(int VersaoId, string Justificativa, DateTimeOffset? VigenciaInicio, AplicacaoVersao Aplicacao);

public record SimularRequest(int MunicipioId, bool UsarRascunho, int? VersaoId, NovaSolicitacaoRequest Solicitacao);

public record ImpactoOsDto(Guid OsId, string Numero, string Municipio, string De, string Para, int PontuacaoDe, int PontuacaoPara);
public record ImpactoCidadeDto(int MunicipioId, string Municipio, int Avaliadas, int Mudariam);
public record ImpactoVersaoDto(int VersaoId, int Avaliadas, int Mudariam, List<ImpactoCidadeDto> PorMunicipio, List<ImpactoOsDto> Ordens);

public record VersaoListaDto(
    int Id, int Numero, StatusVersaoPontuacao Status, bool Demonstrativa, int? MunicipioId, string? MunicipioNome,
    DateTimeOffset? VigenciaInicio, AplicacaoVersao? Aplicacao, string? Justificativa, string? Autor, DateTimeOffset CriadaEm,
    string? PublicadaPor, DateTimeOffset? PublicadaEm, string? AprovadaPor, int? RestauradaDeId, bool EmVigor);

public record DiferencaDto(string Tipo, string Item, string? ValorA, string? ValorB);
public record ComparacaoDto(VersaoListaDto A, VersaoListaDto B, List<DiferencaDto> Diferencas);
