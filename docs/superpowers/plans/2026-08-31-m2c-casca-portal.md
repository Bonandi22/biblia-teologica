# M2c — Casca do portal — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transformar o site (que hoje *é* o fichário dos 66 livros) numa casca de portal — header e footer globais, homepage, páginas "Em breve" — sem criar nenhum tipo de conteúdo novo.

**Architecture:** Só frontend Angular 22 (standalone + signals). Header e footer entram no `AppComponent`; a rota `''` passa a servir uma `HomeComponent` em vez de redirecionar para `/biblia`. Três rotas novas (`/estudos`, `/noticias`, `/curiosidades`) partilham uma `EmBreveComponent`. A nova identidade visual "editorial" aplica-se por troca dos **valores** dos tokens em `_variables.scss` (nomes iguais), mais o swap dos tints brass escritos à mão. Nenhuma alteração a Domain/Application/Infrastructure/Api, nenhum endpoint, nenhuma migração.

**Tech Stack:** Angular 22 (standalone components, signals, `@if`/`@for` control flow, `input()`), SCSS com `@use`, Vitest (`@angular/build:unit-test`, jsdom), Prettier 3.

**Spec:** `docs/superpowers/specs/2026-08-31-m2c-casca-portal-design.md`

## Global Constraints

- **Só frontend.** Nenhuma alteração fora de `frontend/biblia-teologica-web/src/` (exceto `docs/ROADMAP.md`). Sem endpoints, sem migração, sem alteração de contrato de API.
- **Componentes:** `standalone: true`, `changeDetection: ChangeDetectionStrategy.OnPush`, seletor com prefixo `bt-`.
- **Sem HTTP novo.** Nenhum componente desta leva injeta `HttpClient`. A newsletter do footer é inerte.
- **Datas:** `new Date().getFullYear()` no footer é a única data; não há lógica de `DateTime` de backend aqui.
- **RxJS em código novo:** qualquer subscrição usa `takeUntilDestroyed(this.destroyRef)`.
- **Sem `innerHTML` de dado bruto; nunca `bypassSecurityTrust*`.** Links externos: `target="_blank"` + `rel="noopener noreferrer"`.
- **Testes:** nomes `Metodo_ResultadoEsperado_QuandoCondicao` (aqui em pt: descrições curtas em português, como no resto do repo). Backend não é tocado — não correr `dotnet test`.
- **Tokens:** em `src/styles/_variables.scss` mudam os **valores**, nunca os nomes das variáveis.
- **Prettier:** `printWidth: 100`, `singleQuote: true`. Correr `npx prettier --write` nos ficheiros novos/alterados antes de cada commit.
- **Tipos:** `npx tsc --noEmit -p tsconfig.app.json` limpo antes de cada commit.
- **Comandos correm em** `frontend/biblia-teologica-web/`. Full-suite: `npm test`. Ficheiro único: `npx ng test --watch=false --include <caminho-relativo-ao-projeto>`.
- Mensagem de commit termina sempre com:
  ```
  Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
  ```

## Copy (texto final — usar exatamente isto)

**Homepage:**
- Hero `<h1>`: `Conhecer a Deus pela Sua Palavra`
- Hero lead: `Um portal de conhecimento teológico: as fichas dos 66 livros da Bíblia, estudos aprofundados e reflexões — tudo ancorado nas Escrituras.`
- Hero CTA 1: `Explorar o fichário` → `/biblia`
- Hero CTA 2: `Criar conta` → `/registar`
- Destaque selo: `Disponível agora`
- Destaque `<h2>`: `O fichário dos 66 livros`
- Destaque texto: `Cada livro do cânon protestante numa ficha estruturada — autor, data, contexto, esboço e como Cristo aparece no livro.`
- Destaque chips: `Génesis` `Salmos` `Isaías` `Mateus` `João` `Romanos` `Hebreus` `Apocalipse`
- Destaque link: `Ver o fichário` → `/biblia`
- Secção futuras `<h2>`: `A crescer`
- Teaser membros `<h2>`: `Alguns conteúdos serão só para membros`
- Teaser membros texto: `Cria uma conta gratuita para acompanhares estudos e notícias reservadas quando começarem a sair.`
- Teaser membros CTA: `Criar conta` → `/registar`

**Cards de secção (na home) e descrições das páginas "Em breve":**
| Secção | Rota | Descrição curta (card) | Descrição da página |
|---|---|---|---|
| Estudos | `/estudos` | `Artigos longos sobre teologia, história da igreja e vida cristã.` | `Artigos longos sobre teologia, história da igreja e vida cristã. Esta secção está a ser preparada.` |
| Notícias | `/noticias` | `O que se passa na igreja e no mundo, lido à luz da Bíblia.` | `Notícias da igreja e do mundo, lidas à luz da Bíblia. Esta secção está a ser preparada.` |
| Curiosidades | `/curiosidades` | `Descobertas sobre o texto bíblico, línguas originais e contexto.` | `Pequenas descobertas sobre o texto bíblico e o seu contexto. Esta secção está a ser preparada.` |

**Header:**
- Marca: `Bíblia Teológica` → `/`
- Nav: `Início` (`/`), `Fichário` (`/biblia`), `Estudos` (`/estudos`), `Notícias` (`/noticias`), `Curiosidades` (`/curiosidades`)
- Deslogado: `Entrar` (`/entrar`), `Criar conta` (`/registar`)
- Logado: `A minha conta` (`/conta`)
- Botão mobile: `aria-label="Abrir menu"` / `aria-label="Fechar menu"`

**Footer:**
- Marca: `Bíblia Teológica` — tagline: `Portal de conhecimento teológico.`
- Redes: `Facebook`, `Instagram`, `YouTube` (todas `href="#"`, `aria-label` com o nome, `rel="noopener noreferrer"`, `target="_blank"`)
- Coluna "Navegar": `Fichário` (`/biblia`), `Estudos` (`/estudos`), `Notícias` (`/noticias`), `Curiosidades` (`/curiosidades`)
- Coluna "Newsletter": título `Novidades por email`, input `placeholder="o teu email"`, botão `Subscrever`
- Mensagem ao submeter a newsletter: `A newsletter ainda não está disponível — em breve.`
- Rodapé: `© {{ ano }} Bíblia Teológica`

## Paleta / tokens (valores finais)

`src/styles/_variables.scss` — trocar valores, manter nomes; acrescentar dois tokens:

```scss
$ink: #20242e;
$parchment: #faf8f4;
$parchment-2: #f2ede3;
$brass: #8c2f2f;
$sage: #5c6b4f; // inalterado
$near-black: #20242e;
$card: #ffffff;
$line: rgba(32, 36, 46, 0.14);

$altura-header: 64px;
$altura-header-mobile: 56px;
```

Fontes (`$fonte-serif`, `$fonte-display`, `$fonte-mono`) e `$bp-mobile` — **inalterados**. `index.html` — **inalterado**.

---

## File Structure

**Criar:**
| Ficheiro | Responsabilidade |
|---|---|
| `src/app/core/layout/nav.ts` | `interface ItemNav` + `const NAV_PRINCIPAL: ItemNav[]` |
| `src/app/core/layout/header/header.component.ts` / `.html` / `.scss` / `.spec.ts` | `bt-header` — marca, nav, ações de auth, menu mobile |
| `src/app/core/layout/footer/footer.component.ts` / `.html` / `.scss` / `.spec.ts` | `bt-footer` — 3 colunas + newsletter inerte + copyright |
| `src/app/shared/components/secao-card/secao-card.component.ts` / `.scss` / `.spec.ts` | `bt-secao-card` — card de secção com selo "Em breve" opcional |
| `src/app/features/home/home.component.ts` / `.html` / `.scss` / `.spec.ts` | `bt-home` — 4 blocos da homepage |
| `src/app/features/institucional/em-breve.component.ts` / `.html` / `.scss` / `.spec.ts` | `bt-em-breve` — página partilhada, lê `titulo`/`descricao` do `data` da rota |

**Modificar:**
| Ficheiro | Alteração |
|---|---|
| `src/styles/_variables.scss` | valores dos tokens + 2 tokens novos |
| `src/styles.scss` | `body` background creme; remover os 2 `radial-gradient` |
| `src/app/features/biblia/biblia-lista/biblia-lista.component.scss` | swap `rgba(166,118,63,…)` → `rgba($brass, …)`; sidebar sticky sob o header; remover estilos `.sidebar__brand` / `.sidebar__conta` |
| `src/app/features/biblia/biblia-ficha/biblia-ficha.component.scss` | swap `rgba(28,37,65,…)` → `rgba($ink, …)` e `rgba(166,118,63,…)` → `rgba($brass, …)` |
| `src/app/features/biblia/biblia-lista/biblia-lista.component.html` | remover o `<header class="sidebar__brand">` |
| `src/app/features/biblia/biblia-lista/biblia-lista.component.ts` | remover `AuthStore` (deixa de ser usado) |
| `src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts` | remover os 2 testes de "Entrar" / "A minha conta" na sidebar |
| `src/app/app.ts` | importar `HeaderComponent`, `FooterComponent` |
| `src/app/app.html` | `<bt-header /> <main class="app-main"><router-outlet /></main> <bt-footer />` |
| `src/app/app.scss` | `.app-main { min-height: calc(100vh - #{$altura-header}); }` |
| `src/app/app.spec.ts` | assert presença de `bt-header` e `bt-footer` |
| `src/app/app.routes.ts` | `''` → `HomeComponent` (lazy); `estudos`/`noticias`/`curiosidades` → `EmBreveComponent` (eager) com `data`; `**` → `redirectTo: ''` |
| `docs/ROADMAP.md` | entrada M2c |

---

## Task 1: Nova identidade visual (tokens globais + swap brass→bordô)

**Files:**
- Modify: `frontend/biblia-teologica-web/src/styles/_variables.scss`
- Modify: `frontend/biblia-teologica-web/src/styles.scss`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista/biblia-lista.component.scss:139,145`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-ficha/biblia-ficha.component.scss:8,18`

**Interfaces:**
- Consumes: nada.
- Produces: tokens `$altura-header` (`64px`) e `$altura-header-mobile` (`56px`) em `src/styles/_variables.scss`, usados pelas Tasks 4 e 8. Valores de paleta atualizados (todas as tasks herdam).

Esta task é só estilos; a verificação é a suite existente continuar verde + o build passar + revisão visual do utilizador (Docker). Sem teste novo.

- [ ] **Step 1: Atualizar `_variables.scss`**

Substituir o topo do ficheiro (linhas 1-16) por:

```scss
// Paleta e tipografia — identidade "editorial teológico" (M2c)
$ink: #20242e;
$parchment: #faf8f4;
$parchment-2: #f2ede3;
$brass: #8c2f2f;
$sage: #5c6b4f;
$near-black: #20242e;
$card: #ffffff;
$line: rgba(32, 36, 46, 0.14);

$fonte-serif: 'Source Serif 4', Georgia, serif;
$fonte-display: 'Fraunces', Georgia, serif;
$fonte-mono: 'IBM Plex Mono', ui-monospace, monospace;

$bp-mobile: 900px;

$altura-header: 64px;
$altura-header-mobile: 56px;
```

- [ ] **Step 2: Atualizar `styles.scss`** — fundo liso, sem gradientes

Substituir o bloco `body { … }` (linhas 13-22) por:

```scss
body {
  background: $parchment;
  color: $near-black;
  font-family: $fonte-serif;
  min-height: 100vh;
  -webkit-font-smoothing: antialiased;
}
```

(Remover as duas linhas `background-image: radial-gradient(...)`.)

- [ ] **Step 3: Swap dos tints brass escritos à mão**

`biblia-lista.component.scss`:
- linha ~139: `background: rgba(166, 118, 63, 0.08);` → `background: rgba($brass, 0.08);`
- linha ~145: `background: rgba(166, 118, 63, 0.12);` → `background: rgba($brass, 0.12);`

`biblia-ficha.component.scss`:
- linha ~8: `box-shadow: 3px 4px 0 rgba(28, 37, 65, 0.05);` → `box-shadow: 3px 4px 0 rgba($ink, 0.06);`
- linha ~18: `background: linear-gradient(180deg, rgba(166, 118, 63, 0.05), transparent);` → `background: linear-gradient(180deg, rgba($brass, 0.05), transparent);`

- [ ] **Step 4: Confirmar que não sobra nenhum literal antigo**

Run: `cd frontend/biblia-teologica-web && grep -rnE "166, 118, 63|a6763f|28, 37, 65|1c2541|efe6d5|211d16|f7f2e7|e5dac3" src/`
Expected: **sem resultados** (todos os hex/rgba antigos foram substituídos por tokens).

- [ ] **Step 5: Build + suite existente**

Run: `cd frontend/biblia-teologica-web && npx ng build && npm test`
Expected: build PASS; todos os testes existentes PASS (nada foi tocado em lógica).

- [ ] **Step 6: Prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx prettier --write src/styles.scss src/styles/_variables.scss "src/app/features/biblia/**/*.scss"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/styles.scss frontend/biblia-teologica-web/src/styles/_variables.scss frontend/biblia-teologica-web/src/app/features/biblia
git commit -m "$(cat <<'EOF'
feat(web): identidade visual editorial — tokens globais + swap brass→bordô

Troca os valores dos tokens em _variables.scss (nomes iguais): fundo creme,
sotaque bordô, texto tinta escura, cards brancos. Remove os gradientes
radiais do body. Substitui os tints brass escritos à mão no fichário por
rgba($brass/$ink, …). Sem alteração de lógica.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 2: Config de navegação + `HeaderComponent`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/core/layout/nav.ts`
- Create: `frontend/biblia-teologica-web/src/app/core/layout/header/header.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/core/layout/header/header.component.html`
- Create: `frontend/biblia-teologica-web/src/app/core/layout/header/header.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/core/layout/header/header.component.spec.ts`

**Interfaces:**
- Consumes: `AuthStore` (`src/app/core/auth/auth.store.ts`) — `estaAutenticado(): Signal<boolean>`.
- Produces:
  - `nav.ts`: `interface ItemNav { rotulo: string; rota: string; membros?: boolean }` e `const NAV_PRINCIPAL: ItemNav[]`.
  - `HeaderComponent` (`selector: 'bt-header'`), sem inputs. Métodos públicos: `alternarMenu(): void`, `fecharMenu(): void`. Signal público `menuAberto: Signal<boolean>`. Usado pela Task 4.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/core/layout/header/header.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HeaderComponent } from './header.component';
import { AuthStore } from '../../auth/auth.store';

describe('HeaderComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra os itens de navegação principais', () => {
    const texto = criar().nativeElement.textContent;
    for (const rotulo of ['Início', 'Fichário', 'Estudos', 'Notícias', 'Curiosidades']) {
      expect(texto).toContain(rotulo);
    }
  });

  it('mostra "Entrar" e "Criar conta" quando anónimo', () => {
    const el = criar().nativeElement as HTMLElement;
    const entrar = el.querySelector('a[href="/entrar"]');
    const criarConta = el.querySelector('a[href="/registar"]');
    expect(entrar?.textContent?.trim()).toBe('Entrar');
    expect(criarConta?.textContent?.trim()).toBe('Criar conta');
    expect(el.querySelector('a[href="/conta"]')).toBeNull();
  });

  it('mostra "A minha conta" quando autenticado', () => {
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/conta"]')?.textContent?.trim()).toBe('A minha conta');
    expect(el.querySelector('a[href="/entrar"]')).toBeNull();
  });

  it('alterna o menu mobile com o botão', () => {
    const fixture = criar();
    const cmp = fixture.componentInstance;
    const botao = fixture.nativeElement.querySelector('.header__toggle') as HTMLButtonElement;
    expect(cmp.menuAberto()).toBe(false);
    botao.click();
    fixture.detectChanges();
    expect(cmp.menuAberto()).toBe(true);
    expect(botao.getAttribute('aria-expanded')).toBe('true');
  });

  it('fecharMenu() fecha o menu', () => {
    const cmp = criar().componentInstance;
    cmp.alternarMenu();
    expect(cmp.menuAberto()).toBe(true);
    cmp.fecharMenu();
    expect(cmp.menuAberto()).toBe(false);
  });
});
```

- [ ] **Step 2: Correr o teste — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/core/layout/header/header.component.spec.ts`
Expected: FAIL — `Cannot find module './header.component'`.

- [ ] **Step 3: Criar `nav.ts`**

`src/app/core/layout/nav.ts`:

```ts
export interface ItemNav {
  rotulo: string;
  rota: string;
  /** true → só visível quando autenticado. Ponto de extensão para secções reservadas a membros. */
  membros?: boolean;
}

export const NAV_PRINCIPAL: ItemNav[] = [
  { rotulo: 'Início', rota: '/' },
  { rotulo: 'Fichário', rota: '/biblia' },
  { rotulo: 'Estudos', rota: '/estudos' },
  { rotulo: 'Notícias', rota: '/noticias' },
  { rotulo: 'Curiosidades', rota: '/curiosidades' },
];
```

- [ ] **Step 4: Criar o componente**

`src/app/core/layout/header/header.component.ts`:

```ts
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthStore } from '../../auth/auth.store';
import { NAV_PRINCIPAL } from '../nav';

@Component({
  selector: 'bt-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  host: {
    '(document:keydown.escape)': 'fecharMenu()',
  },
})
export class HeaderComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly autenticado = this.auth.estaAutenticado;
  private readonly _menuAberto = signal(false);
  readonly menuAberto = this._menuAberto.asReadonly();

  readonly itens = computed(() =>
    NAV_PRINCIPAL.filter((item) => (item.membros ? this.autenticado() : true)),
  );

  constructor() {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this._menuAberto.set(false));
  }

  alternarMenu(): void {
    this._menuAberto.update((v) => !v);
  }

  fecharMenu(): void {
    this._menuAberto.set(false);
  }
}
```

`src/app/core/layout/header/header.component.html`:

```html
<header class="header">
  <div class="header__inner">
    <a class="header__marca" routerLink="/" (click)="fecharMenu()">Bíblia Teológica</a>

    <button
      class="header__toggle"
      type="button"
      [attr.aria-expanded]="menuAberto()"
      [attr.aria-label]="menuAberto() ? 'Fechar menu' : 'Abrir menu'"
      (click)="alternarMenu()"
    >
      <span class="header__toggle-barra"></span>
    </button>

    <nav class="header__nav" [class.header__nav--aberto]="menuAberto()">
      <ul class="header__lista">
        @for (item of itens(); track item.rota) {
          <li>
            <a
              [routerLink]="item.rota"
              routerLinkActive="ativo"
              [routerLinkActiveOptions]="{ exact: item.rota === '/' }"
              (click)="fecharMenu()"
              >{{ item.rotulo }}</a
            >
          </li>
        }
      </ul>

      <div class="header__acoes">
        @if (autenticado()) {
          <a routerLink="/conta" (click)="fecharMenu()">A minha conta</a>
        } @else {
          <a routerLink="/entrar" (click)="fecharMenu()">Entrar</a>
          <a class="header__cta" routerLink="/registar" (click)="fecharMenu()">Criar conta</a>
        }
      </div>
    </nav>
  </div>
</header>
```

`src/app/core/layout/header/header.component.scss`:

```scss
@use '../../../../styles/variables' as *;
@use '../../../../styles/mixins' as *;

.header {
  position: sticky;
  top: 0;
  z-index: 20;
  background: #fff;
  border-bottom: 1px solid $line;
}

.header__inner {
  display: flex;
  align-items: center;
  gap: 24px;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  height: $altura-header;

  @include mobile {
    height: $altura-header-mobile;
    padding: 0 18px;
  }
}

.header__marca {
  font-family: $fonte-display;
  font-weight: 700;
  font-size: 1.15rem;
  color: $ink;
  text-decoration: none;
  white-space: nowrap;
}

.header__toggle {
  display: none;
  margin-left: auto;
  width: 40px;
  height: 40px;
  border: 1px solid $line;
  border-radius: 3px;
  background: #fff;
  cursor: pointer;

  @include mobile {
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  &-barra,
  &-barra::before,
  &-barra::after {
    content: '';
    display: block;
    width: 16px;
    height: 2px;
    background: $ink;
    position: relative;
  }

  &-barra::before {
    position: absolute;
    top: -5px;
  }
  &-barra::after {
    position: absolute;
    top: 5px;
  }
}

.header__nav {
  display: flex;
  flex: 1;
  align-items: center;
  gap: 24px;

  @include mobile {
    display: none;
    position: absolute;
    left: 0;
    right: 0;
    top: $altura-header-mobile;
    flex-direction: column;
    align-items: stretch;
    gap: 0;
    padding: 8px 18px 16px;
    background: #fff;
    border-bottom: 1px solid $line;

    &--aberto {
      display: flex;
    }
  }
}

.header__lista {
  display: flex;
  gap: 20px;
  list-style: none;
  margin: 0;
  padding: 0;

  @include mobile {
    flex-direction: column;
    gap: 0;
  }

  a {
    font-family: $fonte-mono;
    font-size: 0.72rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: $ink;
    text-decoration: none;
    padding: 6px 0;
    display: inline-block;

    &:hover,
    &.ativo {
      color: $brass;
    }

    @include mobile {
      padding: 10px 0;
      border-bottom: 1px solid $line;
    }
  }
}

.header__acoes {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-left: auto;

  @include mobile {
    margin-left: 0;
    margin-top: 10px;
  }

  a {
    font-family: $fonte-mono;
    font-size: 0.72rem;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    color: $ink;
    text-decoration: none;

    &:hover {
      color: $brass;
    }
  }
}

.header__cta {
  padding: 7px 14px;
  border: 1px solid $brass;
  border-radius: 3px;
  color: $brass !important;

  &:hover {
    background: $brass;
    color: #fff !important;
  }
}
```

- [ ] **Step 5: Correr o teste — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/core/layout/header/header.component.spec.ts`
Expected: PASS (5 testes).

- [ ] **Step 6: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/core/layout/**/*.{ts,html,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/core/layout
git commit -m "$(cat <<'EOF'
feat(web): HeaderComponent global + config de navegação

bt-header: marca, nav principal a partir de NAV_PRINCIPAL (com flag
`membros` como gancho para secções futuras reservadas a login), ações
Entrar/Criar conta vs A minha conta conforme AuthStore, e menu hambúrguer
no mobile que fecha ao navegar e com Escape.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 3: `FooterComponent`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/core/layout/footer/footer.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/core/layout/footer/footer.component.html`
- Create: `frontend/biblia-teologica-web/src/app/core/layout/footer/footer.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/core/layout/footer/footer.component.spec.ts`

**Interfaces:**
- Consumes: nada.
- Produces: `FooterComponent` (`selector: 'bt-footer'`), sem inputs. Usado pela Task 4.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/core/layout/footer/footer.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { FooterComponent } from './footer.component';

describe('FooterComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [FooterComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(FooterComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra os links de navegação do rodapé', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/biblia"]')).toBeTruthy();
    expect(el.querySelector('a[href="/estudos"]')).toBeTruthy();
    expect(el.querySelector('a[href="/noticias"]')).toBeTruthy();
    expect(el.querySelector('a[href="/curiosidades"]')).toBeTruthy();
  });

  it('mostra as redes sociais com aria-label e rel seguro', () => {
    const el = criar().nativeElement as HTMLElement;
    for (const rede of ['Facebook', 'Instagram', 'YouTube']) {
      const link = el.querySelector(`a[aria-label="${rede}"]`) as HTMLAnchorElement;
      expect(link).toBeTruthy();
      expect(link.getAttribute('rel')).toContain('noopener');
    }
  });

  it('mostra o ano corrente no copyright', () => {
    expect(criar().nativeElement.textContent).toContain(String(new Date().getFullYear()));
  });

  it('submeter a newsletter não navega e mostra aviso de indisponível', () => {
    const fixture = criar();
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(fixture.componentInstance.mensagem()).toContain('ainda não está disponível');
    expect(fixture.nativeElement.textContent).toContain('ainda não está disponível');
  });
});
```

- [ ] **Step 2: Correr o teste — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/core/layout/footer/footer.component.spec.ts`
Expected: FAIL — `Cannot find module './footer.component'`.

- [ ] **Step 3: Criar o componente**

`src/app/core/layout/footer/footer.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'bt-footer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  readonly ano = new Date().getFullYear();

  private readonly _mensagem = signal('');
  readonly mensagem = this._mensagem.asReadonly();

  readonly redes = [
    { nome: 'Facebook', url: '#' },
    { nome: 'Instagram', url: '#' },
    { nome: 'YouTube', url: '#' },
  ];

  submeterNewsletter(evento: Event): void {
    evento.preventDefault();
    this._mensagem.set('A newsletter ainda não está disponível — em breve.');
  }
}
```

`src/app/core/layout/footer/footer.component.html`:

```html
<footer class="rodape">
  <div class="rodape__inner">
    <div class="rodape__coluna rodape__coluna--marca">
      <p class="rodape__marca">Bíblia Teológica</p>
      <p class="rodape__tagline">Portal de conhecimento teológico.</p>
      <ul class="rodape__redes">
        @for (rede of redes; track rede.nome) {
          <li>
            <a [href]="rede.url" [attr.aria-label]="rede.nome" target="_blank" rel="noopener noreferrer"
              >{{ rede.nome }}</a
            >
          </li>
        }
      </ul>
    </div>

    <nav class="rodape__coluna">
      <h2 class="rodape__titulo">Navegar</h2>
      <ul>
        <li><a routerLink="/biblia">Fichário</a></li>
        <li><a routerLink="/estudos">Estudos</a></li>
        <li><a routerLink="/noticias">Notícias</a></li>
        <li><a routerLink="/curiosidades">Curiosidades</a></li>
      </ul>
    </nav>

    <div class="rodape__coluna">
      <h2 class="rodape__titulo">Novidades por email</h2>
      <form class="rodape__newsletter" (submit)="submeterNewsletter($event)">
        <input type="email" name="email" placeholder="o teu email" autocomplete="email" />
        <button type="submit">Subscrever</button>
      </form>
      @if (mensagem()) {
        <p class="rodape__aviso" role="status">{{ mensagem() }}</p>
      } @else {
        <p class="rodape__aviso">A newsletter ainda não está disponível — em breve.</p>
      }
    </div>
  </div>

  <p class="rodape__copy">© {{ ano }} Bíblia Teológica</p>
</footer>
```

> Nota: o aviso "ainda não está disponível" aparece sempre (honesto); ao submeter, `mensagem()` passa a preenchido e substitui o texto estático pelo mesmo aviso com `role="status"`. O teste do Step 1 cobre ambos.

`src/app/core/layout/footer/footer.component.scss`:

```scss
@use '../../../../styles/variables' as *;
@use '../../../../styles/mixins' as *;

.rodape {
  margin-top: 80px;
  background: $ink;
  color: rgba(255, 255, 255, 0.82);
  font-family: $fonte-serif;
}

.rodape__inner {
  display: grid;
  grid-template-columns: 1.6fr 1fr 1.2fr;
  gap: 40px;
  max-width: 1200px;
  margin: 0 auto;
  padding: 48px 24px 32px;

  @include mobile {
    grid-template-columns: 1fr;
    gap: 28px;
    padding: 36px 18px 24px;
  }
}

.rodape__marca {
  font-family: $fonte-display;
  font-weight: 700;
  font-size: 1.1rem;
  color: #fff;
  margin: 0 0 6px;
}

.rodape__tagline {
  margin: 0 0 14px;
  font-size: 0.9rem;
}

.rodape__redes {
  display: flex;
  gap: 16px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.rodape__titulo {
  font-family: $fonte-mono;
  font-size: 0.68rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.6);
  margin: 0 0 12px;
}

.rodape__coluna ul {
  list-style: none;
  margin: 0;
  padding: 0;
}

.rodape__coluna li {
  margin-bottom: 8px;
}

.rodape a {
  color: rgba(255, 255, 255, 0.82);
  text-decoration: none;

  &:hover {
    color: #fff;
    text-decoration: underline;
  }
}

.rodape__newsletter {
  display: flex;
  gap: 8px;

  input {
    flex: 1;
    min-width: 0;
    padding: 8px 10px;
    border: 1px solid rgba(255, 255, 255, 0.25);
    border-radius: 3px;
    background: rgba(255, 255, 255, 0.06);
    color: #fff;
    font-family: $fonte-serif;
    font-size: 0.9rem;
  }

  button {
    padding: 8px 14px;
    border: 0;
    border-radius: 3px;
    background: $brass;
    color: #fff;
    font-family: $fonte-mono;
    font-size: 0.68rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    cursor: pointer;
  }
}

.rodape__aviso {
  margin: 10px 0 0;
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.55);
}

.rodape__copy {
  max-width: 1200px;
  margin: 0 auto;
  padding: 16px 24px 28px;
  border-top: 1px solid rgba(255, 255, 255, 0.12);
  font-family: $fonte-mono;
  font-size: 0.68rem;
  letter-spacing: 0.06em;
  color: rgba(255, 255, 255, 0.5);

  @include mobile {
    padding: 16px 18px 24px;
  }
}
```

- [ ] **Step 4: Correr o teste — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/core/layout/footer/footer.component.spec.ts`
Expected: PASS (4 testes).

- [ ] **Step 5: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/core/layout/footer/**/*.{ts,html,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/core/layout/footer
git commit -m "$(cat <<'EOF'
feat(web): FooterComponent global (sobre + navegar + newsletter inerte)

bt-footer: 3 colunas (marca/redes, navegar, newsletter) + copyright com
o ano corrente. A newsletter não faz HTTP — ao submeter mostra um aviso
de indisponível.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 4: Montar header/footer no `AppComponent`

**Files:**
- Modify: `frontend/biblia-teologica-web/src/app/app.ts`
- Modify: `frontend/biblia-teologica-web/src/app/app.html`
- Modify: `frontend/biblia-teologica-web/src/app/app.scss`
- Modify: `frontend/biblia-teologica-web/src/app/app.spec.ts`

**Interfaces:**
- Consumes: `HeaderComponent` (Task 2), `FooterComponent` (Task 3), token `$altura-header` (Task 1).
- Produces: casca visível em todas as rotas. Nada consumido por tasks posteriores.

- [ ] **Step 1: Atualizar `app.spec.ts` (teste que falha)**

Substituir o corpo do `describe` por:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('cria o componente raiz', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('rende o cabeçalho e o rodapé globais', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('bt-header')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-footer')).toBeTruthy();
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/app.spec.ts`
Expected: FAIL — `bt-header` não encontrado.

- [ ] **Step 3: Atualizar `app.ts`**

```ts
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './core/layout/header/header.component';
import { FooterComponent } from './core/layout/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, HeaderComponent, FooterComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
```

- [ ] **Step 4: Atualizar `app.html`**

```html
<bt-header />
<main class="app-main">
  <router-outlet />
</main>
<bt-footer />
```

- [ ] **Step 5: Atualizar `app.scss`**

```scss
@use '../styles/variables' as *;

.app-main {
  display: block;
  min-height: calc(100vh - #{$altura-header});
}
```

- [ ] **Step 6: Correr — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/app.spec.ts`
Expected: PASS (2 testes).

- [ ] **Step 7: Suite completa (o header aparece agora em todos os specs que montam `App`)**

Run: `cd frontend/biblia-teologica-web && npm test`
Expected: PASS. Se algum spec falhar por causa do header (ex.: `provideRouter` em falta), corrigir esse spec adicionando `provideRouter([])`.

- [ ] **Step 8: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/app.*"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/app.ts frontend/biblia-teologica-web/src/app/app.html frontend/biblia-teologica-web/src/app/app.scss frontend/biblia-teologica-web/src/app/app.spec.ts
git commit -m "$(cat <<'EOF'
feat(web): montar header e footer globais no AppComponent

app.html passa a <bt-header/> + <main> + <bt-footer/>. Todas as rotas
renderizam dentro da casca.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 5: `SecaoCardComponent`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/components/secao-card/secao-card.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/shared/components/secao-card/secao-card.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/shared/components/secao-card/secao-card.component.spec.ts`

**Interfaces:**
- Consumes: nada.
- Produces: `SecaoCardComponent` (`selector: 'bt-secao-card'`). Inputs:
  - `titulo = input.required<string>()`
  - `descricao = input.required<string>()`
  - `rota = input.required<string>()`
  - `disponivel = input<boolean>(false)`
  Usado pela Task 6.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/shared/components/secao-card/secao-card.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SecaoCardComponent } from './secao-card.component';

describe('SecaoCardComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SecaoCardComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = (disponivel: boolean) => {
    const fixture = TestBed.createComponent(SecaoCardComponent);
    fixture.componentRef.setInput('titulo', 'Estudos');
    fixture.componentRef.setInput('descricao', 'Artigos longos.');
    fixture.componentRef.setInput('rota', '/estudos');
    fixture.componentRef.setInput('disponivel', disponivel);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o selo "Em breve" quando não disponível', () => {
    expect(criar(false).nativeElement.textContent).toContain('Em breve');
  });

  it('esconde o selo quando disponível', () => {
    expect(criar(true).nativeElement.textContent).not.toContain('Em breve');
  });

  it('liga para a rota indicada', () => {
    const link = criar(false).nativeElement.querySelector('a') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/estudos');
    expect(link.textContent).toContain('Estudos');
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/components/secao-card/secao-card.component.spec.ts`
Expected: FAIL — módulo não encontrado.

- [ ] **Step 3: Criar o componente** (template inline, como `badge-secao`)

`src/app/shared/components/secao-card/secao-card.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'bt-secao-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <a class="secao-card" [routerLink]="rota()">
      @if (!disponivel()) {
        <span class="secao-card__selo">Em breve</span>
      }
      <h3 class="secao-card__titulo">{{ titulo() }}</h3>
      <p class="secao-card__descricao">{{ descricao() }}</p>
    </a>
  `,
  styleUrl: './secao-card.component.scss',
})
export class SecaoCardComponent {
  readonly titulo = input.required<string>();
  readonly descricao = input.required<string>();
  readonly rota = input.required<string>();
  readonly disponivel = input<boolean>(false);
}
```

`src/app/shared/components/secao-card/secao-card.component.scss`:

```scss
@use '../../../../styles/variables' as *;

.secao-card {
  display: block;
  padding: 22px;
  background: $card;
  border: 1px solid $line;
  border-radius: 4px;
  text-decoration: none;
  color: inherit;
  transition: border-color 0.15s ease;

  &:hover {
    border-color: $brass;
  }
}

.secao-card__selo {
  display: inline-block;
  margin-bottom: 10px;
  padding: 3px 8px;
  border-radius: 3px;
  background: $parchment-2;
  font-family: $fonte-mono;
  font-size: 0.6rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: $sage;
}

.secao-card__titulo {
  margin: 0 0 6px;
  font-family: $fonte-display;
  font-weight: 600;
  font-size: 1.25rem;
  color: $ink;
}

.secao-card__descricao {
  margin: 0;
  font-size: 0.95rem;
  line-height: 1.5;
  color: $near-black;
}
```

- [ ] **Step 4: Correr — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/components/secao-card/secao-card.component.spec.ts`
Expected: PASS (3 testes).

- [ ] **Step 5: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/shared/components/secao-card/**/*.{ts,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/shared/components/secao-card
git commit -m "$(cat <<'EOF'
feat(web): SecaoCardComponent (card de secção com selo "Em breve")

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 6: `HomeComponent` + rota `''`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/features/home/home.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/features/home/home.component.html`
- Create: `frontend/biblia-teologica-web/src/app/features/home/home.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/features/home/home.component.spec.ts`
- Modify: `frontend/biblia-teologica-web/src/app/app.routes.ts`

**Interfaces:**
- Consumes: `SecaoCardComponent` (Task 5), `AuthStore` (`estaAutenticado`).
- Produces: `HomeComponent` (`selector: 'bt-home'`), lazy via `loadComponent`. Rota `''` passa a servi-lo.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/features/home/home.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './home.component';
import { AuthStore } from '../../core/auth/auth.store';

describe('HomeComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o título do hero e os CTA', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('Conhecer a Deus pela Sua Palavra');
    expect(el.querySelector('a[href="/biblia"]')).toBeTruthy();
    expect(el.querySelector('a[href="/registar"]')).toBeTruthy();
  });

  it('mostra os três cards de secção futura', () => {
    const texto = criar().nativeElement.textContent;
    expect(texto).toContain('Estudos');
    expect(texto).toContain('Notícias');
    expect(texto).toContain('Curiosidades');
    expect(criar().nativeElement.querySelectorAll('bt-secao-card').length).toBe(3);
  });

  it('mostra o teaser de membros quando anónimo', () => {
    expect(criar().nativeElement.textContent).toContain('Alguns conteúdos serão só para membros');
  });

  it('esconde o teaser de membros quando autenticado', () => {
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    expect(criar().nativeElement.textContent).not.toContain(
      'Alguns conteúdos serão só para membros',
    );
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/home/home.component.spec.ts`
Expected: FAIL — módulo não encontrado.

- [ ] **Step 3: Criar o componente**

`src/app/features/home/home.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { SecaoCardComponent } from '../../shared/components/secao-card/secao-card.component';

interface CardSecao {
  titulo: string;
  descricao: string;
  rota: string;
}

@Component({
  selector: 'bt-home',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, SecaoCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  private readonly auth = inject(AuthStore);
  readonly autenticado = this.auth.estaAutenticado;

  readonly livrosDestaque = [
    'Génesis',
    'Salmos',
    'Isaías',
    'Mateus',
    'João',
    'Romanos',
    'Hebreus',
    'Apocalipse',
  ];

  readonly secoes: CardSecao[] = [
    {
      titulo: 'Estudos',
      descricao: 'Artigos longos sobre teologia, história da igreja e vida cristã.',
      rota: '/estudos',
    },
    {
      titulo: 'Notícias',
      descricao: 'O que se passa na igreja e no mundo, lido à luz da Bíblia.',
      rota: '/noticias',
    },
    {
      titulo: 'Curiosidades',
      descricao: 'Descobertas sobre o texto bíblico, línguas originais e contexto.',
      rota: '/curiosidades',
    },
  ];
}
```

`src/app/features/home/home.component.html`:

```html
<section class="hero">
  <div class="hero__inner">
    <h1 class="hero__titulo">Conhecer a Deus pela Sua Palavra</h1>
    <p class="hero__lead">
      Um portal de conhecimento teológico: as fichas dos 66 livros da Bíblia, estudos aprofundados
      e reflexões — tudo ancorado nas Escrituras.
    </p>
    <div class="hero__acoes">
      <a class="btn btn--primario" routerLink="/biblia">Explorar o fichário</a>
      <a class="btn btn--fantasma" routerLink="/registar">Criar conta</a>
    </div>
  </div>
</section>

<section class="secao destaque">
  <div class="secao__inner">
    <span class="destaque__selo">Disponível agora</span>
    <h2 class="destaque__titulo">O fichário dos 66 livros</h2>
    <p class="destaque__texto">
      Cada livro do cânon protestante numa ficha estruturada — autor, data, contexto, esboço e como
      Cristo aparece no livro.
    </p>
    <ul class="destaque__chips">
      @for (livro of livrosDestaque; track livro) {
        <li>{{ livro }}</li>
      }
    </ul>
    <a class="destaque__link" routerLink="/biblia">Ver o fichário →</a>
  </div>
</section>

<section class="secao">
  <div class="secao__inner">
    <h2 class="secao__titulo">A crescer</h2>
    <div class="grelha">
      @for (secao of secoes; track secao.rota) {
        <bt-secao-card
          [titulo]="secao.titulo"
          [descricao]="secao.descricao"
          [rota]="secao.rota"
          [disponivel]="false"
        />
      }
    </div>
  </div>
</section>

@if (!autenticado()) {
  <section class="secao membros">
    <div class="secao__inner">
      <h2 class="secao__titulo">Alguns conteúdos serão só para membros</h2>
      <p>
        Cria uma conta gratuita para acompanhares estudos e notícias reservadas quando começarem a
        sair.
      </p>
      <a class="btn btn--primario" routerLink="/registar">Criar conta</a>
    </div>
  </section>
}
```

`src/app/features/home/home.component.scss`:

```scss
@use '../../../styles/variables' as *;
@use '../../../styles/mixins' as *;

.secao__inner,
.hero__inner {
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 24px;

  @include mobile {
    padding: 0 18px;
  }
}

.hero {
  padding: 72px 0 48px;

  @include mobile {
    padding: 44px 0 32px;
  }
}

.hero__titulo {
  margin: 0 0 16px;
  font-family: $fonte-display;
  font-weight: 700;
  font-size: clamp(2.2rem, 5vw, 3.4rem);
  line-height: 1.08;
  color: $ink;
}

.hero__lead {
  max-width: 60ch;
  margin: 0 0 28px;
  font-size: 1.15rem;
  line-height: 1.6;
}

.hero__acoes {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.btn {
  display: inline-block;
  padding: 11px 20px;
  border-radius: 3px;
  font-family: $fonte-mono;
  font-size: 0.72rem;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  text-decoration: none;
}

.btn--primario {
  background: $brass;
  color: #fff;

  &:hover {
    background: darken($brass, 6%);
  }
}

.btn--fantasma {
  border: 1px solid $brass;
  color: $brass;

  &:hover {
    background: $brass;
    color: #fff;
  }
}

.secao {
  padding: 40px 0;
}

.secao__titulo {
  margin: 0 0 20px;
  font-family: $fonte-display;
  font-weight: 600;
  font-size: 1.6rem;
  color: $ink;
}

.destaque {
  background: $parchment-2;
}

.destaque__selo {
  display: inline-block;
  margin-bottom: 12px;
  padding: 4px 10px;
  border-radius: 3px;
  background: $card;
  border: 1px solid $line;
  font-family: $fonte-mono;
  font-size: 0.62rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: $sage;
}

.destaque__titulo {
  margin: 0 0 10px;
  font-family: $fonte-display;
  font-weight: 700;
  font-size: clamp(1.8rem, 3.5vw, 2.4rem);
  color: $ink;
}

.destaque__texto {
  max-width: 58ch;
  margin: 0 0 18px;
  font-size: 1.05rem;
  line-height: 1.6;
}

.destaque__chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  list-style: none;
  margin: 0 0 20px;
  padding: 0;

  li {
    padding: 6px 10px;
    background: $card;
    border: 1px solid $line;
    border-radius: 3px;
    font-size: 0.85rem;
  }
}

.destaque__link {
  font-family: $fonte-mono;
  font-size: 0.75rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $brass;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
}

.grelha {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;

  @include mobile {
    grid-template-columns: 1fr;
  }
}

.membros {
  background: $parchment-2;

  p {
    max-width: 56ch;
    margin: 0 0 18px;
    font-size: 1.05rem;
    line-height: 1.6;
  }
}
```

> Se `darken()` do Sass emitir aviso de depreciação no build, trocar `darken($brass, 6%)` por `color.adjust($brass, $lightness: -6%)` e adicionar `@use 'sass:color';` no topo. Verificar no Step 5.

- [ ] **Step 4: Ligar a rota `''`** — `src/app/app.routes.ts`

Substituir a primeira entrada (`{ path: '', pathMatch: 'full', redirectTo: 'biblia' }`) por:

```ts
{
  path: '',
  pathMatch: 'full',
  loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
},
```

E trocar a última entrada `{ path: '**', redirectTo: 'biblia' }` por `{ path: '**', redirectTo: '' }`.

Também **remover** o `effect` de auto-navegação no `BibliaListaComponent`? **Não** — fica na Task 8. Aqui só a rota.

- [ ] **Step 5: Correr o teste do componente + build**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/home/home.component.spec.ts && npx ng build`
Expected: PASS (4 testes); build PASS sem avisos de Sass (se houver, aplicar a nota do Step 3).

- [ ] **Step 6: Suite completa**

Run: `cd frontend/biblia-teologica-web && npm test`
Expected: PASS. O `biblia-lista.spec` ainda passa (a rota `''` mudou mas o spec usa `provideRouter([])` próprio).

- [ ] **Step 7: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/features/home/**/*.{ts,html,scss}" src/app/app.routes.ts
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/features/home frontend/biblia-teologica-web/src/app/app.routes.ts
git commit -m "$(cat <<'EOF'
feat(web): HomeComponent na rota raiz

Homepage com 4 blocos: hero, destaque do fichário (chips estáticos),
grelha de secções futuras (Estudos/Notícias/Curiosidades) e teaser de
membros escondido quando autenticado. A rota '' deixa de redirecionar
para /biblia.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 7: `EmBreveComponent` + rotas `/estudos` `/noticias` `/curiosidades`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/features/institucional/em-breve.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/features/institucional/em-breve.component.html`
- Create: `frontend/biblia-teologica-web/src/app/features/institucional/em-breve.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/features/institucional/em-breve.component.spec.ts`
- Modify: `frontend/biblia-teologica-web/src/app/app.routes.ts`

**Interfaces:**
- Consumes: `ActivatedRoute` (`snapshot.data.titulo`, `snapshot.data.descricao`).
- Produces: `EmBreveComponent` (`selector: 'bt-em-breve'`), **eager** (`component:` nas rotas, não `loadComponent` — é pequeno e partilhado por 3 rotas).

- [ ] **Step 1: Escrever os testes que falham**

`src/app/features/institucional/em-breve.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { EmBreveComponent } from './em-breve.component';
import { routes } from '../../app.routes';

describe('EmBreveComponent', () => {
  const montar = (data: Record<string, unknown>) => {
    TestBed.configureTestingModule({
      imports: [EmBreveComponent],
      providers: [provideRouter([]), { provide: ActivatedRoute, useValue: { snapshot: { data } } }],
    });
    const fixture = TestBed.createComponent(EmBreveComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o título e a descrição vindos do data da rota', () => {
    const el = montar({ titulo: 'Estudos', descricao: 'Descrição de teste.' }).nativeElement;
    expect(el.querySelector('h1')?.textContent).toContain('Estudos');
    expect(el.textContent).toContain('Descrição de teste.');
    expect(el.textContent).toContain('Em breve');
  });
});

describe('rotas "Em breve"', () => {
  it('estudos, noticias e curiosidades apontam para EmBreveComponent com data', () => {
    for (const [path, titulo] of [
      ['estudos', 'Estudos'],
      ['noticias', 'Notícias'],
      ['curiosidades', 'Curiosidades'],
    ]) {
      const rota = routes.find((r) => r.path === path);
      expect(rota?.component).toBe(EmBreveComponent);
      expect((rota?.data as { titulo: string }).titulo).toBe(titulo);
      expect(typeof (rota?.data as { descricao: string }).descricao).toBe('string');
    }
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/institucional/em-breve.component.spec.ts`
Expected: FAIL — módulo não encontrado.

- [ ] **Step 3: Criar o componente**

`src/app/features/institucional/em-breve.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'bt-em-breve',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './em-breve.component.html',
  styleUrl: './em-breve.component.scss',
})
export class EmBreveComponent {
  private readonly rota = inject(ActivatedRoute);

  readonly titulo: string = this.rota.snapshot.data['titulo'] ?? 'Em breve';
  readonly descricao: string = this.rota.snapshot.data['descricao'] ?? '';
}
```

`src/app/features/institucional/em-breve.component.html`:

```html
<section class="em-breve">
  <span class="em-breve__selo">Em breve</span>
  <h1 class="em-breve__titulo">{{ titulo }}</h1>
  <p class="em-breve__descricao">{{ descricao }}</p>
  <a class="em-breve__voltar" routerLink="/">← Voltar ao início</a>
</section>
```

`src/app/features/institucional/em-breve.component.scss`:

```scss
@use '../../../styles/variables' as *;
@use '../../../styles/mixins' as *;

.em-breve {
  max-width: 640px;
  margin: 0 auto;
  padding: 80px 24px;
  text-align: center;

  @include mobile {
    padding: 56px 18px;
  }
}

.em-breve__selo {
  display: inline-block;
  margin-bottom: 16px;
  padding: 4px 12px;
  border-radius: 3px;
  background: $parchment-2;
  border: 1px solid $line;
  font-family: $fonte-mono;
  font-size: 0.65rem;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: $sage;
}

.em-breve__titulo {
  margin: 0 0 14px;
  font-family: $fonte-display;
  font-weight: 700;
  font-size: clamp(2rem, 4vw, 2.8rem);
  color: $ink;
}

.em-breve__descricao {
  margin: 0 0 28px;
  font-size: 1.1rem;
  line-height: 1.6;
}

.em-breve__voltar {
  font-family: $fonte-mono;
  font-size: 0.72rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $brass;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
}
```

- [ ] **Step 4: Ligar as rotas** — `src/app/app.routes.ts`

Adicionar o import no topo:

```ts
import { EmBreveComponent } from './features/institucional/em-breve.component';
```

Acrescentar estas três entradas **depois** da rota `biblia` e **antes** de `entrar`:

```ts
{
  path: 'estudos',
  component: EmBreveComponent,
  data: {
    titulo: 'Estudos',
    descricao:
      'Artigos longos sobre teologia, história da igreja e vida cristã. Esta secção está a ser preparada.',
  },
},
{
  path: 'noticias',
  component: EmBreveComponent,
  data: {
    titulo: 'Notícias',
    descricao:
      'Notícias da igreja e do mundo, lidas à luz da Bíblia. Esta secção está a ser preparada.',
  },
},
{
  path: 'curiosidades',
  component: EmBreveComponent,
  data: {
    titulo: 'Curiosidades',
    descricao:
      'Pequenas descobertas sobre o texto bíblico e o seu contexto. Esta secção está a ser preparada.',
  },
},
```

- [ ] **Step 5: Correr o teste — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/institucional/em-breve.component.spec.ts`
Expected: PASS (2 testes).

- [ ] **Step 6: Suite completa + build**

Run: `cd frontend/biblia-teologica-web && npm test && npx ng build`
Expected: PASS.

- [ ] **Step 7: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/features/institucional/**/*.{ts,html,scss}" src/app/app.routes.ts
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/features/institucional frontend/biblia-teologica-web/src/app/app.routes.ts
git commit -m "$(cat <<'EOF'
feat(web): páginas "Em breve" para Estudos, Notícias e Curiosidades

EmBreveComponent partilhado; título e descrição vêm do data da rota.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 8: Encaixar o fichário sob o header global

**Files:**
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista/biblia-lista.component.html`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista/biblia-lista.component.ts`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista/biblia-lista.component.scss`
- Modify: `frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts`

**Interfaces:**
- Consumes: token `$altura-header` / `$altura-header-mobile` (Task 1).
- Produces: nada para tasks posteriores.

- [ ] **Step 1: Ajustar o spec (testes que falham / desaparecem)**

Em `biblia-lista.component.spec.ts`:
- Remover os dois testes `it('mostra "Entrar" no cabeçalho quando anónimo', …)` e `it('mostra "A minha conta" no cabeçalho quando autenticado', …)` (linhas ~47-71).
- Remover o import `import { AuthStore } from '../../../core/auth/auth.store';` (deixa de ser usado).
- Manter os testes `lista os livros…` e `filtra pela busca`.
- Acrescentar este teste:

```ts
it('não repete a marca do site na sidebar (vem do header global)', () => {
  const fixture = TestBed.createComponent(BibliaListaComponent);
  fixture.detectChanges();
  http.expectOne('/api/livros').flush([item('genesis', 1)]);
  fixture.detectChanges();
  expect(fixture.nativeElement.querySelector('.sidebar__brand')).toBeNull();
  expect(fixture.nativeElement.querySelector('.sidebar__conta')).toBeNull();
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts`
Expected: FAIL — `.sidebar__brand` ainda existe.

- [ ] **Step 3: Remover o bloco de marca do template**

Em `biblia-lista.component.html`, apagar todo o elemento `<header class="sidebar__brand"> … </header>` (linhas 3-13), deixando `<aside class="sidebar">` a começar diretamente com o `<input class="sidebar__busca" …>`.

- [ ] **Step 4: Limpar o componente**

Em `biblia-lista.component.ts`:
- Remover `import { AuthStore } from '../../../core/auth/auth.store';`
- Remover a linha `readonly auth = inject(AuthStore);`
- Manter tudo o resto (o `effect` de auto-navegação para o primeiro livro fica — ao entrar em `/biblia` continua a abrir o primeiro livro).

- [ ] **Step 5: Ajustar o SCSS**

Em `biblia-lista.component.scss`:
- `.fichario { min-height: 100vh; … }` → `min-height: calc(100vh - #{$altura-header});`
- `.sidebar { … position: sticky; top: 0; … max-height: 100vh; … }` → `top: $altura-header;` e `max-height: calc(100vh - #{$altura-header});`
- No bloco `@include mobile` dentro de `.sidebar`, acrescentar `top: $altura-header-mobile;` (inofensivo — a sidebar mobile é `position: relative`).
- Remover os blocos `.sidebar__brand { … }` e `.sidebar__conta { … }` inteiros (já não há markup que os use).
- (Os `rgba($brass, …)` já foram tratados na Task 1.)

- [ ] **Step 6: Correr o teste — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts`
Expected: PASS (3 testes: lista, filtra, sem marca duplicada).

- [ ] **Step 7: Suite completa + build**

Run: `cd frontend/biblia-teologica-web && npm test && npx ng build`
Expected: PASS.

- [ ] **Step 8: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/features/biblia/biblia-lista/**/*.{ts,html,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/features/biblia/biblia-lista
git commit -m "$(cat <<'EOF'
refactor(web): fichário encaixa sob o header global

Remove a marca "Fichário" e o link Entrar/A minha conta da sidebar
(passam a vir do bt-header). A sidebar fica sticky abaixo do header em
vez de ocupar o viewport inteiro.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 9: Roadmap + verificação final

**Files:**
- Modify: `docs/ROADMAP.md`

**Interfaces:**
- Consumes: nada.
- Produces: nada.

- [ ] **Step 1: Atualizar `docs/ROADMAP.md`**

Na lista de Marcos, entre a entrada `M2` e a entrada `M3`, inserir:

```
- M2c — Casca do portal. Header/footer globais, homepage (hero + destaque do fichário +
  secções futuras + teaser de membros), páginas "Em breve" (Estudos/Notícias/Curiosidades),
  nova identidade visual "editorial" (tokens globais). Só frontend. Deixa o gancho de
  navegação/rotas para conteúdo reservado a membros, sem o implementar. Planeado.
  Spec: docs/superpowers/specs/2026-08-31-m2c-casca-portal-design.md
  Plano: docs/superpowers/plans/2026-08-31-m2c-casca-portal.md
```

- [ ] **Step 2: Verificação final completa**

Run:
```bash
cd frontend/biblia-teologica-web && npm ci && npm test && npx ng build && npx tsc --noEmit -p tsconfig.app.json && npx prettier --check "src/**/*.{ts,html,scss}"
```
Expected: tudo PASS. `prettier --check` limpo (se acusar, correr `--write` e incluir no commit).

- [ ] **Step 3: Commit**

```bash
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add docs/ROADMAP.md
git commit -m "$(cat <<'EOF'
docs(roadmap): marco M2c — casca do portal

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

- [ ] **Step 4: Teste manual (utilizador — guardrail de qualidade)**

O utilizador corre `docker compose up --build` e verifica, com o diff já revisto:
1. Homepage `/` — 4 blocos, CTA funcionam, identidade editorial (fundo creme, sotaque bordô, header claro).
2. Header em desktop: nav navega, `routerLinkActive` marca a página atual, `Entrar`/`Criar conta` vs `A minha conta` conforme sessão.
3. Header em largura mobile (< 900px): hambúrguer abre/fecha, fecha ao navegar e com Escape.
4. `/estudos`, `/noticias`, `/curiosidades` — página "Em breve" com o texto certo.
5. `/biblia` — fichário funciona aninhado sob o header; sidebar sticky sem sobrepor o header; abre o primeiro livro; sem marca duplicada.
6. `/entrar`, `/registar`, `/conta` — continuam a funcionar, agora com header/footer à volta.
7. Footer — links navegam; submeter a newsletter mostra o aviso e não faz pedido de rede (confirmar na aba Network).

---

## Self-Review

**1. Spec coverage:**
- §2 "só frontend / sem backend" → Global Constraints + todas as tasks. ✅
- §2 header global → Task 2 + Task 4. ✅
- §2 footer global (3 colunas + newsletter inerte + ©) → Task 3. ✅
- §2 `HomeComponent` 4 blocos → Task 6. ✅
- §2 `EmBreveComponent` + rotas → Task 7. ✅
- §2 `secao-card` → Task 5. ✅
- §2 ajuste mínimo ao `BibliaListaComponent` → Task 8. ✅
- §2 identidade via troca de valores dos tokens + swap brass→bordô → Task 1. ✅
- §2 entrada M2c no ROADMAP → Task 9. ✅
- §4.3 config de nav com flag `membros` → Task 2 (`nav.ts`, `itens` computed). ✅
- §4.1 header mobile fecha ao navegar / Escape → Task 2 (router events + host listener). ✅
- §5 tabela de rotas (incl. `**` → `''`) → Task 6 (Step 4) + Task 7 (Step 4). ✅
- §6 sidebar sticky sob o header, remover marca/`AuthStore` → Task 8. ✅
- §7.1 valores exatos dos tokens → Task 1 Step 1 (copiados verbatim). ✅
- §7.2 body creme, remover gradientes → Task 1 Step 2. ✅
- §7.3 swap dos literais brass (4 sítios + grep de confirmação) → Task 1 Steps 3-4. ✅
- §8 todos os testes listados → Tasks 2,3,4,5,6,7,8 (um `spec` por componente + `app.spec` + `biblia-lista.spec`). ✅
- §9 segurança (rel="noopener", sem innerHTML, sem HTTP) → Task 3 + Global Constraints. ✅

**2. Placeholder scan:** sem "TBD"/"TODO"/"handle edge cases". Todo o código e copy estão inline. A única condicional ("se `darken()` avisar…") tem instrução concreta de substituição. ✅

**3. Type consistency:**
- `NAV_PRINCIPAL` / `ItemNav` — definido na Task 2, consumido só na Task 2. ✅
- `HeaderComponent.menuAberto` (signal readonly), `alternarMenu()`, `fecharMenu()` — usados no template e no spec da Task 2 com os mesmos nomes. ✅
- `FooterComponent.mensagem()` (signal), `submeterNewsletter($event)` — spec e template coerentes. ✅
- `SecaoCardComponent` inputs `titulo`/`descricao`/`rota`/`disponivel` — mesmos nomes na Task 5 (definição), Task 6 (uso no template `home.component.html`) e specs. ✅
- `EmBreveComponent` lê `data['titulo']` / `data['descricao']`; rotas na Task 7 definem `data: { titulo, descricao }` — coerente; specs idem. ✅
- Token `$altura-header` — criado na Task 1, usado nas Tasks 4 (`app.scss`) e 8 (`biblia-lista.scss`). ✅
- Assinatura `AuthStore.definir(Usuario)` com `{ email, nomeExibicao, roles, deveTrocarSenha }` — igual à usada no spec existente do `biblia-lista`. ✅
