# Bíblia Teológica

Portal de conhecimento teológico — livros da Bíblia, estudos long-form, vídeos e
teologia sistemática. Projeto híbrido: peça de portfólio de engenharia .NET e
site pessoal, leve de manter.

A primeira vertical entregue é o **fichário dos 66 livros do cânon protestante**:
ficha estruturada por livro (quem escreveu, porquê, para quem, estrutura, tema,
ideias-chave, versículo de referência), navegável e agrupada por seção canônica.

[![CI/CD](https://github.com/Bonandi22/biblia-teologica/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Bonandi22/biblia-teologica/actions/workflows/ci-cd.yml)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

---

## Estado

| Marco | Âmbito | Estado |
|---|---|---|
| **M1** | Site público de Livros — camadas .NET, EF Core/SQLite, Angular 22, Docker, CI | ✅ entregue |
| M2 | ASP.NET Core Identity + JWT + área `/admin` + CRUD de Livros + upload | planeado |
| M3 | Estudos (Markdown, sanitização) · M3.5 Vídeos | planeado |
| M4+ | Cadastro público, newsletter, Doutrinas, Glossário, Cronologia, deploy real | planeado |

Roadmap completo: [`docs/ROADMAP.md`](docs/ROADMAP.md).

## Arquitetura

Monólito .NET 8 em Clean Architecture de 4 camadas. Sem microserviços, sem
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
  │                 mapeamento, interfaces       │
  ├─────────────────────────────────────────────┤
  │  Domain         entidades (invariantes),     │
  │                 enums, exceções de domínio   │
  ├─────────────────────────────────────────────┤
  │  Infrastructure EF Core DbContext, repos,    │
  │                 migrações, seed (66 livros)  │
  └─────────────────────────────────────────────┘
        │
   SQLite (ficheiro em volume Docker)
```

A regra de negócio vive no Domain/Application. As entidades protegem os próprios
invariantes (sem setters públicos soltos); os controllers só validam formato e
delegam. DTOs são o contrato da API — entidades nunca saem numa resposta.
Convenções e guardrails completos: [`CLAUDE.md`](CLAUDE.md).

## Stack

| Camada | Tecnologias |
|---|---|
| **Backend** | .NET 8 · ASP.NET Core · Entity Framework Core 8 (SQLite) |
| **Frontend** | Angular 22 · standalone components · signals · SCSS · CSR (sem SSR) |
| **Testes** | xUnit · Moq · AwesomeAssertions · EF Core Sqlite · Vitest (frontend) |
| **Infra** | Docker · Docker Compose · nginx · GitHub Actions (GHCR) |

## Quick start (Docker)

```bash
docker compose up --build       # api :5080 · web :4300
```

- Site: <http://localhost:4300>
- API: <http://localhost:5080/api/livros>
- Swagger (dev): <http://localhost:5080/swagger>
- Health: <http://localhost:5080/health>

O seed dos 66 livros corre no arranque da API (idempotente). O SQLite vive no
volume `biblia-data`.

```bash
docker compose down             # para, preserva o volume
docker compose down -v          # zera o volume do SQLite
```

## Desenvolvimento local (sem Docker)

<details>
<summary><strong>Pré-requisitos e passos</strong></summary>

**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) · [Node.js 24+](https://nodejs.org/)

**Backend**

```bash
dotnet build BibliaTeologica.sln
dotnet ef database update --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
dotnet run --project src/BibliaTeologica.Api      # https://localhost:5080/swagger
```

`appsettings.Development.json` (git-ignored) para overrides locais — connection
string, origens de CORS, etc.

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
    BibliaTeologica.Infrastructure/  ← DbContext, repositórios, migrações, seed
    BibliaTeologica.Api/             ← controllers, Program.cs, middleware, Dockerfile
  tests/
    BibliaTeologica.{Domain,Application,Infrastructure,Api}.Tests/
  frontend/biblia-teologica-web/     ← Angular 22 (feature `biblia` lazy, nginx Dockerfile)
  docs/
    ROADMAP.md                       ← verticais e marcos
    superpowers/                     ← specs e planos de implementação
    prototipo-ficha-66-livros.html   ← protótipo visual (referência do design)
  .github/
    workflows/ci-cd.yml              ← build+test em PRs · push de imagens para GHCR em main
    workflows/claude-code-review.yml ← code review automático por PR
    workflows/claude.yml             ← responde a menções @claude em issues/PRs
    ISSUE_TEMPLATE/ · pull_request_template.md · dependabot.yml
  docker-compose.yml                 ← api + web (+ override de dev, exemplo de prod)
  Directory.Packages.props           ← Central Package Management (versões só aqui)
```

## CI/CD

`.github/workflows/ci-cd.yml`:

- **Em Pull Requests** — restore + build + testes (backend `dotnet test`, frontend `tsc` + build + `npm test`)
- **Em push para `main`** — build e push de duas imagens Docker para o GitHub Container Registry: `biblia-teologica-api` e `biblia-teologica-web`

Cada PR corre ainda o **code review automático** (`claude-code-review.yml`). Não
fazer merge com checks vermelhos.

## Contribuir

Fluxo: **Issue → branch `feature/BT-<n>-descricao` → commits → PR → checks → merge (squash)**.
Push direto para `main` só com pedido explícito.

- Abrir uma [issue](../../issues/new/choose) pelos templates (bug / funcionalidade)
- Abrir o PR com `Closes #<n>` — o [template de PR](.github/pull_request_template.md) traz o checklist das regras de ouro
- Convenções, arquitetura e guardrails: [`CLAUDE.md`](CLAUDE.md)
- Vulnerabilidades de segurança: **não** por issue pública — ver [`SECURITY.md`](SECURITY.md)

## Deploy

Ainda não está em produção. O deploy real (VPS, domínio, HTTPS, `.env` de
produção) é um item de M5+ no roadmap.
