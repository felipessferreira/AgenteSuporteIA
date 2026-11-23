using AgenteSuporte.Api;
using AgenteSuporte.Application;
using AgenteSuporte.Application.Agente;
using AgenteSuporte.Application.Tickets;
using AgenteSuporte.Infrastructure;
using AgenteSuporte.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeExcecoes>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Atalho para estudo. Em um projeto real, use migrations (dotnet ef migrations add ...).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // O LocalDB desliga sozinho após alguns minutos ocioso. Se a API sobe bem nesse momento, a primeira
    // conexão falha ("error: 50 - SQL Server process failed to start"); alguns segundos depois funciona.
    const int tentativas = 5;
    for (var tentativa = 1; ; tentativa++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            break;
        }
        catch (Exception ex) when (tentativa < tentativas)
        {
            app.Logger.LogWarning("Banco indisponível (tentativa {Tentativa}/{Total}): {Erro}", tentativa, tentativas, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(3 * tentativa));
        }
    }
}

// ---- Agente ----
app.MapPost("/api/agente/conversas/{sessaoId}/mensagens",
    async (string sessaoId, EnviarMensagemRequest request, IAgenteSuporte agente, CancellationToken ct) =>
    {
        if (string.IsNullOrWhiteSpace(request.Mensagem))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["mensagem"] = ["A mensagem é obrigatória."]
            });

        var resposta = await agente.ConversarAsync(sessaoId, request.Mensagem, ct);
        return Results.Ok(resposta);
    });

// ---- Tickets (mesmos casos de uso que o agente usa) ----
app.MapGet("/api/tickets",
    async (ITicketAppService tickets, CancellationToken ct) => Results.Ok(await tickets.ListarAbertosAsync(50, ct)));

app.MapGet("/api/tickets/{id:int}",
    async (int id, ITicketAppService tickets, CancellationToken ct) =>
        await tickets.ObterAsync(id, ct) is { } ticket ? Results.Ok(ticket) : Results.NotFound());

app.Run();

public sealed record EnviarMensagemRequest(string Mensagem);
