# Repaginação Bíblia de Estudo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar as funções decididas do Panorama e dar a todas as telas o mundo visual Bíblia de Estudo, com Isaías como livro padrão.

**Architecture:** Primeiro muda o formato do conteúdo e a API ainda não publicada; depois a fundação visual e a página do livro, que o utilizador aprova; só então as demais telas herdam o padrão. Componentes próprios substituem o Angular Material.

**Tech Stack:** .NET 8, EF Core + SQLite, xUnit + Moq + AwesomeAssertions; Angular 22 standalone + signals, SCSS, Vitest.

**Spec:** `docs/superpowers/specs/2026-11-03-repaginacao-biblia-de-estudo-design.md`. Contexto: `PRODUCT.md` e o surface brief em `.impeccable/surfaces/`.

## Global Constraints

- `/api/livros` e `/api/livros/{slug}` não mudam. Rotas e URLs atuais não mudam.
- Textos visíveis em pt-BR. Nada de conteúdo fictício em commit; só o utilizador marca conteúdo como `revisado`.
- Neutralidade: nenhuma tela ou campo indica categoria ou cumprimento de profecia.
- Rubrica `#9E1B1B` só para referência, nota e lugar atual. Sem cartões, sombras, gradientes, ornamentos religiosos, rótulo em caixa alta, eyebrow ou monoespaçada.
- Toda página de topo tem fundo e altura mínima próprios e conferência visual em 375, 1366 e 1920px.
- Convenções do `CLAUDE.md`: `takeUntilDestroyed`, sem `innerHTML` de dado bruto, teste com caminho feliz e rejeição, endpoint público de leitura com comentário.
- Conferir staged antes de cada commit; não incluir `.agents/`, `.aider-desk/`, `.github/agents|hooks|skills/` nem `skills-lock.json`.
- Commits terminam com `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`. Sem push nem PR até o utilizador pedir.
- Antes de editar telas: `frontend-design` e `.claude/skills/impeccable/reference/craft-floor.md`.
- Máximo de 3 rondas de revisão por task.

## Review Focus

- Referência que não pode ser lida, com apelido ("Atos", "Salmo", "Cantares") ou entre capítulos ("Isaías 52:13–53:12"): nunca gera link quebrado. Teste na Task 3.
- URL direta `/biblia/isaias/promessas` em livro sem promessas: volta ao Panorama, como as outras abas. Teste na Task 2.
- Nota cuja chamada está só no `saberMais` fechado, ou nota sem chamada: continua visível, sem sumir da coluna. Teste na Task 5.
- Livro sem panorama: abre a ficha na mesma moldura, sem abas nem aviso de erro. Teste na Task 6.
- Falha de `/api/livros` ou `/api/panoramas/livros`: a moldura e o índice mostram um estado claro; o índice continua sem marca, sem erro. Teste na Task 6.

---

### Task 1: Formato do conteúdo e API do Panorama

**Files:**
- Modify: `.gitignore` (acrescentar `.impeccable/mocks/`, `.impeccable/review/`, `.impeccable/config.local.json`, `.impeccable/live/`, `.playwright-mcp/`)
- Modify: `src/BibliaTeologica.Application/Conteudo/Registros.cs`, `ValidadorConteudo.cs`, `ConstrutorDocumentos.cs`, `Dtos/ConteudoDtos.cs`
- Modify: `tests/BibliaTeologica.Application.Tests/Conteudo/ValidadorConteudoTests.cs`, `ConstrutorDocumentosTests.cs`; `tests/BibliaTeologica.Infrastructure.Tests/Conteudo/FonteConteudoArquivosTests.cs`, `ImportacaoConteudoTests.cs`; `tests/BibliaTeologica.Api.Tests/PanoramaEndpointsTests.cs`
- Modify: `.superpowers/sdd/2026-11-02-panorama-biblico/formato.md` (git-ignored; manter atualizado)

**Interfaces:**
- Produces, em `ConteudoDtos.cs`:
  - `record PerfilItemDto(string Rotulo, string Texto, IReadOnlyList<string> Referencias)`
  - `record ApontamentoDto(string Referencia, string Anuncia, IReadOnlyList<NotaDto> Notas)`
  - `record LivroPanoramaDto(string LivroSlug, string Resumo, IReadOnlyList<PerfilItemDto> Perfil, BlocoDto Panorama, BlocoDto Contexto, BlocoDto Mundo, IReadOnlyList<ApontamentoDto> Profecias, IReadOnlyList<ApontamentoDto> Promessas, IReadOnlyList<CitacaoDto> Citacoes)`
- Em `Registros.cs`: `RegistroPerfilItem { Rotulo, Texto, Referencias }`, `RegistroApontamento { Referencia, Anuncia, Notas }`. `RegistroLivro` perde `Quem`, `ParaQuem`, `Porque` e ganha `Perfil`, `Promessas`; `Profecias` passa a `List<RegistroApontamento>`. Sai `RegistroProfecia`.

- [ ] **Step 1: Escrever os testes do validador que falham**

Em `ValidadorConteudoTests.cs`, ajustar os dados base ao novo formato, remover testes de categoria e cumprimento e adicionar:
`Validar_RetornaErro_QuandoPerfilVazio`, `Validar_RetornaErro_QuandoItemDoPerfilSemRotuloOuTexto`, `Validar_RetornaErro_QuandoReferenciaDoPerfilInvalida`, `Validar_RetornaErro_QuandoTextoDoPerfilTemMarcadorDeNota`, `Validar_RetornaErro_QuandoPromessaSemAnunciaOuReferenciaInvalida`, `Validar_AceitaCitacao_QuandoCitadoSoNoAntigoTestamento`, `Validar_RetornaErro_QuandoCitadoEmVazio`, `Validar_RetornaErro_QuandoCitadoEmApontaParaOProprioLivro`.

- [ ] **Step 2: Rodar e ver falhar**

Run: `dotnet test BibliaTeologica.sln --filter ValidadorConteudo`
Expected: erro de compilação ou FAIL nos testes novos.

- [ ] **Step 3: Implementar registros, DTOs, validador e construtor**

Aplicar as interfaces. No validador, remover `Categorias` e `TemLivroDoNovoTestamento`; manter a regra 6 (aspas exigem origem) no perfil, com referências do item como origem; `Perfil` não aceita `[^n]`, porque não tem lista de notas. Profecias e promessas compartilham `ValidarApontamento`. O construtor preserva a ordem do arquivo. Não é preciso migração: o JSON é regravado na importação.

- [ ] **Step 4: Ajustar os demais testes e rodar tudo**

Atualizar os YAML de teste e o caso feliz do endpoint para `perfil` e `promessas`. Afirmar que o JSON do endpoint não contém `categoria`, `cumprimento` nem `fatoHistorico`.
Run: `dotnet test BibliaTeologica.sln`
Expected: todos passam.

- [ ] **Step 5: Atualizar `formato.md` e commitar**

Registrar os campos novos e as regras removidas. Commit: `feat(panorama): perfil do autor, promessas e profecias sem categoria`.

### Task 2: Abas e telas das funções novas

**Files:**
- Modify: `frontend/biblia-teologica-web/src/app/core/models/conteudo.model.ts`
- Modify: `features/biblia/livro-pagina/livro-pagina.component.{ts,html,spec.ts}`
- Rename: `features/biblia/livro-pagina/profecias/` para `apontamentos/` (`ApontamentosComponent`, seletor `bt-apontamentos`)
- Modify: `features/biblia/livro-pagina/citacoes/citacoes.component.{ts,html,spec.ts}` se o texto citar o Novo Testamento

**Interfaces:**
- Consumes: JSON da Task 1.
- Produces: `interface PerfilItem { rotulo: string; texto: string; referencias: string[] }`, `interface Apontamento { referencia: string; anuncia: string; notas: Nota[] }`; `LivroPanorama` sem `quem`, `paraQuem`, `porque`, com `perfil`, `profecias: Apontamento[]`, `promessas: Apontamento[]`. `ApontamentosComponent` com inputs `itens: Apontamento[]` e `prefixo: string`.

- [ ] **Step 1: Escrever os testes que falham**

`livro-pagina.component.spec.ts`: "mostra a aba Promessas quando há promessas"; "esconde a aba Promessas e volta ao panorama pela URL direta quando não há"; "mostra cada item do perfil com rótulo e texto". Spec de apontamentos: "lista na ordem recebida"; "não mostra categoria nem cumprimento"; "prefixos de profecias e promessas geram ids de nota distintos".

- [ ] **Step 2: Rodar e ver falhar**

Run: `cd frontend/biblia-teologica-web && npm test`
Expected: FAIL nos testes novos.

- [ ] **Step 3: Implementar**

`ChaveAba` ganha `promessas`, entre Profecias e Citações. Perfil renderizado como `<dl>`. Prefixos de nota: `pro-<i>` para profecias, `prm-<i>` para promessas.

- [ ] **Step 4: Rodar testes e tipos**

Run: `npm test && npx tsc --noEmit -p tsconfig.app.json`
Expected: tudo passa.

- [ ] **Step 5: Commit**

`feat(panorama): abas de promessas e perfil do autor, sem selos nas profecias`

### Task 3: Links para a ARC

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/core/biblia/arc.ts`, `arc.spec.ts`
- Create: `src/app/shared/ui/referencia/referencia.component.ts`, `.scss`, `.spec.ts`

**Interfaces:**
- Produces, em `arc.ts`: `LIVROS_ARC: Record<string, { sigla: string; capitulos: number }>`, chaves iguais a `SlugsCanonicos` de `ReferenciaBiblica.cs`; `slugDaReferencia(ref: string): string | null`; `urlCapitulo(slug: string, capitulo: number): string | null`; `urlReferencia(ref: string): string | null`.
- Produces: `ReferenciaComponent`, seletor `bt-referencia`, input `texto: string`. Renderiza `<a target="_blank" rel="noopener noreferrer">` ou `<span>` quando a URL é `null`. O `aria-label` avisa que abre em nova aba.

- [ ] **Step 1: Conferir as 66 siglas no site**

Para cada livro, abrir `https://www.bibliaonline.com.br/arc/<sigla>/1` e registrar a sigla que responde com o livro certo. Conferir também `/arc/is/53/7` e `/arc/is/6/1-8`. Se o formato diferir do previsto, ajustar a URL e registrar no plano; se o site bloquear a conferência automática, parar e levar ao utilizador.

- [ ] **Step 2: Escrever os testes que falham**

`arc.spec.ts`:
```ts
expect(Object.keys(LIVROS_ARC)).toHaveLength(66);
expect(Object.values(LIVROS_ARC).reduce((s, l) => s + l.capitulos, 0)).toBe(1189);
expect(urlReferencia('Isaías 6:1–8')).toBe('https://www.bibliaonline.com.br/arc/is/6/1-8');
expect(urlReferencia('Isaías 53:7')).toBe('https://www.bibliaonline.com.br/arc/is/53/7');
expect(urlReferencia('Isaías 53')).toBe('https://www.bibliaonline.com.br/arc/is/53');
expect(urlReferencia('Isaías 52:13–53:12')).toBe('https://www.bibliaonline.com.br/arc/is/52/13');
expect(urlReferencia('Atos 8:32')).toContain('/8/32');
expect(urlReferencia('Salmo 23')).toContain('/23');
expect(urlReferencia('Cantares 2:1')).toContain('/2/1');
expect(urlReferencia('Isaías 67')).toBeNull();
expect(urlReferencia('Livro Inexistente 1:1')).toBeNull();
expect(urlCapitulo('isaias', 66)).toContain('/arc/is/66');
expect(urlCapitulo('isaias', 67)).toBeNull();
```
Spec do componente: link com `target`, `rel` e `aria-label`; `<span>` sem `href` para referência inválida.

- [ ] **Step 3: Rodar e ver falhar**

Run: `npm test`
Expected: FAIL.

- [ ] **Step 4: Implementar e aplicar**

O slug segue a mesma normalização e os mesmos apelidos de `ReferenciaBiblica.cs`. Usar `bt-referencia` em todo lugar que hoje mostra referência como texto: item de estudo, apontamentos, citações, perfil.

- [ ] **Step 5: Rodar testes e commitar**

Run: `npm test`
Expected: tudo passa. Commit: `feat(biblia): referencias abrem a ARC na Biblia Online`

### Task 4: Fundação visual e moldura comum

**Files:**
- Modify: `src/index.html`, `src/styles.scss`, `src/styles/_variables.scss`, `src/styles/_mixins.scss`
- Modify: `shared/ui/{botao,campo,campo-senha,alerta,cartao}/*.scss`, `shared/directives` de input, se houver estilo
- Modify: `core/layout/header/*`, `core/layout/footer/*`
- Create: `core/layout/lugar-atual.service.ts`

**Interfaces:**
- Produces: `LugarAtualService` com `readonly texto: Signal<string>`, `definir(texto: string): void`, `limpar(): void`. Tokens SCSS `$papel`, `$tinta`, `$rubrica`, `$filete`, `$capa`, `$sobre-capa`, `$fonte-texto`, `$fonte-corrente`, `$coluna-leitura: 42rem`, `$coluna-referencias: 18rem`. Os aliases antigos continuam apontando para os novos até a Task 9.

- [ ] **Step 1: Escrever os testes que falham**

Header: "mostra Livros e Estudos, sem Notícias nem Curiosidades"; "mostra o lugar atual definido no serviço"; "usa Minha conta e Criar conta". Footer: "sem formulário de newsletter nem redes"; "tem links para Livros e Estudos e a nota sobre a ARC". Remover testes da newsletter.

- [ ] **Step 2: Rodar e ver falhar**

Run: `npm test`
Expected: FAIL.

- [ ] **Step 3: Implementar**

Valores conforme a spec 4.1. Fontes: Gelasio (400, 600, 700, itálico 400) e Alegreya Sans SC (500, 700), mantendo o domínio já permitido pela CSP. Remover Space Grotesk, Source Serif, IBM Plex Mono do `index.html`; Material Icons sai só na Task 8. Tematizar `::selection`, sublinhado, foco e barra de rolagem. Header conforme spec 4.3 e o esboço aprovado `.impeccable/mocks/decision/model-pick.html`; no celular, menu recolhido com a marca visível. Texto de rodapé: "As referências bíblicas abrem a tradução Almeida Revista e Corrigida na Bíblia Online."

- [ ] **Step 4: Rodar testes, build e conferir visualmente**

Run: `npm test && npm run build`
Expected: tudo passa. Capturar header e rodapé em 375 e 1366px. O contraste de `#E9E7DD` sobre `#2A3550` precisa ser ao menos 4.5:1.

- [ ] **Step 5: Commit**

`feat(ui): fundacao visual Biblia de Estudo, titulo corrente e rodape`

### Task 5: Página do livro e coluna de referências (Isaías, padrão)

**Files:**
- Create: `features/biblia/livro-moldura/livro-moldura.component.{ts,html,scss,spec.ts}`
- Create: `shared/ui/linha-estudo/linha-estudo.component.{ts,scss,spec.ts}`
- Create: `shared/ui/texto-notas/notas-do-texto.ts`, `.spec.ts`
- Modify: `features/biblia/livro-pagina/*`, `features/biblia/livro-pagina/apontamentos/*`, `citacoes/*`
- Modify: `shared/ui/bloco-conteudo/*`, `item-estudo/*`, `bibliografia/*`, `texto-notas/*`

**Interfaces:**
- Consumes: `LugarAtualService`, `urlCapitulo`, `bt-referencia`, `idNota`.
- Produces: `LivroMolduraComponent` (`bt-livro-moldura`), inputs `slug`, `nome`, `resumo: string | null`, conteúdo projetado. Lista os livros da mesma `secaoRotulo` a partir de `LivroStore`, com capítulos abaixo do título.
- Produces: `LinhaEstudoComponent` (`bt-linha-estudo`), grade com slot de texto e slot `[referencias]`; inputs `referencias: string[]`, `notas: Nota[]`, `prefixo`.
- Produces: `notasDoTexto(texto: string, notas: Nota[]): Nota[]`, com as notas cujo marcador aparece no texto, na ordem do texto.

- [ ] **Step 1: Escrever os testes que falham**

- `notas-do-texto.spec.ts`: devolve na ordem do texto; ignora número sem nota; texto sem marcador devolve vazio.
- `linha-estudo`: "mostra referências e notas na coluna"; "`id` de nota preservado para âncora"; "focar a chamada marca a nota com `is-destaque`, e focar a nota marca a chamada".
- `bloco-conteudo`: "nota usada só no saberMais fechado continua na linha do item"; "nota sem marcador aparece na linha final".
- `livro-moldura`: "lista só os livros da divisão com o atual em `aria-current`"; "capítulos são links da ARC em nova aba"; "define e limpa o lugar atual"; "lugar atual é 'Isaías 1–66'".

- [ ] **Step 2: Rodar e ver falhar**

Run: `npm test`
Expected: FAIL.

- [ ] **Step 3: Implementar**

Grade de duas colunas, `minmax(0, $coluna-leitura) $coluna-referencias`, em cada linha. Abaixo de 900px a coluna vai para baixo do texto, sem JS. Destaque por eventos `mouseenter`, `focusin` e equivalentes, com sinal local à linha; sem animação de entrada. Título em 112px no computador, com `clamp` no celular. Abas com indicação de rolagem por máscara em CSS no celular. Lista da divisão recolhida em `<details>` no celular. Sem `SecaoCardComponent` nem `bt-badge-secao` aqui.

- [ ] **Step 4: Rodar testes**

Run: `npm test && npx tsc --noEmit -p tsconfig.app.json`
Expected: tudo passa.

- [ ] **Step 5: Conferência visual com conteúdo fictício**

Stack `-p biblia-teste` com `Conteudo__Pasta` apontando para conteúdo fictício de Isaías, reescrito no novo formato no scratchpad. Capturar cada aba em 375, 1366 e 1920px, incluindo perfil, promessas, citação do Antigo Testamento e nota aberta. Corrigir o que aparecer, no máximo duas rodadas de captura. Rodar `.claude/skills/impeccable/scripts/impeccable detect --json` nos arquivos alterados.

- [ ] **Step 6: Commit e PARAR para aprovação do utilizador**

Commit: `feat(biblia): pagina do livro com coluna de referencias`. Entregar as capturas e o caminho do teste local. **Não iniciar a Task 6 sem aprovação explícita do padrão.**

### Task 6: Página inicial, índice e ficha

**Files:**
- Create: `shared/ui/indice-canon/indice-canon.component.{ts,html,scss,spec.ts}`
- Modify: `features/home/*`, `features/biblia/biblia-lista/*`, `features/biblia/biblia-ficha/*`, `core/services/conteudo.service.ts`
- Modify (backend): `IConteudoQueryService` e implementação, controller do Panorama, `tests/BibliaTeologica.Api.Tests/PanoramaEndpointsTests.cs`

**Interfaces:**
- Produces: `GET /api/panoramas/livros` devolve `string[]` com slugs publicados, com comentário que justifica ser público. `ConteudoService.listarLivrosComPanorama(): Observable<string[]>`.
- Produces: `IndiceCanonComponent` (`bt-indice-canon`), inputs `livros: LivroResumo[]`, `comPanorama: string[]`, e `filtro: string = ''`. Agrupa por testamento (`ordem < 40` é Antigo) e `secaoRotulo`.

- [ ] **Step 1: Escrever os testes que falham**

Backend: `GET_PanoramasLivros_RetornaSlugsPublicados`, `GET_PanoramasLivros_RetornaVazio_QuandoSemConteudo`, `POST_PanoramasLivros_Retorna405`. Frontend: índice "mantém a ordem do cânon com filtro"; "marca só livros com panorama"; "sem marca e sem erro quando a lista falha"; home "sem Notícias, Curiosidades, membros, Criar conta como chamada principal nem 'Cristo aparece'"; "lista estudos só quando há"; ficha "abre na moldura, sem abas".

- [ ] **Step 2: Rodar e ver falhar**

Run: `dotnet test BibliaTeologica.sln && cd frontend/biblia-teologica-web && npm test`
Expected: FAIL nos novos.

- [ ] **Step 3: Implementar**

Conforme spec 5.2 e 5.3. Frase da página inicial: "Cada livro da Bíblia situado: quem escreveu, para quem e o que acontecia ao redor, com referência para conferir." A guarda em `$capa` fecha a página. Ficha usa `bt-livro-moldura` e mantém os campos atuais e a navegação anterior/próximo, com "Pra quem" corrigido para "Para quem" e "Porque escreveu" para "Por que escreveu".

- [ ] **Step 4: Rodar testes, conferir e commitar**

Run: `dotnet test BibliaTeologica.sln && npm test`
Expected: tudo passa. Capturas em 375, 1366 e 1920px de início, Livros com busca e ficha.
Commit: `feat(home): indice do canon na pagina inicial e em livros`

### Task 7: Estudos

**Files:**
- Modify: `features/estudos/estudos-lista/*`, `features/estudos/estudo-pagina/*`, `shared/ui/markdown/markdown.component.scss`

**Interfaces:**
- Consumes: `bt-linha-estudo`, `notasDoTexto`, `bt-referencia`, `LugarAtualService`.

- [ ] **Step 1: Escrever os testes que falham**

"Estudo define o lugar atual e o limpa ao sair"; "notas do estudo aparecem na coluna e o `id` da âncora continua igual"; "lista vazia diz que ainda não há estudos e leva a Livros"; "estudo 404 oferece voltar à lista".

- [ ] **Step 2: Rodar e ver falhar**

Run: `npm test`
Expected: FAIL.

- [ ] **Step 3: Implementar**

Conforme spec 5.4. O corpo em Markdown continua sanitizado, sem `bypassSecurityTrust`. As notas ficam em uma coluna fixa ao lado do corpo, porque o Markdown não se divide em linhas de grade; no celular vão para o fim. Bibliografia no fim.

- [ ] **Step 4: Rodar testes, conferir e commitar**

Run: `npm test && npm run build`
Expected: tudo passa. Capturas em 375, 1366 e 1920px.
Commit: `feat(estudos): lista e leitura no mundo Biblia de Estudo`

### Task 8: Conta, institucional e remoção do Material

**Files:**
- Modify: `features/auth/entrar/*`, `features/auth/registar/*`, `features/conta/perfil/*`, `features/conta/trocar-senha/*`, `features/institucional/*`
- Delete: `src/styles/material.scss`; entrada em `angular.json:35`
- Modify: `package.json`, `package-lock.json` (remover `@angular/material` e `@angular/cdk`), `src/index.html` (Material Icons)

**Interfaces:**
- Consumes: `bt-botao`, `bt-campo`, `bt-campo-senha`, `bt-alerta`.

- [ ] **Step 1: Ajustar os testes primeiro**

Trocar seletores `mat-*` por `bt-*`. Manter as asserções de validação, erro, 2FA e redirecionamento. Adicionar: "textos em pt-BR: Criar conta, Minha conta, senha"; "em-breve diz que a seção ainda não tem conteúdo e leva a Livros".

- [ ] **Step 2: Rodar e ver falhar**

Run: `npm test`
Expected: FAIL.

- [ ] **Step 3: Implementar e remover**

Antes de remover o pacote, procurar `@angular/material` e `@angular/cdk` em `src/` e resolver o que restar, inclusive `MatSnackBar`, overlays e animações em `app.config.ts`. Reescrever os textos em pt-PT em toda a interface, sem mudar a URL `/registar`.

- [ ] **Step 4: Rodar tudo**

Run: `npm ci && npm test && npm run build && npx tsc --noEmit -p tsconfig.app.json`
Expected: tudo passa; `grep -r "@angular/material\|@angular/cdk\|mat-" src` sem resultado de código. Comparar o tamanho do bundle inicial com o de antes.

- [ ] **Step 5: Conferir e commitar**

Capturas das 4 telas e de "em breve" em 375 e 1366px, em repouso e com erro.
Commit: `feat(conta): telas com componentes proprios e remove Angular Material`

### Task 9: Limpeza, revisão final e documentação

**Files:**
- Modify: `src/styles/_variables.scss`, `_mixins.scss` (saem aliases, `rotulo-mono`, `eyebrow`)
- Delete: componentes sem uso confirmados por busca (`secao-card`, `badge-secao`, `campo-ficha`, `cartao`, se sobrarem), `features/ui-provas` só se o utilizador autorizar
- Create: `DESIGN.md`, `.impeccable/design.json` (via `impeccable-documenter`)
- Modify: `docs/ROADMAP.md`, `CLAUDE.md` (frontend sem Material), memória do projeto

- [ ] **Step 1: Remover restos e rodar tudo**

Run: `dotnet test BibliaTeologica.sln && cd frontend/biblia-teologica-web && npm test && npm run build`
Expected: tudo passa; busca por `$acc`, `$parchment`, `$brass`, `Space Grotesk` e `IBM Plex` sem resultado em `src/`.

- [ ] **Step 2: Revisão final de design**

Rodar `impeccable detect --json`. Capturar `.impeccable/review/desktop.png`, `mobile.png` e a largura 1920. Acionar `impeccable-finish-reviewer` com o pedido, respostas, contrato, capturas, achados do detector e `model-pick.html` como referência de crítica. Agir conforme a disposição: `ship`, `fix`, `rebuild` ou `recapture`.

- [ ] **Step 3: Documentar**

Acionar `impeccable-documenter`. Verificar `DESIGN.md` com tokens e `.impeccable/design.json`.

- [ ] **Step 4: Revisão de código da branch**

Revisão completa da branch por revisor novo. Até 3 rondas; decisão de produto ou segurança vai ao utilizador. Commit: `chore(ui): remove restos do visual anterior e documenta o design`.

- [ ] **Step 5: Entregar**

Apresentar diff, riscos, estimativa de esforço e os passos do teste manual em Docker. **Não fazer push nem PR sem pedido.** Conteúdo real de Isaías (Task 10 do plano do Panorama) e demais livros seguem em planos próprios, depois da aprovação do padrão.
