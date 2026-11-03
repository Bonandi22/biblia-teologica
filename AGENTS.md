# AGENTS.md

Guia para o Codex neste repositório.

## Project context

Bíblia Teológica — portal de conhecimento teológico (livros da Bíblia, estudos
long-form, vídeos, teologia sistemática). Projeto híbrido: peça de portfólio de
engenharia .NET e site pessoal leve de manter. Primeira vertical entregue: o
fichário dos 66 livros do cânon protestante.

Roadmap: docs/ROADMAP.md. Specs e planos: docs/superpowers/.

## Guardrails (regras permanentes)

Aplicam-se a qualquer tarefa. Só o utilizador as suspende, explicitamente, para uma tarefa concreta.

### Qualidade

Nenhuma mudança está pronta sem testes a passar e sem o utilizador ter revisto o diff.

Antes de declarar terminado:
- apresentar o diff ou um resumo preciso das alterações;
- apresentar riscos ou possíveis impactos;
- para mudanças com comportamento em runtime, indicar que o teste manual local faz parte da validação final.

O teste manual local consiste em subir a stack em Docker e exercitar o fluxo relevante; essa etapa é executada pelo utilizador quando aplicável.

### Segurança

- Nunca escrever segredos em código, commits, migrações, logs ou output.
- Informação sensível vive em variáveis de ambiente ou em `appsettings.Development.json` (git-ignored).
- Verificar arquivos staged antes de qualquer commit.
- Nunca expor tokens, hashes, credenciais ou outros dados sensíveis em respostas de API.

### Custo

No fim de tarefas que envolvam loops, subagentes ou múltiplas iterações, estimar o esforço gasto e sugerir onde reduzir trabalho/custo quando houver oportunidade.

## Arquitetura

Monólito .NET 8 em Clean Architecture de 4 camadas:

1. Domain — entidades, enums e exceções de domínio.
2. Application — Application Services, DTOs e perfis AutoMapper.
3. Infrastructure — DbContext EF Core, repositórios, migrações e seed.
4. Api — controllers, Program.cs e middleware.

Fluxo de dependências:

`Domain -> Application -> Infrastructure -> Api`

Não introduzir microserviços nem mensageria sem autorização explícita do utilizador.

### Persistência

- EF Core + SQLite (arquivo em volume Docker).
- Não colocar detalhes específicos do provider fora da Infrastructure.

### Frontend

- Angular 22 standalone + signals.
- CSR, sem SSR.
- Features lazy-loaded em `features/`.
- URL da API em `environment.ts` (`/api`).

## Princípios de Engenharia

- Regras de negócio vivem em Domain/Application, não em Infrastructure/Controllers.
- Entidades protegem os próprios invariantes; evitar setters públicos soltos.
- Application Services orquestram casos de uso.
- Repositórios apenas persistem/recuperam dados.
- Controllers apenas validam formato/contrato e delegam.
- DTOs são o contrato da API, nunca o modelo de domínio.
- AutoMapper mapeia; não decide regras de negócio.
- Aplicar SRP por classe.
- Depender de abstrações.
- Preferir composição a herança quando não existir uma relação de domínio clara.

## Convenções de código

- Usar `DateTime.UtcNow`, nunca `DateTime.Now`, em domínio/aplicação.
- Async de ponta a ponta; nunca `.Result` ou `.Wait()`.
- Propagar `CancellationToken`.
- Nunca engolir `Exception`; usar `ILogger` antes de tratar/relançar.
- EF Core: consultas somente leitura usam `AsNoTracking()` quando apropriado.
- Evitar N+1 e `Include` desnecessário.
- DTO de escrita nunca deve ser mapeado diretamente para a entidade quando houver campos de auditoria/propriedade que precisem ser protegidos.
- Central Package Management: versões apenas em `Directory.Packages.props`; `.csproj` sem `Version`.
- Testes: `Metodo_ResultadoEsperado_QuandoCondicao`.
- Backend: xUnit + Moq + AwesomeAssertions + EF Core Sqlite.
- Frontend: Vitest (`@angular/build:unit-test`, jsdom), não Karma.
- Subscrições RxJS em código novo: `takeUntilDestroyed(this.destroyRef)`.
- Nunca usar `innerHTML` com dados brutos.
- Nunca usar `bypassSecurityTrust*` em conteúdo de utilizador.
- Auth: cookies HttpOnly (`bt_access`/`bt_refresh`) + double-submit CSRF (`bt_csrf` + header `X-CSRF-Token`).
- Nunca guardar token em localStorage.
- Nunca devolver `ApplicationUser`, hash ou refresh token no corpo de uma resposta.
- Identity vive na Infrastructure; Application fala com Identity apenas através de abstrações como `IIdentityService`, `ITokenService`, etc.

## Comandos essenciais

### Backend

```bash
dotnet build BibliaTeologica.sln
dotnet test BibliaTeologica.sln
dotnet ef migrations add <Nome> --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
dotnet ef database update --project src/BibliaTeologica.Infrastructure --startup-project src/BibliaTeologica.Api
```

### Frontend

```bash
cd frontend/biblia-teologica-web
npm ci
npm start
npm run build
npm test
npx tsc --noEmit -p tsconfig.app.json
```

`npm test` usa Vitest e deve executar uma passagem; `CI=true` força one-shot quando necessário.

### Docker

```bash
docker compose up --build
docker compose down -v
```

- API: `:5080`
- Web: `:4300`
- Docker precisa de `.env`; copiar de `.env.example` e fornecer `BT_JWT_KEY` e `BT_ADMIN_*`.
- `docker compose down -v` zera o volume do SQLite.

Código 2FA em dev nos logs:

```bash
docker compose logs api | grep EMAIL
```

Swagger em dev:

`http://localhost:5080/swagger`

## Git

- `main` executa CI (`ci-cd.yml`: build + test em PR, push de imagens para GHCR em main).
- Existe code review automático em PR e automações de CI associadas ao repositório.
- Convenção: Issue -> branch `feature/BT-<n>-nome` -> PR -> checks -> merge squash.
- Sem branch protection: tratar os checks como gate obrigatório mesmo assim.
- Só fazer push direto para `main` com pedido explícito do utilizador.
- Nunca fazer merge com checks vermelhos.
- Não alterar arquivos não relacionados à tarefa.
- Antes de finalizar uma tarefa, revisar o diff e informar alterações relevantes e riscos.

## Regras de ouro — nunca fazer

- Nunca adicionar endpoint de escrita sem `[Authorize(Roles=...)]` ou comentário explícito justificando por que é público.
- Guards do Angular são apenas UX; autorização real deve estar no backend.
- Nunca usar `_mapper.Map(dto, entity)` em Update sem confirmar que campos de auditoria/propriedade estão protegidos/ignorados.
- Nunca retornar entidade crua nem campos sensíveis (hash, token, e-mail de terceiros) numa resposta de API.
- Nunca usar `AllowAnyOrigin` no CORS; nunca combinar `AllowAnyOrigin` com `AllowCredentials`.
- Nunca deixar mensagens de erro vazarem stack trace ou detalhes internos em Production.
- Nunca fazer upload sem validar content-type, magic bytes, tamanho, nome seguro e destino fora do webroot.
- Nunca commitar `appsettings.Development.json`, `.env` ou qualquer segredo em arquivo versionado.
- Nunca criar endpoint/service novo sem teste cobrindo caminho feliz e rejeição.
- Nunca fazer `git push` direto para `main` sem pedido explícito.
- Nunca declarar a tarefa concluída sem testes a passar e sem o utilizador ter revisto o diff.

## Como o Codex deve trabalhar

Para tarefas não triviais:

1. Ler `AGENTS.md` e a documentação relevante antes de alterar código.
2. Inspecionar a implementação existente e procurar padrões já utilizados no projeto.
3. Antes de criar uma nova abstração, verificar se já existe uma solução equivalente.
4. Explicar brevemente o plano quando a tarefa envolver várias mudanças.
5. Fazer a menor alteração necessária para cumprir o objetivo.
6. Não refatorar partes não relacionadas só porque podem ser melhoradas.
7. Executar build e testes relevantes após a implementação.
8. Revisar o diff procurando regressões, alterações acidentais e violações das regras deste arquivo.
9. Informar claramente o que foi alterado, quais testes foram executados e quais etapas manuais ainda precisam ser feitas pelo utilizador.
10. Se houver ambiguidade que possa causar uma mudança destrutiva ou incompatível, parar e pedir confirmação em vez de assumir.

## Definition of Done

Uma tarefa só está pronta quando:

- a implementação atende ao requisito;
- a arquitetura existente foi respeitada;
- testes foram criados/atualizados quando apropriado;
- a solução compila;
- os testes relevantes passam;
- não há alterações não relacionadas;
- o diff foi revisado;
- riscos e breaking changes foram comunicados;
- o teste manual local foi identificado quando houver mudança de comportamento em runtime.
