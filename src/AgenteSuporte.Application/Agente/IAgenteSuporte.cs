namespace AgenteSuporte.Application.Agente;

/// <summary>
/// Porta de entrada do agente. A API conversa com esta interface e não sabe
/// se por trás existe Semantic Kernel, LangChain ou um modelo local.
/// </summary>
public interface IAgenteSuporte
{
    Task<RespostaAgente> ConversarAsync(string sessaoId, string mensagem, CancellationToken ct = default);
}

public sealed record RespostaAgente(string SessaoId, string Resposta);
