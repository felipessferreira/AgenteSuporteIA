using AgenteSuporte.Application.Agente;
using AgenteSuporte.Application.Conhecimento;
using AgenteSuporte.Domain.Comum;
using AgenteSuporte.Domain.Tickets;
using AgenteSuporte.Infrastructure.Conhecimento;
using AgenteSuporte.Infrastructure.IA;
using AgenteSuporte.Infrastructure.IA.Configuracao;
using AgenteSuporte.Infrastructure.IA.Filtros;
using AgenteSuporte.Infrastructure.IA.Historico;
using AgenteSuporte.Infrastructure.IA.Plugins;
using AgenteSuporte.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OpenAI;
using System.ClientModel;

namespace AgenteSuporte.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuracao)
    {
        AdicionarPersistencia(services, configuracao);
        AdicionarBaseConhecimento(services, configuracao);
        AdicionarIA(services, configuracao);
        return services;
    }

    private static void AdicionarPersistencia(IServiceCollection services, IConfiguration configuracao)
    {
        var connectionString = configuracao.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer não configurada.");

        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
    }

    private static void AdicionarBaseConhecimento(IServiceCollection services, IConfiguration configuracao)
    {
        services.Configure<OpcoesBaseConhecimento>(configuracao.GetSection(OpcoesBaseConhecimento.Secao));
        services.AddSingleton<IBaseConhecimento, BaseConhecimentoEmMemoria>();
    }

    private static void AdicionarIA(IServiceCollection services, IConfiguration configuracao)
    {
        var opcoes = configuracao.GetSection(OpcoesIA.Secao).Get<OpcoesIA>()
            ?? throw new InvalidOperationException("Seção 'IA' não configurada.");

        if (string.IsNullOrWhiteSpace(opcoes.ApiKey) || string.IsNullOrWhiteSpace(opcoes.Modelo))
            throw new InvalidOperationException("Configure IA:Modelo e IA:ApiKey (use dotnet user-secrets).");

        var kernelBuilder = services.AddKernel();

        switch (opcoes.Provedor)
        {
            case "AzureOpenAI":
                if (string.IsNullOrWhiteSpace(opcoes.Endpoint))
                    throw new InvalidOperationException("Configure IA:Endpoint para Azure OpenAI.");
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    deploymentName: opcoes.Modelo, endpoint: opcoes.Endpoint, apiKey: opcoes.ApiKey);
                services.AddSingleton<IFabricaConfiguracaoExecucao, FabricaConfiguracaoOpenAI>();
                break;

            case "OpenAI":
                kernelBuilder.AddOpenAIChatCompletion(modelId: opcoes.Modelo, apiKey: opcoes.ApiKey);
                services.AddSingleton<IFabricaConfiguracaoExecucao, FabricaConfiguracaoOpenAI>();
                break;

            // Qualquer provedor que exponha a API no formato da OpenAI: Gemini, Ollama, GitHub Models, Groq...
            case "OpenAICompativel":
                if (string.IsNullOrWhiteSpace(opcoes.Endpoint))
                    throw new InvalidOperationException("Configure IA:Endpoint para o provedor compatível com OpenAI.");

                var cliente = new OpenAIClient(
                    new ApiKeyCredential(opcoes.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(opcoes.Endpoint) });

                kernelBuilder.AddOpenAIChatCompletion(modelId: opcoes.Modelo, openAIClient: cliente);
                services.AddSingleton<IFabricaConfiguracaoExecucao, FabricaConfiguracaoOpenAI>();
                break;

            // Conector nativo do Gemini (API generateContent). Funciona com as chaves novas "AQ." do AI Studio.
            case "GoogleGemini":
                kernelBuilder.AddGoogleAIGeminiChatCompletion(modelId: opcoes.Modelo, apiKey: opcoes.ApiKey);
                services.AddSingleton<IFabricaConfiguracaoExecucao, FabricaConfiguracaoGemini>();
                break;

            default:
                throw new InvalidOperationException($"Provedor de IA '{opcoes.Provedor}' não suportado.");
        }

        // Filtros registrados no DI são aplicados automaticamente a todo Kernel criado pelo container.
        services.AddSingleton<IFunctionInvocationFilter, FiltroAuditoriaFuncoes>();

        services.AddSingleton<IHistoricoConversas, HistoricoConversasEmMemoria>();
        services.AddScoped<TicketsPlugin>();
        services.AddScoped<DocumentacaoPlugin>();
        services.AddScoped<IAgenteSuporte, AgenteSuporteSemanticKernel>();
    }
}
