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

    docker compose up --build      # api :5080 + web :4300
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
