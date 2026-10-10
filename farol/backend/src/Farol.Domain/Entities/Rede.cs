using Farol.Domain.Enums;

namespace Farol.Domain.Entities;

/// <summary>Recorte territorial da operação (seção 3A).</summary>
public class Municipio
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Uf { get; set; }
    public string? CodigoIbge { get; set; }
    /// <summary>Prefixo usado na numeração de OS e solicitações.</summary>
    public required string Prefixo { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    /// <summary>Raio aproximado do território, usado só para alertar coordenadas fora do município.</summary>
    public double? RaioKm { get; set; }
    /// <summary>Preenchido quando o município tem CEP único (o CEP não identifica a rua).</summary>
    public string? CepUnico { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Demonstrativo { get; set; }
}

/// <summary>Localidade identificada pelos 3 primeiros dígitos do número do transformador.</summary>
public class Localidade
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public bool Demonstrativo { get; set; }
}

/// <summary>Subestação que alimenta o ponto da rede — o "circuito" da operação.</summary>
public class Subestacao
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    /// <summary>Local da subestação (descrição textual).</summary>
    public string? Local { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool Demonstrativo { get; set; }
    public List<ConjuntoEletrico> Conjuntos { get; set; } = [];
}

public class ConjuntoEletrico
{
    public int Id { get; set; }
    public int SubestacaoId { get; set; }
    public Subestacao? Subestacao { get; set; }
    public required string Numero { get; set; }
    public bool Demonstrativo { get; set; }
}

public class Transformador
{
    public int Id { get; set; }
    public required string NumeroCompleto { get; set; }
    public required string CodigoLocalidade { get; set; }
    public required string NumeroLocal { get; set; }
    public int? LocalidadeId { get; set; }
    public Localidade? Localidade { get; set; }
    public int? SubestacaoId { get; set; }
    public Subestacao? Subestacao { get; set; }
    public int? ConjuntoId { get; set; }
    public ConjuntoEletrico? Conjunto { get; set; }
    public int MunicipioId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool Demonstrativo { get; set; }
}

public class ClasseCliente
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    /// <summary>Quando verdadeira, o formulário pede o detalhamento de serviço essencial.</summary>
    public bool Essencial { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}

public class UnidadeConsumidora
{
    public int Id { get; set; }
    public required string Numero { get; set; }
    public required string ClienteNome { get; set; }
    public int ClasseClienteId { get; set; }
    public ClasseCliente? ClasseCliente { get; set; }
    public SituacaoUnidade Situacao { get; set; }
    public string? Logradouro { get; set; }
    public string? NumeroImovel { get; set; }
    public string? Bairro { get; set; }
    public string? Cep { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public int? TransformadorId { get; set; }
    public Transformador? Transformador { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Medidor { get; set; }
    public string? Fases { get; set; }
    public bool Demonstrativo { get; set; }
}

/// <summary>Base local de CEPs usada pelo provedor demonstrativo de consulta de CEP.</summary>
public class CepCadastrado
{
    public required string Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Bairro { get; set; }
    public int MunicipioId { get; set; }
    public bool Demonstrativo { get; set; }
}

public class TipoOcorrencia
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public TipoManutencao? TipoManutencaoSugerido { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public List<TipoOcorrenciaQualificacao> Qualificacoes { get; set; } = [];
}

/// <summary>Qualificação exigida da equipe para atender um tipo de ocorrência.</summary>
public class TipoOcorrenciaQualificacao
{
    public int TipoOcorrenciaId { get; set; }
    public int QualificacaoId { get; set; }
    public Qualificacao? Qualificacao { get; set; }
}

public class Feriado
{
    public int Id { get; set; }
    public DateOnly Data { get; set; }
    public required string Nome { get; set; }
    /// <summary>Nulo = feriado em todos os municípios.</summary>
    public int? MunicipioId { get; set; }
}
