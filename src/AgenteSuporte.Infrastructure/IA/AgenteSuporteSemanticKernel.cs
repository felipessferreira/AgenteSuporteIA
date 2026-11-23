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

            ChatMessageContent resposta;
            try
            {
                resposta = await _chat.GetChatMessageContentAsync(sessao.Historico, configuracao, _kernel, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Sem resposta, a mensagem do usuário não pode ficar no histórico: a próxima chamada
                // enviaria duas mensagens de usuário seguidas (e mensagens de ferramenta órfãs).
                RemoverMensagensDesde(sessao.Historico, mensagem);
                var http = ex as HttpOperationException;
                var mensagemUsuario = (int?)http?.StatusCode switch
                {
                    429 => "O provedor de IA atingiu o limite de uso (cota). Tente novamente mais tarde ou troque de modelo/plano.",
                    401 or 403 => "A chave de API do provedor de IA é inválida ou não tem permissão. Verifique IA:ApiKey.",
                    404 => "O modelo configurado não foi encontrado no provedor de IA. Verifique IA:Modelo e IA:Endpoint.",
                    500 or 502 or 503 or 504 => "O provedor de IA está instável ou sobrecarregado no momento. Tente novamente em alguns segundos.",
                    _ => "O provedor de IA falhou ou devolveu uma resposta inválida. Tente novamente em instantes."
                };
                throw new FalhaProvedorIAException(mensagemUsuario, http?.ResponseContent, ex);
            }

            sessao.Historico.Add(resposta);

            return new RespostaAgente(sessaoId, resposta.Content ?? string.Empty);
        }
        finally
        {
            sessao.Trava.Release();
        }
    }

    /// <summary>Remove a última mensagem do usuário e tudo que foi adicionado depois dela (ex.: chamadas de ferramentas).</summary>
    private static void RemoverMensagensDesde(ChatHistory historico, string mensagemUsuario)
    {
        for (var i = historico.Count - 1; i >= 0; i--)
        {
            if (historico[i].Role == AuthorRole.User && historico[i].Content == mensagemUsuario)
            {
                historico.RemoveRange(i, historico.Count - i);
                return;
            }
        }
    }
}
