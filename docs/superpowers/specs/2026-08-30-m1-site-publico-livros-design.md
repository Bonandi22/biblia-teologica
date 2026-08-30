# M1 — Site público de Livros (design)

**Data:** 2026-08-30
**Estado:** aguardando revisão do utilizador
**Marco:** M1 de 5 (ver `docs/ROADMAP.md`)

---

## 1. Contexto

**Bíblia Teológica** é um portal de conhecimento teológico — livros da Bíblia, estudos
long-form, vídeos, teologia sistemática e conteúdo afim. Projeto **híbrido**: peça de
portfólio (Software Engineer .NET) *e* site pessoal que precisa ser leve de manter.

A primeira vertical é o **fichário dos 66 livros do cânon protestante** — já existe um
protótipo HTML/JS single-file (sidebar + busca + "cartão de fichário" com campos
estruturados) que define o modelo de dados, o visual e as interações. O M1 transforma
esse protótipo num sistema em camadas, com API e SPA, pronto para deploy (sem deploy
ainda).

## 2. Visão de produto (roadmap)

| Marco | Entrega | Novo no projeto |
|---|---|---|
| **M1** | Site público de Livros (esta spec) | Solução .NET em camadas, EF Core + SQLite, Angular, Docker, CI |
| **M2** | Autenticação (Identity + JWT) + Admin de Livros | Identity, roles, guards Angular, formulários, upload |
| **M3** | Estudos — artigos long-form em Markdown (público + admin) | Markdown, sanitização, editor. Vira o "template" das próximas verticais |
| **M3.5** | Vídeos — embed YouTube/Vimeo + metadados | Entidade simples sem corpo |
| **M4** | Cadastro público + Newsletter (captura; envio depois) | Fluxo de registo público, e-mail transacional |
| **M5+** | Doutrinas, Glossário, Cronologia, Teologia Sistemática (séries de Estudos), disparo de e-mail marketing | Repetição de padrão |

**Regra de três:** quando aparecer o 2º tipo long-form (M3), avaliar extrair uma base
comum `Conteudo` (slug, título, publicado, SEO, timestamps, categoria, tags). No M1,
`Livro` fica sozinho — sem abstração prematura.

## 3. Escopo do M1

### Dentro
- Solução .NET 8, Clean Architecture 4 camadas: Domain → Application → Infrastructure → Api
- Camada Application no estilo **Application Services** (igual ao projeto ICM), não CQRS/MediatR
- Persistência EF Core + **SQLite** (arquivo único, volume Docker, sem container de banco)
- Seed automático dos 66 livros no startup a partir de `livros.json` (o JSON do protótipo)
- API pública read-only: `GET /api/livros`, `GET /api/livros/{slug}`, `GET /health`
- Rate limiting global + middleware de security headers + CORS restrito + erros higienizados
- SPA Angular 22 (standalone, signals, **sem SSR**) reproduzindo o protótipo: lista+busca e ficha
- Docker-first: `docker-compose` (api + web), override de dev com hot reload, `docker-compose.prod.yml` de exemplo (não executado)
- Testes: Domain, Application, Api (integração), Frontend (mínimo)
- CI GitHub Actions: `ci.yml` (build+test back e front) + `claude-code-review.yml` (review por PR)
- `CLAUDE.md` adaptado do ICM + `docs/ROADMAP.md`

### Fora (registado no ROADMAP)
Auth/Identity, área admin, qualquer CRUD/escrita, upload, cadastro público, newsletter,
Estudos, Vídeos, Teologia Sistemática, Doutrinas, Glossário, Cronologia, SSR/prerender,
deploy real, versionamento de API.

## 4. Estrutura da solução

```
BibliaTeologica/
├── CLAUDE.md                         # 1ª tarefa do plano (adaptado do ICM)
├── .gitignore  .editorconfig
├── BibliaTeologica.sln
├── Directory.Packages.props          # Central Package Management
├── Directory.Build.props             # nullable enable, LangVersion, warnings-as-errors
├── docker-compose.yml                # api + web
├── docker-compose.override.yml       # dev: hot reload, Swagger, volumes de código
├── docker-compose.prod.yml           # exemplo pronto-pra-deploy (não usado ainda)
│
├── src/
│   ├── BibliaTeologica.Domain/
│   │   ├── Entities/Livro.cs
│   │   ├── Enums/SecaoCanonica.cs
│   │   ├── Slugs/Slug.cs               # classe estática: Slug.From(nome)
│   │   └── Exceptions/DomainException.cs
│   ├── BibliaTeologica.Application/
│   │   ├── Common/Interfaces/ILivroRepository.cs
│   │   ├── Common/Mapping/LivroMappingProfile.cs
│   │   ├── Livros/Dtos/{LivroListItemDto,LivroDetailDto}.cs
│   │   ├── Livros/{ILivroQueryService,LivroQueryService}.cs
│   │   └── DependencyInjection.cs
│   ├── BibliaTeologica.Infrastructure/
│   │   ├── Persistence/AppDbContext.cs
│   │   ├── Persistence/Configurations/LivroConfiguration.cs
│   │   ├── Persistence/Migrations/
│   │   ├── Persistence/Seed/{livros.json,LivroSeeder.cs}
│   │   ├── Repositories/LivroRepository.cs
│   │   └── DependencyInjection.cs
│   └── BibliaTeologica.Api/
│       ├── Controllers/LivrosController.cs
│       ├── Middleware/{ExceptionHandlingMiddleware,SecurityHeadersMiddleware}.cs
│       ├── Program.cs
│       ├── appsettings.json                 # sem segredos
│       ├── appsettings.Development.json      # git-ignored
│       ├── Dockerfile  .dockerignore
│
├── tests/
│   ├── BibliaTeologica.Domain.Tests/
│   ├── BibliaTeologica.Application.Tests/
│   └── BibliaTeologica.Api.Tests/           # WebApplicationFactory + SQLite
│
├── frontend/biblia-teologica-web/           # Angular 22, standalone, signals
│   ├── Dockerfile  .dockerignore  proxy.conf.json
│   └── src/app/
│       ├── core/services/livro.service.ts
│       ├── core/models/livro.model.ts
│       ├── features/biblia/{biblia-lista,biblia-ficha}/
│       ├── features/biblia/biblia.routes.ts
│       ├── shared/components/{badge-secao,campo-ficha}/
│       ├── styles/{_variables.scss,_mixins.scss}
│       ├── app.routes.ts  app.config.ts
│
├── .github/workflows/{ci.yml,claude-code-review.yml}
└── docs/
    ├── superpowers/specs/   superpowers/plans/
    └── ROADMAP.md
```

Nomes de projeto: `BibliaTeologica.Api` (não misturar `.Api`/`.WebApi`).
`Doutrina`/`TermoGlossario`/`Estudo` **não entram no M1**.

## 5. Domain

### `SecaoCanonica` (enum) — `Domain/Enums/`

10 seções na ordem canônica; valor de exibição em `[Description]`, resolvido na camada
de mapeamento (o Domain guarda o enum, não a string).

```
Pentateuco=1 "PENTATEUCO"          | Evangelhos=6 "EVANGELHOS"
LivrosHistoricos=2 "LIVROS HISTÓRICOS" | Atos=7 "ATOS"
LivrosPoeticos=3 "LIVROS POÉTICOS"  | CartasPaulinas=8 "CARTAS PAULINAS"
ProfetasMaiores=4 "PROFETAS MAIORES" | CartasGerais=9 "CARTAS GERAIS"
ProfetasMenores=5 "PROFETAS MENORES" | Apocalipse=10 "APOCALIPSE"
```

`Testamento` — enum derivado (Antigo ≤ ProfetasMenores; Novo ≥ Evangelhos), **não persistido**.

### `Livro` (entidade) — `Domain/Entities/`

Sem setters públicos; criada só via `Livro.Create(...)`. Construtor `private Livro()` para o EF Core.

| Propriedade | Tipo | Regra |
|---|---|---|
| `Id` | `int` | identity |
| `Slug` | `string` | gerado de `Nome` via `Slug.From()`; único |
| `Nome` | `string` | obrigatório, não vazio |
| `Secao` | `SecaoCanonica` | enum definido |
| `Ordem` | `int` | 1–66; único |
| `Quem` | `string` | obrigatório |
| `Porque` | `string` | obrigatório |
| `ParaQuem` | `string` | obrigatório |
| `Estrutura` | `string` | obrigatório |
| `Tema` | `string` | obrigatório |
| `Ideias` | `IReadOnlyList<string>` | ≥ 1 item; backing field `_ideias` |
| `Versiculo` | `string` | obrigatório |
| `Publicado` | `bool` | default `true` |
| `CriadoEm` / `AtualizadoEm` | `DateTime` UTC | auditoria |

- `Slug.From(string)` — minúsculas, remove acentos (`GÊNESIS`→`genesis`), não-alfanumérico→`-`,
  colapsa hífens repetidos, trim. `"1 SAMUEL"` → `1-samuel`.
- `Create(...)` valida cada campo e lança `DomainException` em invariante violada;
  define `Slug`, `CriadoEm`/`AtualizadoEm = DateTime.UtcNow`, `Publicado = true`.
- Métodos de mutação (`AtualizarConteudo`, `Renomear`, `MoverPara`, `Publicar`/`Despublicar`)
  ficam para o **M2** — não implementar agora.
- `DomainException` — `Domain/Exceptions/`, mapeada para HTTP 400 no middleware.

### Testes de Domain (`BibliaTeologica.Domain.Tests`)
- `Create_GeraSlugSemAcentos_QuandoNomeTemAcento`
- `Create_LancaDomainException_QuandoNomeVazio`
- `Create_LancaDomainException_QuandoOrdemForaDoIntervalo`
- `Create_LancaDomainException_QuandoIdeiasVazia`
- `Slug_From_ConverteEspacosEmHifens`

## 6. Application

### DTOs (contrato da API, nunca a entidade)

```
LivroListItemDto(Slug, Nome, Ordem, Secao, SecaoRotulo)
LivroDetailDto(Slug, Nome, Ordem, Secao, SecaoRotulo, Testamento,
               Quem, Porque, ParaQuem, Estrutura, Tema, Ideias[], Versiculo)
```

`Secao` = nome do enum (`"LivrosHistoricos"`, estável para código do frontend);
`SecaoRotulo` = `[Description]` (`"LIVROS HISTÓRICOS"`, para exibir).

### `ILivroQueryService` / `LivroQueryService`

```
Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct)
    -> repo.ListarPublicadosAsync() ordenado por Ordem -> map

Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct)
    -> repo.ObterPorSlugAsync(slug); null se não achar OU não publicado
```

### `ILivroRepository` — `Common/Interfaces/`
```
Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct)
Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct)
```

### AutoMapper — `LivroMappingProfile`
Só mapeia: `Secao` → `.ToString()`, `SecaoRotulo` → helper de `[Description]`,
`Testamento` → derivado. **Zero lógica de negócio.**

### `DependencyInjection.AddApplication()`
Regista `ILivroQueryService`, AutoMapper (assembly scan de perfis).

### Testes (`BibliaTeologica.Application.Tests` — xUnit + Moq + AwesomeAssertions)
- `ListarAsync_RetornaSomentePublicados_OrdenadosPorOrdem`
- `ObterPorSlugAsync_RetornaNull_QuandoSlugNaoExiste`
- `ObterPorSlugAsync_RetornaNull_QuandoLivroDespublicado`
- `ObterPorSlugAsync_MapeiaSecaoRotuloETestamento_Corretamente`

## 7. Infrastructure

### `AppDbContext`
`DbSet<Livro> Livros`; `ApplyConfigurationsFromAssembly`.

### `LivroConfiguration : IEntityTypeConfiguration<Livro>`
- `Slug` → índice **único**; `Ordem` → índice único
- `Secao` → `.HasConversion<string>()` (guarda `"LivrosHistoricos"`, legível no `.db`)
- `Ideias` → coleção primitiva EF Core 8 (JSON automático); mapeia backing field `_ideias`
- `Nome`/`Slug` com `MaxLength`; textos longos sem limite

### `LivroRepository : ILivroRepository`
`AsNoTracking()` em tudo (só-leitura no M1); sem `Include` (não há navegação).

### Seed — `Persistence/Seed/`
- `livros.json` — o JSON do protótipo **verbatim**, adicionando só `"slug"` e `"ordem"`
  em cada objeto (gerado uma vez a partir do array `BOOKS`). `section` continua string
  (`"PENTATEUCO"`), convertida para enum no seeder.
- `LivroSeeder.SeedAsync()` — lê o JSON; **se a tabela estiver vazia**, insere os 66 via
  `Livro.Create(...)`. Idempotente.

### `DependencyInjection.AddInfrastructure(config)`
`AddDbContext` SQLite (`ConnectionStrings:Default`), regista `ILivroRepository`, `LivroSeeder`.

### Migração inicial
`dotnet ef migrations add InitialCreate` — `Migrations/` commitado no repo.

### Regra de portabilidade
Nada específico de provider (SQLite/SQL Server/Postgres) fora da Infrastructure. Trocar de
banco = trocar pacote do provider + connection string + regerar migrações; Domain e
Application intactos.

## 8. API

### `LivrosController` — `[ApiController]`, rota `api/livros`
```
GET /api/livros        -> 200 LivroListItemDto[]  (publicados, ordenados por Ordem)
GET /api/livros/{slug} -> 200 LivroDetailDto
                          404 ProblemDetails       (slug inexistente ou despublicado)
```
- Injeta `ILivroQueryService`; tudo `async` + `CancellationToken`.
- Só valida formato/contrato e delega — zero regra de negócio.
- Sem `[Authorize]` no M1: `// público: catálogo de conteúdo` (comentário justificando, por regra do CLAUDE.md).
- Sem versionamento de API (não replicar o padrão V1/V2 do ICM sem necessidade).

### `Program.cs` (pipeline)
1. `AddApplication()` + `AddInfrastructure(config)`
2. Controllers + `AddProblemDetails()`
3. Swagger — só em `Development`
4. **CORS** — política nomeada, origens de `Cors:AllowedOrigins` (dev: `http://localhost:4200`). **Nunca `AllowAnyOrigin`**; nunca `AllowAnyOrigin` + `AllowCredentials`.
5. **Rate limiting** — `AddRateLimiter`, janela deslizante global (ex.: 100 req / 1 min / IP), resposta 429.
6. Health check — `GET /health` + `AddDbContextCheck<AppDbContext>()`
7. `SecurityHeadersMiddleware` — `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, CSP básico
8. `ExceptionHandlingMiddleware` no topo
9. Startup (dentro de `IServiceScope`): `db.Database.Migrate()` + `LivroSeeder.SeedAsync()`
10. `MapControllers()`

### `ExceptionHandlingMiddleware`
| Exceção | HTTP |
|---|---|
| `DomainException` | 400 |
| `KeyNotFoundException` | 404 |
| outra | 500 — **logada via `ILogger` antes** de responder |

`Development` inclui `detail`/stack; `Production` → mensagem genérica no 500 (ponto de atenção #10).

### Configuração
`appsettings.json` (commitado, sem segredos):
```json
{
  "ConnectionStrings": { "Default": "Data Source=/data/biblia.db" },
  "Cors": { "AllowedOrigins": ["http://localhost:4200"] },
  "RateLimiting": { "PermitLimit": 100, "WindowSeconds": 60 }
}
```
Overrides locais em `appsettings.Development.json` (git-ignored) ou env vars
(`ConnectionStrings__Default`, etc.) para Docker/deploy.

### `Dockerfile` (`src/BibliaTeologica.Api/`)
Multi-stage: `sdk:8.0` (COPY `Directory.Packages.props` + `Directory.Build.props` antes do
`restore` → build → publish) → `aspnet:8.0`. Porta 8080. `/data` = volume do `biblia.db`.
`.dockerignore` ao lado (`bin/`, `obj/`, `.git`, `tests/`, `frontend/`, `*.db`).

### Testes (`BibliaTeologica.Api.Tests` — `WebApplicationFactory<Program>` + SQLite temp)
- `GET_ApiLivros_Retorna200ComListaOrdenada`
- `GET_ApiLivrosSlug_Retorna200_QuandoExiste`
- `GET_ApiLivrosSlug_Retorna404_QuandoNaoExiste`
- `GET_ApiLivrosSlug_Retorna404_QuandoDespublicado`
- `GET_Health_Retorna200`
- `GET_ApiLivros_Retorna429_QuandoExcedeLimite`
- `Resposta_ContemSecurityHeaders`

## 9. Frontend (Angular 22)

`ng new` standalone + signals + SCSS, **sem SSR**, roteamento habilitado.

### `core/`
```ts
interface LivroListItem { slug; nome; ordem; secao; secaoRotulo }
interface LivroDetail extends LivroListItem {
  testamento; quem; porque; paraQuem; estrutura; tema; ideias: string[]; versiculo }

// livro.service.ts — HttpClient -> environment.apiUrl ('/api')
listar(): Observable<LivroListItem[]>
obterPorSlug(slug: string): Observable<LivroDetail>
```

### `features/biblia/` (lazy — `biblia.routes.ts`)
| Rota | Componente | Papel (protótipo) |
|---|---|---|
| `/biblia` | `BibliaListaComponent` | sidebar + busca; agrupa por seção via `computed()`; abre a 1ª ficha |
| `/biblia/:slug` | `BibliaFichaComponent` | o `renderCard()` — campos, ideias, versículo, nav ‹ anterior / próximo › |

- `LivroStore` (serviço `providedIn: 'root'`) mantém a lista num `signal`, carregada uma vez
  e reutilizada pelas duas telas.
- Lista: busca filtrada client-side (`computed`), navegação por `routerLink`.
- Ficha resolve `slug` da rota e busca o detalhe; prev/próximo calculados a partir de `ordem`
  sobre a lista do `LivroStore`.
- `slug` inexistente → redirect para `/biblia` com aviso.
- Raiz `redirectTo: 'biblia'` no M1.

### `shared/components/`
- `badge-secao` — pill com `secaoRotulo` (estilo `.card-section` do protótipo)
- `campo-ficha` — `<label> + <valor>` (o `.field`), reutilizado em todos os campos

### `styles/`
Porta o CSS do protótipo para SCSS: `_variables.scss` (paleta pergaminho, fontes
Fraunces / Source Serif 4 / IBM Plex Mono), `_mixins.scss` (breakpoint 900px, foco visível).
Estilos por componente; `takeUntilDestroyed(destroyRef)` em subscrições; sem `any`.
Nenhum `innerHTML` de dado bruto (ponto de atenção #8) — no M1 o conteúdo é do seed, mas
a convenção vale desde já.

> A recriação fiel do visual (tipografia, textura, "cartão de fichário") usa a skill
> **frontend-design** na fase de implementação.

### Testes (Karma/Jasmine, mínimo)
`LivroService` (HttpTestingController) + smoke test de `BibliaListaComponent` e `BibliaFichaComponent`.

### `Dockerfile` / `proxy.conf.json`
Multi-stage: `node:24` build → `nginx` servindo `dist/` + `try_files` para SPA.
`.dockerignore` (`node_modules`, `dist`, `.angular`). Proxy dev: `/api` → `http://api:8080`.

## 10. Docker Compose

`docker-compose.yml` (raiz):
```yaml
services:
  api:
    build: ./src/BibliaTeologica.Api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Data Source=/data/biblia.db
    volumes: ["biblia-data:/data"]
    ports: ["5080:8080"]
  web:
    build: ./frontend/biblia-teologica-web
    ports: ["4200:80"]
    depends_on: [api]
volumes:
  biblia-data:
```
- `docker-compose.override.yml` (dev) — código como volume, `dotnet watch` / `ng serve`, hot reload, Swagger.
- `docker-compose.prod.yml` — exemplo: builds de produção, sem watch, `ASPNETCORE_ENVIRONMENT=Production`, connection string via env. **Não executado neste projeto** — documentação para quando houver deploy.

## 11. CI — `.github/workflows/`

Espelha o ICM, **sem job de deploy**:
- **`ci.yml`**
  - `backend`: `dotnet build BibliaTeologica.sln` + `dotnet test` (Domain, Application, Api)
  - `frontend`: `npm ci` + `npx tsc --noEmit` + `npm run build` + `npm test -- --watch=false --browsers=ChromeHeadless`
- **`claude-code-review.yml`** — review automático por PR (mantido, como no ICM)

Fluxo de contribuição (convenção, sem branch protection): Issue → branch `feature/BT-<n>-nome`
→ commits → PR → checks → merge squash. Sem push direto para `main` sem pedido explícito.

## 12. Segurança — pontos de atenção permanentes

Entram nas "Regras de ouro" do `CLAUDE.md`. Aplicam-se a todos os marcos.

| # | Ponto | Tratamento | M1 |
|---|---|---|---|
| 1 | Validação no servidor | FluentValidation + invariantes de Domain em todo endpoint de escrita | M2/M3 |
| 2 | Rotas protegidas só no front | Guards Angular = UX; `[Authorize(Roles=)]` **obrigatório** no servidor | M2 |
| 3 | Secrets no código | `appsettings.Development.json` git-ignored, env vars, user-secrets; nada sensível commitado; verificar staged antes de commitar | **Sim** |
| 4 | Autorização por utilizador | Validar dono do recurso (padrão análogo ao cross-church do ICM) | M2/M4 |
| 5 | Rate limiting | `AddRateLimiter` — janela deslizante global; 429 ao exceder | **Sim** |
| 6 | Upload sem validação | Content-type + magic bytes + tamanho máx + nome seguro + fora do webroot; nunca executar | M2 |
| 7 | Dados sensíveis na resposta | Só DTOs curados, nunca entidade; nunca hash/token/e-mail de terceiros; sem campos de auditoria desnecessários | **Sim** |
| 8 | XSS | Angular escapa por padrão; Markdown (M3) sanitizado (DOMPurify); nunca `bypassSecurityTrust*` em conteúdo de utilizador; sem `innerHTML` de dado bruto | Headers no M1 |
| 9 | CORS | Política nomeada, origens explícitas de config; nunca `AllowAnyOrigin`, nunca `AllowAnyOrigin`+`AllowCredentials` | **Sim** |
| 10 | Erros vazando interno | `ProblemDetails` genérico em Production, `detail`/stack só em Development; exceções inesperadas logadas antes | **Sim** |

## 13. Sequência de implementação (para o plano)

1. `CLAUDE.md` adaptado + `docs/ROADMAP.md` + `.gitignore` + `.editorconfig`
2. Solução + projetos + `Directory.{Packages,Build}.props` (build vazio a passar)
3. Domain (`Livro`, `SecaoCanonica`, `Slug`, `DomainException`) + testes de Domain
4. Application (DTOs, `ILivroRepository`, `ILivroQueryService`, mapping, DI) + testes com repo mockado
5. Infrastructure (`AppDbContext`, config, repositório, migração inicial, `livros.json`, seeder) + DI
6. API (controller, `Program.cs`, middlewares, rate limiter, CORS, health, Swagger) + testes de integração
7. `Dockerfile` da API + `.dockerignore`
8. Angular: scaffold, models, service, feature `biblia`, componentes shared, estilos (skill frontend-design)
9. `Dockerfile` do frontend + `proxy.conf.json`
10. `docker-compose.yml` + `.override.yml` + `.prod.yml` — subir a stack completa e exercitar o fluxo
11. `.github/workflows/ci.yml` + `claude-code-review.yml`
12. Teste manual local (do utilizador) do fluxo ponta a ponta em Docker

## 14. Decisões fechadas

- **.NET 8** (único SDK instalado; igual ao ICM) — sem instalar .NET 10
- **Angular 22**, standalone + signals, **sem SSR** (CSR; prerender/SEO fica para depois)
- **SQLite** (arquivo em volume; sem container de banco)
- Application no estilo **Application Services**, não CQRS/MediatR
- **Seed automático no startup** com check de tabela vazia
- **Sem autenticação no M1** (site público read-only)
- Repo git **próprio** em `BibliaTeologica/`, isolado do repo da home
- Dev **100% em Docker** (não pesar o portátil)
- Deploy: **tudo pronto, sem deploy** neste projeto ainda

## 15. Questões em aberto

Nenhuma bloqueante. A revisitar no M3: extração de base `Conteudo` comum (regra de três).
