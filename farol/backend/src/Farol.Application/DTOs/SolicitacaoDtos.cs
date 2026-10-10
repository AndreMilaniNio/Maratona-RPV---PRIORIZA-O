using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record LocalizacaoInput
{
    public string? Logradouro { get; init; }
    public string? Numero { get; init; }
    public string? Bairro { get; init; }
    public string? Cep { get; init; }
    public string? EnderecoCompleto { get; init; }
    public string? PontoReferencia { get; init; }
    public string? Observacoes { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public OrigemCoordenada? OrigemCoordenada { get; init; }
    public double? PrecisaoMetros { get; init; }
}

public record RedeInput
{
    public int? SubestacaoId { get; init; }
    public int? ConjuntoId { get; init; }
    public string? TransformadorNumero { get; init; }
    public string? Trecho { get; init; }
    public string? EquipamentoDescricao { get; init; }
    public string? IdentificadorEquipamento { get; init; }
    public string? ChaveEletrica { get; init; }
}

/// <summary>Fatos de impacto e gravidade (seção 4.5). Cada campo é um código de opção do critério.</summary>
public record ImpactoInput
{
    public string PessoasAfetadas { get; init; } = "DESCONHECIDA";
    public int? QuantidadePessoas { get; init; }
    public string UcsAfetadas { get; init; } = "DESCONHECIDA";
    public int? QuantidadeUcs { get; init; }
    public string ServicoEssencial { get; init; } = "NAO_IDENTIFICADO";
    public string SituacaoCliente { get; init; } = "NAO_INFORMADA";
    public List<string> CondicoesSeguranca { get; init; } = [];
    public string CondicaoFornecimento { get; init; } = "DESCONHECIDA";
    public string Redundancia { get; init; } = "DESCONHECIDA";
    public string FonteReserva { get; init; } = "DESCONHECIDA";
    public string EquipeEspecializada { get; init; } = "DESCONHECIDA";
    public string EquipamentoAfetado { get; init; } = "NAO_IDENTIFICADO";
    public string Abrangencia { get; init; } = "DESCONHECIDA";
    public string NivelRede { get; init; } = "NAO_IDENTIFICADO";
    public int? QuantidadeEquipamentos { get; init; }
    public int? DuracaoEstimadaMin { get; init; }
    public DateTimeOffset? DataLimite { get; init; }
    public List<int> RecursosIds { get; init; } = [];
}

public record RespostaPersonalizadaInput(int CriterioId, int OpcaoId);

public record NovaSolicitacaoRequest
{
    public Guid ChaveIdempotencia { get; init; }
    public int MunicipioId { get; init; }
    public CanalEntrada Canal { get; init; }
    public string? Origem { get; init; }
    public string? ProtocoloExterno { get; init; }
    public bool UcNaoInformada { get; init; }
    public string? MotivoUcNaoInformada { get; init; }
    public List<string> Ucs { get; init; } = [];
    public List<int> ClassesIds { get; init; } = [];
    public LocalizacaoInput Localizacao { get; init; } = new();
    public RedeInput Rede { get; init; } = new();
    public TipoManutencao TipoManutencao { get; init; }
    public int TipoOcorrenciaId { get; init; }
    public ImpactoInput Impacto { get; init; } = new();
    public string Descricao { get; init; } = string.Empty;
    public Guid? OsExistenteId { get; init; }
    public List<RespostaPersonalizadaInput> RespostasPersonalizadas { get; init; } = [];
}

public record DuplicidadeDto(Guid OsId, string Numero, string Status, string TipoOcorrencia, string? Endereco, double? DistanciaM, List<string> Motivos, DateTimeOffset AbertaEm);

public record NovaSolicitacaoResponse(
    Guid SolicitacaoId,
    string SolicitacaoNumero,
    Guid OsId,
    string OsNumero,
    bool VinculadaAOsExistente,
    PrioridadeResumoDto Prioridade,
    int Pontuacao,
    bool VersaoDemonstrativa,
    List<string> Alertas,
    List<DuplicidadeDto> PossiveisDuplicidades);

public record PrioridadeResumoDto(int Id, string Codigo, string Nome, string Cor, int Rank, bool Critica);

public record ItemPontuacaoDto(string CriterioCodigo, string Criterio, List<string> Opcoes, List<string> OpcoesCodigos, int Pontos, bool SemPontosDefinidos, bool RequerConfirmacao);

public record RegraAplicadaDto(string Codigo, string Nome, int NivelPrecedencia, string PrioridadeMinima);

public record VersaoResumoDto(int Id, int Numero, int? MunicipioId, string? MunicipioNome, bool Demonstrativa, DateTimeOffset? VigenciaInicio);

/// <summary>Resultado de classificação explicado (prévia, simulador e detalhe da OS).</summary>
public record ClassificacaoDto(
    int Pontuacao,
    PrioridadeResumoDto Prioridade,
    PrioridadeResumoDto PrioridadePelaFaixa,
    int NivelPrecedencia,
    string MotivoPrincipal,
    List<ItemPontuacaoDto> Itens,
    List<RegraAplicadaDto> RegrasAplicadas,
    VersaoResumoDto Versao,
    DateTimeOffset CalculadoEm,
    List<string> Alertas);

public record SolicitacaoDto(
    Guid Id,
    string Numero,
    int MunicipioId,
    string Municipio,
    CanalEntrada Canal,
    string? Origem,
    string? ProtocoloExterno,
    DateTimeOffset RegistradaEm,
    string RegistradaPor,
    string Descricao,
    bool UcNaoInformada,
    string? MotivoUcNaoInformada,
    List<SolicitacaoUcDto> Ucs,
    Guid OrdemServicoId,
    string OrdemServicoNumero,
    string DadosInformados);

public record SolicitacaoUcDto(string Numero, bool ValidadaNoCadastro);
