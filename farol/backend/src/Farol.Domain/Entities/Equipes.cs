using Farol.Domain.Enums;

namespace Farol.Domain.Entities;

public class Equipe
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public int MunicipioBaseId { get; set; }
    public Municipio? MunicipioBase { get; set; }
    public StatusEquipe Status { get; set; } = StatusEquipe.Disponivel;
    /// <summary>Quantidade máxima de despachos ativos simultâneos.</summary>
    public int Capacidade { get; set; } = 1;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTimeOffset? LocalizacaoAtualizadaEm { get; set; }
    public OrigemLocalizacaoEquipe? OrigemLocalizacao { get; set; }
    public bool Ativa { get; set; } = true;
    public bool Demonstrativa { get; set; }
    public uint Versao { get; set; }

    public List<EquipeMunicipio> MunicipiosAdicionais { get; set; } = [];
    public List<Integrante> Integrantes { get; set; } = [];
    public List<EquipeQualificacao> Qualificacoes { get; set; } = [];
    public List<EquipeRecurso> Recursos { get; set; } = [];
}

public class EquipeMunicipio
{
    public int EquipeId { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
}

public class Integrante
{
    public int Id { get; set; }
    public int EquipeId { get; set; }
    public required string Nome { get; set; }
    public string? Matricula { get; set; }
    public string? Funcao { get; set; }
}

public class Qualificacao
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
}

public class EquipeQualificacao
{
    public int EquipeId { get; set; }
    public int QualificacaoId { get; set; }
    public Qualificacao? Qualificacao { get; set; }
    public DateOnly? ValidaAte { get; set; }
}

public class Recurso
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
}

public class EquipeRecurso
{
    public int EquipeId { get; set; }
    public int RecursoId { get; set; }
    public Recurso? Recurso { get; set; }
}

/// <summary>Histórico de posições conhecidas da equipe.</summary>
public class LocalizacaoEquipe
{
    public long Id { get; set; }
    public int EquipeId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public OrigemLocalizacaoEquipe Origem { get; set; }
    public DateTimeOffset RegistradaEm { get; set; }
    public Guid? RegistradaPorId { get; set; }
}

/// <summary>Atribuição formal de uma OS a uma equipe.</summary>
public class Despacho
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public OrdemServico? OrdemServico { get; set; }
    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }
    public bool Ativo { get; set; } = true;
    public Guid DesignadoPorId { get; set; }
    public DateTimeOffset DesignadoEm { get; set; }
    public DateTimeOffset? AceitoEm { get; set; }
    public DateTimeOffset? IniciadoEm { get; set; }
    public DateTimeOffset? EncerradoEm { get; set; }
    public string? MotivoEncerramento { get; set; }
    public double? EquipeLatitude { get; set; }
    public double? EquipeLongitude { get; set; }
    public DateTimeOffset? EquipeLocalizacaoEm { get; set; }
    public double? DistanciaKm { get; set; }
    public double? TempoEstimadoMin { get; set; }
    /// <summary>"ROTEAMENTO" quando veio de serviço de rotas; "LINHA_RETA" quando só há distância.</summary>
    public string? OrigemEstimativa { get; set; }
    public bool ApoioIntermunicipal { get; set; }
    public bool Excecao { get; set; }
    public string? Justificativa { get; set; }
    public string? ObservacaoConclusao { get; set; }
}
