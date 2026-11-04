# Bíblia Teológica

Plataforma de estudo bíblico que mostra cada livro situado: quem escreveu, para
quem e por quê, o que acontecia em Israel e no mundo, e quais profecias e promessas
o livro contém. Projeto híbrido: peça de portfólio de engenharia .NET e site
pessoal, leve de manter.

Já estão em `main` o **fichário dos 66 livros do cânon protestante** e a base do
**Panorama Bíblico**: conteúdo editorial em arquivos, com notas e bibliografia, lido
pela API. O conteúdo do primeiro panorama, Isaías, ainda está sendo escrito.

[![CI/CD](https://github.com/Bonandi22/biblia-teologica/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Bonandi22/biblia-teologica/actions/workflows/ci-cd.yml)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

---

## Estado

| Marco | Escopo | Estado |
|---|---|---|
| **M1** | Site público de Livros: camadas .NET, EF Core/SQLite, Angular 22, Docker, CI | ✅ entregue |
| **M2a** | Autenticação (Identity + JWT em cookies, refresh rotativo, 2FA por e-mail), `/conta` e registro de Membro | ✅ entregue |
| **M2c** | Casca do portal: cabeçalho, rodapé e página inicial | ✅ entregue |
| **Repaginação** | Todo o frontend no visual de Bíblia de estudo, sem Angular Material ([`DESIGN.md`](DESIGN.md)) | ✅ entregue |
| **Panorama Bíblico** | Conteúdo em arquivos, servido em `/biblia/:slug` e `/estudos` | 🚧 base técnica entregue; conteúdo de Isaías pendente ([#28](../../issues/28)) |
| M2b | Área `/admin` e CRUD de Livros com upload de capa | ⏸️ em espera: o conteúdo nasce em arquivos |
| M3.5 | Vídeos | planejado |
| M4 | Confirmação de e-mail, recuperação de senha e newsletter | planejado |
| M5+ | Doutrinas, Glossário, Cronologia, Teologia Sistemática e deploy real | planejado |

Roadmap completo: [`docs/ROADMAP.md`](docs/ROADMAP.md). Produto e público:
[`PRODUCT.md`](PRODUCT.md).

## Arquitetura

Monólito .NET 8 em Clean Architecture de 4 camadas, sem microsserviços e sem
mensageria.

```
  Angular 22 (standalone + signals, CSR)
        │  HTTP  /api
        ▼
  ┌─────────────────────────────────────────────┐
  │  Api            controllers, Program.cs,     │
  │                 middleware (erro, segurança, │
  │                 rate limiting), CORS, health │
  ├─────────────────────────────────────────────┤
  │  Application    Application Services, DTOs,  │
  │                 mapeamento, interfaces,      │
  │                 importação de conteúdo       │
  ├─────────────────────────────────────────────┤
  │  Domain         entidades (invariantes),     │
  │                 enums, exceções de domínio   │
  ├─────────────────────────────────────────────┤
  │  Infrastructure EF Core DbContext, repos,    │
  │                 migrações, seed (66 livros), │
  │                 leitura de content/          │
  └─────────────────────────────────────────────┘
        │
   SQLite (arquivo em volume Docker)
```

A regra de negócio vive em Domain e Application. As entidades protegem os próprios
invariantes (sem setters públicos soltos), e os controllers só validam o formato e
delegam. DTOs são o contrato da API: entidades nunca saem em uma resposta.
Convenções e guardrails completos: [`CLAUDE.md`](CLAUDE.md).

## Stack

| Camada | Tecnologias |
|---|---|
| **Backend** | .NET 8 · ASP.NET Core · Entity Framework Core 8 (SQLite) · YamlDotNet |
| **Frontend** | Angular 22 · standalone components · signals · SCSS · marked + DOMPurify · CSR (sem SSR) |
| **Testes** | xUnit · Moq · AwesomeAssertions · EF Core Sqlite · Vitest (frontend) |
| **Infra** | Docker · Docker Compose · nginx · GitHub Actions (GHCR) |

## Quick start (Docker)

O `docker compose up` precisa de um `.env` na raiz:

```bash
cp .env.example .env            # depois preencha os valores
docker compose up --build       # api :5080 · web :4300
```

- `BT_JWT_KEY`: chave de assinatura do JWT (ex.: `openssl rand -base64 48`)
- `BT_ADMIN_EMAIL` / `BT_ADMIN_SENHA`: o admin criado na inicialização, com troca de
  senha obrigatória no primeiro login

Endereços:

- Site: <http://localhost:4300>
- API: <http://localhost:5080/api/livros>
- Swagger (dev): <http://localhost:5080/swagger>
- Health: <http://localhost:5080/health>

O seed dos 66 livros e a importação de `content/` rodam na inicialização da API. O
SQLite fica no volume `biblia-data`.

```bash
docker compose down             # para e preserva o volume
docker compose down -v          # zera o volume do SQLite
```

### Autenticação

Login em <http://localhost:4300/entrar>. Em desenvolvimento, o código 2FA aparece
nos logs da API, na linha seguinte à do assunto:

```bash
docker compose logs api | grep -A1 EMAIL
```

`/conta` traz o perfil (nome e e-mail) e a troca de senha, para qualquer usuário
autenticado. `/registar` cria uma conta de Membro.

## Conteúdo

O conteúdo editorial nasce em arquivos versionados e é importado para o banco na
inicialização da API.

```
content/
  bibliografia.yaml   ← obras citadas
  livros/*.yaml       ← panorama de cada livro
  estudos/*.md        ← estudos em Markdown
```

- Hoje o repositório só tem `bibliografia.yaml`. As pastas `livros/` e `estudos/`
  surgem com o primeiro conteúdo aprovado.
- Arquivos marcados como rascunho nunca entram em Production. Em desenvolvimento,
  `docker-compose.override.yml` os inclui com `Conteudo__IncluirRascunhos`.
- `Conteudo__Pasta` aponta para outra pasta, útil para testar sem tocar em `content/`.
- Um arquivo publicável inválido impede a API de subir: o erro aparece na
  inicialização, e não para quem está lendo.

## Desenvolvimento local (sem Docker)

<details>
<summary><strong>Pré-requisitos e passos</strong></summary>

**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) · [Node.js 24+](https://nodejs.org/)

**Backend**

```bash
dotnet build BibliaTeologica.sln
dotnet ef database update --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
dotnet run --project src/BibliaTeologica.Api --urls http://localhost:5080      # /swagger
```

A porta 5080 é a que o proxy do frontend espera. Use `appsettings.Development.json`
(ignorado pelo git) para overrides locais: connection string, origens de CORS e os
segredos de autenticação.

```json
{
  "ConnectionStrings": { "Default": "Data Source=./biblia.dev.db" },
  "Jwt": { "SigningKey": "GERE-UMA-CHAVE-ALEATORIA-COM-64-CHARS-openssl-rand-base64-48" },
  "AdminSeed": [ { "Email": "admin@local", "SenhaInicial": "Admin!Local1" } ]
}
```

**Frontend**

```bash
cd frontend/biblia-teologica-web
npm ci
npm start        # http://localhost:4300  (proxy /api -> :5080)
```

</details>

## Testes

```bash
# Backend
dotnet test BibliaTeologica.sln
dotnet test tests/BibliaTeologica.Domain.Tests --filter "FullyQualifiedName~SlugTests"

# Frontend (Vitest)
cd frontend/biblia-teologica-web
npm test
npx tsc --noEmit -p tsconfig.app.json
```

## Estrutura do repositório

```
BibliaTeologica/
  src/
    BibliaTeologica.Domain/          ← entidades, enums, exceções de domínio
    BibliaTeologica.Application/     ← Application Services, DTOs, mapeamento
                                       (Auth, Conta, Conteudo)
    BibliaTeologica.Infrastructure/  ← DbContext, repositórios, migrações, seed
                                       Identity/ (JWT, refresh, 2FA), Email/, Conteudo/
    BibliaTeologica.Api/             ← controllers, Program.cs, middleware, Dockerfile
                                       Auth/ (cookies, CSRF, usuário atual)
  tests/
    BibliaTeologica.{Domain,Application,Infrastructure,Api}.Tests/
  frontend/biblia-teologica-web/     ← Angular 22: features lazy em features/,
                                       primitivos em shared/ui, interceptors e guards
                                       em core/, nginx Dockerfile
  content/                           ← conteúdo editorial (ver "Conteúdo")
  docs/
    ROADMAP.md                       ← verticais e marcos
    superpowers/                     ← specs e planos de implementação
    prototipo-ficha-66-livros.html   ← protótipo visual antigo (referência)
  .github/
    workflows/ci-cd.yml              ← build+test em PRs · push de imagens para GHCR em main
    workflows/claude-code-review.yml ← code review automático por PR
    workflows/claude.yml             ← responde a menções @claude em issues/PRs
    ISSUE_TEMPLATE/ · pull_request_template.md · dependabot.yml
  docker-compose.yml                 ← api + web (+ override de dev, exemplo de prod)
  Directory.Packages.props           ← Central Package Management (versões só aqui)
  PRODUCT.md · DESIGN.md             ← produto e sistema visual
  CLAUDE.md · AGENTS.md              ← guias para agentes de código
```

## CI/CD

`.github/workflows/ci-cd.yml`:

- **Em Pull Requests para `main`:** restore, build e testes (backend `dotnet test`, frontend `tsc`, build e `npm test`)
- **Em push para `main`:** build e push de duas imagens Docker para o GitHub Container Registry: `biblia-teologica-api` e `biblia-teologica-web`

Cada PR também passa pelo **code review automático** (`claude-code-review.yml`). Não
faça merge com checks vermelhos. Em PRs empilhados, o CI só roda quando a base é
`main`.

## Contribuir

Fluxo: **Issue → branch `feature/BT-<n>-descricao` → commits → PR → checks → merge (squash)**.
Push direto para `main` só com pedido explícito.

- Abra uma [issue](../../issues/new/choose) pelos templates (bug ou funcionalidade)
- Abra o PR com `Closes #<n>`: o [template de PR](.github/pull_request_template.md) traz o checklist das regras de ouro
- Convenções, arquitetura e guardrails: [`CLAUDE.md`](CLAUDE.md)
- Vulnerabilidades de segurança **não** vão em issue pública: veja [`SECURITY.md`](SECURITY.md)

## Deploy

Ainda não está em produção. Antes de abrir ao público faltam:

- envio real de e-mail ([#34](../../issues/34)), hoje o código 2FA só sai nos logs;
- conteúdo aprovado e confirmação escrita da SBB sobre os links para a ARC;
- VPS, domínio, HTTPS e `.env` de produção (M5+ no roadmap).
