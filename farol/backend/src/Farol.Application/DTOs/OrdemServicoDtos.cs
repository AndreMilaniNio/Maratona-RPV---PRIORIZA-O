using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record PaginaDto<T>(List<T> Itens, int Total, int Pagina, int TamanhoPagina);

public record FiltroFila
{
    public int? MunicipioId { get; init; }
    public List<int>? PrioridadeIds { get; init; }
    public TipoManutencao? TipoManutencao { get; init; }
    public int? TipoOcorrenciaId { get; init; }
    public List<StatusOrdemServico>? Status { get; init; }
    public string? Bairro { get; init; }
    public string? Logradouro { get; init; }
    public string? Trecho { get; init; }
    public string? FaixaPessoas { get; init; }
    public string? FaixaUcs { get; init; }
    /// <summary>Código de condição de segurança, ou "QUALQUER" para qualquer risco real.</summary>
    public string? Risco { get; init; }
    public string? ServicoEssencial { get; init; }
    public string? Equipamento { get; init; }
    public int? EquipeId { get; init; }
    public DateTimeOffset? AbertaDe { get; init; }
    public DateTimeOffset? AbertaAte { get; init; }
    /// <summary>"vencido" ou "proximo" (vence nas próximas 2 horas).</summary>
    public string? Prazo { get; init; }
    public int? SubestacaoId { get; init; }
    public int? ConjuntoId { get; init; }
    public string? Transformador { get; init; }
    public int? ClasseId { get; init; }
    public string? SituacaoCliente { get; init; }
    public string? Uc { get; init; }
    public string? Busca { get; init; }
    /// <summary>"fila" (padrão), "abertura", "prazo", "pontuacao", "numero"; prefixo "-" inverte.</summary>
    public string? OrdenarPor { get; init; }
    public bool IncluirEncerradas { get; init; }
    public int Pagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 25;
}

public record ProximoPrazoDto(TipoPrazo Tipo, DateTimeOffset Limite, bool Vencido, double MinutosRestantes);

public record EquipeResumoDto(int Id, string Codigo, string Nome);

public record FilaItemDto(
    int? Posicao,
    Guid Id,
    string Numero,
    PrioridadeResumoDto? Prioridade,
    bool PrioridadeManual,
    int Pontuacao,
    int NivelPrecedencia,
    TipoManutencao TipoManutencao,
    string TipoOcorrencia,
    string? Endereco,
    string? Bairro,
    string? Trecho,
    int MunicipioId,
    string Municipio,
    List<string> Ucs,
    bool UcNaoInformada,
    List<string> Classes,
    string? Subestacao,
    string? Conjunto,
    string? Transformador,
    string FaixaPessoas,
    string FaixaUcs,
    bool Risco,
    List<string> CondicoesSeguranca,
    string ServicoEssencial,
    string SituacaoCliente,
    DateTimeOffset AbertaEm,
    ProximoPrazoDto? ProximoPrazo,
    StatusOrdemServico Status,
    EquipeResumoDto? Equipe,
    bool LocalizacaoPendente,
    bool TemCoordenadas,
    bool VersaoDemonstrativa);

public record FilaDto(List<FilaItemDto> Itens, int Total, int Pagina, int TamanhoPagina, bool VersaoDemonstrativa, DateTimeOffset AtualizadoEm);

public record IndicadoresCidadeDto(int MunicipioId, string Municipio, int Abertas, int Criticas, int AguardandoDespacho, int EmAtendimento, int Vencidas, int EquipesDisponiveis, int EquipesDeslocamento, int EquipesExecutando);

public record IndicadoresDto(
    int Abertas,
    int Criticas,
    int AguardandoDespacho,
    int EmAtendimento,
    int Vencidas,
    int EquipesDisponiveis,
    int EquipesDeslocamento,
    int EquipesExecutando,
    List<IndicadoresCidadeDto> PorMunicipio,
    DateTimeOffset AtualizadoEm);

public record CoordenadaDto(double Latitude, double Longitude, OrigemCoordenada? Origem, double? PrecisaoMetros, DateTimeOffset? RegistradaEm);

public record PrazosDto(DateTimeOffset? Triagem, DateTimeOffset? Despacho, DateTimeOffset? Inicio, DateTimeOffset? Restabelecimento, DateTimeOffset? Conclusao, ProximoPrazoDto? Proximo, string Calendario, bool Demonstrativos);

public record RedeDto(
    int? SubestacaoId, string? SubestacaoCodigo, string? SubestacaoNome, string? SubestacaoLocal,
    int? ConjuntoId, string? Conjunto,
    string? TransformadorNumero, string? TransformadorLocalidade, string? LocalidadeNome, string? TransformadorLocal,
    OrigemRede Origem, string? Trecho, bool TrechoConfirmado, string? Equipamento, string? IdentificadorEquipamento, string? ChaveEletrica);

public record ImpactoDto(
    string PessoasAfetadas, int? QuantidadePessoas, string UcsAfetadas, int? QuantidadeUcs,
    string ServicoEssencial, string SituacaoCliente, List<string> CondicoesSeguranca, string CondicaoFornecimento,
    string Redundancia, string FonteReserva, string EquipeEspecializada, string EquipamentoAfetado, string Abrangencia,
    string NivelRede, int? QuantidadeEquipamentos, int? DuracaoEstimadaMin, DateTimeOffset? DataLimite, List<string> RecursosNecessarios,
    List<string> Classes);

public record DespachoDto(
    Guid Id, EquipeResumoDto Equipe, bool Ativo, string DesignadoPor, DateTimeOffset DesignadoEm, DateTimeOffset? AceitoEm,
    DateTimeOffset? IniciadoEm, DateTimeOffset? EncerradoEm, string? MotivoEncerramento, double? DistanciaKm,
    double? TempoEstimadoMin, string? OrigemEstimativa, bool ApoioIntermunicipal, bool Excecao, string? Justificativa);

public record HistoricoDto(DateTimeOffset OcorridoEm, string Tipo, string Descricao, string? Usuario, string? StatusAnterior, string? StatusNovo, string? Justificativa);

public record ComentarioDto(long Id, string Usuario, string Texto, DateTimeOffset CriadoEm);

public record AnexoDto(Guid Id, string NomeArquivo, string TipoConteudo, long Tamanho, DateTimeOffset EnviadoEm);

public record OrdemServicoDetalheDto(
    Guid Id,
    string Numero,
    int? Posicao,
    StatusOrdemServico Status,
    TipoManutencao TipoManutencao,
    int TipoOcorrenciaId,
    string TipoOcorrencia,
    int MunicipioId,
    string Municipio,
    DateTimeOffset AbertaEm,
    DateTimeOffset AtualizadaEm,
    DateTimeOffset? EncerradaEm,
    string? MotivoCancelamento,
    SolicitacaoDto Solicitacao,
    List<SolicitacaoDto> SolicitacoesVinculadas,
    List<UcDetalheDto> Ucs,
    string? Logradouro, string? NumeroEndereco, string? Bairro, string? Cep, string? EnderecoCompleto,
    string? PontoReferencia, string? ObservacoesLocalizacao, bool LocalizacaoPendente,
    CoordenadaDto? Coordenadas,
    RedeDto Rede,
    ImpactoDto Impacto,
    ClassificacaoDto? Classificacao,
    bool PrioridadeManual,
    string? JustificativaManual,
    PrazosDto Prazos,
    DespachoDto? DespachoAtivo,
    List<DespachoDto> Despachos,
    List<HistoricoDto> Historico,
    List<ComentarioDto> Comentarios,
    List<AnexoDto> Anexos,
    List<DuplicidadeDto> PossiveisDuplicidades,
    List<string> AcoesDisponiveis);

public record UcDetalheDto(string Numero, bool ValidadaNoCadastro, string? ClienteMascarado, string? Classe, string? Situacao, string? Endereco, bool Demonstrativa);

public record ReclassificarRequest(int? PrioridadeManualId, string Justificativa);

public record AlterarStatusRequest(StatusOrdemServico Status, string? Justificativa);

public record TrocarMunicipioRequest(int MunicipioId, string Justificativa);

public record UnificarRequest(Guid OsDuplicadaId, string Justificativa);

public record AtualizarFatosRequest
{
    public ImpactoInput Impacto { get; init; } = new();
    public List<int>? ClassesIds { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public OrigemCoordenada? OrigemCoordenada { get; init; }
    public bool? TrechoConfirmado { get; init; }
    public bool? RedeConfirmada { get; init; }
    public string Justificativa { get; init; } = string.Empty;
}

public record ComentarioRequest(string Texto);
