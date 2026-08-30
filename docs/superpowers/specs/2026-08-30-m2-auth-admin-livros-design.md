# M2 — Autenticação + Admin de Livros (design)

**Data:** 2026-08-30
**Estado:** revista — pronta para o plano de implementação
**Marco:** M2 de 5 (ver `docs/ROADMAP.md`)
**Depende de:** M1 (site público de Livros) — já em `main`.

---

## 1. Contexto

O M1 entregou o fichário público dos 66 livros: 4 camadas .NET 8, EF Core + SQLite,
API pública **read-only** (`GET /api/livros`, `GET /api/livros/{slug}`), SPA Angular 22,
Docker, CI. O `Livro` foi criado deliberadamente **sem métodos de mutação**.

O M2 acrescenta a primeira camada de escrita e de identidade:

- **Autenticação** — ASP.NET Core Identity + JWT em cookies HttpOnly, com refresh
  rotativo e 2FA por email (OTP).
- **Autorização** — roles `Admin` e `Membro`.
- **Área `/admin`** (só `Admin`) — CRUD completo de Livros + upload de imagem de capa.
- **Área `/conta`** (qualquer autenticado) — perfil (nome, email, password).
- **Registo mínimo de Membro** — email/password, sem confirmação de email (isso é M4).

## 2. Visão de produto (roadmap)

| Marco | Entrega | Novo no projeto |
|---|---|---|
| M1 | Site público de Livros | Solução em camadas, EF Core/SQLite, Angular, Docker, CI |
| **M2** | Autenticação + Admin de Livros (esta spec) | Identity, JWT/cookies, refresh rotativo, 2FA email, roles, upload, Angular Material, email transacional |
| M3 | Estudos — Markdown long-form (público + admin) | Markdown, sanitização, editor |
| M3.5 | Vídeos — embed + metadados | Entidade simples |
| M4 | Cadastro público + confirmação de email + newsletter | Confirmação de email, fluxo de registo completo |
| M5+ | Doutrinas, Glossário, Cronologia, Teologia Sistemática, email marketing, deploy real | Repetição de padrão |

**Nota de roadmap:** o M2 antecipa duas coisas que estavam listadas para o M4 —
o conceito de utilizador comum (role `Membro` + área de perfil) e a infra de email
transacional (necessária para o OTP). O M4 acrescenta por cima: confirmação de email
no registo, e captura/envio de newsletter.

## 3. Escopo do M2

### Dentro

- ASP.NET Core Identity (`ApplicationUser : IdentityUser<Guid>`), roles `Admin`/`Membro`.
- Login em 2 passos (senha → OTP por email), cookies `bt_access` / `bt_refresh` / `bt_csrf`.
- Refresh token rotativo com hash SHA-256 na BD e deteção de reutilização.
- 2FA por email via token provider embutido do Identity.
- Lockout após N tentativas, rate limit apertado em `/api/auth/*`, troca de senha forçada
  no 1º login do admin seed.
- `IEmailSender` (Application) + `SmtpEmailSender` / `LogEmailSender` (sink de dev).
- `Livro` ganha comportamento: `Atualizar`, `Publicar`/`Despublicar`, `DefinirCapa`/`RemoverCapa`,
  campo `CapaImagemPath`.
- `ILivroCommandService` — CRUD completo de Livros (criar, editar, apagar, publicar).
- Upload de capa: 1 imagem por livro, validação content-type + magic bytes + tamanho +
  re-encode WebP + strip metadata, armazenamento fora do webroot, servido por
  `GET /api/media/livros/{arquivo}`.
- Registo mínimo de Membro (`POST /api/auth/registar`), sem confirmação de email.
- Área `/conta` — ver/editar nome, trocar email (re-autenticação por senha, sem
  re-verificação), trocar password.
- Angular Material só em `/admin` e nas páginas de auth; `AuthStore` (signals),
  interceptors (withCredentials, CSRF, refresh), guards (`authGuard`, `adminGuard`).
- Migração única `M2_IdentityELivroCapa`; `IdentitySeeder` no arranque.
- `docker-compose` ganha volume `biblia-media`.

### Fora (registado no ROADMAP)

- Confirmação de email no registo, newsletter — M4.
- Recuperação de password ("esqueci-me") — M4 (ou item próprio; **não** no M2).
- 2FA por TOTP (app autenticadora) — não previsto.
- Gestão de utilizadores no `/admin` (convidar/desativar contas) — não no M2.
- Autoria de Livros por utilizador (nenhuma regra de domínio liga `Livro` a um `User`).
- Servir imagens estáticas direto pelo nginx — otimização futura.
- Auditoria/log de ações do admin — futuro.

## 4. Estrutura da solução (deltas ao M1)

```
src/BibliaTeologica.Domain/
  Entities/Livro.cs                         (+ Atualizar, Publicar, Despublicar,
                                               DefinirCapa, RemoverCapa, CapaImagemPath)
  Exceptions/ConflitoException.cs           (novo — slug/ordem duplicados -> 409)

src/BibliaTeologica.Application/
  Common/Interfaces/
    ILivroRepository.cs                      (+ ObterPorIdAsync, ListarTodosAsync,
                                               AdicionarAsync, RemoverAsync,
                                               SlugExisteAsync, OrdemExisteAsync, SalvarAsync)
    IIdentityService.cs                      (novo)
    ITokenService.cs                         (novo)
    IRefreshTokenStore.cs                    (novo)
    IEmailSender.cs                          (novo)
    IProcessadorImagem.cs                    (novo)
    IArmazenamentoImagens.cs                 (novo)
    IUsuarioAtual.cs                         (novo — abstrai HttpContext.User)
  Auth/
    IAuthService.cs / AuthService.cs         (login, 2fa, refresh, logout, registar)
    Dtos/  (LoginDto, Verificar2faDto, RegistarMembroDto, UsuarioDto,
            ResultadoLoginDto, TokensDto)
  Conta/
    IContaService.cs / ContaService.cs
    Dtos/  (PerfilDto, AtualizarPerfilDto, TrocarEmailDto, TrocarSenhaDto)
  Livros/
    ILivroCommandService.cs / LivroCommandService.cs
    Dtos/  (CriarLivroDto, AtualizarLivroDto, LivroAdminListItemDto, LivroAdminDetailDto)
  DependencyInjection.cs                     (+ AddScoped dos serviços novos)

src/BibliaTeologica.Infrastructure/
  Persistence/
    AppDbContext.cs                          (passa a : IdentityDbContext<ApplicationUser,
                                               IdentityRole<Guid>, Guid>)
    Configurations/RefreshTokenConfiguration.cs
    Configurations/LivroConfiguration.cs     (+ CapaImagemPath)
    Migrations/…M2_IdentityELivroCapa
    Seed/IdentitySeeder.cs                   (novo)
  Identity/
    ApplicationUser.cs
    RefreshToken.cs
    IdentityService.cs
    TokenService.cs
    RefreshTokenStore.cs
  Email/
    SmtpEmailSender.cs
    LogEmailSender.cs
  Imagens/
    ProcessadorImagemSharp.cs
    ArmazenamentoImagensLocal.cs
  DependencyInjection.cs                     (+ AddIdentityCore, AddAuthentication/JwtBearer,
                                               registo condicional do email sink)

src/BibliaTeologica.Api/
  Controllers/
    AuthController.cs                        (novo)
    ContaController.cs                       (novo)
    LivrosAdminController.cs                 (novo)
    MediaController.cs                       (novo)
    LivrosController.cs                      (M1 — DTOs ganham capaUrl)
  Auth/
    CookieWriter.cs                          (helper — grava/limpa os 3 cookies)
    CsrfActionFilter.cs                      (double-submit)
    UsuarioAtual.cs                          (IUsuarioAtual via IHttpContextAccessor)
  Program.cs                                 (+ auth, + filtro CSRF, + rate limit /auth)

frontend/biblia-teologica-web/
  src/app/core/
    auth/auth.store.ts, auth.service.ts, auth.models.ts
    auth/guards/auth.guard.ts, admin.guard.ts
    interceptors/{with-credentials,csrf,refresh}.interceptor.ts
    livros-admin.service.ts
  src/app/features/auth/           (entrar, registar — lazy)
  src/app/features/conta/          (perfil, trocar-senha — lazy)
  src/app/features/admin/          (shell Material, livros lista/form — lazy, adminGuard)
  angular.json / package.json      (+ @angular/material)
```

### Regra de camadas (mantida)

- `ApplicationUser` e `RefreshToken` são **modelos de persistência/framework** — vivem na
  Infrastructure, nunca no Domain. Nenhuma regra de domínio do M2 envolve um utilizador.
- A Application fala com o Identity **só** por interfaces (`IIdentityService`) cujos
  inputs/outputs são primitivos/DTOs — `ApplicationUser` não vaza para cima.
- `AuthController` grava/limpa cookies (concern HTTP) e mapeia resultados para status;
  toda a decisão está no `AuthService`.

## 5. Domain

### `Livro` — comportamento novo

```
Atualizar(nome, secao, ordem, quem, porque, paraQuem, estrutura, tema, ideias, versiculo)
  - revalida os MESMOS invariantes de Create (campos obrigatórios, ordem 1..66,
    >=1 ideia não-vazia, secao definida)
  - regenera Slug a partir de nome
  - AtualizadoEm = DateTime.UtcNow

Publicar()      -> Publicado = true;  AtualizadoEm = UtcNow
Despublicar()   -> Publicado = false; AtualizadoEm = UtcNow
DefinirCapa(string caminho)  -> CapaImagemPath = caminho; AtualizadoEm = UtcNow
RemoverCapa()                -> CapaImagemPath = null;    AtualizadoEm = UtcNow
```

- Campo novo: `string? CapaImagemPath` (get privado) — **chave de armazenamento relativa**
  (`{guid}.webp`), nunca URL.
- `Livro.Id` continua **`int`** (identity, do M1). Os endpoints de admin usam `{id:int}`.
  Só o Identity (`ApplicationUser`, roles, `RefreshToken`) usa `Guid`.
- Invariante `Ordem` 1..66 **mantém-se**. "Criar livro" no /admin serve para repor um
  livro apagado ou preencher uma lacuna, não para estender o cânon.
- `Livro.Create` (M1) inalterado; capa é sempre passo separado (nunca no `Create`).
- Slug segue o nome: renomear um livro muda o slug e o URL público antigo passa a 404.
  Aceitável — é ação de admin sobre um cânon de nomes estáveis.

### `ConflitoException : Exception` — `Domain/Exceptions/`

Lançada por `LivroCommandService` quando o slug ou a ordem já existem noutro livro.
Mapeada para `409` no middleware.

### Testes de Domain (`BibliaTeologica.Domain.Tests`)

- `Atualizar_RevalidaInvariantes_QuandoDadosInvalidos`
- `Atualizar_RegeneraSlug_QuandoNomeMuda`
- `Publicar_Despublicar_AlternamFlagETocamAtualizadoEm`
- `DefinirCapa_RemoverCapa_GerenciamCampo`

## 6. Application

### DTOs (contrato da API, nunca a entidade)

```
// Auth
LoginDto            { Email, Senha }
Verificar2faDto     { TokenDesafio, Codigo }
RegistarMembroDto   { Email, Senha, NomeExibicao }
UsuarioDto          { Email, NomeExibicao, Roles[], DeveTrocarSenha }
ResultadoLoginDto   { Desafio2fa: bool, TokenDesafio?: string, Usuario?: UsuarioDto,
                      Tokens?: TokensDto }
TokensDto           { AccessToken, AccessExpiraEm, RefreshToken, RefreshExpiraEm, Csrf }
                    // interno — nunca serializado; o controller transforma em cookies

// Conta
PerfilDto           { Email, NomeExibicao, Roles[], CriadoEm }
AtualizarPerfilDto  { NomeExibicao }
TrocarEmailDto      { NovoEmail, Senha }
TrocarSenhaDto      { SenhaAtual, NovaSenha }

// Livros (escrita) — Id é int
CriarLivroDto       { Nome, Secao, Ordem, Quem, Porque, ParaQuem, Estrutura, Tema,
                      Ideias[], Versiculo }
AtualizarLivroDto   { …iguais… }
LivroAdminListItemDto  { Id: int, Slug, Nome, Ordem, Secao, Publicado, TemCapa, AtualizadoEm }
LivroAdminDetailDto    { Id: int, Slug, Nome, Ordem, Secao, SecaoRotulo, Quem, Porque,
                         ParaQuem, Estrutura, Tema, Ideias[], Versiculo, Publicado,
                         CapaUrl?, CriadoEm, AtualizadoEm }

// Tipos auxiliares de resultado (records pequenos, Application)
ResultadoSenha     { Sucesso: bool, Bloqueado: bool, UserId: Guid? }
IdentityResultDto  { Sucesso: bool, Erros: string[] }
RefreshTokenInfo   { UserId: Guid, Hash, ExpiraEm, RevogadoEm: DateTime? }
```

Todos os DTOs de escrita são mapeados **campo a campo** para `Livro.Create`/`Atualizar` —
**nunca** `_mapper.Map(dto, entity)` (regra de ouro; slug e auditoria protegidos).

### `IAuthService` / `AuthService`

```
Task<ResultadoLoginDto> LoginAsync(LoginDto, string? ip, CancellationToken)
Task<ResultadoLoginDto> Verificar2faAsync(Verificar2faDto, string? ip, CancellationToken)
Task<ResultadoLoginDto> RefreshAsync(string refreshTokenCru, string? ip, CancellationToken)
Task LogoutAsync(string refreshTokenCru, CancellationToken)
Task RegistarMembroAsync(RegistarMembroDto, CancellationToken)
```

- `LoginAsync`: `IIdentityService.VerificarSenhaAsync(email, senha, lockoutOnFailure: true)`.
  Bloqueado → resultado marcado (controller → 423). Inválido → resultado vazio (→ 401).
  Ok → `IIdentityService.GerarTokenEmailAsync(userId)` + `IEmailSender.EnviarAsync(...)`,
  devolve `Desafio2fa = true` + `TokenDesafio` (data-protection, purpose `login-2fa`, ~5 min).
- `Verificar2faAsync`: valida `TokenDesafio` (assinatura, purpose, expiração), resolve user,
  `IIdentityService.VerificarTokenEmailAsync(userId, codigo)`. Falha → `RegistarFalhaAsync`
  (conta para lockout) → 401. Ok → `ResetarFalhasAsync` + emite tokens (via `ITokenService`
  + `IRefreshTokenStore.GuardarAsync`).
- `RefreshAsync`: `IRefreshTokenStore.ObterValidoAsync(hash)`. Revogado (reutilização) →
  `RevogarCadeiaAsync` → 401. Válido → rotaciona + emite novos tokens.
- `RegistarMembroAsync`: `IIdentityService.CriarMembroAsync(email, senha, nome)` — role
  `Membro`, `EmailConfirmed = true`, `DeveTrocarSenha = false`. Sem auto-login.

### `IContaService` / `ContaService`

```
Task<PerfilDto> ObterAsync(Guid userId, CancellationToken)
Task AtualizarAsync(Guid userId, AtualizarPerfilDto, CancellationToken)
Task TrocarEmailAsync(Guid userId, TrocarEmailDto, CancellationToken)   // exige senha
Task<ResultadoLoginDto> TrocarSenhaAsync(Guid userId, TrocarSenhaDto, CancellationToken)
      // limpa DeveTrocarSenha, reemite tokens
```

### `ILivroCommandService` / `LivroCommandService`

```
Task<int> CriarAsync(CriarLivroDto, CancellationToken)
Task AtualizarAsync(int id, AtualizarLivroDto, CancellationToken)
Task ApagarAsync(int id, CancellationToken)                  // apaga ficheiro de capa
Task DefinirPublicacaoAsync(int id, bool publicado, CancellationToken)
Task<string> DefinirCapaAsync(int id, Stream imagem, string contentType, CancellationToken)
Task RemoverCapaAsync(int id, CancellationToken)
```

- `CriarAsync` / `AtualizarAsync`: `SlugExisteAsync` / `OrdemExisteAsync` (excluindo o
  próprio id no update) → `ConflitoException` se colidir. Constrói via `Livro.Create`/`Atualizar`.
- `DefinirCapaAsync`: valida magic bytes → `IProcessadorImagem.NormalizarAsync` (re-encode
  WebP, resize 1200px, strip EXIF) → `IArmazenamentoImagens.GuardarAsync` → `livro.DefinirCapa`
  + `SalvarAsync` → `IArmazenamentoImagens.RemoverAsync(antiga)` (best-effort, logado se falhar).

### `IIdentityService` (interface — sem fugir `ApplicationUser`)

```
Task<ResultadoSenha> VerificarSenhaAsync(string email, string senha, bool lockoutOnFailure)
Task<Guid?> ResolverIdPorEmailAsync(string email)
Task<string> GerarTokenEmailAsync(Guid userId)
Task<bool> VerificarTokenEmailAsync(Guid userId, string codigo)
Task RegistarFalhaAsync(Guid userId)  /  Task ResetarFalhasAsync(Guid userId)
Task<IReadOnlyList<string>> RolesAsync(Guid userId)
Task<UsuarioDto> ObterUsuarioAsync(Guid userId)
Task<IdentityResultDto> CriarMembroAsync(string email, string senha, string nome)
Task<IdentityResultDto> TrocarSenhaAsync(Guid userId, string atual, string nova)
Task<IdentityResultDto> TrocarEmailAsync(Guid userId, string novo, string senha)
Task AtualizarNomeAsync(Guid userId, string nome)
```

### `ITokenService`, `IRefreshTokenStore`, `IEmailSender`, `IProcessadorImagem`, `IArmazenamentoImagens`, `IUsuarioAtual`

```
ITokenService:
  string GerarAccessToken(Guid userId, string email, string nome, IEnumerable<string> roles,
                          bool deveTrocarSenha)
  (string tokenCru, string hash, DateTime expiraEm) GerarRefreshToken()
  string Hash(string tokenCru)

IRefreshTokenStore:
  Task GuardarAsync(Guid userId, string hash, DateTime expiraEm, string? ip, CancellationToken)
  Task<RefreshTokenInfo?> ObterValidoAsync(string hash, CancellationToken)
  Task<RefreshTokenInfo?> ObterQualquerAsync(string hash, CancellationToken)  // detetar reuse
  Task RotacionarAsync(string hashAntigo, string hashNovo, DateTime expira, string? ip, CT)
  Task RevogarAsync(string hash, CancellationToken)
  Task RevogarCadeiaAsync(Guid userId, CancellationToken)

IEmailSender:
  Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken)

IProcessadorImagem:
  Task<Stream> NormalizarAsync(Stream entrada, int dimensaoMaxPx, CancellationToken)
  bool AssinaturaValida(ReadOnlySpan<byte> cabecalho, string contentType)

IArmazenamentoImagens:
  Task<string> GuardarAsync(Stream webp, CancellationToken)     // devolve "{guid}.webp"
  Task RemoverAsync(string chave, CancellationToken)
  Task<Stream?> AbrirAsync(string chave, CancellationToken)

IUsuarioAtual:
  Guid? Id;  string? Email;  bool EstaAutenticado;  bool EhAdmin
```

### `DependencyInjection.AddApplication()`

`+ AddScoped<IAuthService, AuthService>`, `IContaService`, `ILivroCommandService`.

### Testes (`BibliaTeologica.Application.Tests` — xUnit + Moq + AwesomeAssertions)

- `AuthServiceTests` — login→desafio; 2fa ok→tokens; 2fa errado→falha + `RegistarFalhaAsync`;
  bloqueado→resultado bloqueado; refresh rotaciona; reutilização→`RevogarCadeiaAsync`;
  registar membro chama `CriarMembroAsync`.
- `LivroCommandServiceTests` — criar/atualizar/apagar; `409` slug/ordem duplicados;
  `DefinirCapaAsync` chama processador + armazenamento + `DefinirCapa`; apagar remove ficheiro.
- `ContaServiceTests` — trocar senha limpa `DeveTrocarSenha` e devolve tokens novos.

## 7. Infrastructure

### `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`

- Um só ficheiro SQLite, um só `__EFMigrationsHistory`. `OnModelCreating` chama
  `base.OnModelCreating(...)` **antes** de `ApplyConfigurationsFromAssembly(...)`.
- `DbSet<Livro> Livros` (M1) + `DbSet<RefreshToken> RefreshTokens`.

### `ApplicationUser : IdentityUser<Guid>`

`+ string NomeExibicao`, `bool DeveTrocarSenha`, `DateTime CriadoEm`.

### `RefreshToken` (modelo de persistência)

```
Id (Guid, PK)
UsuarioId (Guid, FK -> AspNetUsers, cascade)
TokenHash (string, único, index)      // SHA-256 hex do token cru
ExpiraEm (DateTime)
CriadoEm (DateTime)
RevogadoEm (DateTime?)
SubstituidoPorHash (string?)
CriadoPorIp (string?)
```

`RefreshTokenConfiguration` — `HasIndex(TokenHash).IsUnique()`, `MaxLength(64)`.

### `IdentityService`, `TokenService`, `RefreshTokenStore`

- `IdentityService` usa `UserManager<ApplicationUser>` / `SignInManager<ApplicationUser>`.
  `VerificarSenhaAsync` → `CheckPasswordSignInAsync(user, senha, lockoutOnFailure)`.
  Tokens de email via `GenerateTwoFactorTokenAsync(user, "Email")` / `VerifyTwoFactorTokenAsync`.
- `TokenService` — `JwtSecurityTokenHandler`, HS256, chave de `Jwt:SigningKey` (env).
  Refresh = `RandomNumberGenerator.GetBytes(32)` em base64url; hash = SHA-256 hex.
- `RefreshTokenStore` — EF; `AsNoTracking` nas leituras de verificação, tracking na rotação.

### `SmtpEmailSender` / `LogEmailSender`

- `LogEmailSender` (default em `Development`) — `ILogger.LogInformation` com assunto + corpo
  (o OTP aparece nos logs). `SmtpEmailSender` — `System.Net.Mail.SmtpClient` com
  host/porta/credenciais de config. Escolha por `Email:Sink` (`Dev` | `Smtp`).

### `ProcessadorImagemSharp` / `ArmazenamentoImagensLocal`

- `ProcessadorImagemSharp` — `SixLabors.ImageSharp`: valida assinatura, `Image.Load`,
  `Mutate(x => x.Resize(...))` mantendo aspeto até `dimensaoMaxPx`, `SaveAsWebpAsync`,
  metadados descartados por padrão na re-codificação.
- `ArmazenamentoImagensLocal` — `Armazenamento:CaminhoImagens`; escreve `{guid}.webp`;
  `AbrirAsync` valida a chave contra `^[0-9a-f-]{36}\.webp$` e confirma que o caminho
  resolvido continua sob o baseDir (anti path-traversal).

### `IdentitySeeder` — `Persistence/Seed/`

`SeedAsync`: usa `RoleManager`/`UserManager` **diretamente** (é infra — não passa pelo
`IIdentityService`). Cria roles `Admin`/`Membro` se não existirem; para cada entrada de
`AdminSeed:*`, se o email não existir, cria o utilizador na role `Admin` com
`DeveTrocarSenha = true` e `EmailConfirmed = true`. Idempotente. Corre depois do `LivroSeeder`.

### `DependencyInjection.AddInfrastructure(config)`

`+ AddIdentityCore<ApplicationUser>()` com `.AddRoles<IdentityRole<Guid>>()`,
`.AddEntityFrameworkStores<AppDbContext>()`, `.AddDefaultTokenProviders()`;
opções de password e lockout de `Identity:*`; `AddAuthentication(JwtBearerDefaults)`
+ `AddJwtBearer` com `OnMessageReceived` a ler o cookie `bt_access`; registo condicional
do email sink; `AddDataProtection()`.

### Migração

`dotnet ef migrations add M2_IdentityELivroCapa` — `Migrations/` commitado.
O `Program.cs` do M1 já faz `await db.Database.MigrateAsync()` no arranque — a migração
nova aplica-se automaticamente.

### Testes (`BibliaTeologica.Infrastructure.Tests` — SQLite real)

- `RefreshTokenStoreTests` — guardar/obter válido/rotacionar/revogar cadeia; token expirado
  não é válido; reutilização detetada por `ObterQualquerAsync`.
- `IdentitySeederTests` — cria roles + admin; idempotente numa 2ª chamada.
- `ProcessadorImagemSharpTests` — `AssinaturaValida` rejeita mismatch; `NormalizarAsync`
  produz WebP; resize respeita a dimensão máx.
- `ArmazenamentoImagensLocalTests` — guarda/abre/apaga; `AbrirAsync` recusa `../` e chaves
  fora do padrão.

## 8. API

### `AuthController` — `api/auth`, `[AllowAnonymous]`, rate limit `auth` (10/min/IP)

```
POST /login       { email, senha }          -> 200 { desafio2fa, tokenDesafio } | 401 | 423
POST /login/2fa   { tokenDesafio, codigo }   -> 200 { usuario }  + Set-Cookie x3 | 401
POST /refresh     (cookie bt_refresh)        -> 200 { usuario }  + Set-Cookie x3 | 401   [CSRF]
POST /logout                                  -> 204  + limpa cookies             | 401   [CSRF]
POST /registar    { email, senha, nomeExibicao } -> 201 | 400 | 409                       [rate limit]
GET  /eu          (cookie bt_access)         -> 200 { usuario } | 401
```

- O `CookieWriter` grava `bt_access` (HttpOnly, `/`, 15 min), `bt_refresh` (HttpOnly,
  `/api/auth`, 14 d), `bt_csrf` (não-HttpOnly, `/`, sessão). Todos `Secure` + `SameSite=Strict`.
- `GET /eu` serve o SPA para hidratar o estado no arranque.

### `ContaController` — `api/conta`, `[Authorize]`, `[CsrfActionFilter]` nas escritas

```
GET  /                        -> 200 PerfilDto
PUT  /        { nomeExibicao } -> 204
PUT  /email   { novoEmail, senha } -> 204 | 400 | 409
POST /senha   { senhaAtual, novaSenha } -> 200 { usuario } + Set-Cookie x3 | 400
```

### `LivrosAdminController` — `api/admin/livros`, `[Authorize(Roles="Admin")]`, `[CsrfActionFilter]`

```
GET    /                 -> 200 LivroAdminListItemDto[]   (todos, incl. despublicados)
GET    /{id}             -> 200 LivroAdminDetailDto | 404
POST   /                 -> 201 { id } | 400 | 409
PUT    /{id}             -> 204 | 400 | 404 | 409
DELETE /{id}             -> 204 | 404
PUT    /{id}/publicado   { publicado } -> 204 | 404
PUT    /{id}/capa        (multipart file) -> 200 { capaUrl } | 400 | 404 | 413
DELETE /{id}/capa        -> 204 | 404
```

`PUT /{id}/capa` — `[RequestSizeLimit(2_500_000)]`, `[RequestFormLimits(MultipartBodyLengthLimit=2_500_000)]`;
o controller faz checks baratos e passa `file.OpenReadStream()` ao serviço.

### `MediaController` — `api/media`

```
GET /livros/{arquivo}  -> 200 image/webp (Cache-Control: public, max-age=604800, immutable)
                          404
```

`{arquivo}` validado contra `^[0-9a-f-]{36}\.webp$`; delega em `IArmazenamentoImagens.AbrirAsync`.
Anónimo — as capas são conteúdo público.

### `LivrosController` (M1) — alteração

`LivroDetailDto` e `LivroListItemDto` ganham `string? CapaUrl` (montado como
`/api/media/livros/{CapaImagemPath}` quando não-nulo). Comportamento inalterado.

### `Program.cs` (deltas ao M1)

- `AddAuthentication` + `AddAuthorization`; `UseAuthentication()` / `UseAuthorization()`
  no pipeline (depois de `UseCors`, antes de `MapControllers`).
- `CsrfActionFilter` registado; aplicado por atributo nos controllers de escrita.
- Rate limiter: política nomeada `auth` (10/min/IP) além da global; `[EnableRateLimiting("auth")]`
  no `AuthController`.
- Startup (M1 já tem os 2 primeiros): `await db.Database.MigrateAsync()` →
  `LivroSeeder.SeedAsync()` → **`IdentitySeeder.SeedAsync()`** (novo).
- `SecurityHeadersMiddleware`: CSP inalterado (o `/admin` é servido pelo mesmo SPA).

### `ExceptionHandlingMiddleware` — mapa atualizado

| Exceção | HTTP |
|---|---|
| `DomainException` | 400 |
| `ConflitoException` | 409 |
| `KeyNotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| outra | 500 — logada antes |

### Configuração

`appsettings.json` (sem segredos):

```json
{
  "Jwt": { "Issuer": "BibliaTeologica", "Audience": "BibliaTeologica",
           "AccessTokenMinutes": 15, "RefreshTokenDays": 14 },
  "Identity": { "Lockout": { "MaxTentativas": 5, "Minutos": 15 } },
  "Email": { "Sink": "Dev", "Remetente": "nao-responder@biblia-teologica.local",
             "Smtp": { "Host": "", "Port": 587, "User": "", "From": "" } },
  "Armazenamento": { "CaminhoImagens": "/data/media/livros" },
  "Upload": { "TamanhoMaximoBytes": 2097152, "DimensaoMaximaPx": 1200,
              "FormatosAceites": ["image/jpeg", "image/png", "image/webp"] }
}
```

Segredos só em env / `appsettings.Development.json` (git-ignored):
`Jwt__SigningKey`, `AdminSeed__0__Email`, `AdminSeed__0__SenhaInicial`,
`Email__Smtp__Password`, `Email__Smtp__User`.

### Testes (`BibliaTeologica.Api.Tests` — `WebApplicationFactory<Program>`)

Factory override: `IEmailSender` → sink em memória que expõe o último OTP; SQLite temp;
seed de um admin (`admin@teste.local`) e um membro conhecidos, ambos com `DeveTrocarSenha=false`.

- `Login_Retorna200ComDesafio_QuandoCredenciaisValidas`
- `Login_Retorna401_QuandoSenhaErrada`
- `Login2fa_GravaCookies_QuandoCodigoCorreto`
- `Login2fa_Retorna401_QuandoCodigoErrado`
- `Refresh_RotacionaCookies_QuandoRefreshValido`
- `Refresh_Retorna401ERevoga_QuandoReutilizacao`
- `AdminLivros_Retorna401_SemCookie`
- `AdminLivros_Retorna403_ComCookieMembro`
- `AdminLivros_Retorna200_ComCookieAdmin`
- `AdminEscrita_Retorna403_SemHeaderCsrf`
- `CriarLivro_Retorna409_QuandoSlugDuplicado`
- `PutCapa_Retorna400_QuandoMagicBytesInvalidos`
- `PutCapa_Retorna200ComCapaUrl_QuandoImagemValida`
- `GetMediaLivros_Retorna200ImageWebp`
- `GetMediaLivros_Retorna400_QuandoPathTraversal`
- `RegistarMembro_Retorna201_ECriaNaRoleMembro`

## 9. Frontend (Angular 22 + Material)

`ng add @angular/material` — tema custom mínimo. Material usado **só** em `features/auth`,
`features/conta`, `features/admin`. A feature `biblia` (M1) fica intacta.

### `core/auth/`

```ts
// auth.models.ts
interface Usuario { email; nomeExibicao; roles: string[]; deveTrocarSenha: boolean }

// auth.store.ts  (providedIn: 'root', signals)
usuario = signal<Usuario | null>(null);  carregado = signal(false);
estaAutenticado = computed(() => this.usuario() !== null);
ehAdmin        = computed(() => this.usuario()?.roles.includes('Admin') ?? false);
deveTrocarSenha = computed(() => this.usuario()?.deveTrocarSenha ?? false);

// auth.service.ts
login(email, senha)            -> { desafio2fa, tokenDesafio }
verificar2fa(tokenDesafio, codigo) -> Usuario   (cookies definidos pelo servidor)
registar(email, senha, nome)
logout()
carregarUsuario()             // GET /api/auth/eu -> hidrata store + carregado=true
atualizarPerfil / trocarEmail / trocarSenha
```

### Interceptors (funcionais)

1. `withCredentials` — `req.clone({ withCredentials: true })` para `/api/*`.
2. `csrf` — métodos não-seguros: lê cookie `bt_csrf`, adiciona `X-CSRF-Token`.
3. `refresh` — `401` de endpoint ≠ `/api/auth/*`: um `POST /api/auth/refresh` (mutex
   `inflight$` partilhado), depois repete o pedido; falha → `authStore.usuario.set(null)` +
   `router.navigate(['/entrar'])`.

### Rotas e guards

```
/entrar                 -> AuthEntrarComponent   (email/senha -> OTP)
/registar               -> AuthRegistarComponent
/conta                  -> [authGuard]  ContaPerfilComponent
/conta/trocar-senha     -> [authGuard]  ContaTrocarSenhaComponent
/admin  (lazy admin.routes)             [adminGuard]
  ''                    -> AdminDashboardComponent
  livros                -> AdminLivrosListaComponent   (MatTable)
  livros/novo           -> AdminLivroFormComponent
  livros/:id            -> AdminLivroFormComponent + secção de capa
```

- `authGuard` — hidrata o store se `!carregado`; bloqueia → `/entrar?retorno=…`.
- `adminGuard` — autenticado + `ehAdmin` + `!deveTrocarSenha` (senão → `/conta/trocar-senha`).
- App boot: `carregarUsuario()` corre uma vez (APP_INITIALIZER ou primeiro guard).

### `AdminLivroFormComponent`

Reactive form: `nome`, `mat-select` seção (10 opções do enum), `ordem` (1..66),
textareas (`quem`/`porque`/`paraQuem`/`estrutura`/`tema`), `versiculo`, **`ideias` como
`FormArray`** (adicionar/remover linhas). Capa: só no modo edição — `<input type=file
accept="image/*">` → preview local → `PUT /api/admin/livros/:id/capa` → mostra `capaUrl` +
botão remover. Criar → `201` → `router.navigate(['/admin/livros', id])`.

### Nav pública

Um link discreto **"Entrar"** no rodapé da sidebar do fichário. Sem mais destaque
(Membros ainda não têm funcionalidade).

### Testes (Vitest)

- `AuthStore` / `AuthService` — fluxo login (desafio → 2fa), hidratação via `/eu`, logout.
- Interceptors — `csrf` adiciona header; `refresh` faz um refresh + retry; refresh falha →
  `usuario=null` + navega `/entrar`.
- `adminGuard` — bloqueia Membro, redireciona; deixa passar Admin.
- `AdminLivroFormComponent` — validação (required, ordem), `FormArray` de ideias, submit.
- `AdminLivrosListaComponent` — `MatTable` renderiza + filtro.

## 10. Docker Compose

`docker-compose.yml` — serviço `api` ganha:

```yaml
    environment:
      Jwt__SigningKey: "${BT_JWT_KEY:?defina BT_JWT_KEY}"
      AdminSeed__0__Email: "${BT_ADMIN_EMAIL:?}"
      AdminSeed__0__SenhaInicial: "${BT_ADMIN_SENHA:?}"
      Armazenamento__CaminhoImagens: "/data/media/livros"
      Email__Sink: "Dev"
    volumes:
      - biblia-data:/data
      - biblia-media:/data/media
volumes:
  biblia-data:
  biblia-media:
```

- `.env` na raiz (git-ignored) com `BT_JWT_KEY`, `BT_ADMIN_EMAIL`, `BT_ADMIN_SENHA`;
  `.env.example` commitado.
- `Dockerfile` da API: `RUN mkdir -p /data/media/livros && chown -R $APP_UID /data/media`
  (a imagem `aspnet:8` corre como não-root).
- `docker-compose.prod.yml` — `Email__Sink: Smtp` + credenciais SMTP via env.

## 11. CI

Sem alterações estruturais — `ci-cd.yml` já cobre `dotnet test` (todos os projetos) e
`npm test`. O job `frontend` passa a compilar o Material. O `Docker Push` continua a
empurrar `biblia-teologica-api` / `-web`.

`SECURITY.md` atualizado: secção de autenticação/autorização deixa de ser "quando o M2
chegar" e passa a descrever o modelo real (cookies HttpOnly, refresh rotativo, 2FA,
lockout, CSRF).

## 12. Segurança — checklist do M2 (regras de ouro do `CLAUDE.md`)

| # | Ponto | Tratamento no M2 |
|---|---|---|
| 1 | Validação no servidor | DataAnnotations nos DTOs + invariantes de `Livro` + verificações de conflito no serviço |
| 2 | Rotas protegidas só no front | `[Authorize(Roles="Admin")]` em **todo** `/api/admin/**`; guards Angular são só UX |
| 3 | Segredos | `Jwt__SigningKey`, `AdminSeed__*`, SMTP — só env / `appsettings.Development.json` / `.env` (todos git-ignored); `.env.example` commitado sem valores |
| 4 | Autorização por utilizador | `/api/conta/*` opera sempre sobre `IUsuarioAtual.Id`, nunca sobre um id vindo do cliente |
| 5 | Rate limiting | Política `auth` 10/min/IP em `/api/auth/*` + global do M1 |
| 6 | Upload | content-type + magic bytes + tamanho + re-encode WebP + nome `{guid}.webp` + `/data/media` fora do webroot + validação anti-traversal ao servir |
| 7 | Dados sensíveis na resposta | `UsuarioDto`/`PerfilDto` curados; nunca hash de password, nunca o refresh token no corpo, nunca `ApplicationUser` |
| 8 | XSS | Angular escapa; Material não usa `innerHTML` de dados; sem `bypassSecurityTrust*` |
| 9 | CORS | inalterado — política nomeada, origem explícita; cookies `SameSite=Strict` |
| 10 | Erros | `DomainException`/`ConflitoException` → 4xx limpos; 500 genérico em Production; stack só em Development |
| 11 | Cookies | `HttpOnly` + `Secure` + `SameSite=Strict` no access/refresh; refresh com path restrito e hash na BD; rotação + deteção de reutilização |
| 12 | 2FA / lockout | OTP por email em cada login; lockout após 5 falhas / 15 min; troca de senha forçada no 1º login do admin |

## 13. Sequência de implementação (para o plano)

Ordem: **contratos antes de implementações** (as interfaces vivem na Application, as
implementações na Infra — a Infra não compila sem os contratos).

1. **Domain** — `Livro` (Atualizar/Publicar/Despublicar/DefinirCapa/RemoverCapa +
   `CapaImagemPath`), `ConflitoException` + testes de Domain.
2. **Application — contratos** — todas as interfaces novas (`IIdentityService`,
   `ITokenService`, `IRefreshTokenStore`, `IEmailSender`, `IProcessadorImagem`,
   `IArmazenamentoImagens`, `IUsuarioAtual`), `ILivroRepository` estendido, todos os DTOs
   e tipos auxiliares. Sem implementações — a solução compila.
3. **Infra — Identity persistência** — pacotes (Identity, ImageSharp); `ApplicationUser`,
   `RefreshToken` + config; `AppDbContext` passa a `IdentityDbContext`; `LivroConfiguration`
   + `CapaImagemPath`; migração `M2_IdentityELivroCapa`; `IdentitySeeder` (via
   `UserManager`/`RoleManager`) + testes de Infra do seeder.
4. **Infra — implementações de serviço** — `IdentityService`, `TokenService`,
   `RefreshTokenStore`, `LogEmailSender`/`SmtpEmailSender`, `ProcessadorImagemSharp`,
   `ArmazenamentoImagensLocal`; `AddInfrastructure` (Identity, JwtBearer, sink) + testes de Infra.
5. **Application — implementações + repositório** — `LivroRepository` estendido;
   `AuthService`, `ContaService`, `LivroCommandService`; `AddApplication` + testes com mocks.
6. **API — auth** — `AuthController`, `ContaController`, `CookieWriter`, `CsrfActionFilter`,
   `UsuarioAtual`, `Program.cs` (auth + rate limit `auth`), `ExceptionHandlingMiddleware`
   atualizado + testes de integração de auth.
7. **API — admin/media** — `LivrosAdminController`, `MediaController`, `LivrosController`
   (capaUrl) + testes de integração de admin/upload.
8. **Docker** — `biblia-media` volume, `.env.example`, `chown` no Dockerfile; subir a stack
   e exercitar login → editar livro → upload de capa → ver no site público.
9. **Frontend — auth core** — `@angular/material`, `AuthStore`, `AuthService`, interceptors,
   guards + testes.
10. **Frontend — páginas** — `entrar`, `registar`, `conta` (perfil + trocar-senha),
    shell `/admin`, `admin/livros` lista (MatTable), `admin/livros` form + capa + testes.
11. **Docs** — `SECURITY.md`, `README.md` (secção auth + `appsettings.Development.json`
    exemplo), `CLAUDE.md` (regra de ouro sobre auth se fizer sentido consolidar), `ROADMAP.md`
    (marcar M2, notar o que foi antecipado do M4).

---

**Fim da spec.**
