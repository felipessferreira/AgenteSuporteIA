using AgenteSuporte.Application.Agente;
using AgenteSuporte.Infrastructure.IA.Configuracao;
using AgenteSuporte.Infrastructure.IA.Historico;
using AgenteSuporte.Infrastructure.IA.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgenteSuporte.Infrastructure.IA;

/// <summary>
/// Implementação do agente usando Semantic Kernel. É o ÚNICO lugar que conhece SK, então trocar de
/// framework significa escrever outra implementação de IAgenteSuporte, sem mexer na API nem no domínio.
/// </summary>
internal sealed class AgenteSuporteSemanticKernel : IAgenteSuporte
{
    private const int TamanhoMaximoMensagem = 2000;

    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IHistoricoConversas _historicos;
    private readonly IFabricaConfiguracaoExecucao _fabricaConfiguracao;

    public AgenteSuporteSemanticKernel(
        Kernel kernel,
        TicketsPlugin ticketsPlugin,
        DocumentacaoPlugin documentacaoPlugin,
        IHistoricoConversas historicos,
        IFabricaConfiguracaoExecucao fabricaConfiguracao)
    {
        // O Kernel registrado com AddKernel() é Transient: cada agente recebe uma instância própria,
        // então é seguro adicionar plugins aqui (eles dependem de serviços Scoped, como o DbContext).
        _kernel = kernel;
        _kernel.Plugins.AddFromObject(ticketsPlugin, "Tickets");
        _kernel.Plugins.AddFromObject(documentacaoPlugin, "Documentacao");

        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _historicos = historicos;
        _fabricaConfiguracao = fabricaConfiguracao;
    }

    public async Task<RespostaAgente> ConversarAsync(string sessaoId, string mensagem, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessaoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mensagem);

        if (mensagem.Length > TamanhoMaximoMensagem)
            mensagem = mensagem[..TamanhoMaximoMensagem];

        var sessao = _historicos.ObterOuCriar(sessaoId, PromptsSistema.AgenteSuporte);

        await sessao.Trava.WaitAsync(ct);
        try
        {
            sessao.Historico.AddUserMessage(mensagem);

            // A configuração vem da fábrica do provedor ativo (OpenAI ou Gemini).
            var configuracao = _fabricaConfiguracao.Criar();

            var resposta = await _chat.GetChatMessageContentAsync(sessao.Historico, configuracao, _kernel, ct);

            sessao.Historico.Add(resposta);

            return new RespostaAgente(sessaoId, resposta.Content ?? string.Empty);
        }
        finally
        {
            sessao.Trava.Release();
        }
    }
}
