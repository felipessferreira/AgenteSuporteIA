namespace AgenteSuporte.Application.Agente;

/// <summary>
/// O provedor de LLM (OpenAI, Gemini, GitHub Models...) falhou ou devolveu uma resposta inválida.
/// A API converte em 502 Bad Gateway: o problema está no serviço externo, não na requisição.
/// </summary>
/// <param name="mensagem">Texto curto, seguro para exibir ao usuário.</param>
/// <param name="detalhesTecnicos">Corpo da resposta do provedor etc. Vai só para o log.</param>
public sealed class FalhaProvedorIAException(string mensagem, string? detalhesTecnicos = null, Exception? interna = null)
    : Exception(mensagem, interna)
{
    public string? DetalhesTecnicos { get; } = detalhesTecnicos;
}
