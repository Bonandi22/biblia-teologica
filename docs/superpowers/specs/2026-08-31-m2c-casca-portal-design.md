# M2c — Casca do portal (design)

**Data:** 2026-08-31
**Estado:** revista — pronta para o plano de implementação
**Marco:** M2c (ver `docs/ROADMAP.md`) — extra, entre o M2b e o M3
**Depende de:** M2a (autenticação + `/conta`) — já em `main`. Não depende do M2b.

---

## 1. Contexto

Hoje o site **é** o fichário dos 66 livros: a rota `''` redireciona para `/biblia` e
o `BibliaListaComponent` ocupa o ecrã todo, com a própria marca e o próprio link
`Entrar / A minha conta` no topo da sidebar. Não há homepage, header global nem footer.

O objetivo do projeto é um **portal de conhecimento teológico** com várias verticais
(Estudos, Notícias, Curiosidades, Vídeos, …). O M2c constrói a **casca** desse portal
— header global, footer, homepage e páginas "Em breve" — sem ainda criar nenhum tipo
de conteúdo novo. O fichário passa a ser **uma secção** dentro dessa casca.

Referência de produto discutida com o utilizador: `voltemosaoevangelho.com/blog`
(homepage com feed, secções, autores, área de membro). O M2c fica só pela estrutura;
os feeds/cards/notícias de cada secção vêm nos marcos seguintes.

## 2. Escopo

### Dentro

- **Só frontend Angular.** Nenhuma alteração em Domain / Application / Infrastructure / Api.
  Sem endpoints novos, sem migração, sem alteração de contrato.
- Header global (`bt-header`) — marca, navegação, ações conforme estado de autenticação,
  menu hambúrguer no mobile.
- Footer global (`bt-footer`) — 3 colunas (sobre + redes sociais · navegar · newsletter
  visual) + linha de copyright. Newsletter é inerte (não faz HTTP, não navega).
- `HomeComponent` (`bt-home`) na rota `''` — 4 blocos (hero, destaque do fichário,
  secções futuras, teaser de membros).
- `EmBreveComponent` (`bt-em-breve`) partilhado — rotas `/estudos`, `/noticias`,
  `/curiosidades`, com `titulo` e `descricao` vindos do `data` da rota. Os textos
  concretos (uma frase por secção) são decididos na implementação.
- `secao-card` — componente de card de secção, reutilizado na home.
- Ajuste mínimo ao `BibliaListaComponent` para viver por baixo do header global
  (remover a marca/link duplicados da sidebar; sidebar sticky abaixo do header em vez
  de ocupar o viewport todo).
- Nova identidade visual **"editorial teológico" com header claro**, aplicada por
  troca dos **valores** dos tokens globais em `_variables.scss` (os nomes não mudam).
  Swap dos tints `brass` escritos à mão (castanho → bordô) onde existirem.
- Entrada `M2c` no `ROADMAP.md`.

### Fora (marcos seguintes)

- Qualquer tipo de conteúdo novo (Estudos, Notícias, Curiosidades) — conteúdo, entidades,
  API, admin.
- Newsletter funcional (M4).
- Capacidades novas de membro: comentar, guardar, conteúdo reservado a login. A
  arquitetura **deixa o gancho** (ver §4.3) mas não implementa nada.
- Redesenho componente a componente do fichário, das páginas de auth e de `/conta`.
  Herdam a paleta nova pelos tokens; o polish fica para quando forem tocadas.
- Rota/área `/admin` (M2b).
- SSR / prerender / SEO (M5+).

## 3. Abordagem escolhida

**Header e footer no `AppComponent`.** `app.html` passa de `<router-outlet />` para:

```html
<bt-header />
<main class="app-main"><router-outlet /></main>
<bt-footer />
```

A rota `''` deixa de redirecionar para `/biblia` e passa a `HomeComponent` (lazy).
Todas as outras rotas continuam iguais, agora renderizadas dentro do `<main>`.

Alternativa considerada e rejeitada: `LayoutComponent` como rota-pai com as rotas
como `children`. Nesting mais canónico, mas mais churn e sem ganho — não há nenhuma
rota que deva ficar sem header/footer (mesmo `/entrar` e `/conta` beneficiam da
navegação global).

## 4. Componentes

### 4.1 `core/layout/header/header.component.ts` — `bt-header`

Standalone, `OnPush`. Injeta `AuthStore`.

- **Marca** "Bíblia Teológica" → `routerLink="/"`.
- **Navegação** a partir de um array de config (ver §4.3), com `routerLinkActive`.
  Itens visíveis no M2c: Início (`/`), Fichário (`/biblia`), Estudos (`/estudos`),
  Notícias (`/noticias`), Curiosidades (`/curiosidades`).
- **Ações à direita:**
  - `!auth.estaAutenticado()` → `Entrar` (link `/entrar`) + `Criar conta`
    (link `/registar`, estilo de botão sotaque).
  - `auth.estaAutenticado()` → `A minha conta` (link `/conta`).
  - `ehAdmin` e `/admin` ficam de fora (rota só chega no M2b).
- **Mobile** (`max-width: $bp-mobile`): botão hambúrguer com `aria-expanded`,
  signal `menuAberto`, painel colapsável. Fecha ao navegar (subscrição a
  `Router` events com `takeUntilDestroyed(this.destroyRef)`) e com `Escape`.
- Sem chamadas HTTP. Sem `innerHTML`.

### 4.2 `core/layout/footer/footer.component.ts` — `bt-footer`

Standalone, `OnPush`. Estático.

- **Coluna 1** — marca + tagline ("Portal de conhecimento teológico") + links de
  redes sociais como ícones, `href` placeholder (`#`), `target="_blank"`,
  `rel="noopener noreferrer"`, `aria-label` por rede.
- **Coluna 2** — "Navegar": Fichário, Estudos, Notícias, Curiosidades (`routerLink`).
- **Coluna 3** — "Newsletter": `<form>` com `input[type=email]` + botão. `(ngSubmit)`
  com `preventDefault`; não faz nada além de mostrar uma nota "Em breve". Sem
  `HttpClient`.
- **Rodapé** — `© {{ ano }} Bíblia Teológica`, `ano` calculado com `new Date().getFullYear()`.
- No mobile as colunas empilham.

### 4.3 `core/layout/nav.ts` — config de navegação

```ts
export interface ItemNav {
  rotulo: string;
  rota: string;
  membros?: boolean; // true → só visível autenticado
}

export const NAV_PRINCIPAL: ItemNav[] = [
  { rotulo: 'Início', rota: '/' },
  { rotulo: 'Fichário', rota: '/biblia' },
  { rotulo: 'Estudos', rota: '/estudos' },
  { rotulo: 'Notícias', rota: '/noticias' },
  { rotulo: 'Curiosidades', rota: '/curiosidades' },
];
```

O header filtra: `item.membros ? auth.estaAutenticado() : true`. Nenhum item usa
`membros` no M2c — é o ponto de extensão para secções/notícias reservadas a login
nos marcos seguintes. O mesmo padrão (rota com `data: { membros: true }` +
`authGuard` já existente) cobre o conteúdo em si quando existir.

### 4.4 `features/home/home.component.ts` — `bt-home`

Standalone, `OnPush`, lazy (`loadComponent`). Injeta `AuthStore`. Conteúdo
maioritariamente estático.

1. **Hero** — `<h1>` + parágrafo lead + dois CTA: `Explorar o fichário`
   (`routerLink="/biblia"`) e `Criar conta` (`routerLink="/registar"`).
2. **Destaque do Fichário** — selo "Disponível agora", título, descrição, uma faixa
   de chips com nomes de livros (**lista estática** no template — sem `LivroStore`) e
   link `Ver o fichário` → `/biblia`.
3. **Secções futuras** — 3 `secao-card` (Estudos, Notícias, Curiosidades), cada um
   com selo "Em breve", descrição curta e `routerLink` para a sua página.
4. **Teaser de membros** — título + copy + CTA `Criar conta`. Escondido com
   `@if (!auth.estaAutenticado())`.

### 4.5 `features/institucional/em-breve.component.ts` — `bt-em-breve`

Standalone, `OnPush`, lazy. Lê `ActivatedRoute.snapshot.data` (`titulo`, `descricao`).

- `<h1>{{ titulo }}</h1>` + selo "Em breve" + `<p>{{ descricao }}</p>`.
- Link `Voltar ao início` → `/`.
- Reutiliza o bloco "teaser de membros" (extrair para `shared` se compensar, ou
  duplicar o markup simples).

### 4.6 `shared/components/secao-card/secao-card.component.ts` — `bt-secao-card`

Standalone, `OnPush`. `input`s: `titulo`, `descricao`, `rota`, `disponivel` (bool).
Quando `!disponivel` mostra o selo "Em breve". É um `routerLink` para `rota`.

## 5. Rotas (`app.routes.ts`)

```
''            → HomeComponent                                   (loadComponent)
'biblia'      → bibliaRoutes                                    (inalterado)
'estudos'     → EmBreveComponent  data: { titulo, descricao }
'noticias'    → EmBreveComponent  data: { titulo, descricao }
'curiosidades'→ EmBreveComponent  data: { titulo, descricao }
'entrar'      → EntrarComponent                                 (inalterado)
'registar'    → RegistarComponent                               (inalterado)
'conta'       → contaRoutes (authGuard)                         (inalterado)
'**'          → redirectTo: ''                                  (era 'biblia')
```

## 6. Fichário — ajuste mínimo

Não é redesenho. Alterações em `biblia-lista.component.*`:

- Remover o bloco `<header class="sidebar__brand">` (marca "Fichário", subtítulo e
  `<nav class="sidebar__conta">` com Entrar/A minha conta) — passa a ser
  responsabilidade do header global. A busca fica.
- `.fichario` deixa de usar `min-height: 100vh`; a `.sidebar` fica `sticky` com
  `top` igual à altura do header (novo token `$altura-header`) e
  `max-height: calc(100vh - #{$altura-header})`.
- `AuthStore` deixa de ser usado no componente se só servia esse bloco (confirmar no
  código; a lógica de navegação por `effect` mantém-se).
- Atualizar `biblia-lista.component.spec.ts`: remover asserções sobre a marca/links
  removidos.

## 7. Identidade visual — tokens globais (opção "meio-termo")

### 7.1 `src/styles/_variables.scss` — trocar valores, manter nomes

| Var | Antes | Depois | Uso |
|---|---|---|---|
| `$parchment` | `#efe6d5` | `#faf8f4` | fundo da página (creme editorial) |
| `$parchment-2` | `#e5dac3` | `#f2ede3` | fundos alternados |
| `$ink` | `#1c2541` | `#20242e` | texto primário / títulos |
| `$brass` | `#a6763f` | `#8c2f2f` | sotaque: links, CTA, ativo (bordô) |
| `$card` | `#f7f2e7` | `#ffffff` | superfícies de card |
| `$line` | `rgba(33,29,22,.16)` | `rgba(32,36,46,.14)` | bordas/divisórias |
| `$near-black` | `#211d16` | `#20242e` | texto de corpo |
| `$sage` | `#5c6b4f` | — (mantém) | rótulos secundários |

Novo token: `$altura-header: 64px` (mobile: `56px`).
Fontes **inalteradas** — Fraunces (display) + Source Serif 4 (corpo) +
IBM Plex Mono (rótulos) servem o registo editorial. Nenhuma alteração ao `index.html`.

### 7.2 `src/styles.scss`

- `body` background → `$parchment` (agora creme); remover ou suavizar muito os dois
  `radial-gradient` (o editorial quer fundo liso).
- `:focus-visible` continua com `$brass` (agora bordô) — ok.

### 7.3 Swap brass→bordô escrito à mão

Procurar em todo o `frontend/` por valores brass hard-coded fora dos tokens
(`#a6763f`, `rgba(166, 118, 63, …)` — ex.: `biblia-lista.component.scss` linhas
~139 e ~145, hover/ativo da lista) e substituir pelo equivalente derivado do novo
`$brass` (`rgba(140, 47, 47, …)`), ou preferencialmente por `rgba($brass, …)`.
Objetivo: nenhum castanho-brass órfão depois da mudança de paleta.

## 8. Testes

Vitest (`@angular/build:unit-test`, jsdom). Convenção `Metodo_ResultadoEsperado_QuandoCondicao`.

- **`header.component.spec`** — renderiza os itens de `NAV_PRINCIPAL`; mostra
  `Entrar` + `Criar conta` quando `AuthStore` deslogado e `A minha conta` quando
  logado (mock do `AuthStore`); `menuAberto` alterna com o botão hambúrguer;
  navegar fecha o menu.
- **`footer.component.spec`** — renderiza as 3 colunas e o ano corrente; `ngSubmit`
  da newsletter não navega e não injeta `HttpClient` (nenhuma request).
- **`home.component.spec`** — renderiza os 4 blocos; o teaser de membros está
  ausente quando `AuthStore` autenticado.
- **`secao-card.component.spec`** — mostra o selo "Em breve" quando `disponivel`
  é `false`; `routerLink` aponta para `rota`.
- **`em-breve.component.spec`** — `<h1>` e descrição vêm do `data` da rota
  (`ActivatedRoute` stub).
- **`app.spec.ts`** — passa a verificar `bt-header` e `bt-footer` além do
  `router-outlet`.
- **`biblia-lista.component.spec.ts`** — remover asserções sobre a marca/links
  removidos; garantir que a busca e a lista continuam a renderizar.
- Rotas: `/estudos` resolve `EmBreveComponent` com o `data` certo (spec de routing
  ou coberto pelo teste do componente).

Teste manual (utilizador, guardrail de qualidade): `docker compose up --build`,
percorrer o header em desktop e largura mobile, confirmar que o fichário continua
a funcionar aninhado sob o header, e os estados deslogado/logado do header e do
teaser. Faz parte do "pronto para merge".

## 9. Segurança

- Sem endpoints novos → sem nova superfície de API.
- Newsletter é inerte (sem HTTP, sem persistência).
- Links de redes sociais: `rel="noopener noreferrer"`, `target="_blank"`.
- Nenhum `innerHTML` de dado bruto; nenhum `bypassSecurityTrust*`.
- Nenhum segredo — não aplicável (frontend estático).

## 10. Roadmap

Acrescentar ao `docs/ROADMAP.md`, nos Marcos, entre M2 e M3:

```
- M2c — Casca do portal. Header/footer globais, homepage, páginas "Em breve"
  (Estudos/Notícias/Curiosidades), nova identidade visual "editorial". Só frontend.
  Deixa o gancho para conteúdo reservado a membros. Planeado.
  Spec: docs/superpowers/specs/2026-08-31-m2c-casca-portal-design.md
```

## 11. Estimativa de esforço

~6 componentes novos + ~5 ficheiros editados (`app.ts`/`app.html`/`app.routes.ts`,
`_variables.scss`, `styles.scss`, `biblia-lista.*`). Uma sessão de implementação,
sem loops nem subagents. Custo esperado: baixo-moderado.

## 12. Notas para a implementação

- Usar a skill `frontend-design` ao definir a paleta final e o polish do header/
  homepage — o registo é "editorial teológico, header claro, sotaque bordô,
  fundo creme, muito respiro, serifa nos títulos".
- Mockups de referência da fase de brainstorming em
  `.superpowers/brainstorm/` (git-ignored): `visual-direction.html`,
  `homepage-estrutura.html`.
