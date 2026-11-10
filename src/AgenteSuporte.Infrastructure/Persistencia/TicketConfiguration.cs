using AgenteSuporte.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgenteSuporte.Infrastructure.Persistencia;

/// <summary>
/// Mapeamento separado da entidade: o domínio não recebe atributos de EF ([Key], [MaxLength]...).
/// </summary>
internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo).HasMaxLength(Ticket.TituloMaximo).IsRequired();
        builder.Property(t => t.Descricao).HasMaxLength(Ticket.TextoMaximo).IsRequired();
        builder.Property(t => t.Solucao).HasMaxLength(Ticket.TextoMaximo);

        // Enums gravados como int para permitir ORDER BY por prioridade no banco.
        builder.Property(t => t.Prioridade).IsRequired();
        builder.Property(t => t.Status).IsRequired();

        builder.Property(t => t.AbertoEmUtc).IsRequired();

        builder.Ignore(t => t.EstaAberto);

        builder.HasIndex(t => new { t.Status, t.Prioridade });
    }
}
