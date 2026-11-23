# Arquitetura e boas práticas de desenvolvimento

Resumo dos padrões de desenvolvimento da equipe: arquitetura em camadas (Clean Architecture), DDD, SOLID, Clean Code, testes, code review, segurança, APIs REST e logs. Padrões específicos de linguagem estão em padroes-dotnet-csharp.md (.NET e C#) e padroes-react.md (React).

## Arquitetura em camadas (Clean Architecture)
Os projetos seguem Clean Architecture com quatro camadas, e as dependências apontam sempre para dentro:
- Domain: entidades, regras de negócio e interfaces de repositório. Não depende de nenhum outro projeto nem de framework.
- Application: casos de uso (AppServices), DTOs e interfaces de serviços externos. Depende apenas do Domain.
- Infrastructure: EF Core, repositórios, integrações (IA, e-mail, APIs externas). Implementa as interfaces do Domain e da Application.
- Api: endpoints, configuração, injeção de dependência e tratamento de exceções. Não contém regra de negócio.
Regra prática: se uma classe do Domain precisa de `using Microsoft.EntityFrameworkCore`, ela está na camada errada.

## DDD (Domain-Driven Design): padrões táticos
- Entidade: objeto com identidade (Id) e ciclo de vida. Protege as próprias regras: setters privados e métodos com intenção (`ticket.Fechar()`, nunca `ticket.Status = ...`).
- Value Object: objeto sem identidade, imutável e comparado pelo valor (Email, Cpf, Dinheiro). Use `record` no C#.
- Agregado: grupo de entidades tratado como uma unidade de consistência. Toda alteração passa pela raiz do agregado.
- Repositório: um por raiz de agregado, com interface no Domain e implementação na Infrastructure.
- Serviço de domínio: regra que não pertence naturalmente a uma única entidade.
- Evento de domínio: algo relevante que aconteceu (`TicketFechado`), usado para desacoplar efeitos colaterais.
- Unit of Work: confirma todas as alterações de um caso de uso em uma única transação (`IUnitOfWork.SalvarAlteracoesAsync`).

## DDD (Domain-Driven Design): linguagem ubíqua e regras de negócio
- Use os termos do negócio no código, em português, iguais aos usados pela área de negócio (Ticket, Prioridade, Abrir, Fechar).
- Regra de negócio violada lança `DomainException` com mensagem clara; a API converte em HTTP 400.
- Evite modelo anêmico: entidade que só tem propriedades e deixa as regras espalhadas nos serviços.
- Validações de invariantes ficam no construtor ou nos métodos da entidade, nunca só na tela.

## SOLID
- S (Responsabilidade única): cada classe tem um único motivo para mudar.
- O (Aberto/fechado): estenda comportamento com novas classes (ex.: novo provedor de IA), sem alterar as existentes.
- L (Substituição de Liskov): implementações de uma interface devem ser intercambiáveis sem quebrar quem as usa.
- I (Segregação de interfaces): prefira interfaces pequenas e específicas a uma interface "faz tudo".
- D (Inversão de dependência): dependa de abstrações (interfaces) e receba-as por injeção de dependência.

## Clean Code e nomenclatura geral
- Nomes descritivos e no domínio do negócio; evite abreviações (`qtd`, `obj`, `aux`).
- Métodos curtos, que fazem uma coisa só; acima de ~30 linhas, avalie extrair.
- Evite comentários que repetem o código; comente o PORQUÊ de decisões não óbvias.
- Sem código morto nem código comentado: o histórico fica no Git.
- Retorne cedo (guard clauses) em vez de aninhar muitos `if`.

## Testes automatizados
- Pirâmide de testes: muitos testes de unidade (Domain e Application), menos testes de integração, poucos testes ponta a ponta.
- Padrão AAA (Arrange, Act, Assert), um comportamento por teste.
- Nome do teste descreve o cenário e o resultado: `Fechar_ticket_ja_fechado_lanca_DomainException` (metodo_cenario_resultado).
- Toda correção de bug vem acompanhada de um teste que reproduz o bug.
- Testes não dependem de ordem de execução, de horário nem de serviços externos reais.

## Code review
- Todo PR precisa de pelo menos 1 aprovação e pipeline verde (ver padroes-git.md).
- PRs pequenos: ideal até ~400 linhas alteradas; divida entregas grandes.
- O revisor verifica: regra de negócio no lugar certo, testes, tratamento de erros, segurança, nomes e impacto em banco.
- Comentários de review são sobre o código, nunca sobre a pessoa. Sugestões opcionais começam com "nit:".

## Segurança, senhas e secrets (connection string, chaves de API)
- Nunca versione senhas, chaves de API ou connection strings de ambientes reais.
- Em desenvolvimento, use `dotnet user-secrets`; nos servidores, variáveis de ambiente ou cofre de segredos.
- Valide toda entrada externa e use sempre consultas parametrizadas (o EF Core já faz isso; cuidado com `FromSqlRaw`).
- Não exponha detalhes internos em mensagens de erro da API (stack trace, SQL, respostas de terceiros).
- Logs não podem conter dados sensíveis (senhas, tokens, documentos pessoais).

## APIs REST: convenções
- Rotas no plural e em minúsculas: `/api/tickets`, `/api/tickets/{id}`.
- Verbos HTTP com significado: GET consulta, POST cria, PUT/PATCH altera, DELETE remove.
- Status: 200 OK, 201 Created, 204 No Content, 400 regra/validação, 404 não encontrado, 500 erro interno, 502 falha em serviço externo.
- Erros no formato ProblemDetails (RFC 7807), com `title` e `detail`.
- Listagens sempre paginadas ou com limite máximo.

## Logs e observabilidade
- Use logs estruturados com placeholders: `logger.LogInformation("Ticket {TicketId} fechado", id)`; nunca concatene strings.
- Níveis: Information para eventos de negócio, Warning para situações recuperáveis, Error para falhas que exigem ação.
- Toda exceção não tratada é registrada uma única vez, no tratador global de exceções.
