using Farol.Application.Abstractions;
using Farol.Application.Common;
using Farol.Domain.Entities;
using Farol.Infrastructure.Authentication;
using Farol.Infrastructure.Persistence.Context;
using Farol.Infrastructure.Persistence.Seed;
using Farol.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Farol.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuracao)
    {
        var conexao = configuracao.GetConnectionString("Farol")
                      ?? throw new InvalidOperationException("Defina ConnectionStrings__Farol (veja .env.example).");

        services.AddDbContext<FarolDbContext>(o => o.UseNpgsql(conexao, npg =>
        {
            npg.MigrationsAssembly(typeof(FarolDbContext).Assembly.FullName);
            npg.MigrationsHistoryTable("__ef_migracoes");
        }));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<FarolDbContext>());

        services.AddIdentityCore<Usuario>(o =>
            {
                o.Password.RequiredLength = 10;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = true;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<FarolDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<UsuarioAtual>();
        services.AddScoped<IUsuarioAtual>(sp => sp.GetRequiredService<UsuarioAtual>());
        services.AddSingleton<IRelogio, RelogioSistema>();
        services.AddScoped<IGeradorNumero, GeradorNumero>();
        services.AddScoped<TokenService>();

        services.Configure<JwtOptions>(configuracao.GetSection(JwtOptions.Secao));
        services.Configure<CepOptions>(configuracao.GetSection("Farol:Cep"));
        services.Configure<RoteamentoOptions>(configuracao.GetSection("Farol:Roteamento"));

        services.AddScoped<ProvedorCepLocal>();
        services.AddHttpClient<ProvedorCepHttp>();
        services.AddScoped<IProvedorCep>(sp =>
            sp.GetRequiredService<IOptions<CepOptions>>().Value.Provedor.Equals("Http", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<ProvedorCepHttp>()
                : sp.GetRequiredService<ProvedorCepLocal>());
        services.AddHttpClient<IProvedorRoteamento, ProvedorRoteamentoOsrm>();

        services.AddScoped<Semeador>();
        return services;
    }
}
