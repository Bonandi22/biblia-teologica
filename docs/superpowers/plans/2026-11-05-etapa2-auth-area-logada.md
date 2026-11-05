# Etapa 2: autenticação e área logada — plano de implementação

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Contas confiáveis para abrir o site ao público: cadastro com termo RGPD, código por e-mail, recuperação de senha, troca de e-mail comprovada, "Meu perfil" com avatar no topo e apagar conta.

**Architecture:** Regras na Application (`AuthService`, `ContaService`), Identity atrás de `IIdentityService`, controllers finos. Envios que poderiam revelar uma conta passam por uma fila em memória, e o front partilha um componente de código entre entrar e criar conta.

**Tech Stack:** .NET 10, ASP.NET Identity, EF Core + SQLite, xUnit + Moq + AwesomeAssertions; Angular 22 standalone + signals, Vitest. Nenhum pacote novo.

**Spec:** `docs/superpowers/specs/2026-11-05-etapa2-auth-area-logada-design.md`. Leia antes de qualquer task: ela fixa os fluxos, status e mensagens que este plano não repete.

## Global Constraints

- Nenhuma resposta, em status, corpo ou tempo, revela se um e-mail tem conta no cadastro, em "esqueci", no reenvio ou na troca de e-mail.
- Limite por endereço: 3 envios a cada 15 minutos, por par (endereço normalizado, finalidade). Desafio: 5 minutos. Link de redefinição: 1 hora. Conta não confirmada: apagada após 7 dias.
- Só o código do login é enviado de forma síncrona. Todo o resto passa por `IFilaEmail`.
- Logs nunca contêm código, token, senha ou telefone. O telefone nunca vai para JWT ou `/api/auth/eu`.
- URL de e-mail vem de `Site:UrlPublica`, nunca do cabeçalho Host.
- Endpoint público novo leva `[AllowAnonymous]` e comentário justificando. Autenticado de escrita leva `CsrfActionFilter`.
- `/api/livros` e `/api/livros/{slug}` não mudam. Migrações só na Infrastructure. Versões só em `Directory.Packages.props`.
- Interface em português do Brasil. Telas seguem `DESIGN.md`, sem cartões nem sombras, com o skill frontend-design nas tasks de tela.
- Nomes de teste: `Metodo_ResultadoEsperado_QuandoCondicao`. Subscrições com `takeUntilDestroyed(this.destroyRef)`.
- **SDK:** a máquina só tem o SDK 8. Rode o backend em container (PowerShell, na raiz):
  `docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test BibliaTeologica.sln`
- Rodapé dos commits conforme `CLAUDE.md`. Antes de cada commit, confira o staged: `20260830011929_InitialCreate.cs`, `.impeccable/`, `.agents/`, `.aider-desk/`, `.github/agents|hooks|skills` e `skills-lock.json` são alterações anteriores ou alheias e **não entram**.

## Review Focus

1. E-mail com maiúsculas ou espaços nas pontas (`" Maria@X.com "`) resolve para a mesma conta e o mesmo contador de limite. Teste na Task 1 e na Task 3.
2. `/redefinir-senha` aberta sem fragmento ou com fragmento malformado mostra "Este link é inválido ou venceu." e o link para pedir outro, sem quebrar. Teste na Task 11.
3. Desafio vencido com a tela aberta: a pessoa é informada e levada a `/entrar`, em vez de ver "código inválido" para sempre. Teste na Task 9.
4. Nome com uma palavra, espaços extras, acento ou emoji gera iniciais válidas, sem caractere partido. Teste na Task 12.
5. Telefone só com símbolos, só com espaços ou internacional (`+351 912 345 678`): espaços viram "sem telefone", símbolos são rejeitados, internacional é aceito. Teste na Task 2.

---

## Backend

### Task 1: Limite por endereço e fila de e-mail

**Files:**
- Create: `src/BibliaTeologica.Application/Common/Interfaces/ILimiteEnvio.cs` (com o enum `FinalidadeEnvio`)
- Create: `src/BibliaTeologica.Application/Common/Interfaces/IFilaEmail.cs`
- Create: `src/BibliaTeologica.Infrastructure/Email/LimiteEnvioEmMemoria.cs`
- Create: `src/BibliaTeologica.Infrastructure/Email/FilaEmailEmMemoria.cs` (`Channel` + `BackgroundService`)
- Modify: `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`, `src/BibliaTeologica.Api/appsettings.json`
- Modify: `tests/BibliaTeologica.Api.Tests/AutenticacaoWebFactory.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Email/LimiteEnvioEmMemoriaTests.cs`, `FilaEmailEmMemoriaTests.cs`

**Interfaces:**
- Produces:
  - `enum FinalidadeEnvio { Login, Cadastro, Redefinicao, TrocaEmail, Aviso }`
  - `ILimiteEnvio.TentarConsumir(string chave, FinalidadeEnvio finalidade): bool`
  - `IFilaEmail.Enfileirar(string para, string assunto, string corpoHtml): void`
  - Config `EnvioEmail:MaxPorJanela` (3) e `EnvioEmail:JanelaMinutos` (15)
  - `OtpSink.Enviados`: lista de `(Para, Assunto, Corpo)`, visível aos testes das próximas tasks

- [ ] **Step 1: Branch e spec.** Crie a issue "Etapa 2: autenticação e área logada" (`gh issue create`), a branch `feature/BT-<n>-etapa2-auth` a partir de main, e faça commit do spec e deste plano.
- [ ] **Step 2: Escreva os testes que falham**

```csharp
TentarConsumir_RecusaQuartoEnvio_QuandoMesmoParNaJanela      // true,true,true,false
TentarConsumir_Permite_QuandoFinalidadeDiferente             // Redefinicao esgotada não afeta Login
TentarConsumir_ContaJunto_QuandoEnderecoDifereSoEmCaixaEEspaco // " Maria@X.com " == "maria@x.com"
TentarConsumir_LiberaDeNovo_QuandoJanelaPassa                // com FakeTimeProvider ou TimeProvider próprio
Enfileirar_EntregaAoSender_QuandoServicoRodando
Enfileirar_ContinuaEntregando_QuandoEnvioAnteriorLanca       // 1º sender lança, 2º e-mail chega
```

- [ ] **Step 3: Rode e veja falhar** com o comando de `Global Constraints`, filtro `--filter "FullyQualifiedName~Email"`. Esperado: não compila.
- [ ] **Step 4: Implemente.** `LimiteEnvioEmMemoria` é singleton, usa `IMemoryCache` com expiração absoluta e `lock` e chave `"{finalidade}:{chave.Trim().ToUpperInvariant()}"`. `FilaEmailEmMemoria` é singleton com `Channel.CreateUnbounded`; o `BackgroundService` cria um scope para cada envio, porque `IEmailSender` é scoped. Falha registra `LogError` com a exceção e o assunto, sem destinatário nem corpo.
- [ ] **Step 5: Ajuste a factory.** `EnvioEmail:MaxPorJanela=100000`, porque os testes partilham endereços. Substitua `IFilaEmail` por um dublê que chama o `OtpSink` na hora, para os testes de endpoint serem determinísticos, e acrescente `OtpSink.Enviados`.
- [ ] **Step 6: Rode e veja passar.** Esperado: PASS, sem regressões.
- [ ] **Step 7: Commit** `feat(auth): adiciona limite de envio por endereço e fila de e-mail`.

### Task 2: Dados da conta, telefone, termo e mensagens

**Files:**
- Modify: `src/BibliaTeologica.Infrastructure/Identity/ApplicationUser.cs`
- Create: migração `Etapa2_TermoAceite` em `Persistence/Migrations`
- Create: `src/BibliaTeologica.Application/Common/Telefone.cs`
- Create: `src/BibliaTeologica.Application/Auth/TermoPrivacidade.cs`
- Create: `src/BibliaTeologica.Application/Auth/ModelosEmail.cs`
- Create: `src/BibliaTeologica.Infrastructure/Identity/DescritorErrosPortugues.cs`
- Modify: `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Common/TelefoneTests.cs`, `tests/BibliaTeologica.Infrastructure.Tests/Identity/DescritorErrosPortuguesTests.cs`

**Interfaces:**
- Produces:
  - `ApplicationUser.TermoAceitoEm: DateTime?`, `TermoVersao: string?`
  - `TermoPrivacidade.VersaoAtual: const string = "2026-11-05"`
  - `Telefone.TentarNormalizar(string? valor, out string? normalizado): bool`
  - `ModelosEmail`: métodos estáticos que devolvem `(string Assunto, string Corpo)`: `Codigo(string codigo)`, `CadastroExistente()`, `Redefinicao(string link)`, `SenhaAlterada()`, `TrocaEmailCodigo(string codigo)`, `EmailEmUso()`, `EmailAlterado()`, `ContaApagada()`. O código fica em `<strong>`, como o `OtpSink` espera.

- [ ] **Step 1: Escreva os testes que falham**

```csharp
[Theory] // TentarNormalizar: (entrada, ok, resultado)
("+351 912 345 678", true, "+351912345678")
("(11) 98765-4321", true, "11987654321")
(null, true, null)      ("   ", true, null)
("---", false, null)    ("1234567", false, null)   // 7 dígitos
("1234567890123456", false, null)                  // 16 dígitos
("12a45678", false, null)  ("12+345678", false, null)

Descrever_MensagemEmPortugues_QuandoSenhaFraca  // PasswordTooShort, RequiresDigit, RequiresUpper, RequiresLower
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: não compila.
- [ ] **Step 3: Implemente** `Telefone`, `TermoPrivacidade`, `ModelosEmail` e `DescritorErrosPortugues : IdentityErrorDescriber`, registrado com `.AddErrorDescriber<>()`. Nos e-mails, só interpole códigos gerados e URLs já codificadas.
- [ ] **Step 4: Gere a migração** (só `TermoAceitoEm` e `TermoVersao`):
  `docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 sh -c "dotnet tool install -g dotnet-ef --version 10.* && /root/.dotnet/tools/dotnet-ef migrations add Etapa2_TermoAceite --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api"`
  Confira que o arquivo só acrescenta duas colunas e o snapshot.
- [ ] **Step 5: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(auth): acrescenta aceite do termo, telefone e mensagens em português`.

### Task 3: Criar conta

**Files:**
- Modify: `IIdentityService.cs`, `IdentityService.cs`, `IAuthService.cs`, `AuthService.cs`, `Dtos/RegistarMembroDto.cs`, `AuthController.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Auth/AuthServiceTests.cs`, `tests/BibliaTeologica.Api.Tests/Auth/AuthEndpointsTests.cs`, `SegurancaApiTests.cs` (atualizar chamadas de `registar`)

**Interfaces:**
- Consumes: `IFilaEmail`, `ILimiteEnvio`, `ModelosEmail`, `Telefone`, `TermoPrivacidade`.
- Produces:
  - `RegistarMembroDto(Email, Senha, NomeExibicao, bool AceitouTermo, string? Telefone, string? Site)`
  - `IIdentityService.ValidarSenhaAsync(string senha, CancellationToken): Task<IdentityResultDto>`
  - `IIdentityService.CriarMembroAsync(NovoMembro dados, CancellationToken): Task<ResultadoCriacao>`, com `NovoMembro(Email, Senha, NomeExibicao, Telefone, TermoVersao, TermoAceitoEm)` e `ResultadoCriacao(Guid? UserId, bool JaExiste, IReadOnlyList<string> Erros)`
  - `IAuthService.RegistarMembroAsync(dto, ct): Task<ResultadoCadastro>`, com `ResultadoCadastro(string? TokenDesafio, IReadOnlyList<string> Erros)`

- [ ] **Step 1: Escreva os testes de endpoint que falham**

```csharp
Registar_Responde202ComDesafio_QuandoContaNova               // conta com EmailConfirmed=false, TermoVersao, data
Registar_Responde202ComMesmoFormato_QuandoEmailJaExiste      // conta intacta; 1 aviso; nenhum <strong>; código falha com 401
Registar_MesmaConta_QuandoEmailDifereEmCaixaEEspaco          // " Membro@Teste.Local " cai no caso de existente
Registar_NaoCriaNemEnvia_QuandoCampoSiteVeioPreenchido       // 202, Enviados vazio
Registar_Responde400_QuandoSenhaFraca_MesmoComEmailExistente
Registar_Responde400_QuandoTermoNaoAceito_MesmoComEmailExistente
Registar_Responde400_QuandoTelefoneInvalido
Registar_DescartaEnvioE_Responde202_QuandoLimiteEsgotado     // factory própria com MaxPorJanela=1
VerificarCodigo_AbreSessaoEConfirmaEmail_QuandoCadastroNovo  // POST /login/2fa + GET /eu; EmailConfirmed=true
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL, hoje responde 201.
- [ ] **Step 3: Implemente.** Ordem em `RegistarMembroAsync`: campo `Site` preenchido → desafio falso; termo → erro; telefone → erro; `ValidarSenhaAsync` → erro; `CriarMembroAsync`. Desafio falso é `_token.GerarDesafioLogin(Guid.NewGuid())`. `CriarMembroAsync` grava `EmailConfirmed=false` e devolve `JaExiste` para os erros `DuplicateEmail` ou `DuplicateUserName`. No `JaExiste`, calcule o hash com `_users.PasswordHasher.HashPassword(new ApplicationUser(), senha)`, para igualar o tempo. Faça `Trim()` no e-mail antes de tudo. `VerificarTokenEmailAsync` passa a pôr `EmailConfirmed=true` no acerto.
- [ ] **Step 4: Atualize o controller:** `202 { tokenDesafio }`; erros como `ProblemDetails` 400, com `Detail` em português.
- [ ] **Step 5: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(auth): cadastro com termo, código e resposta única`.

### Task 4: Login e reenvio

**Files:**
- Modify: `AuthService.cs`, `IAuthService.cs`, `Dtos/ResultadoLoginDto.cs`, `IdentityService.cs`, `AuthController.cs`
- Create: `src/BibliaTeologica.Application/Auth/Dtos/ReenviarCodigoDto.cs`
- Test: `AuthServiceTests.cs`, `AuthEndpointsTests.cs`

**Interfaces:**
- Produces:
  - `EstadoLogin` ganha `LimiteEnvio` e `FalhaEnvio`
  - `IAuthService.ReenviarCodigoAsync(string tokenDesafio, CancellationToken): Task<ResultadoReenvio>`, com `enum ResultadoReenvio { Aceito, DesafioInvalido, Limite }`
  - `POST /api/auth/login/2fa/reenviar`: 204, 400 (desafio inválido ou vencido) ou 429

- [ ] **Step 1: Escreva os testes que falham**

```csharp
Login_Responde503SemVazarDetalhe_QuandoEnvioFalha        // sink que lança; corpo sem texto da exceção; erro registrado
Login_Responde429_QuandoLimiteEsgotado                   // senha certa, 4ª tentativa com MaxPorJanela=3
Login_PermiteEntrar_QuandoContaNaoConfirmada
Login_CalculaHash_QuandoEmailDesconhecido                // Application/Infra: verifica o hash falso, para igualar o tempo
Reenviar_Responde204EEnfileira_QuandoDesafioDeContaReal
Reenviar_Responde204SemEnviar_QuandoDesafioDeContaInexistente
Reenviar_Responde429_QuandoLimiteEsgotado_ComContaOuSem  // mesmo status nos dois casos
Reenviar_Responde400_QuandoDesafioAdulteradoOuVencido
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente.** O login consome `FinalidadeEnvio.Login` depois de a senha ser conferida; `catch` da exceção de envio com `LogError`, sem o código, devolve `FalhaEnvio`. O reenvio usa como chave o e-mail da conta ou, sem conta, `desafio:{id}`, e só enfileira se a conta existir e não estiver bloqueada. `VerificarSenhaAsync` calcula um hash descartável quando o e-mail não existe.
- [ ] **Step 4: Atualize o controller:** 429 e 503 como `ProblemDetails`, com as mensagens do spec.
- [ ] **Step 5: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(auth): reenvio de código e falha de envio sem erro interno`.

### Task 5: Esqueci e redefinir senha

**Files:**
- Create: `src/BibliaTeologica.Application/Auth/Dtos/EsqueciSenhaDto.cs`, `RedefinirSenhaDto.cs`
- Create: `src/BibliaTeologica.Infrastructure/Configuracao/SiteOptions.cs`, interface `IConfiguracaoSite` em `Application/Common/Interfaces`
- Modify: `IIdentityService.cs`, `IdentityService.cs`, `AuthService.cs`, `IAuthService.cs`, `AuthController.cs`, `DependencyInjection.cs`, `Program.cs`, `appsettings.json`, `docker-compose.yml`
- Test: `AuthEndpointsTests.cs` (ou `Auth/RedefinicaoSenhaTests.cs`), `EmailEmProducaoTests.cs`

**Interfaces:**
- Produces:
  - `EsqueciSenhaDto(Email, string? Site)`; `RedefinirSenhaDto(Email, Token, NovaSenha)`
  - `IAuthService.PedirRedefinicaoAsync(EsqueciSenhaDto, ct): Task`
  - `IAuthService.RedefinirSenhaAsync(RedefinirSenhaDto, ct): Task<IdentityResultDto>`
  - `IConfiguracaoSite.UrlPublica: string`
  - `IIdentityService.GerarTokenRedefinicaoAsync(Guid, ct)`, `RedefinirSenhaAsync(string email, string token, string nova, ct): Task<ResultadoRedefinicao>`, com `ResultadoRedefinicao(Guid? UserId, bool TokenInvalido, IReadOnlyList<string> Erros)`

- [ ] **Step 1: Escreva os testes que falham**

```csharp
Esqueci_Responde202SemEnviar_QuandoEmailDesconhecido
Esqueci_Responde202EEnviaLink_QuandoContaExiste    // href começa com Site:UrlPublica + "/redefinir-senha#"
Esqueci_IgnoraCabecalhoHost_AoMontarLink           // pedido com Host: evil.test não muda o link
Esqueci_Responde202SemEnviar_QuandoLimiteEsgotado
Esqueci_NaoAfetaLimiteDoLogin_QuandoRepetido       // esgota Redefinicao; login ainda envia código
Redefinir_TrocaSenhaERevogaSessoes_QuandoTokenValido // sessão antiga refresh=401; login novo pede código; aviso enviado
Redefinir_Responde400Generico_QuandoTokenInvalidoOuEmailInexistente // mesma mensagem nos dois
Redefinir_Responde400ComRegra_QuandoTokenValidoESenhaFraca
Redefinir_Falha_QuandoTokenReutilizado
Redefinir_ConfirmaEmailEDesbloqueia_QuandoSucesso  // EmailConfirmed=true, lockout zerado, DeveTrocarSenha=false
Producao_FalhaAoIniciar_QuandoSiteUrlPublicaAusente
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL, rotas inexistentes.
- [ ] **Step 3: Implemente.** `services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(1))`. Link: `{UrlPublica}/redefinir-senha#email={EscapeDataString}&token={EscapeDataString}`. O envio vai sempre pela fila. Rotas `POST /api/auth/senha/esqueci` e `/redefinir`. Para validar o token antes da senha, use `VerifyUserTokenAsync(user, Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token)`. Em `docker-compose.yml`, acrescente `Site__UrlPublica: "http://localhost:4300"`. Em Production, falhe ao iniciar sem o valor, como no e-mail.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(auth): esqueci e redefinir senha sem revelar contas`.

### Task 6: Trocar e-mail com código

**Files:**
- Create: `src/BibliaTeologica.Application/Conta/Dtos/SolicitarTrocaEmailDto.cs`, `ConfirmarTrocaEmailDto.cs`
- Delete: `src/BibliaTeologica.Application/Conta/Dtos/TrocarEmailDto.cs`
- Modify: `IIdentityService.cs`, `IdentityService.cs`, `IContaService.cs`, `ContaService.cs`, `ContaController.cs`
- Test: `tests/BibliaTeologica.Api.Tests/Conta/ContaEndpointsTests.cs`, `tests/BibliaTeologica.Application.Tests/Conta/ContaServiceTests.cs`, `tests/BibliaTeologica.Infrastructure.Tests/Identity/IdentityServiceTests.cs`

**Interfaces:**
- Produces:
  - `SolicitarTrocaEmailDto(NovoEmail, Senha)`, `ConfirmarTrocaEmailDto(NovoEmail, Codigo)`
  - `IIdentityService.ConferirSenhaAsync(Guid, string, ct): Task<ResultadoSenha>`: conta a falha para o bloqueio. Também usada na Task 7.
  - `IIdentityService.GerarCodigoTrocaEmailAsync(Guid, string novoEmail, ct): Task<string>`
  - `IIdentityService.ConfirmarTrocaEmailAsync(Guid, string novoEmail, string codigo, ct): Task<IdentityResultDto>`
  - `IContaService.SolicitarTrocaEmailAsync(Guid, dto, ct): Task<IdentityResultDto>`
  - `IContaService.ConfirmarTrocaEmailAsync(Guid, dto, string? ip, ct): Task<ResultadoLoginDto>`

- [ ] **Step 1: Escreva os testes que falham**

```csharp
Solicitar_EnviaCodigoAoNovoEndereco_SemTrocarAinda      // e-mail da conta continua o antigo
Solicitar_RespondeIgual_QuandoEnderecoJaUsado           // 204; aviso ao dono; nenhum <strong>
Solicitar_Responde400EContaFalha_QuandoSenhaErrada
Solicitar_Responde429_QuandoLimiteEsgotado
Confirmar_TrocaERevogaOutrasSessoes_QuandoCodigoCerto   // cookies novos; sessão antiga 401; login com e-mail novo
Confirmar_MantemEmailConfirmado_QuandoSucesso
Confirmar_AvisaEnderecoAntigo_QuandoSucesso
Confirmar_Falha_QuandoCodigoDeOutroEndereco             // código pedido para A, confirmação para B
Confirmar_ErroGenerico_QuandoEnderecoFoiOcupadoNoMeioTempo
Confirmar_ContaFalha_QuandoCodigoErrado
PutEmail_Responde405Ou404_DepoisDeRemovido
Solicitar_Responde403_QuandoSemCsrf
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: não compila.
- [ ] **Step 3: Implemente.** Código: `GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "TrocarEmail:" + novoNormalizado)`, com `VerifyUserTokenAsync` na volta. Na confirmação, `SetEmailAsync` zera `EmailConfirmed`: reponha `true`, chame `SetUserNameAsync` e atualize o security stamp. Emissão de sessão como em `TrocarSenhaAsync`: `RevogarCadeiaAsync` e depois `EmitirAsync`. Endpoints com `[EnableRateLimiting("auth")]` e `CsrfActionFilter`.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(conta): troca de e-mail comprovada por código`.

### Task 7: Telefone no perfil, aviso de senha e apagar conta

**Files:**
- Create: `src/BibliaTeologica.Application/Conta/Dtos/ApagarContaDto.cs`
- Modify: `Conta/Dtos/PerfilDto.cs`, `AtualizarPerfilDto.cs`, `IIdentityService.cs`, `IdentityService.cs`, `IContaService.cs`, `ContaService.cs`, `ContaController.cs`, `Auth/Dtos/UsuarioDto.cs` só se necessário
- Test: `ContaEndpointsTests.cs`, `ContaServiceTests.cs`

**Interfaces:**
- Produces:
  - `PerfilDto` ganha `Telefone: string?`; `AtualizarPerfilDto(NomeExibicao, string? Telefone)`
  - `IIdentityService.AtualizarPerfilAsync(Guid, string nome, string? telefone, ct)` substitui `AtualizarNomeAsync`
  - `IIdentityService.ObterTelefoneAsync(Guid, ct): Task<string?>`
  - `IContaService.ApagarAsync(Guid, ApagarContaDto, ct): Task<ResultadoApagar>`, com `enum ResultadoApagar { Apagada, SenhaIncorreta, UltimoAdmin }`
  - `DELETE /api/conta` com `[FromBody]` explícito (o `[ApiController]` não infere corpo em DELETE)

- [ ] **Step 1: Escreva os testes que falham**

```csharp
Atualizar_GuardaTelefoneNormalizado_QuandoValido
Atualizar_ApagaTelefone_QuandoVazio
Atualizar_Responde400_QuandoTelefoneInvalido
Eu_NaoExpoeTelefone
TrocarSenha_EnfileiraAviso_QuandoSucesso
Apagar_RemoveContaESessoesELimpaCookies_QuandoSenhaCerta // refresh antigo=401; login=401; aviso enviado; e-mail livre para novo cadastro
Apagar_Responde400EContaFalha_QuandoSenhaErrada
Apagar_Responde409_QuandoUltimoAdmin
Apagar_Permite_QuandoHaOutroAdmin
Apagar_Responde403_QuandoSemCsrf
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente.** Enfileire o aviso de apagamento antes do `DeleteAsync`, guardando o e-mail. Confirme que os refresh tokens somem junto com a conta, removendo-os explicitamente se a FK não cascatear. `PUT /api/conta` responde 400 para telefone inválido.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(conta): telefone no perfil e apagar conta`.

### Task 8: Limpeza de contas não confirmadas

**Files:**
- Create: `src/BibliaTeologica.Infrastructure/Identity/LimpezaContasNaoConfirmadas.cs`
- Modify: `IIdentityService.cs`, `IdentityService.cs`, `DependencyInjection.cs`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Identity/IdentityServiceTests.cs`

**Interfaces:**
- Produces: `IIdentityService.ApagarNaoConfirmadasAsync(DateTime criadasAntesDe, CancellationToken): Task<int>`

- [ ] **Step 1: Escreva os testes que falham**

```csharp
ApagarNaoConfirmadas_Apaga_QuandoCriadaAntesDoLimite
ApagarNaoConfirmadas_Mantem_QuandoCriadaDepoisDoLimite
ApagarNaoConfirmadas_Mantem_QuandoConfirmada
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: não compila.
- [ ] **Step 3: Implemente.** `BackgroundService` com `PeriodicTimer` de 24 horas, primeira execução ao iniciar, limite `DateTime.UtcNow.AddDays(-7)`. Scope por execução; exceção é registrada e o laço continua. Log só da contagem.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS em toda a solução.
- [ ] **Step 5: Commit** `feat(auth): apaga contas não confirmadas após 7 dias`.

## Frontend

Em todas as tasks de tela, carregue o skill `frontend-design` antes de escrever o template. Comando: `cd frontend/biblia-teologica-web && npm test && npx tsc --noEmit -p tsconfig.app.json`.

### Task 9: Serviços, utilitários e tela de código

**Files:**
- Create: `src/app/shared/ui/verificar-codigo/verificar-codigo.component.{ts,html,scss,spec.ts}`
- Create: `src/app/core/auth/erros-auth.ts` (+ spec), `src/app/core/auth/caminho-interno.ts` (+ spec)
- Modify: `core/auth/auth.service.ts` e spec, `core/services/conta.service.ts`, `features/auth/entrar/entrar.component.{ts,html,scss,spec.ts}`

**Interfaces:**
- Consumes: endpoints das Tasks 3 a 7.
- Produces:
  - `AuthService.registar(dto: NovoCadastro): Observable<DesafioLogin>`, `reenviarCodigo(tokenDesafio): Observable<void>`, `esqueciSenha(email, site): Observable<void>`, `redefinirSenha(dto): Observable<void>`
  - `ContaService.atualizarPerfil({ nomeExibicao, telefone })` substitui `atualizarNome`; `solicitarTrocaEmail(dto)`, `confirmarTrocaEmail(dto): Observable<Usuario>`, `apagar(senha)`; `Perfil.telefone: string | null`
  - `<bt-verificar-codigo [tokenDesafio] [aEnviar] [erro] (enviar)="string" (voltar)="void" (vencido)="void">`
  - `mensagemDeErro(e: HttpErrorResponse, padrao: string): string`: só usa `detail` em 400, 409 e 423; 429 tem texto próprio
  - `caminhoInterno(valor: string | null): string | null`: aceita só `/x`, recusa `//x`, `/\x` e URL absoluta

- [ ] **Step 1: Escreva os testes que falham**

```ts
it('mensagemDeErro_naoExpoeDetail_quando500')
it('caminhoInterno_recusa_quandoDuasBarrasOuUrlAbsoluta')
it('verificarCodigo_reenviaEMostraAviso_quandoClicaReenviar') // botão desabilitado durante o envio
it('verificarCodigo_emiteVencido_depoisDe5Minutos')           // vi.useFakeTimers
it('verificarCodigo_emiteVencido_quandoReenvioResponde400')
it('verificarCodigo_mostraEspera_quandoReenvioResponde429')
it('entrar_navegaParaRaiz_quandoSemRetorno_mesmoSendoAdmin')
it('entrar_ignoraRetorno_quandoExterno')
it('entrar_voltaACredenciaisComAviso_quandoDesafioVence')
it('entrar_mostraMensagem_quando503')
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente.** Tira a fase de código de `EntrarComponent` para o componente partilhado. A tela de entrar segue o esboço aprovado: título, lead, campos, "Esqueci minha senha" e, sob filete, "Ainda não tem conta? Criar conta", preservando `retorno`. Remova o aviso `registado`. Destino: `caminhoInterno(retorno) ?? '/'`; `deveTrocarSenha` continua indo para `/conta/trocar-senha`.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS, `tsc` limpo.
- [ ] **Step 5: Commit** `feat(web): tela de código e serviços da conta`.

### Task 10: Tela de criar conta

**Files:**
- Modify: `features/auth/registar/registar.component.{ts,html,scss,spec.ts}`
- Create: `shared/ui/campo/senha-forte.validator.ts` (+ spec)

**Interfaces:**
- Consumes: `AuthService.registar`, `<bt-verificar-codigo>`, `mensagemDeErro`, `caminhoInterno`.
- Produces: `senhaForte(): ValidatorFn`, com 8 ou mais caracteres, maiúscula, minúscula e dígito, igual à política do Identity. Usado também nas Tasks 11 e 13.

- [ ] **Step 1: Escreva os testes que falham**

```ts
it('registar_naoEnvia_quandoTermoDesmarcado')          // caixa começa desmarcada; erro visível
it('registar_enviaTelefoneNulo_quandoVazio')
it('registar_mostraTelaDeCodigo_quando202')
it('registar_abreSessaoENavega_quandoCodigoCerto')     // respeita retorno
it('registar_levaAEntrarComTextoNeutro_quandoDesafioVence')
it('registar_mostraErroDoServidor_quando400')
it('registar_campoSiteFicaForaDaTabulacao')            // tabindex=-1, aria-hidden, autocomplete=off
it('registar_linkDoTermoAbreEmNovaAba')                // /privacidade, target=_blank, rel=noopener
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente** o esboço aprovado: Nome, E-mail, Telefone (opcional), Senha com a regra visível, caixa com o texto exato do spec, "Criar conta" e "Já tem conta? Entrar". O campo `site` fica fora da tela por CSS, sem `display:none`. Depois do 202, mostra `<bt-verificar-codigo>`.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(web): criar conta com termo e telefone opcional`.

### Task 11: Esqueci e nova senha

**Files:**
- Create: `features/auth/esqueci-senha/esqueci-senha.component.{ts,html,scss,spec.ts}`, `features/auth/redefinir-senha/redefinir-senha.component.{ts,html,scss,spec.ts}`
- Modify: `app.routes.ts`

**Interfaces:**
- Consumes: `AuthService.esqueciSenha`, `redefinirSenha`, `senhaForte`.
- Produces: rotas `/esqueci-senha`, `/redefinir-senha`.

- [ ] **Step 1: Escreva os testes que falham**

```ts
it('esqueci_mostraMesmaMensagem_quando202')
it('esqueci_mostraEspera_quando429')
it('redefinir_mostraLinkInvalido_quandoSemFragmento')
it('redefinir_mostraLinkInvalido_quandoFragmentoMalformado') // sem token, ou "%E0%A4%A"
it('redefinir_limpaFragmentoDaBarra_aoCarregar')
it('redefinir_navegaParaEntrarComAviso_quandoSucesso')
it('redefinir_mostraInvalidoComLinkParaPedirOutro_quando400DeToken')
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente.** Leia `route.snapshot.fragment` com `URLSearchParams` dentro de `try`, guarde em variáveis privadas e limpe com `history.replaceState`. A mensagem de "esqueci" é: "Se houver uma conta com esse e-mail, enviamos um link válido por 1 hora."
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(web): recuperação de senha`.

### Task 12: Topo com avatar

**Files:**
- Create: `core/layout/header/iniciais.ts` (+ spec), `core/layout/header/menu-conta/menu-conta.component.{ts,html,scss,spec.ts}`
- Modify: `core/layout/header/header.component.{ts,html,scss,spec.ts}`, `DESIGN.md`

**Interfaces:**
- Consumes: `AuthStore`, `AuthService.logout`.
- Produces: `iniciais(nome: string): string`; `<bt-menu-conta>`.

- [ ] **Step 1: Escreva os testes que falham**

```ts
// iniciais
['Maria Souza','MS'], ['  maria   da   silva ','MS'], ['Ana','A'], ['Élida Órfão','ÉÓ'], ['😀 Ana','😀A'], ['','?']
it('header_mostraSoEntrar_quandoVisitante')            // sem "Criar conta"
it('header_mostraAvatar_quandoLogado')
it('menuConta_abreEFecha_comEscDevolvendoFoco')        // aria-expanded
it('menuConta_fecha_quandoCliqueFora')
it('menuConta_navegaParaPerfil_eFecha')
it('menuConta_saiENavegaParaRaiz_quandoClicaSair')
it('menuConta_atualizaIniciais_quandoNomeMudaNaStore')
it('header_listaItensDiretos_noMenuDoCelular')
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente** conforme o spec: botão de 44px com iniciais, menu sem sombra e filete de tinta de 2px. `iniciais` usa `Array.from` para não partir emoji. Se o logout falhar, mantém a sessão e mostra o erro, como hoje em `perfil`.
- [ ] **Step 4: Documente** o avatar e o menu em `DESIGN.md`, seção Navigation.
- [ ] **Step 5: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(web): avatar e menu da conta no topo`.

### Task 13: Meu perfil e trocar senha

**Files:**
- Modify: `features/conta/perfil/perfil.component.{ts,html,scss,spec.ts}`, `features/conta/trocar-senha/trocar-senha.component.{ts,html,scss,spec.ts}`

**Interfaces:**
- Consumes: `ContaService.atualizarPerfil`, `solicitarTrocaEmail`, `confirmarTrocaEmail`, `apagar`; `senhaForte`; `AuthStore.definir`.

- [ ] **Step 1: Escreva os testes que falham**

```ts
it('perfil_naoMostraPerfis')
it('perfil_atualizaAvatar_quandoSalvaNome')              // store com nome novo
it('perfil_limpaTelefone_quandoCampoVazio')
it('perfil_pedeCodigo_depoisDeSolicitarTrocaEmail')
it('perfil_mostraEmailNovo_quandoConfirmaCodigo')
it('perfil_mostraErro_quandoCodigoInvalido')
it('perfil_pedeConfirmacaoEmTela_antesDeApagar')         // sem window.confirm
it('perfil_limpaSessaoENavegaParaRaiz_quandoApagada')
it('perfil_explicaMotivo_quandoUltimoAdmin')             // 409
it('trocarSenha_exigeSenhaForte')
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente** o esboço: "Meu perfil", Nome e Telefone com um "Salvar", seções "E-mail" e "Senha" sob filete de tinta, e "Apagar conta" por último. O "Sair" sai daqui e fica só no menu. O botão de apagar é fantasma, com texto na cor de erro só no estado de confirmação. Atualize o visual de `trocar-senha`.
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(web): meu perfil com telefone, troca de e-mail e apagar conta`.

### Task 14: Termo e privacidade

**Files:**
- Create: `features/institucional/privacidade/privacidade.component.{ts,html,scss,spec.ts}`, `features/institucional/privacidade/responsavel-dados.ts`
- Modify: `app.routes.ts`, `core/layout/footer/footer.component.{html,ts,spec.ts}`

**Interfaces:**
- Produces: rota pública `/privacidade`; `RESPONSAVEL_DADOS = { nome: 'Bíblia Teológica', email: 'privacidade@example.com' }`, com comentário `// PROVISÓRIO: trocar por dados reais antes de abrir ao público`; `VERSAO_TERMO = '2026-11-05'`, igual a `TermoPrivacidade.VersaoAtual`.

- [ ] **Step 1: Escreva os testes que falham**

```ts
it('privacidade_mostraResponsavelEContato')
it('privacidade_listaDadosEDireitos')    // nome, e-mail, telefone, Gmail, 7 dias, apagamento, autoridade de controle
it('privacidade_mostraVersaoEData')
it('privacidade_temFundoEAlturaMinima')
it('rodape_linkaPrivacidade')
```

- [ ] **Step 2: Rode e veja falhar.** Esperado: FAIL.
- [ ] **Step 3: Implemente** o conteúdo da seção "Termo e privacidade" do spec, em texto de leitura de 40rem, sem innerHTML. O link de contato é `mailto:`. Diga que apagar conta está em "Meu perfil".
- [ ] **Step 4: Rode e veja passar.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(web): página de privacidade e termo`.

## Fechamento

### Task 15: Issue #31 e verificação final

**Files:**
- Test: `tests/BibliaTeologica.Api.Tests/Auth/AuthEndpointsTests.cs`, `frontend/.../core/interceptors` (spec aplicável)

- [ ] **Step 1: Escreva o teste de regressão** `Login_Responde200_QuandoCookiesVencidosPresentes`: cliente com `bt_access`, `bt_refresh` e `bt_csrf` antigos faz login e recebe o desafio na primeira tentativa.
- [ ] **Step 2: Tente reproduzir por até 30 minutos** na stack Docker, com cookies de uma sessão vencida, registrando o status da primeira resposta.
- [ ] **Step 3: Conclua a issue.** Reproduziu: corrija a causa e feche a issue no PR. Não reproduziu: comente na issue o que foi apurado e deixe-a aberta. Isso não bloqueia o merge.
- [ ] **Step 4: Rode tudo.** `dotnet test` em container, `npm test`, `npm run build`, `npx tsc --noEmit -p tsconfig.app.json`. Esperado: tudo verde.
- [ ] **Step 5: Confira visualmente** celular e desktop de todas as telas novas, incluindo o espaçamento dos campos, com a stack em Docker.
- [ ] **Step 6: Abra o PR**, listando as mudanças de contrato da tabela do spec, os riscos e o que o utilizador precisa testar. Não faça merge: o diff e o teste na stack Docker são do utilizador.
- [ ] **Step 7: Estime o esforço gasto** e sugira onde reduzir, como pede o `CLAUDE.md`.
