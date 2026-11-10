using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgenteSuporte.Infrastructure.IA.Historico;

public interface IHistoricoConversas
{
    SessaoConversa ObterOuCriar(string sessaoId, string promptSistema);
}

/// <summary>
/// Cada sessão guarda o próprio ChatHistory e uma trava, porque ChatHistory não é thread-safe
/// (duas requisições simultâneas da mesma sessão corromperiam a ordem das mensagens).
/// </summary>
public sealed record SessaoConversa(ChatHistory Historico, SemaphoreSlim Trava);

/// <summary>
/// Implementação em memória, adequada para estudo. Em produção, persista no Redis ou no SQL Server
/// e aplique uma estratégia de redução do histórico (resumo ou janela deslizante): engenharia de contexto.
/// </summary>
internal sealed class HistoricoConversasEmMemoria : IHistoricoConversas
{
    private readonly ConcurrentDictionary<string, SessaoConversa> _sessoes = new();

    public SessaoConversa ObterOuCriar(string sessaoId, string promptSistema)
        => _sessoes.GetOrAdd(sessaoId, _ => new SessaoConversa(new ChatHistory(promptSistema), new SemaphoreSlim(1, 1)));
}
