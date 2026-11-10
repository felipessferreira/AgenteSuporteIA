namespace AgenteSuporte.Infrastructure.IA.Plugins;

/// <summary>
/// Formato padronizado de retorno das ferramentas. Quando algo falha, o LLM recebe uma mensagem clara
/// e pode corrigir os argumentos sozinho (ex.: título curto demais), em vez de receber uma exceção crua.
/// </summary>
public sealed record ResultadoFerramenta(bool Sucesso, string Mensagem, object? Dados = null)
{
    public static ResultadoFerramenta Ok(object? dados, string mensagem = "OK") => new(true, mensagem, dados);
    public static ResultadoFerramenta Falha(string mensagem) => new(false, mensagem);
}
