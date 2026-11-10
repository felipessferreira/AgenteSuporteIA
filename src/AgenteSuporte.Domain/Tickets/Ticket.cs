using AgenteSuporte.Domain.Comum;

namespace AgenteSuporte.Domain.Tickets;

/// <summary>
/// Raiz de agregado: um chamado de suporte aberto por um desenvolvedor (ou pelo agente, em nome dele).
/// Todas as regras de criação e mudança de estado ficam AQUI, e não espalhadas em controllers ou plugins.
/// </summary>
public sealed class Ticket
{
    public const int TituloMinimo = 5;
    public const int TituloMaximo = 150;
    public const int TextoMaximo = 4000;

    public int Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public Prioridade Prioridade { get; private set; }
    public StatusTicket Status { get; private set; }
    public DateTime AbertoEmUtc { get; private set; }
    public DateTime? FechadoEmUtc { get; private set; }
    public string? Solucao { get; private set; }

    public bool EstaAberto => Status == StatusTicket.Aberto;

    // Construtor exigido pelo EF Core. Fica privado para que ninguém crie um Ticket inválido.
    private Ticket() { }

    public static Ticket Abrir(string titulo, string descricao, Prioridade prioridade, DateTime agoraUtc)
    {
        titulo = titulo?.Trim() ?? string.Empty;
        descricao = descricao?.Trim() ?? string.Empty;

        if (titulo.Length is < TituloMinimo or > TituloMaximo)
            throw new DomainException($"O título deve ter entre {TituloMinimo} e {TituloMaximo} caracteres.");

        if (descricao.Length == 0)
            throw new DomainException("A descrição do ticket é obrigatória.");

        if (descricao.Length > TextoMaximo)
            throw new DomainException($"A descrição deve ter no máximo {TextoMaximo} caracteres.");

        if (!Enum.IsDefined(prioridade))
            throw new DomainException("Prioridade inválida.");

        return new Ticket
        {
            Titulo = titulo,
            Descricao = descricao,
            Prioridade = prioridade,
            Status = StatusTicket.Aberto,
            AbertoEmUtc = agoraUtc
        };
    }

    public void Fechar(string solucao, DateTime agoraUtc)
    {
        if (!EstaAberto)
            throw new DomainException($"O ticket #{Id} já está fechado.");

        solucao = solucao?.Trim() ?? string.Empty;
        if (solucao.Length == 0)
            throw new DomainException("Informe a solução para fechar o ticket.");

        if (solucao.Length > TextoMaximo)
            throw new DomainException($"A solução deve ter no máximo {TextoMaximo} caracteres.");

        Solucao = solucao;
        Status = StatusTicket.Fechado;
        FechadoEmUtc = agoraUtc;
    }
}
