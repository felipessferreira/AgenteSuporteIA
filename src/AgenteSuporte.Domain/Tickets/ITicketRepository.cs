namespace AgenteSuporte.Domain.Tickets;

/// <summary>
/// Contrato de persistência do agregado Ticket. Fica no domínio; a implementação (EF Core / SQL Server)
/// fica na Infrastructure. Isso é a Inversão de Dependência na prática.
/// </summary>
public interface ITicketRepository
{
    Task AdicionarAsync(Ticket ticket, CancellationToken ct = default);
    Task<Ticket?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> ListarAbertosAsync(int limite, CancellationToken ct = default);
}
