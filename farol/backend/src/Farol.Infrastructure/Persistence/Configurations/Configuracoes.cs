using System.Text.Json;
using Farol.Domain.Entities;
using Farol.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farol.Infrastructure.Persistence.Configurations;

internal static class Jsonb
{
    /// <summary>Mapeia uma lista para jsonb com comparação por conteúdo.</summary>
    public static PropertyBuilder<List<T>> ComoJsonb<T>(this PropertyBuilder<List<T>> p) =>
        p.HasColumnType("jsonb").HasConversion(
            v => JsonSerializer.Serialize(v, FarolDbContext.Json),
            v => JsonSerializer.Deserialize<List<T>>(v, FarolDbContext.Json) ?? new List<T>(),
            new ValueComparer<List<T>>(
                (a, c) => JsonSerializer.Serialize(a, FarolDbContext.Json) == JsonSerializer.Serialize(c, FarolDbContext.Json),
                v => JsonSerializer.Serialize(v, FarolDbContext.Json).GetHashCode(),
                v => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(v, FarolDbContext.Json), FarolDbContext.Json)!));
}

public class MunicipioConfig : IEntityTypeConfiguration<Municipio>
{
    public void Configure(EntityTypeBuilder<Municipio> b)
    {
        b.ToTable("municipios");
        b.Property(x => x.Nome).HasMaxLength(120);
        b.Property(x => x.Uf).HasMaxLength(2).IsFixedLength();
        b.Property(x => x.CodigoIbge).HasMaxLength(7);
        b.Property(x => x.Prefixo).HasMaxLength(6);
        b.Property(x => x.CepUnico).HasMaxLength(8);
        b.HasIndex(x => x.Prefixo).IsUnique();
    }
}

public class LocalidadeConfig : IEntityTypeConfiguration<Localidade>
{
    public void Configure(EntityTypeBuilder<Localidade> b)
    {
        b.ToTable("localidades");
        b.Property(x => x.Codigo).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.Nome).HasMaxLength(120);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubestacaoConfig : IEntityTypeConfiguration<Subestacao>
{
    public void Configure(EntityTypeBuilder<Subestacao> b)
    {
        b.ToTable("subestacoes");
        b.Property(x => x.Codigo).HasMaxLength(30);
        b.Property(x => x.Nome).HasMaxLength(120);
        b.Property(x => x.Local).HasMaxLength(200);
        b.HasIndex(x => new { x.MunicipioId, x.Codigo }).IsUnique();
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Conjuntos).WithOne(x => x.Subestacao).HasForeignKey(x => x.SubestacaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConjuntoConfig : IEntityTypeConfiguration<ConjuntoEletrico>
{
    public void Configure(EntityTypeBuilder<ConjuntoEletrico> b)
    {
        b.ToTable("conjuntos_eletricos");
        b.Property(x => x.Numero).HasMaxLength(30);
        b.HasIndex(x => new { x.SubestacaoId, x.Numero }).IsUnique();
    }
}

public class TransformadorConfig : IEntityTypeConfiguration<Transformador>
{
    public void Configure(EntityTypeBuilder<Transformador> b)
    {
        b.ToTable("transformadores");
        b.Property(x => x.NumeroCompleto).HasMaxLength(20);
        b.Property(x => x.CodigoLocalidade).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.NumeroLocal).HasMaxLength(17);
        b.HasIndex(x => new { x.CodigoLocalidade, x.NumeroLocal }).IsUnique();
        b.HasIndex(x => x.NumeroCompleto);
        b.HasOne(x => x.Localidade).WithMany().HasForeignKey(x => x.LocalidadeId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Subestacao).WithMany().HasForeignKey(x => x.SubestacaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Conjunto).WithMany().HasForeignKey(x => x.ConjuntoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Municipio>().WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClasseClienteConfig : IEntityTypeConfiguration<ClasseCliente>
{
    public void Configure(EntityTypeBuilder<ClasseCliente> b)
    {
        b.ToTable("classes_cliente");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nome).HasMaxLength(80);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class UnidadeConsumidoraConfig : IEntityTypeConfiguration<UnidadeConsumidora>
{
    public void Configure(EntityTypeBuilder<UnidadeConsumidora> b)
    {
        b.ToTable("unidades_consumidoras");
        b.Property(x => x.Numero).HasMaxLength(30);
        b.Property(x => x.ClienteNome).HasMaxLength(150);
        b.Property(x => x.Cep).HasMaxLength(8);
        b.HasIndex(x => x.Numero).IsUnique();
        b.HasIndex(x => x.TransformadorId);
        b.HasOne(x => x.ClasseCliente).WithMany().HasForeignKey(x => x.ClasseClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Transformador).WithMany().HasForeignKey(x => x.TransformadorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CepConfig : IEntityTypeConfiguration<CepCadastrado>
{
    public void Configure(EntityTypeBuilder<CepCadastrado> b)
    {
        b.ToTable("ceps");
        b.HasKey(x => x.Cep);
        b.Property(x => x.Cep).HasMaxLength(8);
        b.HasOne<Municipio>().WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TipoOcorrenciaConfig : IEntityTypeConfiguration<TipoOcorrencia>
{
    public void Configure(EntityTypeBuilder<TipoOcorrencia> b)
    {
        b.ToTable("tipos_ocorrencia");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nome).HasMaxLength(120);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasMany(x => x.Qualificacoes).WithOne().HasForeignKey(x => x.TipoOcorrenciaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TipoOcorrenciaQualificacaoConfig : IEntityTypeConfiguration<TipoOcorrenciaQualificacao>
{
    public void Configure(EntityTypeBuilder<TipoOcorrenciaQualificacao> b)
    {
        b.ToTable("tipo_ocorrencia_qualificacoes");
        b.HasKey(x => new { x.TipoOcorrenciaId, x.QualificacaoId });
        b.HasOne(x => x.Qualificacao).WithMany().HasForeignKey(x => x.QualificacaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FeriadoConfig : IEntityTypeConfiguration<Feriado>
{
    public void Configure(EntityTypeBuilder<Feriado> b)
    {
        b.ToTable("feriados");
        b.Property(x => x.Nome).HasMaxLength(120);
        b.HasIndex(x => new { x.Data, x.MunicipioId });
    }
}

public class EquipeConfig : IEntityTypeConfiguration<Equipe>
{
    public void Configure(EntityTypeBuilder<Equipe> b)
    {
        b.ToTable("equipes");
        b.Property(x => x.Codigo).HasMaxLength(30);
        b.Property(x => x.Nome).HasMaxLength(120);
        b.Property(x => x.Versao).IsRowVersion();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasIndex(x => x.MunicipioBaseId);
        b.HasOne(x => x.MunicipioBase).WithMany().HasForeignKey(x => x.MunicipioBaseId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.MunicipiosAdicionais).WithOne().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Integrantes).WithOne().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Qualificacoes).WithOne().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Recursos).WithOne().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EquipeMunicipioConfig : IEntityTypeConfiguration<EquipeMunicipio>
{
    public void Configure(EntityTypeBuilder<EquipeMunicipio> b)
    {
        b.ToTable("equipe_municipios");
        b.HasKey(x => new { x.EquipeId, x.MunicipioId });
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class IntegranteConfig : IEntityTypeConfiguration<Integrante>
{
    public void Configure(EntityTypeBuilder<Integrante> b)
    {
        b.ToTable("integrantes");
        b.Property(x => x.Nome).HasMaxLength(120);
        b.Property(x => x.Matricula).HasMaxLength(30);
        b.Property(x => x.Funcao).HasMaxLength(60);
    }
}

public class QualificacaoConfig : IEntityTypeConfiguration<Qualificacao>
{
    public void Configure(EntityTypeBuilder<Qualificacao> b)
    {
        b.ToTable("qualificacoes");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class EquipeQualificacaoConfig : IEntityTypeConfiguration<EquipeQualificacao>
{
    public void Configure(EntityTypeBuilder<EquipeQualificacao> b)
    {
        b.ToTable("equipe_qualificacoes");
        b.HasKey(x => new { x.EquipeId, x.QualificacaoId });
        b.HasOne(x => x.Qualificacao).WithMany().HasForeignKey(x => x.QualificacaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecursoConfig : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> b)
    {
        b.ToTable("recursos");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class EquipeRecursoConfig : IEntityTypeConfiguration<EquipeRecurso>
{
    public void Configure(EntityTypeBuilder<EquipeRecurso> b)
    {
        b.ToTable("equipe_recursos");
        b.HasKey(x => new { x.EquipeId, x.RecursoId });
        b.HasOne(x => x.Recurso).WithMany().HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocalizacaoEquipeConfig : IEntityTypeConfiguration<LocalizacaoEquipe>
{
    public void Configure(EntityTypeBuilder<LocalizacaoEquipe> b)
    {
        b.ToTable("localizacoes_equipe");
        b.HasIndex(x => new { x.EquipeId, x.RegistradaEm });
        b.HasOne<Equipe>().WithMany().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DespachoConfig : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> b)
    {
        b.ToTable("despachos");
        b.Property(x => x.OrigemEstimativa).HasMaxLength(20);
        // Key decision 6: no máximo um despacho ativo por OS — garantido pelo banco.
        b.HasIndex(x => x.OrdemServicoId).IsUnique().HasFilter("ativo").HasDatabaseName("ux_despachos_os_ativo");
        b.HasIndex(x => new { x.EquipeId, x.Ativo });
        b.HasOne(x => x.OrdemServico).WithMany(x => x.Despachos).HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Equipe).WithMany().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.DesignadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SolicitacaoConfig : IEntityTypeConfiguration<Solicitacao>
{
    public void Configure(EntityTypeBuilder<Solicitacao> b)
    {
        b.ToTable("solicitacoes");
        b.Property(x => x.Numero).HasMaxLength(40);
        b.Property(x => x.Origem).HasMaxLength(200);
        b.Property(x => x.ProtocoloExterno).HasMaxLength(100);
        b.Property(x => x.Descricao).HasMaxLength(4000);
        b.Property(x => x.MotivoUcNaoInformada).HasMaxLength(300);
        b.Property(x => x.DadosInformados).HasColumnType("jsonb");
        b.HasIndex(x => x.Numero).IsUnique();
        b.HasIndex(x => x.ChaveIdempotencia).IsUnique();
        b.HasIndex(x => new { x.MunicipioId, x.RegistradaEm });
        b.HasIndex(x => x.OrdemServicoId);
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OrdemServico).WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RegistradaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Ucs).WithOne().HasForeignKey(x => x.SolicitacaoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SolicitacaoUcConfig : IEntityTypeConfiguration<SolicitacaoUc>
{
    public void Configure(EntityTypeBuilder<SolicitacaoUc> b)
    {
        b.ToTable("solicitacao_ucs");
        b.Property(x => x.Numero).HasMaxLength(30);
        b.HasIndex(x => x.Numero);
        b.HasOne(x => x.UnidadeConsumidora).WithMany().HasForeignKey(x => x.UnidadeConsumidoraId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OrdemServicoConfig : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> b)
    {
        b.ToTable("ordens_servico");
        b.Property(x => x.Numero).HasMaxLength(30);
        b.HasIndex(x => x.Numero).IsUnique();
        b.HasIndex(x => new { x.MunicipioId, x.Status });
        b.HasIndex(x => x.AbertaEm);
        b.HasIndex(x => x.TransformadorNumero);
        // A solicitação de origem referencia a OS; aqui fica só o id, sem FK, para não haver ciclo obrigatório.
        b.HasIndex(x => x.SolicitacaoId);
        foreach (var campo in new[] { "Logradouro", "Bairro", "Trecho", "EquipamentoDescricao", "IdentificadorEquipamento", "ChaveEletrica", "NumeroEndereco" })
            b.Property(campo).HasMaxLength(200);
        b.Property(x => x.EnderecoCompleto).HasMaxLength(400);
        b.Property(x => x.PontoReferencia).HasMaxLength(300);
        b.Property(x => x.ObservacoesLocalizacao).HasMaxLength(1000);
        b.Property(x => x.Cep).HasMaxLength(8);
        b.Property(x => x.TransformadorNumero).HasMaxLength(20);
        b.Property(x => x.TransformadorLocalidade).HasMaxLength(3);
        b.Property(x => x.TransformadorLocal).HasMaxLength(17);
        b.Property(x => x.MotivoCancelamento).HasMaxLength(500);
        b.Property(x => x.JustificativaManual).HasMaxLength(1000);
        foreach (var fato in new[] { "PessoasAfetadas", "UcsAfetadas", "ServicoEssencial", "SituacaoCliente", "CondicaoFornecimento", "Redundancia", "FonteReserva", "EquipeEspecializada", "EquipamentoAfetado", "Abrangencia", "NivelRede" })
            b.Property(fato).HasMaxLength(40);
        b.Property(x => x.Versao).IsRowVersion();

        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoOcorrencia).WithMany().HasForeignKey(x => x.TipoOcorrenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Subestacao).WithMany().HasForeignKey(x => x.SubestacaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Conjunto).WithMany().HasForeignKey(x => x.ConjuntoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Transformador>().WithMany().HasForeignKey(x => x.TransformadorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Prioridade).WithMany().HasForeignKey(x => x.PrioridadeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Prioridade>().WithMany().HasForeignKey(x => x.PrioridadeCalculadaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Prioridade>().WithMany().HasForeignKey(x => x.PrioridadeManualId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VersaoPontuacao>().WithMany().HasForeignKey(x => x.VersaoPontuacaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ResultadoAtual).WithMany().HasForeignKey(x => x.ResultadoAtualId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.CondicoesSeguranca).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Classes).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.RecursosNecessarios).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.RespostasPersonalizadas).WithOne().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OsCondicaoSegurancaConfig : IEntityTypeConfiguration<OsCondicaoSeguranca>
{
    public void Configure(EntityTypeBuilder<OsCondicaoSeguranca> b)
    {
        b.ToTable("os_condicoes_seguranca");
        b.HasKey(x => new { x.OrdemServicoId, x.Codigo });
        b.Property(x => x.Codigo).HasMaxLength(40);
    }
}

public class OsClasseClienteConfig : IEntityTypeConfiguration<OsClasseCliente>
{
    public void Configure(EntityTypeBuilder<OsClasseCliente> b)
    {
        b.ToTable("os_classes_cliente");
        b.HasKey(x => new { x.OrdemServicoId, x.ClasseClienteId });
        b.HasOne(x => x.ClasseCliente).WithMany().HasForeignKey(x => x.ClasseClienteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OsRecursoConfig : IEntityTypeConfiguration<OsRecurso>
{
    public void Configure(EntityTypeBuilder<OsRecurso> b)
    {
        b.ToTable("os_recursos");
        b.HasKey(x => new { x.OrdemServicoId, x.RecursoId });
        b.HasOne(x => x.Recurso).WithMany().HasForeignKey(x => x.RecursoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OsRespostaCriterioConfig : IEntityTypeConfiguration<OsRespostaCriterio>
{
    public void Configure(EntityTypeBuilder<OsRespostaCriterio> b)
    {
        b.ToTable("os_respostas_criterio");
        b.HasKey(x => new { x.OrdemServicoId, x.CriterioId, x.OpcaoId });
        b.HasOne<Criterio>().WithMany().HasForeignKey(x => x.CriterioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<OpcaoCriterio>().WithMany().HasForeignKey(x => x.OpcaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class HistoricoOsConfig : IEntityTypeConfiguration<HistoricoOs>
{
    public void Configure(EntityTypeBuilder<HistoricoOs> b)
    {
        b.ToTable("historico_os");
        b.Property(x => x.Tipo).HasMaxLength(40);
        b.Property(x => x.Descricao).HasMaxLength(1000);
        b.Property(x => x.UsuarioNome).HasMaxLength(150);
        b.Property(x => x.Justificativa).HasMaxLength(1000);
        b.HasIndex(x => new { x.OrdemServicoId, x.OcorridoEm });
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class VinculoSolicitacaoConfig : IEntityTypeConfiguration<VinculoSolicitacao>
{
    public void Configure(EntityTypeBuilder<VinculoSolicitacao> b)
    {
        b.ToTable("vinculos_solicitacao");
        b.Property(x => x.Justificativa).HasMaxLength(1000);
        b.HasOne<Solicitacao>().WithMany().HasForeignKey(x => x.SolicitacaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoOrigemId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NumeracaoSequenciaConfig : IEntityTypeConfiguration<NumeracaoSequencia>
{
    public void Configure(EntityTypeBuilder<NumeracaoSequencia> b)
    {
        b.ToTable("numeracao_sequencias");
        b.HasKey(x => new { x.MunicipioId, x.Ano, x.Tipo });
        b.Property(x => x.Tipo).HasMaxLength(10);
        b.HasOne<Municipio>().WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ComentarioConfig : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> b)
    {
        b.ToTable("comentarios");
        b.Property(x => x.Texto).HasMaxLength(2000);
        b.Property(x => x.UsuarioNome).HasMaxLength(150);
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AnexoConfig : IEntityTypeConfiguration<Anexo>
{
    public void Configure(EntityTypeBuilder<Anexo> b)
    {
        b.ToTable("anexos");
        b.Property(x => x.NomeArquivo).HasMaxLength(255);
        b.Property(x => x.TipoConteudo).HasMaxLength(100);
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CriterioConfig : IEntityTypeConfiguration<Criterio>
{
    public void Configure(EntityTypeBuilder<Criterio> b)
    {
        b.ToTable("criterios");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nome).HasMaxLength(120);
        b.Property(x => x.Descricao).HasMaxLength(1000);
        b.Property(x => x.RegraAplicacao).HasMaxLength(1000);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasMany(x => x.Opcoes).WithOne(x => x.Criterio).HasForeignKey(x => x.CriterioId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Municipios).WithOne().HasForeignKey(x => x.CriterioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CriterioMunicipioConfig : IEntityTypeConfiguration<CriterioMunicipio>
{
    public void Configure(EntityTypeBuilder<CriterioMunicipio> b)
    {
        b.ToTable("criterio_municipios");
        b.HasKey(x => new { x.CriterioId, x.MunicipioId });
        b.HasOne<Municipio>().WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OpcaoCriterioConfig : IEntityTypeConfiguration<OpcaoCriterio>
{
    public void Configure(EntityTypeBuilder<OpcaoCriterio> b)
    {
        b.ToTable("opcoes_criterio");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Rotulo).HasMaxLength(120);
        b.HasIndex(x => new { x.CriterioId, x.Codigo }).IsUnique();
    }
}

public class RegraPrecedenciaConfig : IEntityTypeConfiguration<RegraPrecedencia>
{
    public void Configure(EntityTypeBuilder<RegraPrecedencia> b)
    {
        b.ToTable("regras_precedencia");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nome).HasMaxLength(150);
        b.Property(x => x.Descricao).HasMaxLength(1000);
        b.Property(x => x.Condicoes).ComoJsonb();
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasOne(x => x.PrioridadeMinima).WithMany().HasForeignKey(x => x.PrioridadeMinimaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrioridadeConfig : IEntityTypeConfiguration<Prioridade>
{
    public void Configure(EntityTypeBuilder<Prioridade> b)
    {
        b.ToTable("prioridades");
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nome).HasMaxLength(60);
        b.Property(x => x.Cor).HasMaxLength(7);
        b.Property(x => x.UnidadePrazo).HasMaxLength(10);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class VersaoPontuacaoConfig : IEntityTypeConfiguration<VersaoPontuacao>
{
    public void Configure(EntityTypeBuilder<VersaoPontuacao> b)
    {
        b.ToTable("versoes_pontuacao");
        b.Property(x => x.Justificativa).HasMaxLength(1000);
        b.Property(x => x.Versao).IsRowVersion();
        b.HasIndex(x => new { x.MunicipioId, x.Numero }).IsUnique().AreNullsDistinct(false);
        // Um único rascunho (ou aguardando aprovação) por escopo.
        b.HasIndex(x => x.MunicipioId).IsUnique().AreNullsDistinct(false)
            .HasFilter("status IN ('Rascunho', 'AguardandoAprovacao')").HasDatabaseName("ux_versoes_rascunho_escopo");
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Criterios).WithOne().HasForeignKey(x => x.VersaoPontuacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Pontos).WithOne().HasForeignKey(x => x.VersaoPontuacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Faixas).WithOne().HasForeignKey(x => x.VersaoPontuacaoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class VersaoCriterioConfig : IEntityTypeConfiguration<VersaoCriterio>
{
    public void Configure(EntityTypeBuilder<VersaoCriterio> b)
    {
        b.ToTable("versao_criterios");
        b.HasKey(x => new { x.VersaoPontuacaoId, x.CriterioId });
        b.HasOne(x => x.Criterio).WithMany().HasForeignKey(x => x.CriterioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PontoOpcaoConfig : IEntityTypeConfiguration<PontoOpcao>
{
    public void Configure(EntityTypeBuilder<PontoOpcao> b)
    {
        b.ToTable("pontos_opcao");
        b.HasKey(x => new { x.VersaoPontuacaoId, x.OpcaoId });
        b.Property(x => x.AlteradoPorNome).HasMaxLength(150);
        b.HasOne(x => x.Opcao).WithMany().HasForeignKey(x => x.OpcaoId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FaixaPrioridadeConfig : IEntityTypeConfiguration<FaixaPrioridade>
{
    public void Configure(EntityTypeBuilder<FaixaPrioridade> b)
    {
        b.ToTable("faixas_prioridade");
        b.HasKey(x => new { x.VersaoPontuacaoId, x.PrioridadeId });
        b.HasOne(x => x.Prioridade).WithMany().HasForeignKey(x => x.PrioridadeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResultadoPrioridadeConfig : IEntityTypeConfiguration<ResultadoPrioridade>
{
    public void Configure(EntityTypeBuilder<ResultadoPrioridade> b)
    {
        b.ToTable("resultados_prioridade");
        b.Property(x => x.RegrasAplicadas).ComoJsonb();
        b.Property(x => x.MotivoPrincipal).HasMaxLength(1000);
        b.Property(x => x.Justificativa).HasMaxLength(1000);
        b.HasIndex(x => new { x.OrdemServicoId, x.CalculadoEm });
        b.HasOne<OrdemServico>().WithMany().HasForeignKey(x => x.OrdemServicoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.VersaoPontuacao).WithMany().HasForeignKey(x => x.VersaoPontuacaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Prioridade).WithMany().HasForeignKey(x => x.PrioridadeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Prioridade>().WithMany().HasForeignKey(x => x.PrioridadeCalculadaId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Itens).WithOne().HasForeignKey(x => x.ResultadoPrioridadeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResultadoPrioridadeItemConfig : IEntityTypeConfiguration<ResultadoPrioridadeItem>
{
    public void Configure(EntityTypeBuilder<ResultadoPrioridadeItem> b)
    {
        b.ToTable("resultados_prioridade_itens");
        b.Property(x => x.CriterioCodigo).HasMaxLength(40);
        b.Property(x => x.CriterioNome).HasMaxLength(120);
        b.Property(x => x.OpcoesCodigos).HasMaxLength(500);
        b.Property(x => x.OpcoesRotulos).HasMaxLength(1000);
    }
}

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.Property(x => x.Nome).HasMaxLength(150);
        b.HasMany(x => x.Municipios).WithOne().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Municipio>().WithMany().HasForeignKey(x => x.MunicipioPreferidoId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne<Equipe>().WithMany().HasForeignKey(x => x.EquipeId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class UsuarioMunicipioConfig : IEntityTypeConfiguration<UsuarioMunicipio>
{
    public void Configure(EntityTypeBuilder<UsuarioMunicipio> b)
    {
        b.ToTable("usuario_municipios");
        b.HasKey(x => new { x.UsuarioId, x.MunicipioId });
        b.HasOne(x => x.Municipio).WithMany().HasForeignKey(x => x.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditoriaConfig : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> b)
    {
        b.ToTable("auditoria");
        b.Property(x => x.Acao).HasMaxLength(40);
        b.Property(x => x.Entidade).HasMaxLength(60);
        b.Property(x => x.EntidadeId).HasMaxLength(80);
        b.Property(x => x.UsuarioNome).HasMaxLength(150);
        b.Property(x => x.EnderecoIp).HasMaxLength(64);
        b.Property(x => x.ValoresAnteriores).HasColumnType("jsonb");
        b.Property(x => x.ValoresNovos).HasColumnType("jsonb");
        b.Property(x => x.Justificativa).HasMaxLength(1000);
        b.HasIndex(x => x.OcorridoEm);
        b.HasIndex(x => new { x.Entidade, x.EntidadeId });
    }
}
