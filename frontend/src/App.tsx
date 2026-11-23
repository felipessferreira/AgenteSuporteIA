import { FormEvent, KeyboardEvent, useEffect, useRef, useState } from 'react';
import Markdown from 'react-markdown';
import { enviarMensagem } from './api';

interface Mensagem {
  id: number;
  autor: 'usuario' | 'agente' | 'erro';
  texto: string;
}

// Uma sessão por aba: o backend guarda o histórico da conversa por sessaoId.
const sessaoId = crypto.randomUUID();

export default function App() {
  const [mensagens, setMensagens] = useState<Mensagem[]>([]);
  const [texto, setTexto] = useState('');
  const [enviando, setEnviando] = useState(false);
  const fimRef = useRef<HTMLDivElement>(null);
  const campoRef = useRef<HTMLTextAreaElement>(null);
  const proximoId = useRef(0);

  useEffect(() => {
    fimRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [mensagens, enviando]);

  const adicionar = (autor: Mensagem['autor'], conteudo: string) =>
    setMensagens(atuais => [...atuais, { id: proximoId.current++, autor, texto: conteudo }]);

  async function enviar(e?: FormEvent) {
    e?.preventDefault();
    const pergunta = texto.trim();
    if (!pergunta || enviando) return;

    adicionar('usuario', pergunta);
    setTexto('');
    setEnviando(true);
    try {
      adicionar('agente', await enviarMensagem(sessaoId, pergunta));
    } catch (erro) {
      adicionar('erro', erro instanceof Error ? erro.message : 'Falha ao falar com o agente.');
    } finally {
      setEnviando(false);
      campoRef.current?.focus();
    }
  }

  // Enter envia; Shift+Enter quebra linha.
  function aoTeclar(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      void enviar();
    }
  }

  return (
    <div className="app">
      <main className="conversa">
        {mensagens.length === 0 && (
          <p className="vazio">Pergunte sobre tickets, deploy, padrões de Git…</p>
        )}
        {mensagens.map(m => (
          <div key={m.id} className={`mensagem ${m.autor}`}>
            {m.autor === 'agente' ? <Markdown>{m.texto}</Markdown> : <p>{m.texto}</p>}
          </div>
        ))}
        {enviando && <div className="mensagem agente digitando">Pensando…</div>}
        <div ref={fimRef} />
      </main>

      <form className="entrada" onSubmit={enviar}>
        <textarea
          ref={campoRef}
          value={texto}
          onChange={e => setTexto(e.target.value)}
          onKeyDown={aoTeclar}
          placeholder="Digite sua pergunta e pressione Enter"
          rows={1}
          maxLength={2000}
          autoFocus
        />
        <button type="submit" disabled={enviando || !texto.trim()}>
          Enviar
        </button>
      </form>
    </div>
  );
}
