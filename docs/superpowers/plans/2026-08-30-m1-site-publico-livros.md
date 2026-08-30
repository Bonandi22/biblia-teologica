# M1 — Site público de Livros — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publicar o fichário dos 66 livros da Bíblia como um sistema em camadas (.NET 8 Clean Architecture + EF Core/SQLite) com API pública read-only e SPA Angular 22, tudo rodando em Docker, pronto para deploy.

**Architecture:** Monólito .NET 8 em 4 camadas (Domain → Application → Infrastructure → Api), camada Application no estilo *Application Services* (sem MediatR). Persistência EF Core + SQLite (arquivo em volume). Frontend Angular 22 standalone/signals (CSR, sem SSR) consumindo a API por HTTP. Dev e build 100% em contêineres.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core 8 (Sqlite), AutoMapper 13, xUnit + Moq + AwesomeAssertions, Angular 22, RxJS, Karma/Jasmine, Docker Compose, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-08-30-m1-site-publico-livros-design.md`

## Global Constraints

- **.NET 8** apenas (`net8.0`). Não usar APIs de .NET 9/10. SDK único instalado: `8.0.130`.
- **Angular 22**, standalone components, signals, `--ssr=false`, SCSS.
- **SQLite** como único provider de banco. Nada específico de provider fora de `BibliaTeologica.Infrastructure`.
- **Central Package Management**: versões de pacote só em `Directory.Packages.props`; `.csproj` tem `<PackageReference Include="..." />` **sem** `Version`.
- **`DateTime.UtcNow`**, nunca `DateTime.Now`, em Domain/Application.
- **Async ponta a ponta** com `CancellationToken` propagado (controllers → services → repositórios).
- **Nunca engolir `Exception`** — logar via `ILogger` antes de tratar/relançar.
- **EF Core só-leitura usa `AsNoTracking()`**.
- **DTOs são o contrato da API** — nunca expor entidades; nunca retornar campos de auditoria desnecessários.
- **CORS**: política nomeada com origens explícitas de config; nunca `AllowAnyOrigin`.
- **Segredos**: nada sensível em arquivos commitados; `appsettings.Development.json` é git-ignored.
- **Testes** seguem `Metodo_ResultadoEsperado_QuandoCondicao`. xUnit + Moq + AwesomeAssertions (namespace `AwesomeAssertions`, API `.Should()`).
- **Subscrições RxJS** em componentes/serviços novos usam `takeUntilDestroyed(this.destroyRef)` (`private destroyRef = inject(DestroyRef)`).
- **Commits**: mensagem em português, Conventional Commits. Cada mensagem de commit termina com as duas linhas de trailer definidas no `CLAUDE.md` (`Co-Authored-By:` e `Claude-Session:`).
- **Git**: repo próprio já inicializado em `BibliaTeologica/`. Branch `main` é o alvo. Fluxo Issue → branch `feature/BT-<n>-nome` → PR. Não fazer push direto sem pedido.
- **Frontend build output**: `dist/biblia-teologica-web/browser`.

---

## Mapa de arquivos

**Domain** (`src/BibliaTeologica.Domain/`)
- `Slugs/Slug.cs` — classe estática `Slug.From(string)`.
- `Enums/SecaoCanonica.cs` — enum + `Testamento` enum + `SecaoCanonicaExtensions`.
- `Exceptions/DomainException.cs` — exceção de invariante.
- `Entities/Livro.cs` — agregado, factory `Create`, sem setters públicos.

**Application** (`src/BibliaTeologica.Application/`)
- `Common/Interfaces/ILivroRepository.cs`
- `Common/Mapping/LivroMappingProfile.cs`
- `Livros/Dtos/LivroListItemDto.cs`, `Livros/Dtos/LivroDetailDto.cs`
- `Livros/ILivroQueryService.cs`, `Livros/LivroQueryService.cs`
- `DependencyInjection.cs` — `AddApplication()`

**Infrastructure** (`src/BibliaTeologica.Infrastructure/`)
- `Persistence/AppDbContext.cs`
- `Persistence/Configurations/LivroConfiguration.cs`
- `Persistence/Migrations/*` — `InitialCreate`
- `Persistence/Seed/livros.json`, `Persistence/Seed/LivroSeeder.cs`
- `Repositories/LivroRepository.cs`
- `DependencyInjection.cs` — `AddInfrastructure(IConfiguration)`

**Api** (`src/BibliaTeologica.Api/`)
- `Program.cs`, `appsettings.json`
- `Middleware/ExceptionHandlingMiddleware.cs`, `Middleware/SecurityHeadersMiddleware.cs`
- `Controllers/LivrosController.cs`
- `Dockerfile`

**Tests**
- `tests/BibliaTeologica.Domain.Tests/` — Slug, SecaoCanonica, Livro
- `tests/BibliaTeologica.Application.Tests/` — LivroQueryService (repo mockado), mapping
- `tests/BibliaTeologica.Infrastructure.Tests/` — LivroRepository + LivroSeeder (SQLite real)
- `tests/BibliaTeologica.Api.Tests/` — endpoints via `WebApplicationFactory`

**Frontend** (`frontend/biblia-teologica-web/`)
- `src/environments/environment.ts`, `environment.prod.ts`
- `src/app/core/models/livro.model.ts`
- `src/app/core/services/livro.service.ts`, `livro.store.ts`
- `src/app/shared/components/badge-secao/`, `campo-ficha/`
- `src/app/features/biblia/biblia.routes.ts`, `biblia-lista/`, `biblia-ficha/`
- `src/app/app.routes.ts`, `app.config.ts`
- `src/styles/_variables.scss`, `_mixins.scss`, `styles.scss`
- `proxy.conf.json`, `nginx.conf`, `Dockerfile`

**Raiz**
- `CLAUDE.md`, `docs/ROADMAP.md`, `.editorconfig`, `.dockerignore`
- `BibliaTeologica.sln`, `Directory.Packages.props`, `Directory.Build.props`
- `docker-compose.yml`, `docker-compose.override.yml`, `docker-compose.prod.yml`
- `.github/workflows/ci.yml`, `.github/workflows/claude-code-review.yml`

---

## Task 1: Documentos-base do repositório

**Files:**
- Create: `CLAUDE.md`
- Create: `docs/ROADMAP.md`
- Create: `.editorconfig`
- Create: `docs/prototipo-ficha-66-livros.html` (arquivo do protótipo, referência)

**Interfaces:**
- Consumes: nada.
- Produces: `docs/prototipo-ficha-66-livros.html` contém o array JS `const BOOKS = [...]` com os 66 registros (chaves: `section, nome, quem, porque, praquem, estrutura, tema, ideias, versiculo`) — a Task 8 lê esse array para gerar `livros.json`.

- [ ] **Step 1: Salvar o protótipo**

Salvar o HTML do protótipo (fornecido pelo autor no brainstorming — "Fichário — Os 66 Livros") **verbatim** em `docs/prototipo-ficha-66-livros.html`. Não editar. Confirmar que contém `const BOOKS = [` e os 66 objetos.

- [ ] **Step 2: Criar `.editorconfig`**

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space

[*.{cs,csx}]
indent_size = 4
csharp_style_namespace_declarations = file_scoped:warning

[*.{ts,js,json,html,scss,css,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

- [ ] **Step 3: Criar `docs/ROADMAP.md`**

```markdown
# Roadmap — Bíblia Teológica

Portal de conhecimento teológico. Cada vertical repete o mesmo padrão:
entidade (Domain) -> IXQueryService (Application) -> repositório (Infra) ->
controller (Api) -> feature lazy-loaded (Angular).

## Verticais previstas

| Vertical | Conteúdo | Nota |
|---|---|---|
| Livros | Ficha estruturada (campos curtos) | M1 |
| Estudos | Artigo long-form em Markdown | M3 — vira o template das próximas |
| Vídeos | Embed YouTube/Vimeo + metadados | M3.5 — entidade sem corpo |
| Teologia Sistemática | Séries encadeadas de Estudos (SerieId + Ordem) | M3 (decidir) |
| Doutrinas / Glossário / Cronologia | Entradas estruturadas | M5+ |

## Marcos

- M1 — Site público de Livros. Camadas .NET, EF Core/SQLite, Angular 22, Docker, CI.
  Spec: docs/superpowers/specs/2026-08-30-m1-site-publico-livros-design.md
- M2 — ASP.NET Core Identity + JWT + área /admin (role Admin) + CRUD de Livros + upload.
- M3 — Estudos (público + admin), corpo em Markdown, sanitização (DOMPurify).
- M3.5 — Vídeos.
- M4 — Cadastro público + confirmação de e-mail + captura de newsletter.
- M5+ — Doutrinas, Glossário, Cronologia, Teologia Sistemática, e-mail marketing, SSR/prerender, deploy real.

## Regra de três

No M3, ao surgir o 2º tipo long-form, avaliar extrair base comum Conteudo
(slug, título, publicado, SEO, timestamps, categoria, tags).
```

- [ ] **Step 4: Criar `CLAUDE.md`** — conteúdo completo abaixo (bloco markdown a gravar verbatim como `CLAUDE.md`):

```markdown
# CLAUDE.md

Guia para o Claude Code neste repositório.

## Project context

Bíblia Teológica — portal de conhecimento teológico (livros da Bíblia, estudos
long-form, vídeos, teologia sistemática). Projeto híbrido: peça de portfólio de
engenharia .NET e site pessoal leve de manter. Primeira vertical entregue: o
fichário dos 66 livros do cânon protestante.

Roadmap: docs/ROADMAP.md. Specs e planos: docs/superpowers/.

## Guardrails (regras permanentes)

Aplicam-se a qualquer tarefa. Só o utilizador as suspende, explicitamente, para uma tarefa concreta.

Qualidade — nenhuma mudança está pronta sem testes a passar e sem o utilizador ter revisto o diff. Antes de declarar terminado: apresentar diff (ou resumo) e riscos. Para mudança com comportamento em runtime, o teste manual local (subir a stack em Docker e exercitar o fluxo) é do utilizador e faz parte do "pronto para merge".

Segurança — nunca escrever segredos em código, commits, migrações, logs ou output. Sensível vive em env vars ou appsettings.Development.json (git-ignored). Verificar staged antes de commitar.

Custo — no fim de tarefas com loops/subagents/múltiplas iterações, estimar o esforço gasto e sugerir onde reduzir.

## Arquitetura

1 monólito .NET 8 em Clean Architecture de 4 camadas: Domain (entidades, enums, exceções de domínio) -> Application (Application Services, DTOs, perfis AutoMapper) -> Infrastructure (DbContext EF Core, repositórios, migrações, seed) -> Api (controllers, Program.cs, middleware). Sem microserviços, sem mensageria.

Persistência: EF Core + SQLite (arquivo em volume Docker). Nada específico de provider fora da Infrastructure.

Frontend: Angular 22 standalone + signals, CSR (sem SSR). Features lazy-loaded em features/. URL da API em environment.ts (/api).

## Princípios de Engenharia

- Domain/Application, não Infra/Controllers, é onde vive a regra de negócio. Entidades protegem os próprios invariantes (sem setters públicos soltos); Application Services orquestram casos de uso; repositórios só persistem/recuperam; controllers só validam formato/contrato e delegam.
- DTOs são o contrato da API, nunca o modelo de domínio.
- AutoMapper mapeia, não decide.
- SRP por classe; depender de abstrações; composição em vez de herança sem relação de domínio clara.

## Convenções de código

- DateTime.UtcNow, nunca DateTime.Now, em domínio/aplicação.
- Async de ponta a ponta — nunca .Result/.Wait(); CancellationToken propagado.
- Nunca engolir Exception — logar via ILogger antes de tratar/relançar.
- EF Core: só-leitura usa AsNoTracking(); evitar N+1 e Include desnecessário.
- DTO de escrita nunca é mapeado direto para a entidade quando há campos de auditoria/propriedade.
- Central Package Management — versões só em Directory.Packages.props; .csproj sem Version.
- Testes: Metodo_ResultadoEsperado_QuandoCondicao. xUnit + Moq + AwesomeAssertions + EF Core Sqlite.
- Subscrições RxJS em código novo: takeUntilDestroyed(this.destroyRef).
- Nenhum innerHTML de dado bruto; nunca bypassSecurityTrust* em conteúdo de utilizador.

## Comandos essenciais

Backend:

    dotnet build BibliaTeologica.sln
    dotnet test BibliaTeologica.sln
    dotnet ef migrations add <Nome> --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
    dotnet ef database update --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api

Frontend:

    cd frontend/biblia-teologica-web
    npm ci
    npm start
    npm run build
    npm test -- --watch=false --browsers=ChromeHeadless
    npx tsc --noEmit -p tsconfig.app.json

Docker:

    docker compose up --build      # api :5080 + web :4200
    docker compose down -v         # zera o volume do SQLite

Swagger em dev: http://localhost:5080/swagger

## Git

main corre CI (ci.yml) e code review automático (claude-code-review.yml) em cada PR. Sem branch protection — Issue -> branch feature/BT-<n>-nome -> PR -> checks -> merge squash é convenção. Só push direto para main com pedido explícito; nunca merge com checks vermelhos.

Toda mensagem de commit termina com:

    Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
    Claude-Session: <url da sessão>

## Regras de ouro — nunca fazer

- Nunca adicionar endpoint de escrita sem [Authorize(Roles=...)] (ou comentário justificando por que é público). Guards do Angular são só UX.
- Nunca _mapper.Map(dto, entity) num Update sem confirmar que campos de auditoria/propriedade estão ignorados.
- Nunca retornar entidade crua nem campos sensíveis (hash, token, e-mail de terceiros) numa resposta de API.
- Nunca AllowAnyOrigin no CORS; nunca AllowAnyOrigin + AllowCredentials.
- Nunca deixar mensagem de erro vazar stack/interno em Production.
- Nunca upload sem validar content-type + magic bytes + tamanho + nome seguro + destino fora do webroot.
- Nunca commitar appsettings.Development.json, .env, nem segredo em arquivo versionado.
- Nunca escrever endpoint/service novo sem teste cobrindo caminho feliz + rejeição.
- Nunca git push direto para main sem pedido explícito.
- Nunca declarar tarefa concluída sem testes a passar e sem o utilizador ter revisto o diff.
```

- [ ] **Step 5: Commit**

```bash
git add CLAUDE.md docs/ROADMAP.md .editorconfig docs/prototipo-ficha-66-livros.html
git commit -m "docs: CLAUDE.md, roadmap, editorconfig e arquivo do protótipo"
```

---

## Task 2: Solução .NET e projetos

**Files:**
- Create: `BibliaTeologica.sln`, `Directory.Packages.props`, `Directory.Build.props`
- Create: `src/BibliaTeologica.{Domain,Application,Infrastructure,Api}/*.csproj`
- Create: `tests/BibliaTeologica.{Domain,Application,Infrastructure,Api}.Tests/*.csproj`

**Interfaces:**
- Consumes: nada.
- Produces: solução compilável. Referências: Application→Domain; Infrastructure→Application; Api→Application+Infrastructure. Api expõe `public partial class Program` para `WebApplicationFactory`.

- [ ] **Step 1: Criar solução e projetos**

```bash
dotnet new sln -n BibliaTeologica
dotnet new classlib -n BibliaTeologica.Domain -o src/BibliaTeologica.Domain
dotnet new classlib -n BibliaTeologica.Application -o src/BibliaTeologica.Application
dotnet new classlib -n BibliaTeologica.Infrastructure -o src/BibliaTeologica.Infrastructure
dotnet new webapi -n BibliaTeologica.Api -o src/BibliaTeologica.Api --use-controllers
dotnet new xunit -n BibliaTeologica.Domain.Tests -o tests/BibliaTeologica.Domain.Tests
dotnet new xunit -n BibliaTeologica.Application.Tests -o tests/BibliaTeologica.Application.Tests
dotnet new xunit -n BibliaTeologica.Infrastructure.Tests -o tests/BibliaTeologica.Infrastructure.Tests
dotnet new xunit -n BibliaTeologica.Api.Tests -o tests/BibliaTeologica.Api.Tests
dotnet sln add $(find src tests -name "*.csproj")
find src tests -name "Class1.cs" -delete
find tests -name "UnitTest1.cs" -delete
```

- [ ] **Step 2: Referências**

```bash
dotnet add src/BibliaTeologica.Application reference src/BibliaTeologica.Domain
dotnet add src/BibliaTeologica.Infrastructure reference src/BibliaTeologica.Application
dotnet add src/BibliaTeologica.Api reference src/BibliaTeologica.Application src/BibliaTeologica.Infrastructure
dotnet add tests/BibliaTeologica.Domain.Tests reference src/BibliaTeologica.Domain
dotnet add tests/BibliaTeologica.Application.Tests reference src/BibliaTeologica.Application
dotnet add tests/BibliaTeologica.Infrastructure.Tests reference src/BibliaTeologica.Infrastructure
dotnet add tests/BibliaTeologica.Api.Tests reference src/BibliaTeologica.Api
```

- [ ] **Step 3: `Directory.Build.props`**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

Depois remover `<TargetFramework>` duplicado dos `.csproj` gerados (fica só no props).

- [ ] **Step 4: `Directory.Packages.props`**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="AutoMapper" Version="13.0.1" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.10" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10" />
    <PackageVersion Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.10" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.9.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.10" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
    <PackageVersion Include="Moq" Version="4.20.72" />
    <PackageVersion Include="AwesomeAssertions" Version="7.0.0" />
  </ItemGroup>
</Project>
```

Remover todos os `Version=` dos `<PackageReference>` nos `.csproj` de teste.

- [ ] **Step 5: `Program.cs` mínimo em `BibliaTeologica.Api`**

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();

public partial class Program;
```

Apagar `WeatherForecast*` e controller de exemplo.

- [ ] **Step 6: Build**

Run: `dotnet build BibliaTeologica.sln`
Expected: `Build succeeded`, 0 warning(s), 0 error(s).

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "chore: estrutura da solução .NET 8 em 4 camadas + projetos de teste"
```

---

## Task 3: Domain — `Slug`

**Files:**
- Create: `src/BibliaTeologica.Domain/Slugs/Slug.cs`
- Test: `tests/BibliaTeologica.Domain.Tests/Slugs/SlugTests.cs`

**Interfaces:**
- Consumes: nada.
- Produces: `static partial class BibliaTeologica.Domain.Slugs.Slug` com `static string From(string texto)`. Lança `ArgumentException` se nulo/branco.

- [ ] **Step 1: Teste que falha**

```csharp
using AwesomeAssertions;
using BibliaTeologica.Domain.Slugs;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Slugs;

public class SlugTests
{
    [Theory]
    [InlineData("GÊNESIS", "genesis")]
    [InlineData("ÊXODO", "exodo")]
    [InlineData("1 SAMUEL", "1-samuel")]
    [InlineData("CANTARES DE SALOMÃO", "cantares-de-salomao")]
    [InlineData("  Provérbios  ", "proverbios")]
    [InlineData("ATOS DOS APÓSTOLOS", "atos-dos-apostolos")]
    public void From_GeraSlugMinusculoSemAcento_QuandoNomeValido(string entrada, string esperado)
        => Slug.From(entrada).Should().Be(esperado);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_LancaArgumentException_QuandoTextoVazio(string? entrada)
    {
        var acao = () => Slug.From(entrada!);
        acao.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~SlugTests"`
Expected: FAIL — `Slug` não existe.

- [ ] **Step 3: Implementar**

```csharp
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BibliaTeologica.Domain.Slugs;

public static partial class Slug
{
    public static string From(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("Texto para slug não pode ser vazio.", nameof(texto));

        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);

        foreach (var ch in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        return HifensRepetidos().Replace(sb.ToString(), "-").Trim('-');
    }

    [GeneratedRegex("-{2,}")]
    private static partial Regex HifensRepetidos();
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~SlugTests"`
Expected: PASS (9 casos).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Domain/Slugs tests/BibliaTeologica.Domain.Tests/Slugs
git commit -m "feat(domain): helper Slug.From com remoção de acentos"
```

---

## Task 4: Domain — `SecaoCanonica`, `Testamento` e extensões

**Files:**
- Create: `src/BibliaTeologica.Domain/Enums/SecaoCanonica.cs`
- Create: `src/BibliaTeologica.Domain/Enums/Testamento.cs`
- Create: `src/BibliaTeologica.Domain/Enums/SecaoCanonicaExtensions.cs`
- Test: `tests/BibliaTeologica.Domain.Tests/Enums/SecaoCanonicaExtensionsTests.cs`

**Interfaces:**
- Consumes: `DomainException` (Task 5) — para evitar dependência circular na ordem, a `FromRotulo` lança `ArgumentException` **nesta task**; a Task 5 NÃO altera isso. (Decisão: `FromRotulo` usa `ArgumentException`.)
- Produces:
  - `enum SecaoCanonica { Pentateuco=1, LivrosHistoricos=2, LivrosPoeticos=3, ProfetasMaiores=4, ProfetasMenores=5, Evangelhos=6, Atos=7, CartasPaulinas=8, CartasGerais=9, Apocalipse=10 }`
  - `enum Testamento { Antigo, Novo }`
  - `static class SecaoCanonicaExtensions`: `string Rotulo(this SecaoCanonica)`, `Testamento Testamento(this SecaoCanonica)`, `static SecaoCanonica FromRotulo(string rotulo)`.

- [ ] **Step 1: Teste que falha**

```csharp
using AwesomeAssertions;
using BibliaTeologica.Domain.Enums;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Enums;

public class SecaoCanonicaExtensionsTests
{
    [Theory]
    [InlineData(SecaoCanonica.Pentateuco, "PENTATEUCO")]
    [InlineData(SecaoCanonica.LivrosHistoricos, "LIVROS HISTÓRICOS")]
    [InlineData(SecaoCanonica.CartasPaulinas, "CARTAS PAULINAS")]
    [InlineData(SecaoCanonica.Apocalipse, "APOCALIPSE")]
    public void Rotulo_RetornaTextoDeExibicao(SecaoCanonica secao, string esperado)
        => secao.Rotulo().Should().Be(esperado);

    [Theory]
    [InlineData(SecaoCanonica.Pentateuco, Testamento.Antigo)]
    [InlineData(SecaoCanonica.ProfetasMenores, Testamento.Antigo)]
    [InlineData(SecaoCanonica.Evangelhos, Testamento.Novo)]
    [InlineData(SecaoCanonica.Apocalipse, Testamento.Novo)]
    public void Testamento_DerivaDaSecao(SecaoCanonica secao, Testamento esperado)
        => secao.Testamento().Should().Be(esperado);

    [Theory]
    [InlineData("PENTATEUCO", SecaoCanonica.Pentateuco)]
    [InlineData("livros poéticos", SecaoCanonica.LivrosPoeticos)]
    [InlineData("PROFETAS MENORES", SecaoCanonica.ProfetasMenores)]
    public void FromRotulo_ResolveEnum_IgnorandoCase(string rotulo, SecaoCanonica esperado)
        => SecaoCanonicaExtensions.FromRotulo(rotulo).Should().Be(esperado);

    [Fact]
    public void FromRotulo_LancaArgumentException_QuandoRotuloDesconhecido()
    {
        var acao = () => SecaoCanonicaExtensions.FromRotulo("APÓCRIFOS");
        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TodasAsSecoes_TemDescription()
    {
        foreach (var s in Enum.GetValues<SecaoCanonica>())
            s.Rotulo().Should().NotBe(s.ToString());
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~SecaoCanonicaExtensionsTests"`
Expected: FAIL — tipos não existem.

- [ ] **Step 3: Implementar os enums**

`Enums/Testamento.cs`:

```csharp
namespace BibliaTeologica.Domain.Enums;

public enum Testamento
{
    Antigo,
    Novo
}
```

`Enums/SecaoCanonica.cs`:

```csharp
using System.ComponentModel;

namespace BibliaTeologica.Domain.Enums;

public enum SecaoCanonica
{
    [Description("PENTATEUCO")] Pentateuco = 1,
    [Description("LIVROS HISTÓRICOS")] LivrosHistoricos = 2,
    [Description("LIVROS POÉTICOS")] LivrosPoeticos = 3,
    [Description("PROFETAS MAIORES")] ProfetasMaiores = 4,
    [Description("PROFETAS MENORES")] ProfetasMenores = 5,
    [Description("EVANGELHOS")] Evangelhos = 6,
    [Description("ATOS")] Atos = 7,
    [Description("CARTAS PAULINAS")] CartasPaulinas = 8,
    [Description("CARTAS GERAIS")] CartasGerais = 9,
    [Description("APOCALIPSE")] Apocalipse = 10
}
```

- [ ] **Step 4: Implementar `SecaoCanonicaExtensions`**

```csharp
using System.ComponentModel;
using System.Reflection;

namespace BibliaTeologica.Domain.Enums;

public static class SecaoCanonicaExtensions
{
    public static string Rotulo(this SecaoCanonica secao)
    {
        var campo = typeof(SecaoCanonica).GetField(secao.ToString());
        return campo?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? secao.ToString();
    }

    public static Testamento Testamento(this SecaoCanonica secao)
        => secao <= SecaoCanonica.ProfetasMenores ? Enums.Testamento.Antigo : Enums.Testamento.Novo;

    public static SecaoCanonica FromRotulo(string rotulo)
    {
        if (string.IsNullOrWhiteSpace(rotulo))
            throw new ArgumentException("Rótulo de seção não pode ser vazio.", nameof(rotulo));

        foreach (var secao in Enum.GetValues<SecaoCanonica>())
            if (string.Equals(secao.Rotulo(), rotulo.Trim(), StringComparison.OrdinalIgnoreCase))
                return secao;

        throw new ArgumentException($"Seção canônica desconhecida: '{rotulo}'.", nameof(rotulo));
    }
}
```

- [ ] **Step 5: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~SecaoCanonicaExtensionsTests"`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Domain/Enums tests/BibliaTeologica.Domain.Tests/Enums
git commit -m "feat(domain): enums SecaoCanonica/Testamento e extensões de rótulo"
```

---

## Task 5: Domain — `DomainException` e entidade `Livro`

**Files:**
- Create: `src/BibliaTeologica.Domain/Exceptions/DomainException.cs`
- Create: `src/BibliaTeologica.Domain/Entities/Livro.cs`
- Test: `tests/BibliaTeologica.Domain.Tests/Entities/LivroTests.cs`

**Interfaces:**
- Consumes: `Slug.From` (Task 3); `SecaoCanonica`, `Testamento`, `SecaoCanonicaExtensions.Testamento` (Task 4).
- Produces:
  - `class DomainException : Exception` com ctor `DomainException(string message)`.
  - `class Livro` com ctor `private Livro()` (EF) e factory:
    `static Livro Create(string nome, SecaoCanonica secao, int ordem, string quem, string porque, string paraQuem, string estrutura, string tema, IEnumerable<string> ideias, string versiculo)`
  - Props somente-get: `int Id`, `string Slug`, `string Nome`, `SecaoCanonica Secao`, `int Ordem`, `string Quem`, `string Porque`, `string ParaQuem`, `string Estrutura`, `string Tema`, `IReadOnlyList<string> Ideias`, `string Versiculo`, `bool Publicado`, `DateTime CriadoEm`, `DateTime AtualizadoEm`, `Testamento Testamento` (derivada).
  - Regras: `nome/quem/porque/paraQuem/estrutura/tema/versiculo` obrigatórios (não branco); `secao` definido; `ordem` 1..66; `ideias` ≥ 1 item não-branco. Violação → `DomainException`.
  - Métodos de mutação NÃO existem no M1.

- [ ] **Step 1: Teste que falha**

```csharp
using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;
using Xunit;

namespace BibliaTeologica.Domain.Tests.Entities;

public class LivroTests
{
    private static Livro LivroValido(string nome = "GÊNESIS", int ordem = 1)
        => Livro.Create(nome, SecaoCanonica.Pentateuco, ordem,
            quem: "Atribuído a Moisés", porque: "Dar origem a Israel",
            paraQuem: "Israel no deserto", estrutura: "História primitiva e patriarcal",
            tema: "Deus criador e aliança", ideias: new[] { "Criação", "Queda", "Aliança com Abraão" },
            versiculo: "Gênesis 1:1");

    [Fact]
    public void Create_PreencheCamposEGeraSlug_QuandoDadosValidos()
    {
        var livro = LivroValido();

        livro.Slug.Should().Be("genesis");
        livro.Nome.Should().Be("GÊNESIS");
        livro.Secao.Should().Be(SecaoCanonica.Pentateuco);
        livro.Ordem.Should().Be(1);
        livro.Ideias.Should().HaveCount(3);
        livro.Publicado.Should().BeTrue();
        livro.Testamento.Should().Be(Testamento.Antigo);
        livro.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        livro.AtualizadoEm.Should().Be(livro.CriadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_LancaDomainException_QuandoNomeVazio(string nome)
    {
        var acao = () => LivroValido(nome: nome);
        acao.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(67)]
    [InlineData(-1)]
    public void Create_LancaDomainException_QuandoOrdemForaDoIntervalo(int ordem)
    {
        var acao = () => LivroValido(ordem: ordem);
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_LancaDomainException_QuandoIdeiasVazia()
    {
        var acao = () => Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "Confrontar retribuição", "Israel", "Prólogo, diálogos, epílogo",
            "Justiça de Deus", ideias: Array.Empty<string>(), versiculo: "Jó 42:5");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_LancaDomainException_QuandoIdeiasSoTemBranco()
    {
        var acao = () => Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "x", "Israel", "y", "z", ideias: new[] { "  ", "" }, versiculo: "Jó 42:5");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_IgnoraIdeiasEmBranco_MasMantemAsValidas()
    {
        var livro = Livro.Create("JÓ", SecaoCanonica.LivrosPoeticos, 18,
            "Anônimo", "x", "Israel", "y", "z", ideias: new[] { "Aposta celestial", "  ", "Restauração" },
            versiculo: "Jó 42:5");
        livro.Ideias.Should().Equal("Aposta celestial", "Restauração");
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~LivroTests"`
Expected: FAIL — `Livro`/`DomainException` não existem.

- [ ] **Step 3: Implementar `DomainException`**

```csharp
namespace BibliaTeologica.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

- [ ] **Step 4: Implementar `Livro`**

```csharp
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;

namespace BibliaTeologica.Domain.Entities;

public class Livro
{
    private readonly List<string> _ideias = new();

    private Livro() { }

    private Livro(string nome, SecaoCanonica secao, int ordem, string quem, string porque,
        string paraQuem, string estrutura, string tema, IEnumerable<string> ideias, string versiculo)
    {
        Nome = nome;
        Slug = Slugs.Slug.From(nome);
        Secao = secao;
        Ordem = ordem;
        Quem = quem;
        Porque = porque;
        ParaQuem = paraQuem;
        Estrutura = estrutura;
        Tema = tema;
        _ideias.AddRange(ideias);
        Versiculo = versiculo;
        Publicado = true;
        CriadoEm = AtualizadoEm = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public SecaoCanonica Secao { get; private set; }
    public int Ordem { get; private set; }
    public string Quem { get; private set; } = null!;
    public string Porque { get; private set; } = null!;
    public string ParaQuem { get; private set; } = null!;
    public string Estrutura { get; private set; } = null!;
    public string Tema { get; private set; } = null!;
    public IReadOnlyList<string> Ideias => _ideias.AsReadOnly();
    public string Versiculo { get; private set; } = null!;
    public bool Publicado { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public Testamento Testamento => Secao.Testamento();

    public static Livro Create(string nome, SecaoCanonica secao, int ordem, string quem,
        string porque, string paraQuem, string estrutura, string tema,
        IEnumerable<string> ideias, string versiculo)
    {
        var ideiasValidas = (ideias ?? Enumerable.Empty<string>())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToList();

        Requer(nome, nameof(nome));
        if (!Enum.IsDefined(secao)) throw new DomainException("Seção canônica inválida.");
        if (ordem is < 1 or > 66) throw new DomainException("Ordem deve estar entre 1 e 66.");
        Requer(quem, nameof(quem));
        Requer(porque, nameof(porque));
        Requer(paraQuem, nameof(paraQuem));
        Requer(estrutura, nameof(estrutura));
        Requer(tema, nameof(tema));
        Requer(versiculo, nameof(versiculo));
        if (ideiasValidas.Count == 0)
            throw new DomainException("Livro precisa de ao menos uma ideia-chave.");

        return new Livro(nome, secao, ordem, quem, porque, paraQuem, estrutura, tema,
            ideiasValidas, versiculo);

        static void Requer(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new DomainException($"Campo obrigatório ausente: {campo}.");
        }
    }
}
```

- [ ] **Step 5: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests`
Expected: PASS (todos os testes de Domain).

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Domain/Exceptions src/BibliaTeologica.Domain/Entities tests/BibliaTeologica.Domain.Tests/Entities
git commit -m "feat(domain): DomainException e entidade Livro com invariantes"
```

---

## Task 6: Application — DTOs, interfaces, mapping, `LivroQueryService`, DI

**Files:**
- Create: `src/BibliaTeologica.Application/Livros/Dtos/LivroListItemDto.cs`
- Create: `src/BibliaTeologica.Application/Livros/Dtos/LivroDetailDto.cs`
- Create: `src/BibliaTeologica.Application/Common/Interfaces/ILivroRepository.cs`
- Create: `src/BibliaTeologica.Application/Common/Mapping/LivroMappingProfile.cs`
- Create: `src/BibliaTeologica.Application/Livros/ILivroQueryService.cs`
- Create: `src/BibliaTeologica.Application/Livros/LivroQueryService.cs`
- Create: `src/BibliaTeologica.Application/DependencyInjection.cs`
- Modify: `src/BibliaTeologica.Application/BibliaTeologica.Application.csproj` (add `AutoMapper`, `Microsoft.Extensions.DependencyInjection.Abstractions`)
- Test: `tests/BibliaTeologica.Application.Tests/Livros/LivroQueryServiceTests.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Common/LivroMappingProfileTests.cs`

**Interfaces:**
- Consumes: `Livro`, `SecaoCanonica`, `SecaoCanonicaExtensions.Rotulo`, `Testamento` (Domain).
- Produces:
  - `record LivroListItemDto(string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo)`
  - `record LivroDetailDto(string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo, string Testamento, string Quem, string Porque, string ParaQuem, string Estrutura, string Tema, IReadOnlyList<string> Ideias, string Versiculo)`
  - `interface ILivroRepository { Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct); Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct); }`
  - `interface ILivroQueryService { Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct); Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct); }`
  - `class LivroQueryService : ILivroQueryService` (ctor: `ILivroRepository`, `IMapper`)
  - `static IServiceCollection AddApplication(this IServiceCollection)`
  - `class LivroMappingProfile : AutoMapper.Profile`
- **Contrato de comportamento:** `LivroQueryService.ObterPorSlugAsync` retorna `null` se o repo devolver `null` **ou** se `livro.Publicado == false`. `ListarAsync` apenas mapeia o que o repo devolve (o repo já filtra publicados e ordena).

- [ ] **Step 1: Adicionar pacotes ao `.csproj` da Application**

Editar `BibliaTeologica.Application.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="AutoMapper" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
</ItemGroup>
```

Adicionar `<PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />` ao `Directory.Packages.props`.

Run: `dotnet restore` → Expected: sem `NU1010`.

- [ ] **Step 2: Escrever os testes que falham**

`tests/BibliaTeologica.Application.Tests/Livros/LivroQueryServiceTests.cs`:

```csharp
using AwesomeAssertions;
using AutoMapper;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Application.Livros;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Livros;

public class LivroQueryServiceTests
{
    private readonly Mock<ILivroRepository> _repo = new();
    private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<LivroMappingProfile>()).CreateMapper();

    private LivroQueryService CriarServico() => new(_repo.Object, _mapper);

    private static Livro Livro(string nome, int ordem, SecaoCanonica secao = SecaoCanonica.Pentateuco)
        => BibliaTeologica.Domain.Entities.Livro.Create(nome, secao, ordem,
            "quem", "porque", "paraQuem", "estrutura", "tema", new[] { "ideia" }, "versiculo");

    [Fact]
    public async Task ListarAsync_MapeiaEPreservaOrdemDoRepositorio()
    {
        _repo.Setup(r => r.ListarPublicadosAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[] { Livro("GÊNESIS", 1), Livro("ÊXODO", 2) });

        var resultado = await CriarServico().ListarAsync(CancellationToken.None);

        resultado.Select(l => l.Slug).Should().Equal("genesis", "exodo");
        resultado[0].SecaoRotulo.Should().Be("PENTATEUCO");
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaDetalhe_QuandoLivroPublicado()
    {
        _repo.Setup(r => r.ObterPorSlugAsync("genesis", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Livro("GÊNESIS", 1));

        var dto = await CriarServico().ObterPorSlugAsync("genesis", CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Testamento.Should().Be("Antigo");
        dto.Secao.Should().Be("Pentateuco");
        dto.SecaoRotulo.Should().Be("PENTATEUCO");
        dto.Ideias.Should().ContainSingle();
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaNull_QuandoSlugNaoExiste()
    {
        _repo.Setup(r => r.ObterPorSlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Livro?)null);

        (await CriarServico().ObterPorSlugAsync("inexistente", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaNull_QuandoLivroDespublicado()
    {
        var livro = Livro("GÊNESIS", 1);
        typeof(Livro).GetProperty(nameof(Livro.Publicado))!
            .GetSetMethod(nonPublic: true)!.Invoke(livro, new object?[] { false });
        _repo.Setup(r => r.ObterPorSlugAsync("genesis", It.IsAny<CancellationToken>())).ReturnsAsync(livro);

        (await CriarServico().ObterPorSlugAsync("genesis", CancellationToken.None)).Should().BeNull();
    }
}
```

`tests/BibliaTeologica.Application.Tests/Common/LivroMappingProfileTests.cs`:

```csharp
using AutoMapper;
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Mapping;
using Xunit;

namespace BibliaTeologica.Application.Tests.Common;

public class LivroMappingProfileTests
{
    [Fact]
    public void Configuracao_EhValida()
    {
        var config = new MapperConfiguration(c => c.AddProfile<LivroMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
```

- [ ] **Step 3: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests`
Expected: FAIL — tipos não existem.

- [ ] **Step 4: Implementar DTOs**

`Livros/Dtos/LivroListItemDto.cs`:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroListItemDto(string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo);
```

`Livros/Dtos/LivroDetailDto.cs`:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroDetailDto(
    string Slug,
    string Nome,
    int Ordem,
    string Secao,
    string SecaoRotulo,
    string Testamento,
    string Quem,
    string Porque,
    string ParaQuem,
    string Estrutura,
    string Tema,
    IReadOnlyList<string> Ideias,
    string Versiculo);
```

- [ ] **Step 5: Implementar `ILivroRepository`**

`Common/Interfaces/ILivroRepository.cs`:

```csharp
using BibliaTeologica.Domain.Entities;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface ILivroRepository
{
    Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct);
    Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct);
}
```

- [ ] **Step 6: Implementar `LivroMappingProfile`**

`Common/Mapping/LivroMappingProfile.cs`:

```csharp
using AutoMapper;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;

namespace BibliaTeologica.Application.Common.Mapping;

public class LivroMappingProfile : Profile
{
    public LivroMappingProfile()
    {
        CreateMap<Livro, LivroListItemDto>()
            .ForCtorParam(nameof(LivroListItemDto.Secao), o => o.MapFrom(l => l.Secao.ToString()))
            .ForCtorParam(nameof(LivroListItemDto.SecaoRotulo), o => o.MapFrom(l => l.Secao.Rotulo()));

        CreateMap<Livro, LivroDetailDto>()
            .ForCtorParam(nameof(LivroDetailDto.Secao), o => o.MapFrom(l => l.Secao.ToString()))
            .ForCtorParam(nameof(LivroDetailDto.SecaoRotulo), o => o.MapFrom(l => l.Secao.Rotulo()))
            .ForCtorParam(nameof(LivroDetailDto.Testamento), o => o.MapFrom(l => l.Testamento.ToString()))
            .ForCtorParam(nameof(LivroDetailDto.Ideias), o => o.MapFrom(l => l.Ideias));
    }
}
```

- [ ] **Step 7: Implementar `ILivroQueryService` + `LivroQueryService`**

`Livros/ILivroQueryService.cs`:

```csharp
using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public interface ILivroQueryService
{
    Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct);
    Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct);
}
```

`Livros/LivroQueryService.cs`:

```csharp
using AutoMapper;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public class LivroQueryService : ILivroQueryService
{
    private readonly ILivroRepository _repositorio;
    private readonly IMapper _mapper;

    public LivroQueryService(ILivroRepository repositorio, IMapper mapper)
    {
        _repositorio = repositorio;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LivroListItemDto>> ListarAsync(CancellationToken ct)
    {
        var livros = await _repositorio.ListarPublicadosAsync(ct);
        return livros.Select(_mapper.Map<LivroListItemDto>).ToList();
    }

    public async Task<LivroDetailDto?> ObterPorSlugAsync(string slug, CancellationToken ct)
    {
        var livro = await _repositorio.ObterPorSlugAsync(slug, ct);
        return livro is null || !livro.Publicado ? null : _mapper.Map<LivroDetailDto>(livro);
    }
}
```

- [ ] **Step 8: Implementar `DependencyInjection`**

`DependencyInjection.cs`:

```csharp
using BibliaTeologica.Application.Livros;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ILivroQueryService, LivroQueryService>();
        return services;
    }
}
```

> Nota: `AutoMapper` 13.0.1 traz `AddAutoMapper(params Assembly[])` via `AutoMapper.Extensions.Microsoft.DependencyInjection` embutido. Se o método não resolver, adicionar `<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" />` + `<PackageVersion ... Version="12.0.1" />`.

- [ ] **Step 9: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests`
Expected: PASS (5 testes).

- [ ] **Step 10: Build da solução**

Run: `dotnet build BibliaTeologica.sln`
Expected: `Build succeeded`, 0 warnings.

- [ ] **Step 11: Commit**

```bash
git add src/BibliaTeologica.Application tests/BibliaTeologica.Application.Tests Directory.Packages.props
git commit -m "feat(application): DTOs, ILivroRepository, LivroQueryService e mapping"
```

---

## Task 7: Infrastructure — `AppDbContext`, configuração e migração inicial

**Files:**
- Modify: `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj` (pacotes EF Core)
- Create: `src/BibliaTeologica.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Configurations/LivroConfiguration.cs`
- Create: `src/BibliaTeologica.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Migrations/*` (via `dotnet ef`)
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Persistence/AppDbContextTests.cs`
- Modify: `tests/BibliaTeologica.Infrastructure.Tests/BibliaTeologica.Infrastructure.Tests.csproj` (Sqlite + EF Design)

**Interfaces:**
- Consumes: `Livro` (Domain), `SecaoCanonica`.
- Produces:
  - `class AppDbContext : DbContext` — ctor `AppDbContext(DbContextOptions<AppDbContext>)`, `DbSet<Livro> Livros`.
  - `class LivroConfiguration : IEntityTypeConfiguration<Livro>`.
  - `class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>` (usa `Data Source=biblia-design.db`).
  - Tabela `Livros`: PK `Id` identity; `Slug` único, `MaxLength(120)`; `Ordem` único; `Secao` como `string` (`HasConversion<string>`, `MaxLength(40)`); `Ideias` como coluna `TEXT` JSON (value converter + comparer); `Testamento` ignorado.

- [ ] **Step 1: Pacotes**

`BibliaTeologica.Infrastructure.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

`BibliaTeologica.Infrastructure.Tests.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
</ItemGroup>
```

Instalar a tool: `dotnet tool install --global dotnet-ef` (se não instalada). Verificar: `dotnet ef --version` (deve ser 8.x — se for outra major, `dotnet tool update --global dotnet-ef --version 8.*`).

- [ ] **Step 2: Teste que falha**

`tests/BibliaTeologica.Infrastructure.Tests/Persistence/AppDbContextTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public AppDbContextTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
    }

    private AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task PersisteERecupera_Livro_ComIdeiasComoJson()
    {
        await using (var ctx = CriarContexto())
        {
            ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
                "quem", "porque", "paraQuem", "estrutura", "tema",
                new[] { "Criação", "Queda" }, "Gênesis 1:1"));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            var livro = await ctx.Livros.SingleAsync();
            livro.Slug.Should().Be("genesis");
            livro.Secao.Should().Be(SecaoCanonica.Pentateuco);
            livro.Ideias.Should().Equal("Criação", "Queda");
        }
    }

    [Fact]
    public async Task RejeitaSlugDuplicado()
    {
        await using var ctx = CriarContexto();
        ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v"));
        ctx.Livros.Add(Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 2,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v"));

        var acao = async () => await ctx.SaveChangesAsync();
        await acao.Should().ThrowAsync<DbUpdateException>();
    }

    public void Dispose() => _conexao.Dispose();
}
```

- [ ] **Step 3: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~AppDbContextTests"`
Expected: FAIL — `AppDbContext` não existe.

- [ ] **Step 4: Implementar `AppDbContext`**

```csharp
using BibliaTeologica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

- [ ] **Step 5: Implementar `LivroConfiguration`**

```csharp
using System.Text.Json;
using BibliaTeologica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliaTeologica.Infrastructure.Persistence.Configurations;

public class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("Livros");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Slug).HasMaxLength(120).IsRequired();
        builder.HasIndex(l => l.Slug).IsUnique();

        builder.Property(l => l.Nome).HasMaxLength(120).IsRequired();
        builder.HasIndex(l => l.Ordem).IsUnique();

        builder.Property(l => l.Secao).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.Property(l => l.Quem).IsRequired();
        builder.Property(l => l.Porque).IsRequired();
        builder.Property(l => l.ParaQuem).IsRequired();
        builder.Property(l => l.Estrutura).IsRequired();
        builder.Property(l => l.Tema).IsRequired();
        builder.Property(l => l.Versiculo).IsRequired();
        builder.Property(l => l.Publicado).IsRequired();
        builder.Property(l => l.CriadoEm).IsRequired();
        builder.Property(l => l.AtualizadoEm).IsRequired();

        builder.Ignore(l => l.Testamento);

        // `Ideias` (IReadOnlyList get-only) apenas lê o campo `_ideias`. Persistimos o CAMPO
        // como coluna TEXT (JSON) e ignoramos a propriedade pública — nenhum dado é perdido,
        // porque a leitura da propriedade devolve exatamente o conteúdo de `_ideias`.
        builder.Ignore(l => l.Ideias);

        builder.Property<List<string>>("_ideias")
            .HasColumnName("Ideias")
            .HasColumnType("TEXT")
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    v => v.ToList()));
    }
}
```

> O teste do Step 2 (`PersisteERecupera_Livro_ComIdeiasComoJson`) é a validação: grava um `Livro` com 2 ideias, reabre o contexto e confirma `livro.Ideias` igual. Se a leitura vier vazia, o campo `_ideias` não está sendo materializado — conferir se o nome do campo bate (`_ideias`, com underscore) e se a entidade tem `private readonly List<string> _ideias = new();`.

- [ ] **Step 6: Implementar `DesignTimeDbContextFactory`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BibliaTeologica.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=biblia-design.db")
            .Options;
        return new AppDbContext(options);
    }
}
```

- [ ] **Step 7: Ver passar os testes do contexto**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~AppDbContextTests"`
Expected: PASS (2 testes). Se `Ideias` falhar, ajustar o mapeamento conforme a nota do Step 5 e repetir.

- [ ] **Step 8: Gerar a migração inicial**

```bash
dotnet ef migrations add InitialCreate \
  --project src/BibliaTeologica.Infrastructure \
  --startup-project src/BibliaTeologica.Api \
  --output-dir Persistence/Migrations
```

Expected: cria `Persistence/Migrations/*_InitialCreate.cs` + `AppDbContextModelSnapshot.cs`. Conferir que a tabela `Livros` tem colunas `Ideias` (TEXT), `Secao` (TEXT), índices únicos em `Slug` e `Ordem`.

Adicionar `biblia-design.db` ao `.gitignore` (já coberto por `*.db`).

- [ ] **Step 9: Commit**

```bash
git add src/BibliaTeologica.Infrastructure tests/BibliaTeologica.Infrastructure.Tests
git commit -m "feat(infra): AppDbContext, mapeamento de Livro e migração InitialCreate"
```

---

## Task 8: Infrastructure — `livros.json` e `LivroSeeder`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Seed/livros.json`
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Seed/LivroSeeder.cs`
- Modify: `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj` (copiar o JSON para o output + pacote de logging)
- Create (temporário, não commitar): `scripts/gerar-livros-json.mjs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Persistence/LivroSeederTests.cs`

**Interfaces:**
- Consumes: `AppDbContext` (Task 7), `Livro.Create`, `SecaoCanonicaExtensions.FromRotulo`.
- Produces:
  - `livros.json` — array de 66 objetos com chaves `section, nome, quem, porque, praquem, estrutura, tema, ideias (string[]), versiculo` na ordem canônica (Gênesis → Apocalipse).
  - `class LivroSeeder` — ctor `(AppDbContext db, ILogger<LivroSeeder> log)`, método `Task SeedAsync(CancellationToken ct = default)`. Idempotente: se `Livros` já tem qualquer linha, não faz nada. `ordem` de cada livro = índice no array + 1.

- [ ] **Step 1: Gerar `livros.json` a partir do protótipo**

Criar `scripts/gerar-livros-json.mjs`:

```js
import { readFileSync, writeFileSync } from 'node:fs';

const html = readFileSync('docs/prototipo-ficha-66-livros.html', 'utf8');
const inicio = html.indexOf('const BOOKS = [');
const fim = html.indexOf('];', inicio) + 1;
const arrayLiteral = html.slice(inicio + 'const BOOKS = '.length, fim);
const books = eval(arrayLiteral); // arquivo local e confiável

if (books.length !== 66) throw new Error(`Esperava 66 livros, achei ${books.length}`);

const saida = books.map(b => ({
  section: b.section,
  nome: b.nome,
  quem: b.quem,
  porque: b.porque,
  praquem: b.praquem,
  estrutura: b.estrutura,
  tema: b.tema,
  ideias: b.ideias,
  versiculo: b.versiculo,
}));

writeFileSync(
  'src/BibliaTeologica.Infrastructure/Persistence/Seed/livros.json',
  JSON.stringify(saida, null, 2) + '\n',
  'utf8',
);
console.log('livros.json gerado com', saida.length, 'livros');
```

Run: `node scripts/gerar-livros-json.mjs`
Expected: `livros.json gerado com 66 livros`. Conferir à mão: primeiro objeto `nome: "GÊNESIS"`, último `nome: "APOCALIPSE"`; nenhum campo vazio.

Depois: `git rm -r --cached scripts 2>/dev/null; echo "scripts/" >> .gitignore` — o script é one-off, não versionar. (Se preferir manter para regenerar no futuro, versionar `scripts/` e pular o ignore — decisão do implementador; o default é não versionar.)

- [ ] **Step 2: Copiar o JSON para o output**

`BibliaTeologica.Infrastructure.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  <Content Include="Persistence/Seed/livros.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

`<Content>` (não `<None>`) é o que faz o `livros.json` fluir para o output dos projetos que referenciam a Infrastructure — `BibliaTeologica.Api`, `BibliaTeologica.Infrastructure.Tests` e `BibliaTeologica.Api.Tests` — sem precisar declarar cópia em cada um.

Adicionar `<PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" />` ao `Directory.Packages.props`.

- [ ] **Step 3: Teste que falha**

`tests/BibliaTeologica.Infrastructure.Tests/Persistence/LivroSeederTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class LivroSeederTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public LivroSeederTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
    }

    private AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task SeedAsync_Insere66Livros_QuandoTabelaVazia()
    {
        await using var ctx = CriarContexto();
        await new LivroSeeder(ctx, NullLogger<LivroSeeder>.Instance).SeedAsync();

        ctx.Livros.Count().Should().Be(66);
        (await ctx.Livros.OrderBy(l => l.Ordem).FirstAsync()).Slug.Should().Be("genesis");
        (await ctx.Livros.OrderBy(l => l.Ordem).LastAsync()).Slug.Should().Be("apocalipse");
        ctx.Livros.Select(l => l.Ordem).OrderBy(o => o).Should().Equal(Enumerable.Range(1, 66));
    }

    [Fact]
    public async Task SeedAsync_NaoDuplica_QuandoJaHaLivros()
    {
        await using var ctx = CriarContexto();
        var seeder = new LivroSeeder(ctx, NullLogger<LivroSeeder>.Instance);
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        ctx.Livros.Count().Should().Be(66);
    }

    public void Dispose() => _conexao.Dispose();
}
```

- [ ] **Step 4: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LivroSeederTests"`
Expected: FAIL — `LivroSeeder` não existe.

- [ ] **Step 5: Implementar `LivroSeeder`**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Persistence;

public class LivroSeeder
{
    private static readonly string CaminhoJson =
        Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "livros.json");

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _db;
    private readonly ILogger<LivroSeeder> _log;

    public LivroSeeder(AppDbContext db, ILogger<LivroSeeder> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Livros.AnyAsync(ct))
            return;

        await using var stream = File.OpenRead(CaminhoJson);
        var registros = await JsonSerializer.DeserializeAsync<List<RegistroSeed>>(stream, OpcoesJson, ct)
            ?? throw new InvalidOperationException("livros.json vazio ou inválido.");

        var livros = registros.Select((r, i) => Livro.Create(
            r.Nome,
            SecaoCanonicaExtensions.FromRotulo(r.Section),
            i + 1,
            r.Quem, r.Porque, r.ParaQuem, r.Estrutura, r.Tema, r.Ideias, r.Versiculo));

        _db.Livros.AddRange(livros);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Seed concluído: {Total} livros inseridos.", registros.Count);
    }

    private sealed record RegistroSeed(
        string Section,
        string Nome,
        string Quem,
        string Porque,
        [property: JsonPropertyName("praquem")] string ParaQuem,
        string Estrutura,
        string Tema,
        List<string> Ideias,
        string Versiculo);
}
```

- [ ] **Step 6: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests`
Expected: PASS (todos — contexto + seeder).

- [ ] **Step 7: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Persistence/Seed tests/BibliaTeologica.Infrastructure.Tests Directory.Packages.props src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj .gitignore
git commit -m "feat(infra): livros.json (66) e LivroSeeder idempotente"
```

---

## Task 9: Infrastructure — `LivroRepository` e `AddInfrastructure`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Repositories/LivroRepository.cs`
- Create: `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Repositories/LivroRepositoryTests.cs`

**Interfaces:**
- Consumes: `ILivroRepository` (Application), `AppDbContext`, `LivroSeeder` (para semear dados de teste).
- Produces:
  - `class LivroRepository : ILivroRepository` — ctor `(AppDbContext db)`.
    - `ListarPublicadosAsync`: `AsNoTracking().Where(Publicado).OrderBy(Ordem).ToListAsync`.
    - `ObterPorSlugAsync`: `AsNoTracking().FirstOrDefaultAsync(l => l.Slug == slug)`.
  - `static IServiceCollection AddInfrastructure(this IServiceCollection, IConfiguration config)` — lê `ConnectionStrings:Default`, registra `AppDbContext` (Sqlite), `ILivroRepository→LivroRepository`, `LivroSeeder`.

- [ ] **Step 1: Teste que falha**

```csharp
using AwesomeAssertions;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Repositories;

public sealed class LivroRepositoryTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _db;

    public LivroRepositoryTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);
        _db.Database.EnsureCreated();
    }

    private static Livro Livro(string nome, int ordem, bool publicado = true)
    {
        var l = Domain.Entities.Livro.Create(nome, SecaoCanonica.Pentateuco, ordem,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        if (!publicado)
            typeof(Livro).GetProperty(nameof(Livro.Publicado))!
                .GetSetMethod(nonPublic: true)!.Invoke(l, new object?[] { false });
        return l;
    }

    [Fact]
    public async Task ListarPublicadosAsync_RetornaSoPublicados_OrdenadosPorOrdem()
    {
        _db.Livros.AddRange(Livro("ÊXODO", 2), Livro("GÊNESIS", 1), Livro("LEVÍTICO", 3, publicado: false));
        await _db.SaveChangesAsync();

        var repo = new LivroRepository(_db);
        var resultado = await repo.ListarPublicadosAsync(CancellationToken.None);

        resultado.Select(l => l.Slug).Should().Equal("genesis", "exodo");
    }

    [Fact]
    public async Task ObterPorSlugAsync_RetornaLivro_QuandoExiste()
    {
        _db.Livros.Add(Livro("GÊNESIS", 1));
        await _db.SaveChangesAsync();

        var repo = new LivroRepository(_db);
        (await repo.ObterPorSlugAsync("genesis", CancellationToken.None)).Should().NotBeNull();
        (await repo.ObterPorSlugAsync("nao-existe", CancellationToken.None)).Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LivroRepositoryTests"`
Expected: FAIL — `LivroRepository` não existe.

- [ ] **Step 3: Implementar `LivroRepository`**

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Repositories;

public class LivroRepository : ILivroRepository
{
    private readonly AppDbContext _db;

    public LivroRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct)
        => await _db.Livros.AsNoTracking()
            .Where(l => l.Publicado)
            .OrderBy(l => l.Ordem)
            .ToListAsync(ct);

    public Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct)
        => _db.Livros.AsNoTracking().FirstOrDefaultAsync(l => l.Slug == slug, ct);
}
```

- [ ] **Step 4: Implementar `DependencyInjection`**

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<LivroSeeder>();

        return services;
    }
}
```

Adicionar `<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />` e `Microsoft.Extensions.DependencyInjection.Abstractions` ao `.csproj` da Infrastructure; versões (`8.0.0` / `8.0.2`) no `Directory.Packages.props` se ainda não houver.

- [ ] **Step 5: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests`
Expected: PASS (todos).

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Infrastructure tests/BibliaTeologica.Infrastructure.Tests Directory.Packages.props
git commit -m "feat(infra): LivroRepository e AddInfrastructure"
```

---

## Task 10: Api — `Program.cs`, middlewares, rate limiting, CORS, health

**Files:**
- Modify: `src/BibliaTeologica.Api/BibliaTeologica.Api.csproj` (Swashbuckle, HealthChecks EF)
- Create: `src/BibliaTeologica.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `src/BibliaTeologica.Api/Middleware/SecurityHeadersMiddleware.cs`
- Rewrite: `src/BibliaTeologica.Api/Program.cs`
- Create: `src/BibliaTeologica.Api/appsettings.json` (sobrescrever o gerado)
- Test: `tests/BibliaTeologica.Api.Tests/AppFactory.cs`
- Test: `tests/BibliaTeologica.Api.Tests/InfraestruturaTests.cs`
- Modify: `tests/BibliaTeologica.Api.Tests/*.csproj` (`Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.Sqlite`)

**Interfaces:**
- Consumes: `AddApplication()`, `AddInfrastructure(config)`, `AppDbContext`, `LivroSeeder`, `DomainException`.
- Produces:
  - `class ExceptionHandlingMiddleware` — `DomainException`→400, outras→500 (logadas), corpo `ProblemDetails`. `detail` só em `Development`.
  - `class SecurityHeadersMiddleware` — adiciona `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'`.
  - `Program` configurado: controllers, ProblemDetails, Swagger (dev), CORS política `"spa"` (origens de `Cors:AllowedOrigins`), rate limiter fixo global (`RateLimiting:PermitLimit`/`WindowSeconds`, default 100/60, rejeição 429), health `/health`, migração+seed no startup.
  - `AppFactory : WebApplicationFactory<Program>` para os testes — usa arquivo SQLite temporário; expõe `SeedAsync()` e `Task<AppDbContext> AbrirContextoAsync()`.

- [ ] **Step 1: Pacotes**

`BibliaTeologica.Api.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Swashbuckle.AspNetCore" />
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" />
</ItemGroup>
```

`BibliaTeologica.Api.Tests.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
</ItemGroup>
```

- [ ] **Step 2: `appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Data Source=/data/biblia.db"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60
  }
}
```

Apagar `appsettings.Development.json` do controle de versão se o template criou um com conteúdo — deve ficar git-ignored (já está no `.gitignore`).

- [ ] **Step 3: `SecurityHeadersMiddleware`**

```csharp
namespace BibliaTeologica.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        await _next(context);
    }
}
```

- [ ] **Step 4: `ExceptionHandlingMiddleware`**

```csharp
using BibliaTeologica.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log, IHostEnvironment env)
    {
        _next = next;
        _log = log;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await EscreverProblema(context, StatusCodes.Status400BadRequest, "Requisição inválida", ex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
            var detalhe = _env.IsDevelopment() ? ex.ToString() : "Ocorreu um erro inesperado.";
            await EscreverProblema(context, StatusCodes.Status500InternalServerError, "Erro interno", detalhe);
        }
    }

    private static async Task EscreverProblema(HttpContext context, int status, string titulo, string detalhe)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path
        });
    }
}
```

- [ ] **Step 5: `Program.cs` (reescrever)**

```csharp
using System.Threading.RateLimiting;
using BibliaTeologica.Api.Middleware;
using BibliaTeologica.Application;
using BibliaTeologica.Infrastructure;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var origensCors = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddPolicy("spa", policy =>
    policy.WithOrigins(origensCors).AllowAnyHeader().AllowAnyMethod()));

var permitLimit = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 100;
var windowSeconds = builder.Configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0
            }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<LivroSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("spa");
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
```

- [ ] **Step 6: `AppFactory` de teste**

`tests/BibliaTeologica.Api.Tests/AppFactory.cs`:

```csharp
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BibliaTeologica.Api.Tests;

public class AppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _arquivoDb = Path.Combine(Path.GetTempPath(), $"biblia-teste-{Guid.NewGuid():N}.db");

    public Dictionary<string, string?> ConfiguracaoExtra { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_arquivoDb}"
            });
            if (ConfiguracaoExtra.Count > 0)
                config.AddInMemoryCollection(ConfiguracaoExtra);
        });
    }

    public async Task<AppDbContext> AbrirContextoAsync()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        return db;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (File.Exists(_arquivoDb)) File.Delete(_arquivoDb);
    }
}
```

- [ ] **Step 7: Testes de infraestrutura da API**

`tests/BibliaTeologica.Api.Tests/InfraestruturaTests.cs`:

```csharp
using System.Net;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class InfraestruturaTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public InfraestruturaTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_Retorna200()
    {
        var resposta = await _factory.CreateClient().GetAsync("/health");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Resposta_ContemSecurityHeaders()
    {
        var resposta = await _factory.CreateClient().GetAsync("/health");
        resposta.Headers.TryGetValues("X-Content-Type-Options", out var valores).Should().BeTrue();
        valores!.Should().Contain("nosniff");
    }

    [Fact]
    public async Task Startup_SemeiaOs66Livros()
    {
        _ = _factory.CreateClient();
        await using var db = await _factory.AbrirContextoAsync();
        db.Livros.Count().Should().Be(66);
    }
}
```

- [ ] **Step 8: Rodar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests`
Expected: PASS (3 testes). O startup roda `MigrateAsync` + seed no arquivo temporário.

- [ ] **Step 9: Build completo**

Run: `dotnet build BibliaTeologica.sln && dotnet test BibliaTeologica.sln`
Expected: tudo verde.

- [ ] **Step 10: Commit**

```bash
git add src/BibliaTeologica.Api tests/BibliaTeologica.Api.Tests
git commit -m "feat(api): Program.cs, middlewares de erro/segurança, rate limiting, health e seed no startup"
```

---

## Task 11: Api — `LivrosController` e testes de endpoint

**Files:**
- Create: `src/BibliaTeologica.Api/Controllers/LivrosController.cs`
- Test: `tests/BibliaTeologica.Api.Tests/LivrosEndpointsTests.cs`

**Interfaces:**
- Consumes: `ILivroQueryService`, `LivroListItemDto`, `LivroDetailDto`, `AppFactory`.
- Produces:
  - `GET /api/livros` → `200` com `LivroListItemDto[]` (publicados, ordenados por `Ordem`).
  - `GET /api/livros/{slug}` → `200` `LivroDetailDto` | `404` (slug inexistente ou livro despublicado).

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Api.Tests/LivrosEndpointsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class LivrosEndpointsTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public LivrosEndpointsTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_ApiLivros_Retorna200ComListaOrdenada()
    {
        var cliente = _factory.CreateClient();

        var livros = await cliente.GetFromJsonAsync<List<LivroListItemDto>>("/api/livros");

        livros.Should().NotBeNull();
        livros!.Should().HaveCount(66);
        livros.Select(l => l.Ordem).Should().BeInAscendingOrder();
        livros.First().Slug.Should().Be("genesis");
        livros.Last().Slug.Should().Be("apocalipse");
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna200ComDetalhe_QuandoExiste()
    {
        var cliente = _factory.CreateClient();

        var livro = await cliente.GetFromJsonAsync<LivroDetailDto>("/api/livros/genesis");

        livro.Should().NotBeNull();
        livro!.Nome.Should().Be("GÊNESIS");
        livro.Testamento.Should().Be("Antigo");
        livro.Ideias.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna404_QuandoNaoExiste()
    {
        var resposta = await _factory.CreateClient().GetAsync("/api/livros/nao-existe");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiLivrosSlug_Retorna404_QuandoDespublicado()
    {
        var cliente = _factory.CreateClient();
        await using (var db = await _factory.AbrirContextoAsync())
        {
            var livro = db.Livros.First(l => l.Slug == "obadias");
            db.Entry(livro).Property(nameof(Livro.Publicado)).CurrentValue = false;
            await db.SaveChangesAsync();
        }

        var resposta = await cliente.GetAsync("/api/livros/obadias");
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

E o teste de rate limiting, com fixture de config apertada (classe própria para não vazar o limite para os outros testes):

`tests/BibliaTeologica.Api.Tests/RateLimitingTests.cs`:

```csharp
using System.Net;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests;

public class RateLimitingTests
{
    [Fact]
    public async Task GET_ApiLivros_Retorna429_QuandoExcedeLimite()
    {
        await using var factory = new AppFactory();
        factory.ConfiguracaoExtra["RateLimiting:PermitLimit"] = "3";
        factory.ConfiguracaoExtra["RateLimiting:WindowSeconds"] = "60";

        var cliente = factory.CreateClient();

        HttpStatusCode ultimo = HttpStatusCode.OK;
        for (var i = 0; i < 6; i++)
            ultimo = (await cliente.GetAsync("/api/livros")).StatusCode;

        ultimo.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
```

> `RateLimitingTests` não usa `IClassFixture` — instancia sua própria `AppFactory` para aplicar `ConfiguracaoExtra` antes do primeiro cliente.

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests --filter "FullyQualifiedName~LivrosEndpointsTests"`
Expected: FAIL — rota `/api/livros` retorna 404 (controller não existe).

- [ ] **Step 3: Implementar o controller**

```csharp
using BibliaTeologica.Application.Livros;
using BibliaTeologica.Application.Livros.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/livros")]
public class LivrosController : ControllerBase
{
    private readonly ILivroQueryService _livros;

    public LivrosController(ILivroQueryService livros) => _livros = livros;

    // Público: catálogo de conteúdo, sem dados sensíveis. Sem [Authorize] por design (ver CLAUDE.md).
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LivroListItemDto>>> Listar(CancellationToken ct)
        => Ok(await _livros.ListarAsync(ct));

    [HttpGet("{slug}")]
    public async Task<ActionResult<LivroDetailDto>> ObterPorSlug(string slug, CancellationToken ct)
    {
        var livro = await _livros.ObterPorSlugAsync(slug, ct);
        return livro is null ? NotFound() : Ok(livro);
    }
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests`
Expected: PASS (todos — infra + endpoints + rate limiting).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Api/Controllers tests/BibliaTeologica.Api.Tests
git commit -m "feat(api): LivrosController (GET lista + GET por slug)"
```

---

## Task 12: Api — `Dockerfile` e `.dockerignore` da raiz

**Files:**
- Create: `src/BibliaTeologica.Api/Dockerfile`
- Create: `.dockerignore` (raiz — o build da API usa a raiz como contexto)

**Interfaces:**
- Consumes: solução compilável (Tasks 2–11).
- Produces: imagem que expõe a API na porta `8080`, lê `ConnectionStrings__Default` do ambiente, guarda o `.db` em `/data`.

- [ ] **Step 1: `.dockerignore` na raiz**

```gitignore
**/bin/
**/obj/
**/.vs/
**/.idea/
**/node_modules/
frontend/**/dist/
frontend/**/.angular/
**/*.db
**/*.db-shm
**/*.db-wal
.git/
docs/
tests/
scripts/
**/appsettings.Development.json
**/.env
**/.env.*
```

- [ ] **Step 2: `Dockerfile` da API**

```dockerfile
# build a partir da raiz do repo: docker build -f src/BibliaTeologica.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/BibliaTeologica.Domain/*.csproj src/BibliaTeologica.Domain/
COPY src/BibliaTeologica.Application/*.csproj src/BibliaTeologica.Application/
COPY src/BibliaTeologica.Infrastructure/*.csproj src/BibliaTeologica.Infrastructure/
COPY src/BibliaTeologica.Api/*.csproj src/BibliaTeologica.Api/
RUN dotnet restore src/BibliaTeologica.Api/BibliaTeologica.Api.csproj

COPY src/ src/
RUN dotnet publish src/BibliaTeologica.Api/BibliaTeologica.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /data
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "BibliaTeologica.Api.dll"]
```

- [ ] **Step 3: Build da imagem**

Run: `docker build -f src/BibliaTeologica.Api/Dockerfile -t biblia-api:dev .`
Expected: `Successfully tagged biblia-api:dev`.

- [ ] **Step 4: Fumaça — subir o contêiner isolado**

Run:
```bash
docker run --rm -d --name biblia-api-smoke -p 8099:8080 \
  -e ConnectionStrings__Default="Data Source=/data/biblia.db" biblia-api:dev
sleep 3
curl -fs http://localhost:8099/health && echo OK
curl -fs http://localhost:8099/api/livros | head -c 200
docker stop biblia-api-smoke
```
Expected: `/health` responde e `/api/livros` retorna JSON com livros.

- [ ] **Step 5: Commit**

```bash
git add .dockerignore src/BibliaTeologica.Api/Dockerfile
git commit -m "build(api): Dockerfile multi-stage e .dockerignore da raiz"
```

---

## Task 13: Frontend — scaffold Angular 22, ambiente e proxy

**Files:**
- Create: `frontend/biblia-teologica-web/` (via `ng new`)
- Create: `frontend/biblia-teologica-web/src/environments/environment.ts`, `environment.prod.ts`
- Create: `frontend/biblia-teologica-web/proxy.conf.json`
- Modify: `frontend/biblia-teologica-web/angular.json` (usar o proxy no `serve`; `fileReplacements` de produção)
- Modify: `frontend/biblia-teologica-web/src/app/app.config.ts` (HttpClient)

**Interfaces:**
- Consumes: nada.
- Produces:
  - `export const environment = { production: boolean, apiBaseUrl: string }` em ambos os arquivos. Dev: `apiBaseUrl: '/api'`. Prod: `apiBaseUrl: '/api'`.
  - `provideHttpClient()` registrado em `app.config.ts`.
  - `npm start` serve em `:4200` com `/api` → `http://localhost:5080`.

- [ ] **Step 1: `ng new`**

```bash
cd frontend
npx -y @angular/cli@22 new biblia-teologica-web \
  --style=scss --ssr=false --routing --package-manager=npm --skip-git
cd biblia-teologica-web
npx ng version   # confirmar Angular 22.x
```

- [ ] **Step 2: Ambientes**

`src/environments/environment.ts`:

```ts
export const environment = {
  production: false,
  apiBaseUrl: '/api',
};
```

`src/environments/environment.prod.ts`:

```ts
export const environment = {
  production: true,
  apiBaseUrl: '/api',
};
```

- [ ] **Step 3: `proxy.conf.json`**

```json
{
  "/api": {
    "target": "http://localhost:5080",
    "secure": false,
    "changeOrigin": true
  }
}
```

- [ ] **Step 4: `angular.json`**

- Em `projects.biblia-teologica-web.architect.serve.options`, adicionar `"proxyConfig": "proxy.conf.json"`.
- Em `architect.build.configurations.production`, adicionar:

```json
"fileReplacements": [
  { "replace": "src/environments/environment.ts", "with": "src/environments/environment.prod.ts" }
]
```

- Confirmar `architect.build.options.outputPath` = `dist/biblia-teologica-web`.

- [ ] **Step 5: `app.config.ts`**

```ts
import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withFetch()),
  ],
};
```

> Se o scaffold do v22 já usa `provideZonelessChangeDetection` (ou `provideBrowserGlobalErrorListeners`), manter o que veio e só **adicionar** `provideHttpClient(withFetch())`.

- [ ] **Step 6: Verificações**

Run: `npm run build`
Expected: build de produção OK, saída em `dist/biblia-teologica-web/browser`.

Run: `npx tsc --noEmit -p tsconfig.app.json`
Expected: sem erros.

- [ ] **Step 7: Commit**

```bash
git add frontend/biblia-teologica-web
git commit -m "chore(web): scaffold Angular 22 + ambientes + proxy /api"
```

---

## Task 14: Frontend — modelos, `LivroService` e `LivroStore`

**Files:**
- Create: `src/app/core/models/livro.model.ts`
- Create: `src/app/core/services/livro.service.ts`
- Create: `src/app/core/services/livro.store.ts`
- Test: `src/app/core/services/livro.service.spec.ts`
- Test: `src/app/core/services/livro.store.spec.ts`

**Interfaces:**
- Consumes: `environment.apiBaseUrl`.
- Produces:
  - `interface LivroListItem { slug: string; nome: string; ordem: number; secao: string; secaoRotulo: string; }`
  - `interface LivroDetail extends LivroListItem { testamento: string; quem: string; porque: string; paraQuem: string; estrutura: string; tema: string; ideias: string[]; versiculo: string; }`
  - `class LivroService` (`providedIn: 'root'`): `listar(): Observable<LivroListItem[]>`, `obterPorSlug(slug: string): Observable<LivroDetail>`.
  - `class LivroStore` (`providedIn: 'root'`): `readonly livros: Signal<LivroListItem[]>`, `readonly carregando: Signal<boolean>`, `carregar(): void` (idempotente), `vizinhos(slug): { anterior?: LivroListItem; proximo?: LivroListItem }`.

- [ ] **Step 1: Testes que falham**

`src/app/core/services/livro.service.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LivroService } from './livro.service';

describe('LivroService', () => {
  let service: LivroService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), LivroService],
    });
    service = TestBed.inject(LivroService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listar() faz GET em /api/livros', () => {
    service.listar().subscribe();
    const req = http.expectOne('/api/livros');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('obterPorSlug() faz GET em /api/livros/:slug', () => {
    service.obterPorSlug('genesis').subscribe();
    const req = http.expectOne('/api/livros/genesis');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
```

`src/app/core/services/livro.store.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LivroStore } from './livro.store';
import type { LivroListItem } from '../models/livro.model';

const item = (slug: string, ordem: number): LivroListItem =>
  ({ slug, nome: slug, ordem, secao: 'Pentateuco', secaoRotulo: 'PENTATEUCO' });

describe('LivroStore', () => {
  let store: LivroStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), LivroStore],
    });
    store = TestBed.inject(LivroStore);
    http = TestBed.inject(HttpTestingController);
  });

  it('carrega uma vez só', () => {
    store.carregar();
    store.carregar();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
    http.verify();
    expect(store.livros().length).toBe(2);
  });

  it('vizinhos() devolve anterior e próximo pela ordem', () => {
    store.carregar();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2), item('levitico', 3)]);
    expect(store.vizinhos('exodo')).toEqual({
      anterior: jasmine.objectContaining({ slug: 'genesis' }),
      proximo: jasmine.objectContaining({ slug: 'levitico' }),
    });
    expect(store.vizinhos('genesis').anterior).toBeUndefined();
  });
});
```

- [ ] **Step 2: Ver falhar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: FAIL — módulos não existem.

- [ ] **Step 3: Modelos**

`src/app/core/models/livro.model.ts`:

```ts
export interface LivroListItem {
  slug: string;
  nome: string;
  ordem: number;
  secao: string;
  secaoRotulo: string;
}

export interface LivroDetail extends LivroListItem {
  testamento: string;
  quem: string;
  porque: string;
  paraQuem: string;
  estrutura: string;
  tema: string;
  ideias: string[];
  versiculo: string;
}
```

- [ ] **Step 4: `LivroService`**

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { LivroDetail, LivroListItem } from '../models/livro.model';

@Injectable({ providedIn: 'root' })
export class LivroService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/livros`;

  listar(): Observable<LivroListItem[]> {
    return this.http.get<LivroListItem[]>(this.base);
  }

  obterPorSlug(slug: string): Observable<LivroDetail> {
    return this.http.get<LivroDetail>(`${this.base}/${slug}`);
  }
}
```

- [ ] **Step 5: `LivroStore`**

```ts
import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LivroService } from './livro.service';
import type { LivroListItem } from '../models/livro.model';

@Injectable({ providedIn: 'root' })
export class LivroStore {
  private readonly service = inject(LivroService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _livros = signal<LivroListItem[]>([]);
  private readonly _carregando = signal(false);
  private carregado = false;

  readonly livros = this._livros.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly total = computed(() => this._livros().length);

  carregar(): void {
    if (this.carregado || this._carregando()) return;
    this._carregando.set(true);
    this.service
      .listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (livros) => {
          this._livros.set(livros);
          this.carregado = true;
          this._carregando.set(false);
        },
        error: () => this._carregando.set(false),
      });
  }

  vizinhos(slug: string): { anterior?: LivroListItem; proximo?: LivroListItem } {
    const lista = this._livros();
    const i = lista.findIndex((l) => l.slug === slug);
    if (i === -1) return {};
    return { anterior: lista[i - 1], proximo: lista[i + 1] };
  }
}
```

- [ ] **Step 6: Ver passar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add frontend/biblia-teologica-web/src/app/core
git commit -m "feat(web): modelos, LivroService e LivroStore com signals"
```

---

## Task 15: Frontend — componentes compartilhados `badge-secao` e `campo-ficha`

**Files:**
- Create: `src/app/shared/components/badge-secao/badge-secao.component.ts`
- Create: `src/app/shared/components/campo-ficha/campo-ficha.component.ts`
- Test: `src/app/shared/components/badge-secao/badge-secao.component.spec.ts`
- Test: `src/app/shared/components/campo-ficha/campo-ficha.component.spec.ts`

**Interfaces:**
- Consumes: nada.
- Produces:
  - `BadgeSecaoComponent` — standalone, selector `bt-badge-secao`, input `rotulo = input.required<string>()`, renderiza `<span class="badge-secao">{{ rotulo() }}</span>`.
  - `CampoFichaComponent` — standalone, selector `bt-campo-ficha`, input `rotulo = input.required<string>()`, projeta conteúdo: `<div class="campo"><span class="campo__rotulo">{{ rotulo() }}</span><div class="campo__valor"><ng-content /></div></div>`.

- [ ] **Step 1: Testes que falham**

`badge-secao.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { BadgeSecaoComponent } from './badge-secao.component';

@Component({
  standalone: true,
  imports: [BadgeSecaoComponent],
  template: `<bt-badge-secao rotulo="PENTATEUCO" />`,
})
class HostComponent {}

it('mostra o rótulo da seção', () => {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  expect(fixture.nativeElement.textContent).toContain('PENTATEUCO');
});
```

`campo-ficha.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { CampoFichaComponent } from './campo-ficha.component';

@Component({
  standalone: true,
  imports: [CampoFichaComponent],
  template: `<bt-campo-ficha rotulo="Tema central">Deus criador</bt-campo-ficha>`,
})
class HostComponent {}

it('projeta rótulo e valor', () => {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const texto = fixture.nativeElement.textContent;
  expect(texto).toContain('Tema central');
  expect(texto).toContain('Deus criador');
});
```

- [ ] **Step 2: Ver falhar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: FAIL.

- [ ] **Step 3: `BadgeSecaoComponent`**

```ts
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-badge-secao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge-secao">{{ rotulo() }}</span>`,
  styleUrl: './badge-secao.component.scss',
})
export class BadgeSecaoComponent {
  readonly rotulo = input.required<string>();
}
```

Criar `badge-secao.component.scss` vazio (estilos entram na Task 18).

- [ ] **Step 4: `CampoFichaComponent`**

```ts
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-campo-ficha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="campo">
      <span class="campo__rotulo">{{ rotulo() }}</span>
      <div class="campo__valor"><ng-content /></div>
    </div>
  `,
  styleUrl: './campo-ficha.component.scss',
})
export class CampoFichaComponent {
  readonly rotulo = input.required<string>();
}
```

Criar `campo-ficha.component.scss` vazio.

- [ ] **Step 5: Ver passar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add frontend/biblia-teologica-web/src/app/shared
git commit -m "feat(web): componentes badge-secao e campo-ficha"
```

---

## Task 16: Frontend — feature `biblia`: rotas + shell `BibliaListaComponent`

**Files:**
- Create: `src/app/features/biblia/biblia.routes.ts`
- Create: `src/app/features/biblia/biblia-lista/biblia-lista.component.ts`
- Create: `src/app/features/biblia/biblia-lista/biblia-lista.component.html`
- Create: `src/app/features/biblia/biblia-lista/biblia-lista.component.scss` (vazio)
- Modify: `src/app/app.routes.ts`
- Test: `src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts`

**Interfaces:**
- Consumes: `LivroStore`, `BadgeSecaoComponent`, `RouterLink`, `RouterOutlet`.
- Produces:
  - `export const bibliaRoutes: Routes` — `''` → `BibliaListaComponent` com filhos: `':slug'` → `BibliaFichaComponent` (lazy `loadComponent`).
  - `BibliaListaComponent` — standalone, selector `bt-biblia-lista`. Chama `store.carregar()` no construtor. `computed` `secoes()` agrupa `store.livros()` por `secao` preservando ordem canônica. `busca = signal('')`; `livrosFiltrados` = `computed` filtrando por `nome` (case/acentos-insensitive via `.normalize`). Template: `<aside>` com input de busca + lista agrupada de `<a routerLink>`; `<main><router-outlet /></main>`.
  - Ao carregar, se a rota ativa não tem `:slug`, navega para o slug do primeiro livro (`effect` que observa `store.livros()` + `router.url`).

- [ ] **Step 1: Teste que falha**

`biblia-lista.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { BibliaListaComponent } from './biblia-lista.component';
import type { LivroListItem } from '../../../core/models/livro.model';

const item = (slug: string, ordem: number, secao = 'Pentateuco', rotulo = 'PENTATEUCO'): LivroListItem =>
  ({ slug, nome: slug, ordem, secao, secaoRotulo: rotulo });

describe('BibliaListaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BibliaListaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lista os livros retornados pela API na sidebar', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent.toLowerCase();
    expect(texto).toContain('genesis');
    expect(texto).toContain('exodo');
  });

  it('filtra pela busca', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([
      { ...item('genesis', 1), nome: 'GÊNESIS' },
      { ...item('exodo', 2), nome: 'ÊXODO' },
    ]);
    fixture.componentInstance.busca.set('genes');
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('GÊNESIS');
    expect(texto).not.toContain('ÊXODO');
  });
});
```

- [ ] **Step 2: Ver falhar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: FAIL.

- [ ] **Step 3: `app.routes.ts`**

```ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'biblia' },
  {
    path: 'biblia',
    loadChildren: () => import('./features/biblia/biblia.routes').then((m) => m.bibliaRoutes),
  },
  { path: '**', redirectTo: 'biblia' },
];
```

- [ ] **Step 4: `biblia.routes.ts`**

```ts
import { Routes } from '@angular/router';
import { BibliaListaComponent } from './biblia-lista/biblia-lista.component';

export const bibliaRoutes: Routes = [
  {
    path: '',
    component: BibliaListaComponent,
    children: [
      {
        path: ':slug',
        loadComponent: () =>
          import('./biblia-ficha/biblia-ficha.component').then((m) => m.BibliaFichaComponent),
      },
    ],
  },
];
```

- [ ] **Step 5: `BibliaListaComponent`**

`biblia-lista.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { BadgeSecaoComponent } from '../../../shared/components/badge-secao/badge-secao.component';
import { LivroStore } from '../../../core/services/livro.store';
import type { LivroListItem } from '../../../core/models/livro.model';

const ORDEM_SECOES = [
  'Pentateuco', 'LivrosHistoricos', 'LivrosPoeticos', 'ProfetasMaiores', 'ProfetasMenores',
  'Evangelhos', 'Atos', 'CartasPaulinas', 'CartasGerais', 'Apocalipse',
];

const semAcento = (s: string) =>
  s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();

@Component({
  selector: 'bt-biblia-lista',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterOutlet, BadgeSecaoComponent],
  templateUrl: './biblia-lista.component.html',
  styleUrl: './biblia-lista.component.scss',
})
export class BibliaListaComponent {
  private readonly store = inject(LivroStore);
  private readonly router = inject(Router);

  readonly busca = signal('');
  readonly carregando = this.store.carregando;

  readonly livrosFiltrados = computed(() => {
    const termo = semAcento(this.busca().trim());
    const livros = this.store.livros();
    return termo ? livros.filter((l) => semAcento(l.nome).includes(termo)) : livros;
  });

  readonly secoes = computed(() => {
    const grupos = new Map<string, { rotulo: string; livros: LivroListItem[] }>();
    for (const livro of this.livrosFiltrados()) {
      const grupo = grupos.get(livro.secao) ?? { rotulo: livro.secaoRotulo, livros: [] };
      grupo.livros.push(livro);
      grupos.set(livro.secao, grupo);
    }
    return ORDEM_SECOES
      .filter((s) => grupos.has(s))
      .map((s) => ({ chave: s, ...grupos.get(s)! }));
  });

  constructor() {
    this.store.carregar();

    effect(() => {
      const livros = this.store.livros();
      if (livros.length === 0) return;
      const temSlug = /\/biblia\/[^/]+$/.test(this.router.url);
      if (!temSlug) {
        void this.router.navigate(['/biblia', livros[0].slug]);
      }
    });
  }
}
```

`biblia-lista.component.html`:

```html
<div class="fichario">
  <aside class="sidebar">
    <header class="sidebar__brand">
      <h1>Fichário</h1>
      <p>os 66 livros · cânon protestante</p>
    </header>

    <input
      class="sidebar__busca"
      type="search"
      placeholder="Buscar livro…"
      autocomplete="off"
      [value]="busca()"
      (input)="busca.set($any($event.target).value)"
    />

    @if (carregando()) {
      <p class="sidebar__estado">Carregando…</p>
    }

    @for (secao of secoes(); track secao.chave) {
      <section class="grupo">
        <h2 class="grupo__titulo">
          {{ secao.rotulo }} <span class="grupo__contagem">{{ secao.livros.length }}</span>
        </h2>
        <ul class="grupo__lista">
          @for (livro of secao.livros; track livro.slug) {
            <li>
              <a
                class="item"
                [routerLink]="['/biblia', livro.slug]"
                routerLinkActive="item--ativo"
              >
                <span class="item__num">{{ livro.ordem.toString().padStart(2, '0') }}</span>
                <span class="item__nome">{{ livro.nome }}</span>
              </a>
            </li>
          }
        </ul>
      </section>
    }
  </aside>

  <main class="conteudo">
    <router-outlet />
  </main>
</div>
```

- [ ] **Step 6: Ver passar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add frontend/biblia-teologica-web/src/app/features frontend/biblia-teologica-web/src/app/app.routes.ts
git commit -m "feat(web): rotas lazy da bíblia + shell com sidebar e busca"
```

---

## Task 17: Frontend — `BibliaFichaComponent`

**Files:**
- Create: `src/app/features/biblia/biblia-ficha/biblia-ficha.component.ts`
- Create: `src/app/features/biblia/biblia-ficha/biblia-ficha.component.html`
- Create: `src/app/features/biblia/biblia-ficha/biblia-ficha.component.scss` (vazio)
- Test: `src/app/features/biblia/biblia-ficha/biblia-ficha.component.spec.ts`

**Interfaces:**
- Consumes: `ActivatedRoute` (`paramMap` → `slug`), `LivroService.obterPorSlug`, `LivroStore.vizinhos`, `CampoFichaComponent`, `BadgeSecaoComponent`, `RouterLink`.
- Produces:
  - `BibliaFichaComponent` — standalone, selector `bt-biblia-ficha`.
  - `slug = toSignal(route.paramMap.pipe(map(p => p.get('slug')!)))`.
  - `livro` = resource/signal populado a cada `slug` via `LivroService.obterPorSlug`; em erro 404 → `router.navigate(['/biblia'])`.
  - `vizinhos = computed(() => store.vizinhos(slug()))`.
  - Template: cabeçalho (`Nº XX / 66`, `<bt-badge-secao>`), título, grade de `<bt-campo-ficha>` (Quem/Estrutura/Porque/Pra quem), Tema, lista de Ideias, bloco Versículo, e nav ‹ anterior / próximo › com `routerLink`.

- [ ] **Step 1: Teste que falha**

`biblia-ficha.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { BibliaFichaComponent } from './biblia-ficha.component';

describe('BibliaFichaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BibliaFichaComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'genesis' })) } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('busca e renderiza a ficha do slug da rota', () => {
    const fixture = TestBed.createComponent(BibliaFichaComponent);
    fixture.detectChanges();

    http.expectOne('/api/livros/genesis').flush({
      slug: 'genesis', nome: 'GÊNESIS', ordem: 1, secao: 'Pentateuco', secaoRotulo: 'PENTATEUCO',
      testamento: 'Antigo', quem: 'Moisés', porque: 'Origem', paraQuem: 'Israel',
      estrutura: 'Duas partes', tema: 'Criação e aliança',
      ideias: ['Criação', 'Queda'], versiculo: 'Gênesis 1:1',
    });
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('GÊNESIS');
    expect(texto).toContain('Criação e aliança');
    expect(texto).toContain('Gênesis 1:1');
  });
});
```

- [ ] **Step 2: Ver falhar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: FAIL.

- [ ] **Step 3: `BibliaFichaComponent`**

`biblia-ficha.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { BadgeSecaoComponent } from '../../../shared/components/badge-secao/badge-secao.component';
import { CampoFichaComponent } from '../../../shared/components/campo-ficha/campo-ficha.component';
import { LivroService } from '../../../core/services/livro.service';
import { LivroStore } from '../../../core/services/livro.store';
import type { LivroDetail } from '../../../core/models/livro.model';

@Component({
  selector: 'bt-biblia-ficha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, BadgeSecaoComponent, CampoFichaComponent],
  templateUrl: './biblia-ficha.component.html',
  styleUrl: './biblia-ficha.component.scss',
})
export class BibliaFichaComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(LivroService);
  private readonly store = inject(LivroStore);

  readonly slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });

  readonly livro = signal<LivroDetail | null>(null);
  readonly vizinhos = computed(() => this.store.vizinhos(this.slug()));

  constructor() {
    this.store.carregar();
    // recarrega a ficha sempre que o slug da rota muda
    effect(() => this.carregar(this.slug()));
  }

  private carregar(slug: string): void {
    if (!slug) return;
    this.service.obterPorSlug(slug).subscribe({
      next: (l) => this.livro.set(l),
      error: () => void this.router.navigate(['/biblia']),
    });
  }
}
```

`biblia-ficha.component.html`:

```html
@if (livro(); as l) {
  <article class="ficha">
    <header class="ficha__cabecalho">
      <span class="ficha__num">Nº {{ l.ordem.toString().padStart(2, '0') }} / 66</span>
      <bt-badge-secao [rotulo]="l.secaoRotulo" />
    </header>

    <h2 class="ficha__titulo">{{ l.nome }}</h2>

    <div class="ficha__grade">
      <bt-campo-ficha rotulo="Quem escreveu">{{ l.quem }}</bt-campo-ficha>
      <bt-campo-ficha rotulo="Estrutura">{{ l.estrutura }}</bt-campo-ficha>
      <bt-campo-ficha rotulo="Porque escreveu">{{ l.porque }}</bt-campo-ficha>
      <bt-campo-ficha rotulo="Pra quem escreveu">{{ l.paraQuem }}</bt-campo-ficha>
    </div>

    <bt-campo-ficha rotulo="Tema central">{{ l.tema }}</bt-campo-ficha>

    <bt-campo-ficha rotulo="Ideias-chave">
      <ul class="ideias">
        @for (ideia of l.ideias; track ideia) {
          <li>{{ ideia }}</li>
        }
      </ul>
    </bt-campo-ficha>

    <bt-campo-ficha rotulo="Versículo de referência">
      <p class="versiculo">{{ l.versiculo }}</p>
    </bt-campo-ficha>

    <nav class="ficha__nav">
      @if (vizinhos().anterior; as ant) {
        <a [routerLink]="['/biblia', ant.slug]">‹ {{ ant.nome }}</a>
      }
      @if (vizinhos().proximo; as prox) {
        <a [routerLink]="['/biblia', prox.slug]">{{ prox.nome }} ›</a>
      }
    </nav>
  </article>
} @else {
  <p class="ficha__estado">Carregando ficha…</p>
}
```

- [ ] **Step 4: Ver passar**

Run: `npm test -- --watch=false --browsers=ChromeHeadless`
Expected: PASS.

- [ ] **Step 5: `npx tsc --noEmit` + build**

Run: `npx tsc --noEmit -p tsconfig.app.json && npm run build`
Expected: sem erros.

- [ ] **Step 6: Commit**

```bash
git add frontend/biblia-teologica-web/src/app/features/biblia/biblia-ficha
git commit -m "feat(web): ficha do livro com campos, ideias, versículo e navegação"
```

---

## Task 18: Frontend — porta o visual do protótipo para SCSS

**Files:**
- Create: `src/styles/_variables.scss`, `src/styles/_mixins.scss`
- Modify: `src/styles.scss` (importar fontes + parciais + estilos globais)
- Modify: `index.html` (`<link>` das fontes Google)
- Fill: `badge-secao.component.scss`, `campo-ficha.component.scss`, `biblia-lista.component.scss`, `biblia-ficha.component.scss`
- Modify: `src/app/app.ts` / `app.html` (o template raiz renderiza só `<router-outlet />`)

**Interfaces:**
- Consumes: classes usadas nos templates das Tasks 15–17 (`.fichario`, `.sidebar`, `.sidebar__brand`, `.sidebar__busca`, `.grupo`, `.grupo__titulo`, `.grupo__contagem`, `.grupo__lista`, `.item`, `.item--ativo`, `.item__num`, `.item__nome`, `.conteudo`, `.ficha`, `.ficha__cabecalho`, `.ficha__num`, `.ficha__titulo`, `.ficha__grade`, `.ficha__nav`, `.ficha__estado`, `.badge-secao`, `.campo`, `.campo__rotulo`, `.campo__valor`, `.ideias`, `.versiculo`).
- Produces: aparência fiel ao protótipo (`docs/prototipo-ficha-66-livros.html`) — paleta pergaminho, fontes Fraunces / Source Serif 4 / IBM Plex Mono, layout sidebar+cartão, responsivo em 900px.

> **REQUIRED SUB-SKILL:** Use `frontend-design` para esta task. O protótipo já define a direção estética (cores, tipografia, textura, o "cartão de fichário"); o trabalho é traduzir o CSS inline dele para SCSS organizado, não reinventar. Não introduzir biblioteca de UI.

- [ ] **Step 1: Ler o protótipo**

Abrir `docs/prototipo-ficha-66-livros.html` e extrair: variáveis CSS (`--ink`, `--parchment`, `--parchment-2`, `--brass`, `--sage`, `--near-black`, `--card`, `--line`), o `<link>` das fontes, e as regras de `.sidebar`, `.section-title`, `.book-item`, `.card`, `.card-tab`, `.card-title`, `.field`, `.field-label`, `.field-value`, `.ideias-list`, `.versiculo-block`, `.two-col`, e o media query de 900px.

- [ ] **Step 2: `src/styles/_variables.scss`**

```scss
// Paleta e tipografia — portadas de docs/prototipo-ficha-66-livros.html
$ink: #1c2541;
$parchment: #efe6d5;
$parchment-2: #e5dac3;
$brass: #a6763f;
$sage: #5c6b4f;
$near-black: #211d16;
$card: #f7f2e7;
$line: rgba(33, 29, 22, 0.16);

$fonte-serif: 'Source Serif 4', Georgia, serif;
$fonte-display: 'Fraunces', Georgia, serif;
$fonte-mono: 'IBM Plex Mono', ui-monospace, monospace;

$bp-mobile: 900px;
```

- [ ] **Step 3: `src/styles/_mixins.scss`**

```scss
@use 'variables' as *;

@mixin mobile {
  @media (max-width: $bp-mobile) {
    @content;
  }
}

@mixin rotulo-mono {
  font-family: $fonte-mono;
  font-size: 0.68rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
```

- [ ] **Step 4: `index.html`**

Adicionar no `<head>` (mesmos `<link>` do protótipo):

```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link
  href="https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,400;9..144,600;9..144,700&family=Source+Serif+4:ital,wght@0,400;0,600;1,400&family=IBM+Plex+Mono:wght@400;500;600&display=swap"
  rel="stylesheet"
/>
```

- [ ] **Step 5: `src/styles.scss`**

```scss
@use 'styles/variables' as *;

* { box-sizing: border-box; }

html, body { margin: 0; padding: 0; }

body {
  background: $parchment;
  background-image:
    radial-gradient(circle at 20% 10%, rgba(92, 107, 79, 0.05), transparent 40%),
    radial-gradient(circle at 90% 80%, rgba(166, 118, 63, 0.06), transparent 45%);
  color: $near-black;
  font-family: $fonte-serif;
  min-height: 100vh;
  -webkit-font-smoothing: antialiased;
}

:focus-visible { outline: 2px solid $brass; outline-offset: 2px; }

@media (prefers-reduced-motion: reduce) {
  * { transition: none !important; }
}
```

- [ ] **Step 6: Preencher os `.scss` dos componentes**

Traduzir as regras do protótipo, mapeando os seletores antigos para as classes BEM novas:

| Protótipo | Novo |
|---|---|
| `.sidebar` | `.sidebar` (em `biblia-lista.component.scss`) |
| `.brand` / `.brand-sub` | `.sidebar__brand h1` / `.sidebar__brand p` |
| `.search-input` | `.sidebar__busca` |
| `.section-title` / `.count` | `.grupo__titulo` / `.grupo__contagem` |
| `.book-item` / `.active` / `.num` | `.item` / `.item--ativo` / `.item__num` |
| `.card` | `.ficha` (em `biblia-ficha.component.scss`) |
| `.card-tab` / `.card-catno` / `.card-section` | `.ficha__cabecalho` / `.ficha__num` / (badge) |
| `.card-title` | `.ficha__titulo` |
| `.two-col` | `.ficha__grade` |
| `.field` / `.field-label` / `.field-value` | `.campo` / `.campo__rotulo` / `.campo__valor` (em `campo-ficha.component.scss`) |
| `.ideias-list` | `.ideias` |
| `.versiculo-block` | `.versiculo` |
| `.card-section` (pill) | `.badge-secao` (em `badge-secao.component.scss`) |

Cada `.scss` de componente começa com `@use '../../../styles/variables' as *;` (ajustar a profundidade `../` conforme a pasta) e `@use '../../../styles/mixins' as *;`. Usar `@include mobile { ... }` para o breakpoint de 900px (sidebar full-width, `.ficha__grade` de 1 coluna).

`app.html` (template raiz) deve conter **apenas**:

```html
<router-outlet />
```

- [ ] **Step 7: Verificar visualmente**

Run (dois terminais):
```bash
docker compose up --build api        # sobe só a API em :5080
cd frontend/biblia-teologica-web && npm start
```
Abrir `http://localhost:4200`. Conferir contra o protótipo: sidebar com busca e grupos, cartão de ficha com tipografia serif/display, navegação anterior/próximo, responsivo ao estreitar < 900px. Ajustar SCSS até bater.

- [ ] **Step 8: Testes + build**

Run: `npm test -- --watch=false --browsers=ChromeHeadless && npm run build`
Expected: PASS + build OK.

- [ ] **Step 9: Commit**

```bash
git add frontend/biblia-teologica-web/src
git commit -m "feat(web): visual do fichário portado para SCSS (paleta pergaminho, tipografia)"
```

---

## Task 19: Frontend — `Dockerfile`, nginx e `.dockerignore`

**Files:**
- Create: `frontend/biblia-teologica-web/Dockerfile`
- Create: `frontend/biblia-teologica-web/nginx.conf`
- Create: `frontend/biblia-teologica-web/.dockerignore`

**Interfaces:**
- Consumes: build Angular (`dist/biblia-teologica-web/browser`).
- Produces: imagem nginx na porta `80` servindo o SPA, com `/api/` encaminhado para `http://api:8080` (nome do serviço no compose) e fallback SPA (`try_files`).

- [ ] **Step 1: `.dockerignore`**

```gitignore
node_modules
dist
.angular
.git
*.spec.ts
coverage
```

- [ ] **Step 2: `nginx.conf`**

```nginx
server {
  listen 80;
  server_name _;
  root /usr/share/nginx/html;
  index index.html;

  location /api/ {
    proxy_pass http://api:8080;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }

  location / {
    try_files $uri $uri/ /index.html;
  }
}
```

- [ ] **Step 3: `Dockerfile`**

```dockerfile
FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/biblia-teologica-web/browser /usr/share/nginx/html
EXPOSE 80
```

- [ ] **Step 4: Build da imagem**

Run: `docker build -t biblia-web:dev frontend/biblia-teologica-web`
Expected: `Successfully tagged biblia-web:dev`.

- [ ] **Step 5: Commit**

```bash
git add frontend/biblia-teologica-web/Dockerfile frontend/biblia-teologica-web/nginx.conf frontend/biblia-teologica-web/.dockerignore
git commit -m "build(web): Dockerfile nginx com proxy /api e fallback SPA"
```

---

## Task 20: Orquestração — `docker-compose`

**Files:**
- Create: `docker-compose.yml`
- Create: `docker-compose.override.yml`
- Create: `docker-compose.prod.yml`

**Interfaces:**
- Consumes: `src/BibliaTeologica.Api/Dockerfile` (contexto = raiz), `frontend/biblia-teologica-web/Dockerfile`.
- Produces:
  - `docker compose up --build` → API em `localhost:5080`, web em `localhost:4200`, volume `biblia-data` para o `.db`.
  - `docker-compose.override.yml` (aplicado automaticamente) — `ASPNETCORE_ENVIRONMENT=Development`, Swagger acessível.
  - `docker-compose.prod.yml` — exemplo, **não** executado neste projeto.

- [ ] **Step 1: `docker-compose.yml`**

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/BibliaTeologica.Api/Dockerfile
    environment:
      ConnectionStrings__Default: "Data Source=/data/biblia.db"
      Cors__AllowedOrigins__0: "http://localhost:4200"
    volumes:
      - biblia-data:/data
    ports:
      - "5080:8080"

  web:
    build:
      context: ./frontend/biblia-teologica-web
    ports:
      - "4200:80"
    depends_on:
      - api

volumes:
  biblia-data:
```

- [ ] **Step 2: `docker-compose.override.yml`**

```yaml
services:
  api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
```

- [ ] **Step 3: `docker-compose.prod.yml`**

```yaml
# Exemplo de referência para deploy futuro. NÃO usado no M1.
# Uso: docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
services:
  api:
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__Default: "Data Source=/data/biblia.db"
      Cors__AllowedOrigins__0: "https://SEU-DOMINIO"
    restart: unless-stopped
    ports: !reset []
    expose:
      - "8080"

  web:
    restart: unless-stopped
    ports:
      - "80:80"
```

- [ ] **Step 4: Subir a stack completa e exercitar**

Run:
```bash
docker compose down -v
docker compose up --build -d
sleep 5
curl -fs http://localhost:5080/health && echo " API OK"
curl -fs http://localhost:5080/api/livros | grep -o '"slug":"genesis"' && echo " lista OK"
curl -fs http://localhost:4200/ | grep -qi "<app-root" && echo " web OK"
curl -fs http://localhost:4200/api/livros | grep -o '"slug":"apocalipse"' && echo " proxy web->api OK"
```
Expected: as 4 linhas `OK`.

Abrir `http://localhost:4200` no navegador e navegar por alguns livros (teste manual do utilizador — guardrail de Qualidade).

- [ ] **Step 5: `docker compose down`**

Run: `docker compose down`

- [ ] **Step 6: Commit**

```bash
git add docker-compose.yml docker-compose.override.yml docker-compose.prod.yml
git commit -m "build: docker-compose (api + web) + override de dev + exemplo de prod"
```

---

## Task 21: CI — GitHub Actions

**Files:**
- Create: `.github/workflows/ci.yml`
- Create: `.github/workflows/claude-code-review.yml`

**Interfaces:**
- Consumes: `BibliaTeologica.sln`, `frontend/biblia-teologica-web/package-lock.json`.
- Produces: em cada push/PR para `main` — job `backend` (build + test .NET) e job `frontend` (tsc + build + test). `claude-code-review.yml` roda o review automático por PR.

- [ ] **Step 1: `.github/workflows/ci.yml`**

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore BibliaTeologica.sln
      - run: dotnet build BibliaTeologica.sln --no-restore -c Release
      - run: dotnet test BibliaTeologica.sln --no-build -c Release --verbosity normal

  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: frontend/biblia-teologica-web
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '24'
          cache: npm
          cache-dependency-path: frontend/biblia-teologica-web/package-lock.json
      - run: npm ci
      - run: npx tsc --noEmit -p tsconfig.app.json
      - run: npm run build
      - run: npm test -- --watch=false --browsers=ChromeHeadless
```

- [ ] **Step 2: `.github/workflows/claude-code-review.yml`**

```yaml
name: Claude Code Review

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  review:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      pull-requests: write
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: anthropics/claude-code-action@v1
        with:
          anthropic_api_key: ${{ secrets.ANTHROPIC_API_KEY }}
          mode: review
```

> O secret `ANTHROPIC_API_KEY` precisa ser configurado nas settings do repositório no GitHub (Settings → Secrets and variables → Actions). Enquanto não estiver, o job falha só neste workflow — o `ci.yml` é independente. Se a action `anthropics/claude-code-action` tiver mudado de nome/versão, ajustar conforme a doc vigente; o essencial é: rodar review em PR.

- [ ] **Step 3: Validar YAML localmente**

Run: `docker run --rm -v "$PWD:/w" -w /w cytopia/yamllint .github/workflows/`
(ou qualquer linter de YAML disponível). Expected: sem erros de sintaxe.

- [ ] **Step 4: Commit**

```bash
git add .github/workflows
git commit -m "ci: workflow de build/test + code review automático por PR"
```

- [ ] **Step 5: Push e abrir PR (com o utilizador)**

Confirmar com o utilizador antes. Depois:

```bash
git push -u origin HEAD
```

Abrir PR para `main`, verificar os checks do `ci.yml` verdes.

---

## Riscos conhecidos e pontos de atenção para o executor

1. **Mapeamento de `Ideias` no EF Core (Task 7).** É o ponto mais provável de fricção. A abordagem: persistir o campo `_ideias` como coluna `TEXT` via `HasConversion` para JSON, e `builder.Ignore(l => l.Ideias)` na propriedade pública get-only. O teste `PersisteERecupera_Livro_ComIdeiasComoJson` valida. Se falhar, não trocar de estratégia às cegas — checar nome do campo e o `ValueComparer`.

2. **`Content` vs `None` para o `livros.json` (Task 8).** Precisa ser `<Content ... CopyToOutputDirectory>` para fluir aos projetos de teste. Se algum teste de seed falhar com "arquivo não encontrado", confirmar que `bin/.../Persistence/Seed/livros.json` existe no projeto de teste.

3. **Angular 22 — detecção de mudança.** O scaffold pode vir zoneless por padrão. Manter o que o `ng new` gerar em `app.config.ts` e só **adicionar** `provideHttpClient(withFetch())`. Se vier com zones, os testes e `effect()` continuam funcionando.

4. **`TreatWarningsAsErrors=true`.** Qualquer warning quebra o build. Os templates `dotnet new` geram código limpo depois de remover os arquivos de exemplo (Task 2). Se aparecer warning de nullable em teste, anotar `?`/`!` em vez de suprimir globalmente.

5. **Reflexão para despublicar em teste (Tasks 6 e 9).** `Publicado` tem setter privado — usar `GetSetMethod(nonPublic: true)!.Invoke(...)`, não `SetValue` (que exige setter acessível). Onde há `AppDbContext` (Task 11), preferir `db.Entry(livro).Property(...).CurrentValue = false`.

6. **Contexto de build do Dockerfile da API é a raiz do repo** (`docker build -f src/BibliaTeologica.Api/Dockerfile .`), por causa do `Directory.*.props` e das referências entre projetos. O `docker-compose.yml` já configura `context: .`.

7. **`claude-code-review.yml` precisa do secret `ANTHROPIC_API_KEY`** nas settings do repo no GitHub. Sem ele, só esse workflow falha; o `ci.yml` é independente. Ajustar nome/versão da action conforme a doc vigente se necessário.

8. **AutoMapper 13 + DI.** Se `services.AddAutoMapper(Assembly)` não resolver, adicionar `AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1 ao CPM e ao `.csproj` da Application.

9. **Ordem das tasks é sequencial** — cada uma assume as anteriores concluídas e no verde. Não pular. Rodar `dotnet test BibliaTeologica.sln` ao fim de cada task de backend e `npm test` ao fim de cada task de frontend.

## Cobertura do spec (auto-revisão)

| Seção do spec | Task(s) |
|---|---|
| §4 Estrutura da solução | 1, 2 (+ arquivos ao longo de todas) |
| §5 Domain (`Slug`, `SecaoCanonica`, `Testamento`, `DomainException`, `Livro`) | 3, 4, 5 |
| §6 Application (DTOs, `ILivroRepository`, `ILivroQueryService`, mapping, DI) | 6 |
| §7 Infrastructure (`AppDbContext`, `LivroConfiguration`, migração, `livros.json`, `LivroSeeder`) | 7, 8 |
| §7 `LivroRepository` + `AddInfrastructure` + regra de portabilidade | 9 (+ CLAUDE.md na 1) |
| §8 API (`Program.cs`, middlewares, rate limiter, CORS, health, Swagger, `appsettings.json`) | 10 |
| §8 `LivrosController` + testes de endpoint (200/404/429/headers) | 10, 11 |
| §8 `Dockerfile` da API | 12 |
| §9 Frontend (scaffold, models, service, `LivroStore`, shared, features, rotas, SCSS) | 13, 14, 15, 16, 17, 18 |
| §9 `Dockerfile` do frontend + `proxy.conf.json` + nginx | 13 (proxy), 19 |
| §10 Docker Compose (`yml` + override + prod) | 20 |
| §11 CI (`ci.yml` + `claude-code-review.yml`) | 21 |
| §12 Segurança — 10 pontos (guardrails) | 1 (regras de ouro), 6 (DTOs), 10 (rate limiter, headers, CORS, erros), 18 (sem `innerHTML`) |
| §13 Sequência de implementação | ordem das tasks 1–21 |
| §14 Decisões fechadas | Global Constraints + tasks |

---

**Fim do plano.**
