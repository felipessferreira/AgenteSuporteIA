# Padrões de Git

## Nome de branches
Toda branch nasce a partir da branch da sprint (`sprint/<numero>`).
- Funcionalidade: `feature/<numero-sprint>-<id-tarefa>-descricao-curta`
- Correção: `fix/<numero-sprint>-<id-tarefa>-descricao-curta`
- Correção urgente em produção: `hotfix/<id-tarefa>-descricao-curta`, criada a partir da `main`

Exemplo: `fix/42-1873-timeout-relatorio-vendas`.

## Commits
Use Conventional Commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`.
Mensagens em português, no imperativo: `fix: corrige timeout no relatório de vendas`.

## Pull requests
- PR sempre para a branch da sprint, nunca direto para a `main`.
- Mínimo de 1 aprovação e pipeline verde.
- Descreva o que mudou, como testar e o impacto em banco de dados.
