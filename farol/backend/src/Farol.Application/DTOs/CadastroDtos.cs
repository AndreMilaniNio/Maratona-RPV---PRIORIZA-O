using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record MunicipioDto(int Id, string Nome, string Uf, string? CodigoIbge, string Prefixo, double? Latitude, double? Longitude, double? RaioKm, string? CepUnico, bool Ativo, bool Demonstrativo);
public record LocalidadeDto(int Id, string Codigo, string Nome, int MunicipioId, string Municipio, bool Demonstrativo);
public record SubestacaoDto(int Id, string Codigo, string Nome, string? Local, int MunicipioId, double? Latitude, double? Longitude, int Conjuntos, bool Demonstrativo);
public record ConjuntoDto(int Id, int SubestacaoId, string Numero, bool Demonstrativo);
public record TransformadorDto(int Id, string NumeroCompleto, string CodigoLocalidade, string NumeroLocal, string? Localidade, int? SubestacaoId, string? Subestacao, int? ConjuntoId, string? Conjunto, int MunicipioId, int UcsLigadas, bool Demonstrativo);
public record ClasseClienteDto(int Id, string Codigo, string Nome, bool Essencial, int Ordem, bool Ativo);
public record TipoOcorrenciaDto(int Id, string Codigo, string Nome, TipoManutencao? TipoManutencaoSugerido, int Ordem, bool Ativo, List<ItemCodigoDto> Qualificacoes);

public record InterpretacaoTransformadorDto(
    bool Valido,
    string? Erro,
    string? Numero,
    string? CodigoLocalidade,
    string? NumeroLocal,
    LocalidadeDto? Localidade,
    bool LocalidadeCadastrada,
    TransformadorDto? Transformador,
    List<string> Alertas);

public record UnidadeConsumidoraDto(
    string Numero,
    string ClienteMascarado,
    int ClasseId,
    string Classe,
    string Situacao,
    string? Logradouro,
    string? NumeroImovel,
    string? Bairro,
    string? Cep,
    int MunicipioId,
    string Municipio,
    string? Transformador,
    int? SubestacaoId,
    string? Subestacao,
    int? ConjuntoId,
    string? Conjunto,
    double? Latitude,
    double? Longitude,
    bool Demonstrativa);

public record CepDto(bool Encontrado, string Cep, string? Logradouro, string? Bairro, int? MunicipioId, string? Municipio, bool CepUnico, string Fonte, string? Aviso);

public record InterpretarCoordenadasRequest(string Texto, int? MunicipioId);
public record CoordenadasInterpretadasDto(bool Valido, double? Latitude, double? Longitude, List<string> Alertas, string? Erro);

public record OpcaoFormularioDto(int Id, string Codigo, string Rotulo, bool RepresentaDesconhecido);
public record CriterioFormularioDto(int Id, string Codigo, string Nome, string? Descricao, bool MultiplaEscolha, List<OpcaoFormularioDto> Opcoes);
public record PrioridadeDto(
    int Id, string Codigo, string Nome, string? Descricao, int Rank, string Cor, bool Critica,
    int PrazoTriagemMin, int PrazoDespachoMin, int PrazoInicioMin, int? PrazoRestabelecimentoMin, int PrazoConclusaoMin,
    string UnidadePrazo, CalendarioPrazo Calendario, bool ConsideraFeriados, string? TratamentoCritico, string? Escalonamento,
    DateTimeOffset VigenciaInicio, DateTimeOffset? VigenciaFim, bool Ativo, bool Demonstrativa);

/// <summary>Tudo que o formulário de Nova Solicitação precisa para montar as listas.</summary>
public record CatalogoDto(
    List<MunicipioDto> Municipios,
    List<TipoOcorrenciaDto> TiposOcorrencia,
    List<ClasseClienteDto> Classes,
    List<ItemCodigoDto> Recursos,
    List<ItemCodigoDto> Qualificacoes,
    List<PrioridadeDto> Prioridades,
    List<CriterioFormularioDto> CriteriosFixos,
    List<CriterioFormularioDto> CriteriosPersonalizados,
    Dictionary<string, string> RotulosSituacao,
    ConfiguracaoFormularioDto Configuracao);

public record ConfiguracaoFormularioDto(string FormatoUc, int TransformadorTamanhoMaximo, string FusoHorario, bool DemoHabilitado, bool RoteamentoDisponivel);

public record MapaOsDto(Guid Id, string Numero, double Latitude, double Longitude, PrioridadeResumoDto? Prioridade, StatusOrdemServico Status, string TipoOcorrencia, string? Endereco, bool Critica, OrigemCoordenada? Origem, int MunicipioId);
public record MapaEquipeDto(int Id, string Codigo, string Nome, StatusEquipe Status, bool Disponivel, double Latitude, double Longitude, DateTimeOffset? LocalizacaoEm, OrigemLocalizacaoEquipe? Origem, int MunicipioId);
public record MapaSubestacaoDto(int Id, string Codigo, string Nome, double Latitude, double Longitude, bool Demonstrativo);
public record MapaDto(List<MapaOsDto> Ordens, List<MapaEquipeDto> Equipes, List<MapaSubestacaoDto> Subestacoes, int SemCoordenadas, double? CentroLatitude, double? CentroLongitude);
public record RotaDto(bool Disponivel, double DistanciaKm, double? TempoMin, string Origem, List<double[]> Geometria, string? Aviso);
