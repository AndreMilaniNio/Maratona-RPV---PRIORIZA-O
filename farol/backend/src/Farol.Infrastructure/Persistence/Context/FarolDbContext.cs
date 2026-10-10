using System.Text;
using System.Text.Json;
using Farol.Application.Abstractions;
using Farol.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Farol.Infrastructure.Persistence.Context;

public class FarolDbContext(DbContextOptions<FarolDbContext> options)
    : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<Localidade> Localidades => Set<Localidade>();
    public DbSet<Subestacao> Subestacoes => Set<Subestacao>();
    public DbSet<ConjuntoEletrico> Conjuntos => Set<ConjuntoEletrico>();
    public DbSet<Transformador> Transformadores => Set<Transformador>();
    public DbSet<ClasseCliente> ClassesCliente => Set<ClasseCliente>();
    public DbSet<UnidadeConsumidora> UnidadesConsumidoras => Set<UnidadeConsumidora>();
    public DbSet<CepCadastrado> Ceps => Set<CepCadastrado>();
    public DbSet<TipoOcorrencia> TiposOcorrencia => Set<TipoOcorrencia>();
    public DbSet<Feriado> Feriados => Set<Feriado>();
    public DbSet<Equipe> Equipes => Set<Equipe>();
    public DbSet<Integrante> Integrantes => Set<Integrante>();
    public DbSet<Qualificacao> Qualificacoes => Set<Qualificacao>();
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<LocalizacaoEquipe> LocalizacoesEquipe => Set<LocalizacaoEquipe>();
    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<HistoricoOs> HistoricosOs => Set<HistoricoOs>();
    public DbSet<VinculoSolicitacao> VinculosSolicitacao => Set<VinculoSolicitacao>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Anexo> Anexos => Set<Anexo>();
    public DbSet<Criterio> Criterios => Set<Criterio>();
    public DbSet<OpcaoCriterio> OpcoesCriterio => Set<OpcaoCriterio>();
    public DbSet<RegraPrecedencia> RegrasPrecedencia => Set<RegraPrecedencia>();
    public DbSet<Prioridade> Prioridades => Set<Prioridade>();
    public DbSet<VersaoPontuacao> VersoesPontuacao => Set<VersaoPontuacao>();
    public DbSet<PontoOpcao> PontosOpcao => Set<PontoOpcao>();
    public DbSet<FaixaPrioridade> FaixasPrioridade => Set<FaixaPrioridade>();
    public DbSet<VersaoCriterio> VersaoCriterios => Set<VersaoCriterio>();
    public DbSet<ResultadoPrioridade> ResultadosPrioridade => Set<ResultadoPrioridade>();
    public DbSet<UsuarioMunicipio> UsuarioMunicipios => Set<UsuarioMunicipio>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<NumeracaoSequencia> NumeracaoSequencias => Set<NumeracaoSequencia>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enums gravados como texto: legíveis no banco e estáveis a reordenações.
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(40);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(FarolDbContext).Assembly);

        b.Entity<Usuario>().ToTable("usuarios");
        b.Entity<IdentityRole<Guid>>().ToTable("perfis");
        b.Entity<IdentityUserRole<Guid>>().ToTable("usuario_perfis");
        b.Entity<IdentityUserClaim<Guid>>().ToTable("usuario_claims");
        b.Entity<IdentityUserLogin<Guid>>().ToTable("usuario_logins");
        b.Entity<IdentityUserToken<Guid>>().ToTable("usuario_tokens");
        b.Entity<IdentityRoleClaim<Guid>>().ToTable("perfil_claims");

        AplicarSnakeCase(b);
    }

    /// <summary>Colunas, chaves e índices em snake_case (convenção do PostgreSQL).</summary>
    private static void AplicarSnakeCase(ModelBuilder b)
    {
        foreach (var entidade in b.Model.GetEntityTypes())
        {
            foreach (var p in entidade.GetProperties())
            {
                // Token de concorrência otimista: a coluna de sistema xmin do PostgreSQL.
                if (p.IsConcurrencyToken && p.ClrType == typeof(uint))
                {
                    p.SetColumnName("xmin");
                    p.SetColumnType("xid");
                    continue;
                }
                p.SetColumnName(Snake(p.Name));
            }
            foreach (var k in entidade.GetKeys()) k.SetName(Snake(k.GetName() ?? ""));
            foreach (var fk in entidade.GetForeignKeys()) fk.SetConstraintName(Snake(fk.GetConstraintName() ?? ""));
            foreach (var i in entidade.GetIndexes()) i.SetDatabaseName(Snake(i.GetDatabaseName() ?? ""));
        }
    }

    public static string Snake(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return nome;
        var sb = new StringBuilder(nome.Length + 8);
        for (var i = 0; i < nome.Length; i++)
        {
            var c = nome[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && nome[i - 1] != '_' && (char.IsLower(nome[i - 1]) || char.IsDigit(nome[i - 1]) ||
                                                   (i + 1 < nome.Length && char.IsLower(nome[i + 1]))))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}
