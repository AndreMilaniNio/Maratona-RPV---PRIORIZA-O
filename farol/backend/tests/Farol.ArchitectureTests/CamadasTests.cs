using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Farol.ArchitectureTests;

/// <summary>Garante a direção das dependências: Domain ← Application ← Infrastructure ← API.</summary>
public class CamadasTests
{
    private static readonly Assembly Domain = typeof(Farol.Domain.Entities.OrdemServico).Assembly;
    private static readonly Assembly Application = typeof(Farol.Application.Prioritization.MotorPriorizacao).Assembly;
    private static readonly Assembly Infrastructure = typeof(Farol.Infrastructure.Persistence.Context.FarolDbContext).Assembly;
    private static readonly Assembly Api = typeof(Program).Assembly;

    private static IEnumerable<string> Referencias(Assembly a) => a.GetReferencedAssemblies().Select(r => r.Name!);

    [Fact]
    public void Dominio_nao_depende_de_aplicacao_infraestrutura_nem_ef()
    {
        var refs = Referencias(Domain).ToList();
        Assert.DoesNotContain("Farol.Application", refs);
        Assert.DoesNotContain("Farol.Infrastructure", refs);
        Assert.DoesNotContain(refs, r => r.StartsWith("Microsoft.EntityFrameworkCore"));
        Assert.DoesNotContain(refs, r => r.StartsWith("Npgsql"));
    }

    [Fact]
    public void Aplicacao_nao_depende_de_infraestrutura_api_nem_do_provedor_do_banco()
    {
        var refs = Referencias(Application).ToList();
        Assert.DoesNotContain("Farol.Infrastructure", refs);
        Assert.DoesNotContain("Farol.API", refs);
        Assert.DoesNotContain(refs, r => r.StartsWith("Npgsql"));
        Assert.DoesNotContain(refs, r => r.StartsWith("Microsoft.AspNetCore.Mvc"));
    }

    [Fact]
    public void Motor_de_priorizacao_e_puro_sem_acesso_a_banco()
    {
        var motor = typeof(Farol.Application.Prioritization.MotorPriorizacao);
        Assert.True(motor.IsAbstract && motor.IsSealed, "O motor deve ser estático.");
        var tiposUsados = motor.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType));
        Assert.DoesNotContain(tiposUsados, t => t.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true);
    }

    [Fact]
    public void Controllers_nao_usam_o_dbcontext_concreto()
    {
        var controllers = Api.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);
        foreach (var c in controllers)
        {
            var dependencias = c.GetConstructors().SelectMany(k => k.GetParameters()).Select(p => p.ParameterType);
            Assert.DoesNotContain(typeof(Farol.Infrastructure.Persistence.Context.FarolDbContext), dependencias);
        }
    }

    [Fact]
    public void Todo_controller_exige_autenticacao_exceto_o_login()
    {
        var controllers = Api.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);
        foreach (var c in controllers)
        {
            var protegido = c.GetCustomAttributes<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();
            var metodosAnonimos = c.GetMethods().Count(m => m.GetCustomAttributes<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any());
            Assert.True(protegido || c.Name == "AuthController", $"{c.Name} sem [Authorize].");
            if (c.Name != "AuthController") Assert.Equal(0, metodosAnonimos);
        }
    }

    [Fact]
    public void Infraestrutura_nao_depende_da_api() =>
        Assert.DoesNotContain("Farol.API", Referencias(Infrastructure));
}
