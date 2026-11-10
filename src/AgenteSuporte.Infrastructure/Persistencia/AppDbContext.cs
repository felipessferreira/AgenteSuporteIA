using AgenteSuporte.Domain.Comum;
using AgenteSuporte.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace AgenteSuporte.Infrastructure.Persistencia;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    public Task<int> SalvarAlteracoesAsync(CancellationToken ct = default) => SaveChangesAsync(ct);
}
