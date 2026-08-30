# M2 — Autenticação + Admin de Livros — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **Este plano entrega-se em DUAS PARTES / dois PRs.** Ver "Estrutura em duas partes" abaixo antes de arrancar. Executar a ordem do M2a → parar → PR/merge/sync → executar a ordem do M2b.

**Goal:** Adicionar autenticação (ASP.NET Core Identity + JWT em cookies HttpOnly com refresh rotativo e 2FA por email), uma área `/admin` (role `Admin`) com CRUD completo de Livros e upload de imagem de capa, e uma área `/conta` de perfil, sobre o site público do M1.

**Architecture:** Mantém o monólito .NET 8 em 4 camadas do M1. Identity e JWT são concern de Infrastructure, expostos à Application só por interfaces (`IIdentityService`, `ITokenService`, …) com inputs/outputs primitivos — `ApplicationUser` nunca sobe. `AuthController` trata cookies (concern HTTP); a decisão está em `AuthService`. `LivroCommandService` espelha o `LivroQueryService` do M1 no lado da escrita. Frontend Angular 22 ganha `@angular/material` **só** em `/admin` e nas páginas de auth.

**Tech Stack:** .NET 8, ASP.NET Core Identity, `Microsoft.AspNetCore.Authentication.JwtBearer`, EF Core 8 (Sqlite), `SixLabors.ImageSharp`, xUnit + Moq + AwesomeAssertions, Angular 22, Angular Material, Vitest, Docker Compose.

**Spec:** `docs/superpowers/specs/2026-08-30-m2-auth-admin-livros-design.md` — ler antes de começar. O plano argumenta a partir da spec; ambos viajam juntos.

## Global Constraints

- **.NET 8** apenas (`net8.0`). Nada de APIs .NET 9/10. Pacotes Identity/EF/Auth na linha `8.0.*`.
- **Central Package Management** — versões só em `Directory.Packages.props`; `.csproj` com `<PackageReference Include="..." />` **sem** `Version`.
- **`DateTime.UtcNow`**, nunca `.Now`, em Domain/Application.
- **Async ponta a ponta** com `CancellationToken` propagado (controllers → services → repos/stores).
- **Nunca engolir `Exception`** — logar via `ILogger` antes de tratar/relançar.
- **EF Core só-leitura usa `AsNoTracking()`**.
- **DTOs são o contrato da API** — nunca expor entidades, `ApplicationUser`, hash de password, refresh token no corpo, nem campos de auditoria desnecessários.
- **Mapeamento campo a campo** — nunca `_mapper.Map(dto, entity)` num update com auditoria/slug.
- **`[Authorize(Roles="Admin")]`** obrigatório em **todo** `/api/admin/**`. Guards Angular são só UX.
- **CORS** — política nomeada `spa`, origens de config; nunca `AllowAnyOrigin`.
- **Segredos** — `Jwt__SigningKey`, `AdminSeed__*`, SMTP: só env / `appsettings.Development.json` (git-ignored) / `.env` (git-ignored). `.env.example` commitado sem valores.
- **Cookies** — `bt_access` / `bt_refresh` HttpOnly + `Secure` + `SameSite=Strict`; `bt_csrf` não-HttpOnly. Refresh com path `/api/auth`, hash SHA-256 na BD, rotação + deteção de reutilização.
- **Testes backend** seguem `Metodo_ResultadoEsperado_QuandoCondicao`. xUnit + Moq + AwesomeAssertions (namespace `AwesomeAssertions`, API `.Should()`) + EF Core Sqlite real na Infra.
- **Testes frontend** — Vitest (`@angular/build:unit-test`, jsdom). Comando: `npm test` (uma passagem). O flag `--browsers=ChromeHeadless` **não existe** aqui.
- **Subscrições RxJS** em código novo: `takeUntilDestroyed(this.destroyRef)`.
- **Nenhum `innerHTML` de dado bruto**; nunca `bypassSecurityTrust*`.
- **Commits** — mensagem em português, Conventional Commits, terminando com as duas linhas de trailer do `CLAUDE.md` (`Co-Authored-By:` e `Claude-Session:`).
- **Git** — **M2a** na branch `feature/BT-2-m2a-auth-conta`; **M2b** na branch `feature/BT-<n>-m2b-admin-upload` criada de `main` **depois** do M2a mergeado. A spec (e este plano) já estão em `feature/BT-2-m2-auth-admin-livros`; fazer `git checkout -b feature/BT-2-m2a-auth-conta` a partir daí. Não fazer push nem abrir PR sem pedido explícito.
- **`Livro.Id` é `int`** (do M1). Só o Identity (`ApplicationUser`, roles, `RefreshToken`) usa `Guid`.
- **Comandos**:
  - `dotnet build BibliaTeologica.sln` · `dotnet test BibliaTeologica.sln`
  - `dotnet ef migrations add <Nome> --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api`
  - Frontend em `frontend/biblia-teologica-web/`: `npm ci` · `npm start` (`:4300`) · `npm run build` · `npm test` · `npx tsc --noEmit -p tsconfig.app.json`

---

## Estrutura em duas partes (M2a → PR → M2b)

O M2 vai a `main` em **dois PRs sequenciais**. O M2b assume o M2a já mergeado.
Ver `spec §3.1`. Executar por esta ordem:

### Parte M2a — Autenticação + Conta

Branch `feature/BT-2-m2a-auth-conta`. Ordem das tasks:

**2** (só contratos de auth/conta) → **3** (só Identity: pacotes Identity/JwtBearer, `ApplicationUser`, `RefreshToken`, options `Jwt`/`Email`/`AdminSeed`) → **4** (só `IdentityDbContext` + `RefreshTokenConfiguration` + migração **`M2a_Identity`**, sem `CapaImagemPath`) → **5** → **6** → **7** → **8** → **9** → **12** (só Identity/JwtBearer/DI dos serviços de auth) → **14** → **15** → **17** → **18** → **19** → **22** (só `.env` + `Jwt__SigningKey` + `AdminSeed`) → **23** → **24** → **25** → **26** → **27** → **30** (só `SECURITY.md`, `README` secção auth, `CLAUDE.md`, `ROADMAP` M2a).

**⇒ FIM DO M2a.** Verificação: `dotnet test BibliaTeologica.sln` + `npm test` + `npx tsc --noEmit` + `npm run build` verdes; `docker compose up` (com `.env`) e exercitar login → 2FA → `/conta`. Abrir PR de `feature/BT-2-m2a-auth-conta` → `main`, checks verdes, **merge squash**, `git checkout main && git pull`, `git checkout -b feature/BT-<n>-m2b-admin-upload`.

### Parte M2b — Admin + Upload

Branch `feature/BT-<n>-m2b-admin-upload` (de `main`, pós-merge do M2a). Ordem:

**1** (Domain: `Livro` mutação + `ConflitoException`) → **2** (só contratos de imagem/livro-escrita: `IProcessadorImagem`, `IArmazenamentoImagens`, `IUploadPolitica`, `ILivroRepository` estendido, DTOs de admin, `CapaUrl` nos DTOs públicos, `ILivroCommandService`) → **3** (só: pacote `SixLabors.ImageSharp`, options `Upload`/`Armazenamento`) → **4** (só: `LivroConfiguration` + `CapaImagemPath` + migração **`M2b_LivroCapa`**) → **10** → **11** → **12** (só: DI de `IProcessadorImagem`/`IArmazenamentoImagens`/`IUploadPolitica`) → **13** → **16** → **18** (só o Step do `ExceptionHandlingMiddleware`: `ConflitoException`→409, `KeyNotFoundException`→404, `UnauthorizedAccessException`→401 — se já não foi feito no M2a, fazer aqui) → **20** → **21** → **22** (só: volume `biblia-media`, `chown` no Dockerfile, `docker-compose.prod.yml` SMTP) → **28** → **29** → **30** (só: `README` secção admin, `ROADMAP` M2b).

**⇒ FIM DO M2b.** Verificação completa + `docker compose up` e exercitar login → `/admin/livros` → editar → upload de capa → ver no site público. PR → `main`.

### Tasks que atravessam as duas partes

| Task | M2a faz | M2b faz |
|---|---|---|
| **2** (contratos) | interfaces/DTOs de auth+conta, tipos de resultado, `IIdentityService`, `ITokenService`, `IRefreshTokenStore`, `IEmailSender`, `IUsuarioAtual` | `IProcessadorImagem`, `IArmazenamentoImagens`, `IUploadPolitica`, `ILivroRepository` estendido, DTOs de admin, `CapaUrl` nos DTOs públicos, `ILivroCommandService` (interface) |
| **3** (pacotes+entidades) | `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `...JwtBearer`, `System.IdentityModel.Tokens.Jwt`; `ApplicationUser`, `RefreshToken`; options `Jwt`/`Email`/`AdminSeed` | `SixLabors.ImageSharp`; options `Upload`/`Armazenamento` |
| **4** (DbContext+migração) | `AppDbContext : IdentityDbContext`, `RefreshTokenConfiguration`, migração `M2a_Identity`, testes de `RefreshToken` | `LivroConfiguration` + `CapaImagemPath`, migração `M2b_LivroCapa`, teste de `CapaImagemPath` |
| **12** (`AddInfrastructure`) | `Configure` de `Jwt`/`Email`; `AddDataProtection`; `AddIdentityCore`/`AddJwtBearer`; DI de `IIdentityService`/`ITokenService`/`IRefreshTokenStore`/`IEmailSender`/`IdentitySeeder` | DI de `IProcessadorImagem`/`IArmazenamentoImagens`/`IUploadPolitica`; `Configure` de `Upload`/`Armazenamento` |
| **18** (`Program.cs`+middleware) | `AuthController`, cookies, rate limit `auth`, `UseAuthentication/Authorization`, seed do `IdentitySeeder`, `WebFactoryBase`+`AutenticacaoWebFactory`; **e** já adicionar os `catch` de `ConflitoException`/`KeyNotFoundException`/`UnauthorizedAccessException` ao middleware (inócuos sem os endpoints) | nada novo (o middleware já cobre) — a não ser que o M2a não os tenha feito |
| **22** (Docker) | `.gitignore` (`.env`), `.env.example` (`BT_JWT_KEY`, `BT_ADMIN_*`), `docker-compose.yml` env de auth | `.gitignore` (`.media/`), volume `biblia-media`, `chown` no Dockerfile, `docker-compose.prod.yml` SMTP, exercício manual do upload |
| **30** (Docs) | `SECURITY.md`, `README` (quick start auth, `.env`, `appsettings.Development.json`), `CLAUDE.md` (auth), `ROADMAP` (M2a entregue) | `README` (secção `/admin`, upload), `ROADMAP` (M2b entregue) |

> **Nota sobre a Task 2 no M2b:** os DTOs públicos `LivroListItemDto`/`LivroDetailDto`
> ganham `CapaUrl` só no M2b. No M2a **não se toca** nesses records nem no `LivroMapper` —
> ficam como o M1. Os Steps 12, 14, 15 da Task 2 (que adicionam `CapaUrl` e ajustam o mapper)
> pertencem ao M2b.

> **Nota sobre a Task 30 no M2a:** o `SECURITY.md` do M2a descreve a autenticação como
> **existente** mas menciona que o CRUD de admin e o upload chegam no M2b.

---

## Mapa de ficheiros

**Domain** (`src/BibliaTeologica.Domain/`)
- `Entities/Livro.cs` — + `Atualizar`, `Publicar`, `Despublicar`, `DefinirCapa`, `RemoverCapa`, campo `CapaImagemPath`
- `Exceptions/ConflitoException.cs` — nova

**Application** (`src/BibliaTeologica.Application/`)
- `Common/Interfaces/ILivroRepository.cs` — estendido
- `Common/Interfaces/{IIdentityService,ITokenService,IRefreshTokenStore,IEmailSender,IProcessadorImagem,IArmazenamentoImagens,IUsuarioAtual}.cs` — novas
- `Common/Resultados/{ResultadoSenha,IdentityResultDto,RefreshTokenInfo}.cs` — novas
- `Auth/{IAuthService,AuthService}.cs` · `Auth/Dtos/*` · `Auth/DesafioLoginProtegido.cs`
- `Conta/{IContaService,ContaService}.cs` · `Conta/Dtos/*`
- `Livros/{ILivroCommandService,LivroCommandService}.cs` · `Livros/Dtos/{CriarLivroDto,AtualizarLivroDto,LivroAdminListItemDto,LivroAdminDetailDto}.cs`
- `Livros/Dtos/LivroDetailDto.cs`, `LivroListItemDto.cs` — + `CapaUrl`
- `Common/Mapping/LivroMapper.cs` — + admin mappers, + `CapaUrl`
- `DependencyInjection.cs` — + serviços novos

**Infrastructure** (`src/BibliaTeologica.Infrastructure/`)
- `Identity/{ApplicationUser,RefreshToken,IdentityService,TokenService,RefreshTokenStore}.cs`
- `Persistence/AppDbContext.cs` — passa a `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- `Persistence/Configurations/{LivroConfiguration,RefreshTokenConfiguration}.cs`
- `Persistence/Migrations/*` — `M2_IdentityELivroCapa`
- `Persistence/Seed/IdentitySeeder.cs`
- `Email/{IEmailSenderSink,LogEmailSender,SmtpEmailSender}.cs`
- `Imagens/{ProcessadorImagemSharp,ArmazenamentoImagensLocal}.cs`
- `Repositories/LivroRepository.cs` — estendido
- `Configuracao/{JwtOptions,EmailOptions,UploadOptions,ArmazenamentoOptions,AdminSeedOptions}.cs`
- `DependencyInjection.cs` — + Identity, JwtBearer, DataProtection, serviços

**Api** (`src/BibliaTeologica.Api/`)
- `Controllers/{AuthController,ContaController,LivrosAdminController,MediaController}.cs`
- `Controllers/LivrosController.cs` — inalterado (DTO ganha `CapaUrl` a montante)
- `Auth/{CookieWriter,CsrfActionFilter,UsuarioAtual}.cs`
- `Middleware/ExceptionHandlingMiddleware.cs` — + `ConflitoException`, `UnauthorizedAccessException`
- `Program.cs` — auth, rate limit `auth`, filtro CSRF

**Tests**
- `tests/BibliaTeologica.Domain.Tests/Entities/LivroTests.cs` — + casos M2
- `tests/BibliaTeologica.Application.Tests/{Auth,Conta,Livros}/*`
- `tests/BibliaTeologica.Infrastructure.Tests/{Identity,Email,Imagens,Persistence}/*`
- `tests/BibliaTeologica.Api.Tests/{Auth,Conta,Admin}/*` — + `AutenticacaoWebFactory`

**Frontend** (`frontend/biblia-teologica-web/`)
- `src/app/core/auth/{auth.models.ts,auth.store.ts,auth.service.ts}`
- `src/app/core/auth/guards/{auth.guard.ts,admin.guard.ts}`
- `src/app/core/interceptors/{with-credentials.interceptor.ts,csrf.interceptor.ts,refresh.interceptor.ts}`
- `src/app/core/services/livros-admin.service.ts`
- `src/app/features/auth/{entrar,registar}/*`
- `src/app/features/conta/{perfil,trocar-senha}/*` · `conta.routes.ts`
- `src/app/features/admin/{admin-shell,livros-lista,livro-form}/*` · `admin.routes.ts`
- `src/app/app.config.ts`, `src/app/app.routes.ts` — + interceptors, rotas lazy
- `package.json`, `angular.json` — `@angular/material`

**Raiz**
- `.env.example` · `.gitignore` (+ `.env`, `.media/`) · `docker-compose.yml` · `docker-compose.prod.yml`
- `src/BibliaTeologica.Api/Dockerfile` — + `mkdir`/`chown` de `/data/media`
- `Directory.Packages.props` — pacotes novos
- `SECURITY.md`, `README.md`, `CLAUDE.md`, `docs/ROADMAP.md`

---

## Task 1: Domain — comportamento de `Livro` + `ConflitoException`

> **Parte: M2b.** (A mutação de `Livro` só é usada pelo `LivroCommandService`, que é M2b.)

**Files:**
- Modify: `src/BibliaTeologica.Domain/Entities/Livro.cs`
- Create: `src/BibliaTeologica.Domain/Exceptions/ConflitoException.cs`
- Test: `tests/BibliaTeologica.Domain.Tests/Entities/LivroTests.cs` (existe — adicionar casos)

**Interfaces:**
- Consumes: `Slug.From` (M1), `SecaoCanonica`, `DomainException` (M1).
- Produces:
  - `class ConflitoException : Exception` — ctor `(string message)`.
  - `Livro.Atualizar(string nome, SecaoCanonica secao, int ordem, string quem, string porque, string paraQuem, string estrutura, string tema, IEnumerable<string> ideias, string versiculo)` — revalida os mesmos invariantes de `Create`, regenera `Slug`, `AtualizadoEm = UtcNow`.
  - `Livro.Publicar()` / `Livro.Despublicar()` — alternam `Publicado`, `AtualizadoEm = UtcNow`.
  - `Livro.DefinirCapa(string caminho)` — `CapaImagemPath = caminho`, `AtualizadoEm = UtcNow`. Lança `DomainException` se `caminho` vazio.
  - `Livro.RemoverCapa()` — `CapaImagemPath = null`, `AtualizadoEm = UtcNow`.
  - `Livro.CapaImagemPath` — `string?`, get público / set privado.

- [ ] **Step 1: `ConflitoException`**

Criar `src/BibliaTeologica.Domain/Exceptions/ConflitoException.cs`:

```csharp
namespace BibliaTeologica.Domain.Exceptions;

public class ConflitoException : Exception
{
    public ConflitoException(string message) : base(message) { }
}
```

- [ ] **Step 2: Testes que falham**

Abrir `tests/BibliaTeologica.Domain.Tests/Entities/LivroTests.cs`. Já existe um helper `LivroValido(...)` (do M1). Adicionar, dentro da classe `LivroTests`:

```csharp
    [Fact]
    public void Atualizar_AlteraCamposERegeneraSlug_QuandoNomeMuda()
    {
        var livro = LivroValido(); // GÊNESIS, ordem 1

        livro.Atualizar("ÊXODO", SecaoCanonica.Pentateuco, 2,
            quem: "Moisés", porque: "Libertação", paraQuem: "Israel",
            estrutura: "Duas partes", tema: "Redenção",
            ideias: new[] { "Pragas", "Páscoa" }, versiculo: "Êxodo 3:14");

        livro.Slug.Should().Be("exodo");
        livro.Nome.Should().Be("ÊXODO");
        livro.Ordem.Should().Be(2);
        livro.Ideias.Should().Equal("Pragas", "Páscoa");
        livro.AtualizadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_LancaDomainException_QuandoCampoObrigatorioVazio(string vazio)
    {
        var livro = LivroValido();
        var acao = () => livro.Atualizar(vazio, SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Atualizar_LancaDomainException_QuandoIdeiasVazia()
    {
        var livro = LivroValido();
        var acao = () => livro.Atualizar("GÊNESIS", SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", Array.Empty<string>(), "v");
        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void PublicarDespublicar_AlternamFlagETocamAtualizadoEm()
    {
        var livro = LivroValido();

        livro.Despublicar();
        livro.Publicado.Should().BeFalse();

        livro.Publicar();
        livro.Publicado.Should().BeTrue();
        livro.AtualizadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DefinirCapa_RemoverCapa_GerenciamCampo()
    {
        var livro = LivroValido();

        livro.DefinirCapa("a1b2c3d4-e5f6-4a1b-9c2d-000000000000.webp");
        livro.CapaImagemPath.Should().Be("a1b2c3d4-e5f6-4a1b-9c2d-000000000000.webp");

        livro.RemoverCapa();
        livro.CapaImagemPath.Should().BeNull();
    }

    [Fact]
    public void DefinirCapa_LancaDomainException_QuandoCaminhoVazio()
    {
        var livro = LivroValido();
        var acao = () => livro.DefinirCapa("  ");
        acao.Should().Throw<DomainException>();
    }
```

- [ ] **Step 3: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~LivroTests"`
Expected: FAIL — `Atualizar`/`Publicar`/`DefinirCapa`/`CapaImagemPath` não existem.

- [ ] **Step 4: Implementar em `Livro.cs`**

Refatorar a validação para um método privado partilhado e adicionar os métodos. Substituir o corpo da classe `Livro` por (mantendo o ctor privado sem-args e o ctor privado com-args do M1):

```csharp
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
    public string? CapaImagemPath { get; private set; }
    public bool Publicado { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public Testamento Testamento => Secao.Testamento();

    public static Livro Create(string nome, SecaoCanonica secao, int ordem, string quem,
        string porque, string paraQuem, string estrutura, string tema,
        IEnumerable<string> ideias, string versiculo)
    {
        var ideiasValidas = ValidarCampos(nome, secao, ordem, quem, porque, paraQuem,
            estrutura, tema, ideias, versiculo);

        return new Livro(nome, secao, ordem, quem, porque, paraQuem, estrutura, tema,
            ideiasValidas, versiculo);
    }

    public void Atualizar(string nome, SecaoCanonica secao, int ordem, string quem,
        string porque, string paraQuem, string estrutura, string tema,
        IEnumerable<string> ideias, string versiculo)
    {
        var ideiasValidas = ValidarCampos(nome, secao, ordem, quem, porque, paraQuem,
            estrutura, tema, ideias, versiculo);

        Nome = nome;
        Slug = Slugs.Slug.From(nome);
        Secao = secao;
        Ordem = ordem;
        Quem = quem;
        Porque = porque;
        ParaQuem = paraQuem;
        Estrutura = estrutura;
        Tema = tema;
        _ideias.Clear();
        _ideias.AddRange(ideiasValidas);
        Versiculo = versiculo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Publicar()
    {
        Publicado = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Despublicar()
    {
        Publicado = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void DefinirCapa(string caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            throw new DomainException("Caminho da capa não pode ser vazio.");
        CapaImagemPath = caminho.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void RemoverCapa()
    {
        CapaImagemPath = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static List<string> ValidarCampos(string nome, SecaoCanonica secao, int ordem,
        string quem, string porque, string paraQuem, string estrutura, string tema,
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

        return ideiasValidas;

        static void Requer(string valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new DomainException($"Campo obrigatório ausente: {campo}.");
        }
    }
```

> Nota: o ctor privado `Livro(string nome, …)` do M1 fica igual, mas remove a linha
> `_ideias.AddRange(ideias);`? Não — mantém. O ctor recebe `ideiasValidas` já filtradas.
> Confirmar que o ctor privado continua a fazer `_ideias.AddRange(ideias);` e
> `Publicado = true;` e `CriadoEm = AtualizadoEm = DateTime.UtcNow;`.

- [ ] **Step 5: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Domain.Tests`
Expected: PASS (todos, incl. os do M1).

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Domain tests/BibliaTeologica.Domain.Tests/Entities/LivroTests.cs
git commit -m "feat(domain): mutação de Livro (Atualizar/Publicar/Capa) e ConflitoException"
```

---

## Task 2: Application — contratos (interfaces, DTOs, tipos auxiliares)

> **Parte: M2a + M2b (dividida).** M2a: Steps 1–10 + 13 (auth/conta + tipos de resultado + `.csproj`). M2b: Steps 6 (`IProcessadorImagem`), o `IArmazenamentoImagens`, `IUploadPolitica` (definido na Task 16), `ILivroRepository` estendido (Step 8), DTOs de admin (Step 11), e `CapaUrl` nos DTOs públicos + ajuste do `LivroMapper` (Steps 12, 14, 15). **No M2a NÃO tocar em `LivroListItemDto`/`LivroDetailDto` nem no `LivroMapper`.**

**Files:**
- Create: `src/BibliaTeologica.Application/Common/Resultados/ResultadoSenha.cs`
- Create: `src/BibliaTeologica.Application/Common/Resultados/IdentityResultDto.cs`
- Create: `src/BibliaTeologica.Application/Common/Resultados/RefreshTokenInfo.cs`
- Create: `src/BibliaTeologica.Application/Common/Interfaces/{IIdentityService,ITokenService,IRefreshTokenStore,IEmailSender,IProcessadorImagem,IArmazenamentoImagens,IUsuarioAtual}.cs`
- Modify: `src/BibliaTeologica.Application/Common/Interfaces/ILivroRepository.cs`
- Create: `src/BibliaTeologica.Application/Auth/Dtos/{LoginDto,Verificar2faDto,RegistarMembroDto,UsuarioDto,ResultadoLoginDto,TokensGerados}.cs`
- Create: `src/BibliaTeologica.Application/Conta/Dtos/{PerfilDto,AtualizarPerfilDto,TrocarEmailDto,TrocarSenhaDto}.cs`
- Create: `src/BibliaTeologica.Application/Livros/Dtos/{CriarLivroDto,AtualizarLivroDto,LivroAdminListItemDto,LivroAdminDetailDto}.cs`
- Modify: `src/BibliaTeologica.Application/Livros/Dtos/{LivroDetailDto,LivroListItemDto}.cs` (+ `CapaUrl`)
- Modify: `src/BibliaTeologica.Application/BibliaTeologica.Application.csproj` (+ `Microsoft.Extensions.Logging.Abstractions`)

**Interfaces:**
- Consumes: `Livro`, `SecaoCanonica` (Domain).
- Produces: todas as assinaturas abaixo. **Não há implementações nesta task** — só contratos; a solução compila.

- [ ] **Step 1: Tipos auxiliares**

`Common/Resultados/ResultadoSenha.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Resultados;

public record ResultadoSenha(bool Sucesso, bool Bloqueado, Guid? UserId);
```

`Common/Resultados/IdentityResultDto.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Resultados;

public record IdentityResultDto(bool Sucesso, IReadOnlyList<string> Erros)
{
    public static IdentityResultDto Ok() => new(true, Array.Empty<string>());
    public static IdentityResultDto Falha(params string[] erros) => new(false, erros);
}
```

`Common/Resultados/RefreshTokenInfo.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Resultados;

public record RefreshTokenInfo(Guid UserId, string Hash, DateTime ExpiraEm, DateTime? RevogadoEm)
{
    public bool Valido => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;
}
```

- [ ] **Step 2: `ITokenService`**

`Common/Interfaces/ITokenService.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface ITokenService
{
    string GerarAccessToken(Guid userId, string email, string nome,
        IEnumerable<string> roles, bool deveTrocarSenha);

    (string tokenCru, string hash, DateTime expiraEm) GerarRefreshToken();

    string Hash(string tokenCru);

    /// <summary>Token curto assinado que prova que o passo 1 do login passou.</summary>
    string GerarDesafioLogin(Guid userId);

    /// <summary>Devolve o userId se o desafio for válido e não expirado; senão null.</summary>
    Guid? ValidarDesafioLogin(string token);
}
```

- [ ] **Step 3: `IRefreshTokenStore`**

`Common/Interfaces/IRefreshTokenStore.cs`:

```csharp
using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task GuardarAsync(Guid userId, string hash, DateTime expiraEm, string? ip, CancellationToken ct);

    Task<RefreshTokenInfo?> ObterAsync(string hash, CancellationToken ct);

    /// <summary>Revoga o hash antigo, encadeia-o ao novo e guarda o novo.</summary>
    Task RotacionarAsync(string hashAntigo, Guid userId, string hashNovo, DateTime expira,
        string? ip, CancellationToken ct);

    Task RevogarAsync(string hash, CancellationToken ct);

    Task RevogarCadeiaAsync(Guid userId, CancellationToken ct);
}
```

- [ ] **Step 4: `IIdentityService`**

`Common/Interfaces/IIdentityService.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure, CancellationToken ct);
    Task<Guid?> ResolverIdPorEmailAsync(string email, CancellationToken ct);
    Task<string> GerarTokenEmailAsync(Guid userId, CancellationToken ct);
    Task<bool> VerificarTokenEmailAsync(Guid userId, string codigo, CancellationToken ct);
    Task RegistarFalhaAsync(Guid userId, CancellationToken ct);
    Task ResetarFalhasAsync(Guid userId, CancellationToken ct);
    Task<UsuarioDto?> ObterUsuarioAsync(Guid userId, CancellationToken ct);
    Task<string?> ObterEmailAsync(Guid userId, CancellationToken ct);
    Task<DateTime?> ObterCriadoEmAsync(Guid userId, CancellationToken ct);
    Task<IdentityResultDto> CriarMembroAsync(string email, string senha, string nomeExibicao, CancellationToken ct);
    Task<IdentityResultDto> TrocarSenhaAsync(Guid userId, string atual, string nova, CancellationToken ct);
    Task<IdentityResultDto> TrocarEmailAsync(Guid userId, string novoEmail, string senha, CancellationToken ct);
    Task AtualizarNomeAsync(Guid userId, string nomeExibicao, CancellationToken ct);
    Task LimparDeveTrocarSenhaAsync(Guid userId, CancellationToken ct);
}
```

- [ ] **Step 5: `IEmailSender`**

`Common/Interfaces/IEmailSender.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface IEmailSender
{
    Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct);
}
```

- [ ] **Step 6: `IProcessadorImagem` + `IArmazenamentoImagens`**

`Common/Interfaces/IProcessadorImagem.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface IProcessadorImagem
{
    // byte[] e não ReadOnlySpan — o Moq não consegue mockar ref struct como argumento.
    bool AssinaturaValida(byte[] cabecalho, string contentType);

    /// <summary>Re-codifica para WebP, redimensiona até dimensaoMaxPx (lado maior),
    /// descarta metadados. Devolve um stream posicionado no início.</summary>
    Task<Stream> NormalizarAsync(Stream entrada, int dimensaoMaxPx, CancellationToken ct);
}
```

`Common/Interfaces/IArmazenamentoImagens.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface IArmazenamentoImagens
{
    /// <summary>Grava o stream (já WebP) e devolve a chave "{guid}.webp".</summary>
    Task<string> GuardarAsync(Stream webp, CancellationToken ct);

    Task RemoverAsync(string chave, CancellationToken ct);

    /// <summary>Abre para leitura, ou null se a chave for inválida / não existir.</summary>
    Task<Stream?> AbrirAsync(string chave, CancellationToken ct);
}
```

- [ ] **Step 7: `IUsuarioAtual`**

`Common/Interfaces/IUsuarioAtual.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface IUsuarioAtual
{
    Guid? Id { get; }
    string? Email { get; }
    bool EstaAutenticado { get; }
    bool EhAdmin { get; }
}
```

- [ ] **Step 8: `ILivroRepository` estendido**

Substituir `Common/Interfaces/ILivroRepository.cs`:

```csharp
using BibliaTeologica.Domain.Entities;

namespace BibliaTeologica.Application.Common.Interfaces;

public interface ILivroRepository
{
    // M1
    Task<IReadOnlyList<Livro>> ListarPublicadosAsync(CancellationToken ct);
    Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct);

    // M2
    Task<IReadOnlyList<Livro>> ListarTodosAsync(CancellationToken ct);
    Task<Livro?> ObterPorIdAsync(int id, CancellationToken ct);
    Task AdicionarAsync(Livro livro, CancellationToken ct);
    void Remover(Livro livro);
    Task<bool> SlugExisteAsync(string slug, int? exceptoId, CancellationToken ct);
    Task<bool> OrdemExisteAsync(int ordem, int? exceptoId, CancellationToken ct);
    Task SalvarAsync(CancellationToken ct);
}
```

- [ ] **Step 9: DTOs de Auth**

`Auth/Dtos/LoginDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record LoginDto(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Senha);
```

`Auth/Dtos/Verificar2faDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record Verificar2faDto(
    [property: Required] string TokenDesafio,
    [property: Required] string Codigo);
```

`Auth/Dtos/RegistarMembroDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Auth.Dtos;

public record RegistarMembroDto(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Senha,
    [property: Required, MaxLength(80)] string NomeExibicao);
```

`Auth/Dtos/UsuarioDto.cs`:

```csharp
namespace BibliaTeologica.Application.Auth.Dtos;

public record UsuarioDto(string Email, string NomeExibicao, IReadOnlyList<string> Roles, bool DeveTrocarSenha);
```

`Auth/Dtos/TokensGerados.cs` (interno — nunca serializado para o cliente):

```csharp
namespace BibliaTeologica.Application.Auth.Dtos;

public record TokensGerados(
    string AccessToken, DateTime AccessExpiraEm,
    string RefreshToken, DateTime RefreshExpiraEm,
    string Csrf);
```

`Auth/Dtos/ResultadoLoginDto.cs`:

```csharp
namespace BibliaTeologica.Application.Auth.Dtos;

public enum EstadoLogin { CredenciaisInvalidas, Bloqueado, Desafio2fa, Autenticado }

public record ResultadoLoginDto(
    EstadoLogin Estado,
    string? TokenDesafio = null,
    UsuarioDto? Usuario = null,
    TokensGerados? Tokens = null,
    IReadOnlyList<string>? Erros = null);
```

- [ ] **Step 10: DTOs de Conta**

`Conta/Dtos/PerfilDto.cs`:

```csharp
namespace BibliaTeologica.Application.Conta.Dtos;

public record PerfilDto(string Email, string NomeExibicao, IReadOnlyList<string> Roles, DateTime CriadoEm);
```

`Conta/Dtos/AtualizarPerfilDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record AtualizarPerfilDto([property: Required, MaxLength(80)] string NomeExibicao);
```

`Conta/Dtos/TrocarEmailDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarEmailDto(
    [property: Required, EmailAddress] string NovoEmail,
    [property: Required] string Senha);
```

`Conta/Dtos/TrocarSenhaDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Conta.Dtos;

public record TrocarSenhaDto(
    [property: Required] string SenhaAtual,
    [property: Required, MinLength(8)] string NovaSenha);
```

- [ ] **Step 11: DTOs de Livros (escrita + admin)**

`Livros/Dtos/CriarLivroDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Livros.Dtos;

public record CriarLivroDto(
    [property: Required, MaxLength(120)] string Nome,
    [property: Required] string Secao,
    [property: Range(1, 66)] int Ordem,
    [property: Required] string Quem,
    [property: Required] string Porque,
    [property: Required] string ParaQuem,
    [property: Required] string Estrutura,
    [property: Required] string Tema,
    [property: Required, MinLength(1)] IReadOnlyList<string> Ideias,
    [property: Required] string Versiculo);
```

`Livros/Dtos/AtualizarLivroDto.cs` — idêntico:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BibliaTeologica.Application.Livros.Dtos;

public record AtualizarLivroDto(
    [property: Required, MaxLength(120)] string Nome,
    [property: Required] string Secao,
    [property: Range(1, 66)] int Ordem,
    [property: Required] string Quem,
    [property: Required] string Porque,
    [property: Required] string ParaQuem,
    [property: Required] string Estrutura,
    [property: Required] string Tema,
    [property: Required, MinLength(1)] IReadOnlyList<string> Ideias,
    [property: Required] string Versiculo);
```

`Livros/Dtos/LivroAdminListItemDto.cs`:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroAdminListItemDto(
    int Id, string Slug, string Nome, int Ordem, string Secao,
    bool Publicado, bool TemCapa, DateTime AtualizadoEm);
```

`Livros/Dtos/LivroAdminDetailDto.cs`:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroAdminDetailDto(
    int Id, string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo,
    string Quem, string Porque, string ParaQuem, string Estrutura, string Tema,
    IReadOnlyList<string> Ideias, string Versiculo, bool Publicado, string? CapaUrl,
    DateTime CriadoEm, DateTime AtualizadoEm);
```

- [ ] **Step 12: `LivroDetailDto` / `LivroListItemDto` ganham `CapaUrl`**

`Livros/Dtos/LivroListItemDto.cs`:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroListItemDto(string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo, string? CapaUrl);
```

`Livros/Dtos/LivroDetailDto.cs` — adicionar `string? CapaUrl` como último parâmetro:

```csharp
namespace BibliaTeologica.Application.Livros.Dtos;

public record LivroDetailDto(
    string Slug, string Nome, int Ordem, string Secao, string SecaoRotulo, string Testamento,
    string Quem, string Porque, string ParaQuem, string Estrutura, string Tema,
    IReadOnlyList<string> Ideias, string Versiculo, string? CapaUrl);
```

- [ ] **Step 13: `.csproj` da Application**

Adicionar a `src/BibliaTeologica.Application/BibliaTeologica.Application.csproj`, no `<ItemGroup>` de pacotes (criar se não existir):

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
```

(`Microsoft.Extensions.Logging.Abstractions` já está no `Directory.Packages.props`.)

- [ ] **Step 14: Build**

Run: `dotnet build BibliaTeologica.sln`
Expected: **FALHA de compilação** em `LivroMapper.ToListItem`/`ToDetail` e `LivroQueryService` — os records ganharam um parâmetro `CapaUrl`. Corrigir no Step 15.

- [ ] **Step 15: Ajustar `LivroMapper` e `LivroQueryService` para o novo parâmetro**

Em `src/BibliaTeologica.Application/Common/Mapping/LivroMapper.cs`, os métodos `ToListItem` e `ToDetail` passam `CapaUrl: null` por agora (a montagem do URL vem na Task 21):

```csharp
public static LivroListItemDto ToListItem(Livro l) =>
    new(l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo(), CapaUrl: null);

public static LivroDetailDto ToDetail(Livro l) =>
    new(l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo(), l.Testamento.ToString(),
        l.Quem, l.Porque, l.ParaQuem, l.Estrutura, l.Tema, l.Ideias, l.Versiculo, CapaUrl: null);
```

(Confirmar as assinaturas reais do `LivroMapper` do M1 — pode ter nomes ligeiramente diferentes; o essencial é adicionar `CapaUrl: null`.)

- [ ] **Step 16: Build limpo**

Run: `dotnet build BibliaTeologica.sln`
Expected: `Build succeeded`, 0 warnings.

- [ ] **Step 17: Commit**

```bash
git add src/BibliaTeologica.Application Directory.Packages.props
git commit -m "feat(application): contratos do M2 (interfaces, DTOs, tipos de resultado)"
```

---

## Task 3: Infrastructure — pacotes, options e entidades de Identity

> **Parte: M2a + M2b (dividida).** M2a: pacotes Identity/JwtBearer/`System.IdentityModel.Tokens.Jwt`, `ApplicationUser`, `RefreshToken`, options `JwtOptions`/`EmailOptions`/`AdminSeedOptions`. M2b: pacote `SixLabors.ImageSharp`, options `UploadOptions`/`ArmazenamentoOptions`.

**Files:**
- Modify: `Directory.Packages.props`
- Modify: `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj`
- Create: `src/BibliaTeologica.Infrastructure/Identity/ApplicationUser.cs`
- Create: `src/BibliaTeologica.Infrastructure/Identity/RefreshToken.cs`
- Create: `src/BibliaTeologica.Infrastructure/Configuracao/{JwtOptions,EmailOptions,UploadOptions,ArmazenamentoOptions,AdminSeedOptions}.cs`

**Interfaces:**
- Consumes: nada (só tipos de framework).
- Produces:
  - `class ApplicationUser : IdentityUser<Guid>` com `string NomeExibicao`, `bool DeveTrocarSenha`, `DateTime CriadoEm`.
  - `class RefreshToken` — `Id` (Guid), `UsuarioId` (Guid), `TokenHash` (string), `ExpiraEm`, `CriadoEm`, `RevogadoEm` (DateTime?), `SubstituidoPorHash` (string?), `CriadoPorIp` (string?).
  - Classes de options com secção de config: `JwtOptions` (`Jwt`), `EmailOptions` (`Email`), `UploadOptions` (`Upload`), `ArmazenamentoOptions` (`Armazenamento`), `AdminSeedOptions` (`AdminSeed` → lista).

- [ ] **Step 1: Pacotes no `Directory.Packages.props`**

Adicionar ao `<ItemGroup>`:

```xml
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.30" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.30" />
    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.14.0" />
    <PackageVersion Include="SixLabors.ImageSharp" Version="3.1.12" />
```

> Nota: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.x traz `Microsoft.Extensions.Identity.Stores` e o `IdentityDbContext`. O `JwtBearer` 8.0.x traz `Microsoft.IdentityModel.*` 7.x — o `System.IdentityModel.Tokens.Jwt` explícito garante o `JwtSecurityTokenHandler`. Se houver conflito de versão de `Microsoft.IdentityModel.*`, alinhar todos em `7.1.2` (a versão que o JwtBearer 8.0.30 referencia) em vez de `8.14.0`.

- [ ] **Step 2: `.csproj` da Infrastructure**

Adicionar ao `<ItemGroup>` de `PackageReference` em `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj`:

```xml
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" />
    <PackageReference Include="SixLabors.ImageSharp" />
```

Run: `dotnet restore` → Expected: sem `NU1101`/`NU1605`.

- [ ] **Step 3: `ApplicationUser`**

`src/BibliaTeologica.Infrastructure/Identity/ApplicationUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace BibliaTeologica.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string NomeExibicao { get; set; } = string.Empty;
    public bool DeveTrocarSenha { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 4: `RefreshToken`**

`src/BibliaTeologica.Infrastructure/Identity/RefreshToken.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? RevogadoEm { get; set; }
    public string? SubstituidoPorHash { get; set; }
    public string? CriadoPorIp { get; set; }
}
```

- [ ] **Step 5: Classes de options**

`Configuracao/JwtOptions.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Configuracao;

public class JwtOptions
{
    public const string Secao = "Jwt";
    public string Issuer { get; set; } = "BibliaTeologica";
    public string Audience { get; set; } = "BibliaTeologica";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}
```

`Configuracao/EmailOptions.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Configuracao;

public class EmailOptions
{
    public const string Secao = "Email";
    public string Sink { get; set; } = "Dev";           // "Dev" | "Smtp"
    public string Remetente { get; set; } = "nao-responder@biblia-teologica.local";
    public SmtpOptions Smtp { get; set; } = new();

    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}
```

`Configuracao/UploadOptions.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Configuracao;

public class UploadOptions
{
    public const string Secao = "Upload";
    public long TamanhoMaximoBytes { get; set; } = 2_097_152;
    public int DimensaoMaximaPx { get; set; } = 1200;
    public string[] FormatosAceites { get; set; } = { "image/jpeg", "image/png", "image/webp" };
}
```

`Configuracao/ArmazenamentoOptions.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Configuracao;

public class ArmazenamentoOptions
{
    public const string Secao = "Armazenamento";
    public string CaminhoImagens { get; set; } = "./.media/livros";
}
```

`Configuracao/AdminSeedOptions.cs`:

```csharp
namespace BibliaTeologica.Infrastructure.Configuracao;

public class AdminSeedOptions
{
    public const string Secao = "AdminSeed";
    public List<AdminSeedEntrada> Entradas { get; set; } = new();
}

public class AdminSeedEntrada
{
    public string Email { get; set; } = string.Empty;
    public string SenhaInicial { get; set; } = string.Empty;
}
```

> `AdminSeed` no config é um array (`AdminSeed:0:Email`). Para ligar a `Entradas`,
> a Task 12 faz `config.GetSection("AdminSeed").Get<List<AdminSeedEntrada>>()` diretamente,
> não via `Options` binding do array. Manter `AdminSeedOptions` só como conveniência de leitura.

- [ ] **Step 6: Build**

Run: `dotnet build BibliaTeologica.sln`
Expected: `Build succeeded` (as classes ainda não são usadas em lado nenhum).

- [ ] **Step 7: Commit**

```bash
git add Directory.Packages.props src/BibliaTeologica.Infrastructure
git commit -m "chore(infra): pacotes Identity/JwtBearer/ImageSharp, ApplicationUser, RefreshToken, options"
```

---

## Task 4: Infrastructure — `AppDbContext` como `IdentityDbContext` + migração

> **Parte: M2a + M2b (dividida).**
> **M2a:** `AppDbContext : IdentityDbContext`, `RefreshTokenConfiguration`, teste `PersisteERecupera_RefreshToken`, e migração **`M2a_Identity`** (Step 7 com este nome; NÃO incluir a coluna da capa — o `LivroConfiguration` ainda não mudou). Steps 3 e o teste `PersisteCapaImagemPath_NoLivro` **saltam** no M2a.
> **M2b:** Step 3 (`LivroConfiguration` + `HasMaxLength(60)` no `CapaImagemPath`), teste `PersisteCapaImagemPath_NoLivro`, e migração **`M2b_LivroCapa`** (só `AddColumn CapaImagemPath`).

**Files:**
- Modify: `src/BibliaTeologica.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`
- Modify: `src/BibliaTeologica.Infrastructure/Persistence/Configurations/LivroConfiguration.cs`
- Create (via `dotnet ef`): `src/BibliaTeologica.Infrastructure/Persistence/Migrations/*_M2_IdentityELivroCapa.*`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Persistence/AppDbContextTests.cs` (existe — adicionar caso)

**Interfaces:**
- Consumes: `ApplicationUser`, `RefreshToken` (Task 3), `Livro` + `CapaImagemPath` (Task 1).
- Produces:
  - `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` com `DbSet<Livro> Livros` e `DbSet<RefreshToken> RefreshTokens`.
  - Tabela `RefreshTokens` — `TokenHash` único e indexado, `MaxLength(64)`; FK `UsuarioId` → `AspNetUsers` com `DeleteBehavior.Cascade`.
  - Coluna `Livros.CapaImagemPath` (`TEXT`, nullable, `MaxLength(60)`).

- [ ] **Step 1: `AppDbContext`**

Substituir `src/BibliaTeologica.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // tabelas do Identity
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 2: `RefreshTokenConfiguration`**

`src/BibliaTeologica.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`:

```csharp
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BibliaTeologica.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.SubstituidoPorHash).HasMaxLength(64);
        builder.Property(t => t.CriadoPorIp).HasMaxLength(64);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UsuarioId);
    }
}
```

- [ ] **Step 3: `LivroConfiguration` — coluna da capa**

Em `src/BibliaTeologica.Infrastructure/Persistence/Configurations/LivroConfiguration.cs`, adicionar depois de `builder.Property(l => l.Versiculo).IsRequired();`:

```csharp
        builder.Property(l => l.CapaImagemPath).HasMaxLength(60);
```

- [ ] **Step 4: Teste que falha**

Em `tests/BibliaTeologica.Infrastructure.Tests/Persistence/AppDbContextTests.cs` adicionar:

```csharp
    [Fact]
    public async Task PersisteERecupera_RefreshToken()
    {
        var userId = Guid.NewGuid();
        await using (var ctx = CriarContexto())
        {
            ctx.Users.Add(new BibliaTeologica.Infrastructure.Identity.ApplicationUser
            {
                Id = userId, UserName = "a@b.c", Email = "a@b.c", NomeExibicao = "A"
            });
            ctx.RefreshTokens.Add(new BibliaTeologica.Infrastructure.Identity.RefreshToken
            {
                Id = Guid.NewGuid(), UsuarioId = userId, TokenHash = "hash-1",
                ExpiraEm = DateTime.UtcNow.AddDays(1)
            });
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CriarContexto())
        {
            var token = await ctx.RefreshTokens.SingleAsync();
            token.UsuarioId.Should().Be(userId);
            token.RevogadoEm.Should().BeNull();
        }
    }

    [Fact]
    public async Task PersisteCapaImagemPath_NoLivro()
    {
        await using var ctx = CriarContexto();
        var livro = Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        livro.DefinirCapa("x.webp");
        ctx.Livros.Add(livro);
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        (await ctx.Livros.SingleAsync()).CapaImagemPath.Should().Be("x.webp");
    }
```

> `AppDbContextTests` já tem um helper `CriarContexto()` que faz `EnsureCreated()` sobre
> SQLite `:memory:` (do M1). Com o `IdentityDbContext`, `EnsureCreated()` cria também as
> tabelas do Identity — o teste continua a funcionar sem migração.

- [ ] **Step 5: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~AppDbContextTests"`
Expected: FAIL na compilação (`RefreshTokens` / `Users` / `CapaImagemPath`) ou nos 2 casos novos.

- [ ] **Step 6: Ver passar**

Run: `dotnet build BibliaTeologica.sln` → `Build succeeded`.
Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~AppDbContextTests"` → PASS.

- [ ] **Step 7: Gerar a migração**

Run:
```bash
dotnet ef migrations add M2_IdentityELivroCapa \
  --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
```
Expected: cria `Migrations/*_M2_IdentityELivroCapa.cs` com todas as tabelas `AspNet*`,
`RefreshTokens` e `AddColumn CapaImagemPath` em `Livros`.

- [ ] **Step 8: Aplicar e conferir**

Run:
```bash
rm -f *.db biblia-design.db
dotnet ef database update --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
```
Expected: `Done.` Inspecionar rapidamente que o script tem `CREATE TABLE "AspNetUsers"` com
`NomeExibicao`, `DeveTrocarSenha`, `CriadoEm`.

- [ ] **Step 9: `dotnet test` da solução**

Run: `dotnet test BibliaTeologica.sln`
Expected: PASS (Domain, Application, Infrastructure, Api — os testes de Api do M1 usam
`WebApplicationFactory` que corre `MigrateAsync`; devem continuar verdes).

- [ ] **Step 10: Commit**

```bash
git add src/BibliaTeologica.Infrastructure tests/BibliaTeologica.Infrastructure.Tests
git commit -m "feat(infra): AppDbContext como IdentityDbContext + RefreshTokens + coluna da capa + migração"
```

---

## Task 5: Infrastructure — `IdentitySeeder`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Persistence/Seed/IdentitySeeder.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Persistence/IdentitySeederTests.cs`

**Interfaces:**
- Consumes: `ApplicationUser` (Task 3), `AdminSeedEntrada` (Task 3), `RoleManager<IdentityRole<Guid>>`, `UserManager<ApplicationUser>`.
- Produces:
  - `class IdentitySeeder` — ctor `(RoleManager<IdentityRole<Guid>>, UserManager<ApplicationUser>, IEnumerable<AdminSeedEntrada>, ILogger<IdentitySeeder>)`.
  - `Task SeedAsync(CancellationToken ct = default)` — cria roles `Admin`/`Membro` se faltam; para cada `AdminSeedEntrada` com email não existente, cria user na role `Admin` com `DeveTrocarSenha = true`, `EmailConfirmed = true`. Idempotente.
  - Constantes: `IdentitySeeder.RoleAdmin = "Admin"`, `IdentitySeeder.RoleMembro = "Membro"`.

- [ ] **Step 1: Implementar `IdentitySeeder`**

```csharp
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Persistence.Seed;

public class IdentitySeeder
{
    public const string RoleAdmin = "Admin";
    public const string RoleMembro = "Membro";

    private readonly RoleManager<IdentityRole<Guid>> _roles;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IReadOnlyList<AdminSeedEntrada> _admins;
    private readonly ILogger<IdentitySeeder> _log;

    public IdentitySeeder(RoleManager<IdentityRole<Guid>> roles, UserManager<ApplicationUser> users,
        IEnumerable<AdminSeedEntrada> admins, ILogger<IdentitySeeder> log)
    {
        _roles = roles;
        _users = users;
        _admins = admins.ToList();
        _log = log;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        foreach (var role in new[] { RoleAdmin, RoleMembro })
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole<Guid>(role));

        foreach (var entrada in _admins)
        {
            if (string.IsNullOrWhiteSpace(entrada.Email) || string.IsNullOrWhiteSpace(entrada.SenhaInicial))
            {
                _log.LogWarning("Entrada de AdminSeed ignorada (email ou senha vazios).");
                continue;
            }
            if (await _users.FindByEmailAsync(entrada.Email) is not null) continue;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = entrada.Email,
                Email = entrada.Email,
                EmailConfirmed = true,
                NomeExibicao = "Administrador",
                DeveTrocarSenha = true
            };
            var criado = await _users.CreateAsync(user, entrada.SenhaInicial);
            if (!criado.Succeeded)
            {
                _log.LogError("Falha ao criar admin seed {Email}: {Erros}", entrada.Email,
                    string.Join("; ", criado.Errors.Select(e => e.Description)));
                continue;
            }
            await _users.AddToRoleAsync(user, RoleAdmin);
            _log.LogInformation("Admin seed criado: {Email}", entrada.Email);
        }
    }
}
```

- [ ] **Step 2: Teste**

`tests/BibliaTeologica.Infrastructure.Tests/Persistence/IdentitySeederTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Persistence;

public sealed class IdentitySeederTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly ServiceProvider _sp;

    public IdentitySeederTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        _sp = services.BuildServiceProvider();

        _sp.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    private IdentitySeeder CriarSeeder(params AdminSeedEntrada[] admins)
        => new(_sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
               _sp.GetRequiredService<UserManager<ApplicationUser>>(),
               admins, NullLogger<IdentitySeeder>.Instance);

    [Fact]
    public async Task SeedAsync_CriaRolesEAdmin()
    {
        await CriarSeeder(new AdminSeedEntrada { Email = "admin@teste.local", SenhaInicial = "Senha!Forte1" })
            .SeedAsync();

        var roles = _sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        (await roles.RoleExistsAsync("Admin")).Should().BeTrue();
        (await roles.RoleExistsAsync("Membro")).Should().BeTrue();

        var users = _sp.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync("admin@teste.local");
        admin.Should().NotBeNull();
        admin!.DeveTrocarSenha.Should().BeTrue();
        (await users.IsInRoleAsync(admin, "Admin")).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_Idempotente_NumaSegundaChamada()
    {
        var entrada = new AdminSeedEntrada { Email = "admin@teste.local", SenhaInicial = "Senha!Forte1" };
        await CriarSeeder(entrada).SeedAsync();
        await CriarSeeder(entrada).SeedAsync();

        var users = _sp.GetRequiredService<UserManager<ApplicationUser>>();
        users.Users.Count().Should().Be(1);
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conexao.Dispose();
    }
}
```

- [ ] **Step 3: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~IdentitySeederTests"`
Expected: PASS (2).

> Se `AddIdentityCore` falhar por falta de `IPasswordHasher`/`IUserStore`, garantir que a
> chamada é `.AddEntityFrameworkStores<AppDbContext>()` e que o pacote
> `Microsoft.AspNetCore.Identity.EntityFrameworkCore` está referenciado.

- [ ] **Step 4: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Persistence/Seed/IdentitySeeder.cs tests/BibliaTeologica.Infrastructure.Tests/Persistence/IdentitySeederTests.cs
git commit -m "feat(infra): IdentitySeeder (roles + admin de config, idempotente)"
```

---

## Task 6: Infrastructure — `TokenService`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Identity/TokenService.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Identity/TokenServiceTests.cs`

**Interfaces:**
- Consumes: `ITokenService` (Task 2), `JwtOptions` (Task 3).
- Produces: `class TokenService : ITokenService` — ctor `(IOptions<JwtOptions>, IDataProtectionProvider)`.
  - `GerarAccessToken` — JWT HS256, claims `sub`, `email`, `name`, `role` (um por role), `deve_trocar_senha` (`"true"`/`"false"`), `exp` = agora + `AccessTokenMinutes`.
  - `GerarRefreshToken` — 32 bytes aleatórios base64url; `hash` = SHA-256 hex; `expiraEm` = agora + `RefreshTokenDays`.
  - `Hash(tokenCru)` — SHA-256 hex (lowercase) do UTF-8.
  - `GerarDesafioLogin(userId)` / `ValidarDesafioLogin(token)` — `IDataProtector` com purpose `"login-2fa"`, payload `"{userId}|{ticksExpiração}"`, validade 5 min.

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Infrastructure.Tests/Identity/TokenServiceTests.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public class TokenServiceTests
{
    private static TokenService Criar()
    {
        var opts = Options.Create(new JwtOptions
        {
            Issuer = "BT", Audience = "BT",
            SigningKey = "chave-de-teste-com-pelo-menos-32-caracteres-!!",
            AccessTokenMinutes = 15, RefreshTokenDays = 14
        });
        return new TokenService(opts, DataProtectionProvider.Create("BibliaTeologica.Tests"));
    }

    [Fact]
    public void GerarAccessToken_IncluiSubEmailRolesEDeveTrocarSenha()
    {
        var svc = Criar();
        var id = Guid.NewGuid();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(
            svc.GerarAccessToken(id, "a@b.c", "Ana", new[] { "Admin", "Membro" }, deveTrocarSenha: true));

        jwt.Subject.Should().Be(id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "a@b.c");
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().BeEquivalentTo("Admin", "Membro");
        jwt.Claims.Should().Contain(c => c.Type == "deve_trocar_senha" && c.Value == "true");
    }

    [Fact]
    public void GerarRefreshToken_ProduzHashEExpiracaoNoFuturo()
    {
        var (cru, hash, expira) = Criar().GerarRefreshToken();
        cru.Should().NotBeNullOrWhiteSpace();
        hash.Should().HaveLength(64);
        expira.Should().BeAfter(DateTime.UtcNow.AddDays(13));
    }

    [Fact]
    public void Hash_EhDeterministico()
    {
        var svc = Criar();
        svc.Hash("abc").Should().Be(svc.Hash("abc"));
        svc.Hash("abc").Should().NotBe(svc.Hash("abd"));
    }

    [Fact]
    public void DesafioLogin_RoundTrip()
    {
        var svc = Criar();
        var id = Guid.NewGuid();
        svc.ValidarDesafioLogin(svc.GerarDesafioLogin(id)).Should().Be(id);
    }

    [Fact]
    public void ValidarDesafioLogin_DevolveNull_QuandoTokenLixo()
    {
        Criar().ValidarDesafioLogin("nao-e-um-token").Should().BeNull();
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~TokenServiceTests"`
Expected: FAIL — `TokenService` não existe.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Infrastructure/Identity/TokenService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BibliaTeologica.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opts;
    private readonly IDataProtector _protetorDesafio;

    public TokenService(IOptions<JwtOptions> opts, IDataProtectionProvider dp)
    {
        _opts = opts.Value;
        _protetorDesafio = dp.CreateProtector("login-2fa");
    }

    public string GerarAccessToken(Guid userId, string email, string nome,
        IEnumerable<string> roles, bool deveTrocarSenha)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("email", email),
            new("name", nome),
            new("deve_trocar_senha", deveTrocarSenha ? "true" : "false"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opts.AccessTokenMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string tokenCru, string hash, DateTime expiraEm) GerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var cru = Base64UrlEncoder.Encode(bytes);
        return (cru, Hash(cru), DateTime.UtcNow.AddDays(_opts.RefreshTokenDays));
    }

    public string Hash(string tokenCru)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenCru));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string GerarDesafioLogin(Guid userId)
    {
        var validade = DateTime.UtcNow.AddMinutes(5).Ticks;
        return _protetorDesafio.Protect($"{userId:D}|{validade}");
    }

    public Guid? ValidarDesafioLogin(string token)
    {
        try
        {
            var partes = _protetorDesafio.Unprotect(token).Split('|');
            if (partes.Length != 2) return null;
            if (new DateTime(long.Parse(partes[1]), DateTimeKind.Utc) < DateTime.UtcNow) return null;
            return Guid.Parse(partes[0]);
        }
        catch
        {
            return null; // token adulterado / expirado / de outra purpose
        }
    }
}
```

> `Base64UrlEncoder` vem de `Microsoft.IdentityModel.Tokens` (transitivo do JwtBearer).

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~TokenServiceTests"`
Expected: PASS (5).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Identity/TokenService.cs tests/BibliaTeologica.Infrastructure.Tests/Identity/TokenServiceTests.cs
git commit -m "feat(infra): TokenService (access JWT, refresh hasheado, desafio de login)"
```

---

## Task 7: Infrastructure — `RefreshTokenStore`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Identity/RefreshTokenStore.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Identity/RefreshTokenStoreTests.cs`

**Interfaces:**
- Consumes: `IRefreshTokenStore`, `RefreshTokenInfo` (Task 2), `AppDbContext`, `RefreshToken` (Task 3/4).
- Produces: `class RefreshTokenStore : IRefreshTokenStore` — ctor `(AppDbContext)`.
  - `GuardarAsync` — insere novo `RefreshToken`.
  - `ObterAsync(hash)` — `AsNoTracking`, devolve `RefreshTokenInfo?` (qualquer estado — o `.Valido` decide).
  - `RotacionarAsync(hashAntigo, userId, hashNovo, expira, ip)` — marca o antigo `RevogadoEm = UtcNow` + `SubstituidoPorHash = hashNovo`, insere o novo. Uma `SaveChangesAsync`.
  - `RevogarAsync(hash)` — `RevogadoEm = UtcNow` se existir e não revogado.
  - `RevogarCadeiaAsync(userId)` — `RevogadoEm = UtcNow` em todos os do user ainda ativos.

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Infrastructure.Tests/Identity/RefreshTokenStoreTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public sealed class RefreshTokenStoreTests : IDisposable
{
    private readonly SqliteConnection _conexao;

    public RefreshTokenStoreTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();
        using var ctx = Novo();
        ctx.Database.EnsureCreated();
    }

    private AppDbContext Novo()
        => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);

    [Fact]
    public async Task GuardarEObter_DevolveInfoValida()
    {
        var uid = Guid.NewGuid();
        await new RefreshTokenStore(Novo()).GuardarAsync(uid, "h1", DateTime.UtcNow.AddDays(1), "ip", default);

        var info = await new RefreshTokenStore(Novo()).ObterAsync("h1", default);
        info.Should().NotBeNull();
        info!.UserId.Should().Be(uid);
        info.Valido.Should().BeTrue();
    }

    [Fact]
    public async Task ObterAsync_TokenExpirado_NaoEValido()
    {
        await new RefreshTokenStore(Novo()).GuardarAsync(Guid.NewGuid(), "h2", DateTime.UtcNow.AddMinutes(-1), null, default);
        (await new RefreshTokenStore(Novo()).ObterAsync("h2", default))!.Valido.Should().BeFalse();
    }

    [Fact]
    public async Task Rotacionar_RevogaAntigoEEncadeia()
    {
        var uid = Guid.NewGuid();
        await new RefreshTokenStore(Novo()).GuardarAsync(uid, "antigo", DateTime.UtcNow.AddDays(1), null, default);
        await new RefreshTokenStore(Novo()).RotacionarAsync("antigo", uid, "novo", DateTime.UtcNow.AddDays(1), null, default);

        var antigo = await new RefreshTokenStore(Novo()).ObterAsync("antigo", default);
        antigo!.Valido.Should().BeFalse();
        antigo.RevogadoEm.Should().NotBeNull();
        (await new RefreshTokenStore(Novo()).ObterAsync("novo", default))!.Valido.Should().BeTrue();
    }

    [Fact]
    public async Task RevogarCadeia_RevogaTodosDoUser()
    {
        var uid = Guid.NewGuid();
        var store = new RefreshTokenStore(Novo());
        await store.GuardarAsync(uid, "a", DateTime.UtcNow.AddDays(1), null, default);
        await store.GuardarAsync(uid, "b", DateTime.UtcNow.AddDays(1), null, default);

        await new RefreshTokenStore(Novo()).RevogarCadeiaAsync(uid, default);

        (await new RefreshTokenStore(Novo()).ObterAsync("a", default))!.Valido.Should().BeFalse();
        (await new RefreshTokenStore(Novo()).ObterAsync("b", default))!.Valido.Should().BeFalse();
    }

    public void Dispose() => _conexao.Dispose();
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~RefreshTokenStoreTests"`
Expected: FAIL — `RefreshTokenStore` não existe.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Infrastructure/Identity/RefreshTokenStore.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BibliaTeologica.Infrastructure.Identity;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;

    public RefreshTokenStore(AppDbContext db) => _db = db;

    public async Task GuardarAsync(Guid userId, string hash, DateTime expiraEm, string? ip, CancellationToken ct)
    {
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = hash,
            ExpiraEm = expiraEm,
            CriadoEm = DateTime.UtcNow,
            CriadoPorIp = ip
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RefreshTokenInfo?> ObterAsync(string hash, CancellationToken ct)
    {
        var t = await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        return t is null ? null : new RefreshTokenInfo(t.UsuarioId, t.TokenHash, t.ExpiraEm, t.RevogadoEm);
    }

    public async Task RotacionarAsync(string hashAntigo, Guid userId, string hashNovo,
        DateTime expira, string? ip, CancellationToken ct)
    {
        var antigo = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hashAntigo, ct);
        if (antigo is { RevogadoEm: null })
        {
            antigo.RevogadoEm = DateTime.UtcNow;
            antigo.SubstituidoPorHash = hashNovo;
        }
        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = userId,
            TokenHash = hashNovo,
            ExpiraEm = expira,
            CriadoEm = DateTime.UtcNow,
            CriadoPorIp = ip
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevogarAsync(string hash, CancellationToken ct)
    {
        var t = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (t is { RevogadoEm: null })
        {
            t.RevogadoEm = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevogarCadeiaAsync(Guid userId, CancellationToken ct)
    {
        var ativos = await _db.RefreshTokens
            .Where(x => x.UsuarioId == userId && x.RevogadoEm == null)
            .ToListAsync(ct);
        foreach (var t in ativos) t.RevogadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~RefreshTokenStoreTests"`
Expected: PASS (4).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Identity/RefreshTokenStore.cs tests/BibliaTeologica.Infrastructure.Tests/Identity/RefreshTokenStoreTests.cs
git commit -m "feat(infra): RefreshTokenStore (rotação, revogação de cadeia)"
```

---

## Task 8: Infrastructure — `IdentityService`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Identity/IdentityService.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Identity/IdentityServiceTests.cs`

**Interfaces:**
- Consumes: `IIdentityService`, `ResultadoSenha`, `IdentityResultDto`, `UsuarioDto` (Task 2); `ApplicationUser` (Task 3); `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, `IdentitySeeder.RoleMembro`.
- Produces: `class IdentityService : IIdentityService` — ctor `(UserManager<ApplicationUser>, SignInManager<ApplicationUser>)`.

- [ ] **Step 1: Implementar**

`src/BibliaTeologica.Infrastructure/Identity/IdentityService.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Identity;

namespace BibliaTeologica.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public IdentityService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    public async Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null) return new ResultadoSenha(false, false, null);

        var r = await _signIn.CheckPasswordSignInAsync(user, senha, lockoutOnFailure);
        return new ResultadoSenha(r.Succeeded, r.IsLockedOut, r.Succeeded ? user.Id : null);
    }

    public async Task<Guid?> ResolverIdPorEmailAsync(string email, CancellationToken ct)
        => (await _users.FindByEmailAsync(email))?.Id;

    public async Task<string> GerarTokenEmailAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("Utilizador não encontrado.");
        return await _users.GenerateTwoFactorTokenAsync(user, "Email");
    }

    public async Task<bool> VerificarTokenEmailAsync(Guid userId, string codigo, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        return user is not null && await _users.VerifyTwoFactorTokenAsync(user, "Email", codigo);
    }

    public async Task RegistarFalhaAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is not null) await _users.AccessFailedAsync(user);
    }

    public async Task ResetarFalhasAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is not null) await _users.ResetAccessFailedCountAsync(user);
    }

    public async Task<UsuarioDto?> ObterUsuarioAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var roles = await _users.GetRolesAsync(user);
        return new UsuarioDto(user.Email!, user.NomeExibicao, roles.ToList(), user.DeveTrocarSenha);
    }

    public async Task<string?> ObterEmailAsync(Guid userId, CancellationToken ct)
        => (await _users.FindByIdAsync(userId.ToString()))?.Email;

    public async Task<DateTime?> ObterCriadoEmAsync(Guid userId, CancellationToken ct)
        => (await _users.FindByIdAsync(userId.ToString()))?.CriadoEm;

    public async Task<IdentityResultDto> CriarMembroAsync(string email, string senha, string nomeExibicao, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            NomeExibicao = nomeExibicao,
            DeveTrocarSenha = false
        };
        var criado = await _users.CreateAsync(user, senha);
        if (!criado.Succeeded) return Mapear(criado);
        await _users.AddToRoleAsync(user, IdentitySeeder.RoleMembro);
        return IdentityResultDto.Ok();
    }

    public async Task<IdentityResultDto> TrocarSenhaAsync(Guid userId, string atual, string nova, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityResultDto.Falha("Utilizador não encontrado.");
        var r = await _users.ChangePasswordAsync(user, atual, nova);
        if (r.Succeeded && user.DeveTrocarSenha)
        {
            user.DeveTrocarSenha = false;
            await _users.UpdateAsync(user);
        }
        return Mapear(r);
    }

    public async Task<IdentityResultDto> TrocarEmailAsync(Guid userId, string novoEmail, string senha, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityResultDto.Falha("Utilizador não encontrado.");
        if (!await _users.CheckPasswordAsync(user, senha))
            return IdentityResultDto.Falha("Senha incorreta.");

        var token = await _users.GenerateChangeEmailTokenAsync(user, novoEmail);
        var r = await _users.ChangeEmailAsync(user, novoEmail, token);
        if (r.Succeeded)
        {
            await _users.SetUserNameAsync(user, novoEmail);
        }
        return Mapear(r);
    }

    public async Task AtualizarNomeAsync(Guid userId, string nomeExibicao, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return;
        user.NomeExibicao = nomeExibicao;
        await _users.UpdateAsync(user);
    }

    public async Task LimparDeveTrocarSenhaAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is { DeveTrocarSenha: true })
        {
            user.DeveTrocarSenha = false;
            await _users.UpdateAsync(user);
        }
    }

    private static IdentityResultDto Mapear(IdentityResult r)
        => r.Succeeded ? IdentityResultDto.Ok() : IdentityResultDto.Falha(r.Errors.Select(e => e.Description).ToArray());
}
```

- [ ] **Step 2: Teste (SQLite real + Identity)**

`tests/BibliaTeologica.Infrastructure.Tests/Identity/IdentityServiceTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Identity;

public sealed class IdentityServiceTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly ServiceProvider _sp;

    public IdentityServiceTests()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireNonAlphanumeric = false;
                o.SignIn.RequireConfirmedAccount = false;
                o.Tokens.ProviderMap["Email"] =
                    new TokenProviderDescriptor(typeof(EmailTokenProvider<ApplicationUser>));
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        services.AddHttpContextAccessor();
        _sp = services.BuildServiceProvider();
        _sp.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        _sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>()
            .CreateAsync(new IdentityRole<Guid>("Membro")).GetAwaiter().GetResult();
    }

    private IdentityService Criar()
        => new(_sp.GetRequiredService<UserManager<ApplicationUser>>(),
               _sp.GetRequiredService<SignInManager<ApplicationUser>>());

    [Fact]
    public async Task CriarMembro_E_VerificarSenha()
    {
        var svc = Criar();
        (await svc.CriarMembroAsync("m@teste.local", "Senha!Forte1", "Marta", default)).Sucesso.Should().BeTrue();

        var ok = await svc.VerificarSenhaAsync("m@teste.local", "Senha!Forte1", false, default);
        ok.Sucesso.Should().BeTrue();
        ok.UserId.Should().NotBeNull();

        (await svc.VerificarSenhaAsync("m@teste.local", "errada", false, default)).Sucesso.Should().BeFalse();
    }

    [Fact]
    public async Task TokenEmail_RoundTrip()
    {
        var svc = Criar();
        await svc.CriarMembroAsync("t@teste.local", "Senha!Forte1", "T", default);
        var id = (await svc.ResolverIdPorEmailAsync("t@teste.local", default))!.Value;

        var codigo = await svc.GerarTokenEmailAsync(id, default);
        (await svc.VerificarTokenEmailAsync(id, codigo, default)).Should().BeTrue();
        (await svc.VerificarTokenEmailAsync(id, "000000", default)).Should().BeFalse();
    }

    public void Dispose()
    {
        _sp.Dispose();
        _conexao.Dispose();
    }
}
```

- [ ] **Step 3: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~IdentityServiceTests"`
Expected: PASS (2).

> Se `GenerateTwoFactorTokenAsync(user, "Email")` lançar "No IUserTwoFactorTokenProvider
> named 'Email'", confirmar `.AddDefaultTokenProviders()` e o `ProviderMap["Email"]` no setup.

- [ ] **Step 4: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Identity/IdentityService.cs tests/BibliaTeologica.Infrastructure.Tests/Identity/IdentityServiceTests.cs
git commit -m "feat(infra): IdentityService (senha, 2FA email, criar membro, trocar senha/email)"
```

---

## Task 9: Infrastructure — email (`LogEmailSender`, `SmtpEmailSender`)

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Email/LogEmailSender.cs`
- Create: `src/BibliaTeologica.Infrastructure/Email/SmtpEmailSender.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Email/LogEmailSenderTests.cs`

**Interfaces:**
- Consumes: `IEmailSender` (Task 2), `EmailOptions` (Task 3).
- Produces:
  - `class LogEmailSender : IEmailSender` — ctor `(ILogger<LogEmailSender>)`. `EnviarAsync` faz `LogInformation("EMAIL para {Para} | {Assunto}\n{Corpo}", ...)` e devolve `Task.CompletedTask`.
  - `class SmtpEmailSender : IEmailSender` — ctor `(IOptions<EmailOptions>, ILogger<SmtpEmailSender>)`. `EnviarAsync` usa `System.Net.Mail.SmtpClient`.

- [ ] **Step 1: Teste que falha**

`tests/BibliaTeologica.Infrastructure.Tests/Email/LogEmailSenderTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Email;

public class LogEmailSenderTests
{
    private sealed class LoggerEspiao<T> : ILogger<T>
    {
        public readonly List<string> Mensagens = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Fim();
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex,
            Func<TState, Exception?, string> formatter) => Mensagens.Add(formatter(state, ex));
        private sealed class Fim : IDisposable { public void Dispose() { } }
    }

    [Fact]
    public async Task EnviarAsync_LogaParaAssuntoECorpo()
    {
        var logger = new LoggerEspiao<LogEmailSender>();
        await new LogEmailSender(logger).EnviarAsync("a@b.c", "Código", "<b>123456</b>", default);

        logger.Mensagens.Should().ContainSingle()
            .Which.Should().Contain("a@b.c").And.Contain("Código").And.Contain("123456");
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LogEmailSenderTests"`
Expected: FAIL.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Infrastructure/Email/LogEmailSender.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Infrastructure.Email;

public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _log;

    public LogEmailSender(ILogger<LogEmailSender> log) => _log = log;

    public Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        _log.LogInformation("EMAIL (sink Dev) para {Para} | Assunto: {Assunto}\n{Corpo}",
            para, assunto, corpoHtml);
        return Task.CompletedTask;
    }
}
```

`src/BibliaTeologica.Infrastructure/Email/SmtpEmailSender.cs`:

```csharp
using System.Net;
using System.Net.Mail;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opts;
    private readonly ILogger<SmtpEmailSender> _log;

    public SmtpEmailSender(IOptions<EmailOptions> opts, ILogger<SmtpEmailSender> log)
    {
        _opts = opts.Value;
        _log = log;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        var s = _opts.Smtp;
        using var client = new SmtpClient(s.Host, s.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(s.User, s.Password)
        };
        using var msg = new MailMessage(s.From, para, assunto, corpoHtml) { IsBodyHtml = true };
        try
        {
            await client.SendMailAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Falha ao enviar email para {Para}", para);
            throw;
        }
    }
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LogEmailSenderTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Email tests/BibliaTeologica.Infrastructure.Tests/Email
git commit -m "feat(infra): IEmailSender — LogEmailSender (dev) e SmtpEmailSender"
```

---

## Task 10: Infrastructure — `ProcessadorImagemSharp`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Imagens/ProcessadorImagemSharp.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Imagens/ProcessadorImagemSharpTests.cs`

**Interfaces:**
- Consumes: `IProcessadorImagem` (Task 2), `SixLabors.ImageSharp`.
- Produces: `class ProcessadorImagemSharp : IProcessadorImagem` — sem estado.
  - `AssinaturaValida(cabecalho, contentType)` — JPEG `FF D8 FF`; PNG `89 50 4E 47 0D 0A 1A 0A`; WebP `52 49 46 46` nos bytes 0–3 e `57 45 42 50` nos bytes 8–11. Tem de bater com o `contentType`.
  - `NormalizarAsync(entrada, dimensaoMaxPx)` — `Image.LoadAsync`, `Mutate(Resize com ResizeMode.Max)`, `SaveAsWebpAsync` para um `MemoryStream` reposicionado a 0.

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Infrastructure.Tests/Imagens/ProcessadorImagemSharpTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Imagens;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Imagens;

public class ProcessadorImagemSharpTests
{
    private static byte[] PngDe(int w, int h)
    {
        using var img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(w, h);
        using var ms = new MemoryStream();
        img.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    [Fact]
    public void AssinaturaValida_AceitaPngRealERejeitaMismatch()
    {
        var svc = new ProcessadorImagemSharp();
        var png = PngDe(2, 2);

        svc.AssinaturaValida(png[..12], "image/png").Should().BeTrue();
        svc.AssinaturaValida(png[..12], "image/jpeg").Should().BeFalse();
        svc.AssinaturaValida(new byte[] { 1, 2, 3, 4 }, "image/png").Should().BeFalse();
    }

    [Fact]
    public async Task NormalizarAsync_ProduzWebpRedimensionado()
    {
        var svc = new ProcessadorImagemSharp();
        using var entrada = new MemoryStream(PngDe(2000, 1000));

        await using var saida = await svc.NormalizarAsync(entrada, 1200, default);
        saida.Position.Should().Be(0);

        using var img = await Image.LoadAsync(saida);
        img.Width.Should().Be(1200);
        img.Height.Should().Be(600);
        img.Metadata.DecodedImageFormat!.Name.Should().Be("WebP");
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~ProcessadorImagemSharpTests"`
Expected: FAIL.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Infrastructure/Imagens/ProcessadorImagemSharp.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace BibliaTeologica.Infrastructure.Imagens;

public class ProcessadorImagemSharp : IProcessadorImagem
{
    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public bool AssinaturaValida(byte[] cabecalho, string contentType)
    {
        var h = cabecalho.AsSpan();
        return contentType switch
        {
            "image/jpeg" => h.Length >= 3 && h[..3].SequenceEqual(Jpeg),
            "image/png"  => h.Length >= 8 && h[..8].SequenceEqual(Png),
            "image/webp" => h.Length >= 12
                            && h[..4].SequenceEqual("RIFF"u8)
                            && h[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    public async Task<Stream> NormalizarAsync(Stream entrada, int dimensaoMaxPx, CancellationToken ct)
    {
        using var img = await Image.LoadAsync(entrada, ct);
        img.Metadata.ExifProfile = null;
        img.Metadata.IptcProfile = null;
        img.Metadata.XmpProfile = null;

        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(dimensaoMaxPx, dimensaoMaxPx)
        }));

        var saida = new MemoryStream();
        await img.SaveAsWebpAsync(saida, new WebpEncoder { Quality = 82 }, ct);
        saida.Position = 0;
        return saida;
    }
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~ProcessadorImagemSharpTests"`
Expected: PASS (2).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Imagens/ProcessadorImagemSharp.cs tests/BibliaTeologica.Infrastructure.Tests/Imagens/ProcessadorImagemSharpTests.cs
git commit -m "feat(infra): ProcessadorImagemSharp (magic bytes + re-encode WebP + resize)"
```

---

## Task 11: Infrastructure — `ArmazenamentoImagensLocal`

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Imagens/ArmazenamentoImagensLocal.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Imagens/ArmazenamentoImagensLocalTests.cs`

**Interfaces:**
- Consumes: `IArmazenamentoImagens` (Task 2), `ArmazenamentoOptions` (Task 3).
- Produces: `class ArmazenamentoImagensLocal : IArmazenamentoImagens` — ctor `(IOptions<ArmazenamentoOptions>)`, cria o diretório base no ctor.
  - `GuardarAsync(webp)` — `{guid:N}.webp`, escreve com `FileMode.CreateNew`, devolve a chave.
  - `AbrirAsync(chave)` — se a chave não bate `^[0-9a-f]{32}\.webp$` → `null`; se o caminho resolvido não fica sob o baseDir → `null`; se não existir → `null`; senão `FileStream` de leitura.
  - `RemoverAsync(chave)` — apaga se existir e chave válida; não lança.

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Infrastructure.Tests/Imagens/ArmazenamentoImagensLocalTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Imagens;
using Microsoft.Extensions.Options;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests.Imagens;

public sealed class ArmazenamentoImagensLocalTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bt-media-" + Guid.NewGuid().ToString("N"));

    private ArmazenamentoImagensLocal Criar()
        => new(Options.Create(new ArmazenamentoOptions { CaminhoImagens = _dir }));

    [Fact]
    public async Task GuardarAbrirRemover_RoundTrip()
    {
        var svc = Criar();
        using var conteudo = new MemoryStream(new byte[] { 1, 2, 3 });

        var chave = await svc.GuardarAsync(conteudo, default);
        chave.Should().MatchRegex("^[0-9a-f]{32}\\.webp$");

        await using (var lido = await svc.AbrirAsync(chave, default))
            lido.Should().NotBeNull();

        await svc.RemoverAsync(chave, default);
        (await svc.AbrirAsync(chave, default)).Should().BeNull();
    }

    [Theory]
    [InlineData("../secret.webp")]
    [InlineData("..\\secret.webp")]
    [InlineData("nao-guid.webp")]
    [InlineData("abcdef.txt")]
    public async Task AbrirAsync_DevolveNull_QuandoChaveInvalida(string chave)
        => (await Criar().AbrirAsync(chave, default)).Should().BeNull();

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~ArmazenamentoImagensLocalTests"`
Expected: FAIL.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Infrastructure/Imagens/ArmazenamentoImagensLocal.cs`:

```csharp
using System.Text.RegularExpressions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Imagens;

public partial class ArmazenamentoImagensLocal : IArmazenamentoImagens
{
    private readonly string _base;

    public ArmazenamentoImagensLocal(IOptions<ArmazenamentoOptions> opts)
    {
        _base = Path.GetFullPath(opts.Value.CaminhoImagens);
        Directory.CreateDirectory(_base);
    }

    public async Task<string> GuardarAsync(Stream webp, CancellationToken ct)
    {
        var chave = $"{Guid.NewGuid():N}.webp";
        var caminho = Path.Combine(_base, chave);
        await using var fs = new FileStream(caminho, FileMode.CreateNew, FileAccess.Write);
        webp.Position = 0;
        await webp.CopyToAsync(fs, ct);
        return chave;
    }

    public Task RemoverAsync(string chave, CancellationToken ct)
    {
        var caminho = CaminhoSeguro(chave);
        if (caminho is not null && File.Exists(caminho)) File.Delete(caminho);
        return Task.CompletedTask;
    }

    public Task<Stream?> AbrirAsync(string chave, CancellationToken ct)
    {
        var caminho = CaminhoSeguro(chave);
        if (caminho is null || !File.Exists(caminho)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    private string? CaminhoSeguro(string chave)
    {
        if (!ChaveValida().IsMatch(chave)) return null;
        var completo = Path.GetFullPath(Path.Combine(_base, chave));
        return completo.StartsWith(_base + Path.DirectorySeparatorChar, StringComparison.Ordinal) || completo == _base
            ? completo
            : null;
    }

    [GeneratedRegex("^[0-9a-f]{32}\\.webp$")]
    private static partial Regex ChaveValida();
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~ArmazenamentoImagensLocalTests"`
Expected: PASS (5).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Imagens/ArmazenamentoImagensLocal.cs tests/BibliaTeologica.Infrastructure.Tests/Imagens/ArmazenamentoImagensLocalTests.cs
git commit -m "feat(infra): ArmazenamentoImagensLocal (nome {guid}.webp, anti path-traversal)"
```

---

## Task 12: Infrastructure — `AddInfrastructure` (Identity, JwtBearer, DI dos serviços)

> **Parte: M2a + M2b (dividida).**
> **M2a:** `FrameworkReference Microsoft.AspNetCore.App`; `Configure<JwtOptions>`/`<EmailOptions>`; `AddDataProtection`; `AddIdentityCore`/`AddJwtBearer` (cookie `bt_access`); `AddAuthorization`; DI de `IIdentityService`, `ITokenService`, `IRefreshTokenStore`, `IEmailSender` (por sink), `IdentitySeeder`. O teste de composição só verifica esses.
> **M2b:** adicionar `Configure<UploadOptions>`/`<ArmazenamentoOptions>` e DI de `IProcessadorImagem` (singleton), `IArmazenamentoImagens` (singleton), `IUploadPolitica` (singleton). Estender o `DependencyInjectionTests` com essas 3 linhas do `[Theory]`.

**Files:**
- Modify: `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`
- Modify: `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj` (+ `Microsoft.AspNetCore.App` framework ref para `IDataProtectionProvider`/`AddAuthentication`)
- Test: `tests/BibliaTeologica.Infrastructure.Tests/DependencyInjectionTests.cs`

**Interfaces:**
- Consumes: tudo o que foi criado nas Tasks 3–11 + `IdentitySeeder` (Task 5).
- Produces: `AddInfrastructure(this IServiceCollection, IConfiguration)` estendido:
  - `Configure<JwtOptions>`, `<EmailOptions>`, `<UploadOptions>`, `<ArmazenamentoOptions>` das respetivas secções.
  - `AddDataProtection()`.
  - `AddIdentityCore<ApplicationUser>()` + `.AddRoles<IdentityRole<Guid>>()` + `.AddSignInManager()` + `.AddEntityFrameworkStores<AppDbContext>()` + `.AddDefaultTokenProviders()`; opções de password (min 8, sem exigir não-alfanumérico) e lockout de `Identity:Lockout`.
  - `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` — `TokenValidationParameters` de `JwtOptions`; evento `OnMessageReceived` lê `context.Request.Cookies["bt_access"]`.
  - `AddAuthorization()`.
  - `AddScoped` de `IIdentityService`, `ITokenService` (singleton ok mas scoped é seguro), `IRefreshTokenStore`, `IProcessadorImagem` (singleton), `IArmazenamentoImagens` (singleton), `IEmailSender` (por `Email:Sink`), `IdentitySeeder` (com a lista de `AdminSeedEntrada`).

- [ ] **Step 1: Framework reference no `.csproj`**

`AddAuthentication`, `AddJwtBearer`, `IDataProtectionProvider` e `HttpContext` vivem em
`Microsoft.AspNetCore.App`. Adicionar ao `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj`:

```xml
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
```

- [ ] **Step 2: Reescrever `DependencyInjection.cs`**

`src/BibliaTeologica.Infrastructure/DependencyInjection.cs`:

```csharp
using System.Text;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using BibliaTeologica.Infrastructure.Email;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Imagens;
using BibliaTeologica.Infrastructure.Persistence;
using BibliaTeologica.Infrastructure.Persistence.Seed;
using BibliaTeologica.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BibliaTeologica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Secao));
        services.Configure<EmailOptions>(config.GetSection(EmailOptions.Secao));
        services.Configure<UploadOptions>(config.GetSection(UploadOptions.Secao));
        services.Configure<ArmazenamentoOptions>(config.GetSection(ArmazenamentoOptions.Secao));

        var jwt = config.GetSection(JwtOptions.Secao).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey em falta ou com menos de 32 caracteres.");

        var maxTentativas = config.GetValue<int?>("Identity:Lockout:MaxTentativas") ?? 5;
        var lockoutMinutos = config.GetValue<int?>("Identity:Lockout:Minutos") ?? 15;

        services.AddDataProtection();

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = true;
                o.Lockout.MaxFailedAccessAttempts = maxTentativas;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutMinutos);
                o.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    RoleClaimType = "role",
                    NameClaimType = "name",
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("bt_access", out var cookie))
                            ctx.Token = cookie;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddSingleton<IProcessadorImagem, ProcessadorImagemSharp>();
        services.AddSingleton<IArmazenamentoImagens, ArmazenamentoImagensLocal>();

        var sink = config.GetValue<string>("Email:Sink") ?? "Dev";
        if (string.Equals(sink, "Smtp", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        else
            services.AddScoped<IEmailSender, LogEmailSender>();

        var admins = config.GetSection(AdminSeedOptions.Secao).Get<List<AdminSeedEntrada>>() ?? new();
        services.AddScoped<IdentitySeeder>(sp => new IdentitySeeder(
            sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),
            sp.GetRequiredService<UserManager<ApplicationUser>>(),
            admins,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<IdentitySeeder>>()));

        services.AddScoped<LivroSeeder>();

        return services;
    }
}
```

- [ ] **Step 3: Teste de composição**

`tests/BibliaTeologica.Infrastructure.Tests/DependencyInjectionTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BibliaTeologica.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static IServiceProvider Construir(string sink = "Dev")
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["Jwt:SigningKey"] = "chave-de-teste-com-pelo-menos-32-caracteres!!",
            ["Email:Sink"] = sink
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(config);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(typeof(IIdentityService))]
    [InlineData(typeof(ITokenService))]
    [InlineData(typeof(IRefreshTokenStore))]
    [InlineData(typeof(IEmailSender))]
    [InlineData(typeof(IProcessadorImagem))]
    [InlineData(typeof(IArmazenamentoImagens))]
    [InlineData(typeof(ILivroRepository))]
    public void AddInfrastructure_ResolveOsServicos(Type servico)
    {
        using var scope = Construir().CreateScope();
        scope.ServiceProvider.GetService(servico).Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_Lanca_QuandoSigningKeyCurta()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["Jwt:SigningKey"] = "curta"
        }).Build();

        var acao = () => new ServiceCollection().AddLogging().Services.AddInfrastructure(config);
        acao.Should().Throw<InvalidOperationException>();
    }
}
```

> `new ServiceCollection().AddLogging().Services` — `AddLogging` devolve `IServiceCollection`
> diretamente; ajustar para `var s = new ServiceCollection(); s.AddLogging(); s.AddInfrastructure(config);`
> dentro de um lambda se o encadeamento não compilar.

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~DependencyInjectionTests"`
Expected: PASS.

- [ ] **Step 5: `dotnet test` da solução**

Run: `dotnet test BibliaTeologica.sln`
Expected: PASS — **exceto** os testes de `BibliaTeologica.Api.Tests` do M1, que agora podem
falhar no arranque por falta de `Jwt:SigningKey`. **Não corrigir aqui** — a Task 18 adiciona
a configuração de teste da Api. Se falharem, anotar e seguir.

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Infrastructure tests/BibliaTeologica.Infrastructure.Tests/DependencyInjectionTests.cs
git commit -m "feat(infra): AddInfrastructure — Identity, JwtBearer via cookie, DI dos serviços do M2"
```

---

## Task 13: Application — `LivroRepository` estendido + `AddApplication`

**Files:**
- Modify: `src/BibliaTeologica.Infrastructure/Repositories/LivroRepository.cs`
- Modify: `src/BibliaTeologica.Application/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Repositories/LivroRepositoryTests.cs` (existe — adicionar casos)

**Interfaces:**
- Consumes: `ILivroRepository` estendido (Task 2), `AppDbContext`.
- Produces: `LivroRepository` implementa os 7 métodos novos. `AddApplication` regista `ILivroCommandService`, `IAuthService`, `IContaService` (implementações nas Tasks 14–16 — registar já, mesmo que as classes ainda não existam? **Não**: registar em cada task respetiva. Aqui só `LivroRepository`).

- [ ] **Step 1: Testes que falham**

Em `tests/BibliaTeologica.Infrastructure.Tests/Repositories/LivroRepositoryTests.cs` adicionar:

```csharp
    [Fact]
    public async Task ListarTodos_IncluiDespublicados_OrdenadoPorOrdem()
    {
        await using var ctx = Novo();
        var g = Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1, "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        var e = Livro.Create("ÊXODO", SecaoCanonica.Pentateuco, 2, "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        e.Despublicar();
        ctx.Livros.AddRange(e, g);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();

        var todos = await new LivroRepository(ctx).ListarTodosAsync(default);
        todos.Select(l => l.Nome).Should().Equal("GÊNESIS", "ÊXODO");
    }

    [Fact]
    public async Task SlugExiste_IgnoraOProprioId()
    {
        await using var ctx = Novo();
        var g = Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1, "q", "p", "pq", "e", "t", new[] { "i" }, "v");
        ctx.Livros.Add(g);
        await ctx.SaveChangesAsync();

        var repo = new LivroRepository(ctx);
        (await repo.SlugExisteAsync("genesis", exceptoId: null, default)).Should().BeTrue();
        (await repo.SlugExisteAsync("genesis", exceptoId: g.Id, default)).Should().BeFalse();
        (await repo.OrdemExisteAsync(1, exceptoId: g.Id, default)).Should().BeFalse();
    }

    [Fact]
    public async Task AdicionarERemover_ComSalvar()
    {
        await using var ctx = Novo();
        var repo = new LivroRepository(ctx);
        var g = Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1, "q", "p", "pq", "e", "t", new[] { "i" }, "v");

        await repo.AdicionarAsync(g, default);
        await repo.SalvarAsync(default);
        (await repo.ObterPorIdAsync(g.Id, default)).Should().NotBeNull();

        repo.Remover(g);
        await repo.SalvarAsync(default);
        (await repo.ObterPorIdAsync(g.Id, default)).Should().BeNull();
    }
```

> `LivroRepositoryTests` do M1 tem um helper `Novo()` que devolve um `AppDbContext` sobre
> SQLite em memória partilhado. Reutilizar.

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LivroRepositoryTests"`
Expected: FAIL na compilação (métodos não existem).

- [ ] **Step 3: Implementar**

Substituir `src/BibliaTeologica.Infrastructure/Repositories/LivroRepository.cs`:

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
        => await _db.Livros.AsNoTracking().Where(l => l.Publicado).OrderBy(l => l.Ordem).ToListAsync(ct);

    public Task<Livro?> ObterPorSlugAsync(string slug, CancellationToken ct)
        => _db.Livros.AsNoTracking().FirstOrDefaultAsync(l => l.Slug == slug, ct);

    public async Task<IReadOnlyList<Livro>> ListarTodosAsync(CancellationToken ct)
        => await _db.Livros.AsNoTracking().OrderBy(l => l.Ordem).ToListAsync(ct);

    public Task<Livro?> ObterPorIdAsync(int id, CancellationToken ct)
        => _db.Livros.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task AdicionarAsync(Livro livro, CancellationToken ct)
        => await _db.Livros.AddAsync(livro, ct);

    public void Remover(Livro livro) => _db.Livros.Remove(livro);

    public Task<bool> SlugExisteAsync(string slug, int? exceptoId, CancellationToken ct)
        => _db.Livros.AsNoTracking().AnyAsync(l => l.Slug == slug && (exceptoId == null || l.Id != exceptoId), ct);

    public Task<bool> OrdemExisteAsync(int ordem, int? exceptoId, CancellationToken ct)
        => _db.Livros.AsNoTracking().AnyAsync(l => l.Ordem == ordem && (exceptoId == null || l.Id != exceptoId), ct);

    public Task SalvarAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Infrastructure.Tests --filter "FullyQualifiedName~LivroRepositoryTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Infrastructure/Repositories/LivroRepository.cs tests/BibliaTeologica.Infrastructure.Tests/Repositories/LivroRepositoryTests.cs
git commit -m "feat(infra): LivroRepository — listar todos, obter por id, adicionar/remover, verificar slug/ordem"
```

---

## Task 14: Application — `IEmissorTokens` + `AuthService`

**Files:**
- Create: `src/BibliaTeologica.Application/Auth/IEmissorTokens.cs`
- Create: `src/BibliaTeologica.Application/Auth/EmissorTokens.cs`
- Create: `src/BibliaTeologica.Application/Auth/IAuthService.cs`
- Create: `src/BibliaTeologica.Application/Auth/AuthService.cs`
- Modify: `src/BibliaTeologica.Application/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Auth/{EmissorTokensTests,AuthServiceTests}.cs`

**Interfaces:**
- Consumes: `IIdentityService`, `ITokenService`, `IRefreshTokenStore`, `IEmailSender` (Task 2); DTOs de Auth (Task 2).
- Produces:
  - `interface IEmissorTokens` — `Task<ResultadoLoginDto> EmitirAsync(Guid userId, string? ip, CancellationToken ct)`. Gera access + refresh (guarda o hash) + csrf, devolve `ResultadoLoginDto(Autenticado, Usuario, Tokens)`. **Partilhado** entre `AuthService` e `ContaService` (Task 15).
  - `class EmissorTokens : IEmissorTokens` — ctor `(IIdentityService, ITokenService, IRefreshTokenStore)`.
  - `interface IAuthService`:
    ```
    Task<ResultadoLoginDto> LoginAsync(LoginDto dto, string? ip, CancellationToken ct)
    Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto dto, string? ip, CancellationToken ct)
    Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken ct)
    Task LogoutAsync(string? refreshTokenCru, CancellationToken ct)
    Task<IdentityResultDto> RegistarMembroAsync(RegistarMembroDto dto, CancellationToken ct)
    ```
  - `class AuthService : IAuthService` — ctor `(IIdentityService, ITokenService, IRefreshTokenStore, IEmailSender, ILogger<AuthService>)`.
  - Comportamento:
    - `LoginAsync` — `VerificarSenhaAsync(email, senha, lockoutOnFailure: true)`. `Bloqueado` → `EstadoLogin.Bloqueado`. `!Sucesso` → `CredenciaisInvalidas`. Sucesso → `GerarTokenEmailAsync` + `EmailSender.EnviarAsync` (assunto "Código de acesso", corpo com o código) + devolve `Desafio2fa` com `TokenDesafio = tokenSvc.GerarDesafioLogin(userId)`.
    - `Verificar2faAsync` — `tokenSvc.ValidarDesafioLogin` → `userId?`; null → `CredenciaisInvalidas`. `VerificarTokenEmailAsync(userId, codigo)` → false → `RegistarFalhaAsync` + `CredenciaisInvalidas`. true → `ResetarFalhasAsync` + `EmitirAsync(userId, ip)`.
    - `RefreshAsync` — `hash = tokenSvc.Hash(cru)`; `store.ObterAsync(hash)`. null → `CredenciaisInvalidas`. `info.RevogadoEm != null` (reutilização) → `store.RevogarCadeiaAsync(info.UserId)` + `CredenciaisInvalidas`. `!info.Valido` (expirado) → `CredenciaisInvalidas`. Válido → gera novo refresh, `store.RotacionarAsync(hashAntigo, userId, hashNovo, expira, ip)`, novo access, devolve `Autenticado`.
    - `LogoutAsync` — se `refreshTokenCru` não-nulo, `store.RevogarAsync(tokenSvc.Hash(cru))`.
    - `RegistarMembroAsync` — `identity.CriarMembroAsync`.
    - `Verificar2faAsync` (sucesso) e `RefreshAsync` (após rotação) delegam a emissão de tokens em `IEmissorTokens.EmitirAsync`. No `RefreshAsync`, como o refresh já foi rotacionado (novo hash guardado pela `RotacionarAsync`), o `EmitirAsync` **guardaria um segundo refresh** — por isso o `RefreshAsync` **não** usa o `EmissorTokens`: monta o resultado localmente (gera só o access via `_token`, reusa `novoCru`/`novoExp` da rotação, csrf novo). Só o `Verificar2faAsync` usa o `EmissorTokens`.

- [ ] **Step 1: `IEmissorTokens` + `EmissorTokens`**

`src/BibliaTeologica.Application/Auth/IEmissorTokens.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;

namespace BibliaTeologica.Application.Auth;

public interface IEmissorTokens
{
    Task<ResultadoLoginDto> EmitirAsync(Guid userId, string? ip, CancellationToken ct);
}
```

`src/BibliaTeologica.Application/Auth/EmissorTokens.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;

namespace BibliaTeologica.Application.Auth;

public class EmissorTokens : IEmissorTokens
{
    private readonly IIdentityService _identity;
    private readonly ITokenService _token;
    private readonly IRefreshTokenStore _store;

    public EmissorTokens(IIdentityService identity, ITokenService token, IRefreshTokenStore store)
    {
        _identity = identity;
        _token = token;
        _store = store;
    }

    public async Task<ResultadoLoginDto> EmitirAsync(Guid userId, string? ip, CancellationToken ct)
    {
        var usuario = await _identity.ObterUsuarioAsync(userId, ct)
            ?? throw new InvalidOperationException("Utilizador não encontrado ao emitir tokens.");

        var access = _token.GerarAccessToken(userId, usuario.Email, usuario.NomeExibicao,
            usuario.Roles, usuario.DeveTrocarSenha);
        var (refreshCru, hash, refreshExp) = _token.GerarRefreshToken();
        await _store.GuardarAsync(userId, hash, refreshExp, ip, ct);

        return new ResultadoLoginDto(EstadoLogin.Autenticado, Usuario: usuario,
            Tokens: new TokensGerados(access, DateTime.UtcNow.AddMinutes(15), refreshCru, refreshExp,
                Guid.NewGuid().ToString("N")));
    }
}
```

Teste `tests/BibliaTeologica.Application.Tests/Auth/EmissorTokensTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Auth;

public class EmissorTokensTests
{
    [Fact]
    public async Task EmitirAsync_GeraTokensEGuardaRefresh()
    {
        var uid = Guid.NewGuid();
        var identity = new Mock<IIdentityService>();
        var token = new Mock<ITokenService>();
        var store = new Mock<IRefreshTokenStore>();

        identity.Setup(i => i.ObterUsuarioAsync(uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false));
        token.Setup(t => t.GerarAccessToken(uid, "a@b.c", "Ana", It.IsAny<IEnumerable<string>>(), false)).Returns("acc");
        token.Setup(t => t.GerarRefreshToken()).Returns(("cru", "hash", DateTime.UtcNow.AddDays(14)));

        var r = await new EmissorTokens(identity.Object, token.Object, store.Object).EmitirAsync(uid, "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("acc");
        r.Tokens.Csrf.Should().NotBeNullOrWhiteSpace();
        store.Verify(s => s.GuardarAsync(uid, "hash", It.IsAny<DateTime>(), "ip", It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 2: `IAuthService`**

`src/BibliaTeologica.Application/Auth/IAuthService.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;

namespace BibliaTeologica.Application.Auth;

public interface IAuthService
{
    Task<ResultadoLoginDto> LoginAsync(LoginDto dto, string? ip, CancellationToken ct);
    Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto dto, string? ip, CancellationToken ct);
    Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken ct);
    Task LogoutAsync(string? refreshTokenCru, CancellationToken ct);
    Task<IdentityResultDto> RegistarMembroAsync(RegistarMembroDto dto, CancellationToken ct);
}
```

- [ ] **Step 3: Testes que falham**

`tests/BibliaTeologica.Application.Tests/Auth/AuthServiceTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<ITokenService> _token = new();
    private readonly Mock<IRefreshTokenStore> _store = new();
    private readonly Mock<IEmailSender> _email = new();
    private readonly Mock<IEmissorTokens> _emissor = new();

    private AuthService Criar() => new(_identity.Object, _token.Object, _store.Object,
        _email.Object, _emissor.Object, NullLogger<AuthService>.Instance);

    private static readonly Guid Uid = Guid.NewGuid();

    private void ArranjarEmissorOk() =>
        _emissor.Setup(e => e.EmitirAsync(Uid, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoLoginDto(EstadoLogin.Autenticado,
                Usuario: new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false),
                Tokens: new TokensGerados("access-jwt", DateTime.UtcNow.AddMinutes(15),
                    "refresh-cru", DateTime.UtcNow.AddDays(14), "csrf")));

    private void ArranjarEmissaoLocalOk()
    {
        _identity.Setup(i => i.ObterUsuarioAsync(Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Admin" }, false));
        _token.Setup(t => t.GerarAccessToken(Uid, "a@b.c", "Ana", It.IsAny<IEnumerable<string>>(), false))
            .Returns("access-jwt");
        _token.Setup(t => t.GerarRefreshToken())
            .Returns(("refresh-cru", "refresh-hash", DateTime.UtcNow.AddDays(14)));
    }

    [Fact]
    public async Task LoginAsync_EnviaOtpEDevolveDesafio_QuandoCredenciaisValidas()
    {
        _identity.Setup(i => i.VerificarSenhaAsync("a@b.c", "senha", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(true, false, Uid));
        _identity.Setup(i => i.GerarTokenEmailAsync(Uid, It.IsAny<CancellationToken>())).ReturnsAsync("123456");
        _token.Setup(t => t.GerarDesafioLogin(Uid)).Returns("token-desafio");

        var r = await Criar().LoginAsync(new LoginDto("a@b.c", "senha"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Desafio2fa);
        r.TokenDesafio.Should().Be("token-desafio");
        _email.Verify(e => e.EnviarAsync("a@b.c", It.IsAny<string>(),
            It.Is<string>(c => c.Contains("123456")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DevolveBloqueado_QuandoLockout()
    {
        _identity.Setup(i => i.VerificarSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, true, null));

        (await Criar().LoginAsync(new LoginDto("a@b.c", "x"), null, default)).Estado
            .Should().Be(EstadoLogin.Bloqueado);
    }

    [Fact]
    public async Task LoginAsync_DevolveCredenciaisInvalidas_QuandoSenhaErrada()
    {
        _identity.Setup(i => i.VerificarSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoSenha(false, false, null));

        (await Criar().LoginAsync(new LoginDto("a@b.c", "x"), null, default)).Estado
            .Should().Be(EstadoLogin.CredenciaisInvalidas);
    }

    [Fact]
    public async Task Verificar2faAsync_EmiteTokens_QuandoCodigoCorreto()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.VerificarTokenEmailAsync(Uid, "123456", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ArranjarEmissorOk();

        var r = await Criar().Verificar2faAsync(new Verificar2faDto("td", "123456"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("access-jwt");
        _emissor.Verify(e => e.EmitirAsync(Uid, "ip", It.IsAny<CancellationToken>()), Times.Once);
        _identity.Verify(i => i.ResetarFalhasAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Verificar2faAsync_RegistaFalha_QuandoCodigoErrado()
    {
        _token.Setup(t => t.ValidarDesafioLogin("td")).Returns(Uid);
        _identity.Setup(i => i.VerificarTokenEmailAsync(Uid, "000000", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var r = await Criar().Verificar2faAsync(new Verificar2faDto("td", "000000"), null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _identity.Verify(i => i.RegistarFalhaAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_RotacionaEEmite_QuandoRefreshValido()
    {
        _token.Setup(t => t.Hash("cru")).Returns("hash-antigo");
        _store.Setup(s => s.ObterAsync("hash-antigo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "hash-antigo", DateTime.UtcNow.AddDays(1), null));
        ArranjarEmissaoLocalOk();

        var r = await Criar().RefreshAsync("cru", "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        _store.Verify(s => s.RotacionarAsync("hash-antigo", Uid, "refresh-hash", It.IsAny<DateTime>(), "ip", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_RevogaCadeia_QuandoTokenJaRevogado()
    {
        _token.Setup(t => t.Hash("cru")).Returns("h");
        _store.Setup(s => s.ObterAsync("h", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshTokenInfo(Uid, "h", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMinutes(-1)));

        var r = await Criar().RefreshAsync("cru", null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        _store.Verify(s => s.RevogarCadeiaAsync(Uid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistarMembroAsync_DelegaNoIdentity()
    {
        _identity.Setup(i => i.CriarMembroAsync("m@b.c", "Senha!Forte1", "Marta", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());

        (await Criar().RegistarMembroAsync(new RegistarMembroDto("m@b.c", "Senha!Forte1", "Marta"), default))
            .Sucesso.Should().BeTrue();
    }
}
```

- [ ] **Step 4: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests --filter "FullyQualifiedName~AuthServiceTests"`
Expected: FAIL — `AuthService` não existe.

- [ ] **Step 5: Implementar**

`src/BibliaTeologica.Application/Auth/AuthService.cs` — injeta `IEmissorTokens`; `Verificar2faAsync`
chama `_emissor.EmitirAsync`; `RefreshAsync` monta o resultado localmente (o refresh já foi
rotacionado). O `RegistarMembroAsync` e o `LogoutAsync` ficam iguais:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identity;
    private readonly ITokenService _token;
    private readonly IRefreshTokenStore _store;
    private readonly IEmailSender _email;
    private readonly IEmissorTokens _emissor;
    private readonly ILogger<AuthService> _log;

    public AuthService(IIdentityService identity, ITokenService token, IRefreshTokenStore store,
        IEmailSender email, IEmissorTokens emissor, ILogger<AuthService> log)
    {
        _identity = identity;
        _token = token;
        _store = store;
        _email = email;
        _emissor = emissor;
        _log = log;
    }

    public async Task<ResultadoLoginDto> LoginAsync(LoginDto dto, string? ip, CancellationToken ct)
    {
        var r = await _identity.VerificarSenhaAsync(dto.Email, dto.Senha, lockoutOnFailure: true, ct);
        if (r.Bloqueado) return new ResultadoLoginDto(EstadoLogin.Bloqueado);
        if (!r.Sucesso || r.UserId is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        var codigo = await _identity.GerarTokenEmailAsync(r.UserId.Value, ct);
        await _email.EnviarAsync(dto.Email, "Código de acesso",
            $"<p>O seu código de acesso é <strong>{codigo}</strong>. Expira em poucos minutos.</p>", ct);

        return new ResultadoLoginDto(EstadoLogin.Desafio2fa, TokenDesafio: _token.GerarDesafioLogin(r.UserId.Value));
    }

    public async Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto dto, string? ip, CancellationToken ct)
    {
        var userId = _token.ValidarDesafioLogin(dto.TokenDesafio);
        if (userId is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        if (!await _identity.VerificarTokenEmailAsync(userId.Value, dto.Codigo, ct))
        {
            await _identity.RegistarFalhaAsync(userId.Value, ct);
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);
        }

        await _identity.ResetarFalhasAsync(userId.Value, ct);
        return await _emissor.EmitirAsync(userId.Value, ip, ct);
    }

    public async Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken ct)
    {
        var hash = _token.Hash(refreshTokenCru);
        var info = await _store.ObterAsync(hash, ct);
        if (info is null) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        if (info.RevogadoEm is not null)
        {
            _log.LogWarning("Refresh token reutilizado para {UserId} — a revogar cadeia.", info.UserId);
            await _store.RevogarCadeiaAsync(info.UserId, ct);
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);
        }
        if (!info.Valido) return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas);

        // O refresh é rotacionado aqui (novo hash guardado). NÃO usamos o _emissor
        // (guardaria um 2º refresh). Montamos o resultado localmente.
        var (novoCru, novoHash, novoExp) = _token.GerarRefreshToken();
        await _store.RotacionarAsync(hash, info.UserId, novoHash, novoExp, ip, ct);

        var usuario = await _identity.ObterUsuarioAsync(info.UserId, ct)
            ?? throw new InvalidOperationException("Utilizador não encontrado ao emitir tokens.");
        var access = _token.GerarAccessToken(info.UserId, usuario.Email, usuario.NomeExibicao,
            usuario.Roles, usuario.DeveTrocarSenha);

        return new ResultadoLoginDto(EstadoLogin.Autenticado, Usuario: usuario,
            Tokens: new TokensGerados(access, DateTime.UtcNow.AddMinutes(15), novoCru, novoExp,
                Guid.NewGuid().ToString("N")));
    }

    public async Task LogoutAsync(string? refreshTokenCru, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(refreshTokenCru))
            await _store.RevogarAsync(_token.Hash(refreshTokenCru), ct);
    }

    public Task<IdentityResultDto> RegistarMembroAsync(RegistarMembroDto dto, CancellationToken ct)
        => _identity.CriarMembroAsync(dto.Email, dto.Senha, dto.NomeExibicao, ct);
}
```

> **Nota:** `DateTime.UtcNow.AddMinutes(15)` aparece no `EmissorTokens`, no `RefreshAsync` e
> (implicitamente) no `TokenService.GerarAccessToken` (via `JwtOptions.AccessTokenMinutes`).
> A spec fixa 15 min; manter os 3 alinhados. O `AuthServiceTests` (Step 3) usa mocks do
> `ITokenService` e do `IEmissorTokens`, portanto o valor concreto não é testado aqui.

- [ ] **Step 6: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests --filter "FullyQualifiedName~AuthServiceTests|FullyQualifiedName~EmissorTokensTests"`
Expected: PASS (9).

- [ ] **Step 7: Registar no DI**

`src/BibliaTeologica.Application/DependencyInjection.cs` — adicionar dentro de `AddApplication`:

```csharp
        services.AddScoped<Auth.IEmissorTokens, Auth.EmissorTokens>();
        services.AddScoped<Auth.IAuthService, Auth.AuthService>();
```

- [ ] **Step 8: Commit**

```bash
git add src/BibliaTeologica.Application/Auth src/BibliaTeologica.Application/DependencyInjection.cs tests/BibliaTeologica.Application.Tests/Auth
git commit -m "feat(application): EmissorTokens + AuthService (login 2 passos, 2FA, refresh rotativo, registo de membro)"
```

---

## Task 15: Application — `ContaService`

**Files:**
- Create: `src/BibliaTeologica.Application/Conta/IContaService.cs`
- Create: `src/BibliaTeologica.Application/Conta/ContaService.cs`
- Modify: `src/BibliaTeologica.Application/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Conta/ContaServiceTests.cs`

**Interfaces:**
- Consumes: `IIdentityService` (Task 2), `IEmissorTokens` (Task 14), DTOs de Conta (Task 2).
- Produces:
  - `interface IContaService`:
    ```
    Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct)
    Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct)
    Task<IdentityResultDto> TrocarEmailAsync(Guid userId, TrocarEmailDto dto, CancellationToken ct)
    Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct)
    ```
  - `class ContaService : IContaService` — ctor `(IIdentityService, IEmissorTokens)`.
    - `TrocarSenhaAsync` — `identity.TrocarSenhaAsync` (que já limpa `DeveTrocarSenha`); se sucesso → `emissor.EmitirAsync(userId, ip, ct)`; se falha → `new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas, Erros: r.Erros)` (o `ResultadoLoginDto` já tem `Erros`, adicionado na Task 2). O controller mapeia falha → 400 com os erros.

- [ ] **Step 1: Testes que falham**

`tests/BibliaTeologica.Application.Tests/Conta/ContaServiceTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta;
using BibliaTeologica.Application.Conta.Dtos;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Conta;

public class ContaServiceTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<IEmissorTokens> _emissor = new();
    private static readonly Guid Uid = Guid.NewGuid();

    private ContaService Criar() => new(_identity.Object, _emissor.Object);

    [Fact]
    public async Task TrocarSenhaAsync_ReemiteTokens_QuandoSucesso()
    {
        _identity.Setup(i => i.TrocarSenhaAsync(Uid, "atual", "NovaSenha!1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Ok());
        _emissor.Setup(e => e.EmitirAsync(Uid, "ip", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoLoginDto(EstadoLogin.Autenticado,
                Usuario: new UsuarioDto("a@b.c", "A", new[] { "Membro" }, false),
                Tokens: new TokensGerados("acc", DateTime.UtcNow, "ref", DateTime.UtcNow, "csrf")));

        var r = await Criar().TrocarSenhaAsync(Uid, new TrocarSenhaDto("atual", "NovaSenha!1"), "ip", default);

        r.Estado.Should().Be(EstadoLogin.Autenticado);
        r.Tokens!.AccessToken.Should().Be("acc");
    }

    [Fact]
    public async Task TrocarSenhaAsync_DevolveErros_QuandoIdentityFalha()
    {
        _identity.Setup(i => i.TrocarSenhaAsync(Uid, "errada", "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResultDto.Falha("Senha atual incorreta."));

        var r = await Criar().TrocarSenhaAsync(Uid, new TrocarSenhaDto("errada", "x"), null, default);

        r.Estado.Should().Be(EstadoLogin.CredenciaisInvalidas);
        r.Erros.Should().Contain("Senha atual incorreta.");
        _emissor.Verify(e => e.EmitirAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterAsync_MapeiaPerfil()
    {
        _identity.Setup(i => i.ObterUsuarioAsync(Uid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsuarioDto("a@b.c", "Ana", new[] { "Membro" }, false));

        var p = await Criar().ObterAsync(Uid, default);
        p!.Email.Should().Be("a@b.c");
        p.NomeExibicao.Should().Be("Ana");
    }
}
```

- [ ] **Step 2: Ver falhar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests --filter "FullyQualifiedName~ContaServiceTests"`
Expected: FAIL.

- [ ] **Step 3: Implementar**

`src/BibliaTeologica.Application/Conta/IContaService.cs`:

```csharp
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta.Dtos;

namespace BibliaTeologica.Application.Conta;

public interface IContaService
{
    Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct);
    Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct);
    Task<IdentityResultDto> TrocarEmailAsync(Guid userId, TrocarEmailDto dto, CancellationToken ct);
    Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct);
}
```

`src/BibliaTeologica.Application/Conta/ContaService.cs`:

```csharp
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Resultados;
using BibliaTeologica.Application.Conta.Dtos;

namespace BibliaTeologica.Application.Conta;

public class ContaService : IContaService
{
    private readonly IIdentityService _identity;
    private readonly IEmissorTokens _emissor;

    public ContaService(IIdentityService identity, IEmissorTokens emissor)
    {
        _identity = identity;
        _emissor = emissor;
    }

    public async Task<PerfilDto?> ObterAsync(Guid userId, CancellationToken ct)
    {
        var u = await _identity.ObterUsuarioAsync(userId, ct);
        if (u is null) return null;
        var criadoEm = await _identity.ObterCriadoEmAsync(userId, ct) ?? DateTime.UtcNow;
        return new PerfilDto(u.Email, u.NomeExibicao, u.Roles, criadoEm);
    }

    public Task AtualizarAsync(Guid userId, AtualizarPerfilDto dto, CancellationToken ct)
        => _identity.AtualizarNomeAsync(userId, dto.NomeExibicao, ct);

    public Task<IdentityResultDto> TrocarEmailAsync(Guid userId, TrocarEmailDto dto, CancellationToken ct)
        => _identity.TrocarEmailAsync(userId, dto.NovoEmail, dto.Senha, ct);

    public async Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto dto, string? ip, CancellationToken ct)
    {
        var r = await _identity.TrocarSenhaAsync(userId, dto.SenhaAtual, dto.NovaSenha, ct);
        if (!r.Sucesso)
            return new ResultadoLoginDto(EstadoLogin.CredenciaisInvalidas, Erros: r.Erros);
        return await _emissor.EmitirAsync(userId, ip, ct);
    }
}
```

> `IIdentityService.ObterCriadoEmAsync` já está no contrato (Task 2) e na implementação
> (Task 8): `(await _users.FindByIdAsync(userId.ToString()))?.CriadoEm`. O teste
> `ContaServiceTests.ObterAsync_MapeiaPerfil` faz `_identity.Setup(i => i.ObterCriadoEmAsync(...))`.

- [ ] **Step 4: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Application.Tests --filter "FullyQualifiedName~ContaServiceTests"`
Expected: PASS (3).

- [ ] **Step 5: DI + build da solução**

`DependencyInjection.cs` (Application):

```csharp
        services.AddScoped<Conta.IContaService, Conta.ContaService>();
```

Run: `dotnet build BibliaTeologica.sln` → `Build succeeded`.
Run: `dotnet test tests/BibliaTeologica.Application.Tests` → PASS.

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Application tests/BibliaTeologica.Application.Tests/Conta
git commit -m "feat(application): ContaService + EmissorTokens partilhado"
```

---

## Task 16: Application — `LivroCommandService`

**Files:**
- Create: `src/BibliaTeologica.Application/Livros/ILivroCommandService.cs`
- Create: `src/BibliaTeologica.Application/Livros/LivroCommandService.cs`
- Modify: `src/BibliaTeologica.Application/Common/Mapping/LivroMapper.cs` (+ `ToAdminListItem`, `ToAdminDetail`)
- Modify: `src/BibliaTeologica.Application/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Livros/LivroCommandServiceTests.cs`

**Interfaces:**
- Consumes: `ILivroRepository` estendido, `IProcessadorImagem`, `IArmazenamentoImagens`, `IUploadPolitica` (novo — encapsula `UploadOptions`), DTOs de admin (Task 2), `SecaoCanonicaExtensions.FromRotulo`.
- Produces:
  - `interface IUploadPolitica` (Application) `{ long TamanhoMaximoBytes; int DimensaoMaximaPx; IReadOnlyList<string> FormatosAceites; }` — implementação `UploadPolitica` na Infra a partir de `IOptions<UploadOptions>` (registada na Task 12; **adicionar lá** `services.AddSingleton<IUploadPolitica, UploadPolitica>()`).
  - `interface ILivroCommandService`:
    ```
    Task<IReadOnlyList<LivroAdminListItemDto>> ListarAsync(CancellationToken ct)
    Task<LivroAdminDetailDto?> ObterAsync(int id, CancellationToken ct)
    Task<int> CriarAsync(CriarLivroDto dto, CancellationToken ct)
    Task AtualizarAsync(int id, AtualizarLivroDto dto, CancellationToken ct)
    Task ApagarAsync(int id, CancellationToken ct)
    Task DefinirPublicacaoAsync(int id, bool publicado, CancellationToken ct)
    Task<string> DefinirCapaAsync(int id, Stream imagem, string contentType, CancellationToken ct)
    Task RemoverCapaAsync(int id, CancellationToken ct)
    ```
  - `class LivroCommandService : ILivroCommandService` — ctor `(ILivroRepository, IProcessadorImagem, IArmazenamentoImagens, IUploadPolitica, ILogger<LivroCommandService>)`.
  - Comportamento:
    - `CriarAsync` — resolve `SecaoCanonica` de `dto.Secao` (aceita nome do enum **ou** rótulo — tentar `Enum.TryParse` e depois `FromRotulo`); `slug = Slug.From(dto.Nome)`; se `repo.SlugExisteAsync(slug, null)` ou `repo.OrdemExisteAsync(dto.Ordem, null)` → `ConflitoException`; `Livro.Create(...)`; `repo.AdicionarAsync` + `repo.SalvarAsync`; devolve `livro.Id`.
    - `AtualizarAsync` — `repo.ObterPorIdAsync(id)` → null → `KeyNotFoundException`; conflito de slug/ordem excluindo `id` → `ConflitoException`; `livro.Atualizar(...)`; `repo.SalvarAsync`.
    - `ApagarAsync` — obter; null → `KeyNotFoundException`; guardar `capa = livro.CapaImagemPath`; `repo.Remover` + `SalvarAsync`; se `capa != null` → `armazenamento.RemoverAsync(capa)` (best-effort).
    - `DefinirPublicacaoAsync` — obter; `Publicar()`/`Despublicar()`; `SalvarAsync`.
    - `DefinirCapaAsync` — obter (`KeyNotFoundException` se null); ler os primeiros 16 bytes → `processador.AssinaturaValida` (`DomainException` se falhar); `stream.Position = 0`; `webp = processador.NormalizarAsync(stream, politica.DimensaoMaximaPx)`; `chave = armazenamento.GuardarAsync(webp)`; `antiga = livro.CapaImagemPath`; `livro.DefinirCapa(chave)`; `SalvarAsync`; se `antiga != null` → `armazenamento.RemoverAsync(antiga)`; devolve `chave`.
    - `RemoverCapaAsync` — obter; `antiga = livro.CapaImagemPath`; `livro.RemoverCapa()`; `SalvarAsync`; se antiga → `RemoverAsync`.
    - Validação de content-type/tamanho: o **controller** (Task 20) valida tamanho e content-type declarado antes de chamar; o serviço valida magic bytes.

- [ ] **Step 1: `IUploadPolitica` + `LivroMapper` admin**

`src/BibliaTeologica.Application/Common/Interfaces/IUploadPolitica.cs`:

```csharp
namespace BibliaTeologica.Application.Common.Interfaces;

public interface IUploadPolitica
{
    long TamanhoMaximoBytes { get; }
    int DimensaoMaximaPx { get; }
    IReadOnlyList<string> FormatosAceites { get; }
}
```

`src/BibliaTeologica.Infrastructure/Imagens/UploadPolitica.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Configuracao;
using Microsoft.Extensions.Options;

namespace BibliaTeologica.Infrastructure.Imagens;

public class UploadPolitica : IUploadPolitica
{
    private readonly UploadOptions _o;
    public UploadPolitica(IOptions<UploadOptions> o) => _o = o.Value;
    public long TamanhoMaximoBytes => _o.TamanhoMaximoBytes;
    public int DimensaoMaximaPx => _o.DimensaoMaximaPx;
    public IReadOnlyList<string> FormatosAceites => _o.FormatosAceites;
}
```

Adicionar em `AddInfrastructure` (Task 12): `services.AddSingleton<IUploadPolitica, UploadPolitica>();`

`LivroMapper.cs` — adicionar:

```csharp
    public static LivroAdminListItemDto ToAdminListItem(Livro l) =>
        new(l.Id, l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Publicado,
            l.CapaImagemPath is not null, l.AtualizadoEm);

    public static LivroAdminDetailDto ToAdminDetail(Livro l, Func<string?, string?> capaUrl) =>
        new(l.Id, l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo(),
            l.Quem, l.Porque, l.ParaQuem, l.Estrutura, l.Tema, l.Ideias, l.Versiculo,
            l.Publicado, capaUrl(l.CapaImagemPath), l.CriadoEm, l.AtualizadoEm);
```

- [ ] **Step 2: Testes que falham**

`tests/BibliaTeologica.Application.Tests/Livros/LivroCommandServiceTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Livros;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BibliaTeologica.Application.Tests.Livros;

public class LivroCommandServiceTests
{
    private readonly Mock<ILivroRepository> _repo = new();
    private readonly Mock<IProcessadorImagem> _img = new();
    private readonly Mock<IArmazenamentoImagens> _arm = new();
    private readonly Mock<IUploadPolitica> _pol = new();

    public LivroCommandServiceTests()
    {
        _pol.SetupGet(p => p.DimensaoMaximaPx).Returns(1200);
        _pol.SetupGet(p => p.FormatosAceites).Returns(new[] { "image/png" });
    }

    private LivroCommandService Criar() => new(_repo.Object, _img.Object, _arm.Object,
        _pol.Object, NullLogger<LivroCommandService>.Instance);

    private static CriarLivroDto DtoCriar(string nome = "GÊNESIS", int ordem = 1) =>
        new(nome, "Pentateuco", ordem, "q", "p", "pq", "e", "t", new[] { "i" }, "v");

    private static Livro LivroEx(string nome = "GÊNESIS", int ordem = 1)
        => Livro.Create(nome, SecaoCanonica.Pentateuco, ordem, "q", "p", "pq", "e", "t", new[] { "i" }, "v");

    [Fact]
    public async Task CriarAsync_PersisteEDevolveId()
    {
        _repo.Setup(r => r.SlugExisteAsync("genesis", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.OrdemExisteAsync(1, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Criar().CriarAsync(DtoCriar(), default);

        _repo.Verify(r => r.AdicionarAsync(It.Is<Livro>(l => l.Slug == "genesis"), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_LancaConflito_QuandoSlugExiste()
    {
        _repo.Setup(r => r.SlugExisteAsync("genesis", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var acao = () => Criar().CriarAsync(DtoCriar(), default);
        await acao.Should().ThrowAsync<ConflitoException>();
    }

    [Fact]
    public async Task AtualizarAsync_LancaKeyNotFound_QuandoNaoExiste()
    {
        _repo.Setup(r => r.ObterPorIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync((Livro?)null);
        var acao = () => Criar().AtualizarAsync(9, new AtualizarLivroDto("N", "Pentateuco", 1, "q", "p", "pq", "e", "t", new[] { "i" }, "v"), default);
        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApagarAsync_RemoveCapa_QuandoExiste()
    {
        var livro = LivroEx();
        livro.DefinirCapa("cap.webp");
        _repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(livro);

        await Criar().ApagarAsync(1, default);

        _repo.Verify(r => r.Remover(livro), Times.Once);
        _arm.Verify(a => a.RemoverAsync("cap.webp", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DefinirCapaAsync_RejeitaMagicBytesInvalidos()
    {
        _repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(LivroEx());
        _img.Setup(i => i.AssinaturaValida(It.IsAny<byte[]>(), "image/png")).Returns(false);

        using var s = new MemoryStream(new byte[] { 0, 1, 2, 3 });
        var acao = () => Criar().DefinirCapaAsync(1, s, "image/png", default);
        await acao.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DefinirCapaAsync_GuardaEDefine_QuandoValida()
    {
        var livro = LivroEx();
        _repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(livro);
        _img.Setup(i => i.AssinaturaValida(It.IsAny<byte[]>(), "image/png")).Returns(true);
        _img.Setup(i => i.NormalizarAsync(It.IsAny<Stream>(), 1200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 9 }));
        _arm.Setup(a => a.GuardarAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync("nova.webp");

        using var s = new MemoryStream(new byte[16]);
        var chave = await Criar().DefinirCapaAsync(1, s, "image/png", default);

        chave.Should().Be("nova.webp");
        livro.CapaImagemPath.Should().Be("nova.webp");
    }
}
```

> `IProcessadorImagem.AssinaturaValida` recebe `byte[]` (já definido assim na Task 2 e na
> Task 10) exatamente por causa deste mock — o Moq não consegue `It.IsAny<ReadOnlySpan<byte>>()`.

- [ ] **Step 3: Implementar `LivroCommandService`**

`src/BibliaTeologica.Application/Livros/ILivroCommandService.cs`:

```csharp
using BibliaTeologica.Application.Livros.Dtos;

namespace BibliaTeologica.Application.Livros;

public interface ILivroCommandService
{
    Task<IReadOnlyList<LivroAdminListItemDto>> ListarAsync(CancellationToken ct);
    Task<LivroAdminDetailDto?> ObterAsync(int id, CancellationToken ct);
    Task<int> CriarAsync(CriarLivroDto dto, CancellationToken ct);
    Task AtualizarAsync(int id, AtualizarLivroDto dto, CancellationToken ct);
    Task ApagarAsync(int id, CancellationToken ct);
    Task DefinirPublicacaoAsync(int id, bool publicado, CancellationToken ct);
    Task<string> DefinirCapaAsync(int id, Stream imagem, string contentType, CancellationToken ct);
    Task RemoverCapaAsync(int id, CancellationToken ct);
}
```

`src/BibliaTeologica.Application/Livros/LivroCommandService.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Application.Livros.Dtos;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using BibliaTeologica.Domain.Exceptions;
using BibliaTeologica.Domain.Slugs;
using Microsoft.Extensions.Logging;

namespace BibliaTeologica.Application.Livros;

public class LivroCommandService : ILivroCommandService
{
    private readonly ILivroRepository _repo;
    private readonly IProcessadorImagem _img;
    private readonly IArmazenamentoImagens _arm;
    private readonly IUploadPolitica _pol;
    private readonly ILogger<LivroCommandService> _log;

    public LivroCommandService(ILivroRepository repo, IProcessadorImagem img,
        IArmazenamentoImagens arm, IUploadPolitica pol, ILogger<LivroCommandService> log)
    {
        _repo = repo;
        _img = img;
        _arm = arm;
        _pol = pol;
        _log = log;
    }

    public async Task<IReadOnlyList<LivroAdminListItemDto>> ListarAsync(CancellationToken ct)
        => (await _repo.ListarTodosAsync(ct)).Select(LivroMapper.ToAdminListItem).ToList();

    public async Task<LivroAdminDetailDto?> ObterAsync(int id, CancellationToken ct)
    {
        var l = await _repo.ObterPorIdAsync(id, ct);
        return l is null ? null : LivroMapper.ToAdminDetail(l, CapaUrl);
    }

    public async Task<int> CriarAsync(CriarLivroDto dto, CancellationToken ct)
    {
        var secao = ResolverSecao(dto.Secao);
        var slug = Slug.From(dto.Nome);
        if (await _repo.SlugExisteAsync(slug, null, ct))
            throw new ConflitoException($"Já existe um livro com o slug '{slug}'.");
        if (await _repo.OrdemExisteAsync(dto.Ordem, null, ct))
            throw new ConflitoException($"Já existe um livro na ordem {dto.Ordem}.");

        var livro = Livro.Create(dto.Nome, secao, dto.Ordem, dto.Quem, dto.Porque, dto.ParaQuem,
            dto.Estrutura, dto.Tema, dto.Ideias, dto.Versiculo);
        await _repo.AdicionarAsync(livro, ct);
        await _repo.SalvarAsync(ct);
        return livro.Id;
    }

    public async Task AtualizarAsync(int id, AtualizarLivroDto dto, CancellationToken ct)
    {
        var livro = await _repo.ObterPorIdAsync(id, ct) ?? throw new KeyNotFoundException("Livro não encontrado.");
        var secao = ResolverSecao(dto.Secao);
        var slug = Slug.From(dto.Nome);
        if (await _repo.SlugExisteAsync(slug, id, ct))
            throw new ConflitoException($"Já existe um livro com o slug '{slug}'.");
        if (await _repo.OrdemExisteAsync(dto.Ordem, id, ct))
            throw new ConflitoException($"Já existe um livro na ordem {dto.Ordem}.");

        livro.Atualizar(dto.Nome, secao, dto.Ordem, dto.Quem, dto.Porque, dto.ParaQuem,
            dto.Estrutura, dto.Tema, dto.Ideias, dto.Versiculo);
        await _repo.SalvarAsync(ct);
    }

    public async Task ApagarAsync(int id, CancellationToken ct)
    {
        var livro = await _repo.ObterPorIdAsync(id, ct) ?? throw new KeyNotFoundException("Livro não encontrado.");
        var capa = livro.CapaImagemPath;
        _repo.Remover(livro);
        await _repo.SalvarAsync(ct);
        if (capa is not null) await RemoverFicheiroSeguro(capa, ct);
    }

    public async Task DefinirPublicacaoAsync(int id, bool publicado, CancellationToken ct)
    {
        var livro = await _repo.ObterPorIdAsync(id, ct) ?? throw new KeyNotFoundException("Livro não encontrado.");
        if (publicado) livro.Publicar(); else livro.Despublicar();
        await _repo.SalvarAsync(ct);
    }

    public async Task<string> DefinirCapaAsync(int id, Stream imagem, string contentType, CancellationToken ct)
    {
        var livro = await _repo.ObterPorIdAsync(id, ct) ?? throw new KeyNotFoundException("Livro não encontrado.");

        var cabecalho = new byte[16];
        var lidos = await imagem.ReadAsync(cabecalho.AsMemory(0, 16), ct);
        if (!_img.AssinaturaValida(cabecalho[..lidos], contentType))
            throw new DomainException("O ficheiro não é uma imagem válida ou não corresponde ao tipo declarado.");

        if (imagem.CanSeek) imagem.Position = 0;
        await using var webp = await _img.NormalizarAsync(imagem, _pol.DimensaoMaximaPx, ct);
        var chave = await _arm.GuardarAsync(webp, ct);

        var antiga = livro.CapaImagemPath;
        livro.DefinirCapa(chave);
        await _repo.SalvarAsync(ct);
        if (antiga is not null) await RemoverFicheiroSeguro(antiga, ct);
        return chave;
    }

    public async Task RemoverCapaAsync(int id, CancellationToken ct)
    {
        var livro = await _repo.ObterPorIdAsync(id, ct) ?? throw new KeyNotFoundException("Livro não encontrado.");
        var antiga = livro.CapaImagemPath;
        livro.RemoverCapa();
        await _repo.SalvarAsync(ct);
        if (antiga is not null) await RemoverFicheiroSeguro(antiga, ct);
    }

    private static string? CapaUrl(string? chave) => chave is null ? null : $"/api/media/livros/{chave}";

    private static SecaoCanonica ResolverSecao(string valor)
    {
        if (Enum.TryParse<SecaoCanonica>(valor, ignoreCase: true, out var e) && Enum.IsDefined(e))
            return e;
        try { return SecaoCanonicaExtensions.FromRotulo(valor); }
        catch (ArgumentException ex) { throw new DomainException(ex.Message); }
    }

    private async Task RemoverFicheiroSeguro(string chave, CancellationToken ct)
    {
        try { await _arm.RemoverAsync(chave, ct); }
        catch (Exception ex) { _log.LogWarning(ex, "Falha ao remover ficheiro de capa {Chave}", chave); }
    }
}
```

- [ ] **Step 4: Ver passar + DI**

`DependencyInjection.cs` (Application): `services.AddScoped<Livros.ILivroCommandService, Livros.LivroCommandService>();`

Run: `dotnet test tests/BibliaTeologica.Application.Tests` → PASS.
Run: `dotnet build BibliaTeologica.sln` → `Build succeeded`.

- [ ] **Step 5: `dotnet test` da solução (backend inteiro)**

Run: `dotnet test BibliaTeologica.sln`
Expected: PASS — exceto os testes de `Api.Tests` do M1 (falta config de auth; resolvido na Task 18).

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Application src/BibliaTeologica.Infrastructure/Imagens/UploadPolitica.cs tests/BibliaTeologica.Application.Tests/Livros
git commit -m "feat(application): LivroCommandService (CRUD + upload de capa) e mappers de admin"
```

---

## Task 17: Api — `UsuarioAtual`, `CookieWriter`, `CsrfActionFilter`

**Files:**
- Create: `src/BibliaTeologica.Api/Auth/UsuarioAtual.cs`
- Create: `src/BibliaTeologica.Api/Auth/CookieWriter.cs`
- Create: `src/BibliaTeologica.Api/Auth/CsrfActionFilter.cs`
- Test: `tests/BibliaTeologica.Api.Tests/Auth/CsrfActionFilterTests.cs`

**Interfaces:**
- Consumes: `IUsuarioAtual` (Task 2), `IHttpContextAccessor`, `TokensGerados` (Task 2).
- Produces:
  - `class UsuarioAtual : IUsuarioAtual` — ctor `(IHttpContextAccessor)`. `Id` de `sub`/`ClaimTypes.NameIdentifier`; `EhAdmin` de `User.IsInRole("Admin")`.
  - `class CookieWriter` — `Escrever(HttpResponse, TokensGerados)` grava `bt_access` (path `/`), `bt_refresh` (path `/api/auth`), `bt_csrf` (path `/`, `HttpOnly=false`); `Limpar(HttpResponse)` apaga os 3. Todos `Secure`, `SameSite=Strict`.
  - `class CsrfActionFilter : IAsyncActionFilter` — em métodos não-seguros (`POST/PUT/PATCH/DELETE`), exige header `X-CSRF-Token` == cookie `bt_csrf`; senão `403` (`ObjectResult` com `ProblemDetails`).

- [ ] **Step 1: `UsuarioAtual`**

```csharp
using System.Security.Claims;
using BibliaTeologica.Application.Common.Interfaces;

namespace BibliaTeologica.Api.Auth;

public class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _acessor;

    public UsuarioAtual(IHttpContextAccessor acessor) => _acessor = acessor;

    private ClaimsPrincipal? User => _acessor.HttpContext?.User;

    public bool EstaAutenticado => User?.Identity?.IsAuthenticated ?? false;
    public bool EhAdmin => User?.IsInRole("Admin") ?? false;
    public string? Email => User?.FindFirstValue("email");

    public Guid? Id
    {
        get
        {
            var raw = User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
```

- [ ] **Step 2: `CookieWriter`**

```csharp
using BibliaTeologica.Application.Auth.Dtos;

namespace BibliaTeologica.Api.Auth;

public class CookieWriter
{
    public const string Access = "bt_access";
    public const string Refresh = "bt_refresh";
    public const string Csrf = "bt_csrf";

    public void Escrever(HttpResponse res, TokensGerados t)
    {
        res.Cookies.Append(Access, t.AccessToken, Base(t.AccessExpiraEm, httpOnly: true, path: "/"));
        res.Cookies.Append(Refresh, t.RefreshToken, Base(t.RefreshExpiraEm, httpOnly: true, path: "/api/auth"));
        res.Cookies.Append(Csrf, t.Csrf, Base(t.RefreshExpiraEm, httpOnly: false, path: "/"));
    }

    public void Limpar(HttpResponse res)
    {
        foreach (var (nome, path) in new[] { (Access, "/"), (Refresh, "/api/auth"), (Csrf, "/") })
            res.Cookies.Append(nome, "", new CookieOptions
            {
                Expires = DateTimeOffset.UnixEpoch,
                HttpOnly = nome != Csrf,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = path
            });
    }

    private static CookieOptions Base(DateTime expira, bool httpOnly, string path) => new()
    {
        Expires = new DateTimeOffset(expira, TimeSpan.Zero),
        HttpOnly = httpOnly,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = path
    };
}
```

- [ ] **Step 3: Teste que falha — `CsrfActionFilter`**

`tests/BibliaTeologica.Api.Tests/Auth/CsrfActionFilterTests.cs`:

```csharp
using AwesomeAssertions;
using BibliaTeologica.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class CsrfActionFilterTests
{
    private static ActionExecutingContext Ctx(HttpContext http)
    {
        var actionCtx = new ActionContext(http, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionCtx, new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), controller: new object());
    }

    [Fact]
    public async Task Recusa403_QuandoHeaderNaoBateComCookie()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Headers["Cookie"] = "bt_csrf=abc";
        http.Request.Headers["X-CSRF-Token"] = "diferente";
        var ctx = Ctx(http);

        await new CsrfActionFilter().OnActionExecutionAsync(ctx, () => throw new Exception("não devia executar"));

        ctx.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Deixa_Passar_QuandoBate()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Headers["Cookie"] = "bt_csrf=abc";
        http.Request.Headers["X-CSRF-Token"] = "abc";
        var ctx = Ctx(http);
        var executou = false;

        await new CsrfActionFilter().OnActionExecutionAsync(ctx,
            () => { executou = true; return Task.FromResult<ActionExecutedContext>(null!); });

        executou.Should().BeTrue();
    }

    [Fact]
    public async Task Ignora_QuandoMetodoSeguro()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "GET";
        var ctx = Ctx(http);
        var executou = false;

        await new CsrfActionFilter().OnActionExecutionAsync(ctx,
            () => { executou = true; return Task.FromResult<ActionExecutedContext>(null!); });

        executou.Should().BeTrue();
    }
}
```

- [ ] **Step 4: Implementar `CsrfActionFilter`**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliaTeologica.Api.Auth;

public class CsrfActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> Seguros = new(StringComparer.OrdinalIgnoreCase)
        { "GET", "HEAD", "OPTIONS", "TRACE" };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var req = context.HttpContext.Request;
        if (!Seguros.Contains(req.Method))
        {
            var cookie = req.Cookies[CookieWriter.Csrf];
            var header = req.Headers["X-CSRF-Token"].ToString();
            if (string.IsNullOrEmpty(cookie) || !string.Equals(cookie, header, StringComparison.Ordinal))
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "CSRF",
                    Detail = "Token CSRF em falta ou inválido."
                }) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }
        }
        await next();
    }
}
```

- [ ] **Step 5: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests --filter "FullyQualifiedName~CsrfActionFilterTests"`
Expected: PASS (3).

- [ ] **Step 6: Commit**

```bash
git add src/BibliaTeologica.Api/Auth tests/BibliaTeologica.Api.Tests/Auth/CsrfActionFilterTests.cs
git commit -m "feat(api): UsuarioAtual, CookieWriter (3 cookies), CsrfActionFilter (double-submit)"
```

---

## Task 18: Api — `AuthController`, `Program.cs` (auth), middleware, factory de teste

> **Parte: M2a** (inteira). O Step 2 (mapa do `ExceptionHandlingMiddleware` com `ConflitoException`/`KeyNotFoundException`/`UnauthorizedAccessException`) fica no M2a — os `catch` são inócuos sem os endpoints do M2b e evitam retrabalho. Se por algum motivo ficarem por fazer, a Parte M2b apanha-os.

**Files:**
- Create: `src/BibliaTeologica.Api/Controllers/AuthController.cs`
- Modify: `src/BibliaTeologica.Api/Program.cs`
- Modify: `src/BibliaTeologica.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Modify: `src/BibliaTeologica.Api/appsettings.json`
- Create: `tests/BibliaTeologica.Api.Tests/AutenticacaoWebFactory.cs`
- Create: `tests/BibliaTeologica.Api.Tests/Auth/AuthEndpointsTests.cs`
- Modify: os testes de Api do M1 (usar a nova factory se partilharem base)

**Interfaces:**
- Consumes: `IAuthService`, `CookieWriter`, `CsrfActionFilter`, `IUsuarioAtual`.
- Produces:
  - `AuthController` (`api/auth`, `[AllowAnonymous]`, `[EnableRateLimiting("auth")]`) — endpoints da spec §8.
  - `Program.cs`: `AddHttpContextAccessor`, `AddScoped<IUsuarioAtual, UsuarioAtual>`, `AddSingleton<CookieWriter>`, `AddScoped<CsrfActionFilter>`; política de rate limit `auth`; `UseAuthentication()` + `UseAuthorization()`; startup chama `IdentitySeeder.SeedAsync()`.
  - `ExceptionHandlingMiddleware`: + `ConflitoException` → 409, `UnauthorizedAccessException` → 401.
  - `AutenticacaoWebFactory : WebApplicationFactory<Program>` — override `ConfigureWebHost`: config in-memory com `Jwt:SigningKey`, `Email:Sink=Dev`; substitui `IEmailSender` por um sink em memória (`OtpSink`); usa SQLite temp file único; seed de `admin@teste.local` / `membro@teste.local` (senha `Teste!Forte1`), ambos com `DeveTrocarSenha=false` e `EmailConfirmed=true`.

- [ ] **Step 1: `appsettings.json` (secções sem segredos)**

Adicionar ao `src/BibliaTeologica.Api/appsettings.json` (fundir com o existente):

```json
{
  "Jwt": { "Issuer": "BibliaTeologica", "Audience": "BibliaTeologica", "AccessTokenMinutes": 15, "RefreshTokenDays": 14 },
  "Identity": { "Lockout": { "MaxTentativas": 5, "Minutos": 15 } },
  "Email": { "Sink": "Dev", "Remetente": "nao-responder@biblia-teologica.local", "Smtp": { "Host": "", "Port": 587, "User": "", "From": "" } },
  "Armazenamento": { "CaminhoImagens": "/data/media/livros" },
  "Upload": { "TamanhoMaximoBytes": 2097152, "DimensaoMaximaPx": 1200, "FormatosAceites": ["image/jpeg", "image/png", "image/webp"] },
  "AuthRateLimiting": { "PermitLimit": 10, "WindowSeconds": 60 }
}
```

`appsettings.Development.json` (git-ignored) exemplo — documentar no README:

```json
{
  "ConnectionStrings": { "Default": "Data Source=./biblia.dev.db" },
  "Jwt": { "SigningKey": "GERE-UMA-CHAVE-ALEATORIA-COM-64-CHARS-openssl-rand-base64-48" },
  "AdminSeed": [ { "Email": "admin@local", "SenhaInicial": "Admin!Local1" } ]
}
```

- [ ] **Step 2: `ExceptionHandlingMiddleware` — novos casos**

Adicionar antes do `catch (Exception ex)`:

```csharp
        catch (BibliaTeologica.Domain.Exceptions.ConflitoException ex)
        {
            await EscreverProblema(context, StatusCodes.Status409Conflict, "Conflito", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await EscreverProblema(context, StatusCodes.Status404NotFound, "Não encontrado", ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await EscreverProblema(context, StatusCodes.Status401Unauthorized, "Não autorizado", "Autenticação necessária.");
        }
```

- [ ] **Step 3: `Program.cs` — auth e rate limit**

Alterações a `src/BibliaTeologica.Api/Program.cs`:

1. Depois de `builder.Services.AddInfrastructure(...)`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BibliaTeologica.Application.Common.Interfaces.IUsuarioAtual, BibliaTeologica.Api.Auth.UsuarioAtual>();
builder.Services.AddSingleton<BibliaTeologica.Api.Auth.CookieWriter>();
builder.Services.AddScoped<BibliaTeologica.Api.Auth.CsrfActionFilter>();
```

2. No `AddRateLimiter`, dentro do `options =>`, adicionar uma política nomeada:

```csharp
    var authLimit = builder.Configuration.GetValue<int?>("AuthRateLimiting:PermitLimit") ?? 10;
    var authWindow = builder.Configuration.GetValue<int?>("AuthRateLimiting:WindowSeconds") ?? 60;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authLimit, Window = TimeSpan.FromSeconds(authWindow), QueueLimit = 0
        }));
```

3. No bloco de startup (dentro do `using (var scope = ...)`), depois do `LivroSeeder`:

```csharp
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
```

(adicionar `using BibliaTeologica.Infrastructure.Persistence.Seed;` — já está.)

4. No pipeline, entre `app.UseCors("spa");` e `app.UseRateLimiter();`:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

- [ ] **Step 4: `AuthController`**

```csharp
using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly CookieWriter _cookies;

    public AuthController(IAuthService auth, CookieWriter cookies)
    {
        _auth = auth;
        _cookies = cookies;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var r = await _auth.LoginAsync(dto, Ip, ct);
        return r.Estado switch
        {
            EstadoLogin.Bloqueado => StatusCode(StatusCodes.Status423Locked,
                new ProblemDetails { Status = 423, Title = "Conta bloqueada", Detail = "Demasiadas tentativas. Tenta mais tarde." }),
            EstadoLogin.Desafio2fa => Ok(new { desafio2fa = true, tokenDesafio = r.TokenDesafio }),
            _ => Unauthorized(new ProblemDetails { Status = 401, Title = "Credenciais inválidas" })
        };
    }

    [HttpPost("login/2fa")]
    public async Task<IActionResult> Login2fa(Verificar2faDto dto, CancellationToken ct)
        => Finalizar(await _auth.Verificar2faAsync(dto, Ip, ct));

    [HttpPost("refresh")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var cru = Request.Cookies[CookieWriter.Refresh];
        if (string.IsNullOrEmpty(cru)) { _cookies.Limpar(Response); return Unauthorized(); }
        return Finalizar(await _auth.RefreshAsync(cru, Ip, ct));
    }

    [HttpPost("logout")]
    [Authorize]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _auth.LogoutAsync(Request.Cookies[CookieWriter.Refresh], ct);
        _cookies.Limpar(Response);
        return NoContent();
    }

    [HttpPost("registar")]
    public async Task<IActionResult> Registar(RegistarMembroDto dto, CancellationToken ct)
    {
        var r = await _auth.RegistarMembroAsync(dto, ct);
        return r.Sucesso
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails { Status = 400, Title = "Registo inválido", Detail = string.Join(" ", r.Erros) });
    }

    [HttpGet("eu")]
    [Authorize]
    public async Task<IActionResult> Eu([FromServices] IAuthService _, CancellationToken ct)
    {
        // O IUsuarioAtual já resolve do cookie; devolvemos os dados do token.
        var email = User.FindFirst("email")?.Value ?? "";
        var nome = User.FindFirst("name")?.Value ?? "";
        var roles = User.FindAll("role").Select(c => c.Value).ToArray();
        var deve = User.FindFirst("deve_trocar_senha")?.Value == "true";
        return Ok(new { email, nomeExibicao = nome, roles, deveTrocarSenha = deve });
    }

    private IActionResult Finalizar(ResultadoLoginDto r)
    {
        if (r.Estado != EstadoLogin.Autenticado || r.Tokens is null || r.Usuario is null)
        {
            _cookies.Limpar(Response);
            return r.Erros is { Count: > 0 }
                ? BadRequest(new ProblemDetails { Status = 400, Title = "Erro", Detail = string.Join(" ", r.Erros) })
                : Unauthorized(new ProblemDetails { Status = 401, Title = "Credenciais inválidas" });
        }
        _cookies.Escrever(Response, r.Tokens);
        return Ok(new
        {
            usuario = new { r.Usuario.Email, r.Usuario.NomeExibicao, r.Usuario.Roles, r.Usuario.DeveTrocarSenha }
        });
    }
}
```

- [ ] **Step 5: `AutenticacaoWebFactory`**

`tests/BibliaTeologica.Api.Tests/AutenticacaoWebFactory.cs`:

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Infrastructure.Identity;
using BibliaTeologica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BibliaTeologica.Api.Tests;

public sealed class OtpSink : IEmailSender
{
    public string? UltimoCorpo { get; private set; }
    public string? UltimoPara { get; private set; }
    public Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct)
    {
        UltimoPara = para;
        UltimoCorpo = corpoHtml;
        return Task.CompletedTask;
    }
    public string? UltimoCodigo => UltimoCorpo is null ? null
        : System.Text.RegularExpressions.Regex.Match(UltimoCorpo, @"<strong>([^<]+)</strong>").Groups[1].Value;
}

public class AutenticacaoWebFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@teste.local";
    public const string MembroEmail = "membro@teste.local";
    public const string Senha = "Teste!Forte1";

    public readonly OtpSink Otp = new();
    private readonly string _db = Path.Combine(Path.GetTempPath(), $"bt-api-{Guid.NewGuid():N}.db");
    private readonly string _media = Path.Combine(Path.GetTempPath(), $"bt-media-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = $"Data Source={_db}",
            ["Jwt:SigningKey"] = "chave-de-teste-com-mais-de-32-caracteres-para-hs256!!",
            ["Email:Sink"] = "Dev",
            ["Armazenamento:CaminhoImagens"] = _media,
            ["Cors:AllowedOrigins:0"] = "http://localhost:4300"
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Otp);
        });
    }

    protected override void ConfigureClient(HttpClient client) { }

    public async Task SemearUsuariosAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var r in new[] { "Admin", "Membro" })
            if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new IdentityRole<Guid>(r));

        await CriarSe(users, AdminEmail, "Admin");
        await CriarSe(users, MembroEmail, "Membro");
    }

    private static async Task CriarSe(UserManager<ApplicationUser> users, string email, string role)
    {
        if (await users.FindByEmailAsync(email) is not null) return;
        var u = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true, NomeExibicao = role };
        await users.CreateAsync(u, Senha);
        await users.AddToRoleAsync(u, role);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_db)) File.Delete(_db);
        if (Directory.Exists(_media)) Directory.Delete(_media, true);
    }
}
```

> `services.RemoveAll<T>()` vem de `Microsoft.Extensions.DependencyInjection.Extensions`.

- [ ] **Step 6: Testes de endpoint de auth**

`tests/BibliaTeologica.Api.Tests/Auth/AuthEndpointsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests.Auth;

public class AuthEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;
    public AuthEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    private async Task<HttpClient> ClienteSemeadoAsync()
    {
        await _f.SemearUsuariosAsync();
        return _f.CreateClient(new() { HandleCookies = true });
    }

    [Fact]
    public async Task Login_Retorna200ComDesafio_QuandoCredenciaisValidas()
    {
        var client = await ClienteSemeadoAsync();
        var res = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = AutenticacaoWebFactory.Senha });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        corpo!["desafio2fa"].ToString().Should().Be("True", "or true");
        _f.Otp.UltimoCodigo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Retorna401_QuandoSenhaErrada()
    {
        var client = await ClienteSemeadoAsync();
        var res = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = "errada" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FluxoCompleto_LoginE2fa_GravaCookiesEDaAcessoAoEu()
    {
        var client = await ClienteSemeadoAsync();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.MembroEmail, senha = AutenticacaoWebFactory.Senha });
        var td = (await login.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["tokenDesafio"].ToString();
        var codigo = _f.Otp.UltimoCodigo!;

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = td, codigo });
        v.StatusCode.Should().Be(HttpStatusCode.OK);
        v.Headers.Should().Contain(h => h.Key == "Set-Cookie" && h.Value.Any(c => c.StartsWith("bt_access=")));

        var eu = await client.GetAsync("/api/auth/eu");
        eu.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login2fa_Retorna401_QuandoCodigoErrado()
    {
        var client = await ClienteSemeadoAsync();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AutenticacaoWebFactory.AdminEmail, senha = AutenticacaoWebFactory.Senha });
        var td = (await login.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["tokenDesafio"].ToString();

        var v = await client.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = td, codigo = "000000" });
        v.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Eu_Retorna401_SemCookie()
    {
        await _f.SemearUsuariosAsync();
        var res = await _f.CreateClient().GetAsync("/api/auth/eu");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegistarMembro_Retorna201()
    {
        var client = await ClienteSemeadoAsync();
        var res = await client.PostAsJsonAsync("/api/auth/registar",
            new { email = $"novo-{Guid.NewGuid():N}@teste.local", senha = "Senha!Forte1", nomeExibicao = "Novo" });
        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

- [ ] **Step 7: Corrigir os testes de Api do M1**

Os testes do M1 (`LivrosEndpointsTests` ou similar) usam `WebApplicationFactory<Program>`
diretamente e vão falhar por falta de `Jwt:SigningKey`. Fazer um dos dois:
- (a) fazê-los herdar `IClassFixture<AutenticacaoWebFactory>` e usar `_f.CreateClient()`, ou
- (b) criar uma `WebFactoryBase` mínima que só injeta `Jwt:SigningKey` + SQLite temp, e ambas as
  fixtures (M1 e M2) derivam dela.

Escolher (b): extrair `WebFactoryBase` com a config in-memory comum; `AutenticacaoWebFactory : WebFactoryBase` acrescenta o `OtpSink` e o seed. Ajustar os testes do M1 para `IClassFixture<WebFactoryBase>`.

- [ ] **Step 8: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests`
Expected: PASS — testes do M1 + `AuthEndpointsTests` + `CsrfActionFilterTests`.

- [ ] **Step 9: `dotnet test` da solução**

Run: `dotnet test BibliaTeologica.sln`
Expected: PASS (tudo verde).

- [ ] **Step 10: Commit**

```bash
git add src/BibliaTeologica.Api tests/BibliaTeologica.Api.Tests
git commit -m "feat(api): AuthController (login 2fa, refresh, logout, registar, eu) + wiring de auth"
```

---

## Task 19: Api — `ContaController`

**Files:**
- Create: `src/BibliaTeologica.Api/Controllers/ContaController.cs`
- Test: `tests/BibliaTeologica.Api.Tests/Conta/ContaEndpointsTests.cs`

**Interfaces:**
- Consumes: `IContaService`, `IUsuarioAtual`, `CookieWriter`, `CsrfActionFilter`.
- Produces: `ContaController` (`api/conta`, `[Authorize]`) — endpoints da spec §8.

- [ ] **Step 1: `ContaController`**

```csharp
using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Auth.Dtos;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Conta;
using BibliaTeologica.Application.Conta.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/conta")]
[Authorize]
public class ContaController : ControllerBase
{
    private readonly IContaService _conta;
    private readonly IUsuarioAtual _usuario;
    private readonly CookieWriter _cookies;

    public ContaController(IContaService conta, IUsuarioAtual usuario, CookieWriter cookies)
    {
        _conta = conta;
        _usuario = usuario;
        _cookies = cookies;
    }

    private Guid Id => _usuario.Id ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var p = await _conta.ObterAsync(Id, ct);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPut]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> Atualizar(AtualizarPerfilDto dto, CancellationToken ct)
    {
        await _conta.AtualizarAsync(Id, dto, ct);
        return NoContent();
    }

    [HttpPut("email")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> TrocarEmail(TrocarEmailDto dto, CancellationToken ct)
    {
        var r = await _conta.TrocarEmailAsync(Id, dto, ct);
        return r.Sucesso ? NoContent()
            : BadRequest(new ProblemDetails { Status = 400, Title = "Erro", Detail = string.Join(" ", r.Erros) });
    }

    [HttpPost("senha")]
    [ServiceFilter(typeof(CsrfActionFilter))]
    public async Task<IActionResult> TrocarSenha(TrocarSenhaDto dto, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var r = await _conta.TrocarSenhaAsync(Id, dto, ip, ct);
        if (r.Estado != EstadoLogin.Autenticado || r.Tokens is null)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Erro", Detail = string.Join(" ", r.Erros ?? Array.Empty<string>()) });
        _cookies.Escrever(Response, r.Tokens);
        return Ok(new { usuario = new { r.Usuario!.Email, r.Usuario.NomeExibicao, r.Usuario.Roles, r.Usuario.DeveTrocarSenha } });
    }
}
```

- [ ] **Step 2: Testes**

`tests/BibliaTeologica.Api.Tests/Conta/ContaEndpointsTests.cs` — helper de login reutilizável
(extrair para `AutenticacaoWebFactory` um `Task<HttpClient> LoginAsync(email, senha)` que
faz login+2fa e devolve o cliente com cookies + injeta o header `X-CSRF-Token` do cookie
`bt_csrf` num `DelegatingHandler`).

```csharp
using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Xunit;

namespace BibliaTeologica.Api.Tests.Conta;

public class ContaEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;
    public ContaEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    [Fact]
    public async Task GetConta_Retorna200_ComPerfil()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        var res = await client.GetAsync("/api/conta");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["email"].ToString()
            .Should().Be(AutenticacaoWebFactory.MembroEmail);
    }

    [Fact]
    public async Task GetConta_Retorna401_SemAutenticacao()
    {
        await _f.SemearUsuariosAsync();
        (await _f.CreateClient().GetAsync("/api/conta")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutConta_Retorna403_SemHeaderCsrf()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha, comCsrf: false);
        var res = await client.PutAsJsonAsync("/api/conta", new { nomeExibicao = "Novo Nome" });
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostSenha_TrocaEReemiteCookies()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        var res = await client.PostAsJsonAsync("/api/conta/senha",
            new { senhaAtual = AutenticacaoWebFactory.Senha, novaSenha = "OutraSenha!2" });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Headers.Should().Contain(h => h.Key == "Set-Cookie" && h.Value.Any(c => c.StartsWith("bt_access=")));
    }
}
```

Adicionar ao `AutenticacaoWebFactory`:

```csharp
    public async Task<HttpClient> LoginAsync(string email, string senha, bool comCsrf = true)
    {
        await SemearUsuariosAsync();
        var client = CreateDefaultClient(new CsrfHandler(comCsrf));
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, senha });
        var td = (await login.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>())!["tokenDesafio"].GetString();
        await client.PostAsJsonAsync("/api/auth/login/2fa", new { tokenDesafio = td, codigo = Otp.UltimoCodigo });
        return client;
    }

    private sealed class CsrfHandler : DelegatingHandler
    {
        private readonly bool _ativo;
        private readonly CookieContainer _cookies = new();
        public CsrfHandler(bool ativo) { _ativo = ativo; InnerHandler = new HttpClientHandler { CookieContainer = _cookies, UseCookies = true }; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            if (_ativo && req.Method != HttpMethod.Get)
            {
                var csrf = _cookies.GetCookies(req.RequestUri!)["bt_csrf"]?.Value;
                if (csrf is not null) req.Headers.Add("X-CSRF-Token", csrf);
            }
            return await base.SendAsync(req, ct);
        }
    }
```

> `CreateDefaultClient(DelegatingHandler)` do `WebApplicationFactory` encadeia o handler
> sobre o `Server.CreateHandler()`. O `CookieContainer` acima é redundante com o do server —
> simplificar: usar `CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true })`
> e um handler que lê o cookie do `HttpResponseMessage` anterior. Ajustar na implementação até
> os testes de CSRF passarem; a ideia (header = cookie `bt_csrf`) é o que conta.

- [ ] **Step 3: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests --filter "FullyQualifiedName~ContaEndpointsTests"`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add src/BibliaTeologica.Api/Controllers/ContaController.cs tests/BibliaTeologica.Api.Tests
git commit -m "feat(api): ContaController (perfil, trocar email, trocar senha) + helpers de login nos testes"
```

---

## Task 20: Api — `LivrosAdminController`

**Files:**
- Create: `src/BibliaTeologica.Api/Controllers/LivrosAdminController.cs`
- Test: `tests/BibliaTeologica.Api.Tests/Admin/LivrosAdminEndpointsTests.cs`

**Interfaces:**
- Consumes: `ILivroCommandService`, `IUploadPolitica`, `CsrfActionFilter`.
- Produces: `LivrosAdminController` (`api/admin/livros`, `[Authorize(Roles="Admin")]`, `[ServiceFilter(typeof(CsrfActionFilter))]` na classe) — endpoints da spec §8.
  - O `PUT /{id:int}/capa` recebe `IFormFile arquivo`; `[RequestSizeLimit(2_500_000)]` + `[RequestFormLimits(MultipartBodyLengthLimit = 2_500_000)]`; valida `arquivo.Length` contra `politica.TamanhoMaximoBytes` e `arquivo.ContentType` contra `politica.FormatosAceites` (400 se falha); passa `arquivo.OpenReadStream()`.

- [ ] **Step 1: `LivrosAdminController`**

```csharp
using BibliaTeologica.Api.Auth;
using BibliaTeologica.Application.Common.Interfaces;
using BibliaTeologica.Application.Livros;
using BibliaTeologica.Application.Livros.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/admin/livros")]
[Authorize(Roles = "Admin")]
[ServiceFilter(typeof(CsrfActionFilter))]
public class LivrosAdminController : ControllerBase
{
    private readonly ILivroCommandService _livros;
    private readonly IUploadPolitica _politica;

    public LivrosAdminController(ILivroCommandService livros, IUploadPolitica politica)
    {
        _livros = livros;
        _politica = politica;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LivroAdminListItemDto>>> Listar(CancellationToken ct)
        => Ok(await _livros.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroAdminDetailDto>> Obter(int id, CancellationToken ct)
    {
        var l = await _livros.ObterAsync(id, ct);
        return l is null ? NotFound() : Ok(l);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarLivroDto dto, CancellationToken ct)
    {
        var id = await _livros.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(Obter), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarLivroDto dto, CancellationToken ct)
    {
        await _livros.AtualizarAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Apagar(int id, CancellationToken ct)
    {
        await _livros.ApagarAsync(id, ct);
        return NoContent();
    }

    public record PublicacaoDto(bool Publicado);

    [HttpPut("{id:int}/publicado")]
    public async Task<IActionResult> Publicacao(int id, PublicacaoDto dto, CancellationToken ct)
    {
        await _livros.DefinirPublicacaoAsync(id, dto.Publicado, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/capa")]
    [RequestSizeLimit(2_500_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2_500_000)]
    public async Task<IActionResult> Capa(int id, IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(Problema("Ficheiro em falta."));
        if (arquivo.Length > _politica.TamanhoMaximoBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, Problema("Ficheiro demasiado grande."));
        if (!_politica.FormatosAceites.Contains(arquivo.ContentType))
            return BadRequest(Problema($"Tipo não aceite: {arquivo.ContentType}."));

        await using var stream = arquivo.OpenReadStream();
        var chave = await _livros.DefinirCapaAsync(id, stream, arquivo.ContentType, ct);
        return Ok(new { capaUrl = $"/api/media/livros/{chave}" });
    }

    [HttpDelete("{id:int}/capa")]
    public async Task<IActionResult> RemoverCapa(int id, CancellationToken ct)
    {
        await _livros.RemoverCapaAsync(id, ct);
        return NoContent();
    }

    private static ProblemDetails Problema(string detalhe) => new() { Status = 400, Title = "Upload inválido", Detail = detalhe };
}
```

- [ ] **Step 2: Testes**

`tests/BibliaTeologica.Api.Tests/Admin/LivrosAdminEndpointsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace BibliaTeologica.Api.Tests.Admin;

public class LivrosAdminEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;
    public LivrosAdminEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    private static ByteArrayContent Png(int w = 800, int h = 600, string contentType = "image/png")
    {
        using var img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(w, h);
        using var ms = new MemoryStream();
        img.Save(ms, new PngEncoder());
        var c = new ByteArrayContent(ms.ToArray());
        c.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return c;
    }

    [Fact]
    public async Task Listar_Retorna401_SemCookie()
    {
        await _f.SemearUsuariosAsync();
        (await _f.CreateClient().GetAsync("/api/admin/livros")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Listar_Retorna403_ComCookieMembro()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.MembroEmail, AutenticacaoWebFactory.Senha);
        (await client.GetAsync("/api/admin/livros")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Listar_Retorna200_ComCookieAdmin()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);
        (await client.GetAsync("/api/admin/livros")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Criar_Retorna403_SemHeaderCsrf()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha, comCsrf: false);
        var res = await client.PostAsJsonAsync("/api/admin/livros", NovoLivro("TESTE-A", 40));
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Criar_Retorna409_QuandoSlugDuplicado()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);
        await client.PostAsJsonAsync("/api/admin/livros", NovoLivro("DUP", 41));
        var res = await client.PostAsJsonAsync("/api/admin/livros", NovoLivro("DUP", 42));
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PutCapa_Retorna400_QuandoMagicBytesInvalidos()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);
        var id = await CriarLivro(client, "COM-CAPA-A", 43);

        var form = new MultipartFormDataContent();
        var falso = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
        falso.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(falso, "arquivo", "x.png");

        var res = await client.PutAsync($"/api/admin/livros/{id}/capa", form);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutCapa_Retorna200ComCapaUrl_QuandoImagemValida()
    {
        var client = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);
        var id = await CriarLivro(client, "COM-CAPA-B", 44);

        var form = new MultipartFormDataContent { { Png(), "arquivo", "capa.png" } };
        var res = await client.PutAsync($"/api/admin/livros/{id}/capa", form);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var corpo = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        corpo!["capaUrl"].Should().MatchRegex(@"^/api/media/livros/[0-9a-f]{32}\.webp$");
    }

    private static object NovoLivro(string nome, int ordem) => new
    {
        nome, secao = "Profetas Menores", ordem,
        quem = "q", porque = "p", paraQuem = "pq", estrutura = "e", tema = "t",
        ideias = new[] { "i1" }, versiculo = "v"
    };

    private static async Task<int> CriarLivro(HttpClient client, string nome, int ordem)
    {
        var res = await client.PostAsJsonAsync("/api/admin/livros", NovoLivro(nome, ordem));
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<Dictionary<string, int>>())!["id"];
    }
}
```

> `MultipartFormDataContent` com `HandleCookies`: o `CsrfHandler` da Task 19 adiciona o header
> `X-CSRF-Token` também nos `PUT` multipart — confirmar que funciona; senão adicionar o header
> manualmente no teste.

- [ ] **Step 3: Ver passar**

Run: `dotnet test tests/BibliaTeologica.Api.Tests --filter "FullyQualifiedName~LivrosAdminEndpointsTests"`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add src/BibliaTeologica.Api/Controllers/LivrosAdminController.cs tests/BibliaTeologica.Api.Tests/Admin
git commit -m "feat(api): LivrosAdminController (CRUD + publicar + upload/remover capa)"
```

---

## Task 21: Api — `MediaController` + `LivrosController` público com `CapaUrl`

**Files:**
- Create: `src/BibliaTeologica.Api/Controllers/MediaController.cs`
- Modify: `src/BibliaTeologica.Application/Common/Mapping/LivroMapper.cs` (montar `CapaUrl`)
- Modify: `src/BibliaTeologica.Application/Livros/LivroQueryService.cs` (nada — o mapper já monta)
- Test: `tests/BibliaTeologica.Api.Tests/Media/MediaEndpointsTests.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Common/LivroMapperTests.cs` (existe? senão criar)

**Interfaces:**
- Consumes: `IArmazenamentoImagens`.
- Produces:
  - `MediaController` (`api/media`, `[AllowAnonymous]`) — `GET /livros/{arquivo}` faz stream com `image/webp` + `Cache-Control: public, max-age=604800, immutable`; `NotFound()` se `AbrirAsync` devolver null.
  - `LivroMapper.ToListItem` / `ToDetail` — `CapaUrl` = `chave is null ? null : $"/api/media/livros/{chave}"`.

- [ ] **Step 1: `MediaController`**

```csharp
using BibliaTeologica.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliaTeologica.Api.Controllers;

[ApiController]
[Route("api/media")]
[AllowAnonymous]
public class MediaController : ControllerBase
{
    private readonly IArmazenamentoImagens _armazenamento;

    public MediaController(IArmazenamentoImagens armazenamento) => _armazenamento = armazenamento;

    [HttpGet("livros/{arquivo}")]
    public async Task<IActionResult> Livro(string arquivo, CancellationToken ct)
    {
        var stream = await _armazenamento.AbrirAsync(arquivo, ct);
        if (stream is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=604800, immutable";
        return File(stream, "image/webp");
    }
}
```

- [ ] **Step 2: `LivroMapper` — montar o `CapaUrl`**

Substituir `ToListItem` / `ToDetail` em `LivroMapper.cs`:

```csharp
    public static LivroListItemDto ToListItem(Livro l) =>
        new(l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo(), CapaUrl(l.CapaImagemPath));

    public static LivroDetailDto ToDetail(Livro l) =>
        new(l.Slug, l.Nome, l.Ordem, l.Secao.ToString(), l.Secao.Rotulo(), l.Testamento.ToString(),
            l.Quem, l.Porque, l.ParaQuem, l.Estrutura, l.Tema, l.Ideias, l.Versiculo, CapaUrl(l.CapaImagemPath));

    private static string? CapaUrl(string? chave) => chave is null ? null : $"/api/media/livros/{chave}";
```

- [ ] **Step 3: Testes**

`tests/BibliaTeologica.Application.Tests/Common/LivroMapperTests.cs` (adicionar ou criar):

```csharp
using AwesomeAssertions;
using BibliaTeologica.Application.Common.Mapping;
using BibliaTeologica.Domain.Entities;
using BibliaTeologica.Domain.Enums;
using Xunit;

namespace BibliaTeologica.Application.Tests.Common;

public class LivroMapperTests
{
    private static Livro Livro()
        => BibliaTeologica.Domain.Entities.Livro.Create("GÊNESIS", SecaoCanonica.Pentateuco, 1,
            "q", "p", "pq", "e", "t", new[] { "i" }, "v");

    [Fact]
    public void ToDetail_CapaUrlNula_QuandoSemCapa()
        => LivroMapper.ToDetail(Livro()).CapaUrl.Should().BeNull();

    [Fact]
    public void ToDetail_MontaCapaUrl_QuandoTemCapa()
    {
        var l = Livro();
        l.DefinirCapa("abc.webp");
        LivroMapper.ToDetail(l).CapaUrl.Should().Be("/api/media/livros/abc.webp");
    }
}
```

`tests/BibliaTeologica.Api.Tests/Media/MediaEndpointsTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace BibliaTeologica.Api.Tests.Media;

public class MediaEndpointsTests : IClassFixture<AutenticacaoWebFactory>
{
    private readonly AutenticacaoWebFactory _f;
    public MediaEndpointsTests(AutenticacaoWebFactory f) => _f = f;

    [Fact]
    public async Task GetMediaLivros_Retorna200ImageWebp_AposUpload()
    {
        var admin = await _f.LoginAsync(AutenticacaoWebFactory.AdminEmail, AutenticacaoWebFactory.Senha);
        var criar = await admin.PostAsJsonAsync("/api/admin/livros", new
        {
            nome = "MEDIA-A", secao = "Profetas Menores", ordem = 50,
            quem = "q", porque = "p", paraQuem = "pq", estrutura = "e", tema = "t",
            ideias = new[] { "i" }, versiculo = "v"
        });
        var id = (await criar.Content.ReadFromJsonAsync<Dictionary<string, int>>())!["id"];

        using var img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(300, 200);
        using var ms = new MemoryStream();
        img.Save(ms, new PngEncoder());
        var png = new ByteArrayContent(ms.ToArray());
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var up = await admin.PutAsync($"/api/admin/livros/{id}/capa", new MultipartFormDataContent { { png, "arquivo", "c.png" } });
        var capaUrl = (await up.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["capaUrl"];

        var anon = _f.CreateClient();
        var res = await anon.GetAsync(capaUrl);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
    }

    [Fact]
    public async Task GetMediaLivros_Retorna404_QuandoNaoExisteOuTraversal()
    {
        await _f.SemearUsuariosAsync();
        var anon = _f.CreateClient();
        (await anon.GetAsync("/api/media/livros/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.webp")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
        (await anon.GetAsync("/api/media/livros/..%2f..%2fsecret.webp")).StatusCode
            .Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
```

- [ ] **Step 4: Ver passar**

Run: `dotnet test BibliaTeologica.sln`
Expected: PASS (solução inteira).

- [ ] **Step 5: Commit**

```bash
git add src/BibliaTeologica.Api/Controllers/MediaController.cs src/BibliaTeologica.Application/Common/Mapping/LivroMapper.cs tests/BibliaTeologica.Api.Tests/Media tests/BibliaTeologica.Application.Tests/Common/LivroMapperTests.cs
git commit -m "feat(api): MediaController (serve capas WebP) + CapaUrl nos DTOs públicos"
```

---

## Task 22: Docker — volume de media, `.env`, `chown`, exercício manual

> **Parte: M2a + M2b (dividida).**
> **M2a:** Step 1 `.gitignore` só `.env`; Step 2 `.env.example` (`BT_JWT_KEY`, `BT_ADMIN_EMAIL`, `BT_ADMIN_SENHA`); Step 4 `docker-compose.yml` só as linhas `Jwt__SigningKey`/`AdminSeed__0__*`/`Email__Sink`. Exercício manual: login → 2FA → `/conta`.
> **M2b:** `.gitignore` `.media/`; Step 3 `chown` no Dockerfile; Step 4 volume `biblia-media` + `Armazenamento__CaminhoImagens`; Step 5 `docker-compose.prod.yml` SMTP; exercício manual do upload de capa.

**Files:**
- Modify: `src/BibliaTeologica.Api/Dockerfile`
- Modify: `docker-compose.yml`
- Modify: `docker-compose.prod.yml`
- Create: `.env.example`
- Modify: `.gitignore` (`.env`, `.media/`)

**Interfaces:**
- Consumes: as secções de config do M2.
- Produces: `docker compose up --build` sobe api (:5080) + web (:4300) com volume `biblia-media`; login → editar → upload → ver capa no site funciona.

- [ ] **Step 1: `.gitignore`**

Adicionar:

```gitignore
.env
.media/
```

- [ ] **Step 2: `.env.example`**

```dotenv
# Copiar para .env e preencher. NÃO commitar o .env.
BT_JWT_KEY=gera-uma-chave-aleatoria-com-64-chars-ex-openssl-rand-base64-48
BT_ADMIN_EMAIL=admin@local
BT_ADMIN_SENHA=Admin!Local1
```

- [ ] **Step 3: `Dockerfile` da API — diretório de media**

No estágio `runtime` de `src/BibliaTeologica.Api/Dockerfile`, antes do `ENTRYPOINT`:

```dockerfile
RUN mkdir -p /data/media/livros && chown -R $APP_UID:$APP_UID /data
```

(`$APP_UID` é definido pela imagem base `aspnet:8.0`. Se não existir, usar `chown -R 1654:1654 /data` ou `USER root` + `mkdir` + `USER $APP_UID`.)

- [ ] **Step 4: `docker-compose.yml`**

No serviço `api`, `environment:` — juntar às linhas existentes:

```yaml
      Jwt__SigningKey: "${BT_JWT_KEY:?defina BT_JWT_KEY no .env}"
      AdminSeed__0__Email: "${BT_ADMIN_EMAIL:?defina BT_ADMIN_EMAIL no .env}"
      AdminSeed__0__SenhaInicial: "${BT_ADMIN_SENHA:?defina BT_ADMIN_SENHA no .env}"
      Armazenamento__CaminhoImagens: "/data/media/livros"
      Email__Sink: "Dev"
```

`volumes:` do `api`:

```yaml
    volumes:
      - biblia-data:/data
      - biblia-media:/data/media
```

E no bloco `volumes:` de topo:

```yaml
volumes:
  biblia-data:
  biblia-media:
```

- [ ] **Step 5: `docker-compose.prod.yml`**

Adicionar ao serviço `api`:

```yaml
    environment:
      Email__Sink: "Smtp"
      Email__Smtp__Host: "${BT_SMTP_HOST}"
      Email__Smtp__User: "${BT_SMTP_USER}"
      Email__Smtp__Password: "${BT_SMTP_PASS}"
      Email__Smtp__From: "${BT_SMTP_FROM}"
```

- [ ] **Step 6: Exercício manual (teste do utilizador — guardrail de Qualidade)**

```bash
cp .env.example .env      # e preencher BT_JWT_KEY etc.
docker compose down -v
docker compose up --build -d
sleep 8
curl -fs http://localhost:5080/health && echo " API OK"
curl -fs http://localhost:4300/ | grep -qi "<app-root" && echo " web OK"
```

Depois, no navegador (http://localhost:4300):
1. `/entrar` → login com o admin do `.env` → recebe OTP (ver `docker compose logs api | grep EMAIL`) → mete o código.
2. É redirecionado para `/conta/trocar-senha` (admin seed tem `DeveTrocarSenha`). Trocar.
3. `/admin/livros` → editar GÊNESIS → carregar uma imagem de capa → guardar.
4. Abrir `/biblia/genesis` no site público → a capa aparece.
5. `/registar` um membro → login → `/conta` mostra o perfil, `/admin` dá 403 (não é Admin).

- [ ] **Step 7: `docker compose down`**

```bash
docker compose down
```

- [ ] **Step 8: Commit**

```bash
git add src/BibliaTeologica.Api/Dockerfile docker-compose.yml docker-compose.prod.yml .env.example .gitignore
git commit -m "build: volume biblia-media, .env obrigatório (JWT+admin seed), chown no Dockerfile"
```

---

## Task 23: Frontend — `@angular/material`, modelos e `AuthStore`/`AuthService`

Trabalhar em `frontend/biblia-teologica-web/`.

**Files:**
- Modify: `package.json` (via `ng add`), `angular.json`, `src/styles.scss`
- Create: `src/app/core/auth/auth.models.ts`
- Create: `src/app/core/auth/auth.store.ts`
- Create: `src/app/core/auth/auth.service.ts`
- Test: `src/app/core/auth/auth.store.spec.ts`, `src/app/core/auth/auth.service.spec.ts`

**Interfaces:**
- Consumes: `HttpClient`, `environment.apiBaseUrl` (`/api`).
- Produces:
  - `interface Usuario { email; nomeExibicao; roles: string[]; deveTrocarSenha: boolean }`
  - `interface DesafioLogin { desafio2fa: true; tokenDesafio: string }`
  - `AuthStore` (`providedIn: 'root'`): `usuario = signal<Usuario | null>(null)`, `carregado = signal(false)`, computeds `estaAutenticado`, `ehAdmin`, `deveTrocarSenha`; métodos `definir(u)`, `limpar()`.
  - `AuthService` (`providedIn: 'root'`): `login(email, senha): Observable<DesafioLogin>`, `verificar2fa(tokenDesafio, codigo): Observable<Usuario>`, `registar(dto): Observable<void>`, `logout(): Observable<void>`, `carregarUsuario(): Observable<Usuario | null>`. Todos escrevem no `AuthStore` onde faz sentido.

- [ ] **Step 1: `ng add @angular/material`**

Run: `npx ng add @angular/material --skip-confirmation --theme=custom --typography --animations=enabled`
Expected: adiciona `@angular/material` + `@angular/cdk` ao `package.json`, cria/ajusta
`src/styles.scss` com o tema, e mete `provideAnimationsAsync()` no `app.config.ts`.

> Se o `ng add` tocar em ficheiros do M1 (ex.: `styles.scss` global do pergaminho), **rever
> o diff**: manter as `@use` do pergaminho e o tema Material lado a lado. O tema Material só
> deve aplicar dentro de `.mat-*` — não deve alterar o `body` nem a tipografia do fichário.
> Se necessário, isolar o tema num ficheiro `src/styles/material.scss` importado só no
> `angular.json` a seguir ao `styles.scss`.

- [ ] **Step 2: `auth.models.ts`**

```ts
export interface Usuario {
  email: string;
  nomeExibicao: string;
  roles: string[];
  deveTrocarSenha: boolean;
}

export interface DesafioLogin {
  desafio2fa: true;
  tokenDesafio: string;
}
```

- [ ] **Step 3: `auth.store.ts`**

```ts
import { Injectable, computed, signal } from '@angular/core';
import type { Usuario } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly _usuario = signal<Usuario | null>(null);
  private readonly _carregado = signal(false);

  readonly usuario = this._usuario.asReadonly();
  readonly carregado = this._carregado.asReadonly();

  readonly estaAutenticado = computed(() => this._usuario() !== null);
  readonly ehAdmin = computed(() => this._usuario()?.roles.includes('Admin') ?? false);
  readonly deveTrocarSenha = computed(() => this._usuario()?.deveTrocarSenha ?? false);

  definir(u: Usuario): void {
    this._usuario.set(u);
    this._carregado.set(true);
  }

  limpar(): void {
    this._usuario.set(null);
    this._carregado.set(true);
  }

  marcarCarregado(): void {
    this._carregado.set(true);
  }
}
```

- [ ] **Step 4: Testes que falham — `auth.service.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { AuthStore } from './auth.store';

describe('AuthService', () => {
  let svc: AuthService;
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    svc = TestBed.inject(AuthService);
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  it('login devolve o desafio', () => {
    let td: string | undefined;
    svc.login('a@b.c', 'x').subscribe((r) => (td = r.tokenDesafio));
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T1' });
    expect(td).toBe('T1');
  });

  it('verificar2fa popula o store', () => {
    svc.verificar2fa('T1', '123456').subscribe();
    http.expectOne('/api/auth/login/2fa').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Admin'], deveTrocarSenha: false },
    });
    expect(store.usuario()?.email).toBe('a@b.c');
    expect(store.ehAdmin()).toBe(true);
  });

  it('carregarUsuario com 401 limpa o store e marca carregado', () => {
    svc.carregarUsuario().subscribe();
    http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(store.usuario()).toBeNull();
    expect(store.carregado()).toBe(true);
  });

  it('logout limpa o store', () => {
    store.definir({ email: 'a@b.c', nomeExibicao: 'A', roles: [], deveTrocarSenha: false });
    svc.logout().subscribe();
    http.expectOne('/api/auth/logout').flush(null);
    expect(store.usuario()).toBeNull();
  });
});
```

- [ ] **Step 5: `auth.service.ts`**

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from './auth.store';
import type { DesafioLogin, Usuario } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly base = `${environment.apiBaseUrl}/auth`;

  login(email: string, senha: string): Observable<DesafioLogin> {
    return this.http.post<DesafioLogin>(`${this.base}/login`, { email, senha });
  }

  verificar2fa(tokenDesafio: string, codigo: string): Observable<Usuario> {
    return this.http
      .post<{ usuario: Usuario }>(`${this.base}/login/2fa`, { tokenDesafio, codigo })
      .pipe(
        map((r) => r.usuario),
        tap((u) => this.store.definir(u)),
      );
  }

  registar(dto: { email: string; senha: string; nomeExibicao: string }): Observable<void> {
    return this.http.post<void>(`${this.base}/registar`, dto);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, {}).pipe(tap(() => this.store.limpar()));
  }

  carregarUsuario(): Observable<Usuario | null> {
    return this.http.get<Usuario>(`${this.base}/eu`).pipe(
      tap((u) => this.store.definir(u)),
      catchError(() => {
        this.store.limpar();
        return of(null);
      }),
    );
  }
}
```

- [ ] **Step 6: `auth.store.spec.ts`**

```ts
import { AuthStore } from './auth.store';

describe('AuthStore', () => {
  it('computeds refletem o utilizador', () => {
    const s = new AuthStore();
    expect(s.estaAutenticado()).toBe(false);
    s.definir({ email: 'a@b.c', nomeExibicao: 'A', roles: ['Membro'], deveTrocarSenha: true });
    expect(s.estaAutenticado()).toBe(true);
    expect(s.ehAdmin()).toBe(false);
    expect(s.deveTrocarSenha()).toBe(true);
    s.limpar();
    expect(s.usuario()).toBeNull();
  });
});
```

- [ ] **Step 7: Ver passar**

Run: `npm test`
Expected: PASS (auth.store + auth.service + os do M1).

- [ ] **Step 8: Commit**

```bash
git add package.json package-lock.json angular.json src/styles.scss src/app/core/auth
git commit -m "feat(web): @angular/material + AuthStore/AuthService (login 2fa, hidratação, logout)"
```

---

## Task 24: Frontend — interceptors (withCredentials, CSRF, refresh)

**Files:**
- Create: `src/app/core/interceptors/with-credentials.interceptor.ts`
- Create: `src/app/core/interceptors/csrf.interceptor.ts`
- Create: `src/app/core/interceptors/refresh.interceptor.ts`
- Create: `src/app/core/interceptors/lercookie.ts` (helper)
- Test: `src/app/core/interceptors/interceptors.spec.ts`

**Interfaces:**
- Consumes: `AuthStore`, `Router`, `environment`.
- Produces: 3 `HttpInterceptorFn`.
  - `withCredentialsInterceptor` — `req.clone({ withCredentials: true })` para URLs que começam com `/api`.
  - `csrfInterceptor` — se método ∈ {POST, PUT, PATCH, DELETE} e URL `/api`, adiciona header `X-CSRF-Token` do cookie `bt_csrf`.
  - `refreshInterceptor` — em `401` de URL que **não** é `/api/auth/*`: partilha um único `POST /api/auth/refresh` (via variável de módulo `refreshEmCurso$`), e em sucesso repete o pedido; em falha limpa o store e navega para `/entrar`.

- [ ] **Step 1: helper `lerCookie`**

```ts
export function lerCookie(nome: string): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|; )${nome}=([^;]*)`));
  return match ? decodeURIComponent(match[1]) : null;
}
```

- [ ] **Step 2: `with-credentials.interceptor.ts`**

```ts
import { HttpInterceptorFn } from '@angular/common/http';

export const withCredentialsInterceptor: HttpInterceptorFn = (req, next) =>
  req.url.startsWith('/api') ? next(req.clone({ withCredentials: true })) : next(req);
```

- [ ] **Step 3: `csrf.interceptor.ts`**

```ts
import { HttpInterceptorFn } from '@angular/common/http';
import { lerCookie } from './lercookie';

const INSEGUROS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('/api') && INSEGUROS.has(req.method)) {
    const csrf = lerCookie('bt_csrf');
    if (csrf) return next(req.clone({ setHeaders: { 'X-CSRF-Token': csrf } }));
  }
  return next(req);
};
```

- [ ] **Step 4: Teste que falha — `interceptors.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { csrfInterceptor } from './csrf.interceptor';
import { refreshInterceptor } from './refresh.interceptor';
import { AuthStore } from '../auth/auth.store';

describe('interceptors', () => {
  let http: HttpClient;
  let ctrl: HttpTestingController;

  beforeEach(() => {
    document.cookie = 'bt_csrf=CSRF123';
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([csrfInterceptor, refreshInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    ctrl = TestBed.inject(HttpTestingController);
  });

  it('csrf: adiciona X-CSRF-Token em POST /api', () => {
    http.post('/api/x', {}).subscribe();
    const req = ctrl.expectOne('/api/x');
    expect(req.request.headers.get('X-CSRF-Token')).toBe('CSRF123');
    req.flush({});
  });

  it('refresh: em 401 tenta refresh e repete', () => {
    http.get('/api/admin/livros').subscribe({ next: () => {}, error: () => {} });
    ctrl.expectOne('/api/admin/livros').flush(null, { status: 401, statusText: 'Unauthorized' });
    ctrl.expectOne('/api/auth/refresh').flush({ usuario: { email: 'a', nomeExibicao: 'a', roles: [], deveTrocarSenha: false } });
    ctrl.expectOne('/api/admin/livros').flush([]);
  });

  it('refresh: em 401 + refresh falhado navega para /entrar', () => {
    const nav = spyOn(TestBed.inject(Router), 'navigate');
    http.get('/api/admin/livros').subscribe({ next: () => {}, error: () => {} });
    ctrl.expectOne('/api/admin/livros').flush(null, { status: 401, statusText: 'Unauthorized' });
    ctrl.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(nav).toHaveBeenCalledWith(['/entrar']);
    expect(TestBed.inject(AuthStore).usuario()).toBeNull();
  });
});
```

- [ ] **Step 5: `refresh.interceptor.ts`**

```ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';

let refreshEmCurso: Observable<unknown> | null = null;

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const store = inject(AuthStore);

  const ehAuth = req.url.startsWith('/api/auth/');
  return next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401 || ehAuth) {
        return throwError(() => err);
      }
      refreshEmCurso ??= next(req.clone({ url: '/api/auth/refresh', method: 'POST', body: {} }));
      return refreshEmCurso.pipe(
        switchMap(() => {
          refreshEmCurso = null;
          return next(req);
        }),
        catchError((e) => {
          refreshEmCurso = null;
          store.limpar();
          void router.navigate(['/entrar']);
          return throwError(() => e);
        }),
      );
    }),
  );
};
```

> `next(req.clone({ url, method, body }))` — `HttpRequest.clone` aceita `method`/`body`.
> Se o teste da repetição falhar por `next` só poder ser chamado uma vez por request,
> usar `HttpClient` injetado dentro do interceptor para o `refresh` em vez de `next`.
> Ajustar até o `interceptors.spec.ts` passar.

- [ ] **Step 6: Ver passar**

Run: `npm test`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/app/core/interceptors
git commit -m "feat(web): interceptors withCredentials + CSRF + refresh com repetição"
```

---

## Task 25: Frontend — guards, `app.config.ts`, `app.routes.ts`

**Files:**
- Create: `src/app/core/auth/guards/auth.guard.ts`
- Create: `src/app/core/auth/guards/admin.guard.ts`
- Modify: `src/app/app.config.ts`
- Modify: `src/app/app.routes.ts`
- Test: `src/app/core/auth/guards/guards.spec.ts`

**Interfaces:**
- Consumes: `AuthStore`, `AuthService`, `Router`.
- Produces:
  - `authGuard: CanActivateFn` — se `!store.carregado()` → `authService.carregarUsuario()` (via `firstValueFrom` / `toObservable`), depois: autenticado → `true`; senão → `router.parseUrl('/entrar')`.
  - `adminGuard: CanActivateFn` — como `authGuard` + `store.ehAdmin()`; se `deveTrocarSenha()` → `router.parseUrl('/conta/trocar-senha')`; se autenticado mas não admin → `router.parseUrl('/')`.
  - `app.config.ts` — `provideHttpClient(withFetch(), withInterceptors([withCredentialsInterceptor, csrfInterceptor, refreshInterceptor]))`, `provideAnimationsAsync()`, e um `provideAppInitializer` que corre `carregarUsuario()` uma vez.
  - `app.routes.ts` — rotas lazy `entrar`, `registar`, `conta`, `admin`.

- [ ] **Step 1: guards**

`src/app/core/auth/guards/auth.guard.ts`:

```ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth.service';
import { AuthStore } from '../auth.store';

async function garantirCarregado(): Promise<void> {
  const store = inject(AuthStore);
  if (!store.carregado()) {
    await firstValueFrom(inject(AuthService).carregarUsuario());
  }
}

export const authGuard: CanActivateFn = async () => {
  const store = inject(AuthStore);
  const router = inject(Router);
  await garantirCarregado();
  return store.estaAutenticado() ? true : router.parseUrl('/entrar');
};

export const adminGuard: CanActivateFn = async () => {
  const store = inject(AuthStore);
  const router = inject(Router);
  await garantirCarregado();
  if (!store.estaAutenticado()) return router.parseUrl('/entrar');
  if (store.deveTrocarSenha()) return router.parseUrl('/conta/trocar-senha');
  return store.ehAdmin() ? true : router.parseUrl('/');
};
```

> `inject()` dentro de uma função async chamada por um `CanActivateFn` só funciona no
> tick síncrono inicial. Refatorar: fazer os `inject()` no topo do guard e passar as
> instâncias a `garantirCarregado(store, service)`. Ajustar até `guards.spec.ts` passar.

- [ ] **Step 2: Teste que falha — `guards.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { adminGuard, authGuard } from './auth.guard';
import { AuthStore } from '../auth.store';

function run(guard: typeof authGuard) {
  return TestBed.runInInjectionContext(() => guard({} as any, {} as any));
}

describe('guards', () => {
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  it('authGuard: hidrata e bloqueia anónimo', async () => {
    const p = run(authGuard);
    http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
    const r = await p;
    expect(TestBed.inject(Router).serializeUrl(r as any)).toBe('/entrar');
  });

  it('adminGuard: membro vai para /', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Membro'], deveTrocarSenha: false });
    const r = await run(adminGuard);
    expect(TestBed.inject(Router).serializeUrl(r as any)).toBe('/');
  });

  it('adminGuard: admin passa', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Admin'], deveTrocarSenha: false });
    expect(await run(adminGuard)).toBe(true);
  });

  it('adminGuard: deveTrocarSenha vai para /conta/trocar-senha', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Admin'], deveTrocarSenha: true });
    const r = await run(adminGuard);
    expect(TestBed.inject(Router).serializeUrl(r as any)).toBe('/conta/trocar-senha');
  });
});
```

- [ ] **Step 3: `app.config.ts`**

```ts
import { ApplicationConfig, provideAppInitializer, provideBrowserGlobalErrorListeners, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { withCredentialsInterceptor } from './core/interceptors/with-credentials.interceptor';
import { csrfInterceptor } from './core/interceptors/csrf.interceptor';
import { refreshInterceptor } from './core/interceptors/refresh.interceptor';
import { AuthService } from './core/auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withFetch(),
      withInterceptors([withCredentialsInterceptor, csrfInterceptor, refreshInterceptor]),
    ),
    provideAppInitializer(() => firstValueFrom(inject(AuthService).carregarUsuario())),
  ],
};
```

- [ ] **Step 4: `app.routes.ts`**

```ts
import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/auth/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'biblia' },
  { path: 'biblia', loadChildren: () => import('./features/biblia/biblia.routes').then((m) => m.bibliaRoutes) },
  { path: 'entrar', loadComponent: () => import('./features/auth/entrar/entrar.component').then((m) => m.EntrarComponent) },
  { path: 'registar', loadComponent: () => import('./features/auth/registar/registar.component').then((m) => m.RegistarComponent) },
  { path: 'conta', canActivate: [authGuard], loadChildren: () => import('./features/conta/conta.routes').then((m) => m.contaRoutes) },
  { path: 'admin', canActivate: [adminGuard], loadChildren: () => import('./features/admin/admin.routes').then((m) => m.adminRoutes) },
  { path: '**', redirectTo: 'biblia' },
];
```

- [ ] **Step 5: Ver passar (tsc + testes)**

Run: `npx tsc --noEmit -p tsconfig.app.json`
Expected: **FALHA** — as rotas apontam para componentes que ainda não existem (Tasks 26–29).
**Isto é esperado.** Criar stubs mínimos para desbloquear o build, ou fazer esta task
**depois** das 26–29. **Decisão:** mover o Step 4 (`app.routes.ts`) para o fim da Task 29;
aqui só entram os guards + `app.config.ts` + `guards.spec.ts`.

Run: `npm test -- src/app/core` (ou `npm test` inteiro) → guards passam.

- [ ] **Step 6: Commit**

```bash
git add src/app/core/auth/guards src/app/app.config.ts
git commit -m "feat(web): guards authGuard/adminGuard + app.config (interceptors, animations, initializer)"
```

---

## Task 26: Frontend — páginas `/entrar` e `/registar`

**Files:**
- Create: `src/app/features/auth/entrar/entrar.component.{ts,html,scss}`
- Create: `src/app/features/auth/registar/registar.component.{ts,html,scss}`
- Test: `src/app/features/auth/entrar/entrar.component.spec.ts`

**Interfaces:**
- Consumes: `AuthService`, `AuthStore`, `Router`, Material (`MatFormField`, `MatInput`, `MatButton`, `MatCard`).
- Produces:
  - `EntrarComponent` (`bt-entrar`) — 2 fases via `signal<'credenciais' | 'codigo'>`. Fase 1: form email/senha → `authService.login` → guarda `tokenDesafio` num signal, passa à fase 2. Fase 2: input do código → `verificar2fa` → sucesso: se `store.deveTrocarSenha()` navega `/conta/trocar-senha`, senão navega para `retorno` (query param) ou `/admin` se admin senão `/conta`. Erros mostram `mensagemErro` signal.
  - `RegistarComponent` (`bt-registar`) — form email/senha/nome → `registar` → sucesso: navega `/entrar` com aviso; 400 mostra os erros.

- [ ] **Step 1: `EntrarComponent`**

`entrar.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'bt-entrar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './entrar.component.html',
  styleUrl: './entrar.component.scss',
})
export class EntrarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  readonly fase = signal<'credenciais' | 'codigo'>('credenciais');
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);
  private tokenDesafio = '';

  readonly formCredenciais = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });
  readonly formCodigo = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.minLength(4)]],
  });

  submeterCredenciais(): void {
    if (this.formCredenciais.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { email, senha } = this.formCredenciais.getRawValue();
    this.auth.login(email, senha).subscribe({
      next: (r) => {
        this.tokenDesafio = r.tokenDesafio;
        this.fase.set('codigo');
        this.aEnviar.set(false);
      },
      error: (e) => {
        this.erro.set(e.status === 423 ? 'Conta bloqueada temporariamente.' : 'Credenciais inválidas.');
        this.aEnviar.set(false);
      },
    });
  }

  submeterCodigo(): void {
    if (this.formCodigo.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth.verificar2fa(this.tokenDesafio, this.formCodigo.getRawValue().codigo).subscribe({
      next: () => {
        this.aEnviar.set(false);
        if (this.store.deveTrocarSenha()) {
          void this.router.navigate(['/conta/trocar-senha']);
          return;
        }
        const retorno = this.rota.snapshot.queryParamMap.get('retorno');
        void this.router.navigateByUrl(retorno ?? (this.store.ehAdmin() ? '/admin' : '/conta'));
      },
      error: () => {
        this.erro.set('Código inválido ou expirado.');
        this.aEnviar.set(false);
      },
    });
  }
}
```

`entrar.component.html`:

```html
<mat-card class="cartao">
  <h1>Entrar</h1>

  @if (erro(); as e) { <p class="erro" role="alert">{{ e }}</p> }

  @if (fase() === 'credenciais') {
    <form [formGroup]="formCredenciais" (ngSubmit)="submeterCredenciais()">
      <mat-form-field appearance="outline">
        <mat-label>Email</mat-label>
        <input matInput type="email" formControlName="email" autocomplete="username" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Senha</mat-label>
        <input matInput type="password" formControlName="senha" autocomplete="current-password" />
      </mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="aEnviar()">Continuar</button>
    </form>
  } @else {
    <p>Enviámos um código para o seu email.</p>
    <form [formGroup]="formCodigo" (ngSubmit)="submeterCodigo()">
      <mat-form-field appearance="outline">
        <mat-label>Código</mat-label>
        <input matInput inputmode="numeric" formControlName="codigo" autocomplete="one-time-code" />
      </mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="aEnviar()">Entrar</button>
    </form>
  }

  <a routerLink="/registar" class="ligacao">Criar conta</a>
</mat-card>
```

> Adicionar `RouterLink` aos `imports` do componente (o `<a routerLink>` no template).

`entrar.component.scss` — layout centrado simples:

```scss
:host {
  display: grid;
  place-items: center;
  min-height: 100vh;
  padding: 24px;
}
.cartao {
  width: 100%;
  max-width: 380px;
  padding: 28px;
}
form { display: flex; flex-direction: column; gap: 8px; }
mat-form-field { width: 100%; }
.erro { color: var(--mat-sys-error, #b3261e); margin: 0 0 12px; }
.ligacao { display: inline-block; margin-top: 16px; }
```

- [ ] **Step 2: `RegistarComponent`**

`registar.component.ts` — análogo, um só form (email, senha, nomeExibicao); no sucesso
`this.router.navigate(['/entrar'], { queryParams: { registado: 1 } })`; no `error` com
`status === 400` mostra `e.error?.detail`. Template Material igual em estilo. `bt-registar`.

```ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'bt-registar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './registar.component.html',
  styleUrl: './registar.component.scss',
})
export class RegistarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);

  readonly form = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(8)]],
  });

  submeter(): void {
    if (this.form.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth.registar(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigate(['/entrar'], { queryParams: { registado: 1 } }),
      error: (e) => {
        this.erro.set(e.error?.detail ?? 'Não foi possível criar a conta.');
        this.aEnviar.set(false);
      },
    });
  }
}
```

(`registar.component.html`/`.scss` seguem o padrão do `EntrarComponent`.)

- [ ] **Step 3: Teste — `entrar.component.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { EntrarComponent } from './entrar.component';

describe('EntrarComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideNoopAnimations()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('passa da fase de credenciais para a de código', () => {
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    const c = fixture.componentInstance;

    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });

    expect(c.fase()).toBe('codigo');
  });

  it('mostra erro quando o login falha', () => {
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    const c = fixture.componentInstance;
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(c.erro()).toContain('Credenciais');
  });
});
```

- [ ] **Step 4: Ver passar**

Run: `npm test`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/app/features/auth
git commit -m "feat(web): páginas /entrar (2 fases: senha -> OTP) e /registar"
```

---

## Task 27: Frontend — `/conta` (perfil + trocar senha)

**Files:**
- Create: `src/app/features/conta/conta.routes.ts`
- Create: `src/app/features/conta/perfil/perfil.component.{ts,html,scss}`
- Create: `src/app/features/conta/trocar-senha/trocar-senha.component.{ts,html,scss}`
- Create: `src/app/core/services/conta.service.ts`
- Test: `src/app/features/conta/perfil/perfil.component.spec.ts`

**Interfaces:**
- Consumes: Material, `AuthStore`, `Router`.
- Produces:
  - `ContaService` — `obter(): Observable<Perfil>`, `atualizarNome(nome): Observable<void>`, `trocarEmail({novoEmail,senha}): Observable<void>`, `trocarSenha({senhaAtual,novaSenha}): Observable<Usuario>` (`{ usuario }` → mapeia; atualiza `AuthStore`).
  - `contaRoutes` — `''` → `PerfilComponent`, `trocar-senha` → `TrocarSenhaComponent`.
  - `PerfilComponent` (`bt-conta-perfil`) — mostra email + roles; form do nome (guarda com `atualizarNome`); form de troca de email (novoEmail + senha); ligação para `trocar-senha`.
  - `TrocarSenhaComponent` (`bt-trocar-senha`) — form senhaAtual + novaSenha + confirmar; no sucesso navega para `/conta` (ou `/admin` se admin) e mostra toast.

- [ ] **Step 1: `Conta` models + `ContaService`**

`src/app/core/services/conta.service.ts`:

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../auth/auth.store';
import type { Usuario } from '../auth/auth.models';

export interface Perfil {
  email: string;
  nomeExibicao: string;
  roles: string[];
  criadoEm: string;
}

@Injectable({ providedIn: 'root' })
export class ContaService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly base = `${environment.apiBaseUrl}/conta`;

  obter(): Observable<Perfil> {
    return this.http.get<Perfil>(this.base);
  }

  atualizarNome(nomeExibicao: string): Observable<void> {
    return this.http.put<void>(this.base, { nomeExibicao });
  }

  trocarEmail(dto: { novoEmail: string; senha: string }): Observable<void> {
    return this.http.put<void>(`${this.base}/email`, dto);
  }

  trocarSenha(dto: { senhaAtual: string; novaSenha: string }): Observable<Usuario> {
    return this.http.post<{ usuario: Usuario }>(`${this.base}/senha`, dto).pipe(
      map((r) => r.usuario),
      tap((u) => this.store.definir(u)),
    );
  }
}
```

- [ ] **Step 2: `contaRoutes`**

```ts
import { Routes } from '@angular/router';

export const contaRoutes: Routes = [
  { path: '', loadComponent: () => import('./perfil/perfil.component').then((m) => m.PerfilComponent) },
  { path: 'trocar-senha', loadComponent: () => import('./trocar-senha/trocar-senha.component').then((m) => m.TrocarSenhaComponent) },
];
```

- [ ] **Step 3: `PerfilComponent` + `TrocarSenhaComponent`**

`perfil.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ContaService } from '../../../core/services/conta.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'bt-conta-perfil',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.scss',
})
export class PerfilComponent {
  private readonly fb = inject(FormBuilder);
  private readonly conta = inject(ContaService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly perfil = toSignal(this.conta.obter());
  readonly msgNome = signal<string | null>(null);
  readonly msgEmail = signal<string | null>(null);

  readonly formNome = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
  });
  readonly formEmail = this.fb.nonNullable.group({
    novoEmail: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });

  constructor() {
    // preenche o nome quando o perfil chega
    queueMicrotask(() => {
      const p = this.perfil();
      if (p) this.formNome.patchValue({ nomeExibicao: p.nomeExibicao });
    });
  }

  guardarNome(): void {
    if (this.formNome.invalid) return;
    this.conta.atualizarNome(this.formNome.getRawValue().nomeExibicao).subscribe({
      next: () => this.msgNome.set('Nome guardado.'),
      error: () => this.msgNome.set('Erro ao guardar.'),
    });
  }

  guardarEmail(): void {
    if (this.formEmail.invalid) return;
    this.conta.trocarEmail(this.formEmail.getRawValue()).subscribe({
      next: () => this.msgEmail.set('Email alterado. Use-o no próximo login.'),
      error: (e) => this.msgEmail.set(e.error?.detail ?? 'Não foi possível alterar o email.'),
    });
  }

  sair(): void {
    this.auth.logout().subscribe(() => void this.router.navigate(['/']));
  }
}
```

> Alternativa ao `queueMicrotask`/`toSignal`: usar `this.conta.obter().subscribe(p => { ... })`
> no construtor e um `signal` do perfil. Escolher o que fizer o `perfil.component.spec.ts` passar.

`perfil.component.html` — `@if (perfil(); as p)` mostra `p.email`, `p.roles`; `<form [formGroup]="formNome">`
com o campo do nome + `guardarNome()` + `{{ msgNome() }}`; `<form [formGroup]="formEmail">` com
`novoEmail` + `senha` + `guardarEmail()`; `<a routerLink="trocar-senha">Trocar senha</a>`; `<button (click)="sair()">Sair</button>`.

`TrocarSenhaComponent` (`bt-trocar-senha`) — form `senhaAtual`, `novaSenha`, `confirmar`
(validador de igualdade); `contaService.trocarSenha` → sucesso navega
`store.ehAdmin() ? '/admin' : '/conta'`; erro mostra `e.error?.detail`.

```ts
// trocar-senha.component.ts
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ContaService } from '../../../core/services/conta.service';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'bt-trocar-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './trocar-senha.component.html',
  styleUrl: './trocar-senha.component.scss',
})
export class TrocarSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly conta = inject(ContaService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);

  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      senhaAtual: ['', Validators.required],
      novaSenha: ['', [Validators.required, Validators.minLength(8)]],
      confirmar: ['', Validators.required],
    },
    { validators: (g: AbstractControl) => (g.get('novaSenha')!.value === g.get('confirmar')!.value ? null : { diferentes: true }) },
  );

  submeter(): void {
    if (this.form.invalid) return;
    const { senhaAtual, novaSenha } = this.form.getRawValue();
    this.conta.trocarSenha({ senhaAtual, novaSenha }).subscribe({
      next: () => void this.router.navigateByUrl(this.store.ehAdmin() ? '/admin' : '/conta'),
      error: (e) => this.erro.set(e.error?.detail ?? 'Não foi possível trocar a senha.'),
    });
  }
}
```

- [ ] **Step 4: Teste — `perfil.component.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { PerfilComponent } from './perfil.component';

describe('PerfilComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideNoopAnimations()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('mostra o email do perfil', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush({ email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], criadoEm: '2026-01-01' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('a@b.c');
  });
});
```

- [ ] **Step 5: Ver passar + Commit**

Run: `npm test` → PASS.

```bash
git add src/app/features/conta src/app/core/services/conta.service.ts
git commit -m "feat(web): área /conta — perfil (nome, email) e troca de senha"
```

---

## Task 28: Frontend — shell `/admin` + lista de livros (MatTable)

**Files:**
- Create: `src/app/features/admin/admin.routes.ts`
- Create: `src/app/features/admin/admin-shell/admin-shell.component.{ts,html,scss}`
- Create: `src/app/features/admin/livros-lista/livros-lista.component.{ts,html,scss}`
- Create: `src/app/core/services/livros-admin.service.ts`
- Test: `src/app/features/admin/livros-lista/livros-lista.component.spec.ts`

**Interfaces:**
- Consumes: Material (`MatToolbar`, `MatSidenav`, `MatList`, `MatTable`, `MatButton`, `MatIcon`, `MatFormField` para o filtro), `AuthService`, `AuthStore`, `Router`.
- Produces:
  - `LivrosAdminService` — `listar(): Observable<LivroAdminListItem[]>`, `obter(id): Observable<LivroAdminDetail>`, `criar(dto): Observable<{id:number}>`, `atualizar(id, dto): Observable<void>`, `apagar(id): Observable<void>`, `definirPublicado(id, publicado): Observable<void>`, `enviarCapa(id, file): Observable<{capaUrl:string}>` (FormData), `removerCapa(id): Observable<void>`. Tipos em `livros-admin.models.ts`.
  - `adminRoutes` — `''` → `AdminShellComponent` com filhos: `''` redirect `livros`, `livros` → `LivrosListaComponent`, `livros/novo` → `LivroFormComponent`, `livros/:id` → `LivroFormComponent`.
  - `AdminShellComponent` (`bt-admin-shell`) — `mat-toolbar` ("Bíblia Teológica · Admin"), botão "Ver site", menu do utilizador (nome de `store.usuario()`, "Sair"), `mat-sidenav` com links (Livros), `<router-outlet />`.
  - `LivrosListaComponent` (`bt-admin-livros-lista`) — `MatTable` com colunas `ordem`, `nome`, `secao`, `publicado` (toggle), `capa` (ícone se tem), `acoes` (Editar, Apagar com confirm via `window.confirm` — **evitar** dialogs Material bloqueantes nos testes; usar `MatDialog` mas mockável). Filtro por nome (input → `filtro` signal → `computed`).

- [ ] **Step 1: modelos + `LivrosAdminService`**

`src/app/core/services/livros-admin.models.ts`:

```ts
export interface LivroAdminListItem {
  id: number;
  slug: string;
  nome: string;
  ordem: number;
  secao: string;
  publicado: boolean;
  temCapa: boolean;
  atualizadoEm: string;
}

export interface LivroAdminDetail {
  id: number;
  slug: string;
  nome: string;
  ordem: number;
  secao: string;
  secaoRotulo: string;
  quem: string;
  porque: string;
  paraQuem: string;
  estrutura: string;
  tema: string;
  ideias: string[];
  versiculo: string;
  publicado: boolean;
  capaUrl: string | null;
  criadoEm: string;
  atualizadoEm: string;
}

export interface LivroFormValor {
  nome: string;
  secao: string;
  ordem: number;
  quem: string;
  porque: string;
  paraQuem: string;
  estrutura: string;
  tema: string;
  ideias: string[];
  versiculo: string;
}
```

`src/app/core/services/livros-admin.service.ts`:

```ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { LivroAdminDetail, LivroAdminListItem, LivroFormValor } from './livros-admin.models';

@Injectable({ providedIn: 'root' })
export class LivrosAdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/admin/livros`;

  listar(): Observable<LivroAdminListItem[]> {
    return this.http.get<LivroAdminListItem[]>(this.base);
  }
  obter(id: number): Observable<LivroAdminDetail> {
    return this.http.get<LivroAdminDetail>(`${this.base}/${id}`);
  }
  criar(dto: LivroFormValor): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, dto);
  }
  atualizar(id: number, dto: LivroFormValor): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, dto);
  }
  apagar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
  definirPublicado(id: number, publicado: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/publicado`, { publicado });
  }
  enviarCapa(id: number, ficheiro: File): Observable<{ capaUrl: string }> {
    const fd = new FormData();
    fd.append('arquivo', ficheiro);
    return this.http.put<{ capaUrl: string }>(`${this.base}/${id}/capa`, fd);
  }
  removerCapa(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}/capa`);
  }
}
```

- [ ] **Step 2: `adminRoutes` + `AdminShellComponent`**

```ts
// admin.routes.ts
import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./admin-shell/admin-shell.component').then((m) => m.AdminShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'livros' },
      { path: 'livros', loadComponent: () => import('./livros-lista/livros-lista.component').then((m) => m.LivrosListaComponent) },
      { path: 'livros/novo', loadComponent: () => import('./livro-form/livro-form.component').then((m) => m.LivroFormComponent) },
      { path: 'livros/:id', loadComponent: () => import('./livro-form/livro-form.component').then((m) => m.LivroFormComponent) },
    ],
  },
];
```

`AdminShellComponent` — `mat-toolbar` + `mat-sidenav-container` + `<router-outlet />`;
`nome = computed(() => this.store.usuario()?.nomeExibicao)`; `sair()` → `authService.logout()`
→ `router.navigate(['/'])`.

- [ ] **Step 3: `LivrosListaComponent`**

```ts
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { LivrosAdminService } from '../../../core/services/livros-admin.service';
import type { LivroAdminListItem } from '../../../core/services/livros-admin.models';

@Component({
  selector: 'bt-admin-livros-lista',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatSlideToggleModule, MatFormFieldModule, MatInputModule],
  templateUrl: './livros-lista.component.html',
  styleUrl: './livros-lista.component.scss',
})
export class LivrosListaComponent {
  private readonly svc = inject(LivrosAdminService);

  readonly colunas = ['ordem', 'nome', 'secao', 'publicado', 'capa', 'acoes'];
  readonly filtro = signal('');
  private readonly _livros = signal<LivroAdminListItem[]>([]);
  readonly livros = this._livros.asReadonly();

  readonly filtrados = computed(() => {
    const t = this.filtro().trim().toLowerCase();
    return t ? this._livros().filter((l) => l.nome.toLowerCase().includes(t)) : this._livros();
  });

  constructor() {
    this.svc.listar().subscribe((ls) => this._livros.set(ls));
  }

  alternarPublicado(l: LivroAdminListItem): void {
    this.svc.definirPublicado(l.id, !l.publicado).subscribe(() => {
      this._livros.update((ls) => ls.map((x) => (x.id === l.id ? { ...x, publicado: !x.publicado } : x)));
    });
  }

  apagar(l: LivroAdminListItem): void {
    if (!confirm(`Apagar "${l.nome}"?`)) return;
    this.svc.apagar(l.id).subscribe(() => {
      this._livros.update((ls) => ls.filter((x) => x.id !== l.id));
    });
  }
}
```

`livros-lista.component.html` — `<mat-form-field>` do filtro + `<table mat-table [dataSource]="filtrados()">` com as colunas; `[routerLink]="['/admin/livros', l.id]"` no Editar; `<a routerLink="/admin/livros/novo" mat-flat-button>Novo livro</a>`.

- [ ] **Step 4: Teste — `livros-lista.component.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { LivrosListaComponent } from './livros-lista.component';

describe('LivrosListaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LivrosListaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideNoopAnimations()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lista e filtra', () => {
    const fixture = TestBed.createComponent(LivrosListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/admin/livros').flush([
      { id: 1, slug: 'genesis', nome: 'GÊNESIS', ordem: 1, secao: 'Pentateuco', publicado: true, temCapa: false, atualizadoEm: '' },
      { id: 2, slug: 'exodo', nome: 'ÊXODO', ordem: 2, secao: 'Pentateuco', publicado: false, temCapa: false, atualizadoEm: '' },
    ]);
    fixture.detectChanges();
    expect(fixture.componentInstance.filtrados().length).toBe(2);

    fixture.componentInstance.filtro.set('gen');
    expect(fixture.componentInstance.filtrados().map((l) => l.slug)).toEqual(['genesis']);
  });
});
```

- [ ] **Step 5: Ver passar + Commit**

Run: `npm test` → PASS.

```bash
git add src/app/features/admin src/app/core/services/livros-admin.service.ts src/app/core/services/livros-admin.models.ts
git commit -m "feat(web): shell /admin + lista de livros (MatTable, filtro, toggle publicado, apagar)"
```

---

## Task 29: Frontend — form de livro (`/admin/livros/novo` e `/:id`) + capa

**Files:**
- Create: `src/app/features/admin/livro-form/livro-form.component.{ts,html,scss}`
- Modify: `src/app/app.routes.ts` (adicionar as rotas de auth/conta/admin — o Step adiado da Task 25)
- Modify: `src/app/features/biblia/biblia-lista/*` (link "Entrar" no rodapé da sidebar)
- Test: `src/app/features/admin/livro-form/livro-form.component.spec.ts`

**Interfaces:**
- Consumes: `LivrosAdminService`, `ActivatedRoute`, `Router`, Material (`MatFormField`, `MatInput`, `MatSelect`, `MatButton`, `MatIcon`, `MatSnackBar`).
- Produces:
  - `LivroFormComponent` (`bt-admin-livro-form`) — deteta modo por `route.snapshot.paramMap.get('id')`:
    - **novo**: form vazio; `submeter()` → `svc.criar` → navega `/admin/livros/{id}` (`{ replaceUrl: true }`) e mostra secção de capa.
    - **edição**: `svc.obter(id)` popula o form (incl. `FormArray` `ideias`); secção de capa visível (preview de `capaUrl`, `<input type=file>` → `svc.enviarCapa` → atualiza preview; botão remover → `svc.removerCapa`).
  - `secoes` — lista `[{ valor: 'Pentateuco', rotulo: 'PENTATEUCO' }, …]` (constante local com os 10).
  - Validação: `nome` required maxlength 120, `ordem` required 1..66, textareas required, `ideias` `FormArray` com ≥1 e cada linha required, `versiculo` required. Erro 409 → `snackBar` "Slug ou ordem já existem".

- [ ] **Step 1: `LivroFormComponent`**

```ts
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LivrosAdminService } from '../../../core/services/livros-admin.service';

const SECOES = [
  ['Pentateuco', 'PENTATEUCO'], ['LivrosHistoricos', 'LIVROS HISTÓRICOS'],
  ['LivrosPoeticos', 'LIVROS POÉTICOS'], ['ProfetasMaiores', 'PROFETAS MAIORES'],
  ['ProfetasMenores', 'PROFETAS MENORES'], ['Evangelhos', 'EVANGELHOS'], ['Atos', 'ATOS'],
  ['CartasPaulinas', 'CARTAS PAULINAS'], ['CartasGerais', 'CARTAS GERAIS'], ['Apocalipse', 'APOCALIPSE'],
].map(([valor, rotulo]) => ({ valor, rotulo }));

@Component({
  selector: 'bt-admin-livro-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule],
  templateUrl: './livro-form.component.html',
  styleUrl: './livro-form.component.scss',
})
export class LivroFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(LivrosAdminService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  readonly secoes = SECOES;
  readonly id = signal<number | null>(this.numeroDaRota());
  readonly modoEdicao = computed(() => this.id() !== null);
  readonly capaUrl = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.maxLength(120)]],
    secao: ['Pentateuco', Validators.required],
    ordem: [1, [Validators.required, Validators.min(1), Validators.max(66)]],
    quem: ['', Validators.required],
    porque: ['', Validators.required],
    paraQuem: ['', Validators.required],
    estrutura: ['', Validators.required],
    tema: ['', Validators.required],
    versiculo: ['', Validators.required],
    ideias: this.fb.array<string>([this.fb.control('', Validators.required)]),
  });

  get ideias(): FormArray {
    return this.form.get('ideias') as FormArray;
  }

  constructor() {
    const id = this.id();
    if (id !== null) {
      this.svc.obter(id).subscribe((l) => {
        this.ideias.clear();
        l.ideias.forEach((i) => this.ideias.push(this.fb.control(i, Validators.required)));
        this.form.patchValue({ ...l });
        this.capaUrl.set(l.capaUrl);
      });
    }
  }

  private numeroDaRota(): number | null {
    const raw = this.route.snapshot.paramMap.get('id');
    return raw && /^\d+$/.test(raw) ? Number(raw) : null;
  }

  adicionarIdeia(): void {
    this.ideias.push(this.fb.control('', Validators.required));
  }
  removerIdeia(i: number): void {
    this.ideias.removeAt(i);
  }

  submeter(): void {
    if (this.form.invalid) return;
    const valor = this.form.getRawValue();
    const id = this.id();
    const req = id === null ? this.svc.criar(valor) : this.svc.atualizar(id, valor);
    req.subscribe({
      next: (r) => {
        if (id === null) {
          const novoId = (r as { id: number }).id;
          this.id.set(novoId);
          void this.router.navigate(['/admin/livros', novoId], { replaceUrl: true });
        } else {
          this.snack.open('Livro guardado.', undefined, { duration: 2500 });
        }
      },
      error: (e) => this.snack.open(e.status === 409 ? 'Slug ou ordem já existem.' : 'Erro ao guardar.', undefined, { duration: 4000 }),
    });
  }

  aoEscolherFicheiro(ev: Event): void {
    const file = (ev.target as HTMLInputElement).files?.[0];
    const id = this.id();
    if (!file || id === null) return;
    this.svc.enviarCapa(id, file).subscribe({
      next: (r) => this.capaUrl.set(r.capaUrl),
      error: () => this.snack.open('Imagem rejeitada (formato ou tamanho).', undefined, { duration: 4000 }),
    });
  }

  removerCapa(): void {
    const id = this.id();
    if (id === null) return;
    this.svc.removerCapa(id).subscribe(() => this.capaUrl.set(null));
  }
}
```

`livro-form.component.html` — `<form [formGroup]="form" (ngSubmit)="submeter()">` com os campos;
`<mat-select formControlName="secao">` com `@for (s of secoes; ...)`; `formArrayName="ideias"`
com `@for (c of ideias.controls; track $index)` inputs + botão remover + "Adicionar ideia".
Bloco de capa `@if (modoEdicao())`: `@if (capaUrl(); as u) { <img [src]="u" /> <button (click)="removerCapa()">Remover</button> }` + `<input type="file" accept="image/*" (change)="aoEscolherFicheiro($event)" />`.

- [ ] **Step 2: `app.routes.ts` completo + link "Entrar"**

Aplicar o `app.routes.ts` da Task 25 Step 4 (agora todos os componentes existem).
Em `biblia-lista.component.html`, no fundo da `<aside>`, adicionar:

```html
<a class="entrar" routerLink="/entrar">Entrar</a>
```

(e `.entrar { display:block; margin-top:24px; font-family:$fonte-mono; font-size:0.7rem; color:$sage; }` no scss.)

- [ ] **Step 3: Teste — `livro-form.component.spec.ts`**

```ts
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, convertToParamMap } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { LivroFormComponent } from './livro-form.component';

describe('LivroFormComponent', () => {
  function configurar(id: string | null) {
    TestBed.configureTestingModule({
      imports: [LivroFormComponent],
      providers: [
        provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideNoopAnimations(),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } } },
      ],
    });
  }

  it('modo novo: form vazio, sem carregar', () => {
    configurar(null);
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(LivroFormComponent);
    fixture.detectChanges();
    http.expectNone('/api/admin/livros/1');
    expect(fixture.componentInstance.modoEdicao()).toBe(false);
  });

  it('modo edição: carrega e popula ideias', () => {
    configurar('5');
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(LivroFormComponent);
    fixture.detectChanges();
    http.expectOne('/api/admin/livros/5').flush({
      id: 5, slug: 'jo', nome: 'JÓ', ordem: 18, secao: 'LivrosPoeticos', secaoRotulo: 'LIVROS POÉTICOS',
      quem: 'x', porque: 'x', paraQuem: 'x', estrutura: 'x', tema: 'x', ideias: ['A', 'B'],
      versiculo: 'Jó 1:1', publicado: true, capaUrl: null, criadoEm: '', atualizadoEm: '',
    });
    fixture.detectChanges();
    expect(fixture.componentInstance.ideias.length).toBe(2);
    expect(fixture.componentInstance.form.getRawValue().nome).toBe('JÓ');
  });
});
```

- [ ] **Step 4: Ver passar (testes + tsc + build)**

Run: `npm test` → PASS.
Run: `npx tsc --noEmit -p tsconfig.app.json` → sem erros.
Run: `npm run build` → OK (verificar budget `anyComponentStyle` < 8kB por ficheiro).

- [ ] **Step 5: Commit**

```bash
git add src/app
git commit -m "feat(web): form de livro (criar/editar, FormArray de ideias, upload de capa) + rotas completas"
```

---

## Task 30: Docs — `SECURITY.md`, `README.md`, `CLAUDE.md`, `ROADMAP.md`

> **Parte: M2a + M2b (dividida).**
> **M2a:** `SECURITY.md` (auth existe; nota que admin/upload chegam no M2b), `README` (quick start com `.env` + `appsettings.Development.json`, sem a secção `/admin`), `CLAUDE.md` (linhas de auth/cookies/CSRF), `ROADMAP` (M2a entregue).
> **M2b:** `README` (secção `/admin` + upload + `docker compose logs api | grep EMAIL`), `SECURITY.md` (linha do upload), `ROADMAP` (M2b entregue; nota do que foi antecipado do M4).

**Files:**
- Modify: `SECURITY.md`, `README.md`, `CLAUDE.md`, `docs/ROADMAP.md`

**Interfaces:**
- Consumes: nada.
- Produces: documentação alinhada com o M2 entregue.

- [ ] **Step 1: `SECURITY.md`**

Na secção "Boas práticas já aplicadas", substituir as linhas sobre auth por:

```markdown
- Autenticação por JWT em cookies HttpOnly + Secure + SameSite=Strict; access curto (15 min)
  e refresh token rotativo (14 dias) com hash SHA-256 na BD e deteção de reutilização
- 2FA obrigatório por código de email em cada login (token do ASP.NET Core Identity)
- Lockout após 5 tentativas falhadas (15 min); rate limit de 10 req/min/IP em /api/auth/*
- CSRF: double-submit (cookie bt_csrf não-HttpOnly + header X-CSRF-Token) em toda a escrita
- Autorização: [Authorize(Roles="Admin")] em todo o /api/admin/**; /api/conta opera sempre
  sobre o utilizador do token, nunca sobre um id do cliente
- Upload de imagens: content-type + magic bytes + limite de tamanho + re-codificação WebP
  (strip de metadados) + nome {guid}.webp + armazenamento fora do webroot + anti path-traversal
- Segredos (Jwt:SigningKey, AdminSeed, SMTP) só em env / appsettings.Development.json / .env
```

E remover a nota "quando o M2 introduzir a área /admin" da secção "Âmbito" (a área existe agora).

- [ ] **Step 2: `README.md`**

- No badge/estado, marcar **M2 ✅ entregue**.
- Adicionar à tabela "Estado" a linha do M2 como concluída.
- Nova subsecção em "Quick start (Docker)":

```markdown
### Autenticação (M2)

O `docker compose up` precisa de um `.env` na raiz (copiar de `.env.example`):

- `BT_JWT_KEY` — chave de assinatura do JWT (ex.: `openssl rand -base64 48`)
- `BT_ADMIN_EMAIL` / `BT_ADMIN_SENHA` — o admin criado no arranque (troca de senha forçada no 1º login)

Login em `http://localhost:4300/entrar`. Em ambiente Dev o código 2FA aparece nos logs:
`docker compose logs api | grep EMAIL`.

`/admin` — CRUD de livros + capa (role Admin). `/conta` — perfil (qualquer utilizador).
`/registar` — criar conta de Membro.
```

- Em "Desenvolvimento local", documentar o `appsettings.Development.json` de exemplo (bloco da Task 18 Step 1).
- Em "Estrutura do repositório", acrescentar `src/BibliaTeologica.Infrastructure/{Identity,Email,Imagens}/` e `src/BibliaTeologica.Api/Auth/`.

- [ ] **Step 3: `CLAUDE.md`**

- Em "Regras de ouro — nunca fazer", as regras sobre `[Authorize]`, upload e `_mapper.Map`
  já existem — confirmar que continuam corretas (continuam).
- Adicionar em "Convenções de código":

```markdown
- Auth: cookies HttpOnly (bt_access/bt_refresh) + double-submit CSRF (bt_csrf + header X-CSRF-Token). Nunca token em localStorage. Nunca ApplicationUser/hash/refresh no corpo de uma resposta.
- Identity vive na Infrastructure; a Application fala com ele só por IIdentityService.
```

- Em "Comandos essenciais", adicionar a nota do `.env` para `docker compose up`.

- [ ] **Step 4: `docs/ROADMAP.md`**

- Marcar **M2** como entregue, com a data.
- Adicionar a nota: "M2 antecipou do M4 o conceito de Membro (role + perfil) e a infra de
  email transacional. O M4 acrescenta: confirmação de email no registo, recuperação de
  password, captura/envio de newsletter."

- [ ] **Step 5: Commit**

```bash
git add SECURITY.md README.md CLAUDE.md docs/ROADMAP.md
git commit -m "docs: M2 — auth/autorização no SECURITY, quick start com .env, roadmap"
```

- [ ] **Step 6: Verificação final da solução**

```bash
dotnet test BibliaTeologica.sln
cd frontend/biblia-teologica-web && npm test && npx tsc --noEmit -p tsconfig.app.json && npm run build
```
Expected: tudo verde.

---

## Riscos conhecidos e pontos de atenção para o executor

1. **`AppDbContext` → `IdentityDbContext` (Task 4).** Migração grande. Se o `dotnet ef migrations add` falhar por o design-time não conseguir construir o contexto, confirmar que existe um `IDesignTimeDbContextFactory<AppDbContext>` (o M1 pode ter um) e que ele fornece uma connection string dummy. A `base.OnModelCreating` tem de vir **antes** do `ApplyConfigurationsFromAssembly`.

2. **`Livro.Id` é `int`, Identity é `Guid` (constraint global).** Não uniformizar. Os endpoints de admin usam `{id:int}`; `ApplicationUser`/roles/`RefreshToken` usam `Guid`.

3. **2FA email token provider (Tasks 8, 18).** O `EmailTokenProvider` do Identity precisa de `.AddDefaultTokenProviders()` **e** que o utilizador tenha `SecurityStamp` (o `UserManager.CreateAsync` põe). Nos testes de Infra, registar Identity com `AddIdentity`/`AddIdentityCore` + `.AddDefaultTokenProviders()`. O código OTP tem validade curta (~3 min, janela do Rfc6238) — testes que geram e verificam no mesmo tick funcionam.

4. **`WebApplicationFactory` + `Jwt:SigningKey` (Task 18).** Os testes de Api do M1 vão partir quando o `AddInfrastructure` passar a exigir `Jwt:SigningKey`. Extrair `WebFactoryBase` com config in-memory comum antes de tocar nos testes do M1.

5. **CSRF nos testes de integração (Tasks 19, 20).** `WebApplicationFactory` com `HandleCookies = true` guarda os cookies; é preciso um `DelegatingHandler` que copie o valor do cookie `bt_csrf` para o header `X-CSRF-Token` nos métodos não-seguros. Iterar até os testes `..._Retorna403_SemHeaderCsrf` e os de caminho feliz passarem.

6. **Moq + `ReadOnlySpan<byte>`.** O Moq não suporta `It.IsAny<ReadOnlySpan<byte>>()` (é `ref struct`). Por isso `IProcessadorImagem.AssinaturaValida` recebe `byte[]` — já assim na Task 2 (contrato), Task 10 (impl + testes com `png[..12]`) e Task 16 (mocks com `It.IsAny<byte[]>()`). Se alguma task for editada, manter os 3 alinhados.

7. **`IEmissorTokens` (Task 14).** Serviço partilhado entre `AuthService` (`Verificar2faAsync`) e `ContaService` (`TrocarSenhaAsync`). O `AuthService.RefreshAsync` **não** o usa — o refresh já foi rotacionado, e o `EmissorTokens` guardaria um 2º refresh; monta o resultado localmente. `ContaService` só é implementado na Task 15, mas `IEmissorTokens` é criado na Task 14.

8. **`ng add @angular/material` pode mexer em ficheiros do M1 (Task 23).** Rever o diff de `styles.scss` e `app.config.ts` com cuidado — manter o pergaminho do fichário intacto; o tema Material só deve aplicar dentro de `.mat-*`. Isolar num `styles/material.scss` se necessário.

9. **`app.routes.ts` aponta para componentes inexistentes (Tasks 25/29).** O `app.routes.ts` completo só entra na Task 29 Step 2, quando todos os componentes lazy existem. Até lá, a Task 25 só adiciona guards + `app.config.ts`.

10. **`refreshInterceptor` e `next()` (Task 24).** Chamar `next(req.clone({ url: '/api/auth/refresh', method: 'POST' }))` pode não funcionar em todas as versões — a alternativa é injetar `HttpClient` no interceptor e fazer `http.post('/api/auth/refresh', {})`. Iterar até `interceptors.spec.ts` passar (o teste é a fonte de verdade do comportamento).

11. **Budget `anyComponentStyle` (4kB warn / 8kB erro).** Os `.scss` dos componentes Material podem crescer. Manter mínimos; usar utilitários globais em `styles.scss` se preciso.

12. **`SmtpClient` obsoleto.** `System.Net.Mail.SmtpClient` está marcado `[Obsolete]` para cenários novos mas funciona e é suficiente para o M2 (sink de dev é o default). Suprimir o warning localmente com `#pragma warning disable SYSLIB0014` à volta do `using`/`SendMailAsync` **apenas** nesse ficheiro, ou aceitar o warning se `TreatWarningsAsErrors` não estiver ligado no projeto Infra (confirmar — o M1 tinha `TreatWarningsAsErrors=true` no `Directory.Build.props`; então **é preciso** o `#pragma`).

13. **`provideAnimationsAsync` import path.** Em Angular 22 é `@angular/platform-browser/animations/async`. Se o `ng add` puser outro, seguir o que o `ng add` gerou.

## Cobertura do spec (auto-revisão)

| Secção do spec | Task(s) |
|---|---|
| §3 Escopo (dentro) | todas |
| §4 Estrutura da solução | mapa de ficheiros + todas |
| §5 Domain (`Livro` comportamento, `ConflitoException`) | 1 |
| §6 Application (interfaces, DTOs, `AuthService`, `ContaService`, `LivroCommandService`) | 2, 14, 15, 16 |
| §7 Infrastructure (`AppDbContext`, `RefreshToken`, `IdentityService`, `TokenService`, `RefreshTokenStore`, email, imagens, seeder, `AddInfrastructure`) | 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 |
| §8 API (`AuthController`, `ContaController`, `LivrosAdminController`, `MediaController`, `Program.cs`, middleware, config) | 17, 18, 19, 20, 21 |
| §9 Frontend (Material, `AuthStore`/`AuthService`, interceptors, guards, `/entrar`, `/registar`, `/conta`, `/admin`, form) | 23, 24, 25, 26, 27, 28, 29 |
| §10 Docker Compose (`biblia-media`, `.env`, `chown`) | 22 |
| §11 CI + `SECURITY.md` | 30 (CI não muda estruturalmente) |
| §12 Segurança — 12 pontos | 1 (validação), 17 (CSRF), 18 (rate limit, cookies, seed), 12 (JwtBearer via cookie), 16 (upload), 20 (Authorize Admin), 21 (media anti-traversal) |
| §13 Sequência de implementação | ordem das tasks por parte (ver "Estrutura em duas partes") |
| §3.1 Entrega em dois PRs | "Estrutura em duas partes" + notas de parte em cada task straddler |
| §3.2 Checklist das 10 regras de ouro | Task 12 (§12) — M2a cobre 1–5,7–10; M2b cobre 1,2,6,7,10 |

---

**Fim do plano.**
