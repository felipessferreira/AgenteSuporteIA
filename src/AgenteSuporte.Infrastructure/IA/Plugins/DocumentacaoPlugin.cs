using System.ComponentModel;
using AgenteSuporte.Application.Conhecimento;
using Microsoft.SemanticKernel;

namespace AgenteSuporte.Infrastructure.IA.Plugins;

/// <summary>
/// Expõe o RAG como ferramenta: o próprio modelo decide QUANDO buscar na documentação
/// (o chamado "agentic RAG"), em vez de buscar contexto em toda mensagem.
/// </summary>
public sealed class DocumentacaoPlugin(IBaseConhecimento baseConhecimento)
{
    [KernelFunction("buscar_documentacao")]
    [Description("Busca trechos da documentação interna (padrões de código, Git, deploy, ambientes). " +
                 "Use antes de responder qualquer dúvida sobre processos ou padrões da empresa.")]
    public async Task<ResultadoFerramenta> BuscarAsync(
        [Description("Pergunta ou palavras-chave a pesquisar.")] string consulta,
        CancellationToken ct = default)
    {
        var trechos = await baseConhecimento.BuscarAsync(consulta, quantidade: 3, ct);

        return trechos.Count == 0
            ? ResultadoFerramenta.Falha("Nenhum trecho relevante encontrado na documentação.")
            : ResultadoFerramenta.Ok(trechos, $"{trechos.Count} trecho(s) encontrado(s). Cite a fonte e a seção na resposta.");
    }
}
