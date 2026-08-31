# M2d-a — Fundações + design system (design)

**Data:** 2026-08-31
**Estado:** revista — pronta para o plano de implementação
**Marco:** M2d-a de 3 (M2d — repaginação visual; ver `docs/ROADMAP.md`)
**Depende de:** M2c (casca do portal) — em `main` (`5846bb8`).

---

## 1. Contexto

O M2c entregou a casca do portal com uma identidade "editorial" leve, aplicada só
por troca dos valores dos tokens. Ficaram dois sistemas visuais a coexistir:

- **Casca + fichário** — tokens SCSS, serifas Fraunces/Source Serif, sotaque bordô.
- **`/entrar`, `/registar`, `/conta`** — Angular Material 3, paleta azul, Roboto — um mundo à parte.

O utilizador quer uma **repaginação visual completa**. Depois de rever direções no
companion de brainstorming, a direção ficou fechada:

- **Organização "portal"** (à la globo.com) — homepage composta por blocos de secção
  empilhados e rotulados, com hierarquia lead/apoio e um rail lateral; cresce à
  medida que os marcos entregam conteúdo.
- **Tipografia** — **Space Grotesk** (display, nav, rótulos, UI) + **Source Serif 4**
  (corpo de leitura, valores da ficha) + **IBM Plex Mono** (eyebrows, metadados).
  Saem Fraunces e Roboto.
- **Sotaque** — **terracota queimada `#A6371C`**, usado com parcimónia (links, estado
  ativo, "ver tudo", barra do versículo, focus ring). Botões primários usam a tinta
  escura, não o sotaque.
- **Estética** — off-white, quase monocromática, divisórias hairline, muito ar.
- **Angular Material** — **híbrido**: sai das páginas públicas (auth, conta), que
  passam a primitivos feitos à mão no design system; fica **só no `/admin`** (M2b),
  ferramenta interna onde tabelas/diálogos/snackbars de CRUD são o ponto forte do
  Material. O `material.scss` é re-tematizado para não destoar.

## 2. Decomposição do M2d

O M2d é grande demais para uma spec/plano só. Três sub-marcos, cada um um PR:

| Sub-marco | Entrega | Depende de |
|---|---|---|
| **M2d-a** (esta spec) | Tokens, tipografia (`index.html`), `styles.scss`/`_mixins.scss`, re-tema do `material.scss`, **primitivos de formulário à mão**, `.gitattributes` | M2c |
| **M2d-b** | `/entrar`, `/registar`, `/conta` trocam Material → primitivos. Header/footer re-estilizados (footer reutiliza `NAV_PRINCIPAL`). Páginas "Em breve". `title:` por rota, página 404. | M2d-a |
| **M2d-c** | Homepage recomposta na organização "portal" (alimentada pelo `LivroStore`). Re-skin do fichário (`biblia-lista`, `biblia-ficha`, `campo-ficha`, `badge-secao`). | M2d-a (melhor depois do -b) |

## 3. Escopo do M2d-a

### Dentro

- **Só frontend Angular.** Nenhuma alteração em Domain/Application/Infrastructure/Api.
- Reescrita dos **valores** de `src/styles/_variables.scss` — cor, tipografia, escala
  tipográfica fluida, escala de espaço, larguras, raio, breakpoints. Os **nomes
  antigos** das variáveis mantêm-se como **alias** para os novos, para -b/-c poderem
  migrar os SCSS de componente gradualmente sem partir o build de -a.
- `src/styles.scss` — `body` em `$fonte-display` + `$ink` + `$paper`; `:focus-visible`
  e `::selection` em `$acc`/`$acc-tint`; estilo base de link.
- `src/styles/_mixins.scss` — `@mixin tablet`, `@mixin eyebrow`.
- `src/index.html` — um só `<link>` de fontes: entra Space Grotesk, ficam Source
  Serif 4 + IBM Plex Mono, saem Fraunces e Roboto. Material Icons fica.
- `src/styles/material.scss` — re-tema: primário quente (`mat.$rose-palette`),
  tipografia Space Grotesk, densidade 0; sai Roboto.
- **Primitivos de formulário** em `src/app/shared/ui/` — `bt-campo` (+ diretiva
  `btInput`), `bt-campo-senha`, `bt-botao`, `bt-cartao`, `bt-alerta`. Standalone,
  OnPush, prefixo `bt-`, a11y embutida, cada um com spec.
- Rota **dev-only `/ui`** — kitchen-sink dos primitivos em todos os estados; só
  registada quando `!environment.production`. Dobra como documentação viva.
- **`.gitattributes`** na raiz do repo — normalização de fins de linha (o ruído CRLF
  que o code review do M2c apontou).
- Entrada **M2d** no `ROADMAP.md`.

### Fora (sub-marcos seguintes ou mais tarde)

- Trocar o Material nas páginas auth/conta pelos primitivos (M2d-b).
- Re-estilizar header/footer, homepage-portal, fichário (M2d-b/-c).
- Match exato de terracota no tema do Material do `/admin` — polish quando o M2b
  construir o `/admin`.
- Qualquer alteração de layout ou de estrutura de página.
- Remover o pacote `@angular/material` (fica para o `/admin`; CDK também fica).

## 4. Design

### 4.1 `src/styles/_variables.scss`

Reescrita. Blocos:

```scss
// ── Cor ──────────────────────────────
$ink:        #141416;   // texto, títulos, botão primário
$ink-mut:    #6a6a70;   // texto secundário, metadados
$paper:      #fdfdfc;   // fundo da página
$superficie: #ffffff;   // cartões, blocos elevados
$hair:       #e9e9e6;   // divisórias hairline
$acc:        #a6371c;   // sotaque: links, nav ativa, marcas
$acc-dk:     #8a2e17;   // hover do sotaque
$acc-tint:   rgba(166, 55, 28, 0.08); // fundo de selos

// alias de transição — nomes do M2c apontam para os novos.
// -b/-c substituem os usos e no fim do M2d os alias são removidos.
$parchment:   $paper;
$parchment-2: #f4f3ef;
$card:        $superficie;
$line:        $hair;
$brass:       $acc;
$near-black:  $ink;
$sage:        $ink-mut;

// ── Tipografia ───────────────────────
$fonte-display: 'Space Grotesk', 'Helvetica Neue', Arial, sans-serif;
$fonte-serif:   'Source Serif 4', Georgia, serif;
$fonte-mono:    'IBM Plex Mono', ui-monospace, monospace;

// ── Escala tipográfica ───────────────
$t-eyebrow: 0.6875rem;                    // 11px
$t-body:    1rem;
$t-lead:    1.125rem;
$t-h3:      1.375rem;
$t-h2:      clamp(1.6rem, 3vw, 2.2rem);
$t-h1:      clamp(2.2rem, 5vw, 3.4rem);
$tracking-tight:   -0.03em;               // títulos display
$tracking-eyebrow:  0.14em;

// ── Espaço / layout ──────────────────
$sp-1: 4px; $sp-2: 8px; $sp-3: 12px; $sp-4: 16px;
$sp-6: 24px; $sp-8: 32px; $sp-12: 48px; $sp-16: 64px;
$largura-conteudo: 1080px;
$largura-leitura:  42rem;
$raio: 6px;
$altura-header: 64px;
$altura-header-mobile: 56px;

// ── Breakpoints ──────────────────────
$bp-mobile: 900px;
$bp-tablet: 640px;
```

Notas:
- `$sage` era verde (`#5c6b4f`); passa a apontar para o cinza neutro `$ink-mut`. O
  único uso é o rótulo `.k`/estado no fichário — fica neutro, o que é o pretendido.
- `$fonte-display` e `$fonte-mono` mantêm os nomes; só muda o valor de `$fonte-display`
  (era Fraunces).

### 4.2 `src/styles.scss`

```scss
@use 'styles/variables' as *;

* { box-sizing: border-box; }
html, body { margin: 0; padding: 0; }

body {
  background: $paper;
  color: $ink;
  font-family: $fonte-display;
  font-size: $t-body;
  line-height: 1.55;
  min-height: 100vh;
  -webkit-font-smoothing: antialiased;
}

a { color: $acc; text-decoration: none; }
a:hover { color: $acc-dk; }

:focus-visible { outline: 2px solid $acc; outline-offset: 2px; }
::selection { background: $acc-tint; }

@media (prefers-reduced-motion: reduce) {
  * { transition: none !important; animation: none !important; }
}
```

O corpo é `$fonte-display` (a maioria do texto é UI). A prosa (valores da ficha,
versículo, futuros Estudos) opta por `$fonte-serif` explicitamente nos SCSS de -c.

### 4.3 `src/styles/_mixins.scss`

Mantém `@mixin mobile` e `@mixin rotulo-mono`. Acrescenta:

```scss
@mixin tablet {
  @media (max-width: $bp-tablet) { @content; }
}

@mixin eyebrow {
  font-family: $fonte-mono;
  font-size: $t-eyebrow;
  letter-spacing: $tracking-eyebrow;
  text-transform: uppercase;
  color: $ink-mut;
}
```

### 4.4 `src/index.html`

Substituir os dois `<link>` de fontes Google por **um**:

```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link
  href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500&family=Source+Serif+4:ital,opsz,wght@0,8..60,400;0,8..60,600;1,8..60,400&family=Space+Grotesk:wght@400;500;700&display=swap"
  rel="stylesheet"
/>
```

O `<link>` de Material Icons fica (o `/admin` do M2b usa). O `<link>` de Roboto sai.

### 4.5 `src/styles/material.scss`

```scss
@use '@angular/material' as mat;

html {
  @include mat.theme((
    color: (
      primary: mat.$rose-palette,
      tertiary: mat.$orange-palette,
    ),
    typography: Space Grotesk,
    density: 0,
  ));
  color-scheme: light;
}
```

A forma exata da config de tipografia do Material 22 (`typography: <family>` vs
`typography: (plain-family: …, brand-family: …)`) é confirmada com o context7 no
plano. Objetivo: tirar o Material do `azure`+Roboto. O `rose` é o mais próximo do
terracota para uma ferramenta interna; o match fino fica para o M2b.

### 4.6 Primitivos — `src/app/shared/ui/`

Pasta nova, distinta de `shared/components/` (que tem widgets de domínio como
`badge-secao`, `campo-ficha`). Todos: `standalone: true`, `OnPush`, prefixo `bt-`.

#### `bt-campo` + diretiva `btInput`

`CampoComponent` — wrapper de campo de formulário.

- Inputs: `label` (required), `erro?: string | null`, `hint?: string`, `obrigatorio?: boolean`.
- Template: `<label [for]="id">{{ label }}</label>`, `<ng-content />` (o controlo),
  `@if (hint && !erro) <p class="campo__hint" [id]="hintId">`, `@if (erro)
  <p class="campo__erro" role="alert" [id]="erroId">{{ erro }}</p>`.
- Gera `id` único (contador de módulo). Query de conteúdo `contentChild(BtInput)`;
  um `effect` empurra para a diretiva: `id`, `aria-describedby` (erroId/hintId),
  `aria-invalid` (`!!erro`).

`BtInputDirective` (`selector: '[btInput]'`) — hook de estilo + a11y no `<input>`/
`<textarea>` projetado.

- `host`: `class="bt-input"`, `[id]`, `[attr.aria-invalid]`, `[attr.aria-describedby]`.
- Métodos/estado expostos ao `bt-campo` via signals (`setId`, `setDescribedBy`,
  `setInvalid`) ou inputs internos — o plano fixa a mecânica.

Contrato a11y: `<label for>` sempre associado; erro anunciado (`role="alert"`) e
ligado por `aria-describedby`; `aria-invalid="true"` no erro.

#### `bt-campo-senha`

`CampoSenhaComponent` — implementa `ControlValueAccessor` (funciona com
`formControlName`). Renderiza um `bt-campo` internamente (ou o mesmo markup) com o
`<input>` cujo `type` alterna `password`↔`text` via um `<button type="button">`
dentro do campo.

- Inputs: `label` (required), `hint?`, `erro?`, `autocomplete?` (default `current-password`).
- Botão de alternância: `[attr.aria-pressed]="visivel()"`, `[attr.aria-label]="visivel() ? 'Esconder senha' : 'Mostrar senha'"`.

#### `bt-botao`

`BotaoComponent` — componente com `<button>` interno (estilos encapsulados;
`type="submit"` interno dentro de um `<form>` continua a submeter).

- Inputs: `variante: 'primario' | 'fantasma' | 'texto'` (default `primario`),
  `type: 'button' | 'submit'` (default `button`), `carregando?: boolean`, `disabled?: boolean`.
- `primario` = fundo `$ink`, texto branco. `fantasma` = borda `$acc`, texto `$acc`.
  `texto` = só texto `$acc`.
- `carregando` → `<button disabled aria-busy="true">` + spinner; o texto projetado
  mantém-se (ou é substituído — o plano decide, mas o `aria-busy` é obrigatório).

#### `bt-cartao`

`CartaoComponent` — superfície para páginas auth/conta.

- Input opcional `titulo?: string` → renderiza `<h1>` no topo.
- `<section class="bt-cartao">` : bg `$superficie`, borda `1px solid $hair`, `$raio`,
  padding generoso, `max-width` de leitura.

#### `bt-alerta`

`AlertaComponent` — mensagem inline (substitui os `.erro`/`.msg` ad-hoc de auth/conta).

- Inputs: `tipo: 'erro' | 'sucesso'` (default `erro`).
- `role="alert"` quando `erro`, `role="status"` quando `sucesso`.
- `<ng-content />` para o texto.

### 4.7 Rota dev-only `/ui`

`src/app/features/ui-provas/ui-provas.component.ts` — página que instancia cada
primitivo em todos os estados (normal, erro, hint, carregando, disabled; botão nas
3 variantes; alerta nos 2 tipos). Registada em `app.routes.ts` **só** quando
`!environment.production`:

```ts
...(environment.production ? [] : [{
  path: 'ui',
  loadComponent: () => import('./features/ui-provas/ui-provas.component').then(m => m.UiProvasComponent),
}]),
```

### 4.8 `.gitattributes` (raiz do repo)

```
* text=auto eol=lf
*.png binary
*.jpg binary
*.ico binary
*.webp binary
```

Corrige o ruído CRLF no Windows que o code review do M2c apontou. Não renormaliza
ficheiros já commitados nesta leva (evita um diff gigante) — só governa a partir daqui.

## 5. O que muda à vista só com o M2d-a

- **Shell (header/footer/home) + fichário** — herdam pelos tokens: paleta terracota/
  near-black/off-white e a fonte de display Space Grotesk. Layout intocado.
- **Auth/conta** — o Material passa de azul→quente e Roboto→Space Grotesk pelo
  re-tema. Continua com forma Material até ao M2d-b.
- **Estado transitório em `main`** — aceitável: não há deploy real (isso é M5+) e o
  M2d-b segue logo a seguir. Alternativa (stack das branches -a→-b→-c e merge no
  fim) fica à decisão do utilizador no plano.

## 6. Testes

Vitest (`@angular/build:unit-test`, jsdom). Convenção `Metodo_ResultadoEsperado_QuandoCondicao`.

- **`bt-campo`** — `<label for>` casa com o `id` do `<input>` projetado; `erro` →
  `<p role="alert">` presente + `aria-invalid="true"` no input + `aria-describedby`
  aponta para o id do erro; `hint` some quando há `erro`.
- **`bt-campo-senha`** — o toggle alterna `type` password↔text; `aria-pressed` e
  `aria-label` do botão acompanham; escreve valor → `ControlValueAccessor` propaga.
- **`bt-botao`** — classe/estilo por `variante`; `carregando` → `disabled` +
  `aria-busy="true"`; `type="submit"` dentro de `<form>` dispara submit.
- **`bt-cartao`** — renderiza `<h1>` quando `titulo`; projeta conteúdo.
- **`bt-alerta`** — `role` correto por `tipo`.
- **`ui-provas`** — smoke: monta sem erros com todos os primitivos.
- **Regressão** — `npm test` (66 existentes) verde; `npx ng build` limpo, sem
  avisos de Sass; `npx tsc --noEmit -p tsconfig.app.json` limpo.
- **Manual (utilizador)** — `docker compose up --build`: shell + fichário na paleta/
  fonte novas; `/entrar`, `/registar`, `/conta` ainda funcionam (Material re-tematizado);
  `/ui` mostra os primitivos em dev.

## 7. Segurança

- Sem endpoints, sem HTTP nos primitivos, sem migração.
- Sem `innerHTML` de dado bruto; sem `bypassSecurityTrust*`.
- `bt-campo-senha` não loga nem expõe o valor; o toggle é puramente de apresentação.

## 8. Roadmap

Acrescentar ao `docs/ROADMAP.md`, entre M2c e M3:

```
- M2d — Repaginação visual (design pass). Direção "editorial moderno": organização
  "portal", Space Grotesk + Source Serif 4 + IBM Plex Mono, sotaque terracota #A6371C,
  off-white quase-monocromático. Angular Material sai das páginas públicas (primitivos
  à mão) e fica só no /admin. Só frontend. Entregue em 3 PRs:
  - M2d-a — fundações + design system (tokens, tipografia, primitivos, re-tema do
    Material, .gitattributes). Spec: docs/superpowers/specs/2026-08-31-m2d-a-fundacoes-design.md
  - M2d-b — páginas públicas re-skin (auth, conta, header/footer, em-breve, 404, titles).
  - M2d-c — homepage "portal" + re-skin do fichário.
```

## 9. Estimativa de esforço

~5 primitivos (componente + scss + spec) + 1 diretiva + 1 página de provas + edições
a 4 ficheiros de estilo/config + `.gitattributes`. Uma sessão de implementação,
sem loops. Custo esperado: moderado (os primitivos com a11y correta são o grosso).

## 10. Notas para a implementação

- Usar a skill `frontend-design` ao afinar os primitivos e a escala tipográfica.
- Confirmar com o context7 a API de tema do Angular Material 22 (`mat.theme`,
  config de tipografia) antes de escrever o `material.scss`.
- Mockups da fase de brainstorming em `.superpowers/brainstorm/` (git-ignored):
  `d2-limpo.html`, `tipo-escolhido.html`, `sotaque-escolhido.html`,
  `organizacao-portal.html`.
