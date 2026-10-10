using Farol.Domain.Entities;
using Farol.Domain.Enums;

namespace Farol.Application.DTOs;

public record MunicipioSalvarRequest(string Nome, string Uf, string? CodigoIbge, string Prefixo, double? Latitude, double? Longitude, double? RaioKm, string? CepUnico, bool Ativo);
public record LocalidadeSalvarRequest(string Codigo, string Nome, int MunicipioId);
public record SubestacaoSalvarRequest(string Codigo, string Nome, string? Local, int MunicipioId, double? Latitude, double? Longitude);
public record ConjuntoSalvarRequest(int SubestacaoId, string Numero);
public record TransformadorSalvarRequest(string Numero, int MunicipioId, int? SubestacaoId, int? ConjuntoId, double? Latitude, double? Longitude);
public record ClasseSalvarRequest(string Codigo, string Nome, bool Essencial, int Ordem, bool Ativo);
public record TipoOcorrenciaSalvarRequest(string Codigo, string Nome, TipoManutencao? TipoManutencaoSugerido, int Ordem, bool Ativo, List<int> QualificacoesIds);
public record ItemSalvarRequest(string Codigo, string Nome);
public record FeriadoSalvarRequest(DateOnly Data, string Nome, int? MunicipioId);
public record FeriadoDto(int Id, DateOnly Data, string Nome, int? MunicipioId);

public record PrioridadeSalvarRequest(
    string Codigo, string Nome, string? Descricao, int Rank, string Cor, bool Critica,
    int PrazoTriagemMin, int PrazoDespachoMin, int PrazoInicioMin, int? PrazoRestabelecimentoMin, int PrazoConclusaoMin,
    string UnidadePrazo, CalendarioPrazo Calendario, bool ConsideraFeriados, string? TratamentoCritico, string? Escalonamento,
    DateTimeOffset? VigenciaInicio, DateTimeOffset? VigenciaFim, bool Ativo);

public record RegraSalvarRequest(string Codigo, string Nome, string? Descricao, List<CondicaoRegra> Condicoes, int NivelPrecedencia, int PrioridadeMinimaId, bool Ativa, string Justificativa);

public record OpcaoSalvarDto(string Codigo, string Rotulo, bool RepresentaDesconhecido);
public record CriterioSalvarRequest(string Codigo, string Nome, string? Descricao, string? RegraAplicacao, AgregacaoCriterio Agregacao, bool MultiplaEscolha, List<OpcaoSalvarDto> Opcoes, List<int> MunicipiosIds, bool Ativo);

public record CriterioAdminDto(int Id, string Codigo, string Nome, string? Descricao, string? RegraAplicacao, TipoCriterio Tipo, AgregacaoCriterio Agregacao,
    bool MultiplaEscolha, int Ordem, bool Ativo, List<OpcaoFormularioDto> Opcoes, List<int> MunicipiosIds, DateTimeOffset CriadoEm, DateTimeOffset AlteradoEm);

public record AuditoriaDto(long Id, DateTimeOffset OcorridoEm, string? Usuario, string Acao, string Entidade, string EntidadeId, int? MunicipioId,
    string? ValoresAnteriores, string? ValoresNovos, string? Justificativa);

public record FiltroAuditoria(string? Entidade, string? EntidadeId, string? Acao, Guid? UsuarioId, int? MunicipioId, DateTimeOffset? De, DateTimeOffset? Ate, int Pagina = 1, int TamanhoPagina = 50);

public record UsuarioDto(Guid Id, string Nome, string Email, bool Ativo, List<string> Perfis, List<int> MunicipiosIds, int? EquipeId, bool Demonstrativo);
public record UsuarioSalvarRequest(string Nome, string Email, string? Senha, bool Ativo, List<string> Perfis, List<int> MunicipiosIds, int? EquipeId);
