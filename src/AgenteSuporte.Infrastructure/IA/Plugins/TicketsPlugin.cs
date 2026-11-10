using System.ComponentModel;
using AgenteSuporte.Application.Tickets;
using AgenteSuporte.Domain.Comum;
using AgenteSuporte.Domain.Tickets;
using Microsoft.SemanticKernel;

namespace AgenteSuporte.Infrastructure.IA.Plugins;

/// <summary>
/// Plugin = conjunto de ferramentas (tool calling) que o LLM pode decidir chamar.
/// As [Description] são lidas pelo modelo: são, na prática, o prompt de cada ferramenta.
/// O plugin é um ADAPTADOR fino: não tem regra de negócio, só traduz LLM → caso de uso.
/// </summary>
public sealed class TicketsPlugin(ITicketAppService tickets)
{
    [KernelFunction("abrir_ticket")]
    [Description("Abre um ticket de suporte no sistema. Use SOMENTE depois que o usuário confirmar explicitamente que deseja abrir o ticket.")]
    public async Task<ResultadoFerramenta> AbrirTicketAsync(
        [Description("Título curto e objetivo do problema (5 a 150 caracteres).")] string titulo,
        [Description("Descrição detalhada: o que aconteceu, passos para reproduzir e impacto.")] string descricao,
        [Description("Prioridade do ticket: Baixa, Media, Alta ou Critica.")] Prioridade prioridade,
        CancellationToken ct = default)
    {
        try
        {
            var ticket = await tickets.AbrirAsync(new AbrirTicketCommand(titulo, descricao, prioridade), ct);
            return ResultadoFerramenta.Ok(ticket, $"Ticket #{ticket.Id} aberto com sucesso.");
        }
        catch (DomainException ex)
        {
            return ResultadoFerramenta.Falha(ex.Message);
        }
    }

    [KernelFunction("consultar_ticket")]
    [Description("Consulta os detalhes e o status de um ticket pelo número.")]
    public async Task<ResultadoFerramenta> ConsultarTicketAsync(
        [Description("Número do ticket.")] int id,
        CancellationToken ct = default)
    {
        var ticket = await tickets.ObterAsync(id, ct);
        return ticket is null
            ? ResultadoFerramenta.Falha($"Ticket #{id} não encontrado.")
            : ResultadoFerramenta.Ok(ticket);
    }

    [KernelFunction("listar_tickets_abertos")]
    [Description("Lista os tickets abertos, ordenados da maior para a menor prioridade.")]
    public async Task<ResultadoFerramenta> ListarTicketsAbertosAsync(
        [Description("Quantidade máxima de tickets a retornar (1 a 20).")] int limite = 10,
        CancellationToken ct = default)
    {
        var lista = await tickets.ListarAbertosAsync(Math.Clamp(limite, 1, 20), ct);
        return ResultadoFerramenta.Ok(lista, $"{lista.Count} ticket(s) aberto(s).");
    }
}
