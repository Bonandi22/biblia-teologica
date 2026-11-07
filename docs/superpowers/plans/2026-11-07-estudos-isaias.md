# Estudos completos de Isaías — plano de execução

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** publicar os seis Estudos completos de Isaías no molde do piloto de Senaqueribe e
permitir o link "Estudo completo" em profecias, promessas e perfil.

**Architecture:** cada estudo é um arquivo `content/estudos/<slug>.md` (frontmatter YAML +
Markdown), importado e validado pela API (`ValidadorConteudo`) e mostrado pela página de
estudo, que já tem o layout padrão. A tarefa de código acrescenta um campo opcional
`estudo` a `RegistroApontamento` e `RegistroPerfilItem`, de ponta a ponta (validador,
DTO, construtor, modelo e telas).

**Tech Stack:** conteúdo em YAML/Markdown; .NET 10 (xUnit + AwesomeAssertions), testes
rodados em `mcr.microsoft.com/dotnet/sdk:10.0` via `docker run` (o SDK local é o 8); Angular
22 com Vitest.

**Spec:** `docs/superpowers/specs/2026-11-07-estudos-isaias-design.md`

## Global Constraints

- Molde do piloto: ~1.200 a 1.500 palavras; relato bíblico; fontes antigas abertas só
  quando ajudam a ler o texto; comparação breve sem decidir quem tem razão.
- Só fonte aberta, aberta na URL e conferida (`content/GUIA-DE-ESTILO.md`, seção 2);
  nunca inventar obra, citação ou local. Enciclopédia: um registro por verbete.
- Termo difícil explicado na primeira vez, com nota para fonte aberta.
- Neutralidade (guia, seções 3 e 4): nada de "cumpre/cumprida"; o que vem de fora é
  atribuído à fonte; divergência entre tradições breve e atribuída.
- Parágrafo com aspas precisa de `[^n]` no mesmo parágrafo (regra 6 do validador).
- Só o utilizador muda `estado` para `revisado`. Ligar em `isaias.yaml` e em
  `relacionados` só depois disso (regra 8: link para estudo não publicado quebra a
  importação em Production).
- Um estudo de cada vez; mudança de molde pedida num estudo vale também para os já
  publicados.
- Git: cada estudo e a tarefa de código em branch própria a partir da `main`
  (`feature/BT-28-<slug>`), com PR próprio. Commits terminam com
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Verificar o staged antes de
  cada commit.
- Contrato: `/api/livros` e `/api/livros/{slug}` não mudam; `/api/panoramas/livros/{slug}`
  só ganha o campo opcional `estudoSlug` em perfil, profecias e promessas.

## Review Focus

1. Estudo ligado em `isaias.yaml` ou em `relacionados` enquanto ainda é rascunho: em
   Production a API não sobe. O passo de ligação roda os testes `ConteudoReal` (com e sem
   rascunhos) e sobe a stack normal antes do commit.
2. Slug de `estudo` em profecia, promessa ou perfil que não existe: a importação de um
   `revisado` precisa rejeitar com o caminho do campo. Coberto na Task 5.
3. Item sem `estudo`: perfil, profecia e promessa continuam sem link e sem espaço vazio.
   Coberto na Task 5.
4. Citação de fonte antiga com algo que a fonte não diz (o erro mais comum do piloto): o
   revisor confere cada aspa na URL. Está no procedimento comum, passo 5.
5. Quebra de layout com conteúdo novo (título longo, muitas notas da ARC): conferir a
   página em 390, 1366 e 1920px antes de entregar ao utilizador.

---

## Procedimento comum de um estudo

As Tasks 1, 2, 3, 4, 6 e 7 seguem estes passos. Cada task só diz o que muda.

- [ ] **P1. Branch:** `git switch main && git pull && git switch -c feature/BT-28-<slug>`.
- [ ] **P2. Fontes:** abrir as passagens da ARC (`https://www.bibliaonline.com.br/arc/<sigla>/<cap>`)
  e as fontes antigas sugeridas na task; guardar as páginas em `<scratchpad>/fontes/`.
  Fonte nova vai para `content/bibliografia.yaml` com comentário de conferência (data e o
  que ela diz).
- [ ] **P3. Rascunho:** `content/estudos/<slug>.md` com `estado: rascunho`, `titulo`,
  `resumo`, `livros`, `relacionados: []` e `notas` numeradas na ordem de uso. Molde e
  tamanho do piloto `content/estudos/senaqueribe-cerco-jerusalem.md`.
- [ ] **P4. Validação local:** subir `docker compose -p biblia-teste -f docker-compose.yml -f <override-rascunhos.yml> up --build -d`
  (override com `ASPNETCORE_ENVIRONMENT: Development` e `Conteudo__IncluirRascunhos: "true"`).
  Esperado no log da API: `Conteúdo importado: … <n> estudo(s)`, sem `Conteúdo inválido`.
  Conferir a página em 390, 1366 e 1920px. Commit `docs(conteudo): estudo <tema> em rascunho`.
- [ ] **P5. Conferência independente:** subagente revisor sem o contexto da escrita, só
  leitura, com os critérios 1 (exatidão: cada aspa conferida na URL e cada `local`
  correto), 2 (neutralidade), 3 (afirmação sem fonte) e 4 (vícios de IA, guia seção 5).
  Saída: até 25 achados ranqueados. Aplicar os de fato, de neutralidade e de estilo; levar
  ao utilizador os que forem decisão dele. Revalidar (P4) e fazer commit
  `docs(conteudo): aplica a revisão independente no estudo <tema>`.
- [ ] **P6. Leitura do utilizador:** subir a stack dele com o override de rascunhos e abrir
  `http://localhost:4300/estudos/<slug>`. Aplicar o que ele pedir; se for mudança de molde,
  aplicar também aos estudos já publicados.
- [ ] **P7. Publicação (só quando o utilizador disser que pode):** `estado: revisado`;
  preencher `estudo: <slug>` nos itens de `isaias.yaml` listados na task; atualizar
  `relacionados` do novo estudo e dos vizinhos já publicados. Rodar
  `docker run --rm -v "<repo>:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~ConteudoReal"`
  (esperado: `Passed: 2`) e subir a stack normal (`docker compose up --build -d`), com
  `estudoSlug` presente em `/api/panoramas/livros/isaias`. Commit
  `docs(conteudo): estudo <tema> revisado e ligado a Isaías`.
- [ ] **P8. Medição:** acrescentar ao fim de `docs/superpowers/plans/2026-11-02-panorama-medicao.md`
  (criar o arquivo se não existir) uma linha: estudo, data do primeiro e do último commit,
  rodadas com o utilizador e tokens do revisor.
- [ ] **P9. PR:** suíte completa (backend no container e `npm test` no frontend), push e
  `gh pr create --base main`, com corpo terminado em
  `🤖 Generated with [Claude Code](https://claude.com/claude-code)`. O merge espera o teste
  manual do utilizador.

---

### Task 1: Estudo — Acaz e a guerra contra Israel e Síria

**Files:** Create `content/estudos/acaz-guerra-siria-israel.md`; Modify `content/livros/isaias.yaml`
(personagem Acaz; acontecimento "Nos dias de Acaz" no panorama; acontecimento "No reinado
de Tiglate-Pileser, entre cerca de 745 e 727 a.C." no contexto), `content/bibliografia.yaml`
se entrar fonte nova.

- [ ] **Passos P1 a P9**, com:
  - Passagens: Isaías 7 e 8; 2 Reis 16; 2 Crônicas 28.
  - Fontes antigas a procurar: inscrições de Tiglate-Pileser III no Oracc/RINAP 1 (menção
    a "Jehoahaz of Judah" entre os tributários); ABC 1, que já está no catálogo.
  - Cuidado: Isaías 7:14 aparece só como texto e referência, sem leitura de cumprimento.
  - `relacionados`: `senaqueribe-cerco-jerusalem` (até a queda de Samaria ser publicada).

### Task 2: Estudo — A queda de Samaria e o avanço assírio

**Files:** Create `content/estudos/queda-de-samaria.md`; Modify `content/livros/isaias.yaml`
(acontecimento "Por volta de 724 a 722 a.C." no contexto) e os `relacionados` de
`acaz-guerra-siria-israel` e `senaqueribe-cerco-jerusalem`.

- [ ] **Passos P1 a P9**, com:
  - Passagens: 2 Reis 17 e 18:9–12; Isaías 10:5–11 e 28:1–4.
  - Fontes antigas: ABC 1 (Salmaneser "devastou Samaria", já usada em Isaías); inscrições
    de Sargão II no Oracc (deportação de Samaria). Datas divergentes viram faixa ("724 ou
    722 a.C.", já no catálogo pelo Livius).
  - `relacionados`: `acaz-guerra-siria-israel`, `senaqueribe-cerco-jerusalem`.

### Task 3: Estudo — Nabucodonosor e a queda de Jerusalém

**Files:** Create `content/estudos/nabucodonosor-queda-jerusalem.md`; Modify
`content/livros/isaias.yaml` (acontecimento "Por volta de 597 a.C." no mundo) e os
`relacionados` de `senaqueribe-cerco-jerusalem`.

- [ ] **Passos P1 a P9**, com:
  - Passagens: 2 Reis 24 e 25; Isaías 39:5–7; 2 Crônicas 36.
  - Fontes antigas: ABC 5 (cerco de 597, já no catálogo); Josefo, *Antiguidades* livro 10,
    tradução aberta do Perseus, para 586. A ABC 5 não cobre 586: não atribuir a ela.
  - Cuidado: manter a decisão `3f91322` (597 com as palavras de 2 Reis 24:13, sem ligar a
    Isaías 39 como cumprimento).
  - `relacionados`: `senaqueribe-cerco-jerusalem`.

### Task 4: Estudo — Ciro e o retorno

**Files:** Create `content/estudos/ciro-e-o-retorno.md`; Modify `content/livros/isaias.yaml`
(personagem Ciro; acontecimento "Por volta de 539 a.C." no mundo) e os `relacionados` de
`nabucodonosor-queda-jerusalem`.

- [ ] **Passos P1 a P9**, com:
  - Passagens: Isaías 44:28–45:13; Esdras 1; 2 Crônicas 36:22–23.
  - Fontes antigas: ABC 7 (Crônica de Nabonido) e Cilindro de Ciro, já no catálogo.
  - Cuidado: o Cilindro não fala de Judá nem do templo de Jerusalém; não atribuir a ele.
  - `relacionados`: `nabucodonosor-queda-jerusalem`.

### Task 5: Link "Estudo completo" em perfil, profecias e promessas (código)

**Files:**
- Modify: `src/BibliaTeologica.Application/Conteudo/Registros.cs` (`RegistroApontamento`, `RegistroPerfilItem`)
- Modify: `src/BibliaTeologica.Application/Conteudo/ValidadorConteudo.cs` (`ValidarPerfil`, `ValidarApontamento`)
- Modify: `src/BibliaTeologica.Application/Conteudo/Dtos/ConteudoDtos.cs` (`ApontamentoDto`, `PerfilItemDto`)
- Modify: `src/BibliaTeologica.Application/Conteudo/ConstrutorDocumentos.cs` (linhas ~29 e ~58)
- Modify: `frontend/biblia-teologica-web/src/app/core/models/conteudo.model.ts` (`Apontamento`, `PerfilItem`)
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/livro-pagina/apontamentos/apontamentos.component.ts`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/livro-pagina/livro-pagina.component.html` (bloco `.perfil`)
- Test: `tests/BibliaTeologica.Application.Tests/Conteudo/ValidadorConteudoTests.cs`, `ConstrutorDocumentosTests.cs`, `apontamentos.component.spec.ts`, `livro-pagina.component.spec.ts`

**Interfaces:**
- Produces: `public string? Estudo { get; set; }` em `RegistroApontamento` e `RegistroPerfilItem`
  (YAML `estudo`); `ApontamentoDto(string Referencia, string Anuncia, IReadOnlyList<NotaDto> Notas, string? EstudoSlug)`;
  `PerfilItemDto(string Rotulo, string Texto, IReadOnlyList<string> Referencias, string? EstudoSlug)`;
  no TypeScript, `estudoSlug: string | null` em `Apontamento` e `PerfilItem`.
- Consumes: `Contexto.EstudoSlugs` e a mensagem da regra 8 (`ValidadorConteudo.cs:167`):
  `o estudo '<slug>' não está publicado nesta importação.`

- [ ] **Step 1: Testes que falham (backend)**, no padrão do projeto:
  - `Validar_RetornaErroComArquivo_QuandoProfeciaApontaEstudoNaoPublicado`: o erro contém
    `profecias[0].estudo` e a mensagem da regra 8.
  - `Validar_RetornaErroComArquivo_QuandoPerfilApontaEstudoNaoPublicado`: o erro contém
    `perfil[0].estudo`.
  - `Validar_Aceita_QuandoPromessaApontaEstudoPublicado`: sem erros.
  - `Construir_LevaEstudoSlug_QuandoApontamentoEPerfilTemEstudo` e
    `Construir_EstudoSlugNulo_QuandoItemSemEstudo`.
- [ ] **Step 2: Rodar** `docker run … dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~ValidadorConteudo|FullyQualifiedName~ConstrutorDocumentos"`.
  Esperado: falha de compilação (propriedade `Estudo` inexistente).
- [ ] **Step 3: Implementar** as propriedades, a checagem da regra 8 em `ValidarPerfil` e
  `ValidarApontamento` (mesma forma da linha 167) e o campo nos DTOs e no construtor.
- [ ] **Step 4: Rodar** os mesmos testes e depois a suíte inteira. Esperado: PASS.
- [ ] **Step 5: Testes que falham (frontend):**
  - `apontamentos.component.spec.ts`: `'mostra Estudo completo com link para /estudos/<slug> quando o item tem estudoSlug'`
    e `'não mostra Estudo completo quando estudoSlug é null'`.
  - `livro-pagina.component.spec.ts`: o mesmo par para um item do perfil.
- [ ] **Step 6: Rodar** `npm test`. Esperado: falham os quatro novos.
- [ ] **Step 7: Implementar** o link com o mesmo markup e a mesma classe do
  `item-estudo.component.ts:33-34` (`<a class="item__estudo" [routerLink]="['/estudos', slug]">Estudo completo</a>`),
  extraindo um componente `bt-link-estudo` em `shared/ui/link-estudo/` se os três lugares
  repetirem o markup.
- [ ] **Step 8: Rodar** `npm test`, `npx tsc --noEmit -p tsconfig.app.json` e `npm run build`. Esperado: tudo verde.
- [ ] **Step 9: Conferir** no navegador, com um slug de teste num rascunho, a profecia e o
  perfil com e sem link, em 390 e 1366px.
- [ ] **Step 10: Commit e PR** `feat(panorama): link Estudo completo em perfil, profecias e promessas`.

### Task 6: Estudo — O Servo sofredor

**Depende de:** Task 5 na `main`.

**Files:** Create `content/estudos/servo-sofredor.md`; Modify `content/livros/isaias.yaml`
(profecia `Isaías 52:13–53:12`, campo `estudo`).

- [ ] **Antes de P3:** procurar fonte aberta para a leitura judaica do Servo (Rashi no
  Sefaria; Jewish Encyclopedia de 1906). Se não houver fonte que abra e diga o que o texto
  vai afirmar, **parar e levar ao utilizador** antes do rascunho.
- [ ] **Passos P1 a P9**, com:
  - Passagens: os cânticos (Isaías 42:1–9; 49:1–6; 50:4–9; 52:13–53:12) e as citações do
    Novo Testamento que já estão na aba Citações.
  - Cuidado: cada leitura atribuída à sua fonte; o texto não diz quem é o Servo.
  - `relacionados`: `isaias-profeta-e-livro` quando ele for publicado (Task 7).

### Task 7: Estudo — Isaías, o profeta e o livro

**Depende de:** Task 5 na `main`.

**Files:** Create `content/estudos/isaias-profeta-e-livro.md`; Modify `content/livros/isaias.yaml`
(perfil "Quem era", campo `estudo`) e os `relacionados` de `servo-sofredor`.

- [ ] **Passos P1 a P9**, com:
  - Passagens: Isaías 1:1, 6, 20, 36–39; 2 Reis 19–20; 2 Crônicas 26:22 e 32:32.
  - Fontes: verbete "Isaiah" da ISBE (online) para a discussão de autoria, em uma frase
    atribuída a quem a defende.
  - Cuidado: não contradizer "a Jerusalém e a um povo cativo na Babilônia" (`3f91322`); a
    posição tradicional é a do texto.
  - `relacionados`: `servo-sofredor`.

### Task 8: Fechamento

- [ ] **Step 1:** consolidar `docs/superpowers/plans/2026-11-02-panorama-medicao.md`:
  tempo médio por estudo, custo do revisor e estimativa para os outros 65 livros, com o
  guardrail de Custo do `CLAUDE.md`.
- [ ] **Step 2:** marcar no plano do panorama os Steps 6 e 8 como feitos e pedir ao
  utilizador a aprovação final de Isaías por inteiro (Step 6.2). Nenhum outro livro
  começa antes disso.
- [ ] **Step 3:** commit e PR `docs(panorama): medição e fechamento de Isaías`.
