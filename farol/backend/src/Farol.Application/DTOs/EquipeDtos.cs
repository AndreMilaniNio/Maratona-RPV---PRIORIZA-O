using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record IntegranteDto(int Id, string Nome, string? Matricula, string? Funcao);

public record ItemCodigoDto(int Id, string Codigo, string Nome);

public record EquipeDto(
    int Id,
    string Codigo,
    string Nome,
    int MunicipioBaseId,
    string MunicipioBase,
    List<ItemCodigoDto> MunicipiosAdicionais,
    StatusEquipe Status,
    int Capacidade,
    int DespachosAtivos,
    bool Disponivel,
    double? Latitude,
    double? Longitude,
    DateTimeOffset? LocalizacaoEm,
    OrigemLocalizacaoEquipe? OrigemLocalizacao,
    List<IntegranteDto> Integrantes,
    List<ItemCodigoDto> Qualificacoes,
    List<ItemCodigoDto> Recursos,
    List<OsAtribuidaDto> OsAtribuidas,
    bool Ativa,
    bool Demonstrativa);

public record OsAtribuidaDto(Guid OsId, string Numero, StatusOrdemServico Status, DateTimeOffset DesignadoEm);

public record EquipeCandidataDto(
    EquipeDto Equipe,
    bool Compativel,
    List<string> Faltando,
    bool Disponivel,
    string? MotivoIndisponivel,
    bool ApoioIntermunicipal,
    double? DistanciaKm,
    double? TempoEstimadoMin,
    string OrigemEstimativa,
    bool Recomendada,
    string Justificativa);

public record CandidatasDto(
    Guid OsId,
    string OsNumero,
    double? OsLatitude,
    double? OsLongitude,
    List<string> QualificacoesExigidas,
    List<string> RecursosExigidos,
    bool RoteamentoDisponivel,
    List<EquipeCandidataDto> Equipes);

public record DesignarRequest(int EquipeId, string? Justificativa, bool Excecao, bool ApoioIntermunicipal);

public record EntregaEquipeDto(
    double? Latitude,
    double? Longitude,
    string? Coordenadas,
    bool LocalizacaoAproximada,
    string? AvisoLocalizacao,
    string? Endereco,
    string? Cep,
    List<string> Ucs,
    string? Subestacao,
    string? Conjunto,
    string? Transformador,
    string? LinkMapa,
    string? GeoUri);

public record DesignacaoDto(Guid DespachoId, Guid OsId, string OsNumero, EquipeResumoDto Equipe, EntregaEquipeDto Entrega);

public record EventoDespachoRequest(string? Observacao, string? Motivo);

public record LocalizacaoEquipeRequest(double Latitude, double Longitude, OrigemLocalizacaoEquipe Origem);

public record EquipeSalvarRequest(
    string Codigo,
    string Nome,
    int MunicipioBaseId,
    List<int> MunicipiosAdicionaisIds,
    StatusEquipe Status,
    int Capacidade,
    List<IntegranteSalvarDto> Integrantes,
    List<int> QualificacoesIds,
    List<int> RecursosIds,
    bool Ativa);

public record IntegranteSalvarDto(string Nome, string? Matricula, string? Funcao);

public record StatusEquipeRequest(StatusEquipe Status, string? Justificativa);
