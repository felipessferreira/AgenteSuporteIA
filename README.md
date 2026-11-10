# Agente de Suporte Interno com Semantic Kernel (.NET 8)

Projeto de estudo e portfólio: um **agente de IA** que ajuda devs respondendo dúvidas com base na
documentação interna (**RAG**) e **consultando ou abrindo tickets** no **SQL Server** via **tool calling**.

Stack: C# · .NET 8 · ASP.NET Core Minimal APIs · Semantic Kernel · Azure OpenAI/OpenAI · EF Core · SQL Server · xUnit

```
Usuário ──POST──► API (.NET 8) ──► IAgenteSuporte ──► Semantic Kernel ──► LLM (Azure OpenAI)
                                                      │  ▲
                                   tool calling (auto)▼  │ resultados
                                 ┌────────────────────────────────────┐
                                 │ TicketsPlugin      DocumentacaoPlugin│
                                 │   │                      │           │
                                 │ ITicketAppService   IBaseConhecimento│
                                 │   │ (Domain: Ticket)     │ (RAG)     │
                                 │ EF Core → SQL Server   docs *.md     │
                                 └────────────────────────────────────┘
                     FiltroAuditoriaFuncoes: loga toda chamada de ferramenta
```

## Como rodar

```bash
# 1. SQL Server em container
docker compose up -d

# 2. Chave do modelo (nunca no appsettings)
cd src/AgenteSuporte.Api
dotnet user-secrets set "IA:ApiKey" "<sua-chave>"
dotnet user-secrets set "IA:Endpoint" "https://<seu-recurso>.openai.azure.com/"
dotnet user-secrets set "IA:Modelo" "<nome-do-deployment>"
#    Usando OpenAI direto em vez do Azure:
#    dotnet user-secrets set "IA:Provedor" "OpenAI"   (e IA:Modelo = gpt-4o-mini)

# 3. Rodar
dotnet run

# 4. Testes de domínio
cd ../.. && dotnet test
```

Use o arquivo `AgenteSuporte.http` (VS / VS Code / Rider) para conversar com o agente.
No console aparecem os logs de cada ferramenta que o modelo decidiu chamar.

> Os pacotes usam versões flutuantes (`1.*`, `8.0.*`). Depois do primeiro `dotnet restore`,
> fixe as versões resolvidas para builds reproduzíveis.

## Estrutura (Clean Architecture)

| Projeto | Responsabilidade | Conhece SK/EF? |
|---|---|---|
| `Domain` | Agregado `Ticket`, regras e invariantes, contratos de repositório | Não |
| `Application` | Casos de uso (`TicketAppService`) e portas (`IAgenteSuporte`, `IBaseConhecimento`) | Não |
| `Infrastructure` | EF Core/SQL Server, Semantic Kernel, plugins, filtro, RAG | Sim |
| `Api` | Endpoints REST, tratamento de erros, composição do DI | Só via DI |

As dependências apontam **sempre para dentro**: Api → Infrastructure → Application → Domain.

## Princípios aplicados (e por quê)

### SOLID
- **S: Responsabilidade Única**
  `TicketsPlugin` só traduz chamadas do LLM para casos de uso. `TicketAppService` só orquestra.
  `Ticket` só guarda regras. `FiltroAuditoriaFuncoes` só audita. Cada classe tem **um motivo para mudar**:
  se mudar a regra de título, você mexe só no `Ticket`, e não no plugin nem na API.
- **O: Aberto/Fechado**
  Novo provedor de LLM? É um `case` no registro do Kernel. Nova ferramenta? Um novo plugin.
  Nova política de governança? Um novo `IFunctionInvocationFilter`. O agente não é alterado em nenhum desses casos.
- **L: Substituição de Liskov**
  Qualquer implementação de `IBaseConhecimento` (memória, Azure AI Search, SQL Server vetorial) precisa
  devolver `TrechoDocumento` ranqueado. O plugin funciona com qualquer uma sem `if` por tipo.
- **I: Segregação de Interfaces**
  `IUnitOfWork` (salvar) é separado de `ITicketRepository` (consultar/adicionar). `IAgenteSuporte` tem um único método.
  Ninguém depende de método que não usa.
- **D: Inversão de Dependência**
  A API depende de `IAgenteSuporte`, não de Semantic Kernel. O caso de uso depende de `ITicketRepository`
  e de `TimeProvider`, não de EF nem de `DateTime.UtcNow`. Por isso é possível trocar o framework de IA
  ou testar com relógio fixo.

### DDD (na medida certa)
- **Agregado com invariantes**: `Ticket` tem construtor privado e só nasce por `Ticket.Abrir(...)`,
  que valida tudo. `Fechar()` impede fechar duas vezes. **Não existe ticket inválido no sistema**, nem quando
  quem cria é o LLM.
- **Linguagem ubíqua**: `Abrir`, `Fechar`, `Prioridade`, `Solução`. O código fala a língua do negócio.
- **Repositório no domínio, implementação na infra**: o domínio diz *o que* precisa, a infra decide *como*.
- O que ficou **de fora de propósito**: Value Objects para Título, Domain Events e CQRS. Para este tamanho de
  domínio, eles adicionariam complexidade sem ganho real.

### Boas práticas de engenharia de IA demonstradas
- **Tool calling automático** com `FunctionChoiceBehavior.Auto()`.
- **Agentic RAG**: o modelo decide quando buscar na documentação, e a resposta cita fonte e seção.
- **Human-in-the-loop**: o prompt exige confirmação antes de `abrir_ticket` (ação com efeito colateral).
- **Erros legíveis pelo LLM**: `ResultadoFerramenta` devolve a falha para o modelo poder se corrigir.
- **Governança e observabilidade**: um filtro audita nome, argumentos e duração de cada ferramenta.
- **Defesa contra prompt injection**: regra explícita no prompt de sistema, limite de tamanho da mensagem
  e ferramentas com escopo mínimo (o agente não tem função de excluir nada).
- **Segredos fora do código** via user-secrets.

## Próximos passos (bons para discutir na entrevista)
1. Trocar o RAG em memória por **embeddings + banco vetorial**, implementando `IBaseConhecimento`.
2. **Reduzir o histórico** (resumo ou janela) e persistir as conversas: engenharia de contexto.
3. **Suíte de avaliação**: dataset de perguntas e respostas esperadas, rodando no CI a cada mudança de prompt ou modelo.
4. **OpenTelemetry** com tokens, custo e latência por conversa.
5. **Multiagente**: um agente triador que encaminha para agentes especialistas (deploy, banco, Git).
6. Front-end em **React** com streaming (`GetStreamingChatMessageContentsAsync` + SSE).
