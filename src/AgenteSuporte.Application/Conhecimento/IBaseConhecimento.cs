namespace AgenteSuporte.Application.Conhecimento;

/// <summary>
/// Recuperação de contexto (o "R" do RAG). Hoje a implementação é uma busca simples em memória;
/// amanhã pode ser Azure AI Search, Qdrant ou o tipo VECTOR do SQL Server, sem tocar no agente.
/// </summary>
public interface IBaseConhecimento
{
    Task<IReadOnlyList<TrechoDocumento>> BuscarAsync(string consulta, int quantidade, CancellationToken ct = default);
}

public sealed record TrechoDocumento(string Fonte, string Secao, string Conteudo, double Relevancia);
