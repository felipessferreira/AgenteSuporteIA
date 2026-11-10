using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace AgenteSuporte.Infrastructure.IA.Configuracao;

/// <summary>Usada com o conector nativo do Google Gemini (API generateContent).</summary>
internal sealed class FabricaConfiguracaoGemini : IFabricaConfiguracaoExecucao
{
    public PromptExecutionSettings Criar() => new GeminiPromptExecutionSettings
    {
        // Equivalente ao FunctionChoiceBehavior.Auto() no conector do Gemini.
        ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 0.2,
        MaxTokens = 800
    };
}
