using AgenteSuporte.Domain.Comum;
using AgenteSuporte.Domain.Tickets;
using Xunit;

namespace AgenteSuporte.Domain.Tests;

public class TicketTests
{
    private static readonly DateTime Agora = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Abrir_com_dados_validos_cria_ticket_aberto()
    {
        var ticket = Ticket.Abrir("Deploy falhando", "Timeout na migration em HML", Prioridade.Alta, Agora);

        Assert.True(ticket.EstaAberto);
        Assert.Equal(Prioridade.Alta, ticket.Prioridade);
        Assert.Equal(Agora, ticket.AbertoEmUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Erro")]
    public void Abrir_com_titulo_invalido_lanca_DomainException(string titulo)
    {
        Assert.Throws<DomainException>(() => Ticket.Abrir(titulo, "descrição", Prioridade.Baixa, Agora));
    }

    [Fact]
    public void Fechar_ticket_ja_fechado_lanca_DomainException()
    {
        var ticket = Ticket.Abrir("Deploy falhando", "Timeout na migration", Prioridade.Media, Agora);
        ticket.Fechar("Índice criado na tabela de pedidos", Agora.AddHours(2));

        Assert.False(ticket.EstaAberto);
        Assert.Throws<DomainException>(() => ticket.Fechar("de novo", Agora.AddHours(3)));
    }
}
