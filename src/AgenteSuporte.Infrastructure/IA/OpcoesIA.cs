namespace AgenteSuporte.Infrastructure.IA;

public sealed class OpcoesIA
{
    public const string Secao = "IA";

    /// <summary>"AzureOpenAI" ou "OpenAI".</summary>
    public string Provedor { get; set; } = "AzureOpenAI";

    /// <summary>Nome do deployment (Azure) ou do modelo (OpenAI), ex.: gpt-4o-mini.</summary>
    public string Modelo { get; set; } = string.Empty;

    /// <summary>Obrigatório apenas para Azure OpenAI.</summary>
    public string? Endpoint { get; set; }

    /// <summary>Nunca versione: use dotnet user-secrets ou variável de ambiente IA__ApiKey.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
