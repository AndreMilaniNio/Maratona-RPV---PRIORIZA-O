using Microsoft.AspNetCore.Identity;

namespace Farol.Domain.Entities;

public class Usuario : IdentityUser<Guid>
{
    public required string Nome { get; set; }
    public bool Ativo { get; set; } = true;
    /// <summary>Última cidade escolhida no cabeçalho (seção 3A.2); nulo = todas, se permitido.</summary>
    public int? MunicipioPreferidoId { get; set; }
    /// <summary>Equipe de campo à qual o usuário pertence, quando o perfil é Equipe de campo.</summary>
    public int? EquipeId { get; set; }
    public bool Demonstrativo { get; set; }
    public List<UsuarioMunicipio> Municipios { get; set; } = [];
}

public class UsuarioMunicipio
{
    public Guid UsuarioId { get; set; }
    public int MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
}

public class Auditoria
{
    public long Id { get; set; }
    public DateTimeOffset OcorridoEm { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public required string Acao { get; set; }
    public required string Entidade { get; set; }
    public required string EntidadeId { get; set; }
    public int? MunicipioId { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNovos { get; set; }
    public string? Justificativa { get; set; }
    public string? EnderecoIp { get; set; }
}
