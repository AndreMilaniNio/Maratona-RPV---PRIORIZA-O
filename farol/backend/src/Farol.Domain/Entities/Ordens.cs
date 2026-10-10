using Farol.Domain.Enums;

namespace Farol.Domain.Entities;

/// <summary>Registro inicial do chamado, como foi informado. Nunca é reescrito.</summary>
public class Solicitacao
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public Guid ChaveIdempotencia { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public CanalEntrada Canal { get; set; }
    public string? Origem { get; set; }
    public string? ProtocoloExterno { get; set; }
    public DateTimeOffset RegistradaEm { get; set; }
    public Guid RegistradaPorId { get; set; }
    public required string Descricao { get; set; }
    public bool UcNaoInformada { get; set; }
    public string? MotivoUcNaoInformada { get; set; }
    /// <summary>Cópia integral do que foi informado no formulário (jsonb).</summary>
    public required string DadosInformados { get; set; }
    /// <summary>OS à qual a solicitação pertence hoje (muda se for unificada).</summary>
    public Guid OrdemServicoId { get; set; }
    public OrdemServico? OrdemServico { get; set; }
    public List<SolicitacaoUc> Ucs { get; set; } = [];
}

public class SolicitacaoUc
{
    public long Id { get; set; }
    public Guid SolicitacaoId { get; set; }
    public required string Numero { get; set; }
    public int? UnidadeConsumidoraId { get; set; }
    public UnidadeConsumidora? UnidadeConsumidora { get; set; }
    public bool ValidadaNoCadastro { get; set; }
}

/// <summary>Registro operacional gerado a partir da solicitação.</summary>
public class OrdemServico
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public Guid SolicitacaoId { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public TipoManutencao TipoManutencao { get; set; }
    public int TipoOcorrenciaId { get; set; }
    public TipoOcorrencia? TipoOcorrencia { get; set; }
    public StatusOrdemServico Status { get; set; }
    public DateTimeOffset AbertaEm { get; set; }
    public DateTimeOffset AtualizadaEm { get; set; }
    public DateTimeOffset? EncerradaEm { get; set; }
    public string? MotivoCancelamento { get; set; }

    // Localização (seção 4.2.2)
    public string? Logradouro { get; set; }
    public string? NumeroEndereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cep { get; set; }
    public string? EnderecoCompleto { get; set; }
    public string? PontoReferencia { get; set; }
    public string? ObservacoesLocalizacao { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public OrigemCoordenada? OrigemCoordenada { get; set; }
    public double? PrecisaoMetros { get; set; }
    public DateTimeOffset? CoordenadaRegistradaEm { get; set; }
    /// <summary>Sem coordenadas nem endereço suficientes: precisa de confirmação.</summary>
    public bool LocalizacaoPendente { get; set; }

    // Rede elétrica (seção 4.2.3)
    public int? SubestacaoId { get; set; }
    public Subestacao? Subestacao { get; set; }
    public int? ConjuntoId { get; set; }
    public ConjuntoEletrico? Conjunto { get; set; }
    public string? TransformadorNumero { get; set; }
    public string? TransformadorLocalidade { get; set; }
    public string? TransformadorLocal { get; set; }
    public int? TransformadorId { get; set; }
    public OrigemRede OrigemRede { get; set; }
    public string? Trecho { get; set; }
    public bool TrechoConfirmado { get; set; }
    public string? EquipamentoDescricao { get; set; }
    public string? IdentificadorEquipamento { get; set; }
    public string? ChaveEletrica { get; set; }

    // Fatos usados na classificação: um código de opção por critério (seção 4.5)
    public string PessoasAfetadas { get; set; } = "DESCONHECIDA";
    public int? QuantidadePessoas { get; set; }
    public string UcsAfetadas { get; set; } = "DESCONHECIDA";
    public int? QuantidadeUcs { get; set; }
    public string ServicoEssencial { get; set; } = "NAO_IDENTIFICADO";
    public string SituacaoCliente { get; set; } = "NAO_INFORMADA";
    public string CondicaoFornecimento { get; set; } = "DESCONHECIDA";
    public string Redundancia { get; set; } = "DESCONHECIDA";
    public string FonteReserva { get; set; } = "DESCONHECIDA";
    public string EquipeEspecializada { get; set; } = "DESCONHECIDA";
    public string EquipamentoAfetado { get; set; } = "NAO_IDENTIFICADO";
    public string Abrangencia { get; set; } = "DESCONHECIDA";
    public string NivelRede { get; set; } = "NAO_IDENTIFICADO";
    public int? QuantidadeEquipamentos { get; set; }
    public int? DuracaoEstimadaMin { get; set; }
    public DateTimeOffset? DataLimite { get; set; }
    public bool RecursosEspeciais { get; set; }

    public List<OsCondicaoSeguranca> CondicoesSeguranca { get; set; } = [];
    public List<OsClasseCliente> Classes { get; set; } = [];
    public List<OsRecurso> RecursosNecessarios { get; set; } = [];
    public List<OsRespostaCriterio> RespostasPersonalizadas { get; set; } = [];

    // Classificação corrente (desnormalizada do resultado atual para ordenar a fila)
    public int VersaoPontuacaoId { get; set; }
    public long? ResultadoAtualId { get; set; }
    public ResultadoPrioridade? ResultadoAtual { get; set; }
    public int? PrioridadeCalculadaId { get; set; }
    public int? PrioridadeId { get; set; }
    public Prioridade? Prioridade { get; set; }
    public int Pontuacao { get; set; }
    public int NivelPrecedencia { get; set; }
    public int? PrioridadeManualId { get; set; }
    public string? JustificativaManual { get; set; }

    // Prazos (seção 8): cinco conceitos distintos
    public DateTimeOffset? PrazoTriagem { get; set; }
    public DateTimeOffset? PrazoDespacho { get; set; }
    public DateTimeOffset? PrazoInicio { get; set; }
    public DateTimeOffset? PrazoRestabelecimento { get; set; }
    public DateTimeOffset? PrazoConclusao { get; set; }

    public uint Versao { get; set; }

    public List<Despacho> Despachos { get; set; } = [];
}

public class OsCondicaoSeguranca
{
    public Guid OrdemServicoId { get; set; }
    public required string Codigo { get; set; }
}

public class OsClasseCliente
{
    public Guid OrdemServicoId { get; set; }
    public int ClasseClienteId { get; set; }
    public ClasseCliente? ClasseCliente { get; set; }
}

public class OsRecurso
{
    public Guid OrdemServicoId { get; set; }
    public int RecursoId { get; set; }
    public Recurso? Recurso { get; set; }
}

/// <summary>Resposta a um critério personalizado cadastrado por administrador.</summary>
public class OsRespostaCriterio
{
    public Guid OrdemServicoId { get; set; }
    public int CriterioId { get; set; }
    public int OpcaoId { get; set; }
}

/// <summary>Linha do tempo da OS: status, dados, despacho, classificação.</summary>
public class HistoricoOs
{
    public long Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public DateTimeOffset OcorridoEm { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public required string Tipo { get; set; }
    public required string Descricao { get; set; }
    public string? StatusAnterior { get; set; }
    public string? StatusNovo { get; set; }
    public string? Justificativa { get; set; }
}

public class VinculoSolicitacao
{
    public long Id { get; set; }
    public Guid SolicitacaoId { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid? OrdemServicoOrigemId { get; set; }
    public Guid VinculadoPorId { get; set; }
    public DateTimeOffset VinculadoEm { get; set; }
    public required string Justificativa { get; set; }
}

/// <summary>Contador por município, ano e tipo de documento (Key decision 5).</summary>
public class NumeracaoSequencia
{
    public int MunicipioId { get; set; }
    public int Ano { get; set; }
    public required string Tipo { get; set; }
    public long Ultimo { get; set; }
}

public class Comentario
{
    public long Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid UsuarioId { get; set; }
    public required string UsuarioNome { get; set; }
    public required string Texto { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}

public class Anexo
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public required string NomeArquivo { get; set; }
    public required string TipoConteudo { get; set; }
    public long Tamanho { get; set; }
    public required byte[] Conteudo { get; set; }
    public Guid EnviadoPorId { get; set; }
    public DateTimeOffset EnviadoEm { get; set; }
}
