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
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
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
