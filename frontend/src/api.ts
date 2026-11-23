export interface RespostaAgente {
  sessaoId: string;
  resposta: string;
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export async function enviarMensagem(sessaoId: string, mensagem: string): Promise<string> {
  const resposta = await fetch(`/api/agente/conversas/${encodeURIComponent(sessaoId)}/mensagens`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mensagem })
  });

  if (!resposta.ok) {
    const problema = (await resposta.json().catch(() => null)) as ProblemDetails | null;
    const validacao = problema?.errors ? Object.values(problema.errors).flat().join(' ') : undefined;
    throw new Error(validacao || problema?.detail || problema?.title || `Erro HTTP ${resposta.status}`);
  }

  const dados = (await resposta.json()) as RespostaAgente;
  return dados.resposta;
}
