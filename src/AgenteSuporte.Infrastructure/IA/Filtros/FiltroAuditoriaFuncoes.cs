using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AgenteSuporte.Infrastructure.IA.Filtros;

/// <summary>
/// Filtro do Semantic Kernel executado em TODA chamada de ferramenta feita pelo modelo.
/// É o ponto central para governança: auditoria, métricas, bloqueio de funções sensíveis,
/// pedido de aprovação humana etc. Funciona como um middleware do ASP.NET Core.
/// </summary>
internal sealed class FiltroAuditoriaFuncoes(ILogger<FiltroAuditoriaFuncoes> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var nome = $"{context.Function.PluginName}.{context.Function.Name}";
        var argumentos = string.Join(", ", context.Arguments.Select(a => $"{a.Key}={a.Value}"));
        var cronometro = Stopwatch.StartNew();

        logger.LogInformation("Agente chamou a ferramenta {Ferramenta}({Argumentos})", nome, argumentos);

        try
        {
            await next(context);
            logger.LogInformation("Ferramenta {Ferramenta} concluída em {Ms} ms", nome, cronometro.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ferramenta {Ferramenta} falhou após {Ms} ms", nome, cronometro.ElapsedMilliseconds);
            throw;
        }
    }
}
