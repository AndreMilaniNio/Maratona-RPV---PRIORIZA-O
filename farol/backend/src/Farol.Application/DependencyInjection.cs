using Farol.Application.Common;
using Farol.Application.Prioritization;
using Farol.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Farol.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<Auditor>();
        services.AddScoped<EscopoMunicipio>();
        services.AddScoped<ServicoClassificacao>();
        services.AddScoped<MontadorOrdemServico>();
        services.AddScoped<DuplicidadeService>();
        services.AddScoped<SolicitacaoService>();
        services.AddScoped<OrdemServicoConsultaService>();
        services.AddScoped<OrdemServicoComandoService>();
        services.AddScoped<EquipeService>();
        services.AddScoped<DespachoService>();
        services.AddScoped<PontuacaoService>();
        services.AddScoped<CadastroRedeService>();
        services.AddScoped<MapaService>();
        services.AddScoped<AdminService>();
        services.AddScoped<ReclassificacaoPeriodicaService>();
        return services;
    }
}
