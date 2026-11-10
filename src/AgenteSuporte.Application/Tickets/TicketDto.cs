using AgenteSuporte.Domain.Tickets;

namespace AgenteSuporte.Application.Tickets;

/// <summary>
/// O que sai da aplicação. A entidade de domínio nunca é exposta para fora (nem para a API, nem para o LLM).
/// </summary>
public sealed record TicketDto(
    int Id,
    string Titulo,
    string Descricao,
    string Prioridade,
    string Status,
    DateTime AbertoEmUtc,
    string? Solucao)
{
    public static TicketDto De(Ticket t) => new(
        t.Id, t.Titulo, t.Descricao, t.Prioridade.ToString(), t.Status.ToString(), t.AbertoEmUtc, t.Solucao);
}

public sealed record AbrirTicketCommand(string Titulo, string Descricao, Prioridade Prioridade);
