using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Farol.Infrastructure.Authentication;

public class JwtOptions
{
    public const string Secao = "Jwt";
    public string Emissor { get; set; } = "farol";
    public string Audiencia { get; set; } = "farol-web";
    /// <summary>Chave simétrica com ao menos 32 caracteres. Definida por variável de ambiente; nunca versionada.</summary>
    public string Chave { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 480;
}

public class TokenService(IOptions<JwtOptions> opcoes)
{
    public (string Token, DateTimeOffset ExpiraEm) Gerar(Usuario usuario, IEnumerable<string> perfis)
    {
        var o = opcoes.Value;
        var expira = DateTimeOffset.UtcNow.AddMinutes(o.ExpiracaoMinutos);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
            new("nome", usuario.Nome),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(perfis.Select(p => new Claim(ClaimTypes.Role, p)));

        var credenciais = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Chave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(o.Emissor, o.Audiencia, claims, expires: expira.UtcDateTime, signingCredentials: credenciais);
        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}

/// <summary>
/// Usuário da requisição. Permissões derivam dos perfis; municípios vêm do banco a cada requisição,
/// para que um vínculo removido valha imediatamente. Rotinas internas (semeadura) podem personificar um usuário.
/// </summary>
public class UsuarioAtual(IHttpContextAccessor http, FarolDbContext db) : IUsuarioAtual
{
    private (Guid Id, string Nome, string[] Perfis)? _personificado;
    private IReadOnlySet<int>? _municipios;
    private bool _municipiosCarregados;
    private int? _equipeId;
    private bool _equipeCarregada;

    private ClaimsPrincipal? Principal => http.HttpContext?.User;

    public void Personificar(Guid id, string nome, params string[] perfis)
    {
        _personificado = (id, nome, perfis);
        _municipiosCarregados = false;
        _equipeCarregada = false;
    }

    public bool Autenticado => _personificado is not null || Principal?.Identity?.IsAuthenticated == true;

    public Guid Id => _personificado?.Id
                      ?? (Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty);

    public string Nome => _personificado?.Nome ?? Principal?.FindFirstValue("nome") ?? "Sistema";

    public IReadOnlyList<string> Perfis => (IReadOnlyList<string>?)_personificado?.Perfis
                                           ?? Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                                           ?? [];

    public IReadOnlySet<string> Permissoes => Application.Common.Perfis.PermissoesDe(Perfis);

    public bool Tem(string permissao) => Permissoes.Contains(permissao);

    public string? EnderecoIp => http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public async Task<IReadOnlySet<int>?> MunicipiosPermitidosAsync(CancellationToken ct = default)
    {
        if (!Autenticado) return new HashSet<int>();
        if (Tem(Application.Common.Permissoes.CidadesTodas)) return null;
        if (_municipiosCarregados) return _municipios;
        var id = Id;
        _municipios = (await db.UsuarioMunicipios.AsNoTracking().Where(u => u.UsuarioId == id).Select(u => u.MunicipioId).ToListAsync(ct)).ToHashSet();
        _municipiosCarregados = true;
        return _municipios;
    }

    public async Task<int?> EquipeIdAsync(CancellationToken ct = default)
    {
        if (_equipeCarregada) return _equipeId;
        var id = Id;
        _equipeId = await db.Users.AsNoTracking().Where(u => u.Id == id).Select(u => u.EquipeId).FirstOrDefaultAsync(ct);
        _equipeCarregada = true;
        return _equipeId;
    }
}
