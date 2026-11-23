using AgenteSuporte.Application.Agente;
using AgenteSuporte.Domain.Comum;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgenteSuporte.Api;

/// <summary>
/// Converte exceções em respostas ProblemDetails (RFC 7807): regra de negócio → 400, resto → 500.
/// </summary>
internal sealed class TratadorDeExcecoes(ILogger<TratadorDeExcecoes> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, titulo) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            BadHttpRequestException e => (e.StatusCode, "Requisição inválida"),
            FalhaProvedorIAException => (StatusCodes.Status502BadGateway, "Falha no provedor de IA"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        if (exception is FalhaProvedorIAException { DetalhesTecnicos: { Length: > 0 } detalhes })
            logger.LogError(exception, "Falha no provedor de IA. Resposta do provedor: {Detalhes}", detalhes);
        else if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado");

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
        }, ct);

        return true;
    }
}
