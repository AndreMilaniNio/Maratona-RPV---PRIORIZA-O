using Farol.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Farol.API.Filters;

/// <summary>Executa o validador FluentValidation de cada argumento do corpo antes da ação.</summary>
public class ValidacaoFilter(IServiceProvider servicos) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate next)
    {
        foreach (var argumento in contexto.ActionArguments.Values.Where(a => a is not null))
        {
            var tipo = typeof(IValidator<>).MakeGenericType(argumento!.GetType());
            if (servicos.GetService(tipo) is not IValidator validador) continue;
            var resultado = await validador.ValidateAsync(new ValidationContext<object>(argumento), contexto.HttpContext.RequestAborted);
            if (!resultado.IsValid)
            {
                var erros = resultado.Errors
                    .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "geral" : char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName[1..])
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                throw new RegraNegocioException("Há campos inválidos.", erros);
            }
        }
        await next();
    }
}
