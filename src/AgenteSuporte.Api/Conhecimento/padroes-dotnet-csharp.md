# Padrões de .NET e C#

Resumo dos padrões de desenvolvimento em .NET e C#: versão e estrutura da solução, nomenclatura, estilo de código, async/await, injeção de dependência, Entity Framework Core, tratamento de exceções, validação, configuração e secrets, testes com xUnit e performance.

## Versão do .NET e estrutura da solução
- Projetos novos usam a versão LTS (suporte de longo prazo) mais recente do .NET. Hoje a base usa .NET 8.
- Configurações comuns (Nullable, ImplicitUsings, TreatWarningsAsErrors) ficam no `Directory.Build.props` da raiz.
- Um projeto por camada: `Empresa.Produto.Domain`, `.Application`, `.Infrastructure`, `.Api`, e testes em `tests/`.
- Pacotes NuGet com versão fixa ou faixa controlada; atualizações de versão maior em PR separado.

## Padrão de nomenclatura em C# (nomes de classes, métodos e variáveis)
- PascalCase: classes, records, interfaces, métodos, propriedades, constantes e enums.
- camelCase: variáveis locais e parâmetros. Campos privados: `_camelCase`.
- Interfaces começam com `I` (`ITicketRepository`). Métodos assíncronos terminam com `Async`.
- Nomes de domínio em português (Ticket, Abrir, Prioridade); termos técnicos consagrados podem ficar em inglês (Repository, Dto, Handler).
- Um tipo público por arquivo, com o mesmo nome do arquivo.

## Estilo de código C#
- Namespaces por arquivo (`namespace X;`), não em bloco.
- `var` quando o tipo é evidente pelo lado direito; tipo explícito quando melhora a leitura.
- Nullable reference types habilitado: trate os avisos, não os silencie com `!` sem motivo.
- Prefira `record` para DTOs e Value Objects, `sealed` para classes que não foram projetadas para herança.
- Construtores primários e collection expressions (`[]`) são bem-vindos quando deixam o código mais simples.
- Use `string.IsNullOrWhiteSpace`, `ArgumentException.ThrowIfNullOrWhiteSpace` e `ArgumentNullException.ThrowIfNull` para guard clauses.

## Async/await e CancellationToken
- Toda operação de I/O (banco, HTTP, arquivo) é assíncrona, do endpoint até o repositório.
- Nunca use `.Result`, `.Wait()` ou `GetAwaiter().GetResult()` em código de aplicação: causa deadlock e esgota threads.
- Propague o `CancellationToken` recebido no endpoint até a última chamada.
- Evite `async void` (exceto em event handlers). Não use `Task.Run` para "tornar assíncrono" código de I/O.

## Injeção de dependência (DI) e lifetimes
- Registre dependências por camada com métodos de extensão: `AddApplication()`, `AddInfrastructure(configuration)`.
- Scoped: DbContext, repositórios e serviços que usam o DbContext (um por requisição).
- Singleton: serviços sem estado ou com estado thread-safe (cache em memória, clientes HTTP configurados).
- Transient: serviços leves e sem estado.
- Nunca injete um serviço Scoped dentro de um Singleton (captive dependency).
- Use `IOptions<T>` para configurações, com uma classe de opções por seção do appsettings.

## Entity Framework Core (EF Core): boas práticas de banco de dados
- Mapeamento com `IEntityTypeConfiguration<T>` (uma classe por entidade), nunca com Data Annotations no Domain.
- Mudanças de esquema sempre por migrations versionadas; `EnsureCreated` só em estudo ou protótipo.
- Consultas de leitura com `AsNoTracking()` e projeção (`Select`) para DTO; não traga a entidade inteira sem necessidade.
- Evite N+1: use `Include` com critério ou projeção. Pagine listagens com `Skip`/`Take`.
- `SaveChanges` uma vez por caso de uso, via Unit of Work.
- SQL manual só com `FromSql` interpolado (parametrizado); nunca concatene strings em `FromSqlRaw`.

## Tratamento de exceções e erros em .NET (como tratar exceções)
- Regras de negócio lançam `DomainException`; o tratador global (`IExceptionHandler`) converte em ProblemDetails 400.
- Não use exceções para controle de fluxo esperado (ex.: "não encontrado" retorna `null` e a API devolve 404).
- Não capture `Exception` genérica para ignorar o erro; capture o tipo específico ou deixe subir para o tratador global.
- Ao relançar, use `throw;` (preserva o stack trace), nunca `throw ex;`.

## Validação de entrada
- Validação de formato (campo obrigatório, tamanho, e-mail) na borda: endpoint ou camada Application.
- Invariantes de negócio sempre na entidade de domínio, mesmo que a tela já valide.
- Erros de validação retornam 400 com `ValidationProblem`, indicando o campo e a mensagem.

## Configuração, appsettings e user-secrets (onde guardar senhas e connection strings)
- `appsettings.json` contém apenas valores não sensíveis e padrões de desenvolvimento.
- Segredos locais com `dotnet user-secrets set "Secao:Chave" "valor"`; em servidores, variáveis de ambiente (`Secao__Chave`).
- Falte uma configuração obrigatória: falhe na inicialização com mensagem clara, não no meio de uma requisição.

## Testes em .NET: como testar com xUnit
- Framework de testes: xUnit. Um projeto de testes por camada testada (`AgenteSuporte.Domain.Tests`).
- `[Fact]` para um cenário; `[Theory]` com `[InlineData]` para o mesmo comportamento com várias entradas.
- Datas e horários são passados como parâmetro (`agoraUtc`) ou via `TimeProvider`, nunca `DateTime.Now` dentro da regra.
- Testes de integração da API com `WebApplicationFactory` e banco isolado.

## Performance em .NET
- Meça antes de otimizar (BenchmarkDotNet, Application Insights, logs de tempo).
- Use `IHttpClientFactory` (ou `AddHttpClient`); nunca crie `new HttpClient()` a cada chamada.
- Em laços com muitas concatenações, use `StringBuilder`.
- Cache (`IMemoryCache`, Redis) para dados lidos com frequência e que mudam pouco, sempre com expiração.
