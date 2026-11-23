# Padrões de React (front-end)

Resumo dos padrões de desenvolvimento em React com TypeScript: stack (Vite), estrutura de pastas, componentes, hooks, gerenciamento de estado, chamadas à API, TypeScript, estilos, formulários, acessibilidade, testes e performance.

## Stack do front-end React
- React com TypeScript, criado e executado com Vite (`npm run dev`, `npm run build`).
- Node.js na versão LTS atual. No Windows, se o PowerShell bloquear o `npm`, use `npm.cmd`.
- Em desenvolvimento, o Vite repassa `/api` para a API .NET (proxy no `vite.config.ts`), sem necessidade de CORS.
- Não use Create React App (descontinuado).

## Estrutura de pastas do projeto React
- `src/componentes/`: componentes reutilizáveis de interface (Botao, Modal, Tabela).
- `src/paginas/`: uma pasta por tela, com os componentes específicos dela.
- `src/api/`: funções de acesso à API, uma por recurso (`tickets.ts`, `agente.ts`).
- `src/hooks/`: hooks customizados (`useTickets`, `useDebounce`).
- `src/tipos/`: tipos TypeScript compartilhados.
- Componentes de tela não chamam `fetch` diretamente: usam as funções de `src/api/`.

## Componentes React: regras e nomenclatura
- Somente componentes de função; não use componentes de classe em código novo.
- Nome do componente e do arquivo em PascalCase (`ListaTickets.tsx`), com export default do componente principal.
- Componentes pequenos e com uma responsabilidade; acima de ~150 linhas, divida.
- Props tipadas com `interface` (`interface ListaTicketsProps`); não use `any`.
- Listas sempre com `key` estável (o id do item), nunca o índice do array quando a lista pode mudar.

## Hooks do React (useState, useEffect, hooks customizados)
- Regras dos hooks: chame hooks só no topo do componente, nunca dentro de `if`, laço ou função interna.
- `useEffect` é para sincronizar com sistemas externos (API, timers, eventos do navegador), não para calcular dados derivados.
- Valores derivados das props ou do estado são calculados durante a renderização, sem `useState` extra.
- Todo `useEffect` que assina eventos ou timers retorna a função de limpeza.
- Lógica repetida entre componentes vira um hook customizado com prefixo `use`.

## Gerenciamento de estado no React
- Estado local (`useState`) é o padrão; suba o estado para o pai só quando dois componentes precisarem dele.
- Context API para dados globais que mudam pouco (usuário logado, tema), não para tudo.
- Dados vindos da API (estado do servidor) com TanStack Query (React Query): cache, carregamento, erro e nova tentativa prontos.
- Biblioteca de estado global (Zustand, Redux Toolkit) só com necessidade comprovada.

## Chamadas à API no React
- Funções de API tipadas, retornando os tipos do TypeScript (`Promise<TicketDto[]>`).
- Trate erros no formato ProblemDetails da API: mostre `detail` ou `title` ao usuário, nunca o JSON bruto.
- Mostre sempre os três estados: carregando, erro e vazio.
- Desabilite o botão de envio enquanto a requisição está em andamento, para evitar envio duplicado.
- URL base relativa (`/api/...`); nada de `http://localhost` fixo no código.

## TypeScript no React
- `strict: true` no `tsconfig.json`; não use `any` (prefira `unknown` e faça a verificação).
- Tipos dos DTOs espelham os contratos da API .NET (mesmos nomes de campos em camelCase).
- Prefira `interface` para props e objetos; `type` para uniões (`type Autor = 'usuario' | 'agente'`).

## Estilos e CSS no React
- CSS com variáveis (`:root`) para cores e espaçamentos; suporte a tema claro e escuro com `prefers-color-scheme`.
- Classes com nomes descritivos em português, um arquivo de estilo por componente ou por tela quando crescer.
- Layout responsivo: a tela deve funcionar a partir de 360px de largura.

## Formulários no React
- Formulários simples: componentes controlados com `useState`.
- Formulários grandes ou com muitas validações: React Hook Form com validação por schema (Zod).
- A validação no front melhora a experiência, mas a API sempre valida de novo.
- Enter envia formulários de uma linha; em `textarea`, Enter envia e Shift+Enter quebra a linha quando fizer sentido.

## Acessibilidade (a11y) no React
- Use elementos semânticos (`button`, `form`, `label`, `main`), não `div` clicável.
- Todo campo tem `label` ou `aria-label`; imagens têm `alt`.
- Navegação completa pelo teclado e foco visível.
- Contraste de cores adequado nos temas claro e escuro.

## Testes no React: como testar componentes (Vitest e Testing Library)
- Testes com Vitest e React Testing Library; arquivos `*.test.tsx` ao lado do componente.
- Teste o comportamento visto pelo usuário (texto na tela, clique, digitação), não detalhes de implementação.
- Busque elementos por papel e texto (`getByRole`, `getByText`), não por classe CSS.
- Mocke as chamadas à API (MSW ou mock das funções de `src/api/`).

## Performance no React
- Otimize só quando houver problema medido (React DevTools Profiler).
- `useMemo` e `useCallback` apenas para cálculos caros ou para estabilizar props de componentes memorizados.
- Divida telas grandes com `lazy` e `Suspense` (carregamento sob demanda).
- Listas muito longas: virtualização (TanStack Virtual).
