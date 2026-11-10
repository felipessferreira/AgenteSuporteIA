# Deploy e ambientes

## Ambientes
- Desenvolvimento: atualizado a cada merge na branch da sprint.
- Homologação: atualizado no fechamento da sprint, após aprovação do QA.
- Produção: janela de deploy às terças e quintas, das 20h às 22h.

## Migrations de banco de dados
Migrations do EF Core rodam automaticamente no pipeline antes do deploy da aplicação.
Migrations que alteram tabelas com mais de 1 milhão de linhas precisam ser revisadas pelo DBA
e executadas fora da janela, com script idempotente.
Timeout na migration em homologação normalmente indica lock em tabela grande: abra ticket com prioridade Alta.

## Rollback
Em caso de falha em produção, o pipeline permite reimplantar a versão anterior em um clique.
Migrations não são revertidas automaticamente: avalie com o DBA.
