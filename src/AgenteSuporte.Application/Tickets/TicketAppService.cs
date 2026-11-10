using AgenteSuporte.Domain.Comum;
using AgenteSuporte.Domain.Tickets;

namespace AgenteSuporte.Application.Tickets;

public sealed class TicketAppService(
    ITicketRepository repositorio,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : ITicketAppService
{
    private const int LimiteMaximo = 50;

    public async Task<TicketDto> AbrirAsync(AbrirTicketCommand comando, CancellationToken ct = default)
    {
        // A regra de negócio (validações) está no agregado; aqui só orquestramos.
        var ticket = Ticket.Abrir(comando.Titulo, comando.Descricao, comando.Prioridade, relogio.GetUtcNow().UtcDateTime);

        await repositorio.AdicionarAsync(ticket, ct);
        await unitOfWork.SalvarAlteracoesAsync(ct);

        return TicketDto.De(ticket);
    }

    public async Task<TicketDto?> ObterAsync(int id, CancellationToken ct = default)
    {
        var ticket = await repositorio.ObterPorIdAsync(id, ct);
        return ticket is null ? null : TicketDto.De(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> ListarAbertosAsync(int limite, CancellationToken ct = default)
    {
        var tickets = await repositorio.ListarAbertosAsync(Math.Clamp(limite, 1, LimiteMaximo), ct);
        return tickets.Select(TicketDto.De).ToList();
    }
}
