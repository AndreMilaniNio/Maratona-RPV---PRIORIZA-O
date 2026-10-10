using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farol.API.Controllers;

public record LoginRequest(string Email, string Senha);
public record MunicipioResumoDto(int Id, string Nome, string Prefixo);
public record UsuarioSessaoDto(Guid Id, string Nome, string Email, IReadOnlyList<string> Perfis, IReadOnlyCollection<string> Permissoes,
    List<MunicipioResumoDto> Municipios, bool TodasCidades, int? MunicipioPreferidoId, int? EquipeId);
public record LoginResponse(string Token, DateTimeOffset ExpiraEm, UsuarioSessaoDto Usuario);
public record PreferenciasRequest(int? MunicipioPreferidoId);

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<Usuario> usuarios, SignInManager<Usuario> acesso, TokenService tokens, IAppDbContext db, Auditor auditor,
    IOptions<FarolOptions> opcoes) : ControllerBase
{
    /// <summary>Autentica e devolve o JWT. Mensagem única para usuário ou senha inválidos.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest r, CancellationToken ct)
    {
        var usuario = await usuarios.FindByEmailAsync(r.Email ?? "");
        if (usuario is null || !usuario.Ativo)
        {
            await auditor.RegistrarAgoraAsync(AcoesAuditoria.LoginRecusado, "Usuario", r.Email ?? "-", ct: ct);
            return Problem(statusCode: 401, title: "Credenciais inválidas.");
        }
        var resultado = await acesso.CheckPasswordSignInAsync(usuario, r.Senha ?? "", lockoutOnFailure: true);
        if (!resultado.Succeeded)
        {
            await auditor.RegistrarAgoraAsync(AcoesAuditoria.LoginRecusado, "Usuario", usuario.Id, ct: ct);
            return Problem(statusCode: 401, title: resultado.IsLockedOut ? "Acesso bloqueado temporariamente por tentativas inválidas." : "Credenciais inválidas.");
        }

        var perfis = await usuarios.GetRolesAsync(usuario);
        var (token, expira) = tokens.Gerar(usuario, perfis);
        return new LoginResponse(token, expira, await SessaoAsync(usuario, perfis.ToList(), ct));
    }

    /// <summary>Entrada sem credenciais exclusiva para a demonstração local.</summary>
    [HttpPost("operador-unico")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> OperadorUnico(CancellationToken ct)
    {
        var configuracao = opcoes.Value;
        if (!configuracao.ModoOperadorUnico || !configuracao.Demo.Habilitado)
            return NotFound();

        var usuario = await usuarios.FindByEmailAsync("admin@farol.demo");
        if (usuario is null || !usuario.Ativo)
            return Problem(statusCode: 503, title: "O operador demonstrativo ainda não está disponível.");

        var perfis = await usuarios.GetRolesAsync(usuario);
        var (token, expira) = tokens.Gerar(usuario, perfis);
        return new LoginResponse(token, expira, await SessaoAsync(usuario, perfis.ToList(), ct));
    }

    [HttpGet("/api/me")]
    [Authorize]
    public async Task<ActionResult<UsuarioSessaoDto>> Me(IUsuarioAtual atual, CancellationToken ct)
    {
        var usuario = await usuarios.FindByIdAsync(atual.Id.ToString());
        if (usuario is null || !usuario.Ativo) return Unauthorized();
        return await SessaoAsync(usuario, (await usuarios.GetRolesAsync(usuario)).ToList(), ct);
    }

    /// <summary>Guarda a última cidade escolhida no cabeçalho (no perfil, não no navegador).</summary>
    [HttpPut("/api/me/preferencias")]
    [Authorize]
    public async Task<IActionResult> Preferencias(PreferenciasRequest r, IUsuarioAtual atual, CancellationToken ct)
    {
        var permitidos = await atual.MunicipiosPermitidosAsync(ct);
        if (r.MunicipioPreferidoId is null && permitidos is not null) return Problem(statusCode: 403, title: "Visão de todas as cidades não autorizada.");
        if (r.MunicipioPreferidoId is { } m && permitidos is not null && !permitidos.Contains(m)) return Problem(statusCode: 403, title: "Município não autorizado.");
        var usuario = await db.Users.FirstAsync(u => u.Id == atual.Id, ct);
        usuario.MunicipioPreferidoId = r.MunicipioPreferidoId;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<UsuarioSessaoDto> SessaoAsync(Usuario u, List<string> perfis, CancellationToken ct)
    {
        var permissoes = Perfis.PermissoesDe(perfis);
        var todas = permissoes.Contains(Permissoes.CidadesTodas);
        var municipios = todas
            ? await db.Municipios.AsNoTracking().Where(m => m.Ativo).OrderBy(m => m.Nome).Select(m => new MunicipioResumoDto(m.Id, m.Nome, m.Prefixo)).ToListAsync(ct)
            : await db.UsuarioMunicipios.AsNoTracking().Where(x => x.UsuarioId == u.Id).OrderBy(x => x.Municipio!.Nome)
                .Select(x => new MunicipioResumoDto(x.MunicipioId, x.Municipio!.Nome, x.Municipio.Prefixo)).ToListAsync(ct);
        return new UsuarioSessaoDto(u.Id, u.Nome, u.Email ?? "", perfis, permissoes.Order().ToList(), municipios, todas, u.MunicipioPreferidoId, u.EquipeId);
    }
}
