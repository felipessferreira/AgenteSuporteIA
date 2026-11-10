using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgenteSuporte.Infrastructure.IA.Configuracao;

/// <summary>Usada para Azure OpenAI, OpenAI e provedores compatíveis com OpenAI.</summary>
internal sealed class FabricaConfiguracaoOpenAI : IFabricaConfiguracaoExecucao
{
    public PromptExecutionSettings Criar() => new OpenAIPromptExecutionSettings
    {
        // Auto = o modelo decide quais ferramentas chamar; o SK executa e devolve o resultado
        // ao modelo, em loop, até ele produzir a resposta final.
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        Temperature = 0.2,
        MaxTokens = 800
    };
}
