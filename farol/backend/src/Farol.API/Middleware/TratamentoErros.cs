using Farol.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farol.API.Middleware;

/// <summary>Traduz exceções de negócio em respostas RFC 7807. Nada interno vaza para o cliente.</summary>
public class TratamentoErros(RequestDelegate next, ILogger<TratamentoErros> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await next(contexto);
        }
        catch (Exception ex) when (!contexto.Response.HasStarted)
        {
            var (status, titulo, extensoes) = ex switch
            {
                RegraNegocioException r => (StatusCodes.Status400BadRequest, r.Message, (object?)r.Erros),
                AcessoNegadoException a => (StatusCodes.Status403Forbidden, a.Message, null),
                NaoEncontradoException n => (StatusCodes.Status404NotFound, n.Message, null),
                ConflitoException c => (StatusCodes.Status409Conflict, c.Message, null),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "O registro foi alterado por outra pessoa. Recarregue e tente novamente.", null),
                NaoProcessavelException p => (StatusCodes.Status422UnprocessableEntity, p.Message, p.Pendencias),
                OperationCanceledException when contexto.RequestAborted.IsCancellationRequested => (499, "Requisição cancelada.", null),
                _ => (StatusCodes.Status500InternalServerError, "Erro inesperado. A ocorrência foi registrada no log do servidor.", null),
            };
            if (status == StatusCodes.Status500InternalServerError) logger.LogError(ex, "Erro não tratado em {Caminho}", contexto.Request.Path);

            var problema = new ProblemDetails { Status = status, Title = titulo, Instance = contexto.Request.Path };
            problema.Extensions["traceId"] = contexto.TraceIdentifier;
            if (ex is RegraNegocioException { Erros.Count: > 0 }) problema.Extensions["errors"] = extensoes;
            if (ex is NaoProcessavelException) problema.Extensions["pendencias"] = extensoes;

            contexto.Response.StatusCode = status;
            contexto.Response.ContentType = "application/problem+json";
            await contexto.Response.WriteAsJsonAsync(problema, contexto.RequestAborted);
        }
    }
}
