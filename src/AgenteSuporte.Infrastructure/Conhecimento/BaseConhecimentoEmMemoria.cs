using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AgenteSuporte.Application.Conhecimento;
using Microsoft.Extensions.Options;

namespace AgenteSuporte.Infrastructure.Conhecimento;

public sealed class OpcoesBaseConhecimento
{
    public const string Secao = "BaseConhecimento";
    public string Pasta { get; set; } = "Conhecimento";
}

/// <summary>
/// RAG didático: carrega arquivos .md, quebra em chunks por seção (## Título) e ranqueia
/// por sobreposição de termos. Serve para entender o fluxo; em produção troque por embeddings
/// + banco vetorial implementando a MESMA interface IBaseConhecimento.
/// </summary>
internal sealed class BaseConhecimentoEmMemoria : IBaseConhecimento
{
    private static readonly HashSet<string> StopWords =
    [
        "que", "com", "para", "por", "uma", "como", "dos", "das", "nos", "nas", "não", "nao",
        "qual", "quais", "onde", "quando", "sobre", "isso", "esse", "essa", "este", "esta", "the", "and"
    ];

    private static readonly Regex SeparadorDeTermos = new(@"[^\p{L}\p{N}]+", RegexOptions.Compiled);

    private readonly Lazy<IReadOnlyList<Chunk>> _chunks;

    public BaseConhecimentoEmMemoria(IOptions<OpcoesBaseConhecimento> opcoes)
    {
        var pasta = Path.Combine(AppContext.BaseDirectory, opcoes.Value.Pasta);
        _chunks = new Lazy<IReadOnlyList<Chunk>>(() => CarregarChunks(pasta));
    }

    public Task<IReadOnlyList<TrechoDocumento>> BuscarAsync(string consulta, int quantidade, CancellationToken ct = default)
    {
        var termosConsulta = Tokenizar(consulta);
        if (termosConsulta.Count == 0)
            return Task.FromResult<IReadOnlyList<TrechoDocumento>>([]);

        IReadOnlyList<TrechoDocumento> resultado = _chunks.Value
            .Select(c => new
            {
                Chunk = c,
                Score = (double)termosConsulta.Count(c.Termos.Contains) / termosConsulta.Count
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(Math.Clamp(quantidade, 1, 5))
            .Select(x => new TrechoDocumento(x.Chunk.Fonte, x.Chunk.Secao, x.Chunk.Conteudo, Math.Round(x.Score, 2)))
            .ToList();

        return Task.FromResult(resultado);
    }

    private static List<Chunk> CarregarChunks(string pasta)
    {
        if (!Directory.Exists(pasta))
            return [];

        var chunks = new List<Chunk>();

        foreach (var arquivo in Directory.EnumerateFiles(pasta, "*.md"))
        {
            var fonte = Path.GetFileName(arquivo);
            string secaoAtual = fonte;
            var conteudo = new StringBuilder();

            foreach (var linha in File.ReadLines(arquivo))
            {
                if (linha.StartsWith("## "))
                {
                    AdicionarChunk(chunks, fonte, secaoAtual, conteudo);
                    secaoAtual = linha[3..].Trim();
                    conteudo.Clear();
                }
                else
                {
                    conteudo.AppendLine(linha);
                }
            }

            AdicionarChunk(chunks, fonte, secaoAtual, conteudo);
        }

        return chunks;
    }

    private static void AdicionarChunk(List<Chunk> chunks, string fonte, string secao, StringBuilder conteudo)
    {
        var texto = conteudo.ToString().Trim();
        if (texto.Length == 0) return;

        chunks.Add(new Chunk(fonte, secao, texto, Tokenizar($"{secao} {texto}")));
    }

    private static HashSet<string> Tokenizar(string texto)
    {
        var normalizado = RemoverAcentos(texto.ToLowerInvariant());

        return SeparadorDeTermos.Split(normalizado)
            .Where(t => t.Length > 2 && !StopWords.Contains(t))
            .ToHashSet();
    }

    private static string RemoverAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record Chunk(string Fonte, string Secao, string Conteudo, HashSet<string> Termos);
}
