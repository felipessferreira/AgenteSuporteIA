using AgenteSuporte.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace AgenteSuporte.Infrastructure.Persistencia;

internal sealed class TicketRepository(AppDbContext db) : ITicketRepository
{
    public async Task AdicionarAsync(Ticket ticket, CancellationToken ct = default)
        => await db.Tickets.AddAsync(ticket, ct);

    public Task<Ticket?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Ticket>> ListarAbertosAsync(int limite, CancellationToken ct = default)
        => await db.Tickets
            .AsNoTracking()
            .Where(t => t.Status == StatusTicket.Aberto)
            .OrderByDescending(t => t.Prioridade)
            .ThenBy(t => t.AbertoEmUtc)
            .Take(limite)
            .ToListAsync(ct);
}
