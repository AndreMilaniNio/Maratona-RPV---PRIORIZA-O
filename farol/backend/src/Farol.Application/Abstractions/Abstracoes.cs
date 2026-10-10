using Farol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Farol.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Municipio> Municipios { get; }
    DbSet<Localidade> Localidades { get; }
    DbSet<Subestacao> Subestacoes { get; }
    DbSet<ConjuntoEletrico> Conjuntos { get; }
    DbSet<Transformador> Transformadores { get; }
    DbSet<ClasseCliente> ClassesCliente { get; }
    DbSet<UnidadeConsumidora> UnidadesConsumidoras { get; }
    DbSet<CepCadastrado> Ceps { get; }
    DbSet<TipoOcorrencia> TiposOcorrencia { get; }
    DbSet<Feriado> Feriados { get; }
    DbSet<Equipe> Equipes { get; }
    DbSet<Integrante> Integrantes { get; }
    DbSet<Qualificacao> Qualificacoes { get; }
    DbSet<Recurso> Recursos { get; }
    DbSet<LocalizacaoEquipe> LocalizacoesEquipe { get; }
    DbSet<Despacho> Despachos { get; }
    DbSet<Solicitacao> Solicitacoes { get; }
    DbSet<OrdemServico> OrdensServico { get; }
    DbSet<HistoricoOs> HistoricosOs { get; }
    DbSet<VinculoSolicitacao> VinculosSolicitacao { get; }
    DbSet<Comentario> Comentarios { get; }
    DbSet<Anexo> Anexos { get; }
    DbSet<Criterio> Criterios { get; }
    DbSet<OpcaoCriterio> OpcoesCriterio { get; }
    DbSet<RegraPrecedencia> RegrasPrecedencia { get; }
    DbSet<Prioridade> Prioridades { get; }
    DbSet<VersaoPontuacao> VersoesPontuacao { get; }
    DbSet<PontoOpcao> PontosOpcao { get; }
    DbSet<FaixaPrioridade> FaixasPrioridade { get; }
    DbSet<VersaoCriterio> VersaoCriterios { get; }
    DbSet<ResultadoPrioridade> ResultadosPrioridade { get; }
    DbSet<Usuario> Users { get; }
    DbSet<UsuarioMunicipio> UsuarioMunicipios { get; }
    DbSet<Auditoria> Auditorias { get; }

    ChangeTracker ChangeTracker { get; }

    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Quem está chamando, com o escopo de cidades já resolvido (Key decision 1).</summary>
public interface IUsuarioAtual
{
    Guid Id { get; }
    string Nome { get; }
    bool Autenticado { get; }
    bool Tem(string permissao);
    IReadOnlySet<string> Permissoes { get; }
    IReadOnlyList<string> Perfis { get; }
    /// <summary>Municípios que o usuário pode acessar; nulo quando pode acessar todos.</summary>
    Task<IReadOnlySet<int>?> MunicipiosPermitidosAsync(CancellationToken ct = default);
    Task<int?> EquipeIdAsync(CancellationToken ct = default);
    string? EnderecoIp { get; }
}

public interface IRelogio
{
    DateTimeOffset Agora { get; }
}

/// <summary>Avisa as telas abertas de que a fila de uma cidade mudou (SignalR).</summary>
public interface INotificadorFila
{
    Task FilaAlteradaAsync(int municipioId, string motivo, CancellationToken ct = default);
}

/// <summary>Gera números únicos por município e ano dentro da transação corrente (Key decision 5).</summary>
public interface IGeradorNumero
{
    Task<string> ProximoAsync(Municipio municipio, string tipo, int ano, CancellationToken ct = default);
}

public sealed record ResultadoCep(bool Encontrado, string Cep, string? Logradouro, string? Bairro, int? MunicipioId, string? MunicipioNome, string Fonte);

/// <summary>Consulta de CEP por provedor configurável — nenhum provedor é presumido.</summary>
public interface IProvedorCep
{
    Task<ResultadoCep> ConsultarAsync(string cep, CancellationToken ct = default);
}

public sealed record Rota(double DistanciaKm, double TempoMin, IReadOnlyList<double[]> Geometria);

/// <summary>Serviço de rotas (ex.: OSRM). Ausente → nenhuma estimativa de tempo é inventada.</summary>
public interface IProvedorRoteamento
{
    bool Disponivel { get; }
    Task<Rota?> CalcularAsync(double deLat, double deLng, double paraLat, double paraLng, CancellationToken ct = default);
}
