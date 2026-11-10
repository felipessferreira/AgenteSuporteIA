using Microsoft.SemanticKernel;

namespace AgenteSuporte.Infrastructure.IA.Configuracao;

/// <summary>
/// Cada provedor de LLM tem a sua própria classe de configuração (OpenAI, Gemini...).
/// Esta fábrica isola essa diferença: o agente pede "uma configuração com tool calling automático"
/// e não precisa saber qual provedor está por trás.
/// </summary>
public interface IFabricaConfiguracaoExecucao
{
    PromptExecutionSettings Criar();
}
