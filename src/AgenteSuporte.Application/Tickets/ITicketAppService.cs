namespace AgenteSuporte.Application.Tickets;

/// <summary>
/// Casos de uso de tickets. Usados tanto pela API REST quanto pelo plugin do agente:
/// a mesma regra serve para humanos e para a IA.
/// </summary>
public interface ITicketAppService
{
    Task<TicketDto> AbrirAsync(AbrirTicketCommand comando, CancellationToken ct = default);
    Task<TicketDto?> ObterAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TicketDto>> ListarAbertosAsync(int limite, CancellationToken ct = default);
}
