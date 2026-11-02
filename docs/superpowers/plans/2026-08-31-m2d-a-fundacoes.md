# M2d-a — Fundações + design system — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Estabelecer o design system novo (tokens, tipografia, primitivos de formulário à mão) e re-tematizar o Angular Material, sem re-skinnar ainda nenhuma página.

**Architecture:** Só frontend Angular 22 (standalone + signals). `src/styles/_variables.scss` é reescrito nos **valores**, com os nomes antigos mantidos como **alias** para os novos — assim os SCSS de componente continuam a compilar e o M2d-b/-c migram-nos gradualmente. Cinco primitivos standalone novos em `src/app/shared/ui/` (com a11y embutida), consumidos pelo M2d-b. O `material.scss` sai de `azure`+Roboto para um primário quente + Space Grotesk (o `/admin` do M2b herda-o). Uma rota `/ui` dev-only serve de kitchen-sink.

**Tech Stack:** Angular 22 (standalone, signals, `input()`/`contentChild()`/`effect()`, `@if`/`@for`), `ControlValueAccessor`, SCSS com `@use`, Angular Material 22 (`mat.theme`), Vitest (`@angular/build:unit-test`, jsdom), Prettier 3.

**Spec:** `docs/superpowers/specs/2026-08-31-m2d-a-fundacoes-design.md`

## Global Constraints

- **Só frontend.** Nada fora de `frontend/biblia-teologica-web/src/`, `docs/ROADMAP.md` e `.gitattributes` (raiz). Sem endpoints, sem migração.
- **Componentes:** `standalone: true`, `changeDetection: ChangeDetectionStrategy.OnPush`, seletor com prefixo `bt-`.
- **Sem HTTP** em nenhum primitivo. Sem `innerHTML` de dado bruto; nunca `bypassSecurityTrust*`.
- **Tokens:** em `src/styles/_variables.scss` mudam os **valores**; os nomes antigos (`$parchment`, `$parchment-2`, `$card`, `$line`, `$brass`, `$near-black`, `$sage`) **mantêm-se** como alias apontando para os novos. Não remover nenhum nome nesta leva.
- **Valores de cor exatos:** `$ink #141416` · `$ink-mut #6a6a70` · `$paper #fdfdfc` · `$superficie #ffffff` · `$hair #e9e9e6` · `$acc #a6371c` · `$acc-dk #8a2e17` · `$acc-tint rgba(166, 55, 28, 0.08)`.
- **Fontes:** `$fonte-display: 'Space Grotesk', 'Helvetica Neue', Arial, sans-serif` · `$fonte-serif: 'Source Serif 4', Georgia, serif` · `$fonte-mono: 'IBM Plex Mono', ui-monospace, monospace`.
- **a11y dos primitivos:** `<label for>` sempre associado ao controlo; erro com `role="alert"` e ligado por `aria-describedby`; `aria-invalid="true"` no erro; toggle de senha com `aria-pressed` + `aria-label`; botão em carregamento com `aria-busy="true"` + `disabled`.
- **Central Package Management** é backend — o frontend não tem. Não adicionar dependências npm nesta leva (o `@angular/material` e `@angular/cdk` já existem e ficam).
- **Testes:** Vitest, convenção `Metodo_ResultadoEsperado_QuandoCondicao` (descrições em português). Backend não é tocado — não correr `dotnet`.
- **Comandos** correm em `frontend/biblia-teologica-web/`. Suite completa: `npm test`. Ficheiro único: `npx ng test --watch=false --include <caminho>`. Build: `npx ng build`. Tipos: `npx tsc --noEmit -p tsconfig.app.json`.
- **Prettier** `printWidth: 100`, `singleQuote: true` — `npx prettier --write` só nos ficheiros que tocas.
- Mensagem de commit termina sempre com:
  ```
  Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
  ```

---

## File Structure

**Criar:**
| Ficheiro | Responsabilidade |
|---|---|
| `.gitattributes` (raiz do repo) | Normalização de fins de linha (mata o ruído CRLF) |
| `src/app/shared/ui/bt-input.directive.ts` | Diretiva `[btInput]` — hook de estilo + a11y no `<input>`/`<textarea>` |
| `src/app/shared/ui/campo/campo.component.ts` / `.scss` / `.spec.ts` | `bt-campo` — wrapper label + controlo + erro + hint |
| `src/app/shared/ui/campo-senha/campo-senha.component.ts` / `.scss` / `.spec.ts` | `bt-campo-senha` — CVA, input password↔text com toggle |
| `src/app/shared/ui/botao/botao.component.ts` / `.scss` / `.spec.ts` | `bt-botao` — variantes + estado de carregamento |
| `src/app/shared/ui/alerta/alerta.component.ts` / `.scss` / `.spec.ts` | `bt-alerta` — mensagem inline erro/sucesso |
| `src/app/shared/ui/cartao/cartao.component.ts` / `.scss` / `.spec.ts` | `bt-cartao` — superfície para páginas auth/conta |
| `src/app/features/ui-provas/ui-provas.component.ts` / `.scss` / `.spec.ts` | Página `/ui` dev-only — kitchen-sink |

**Modificar:**
| Ficheiro | Alteração |
|---|---|
| `src/styles/_variables.scss` | Reescrita dos valores + alias dos nomes antigos + tokens novos |
| `src/styles.scss` | `body` em display/ink/paper; `:focus-visible`/`::selection` em `$acc`; link base |
| `src/styles/_mixins.scss` | Junta `@mixin tablet` e `@mixin eyebrow` |
| `src/index.html` | Um só `<link>` de fontes (entra Space Grotesk, saem Fraunces + Roboto) |
| `src/styles/material.scss` | Re-tema: primário quente + Space Grotesk |
| `src/app/app.routes.ts` | Rota `/ui` dev-only (só `!environment.production`) |
| `docs/ROADMAP.md` | Entrada M2d |

---

## Task 1: Fundações globais — tokens, estilos base, fontes, `.gitattributes`

**Files:**
- Create: `.gitattributes` (raiz do repo, i.e. `C:/Users/elker/RiderProjects/BibliaTeologica/.gitattributes`)
- Modify: `frontend/biblia-teologica-web/src/styles/_variables.scss`
- Modify: `frontend/biblia-teologica-web/src/styles.scss`
- Modify: `frontend/biblia-teologica-web/src/styles/_mixins.scss`
- Modify: `frontend/biblia-teologica-web/src/index.html`

**Interfaces:**
- Consumes: nada.
- Produces: tokens novos (`$ink`, `$ink-mut`, `$paper`, `$superficie`, `$hair`, `$acc`, `$acc-dk`, `$acc-tint`, escala `$t-*`, `$tracking-*`, `$sp-*`, `$largura-*`, `$raio`) e mixins `tablet`/`eyebrow` — usados pelas Tasks 3-7 e pelo M2d-b/-c. Alias antigos preservados.

Sem teste novo — verificação é `ng build` limpo + os 66 testes existentes verdes (os alias garantem compilação) + grep de valores órfãos.

- [ ] **Step 1: `.gitattributes`**

Criar na raiz do repo:

```
* text=auto eol=lf
*.png binary
*.jpg binary
*.jpeg binary
*.ico binary
*.webp binary
*.woff binary
*.woff2 binary
```

Não correr `git add --renormalize` — só governa daqui para a frente.

- [ ] **Step 2: Reescrever `src/styles/_variables.scss`**

Substituir o ficheiro inteiro por:

```scss
// Design system — M2d ("editorial moderno"). Valores mudam; nomes antigos ficam
// como alias durante a transição M2d-b/-c.

// ── Cor ──────────────────────────────
$ink:        #141416;
$ink-mut:    #6a6a70;
$paper:      #fdfdfc;
$superficie: #ffffff;
$hair:       #e9e9e6;
$acc:        #a6371c;
$acc-dk:     #8a2e17;
$acc-tint:   rgba(166, 55, 28, 0.08);

// alias de transição — remover no fim do M2d
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
$t-eyebrow: 0.6875rem;
$t-body:    1rem;
$t-lead:    1.125rem;
$t-h3:      1.375rem;
$t-h2:      clamp(1.6rem, 3vw, 2.2rem);
$t-h1:      clamp(2.2rem, 5vw, 3.4rem);
$tracking-tight:   -0.03em;
$tracking-eyebrow:  0.14em;

// ── Espaço / layout ──────────────────
$sp-1: 4px;
$sp-2: 8px;
$sp-3: 12px;
$sp-4: 16px;
$sp-6: 24px;
$sp-8: 32px;
$sp-12: 48px;
$sp-16: 64px;
$largura-conteudo: 1080px;
$largura-leitura:  42rem;
$raio: 6px;
$altura-header: 64px;
$altura-header-mobile: 56px;

// ── Breakpoints ──────────────────────
$bp-mobile: 900px;
$bp-tablet: 640px;
```

- [ ] **Step 3: Reescrever `src/styles.scss`**

Substituir o ficheiro inteiro por:

```scss
@use 'styles/variables' as *;

* {
  box-sizing: border-box;
}

html,
body {
  margin: 0;
  padding: 0;
}

body {
  background: $paper;
  color: $ink;
  font-family: $fonte-display;
  font-size: $t-body;
  line-height: 1.55;
  min-height: 100vh;
  -webkit-font-smoothing: antialiased;
}

a {
  color: $acc;
  text-decoration: none;

  &:hover {
    color: $acc-dk;
  }
}

:focus-visible {
  outline: 2px solid $acc;
  outline-offset: 2px;
}

::selection {
  background: $acc-tint;
}

@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
    animation: none !important;
  }
}
```

- [ ] **Step 4: Atualizar `src/styles/_mixins.scss`**

Substituir o ficheiro inteiro por:

```scss
@use 'variables' as *;

@mixin mobile {
  @media (max-width: $bp-mobile) {
    @content;
  }
}

@mixin tablet {
  @media (max-width: $bp-tablet) {
    @content;
  }
}

@mixin rotulo-mono {
  font-family: $fonte-mono;
  font-size: 0.68rem;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}

@mixin eyebrow {
  font-family: $fonte-mono;
  font-size: $t-eyebrow;
  letter-spacing: $tracking-eyebrow;
  text-transform: uppercase;
  color: $ink-mut;
}
```

- [ ] **Step 5: Atualizar `src/index.html`**

Substituir as linhas 9-18 (os dois blocos `<link>` de fontes Google — Fraunces/Source Serif/IBM Plex Mono e Roboto) por:

```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link
  href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500&family=Source+Serif+4:ital,opsz,wght@0,8..60,400;0,8..60,600;1,8..60,400&family=Space+Grotesk:wght@400;500;700&display=swap"
  rel="stylesheet"
/>
```

Manter a linha `<link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet" />` (o `/admin` do M2b usa).

- [ ] **Step 6: Verificar valores órfãos**

Run: `cd frontend/biblia-teologica-web && grep -rnE "Fraunces|1c2541|efe6d5|#a6763f|5c6b4f|Roboto" src/`
Expected: só resultados em `src/styles/material.scss` (`Roboto` — tratado na Task 2) e, se houver, comentários. Nenhum `Fraunces`. Nenhum hex antigo fora de comentário.

- [ ] **Step 7: Build + suite existente**

Run: `cd frontend/biblia-teologica-web && npx ng build && npm test`
Expected: build PASS sem avisos de Sass; 66/66 testes PASS (os alias garantem que os SCSS de componente compilam).

- [ ] **Step 8: Prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx prettier --write src/styles.scss src/styles/_variables.scss src/styles/_mixins.scss src/index.html
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add .gitattributes frontend/biblia-teologica-web/src/styles.scss frontend/biblia-teologica-web/src/styles/_variables.scss frontend/biblia-teologica-web/src/styles/_mixins.scss frontend/biblia-teologica-web/src/index.html
git commit -m "$(cat <<'EOF'
feat(web): design system M2d — tokens, tipografia, .gitattributes

Reescreve _variables.scss (cor terracota/near-black/off-white, escala
tipográfica fluida, escala de espaço), mantendo os nomes antigos como
alias para a transição. Space Grotesk entra no index.html, Fraunces e
Roboto saem. styles.scss e _mixins.scss atualizados. .gitattributes
normaliza fins de linha.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 2: Re-tema do Angular Material

**Files:**
- Modify: `frontend/biblia-teologica-web/src/styles/material.scss`

**Interfaces:**
- Consumes: nada (o Material tem o seu próprio sistema de `--mat-sys-*`).
- Produces: tema Material quente + Space Grotesk — herdado pelo `/admin` do M2b. Nada para tasks desta leva.

Sem teste novo — verificação é `ng build` limpo (sem deprecação de Sass) + os 66 testes verdes.

- [ ] **Step 1: Reescrever `src/styles/material.scss`**

Substituir o ficheiro inteiro por:

```scss
// Tema do Angular Material (M3), isolado do design system.
// Carregado no angular.json DEPOIS de src/styles.scss. Só define as CSS vars
// `--mat-sys-*` usadas pelos componentes `.mat-*`; NÃO toca no body nem na
// tipografia do site público.
//
// O Material fica só no /admin (M2b). Primário quente para não destoar do
// sotaque terracota; o match fino fica para quando o M2b construir o /admin.
@use '@angular/material' as mat;

html {
  @include mat.theme(
    (
      color: (
        primary: mat.$orange-palette,
        tertiary: mat.$red-palette,
      ),
      typography: 'Space Grotesk',
      density: 0,
    )
  );

  color-scheme: light;
}
```

- [ ] **Step 2: Build**

Run: `cd frontend/biblia-teologica-web && npx ng build`
Expected: PASS, sem avisos de deprecação de Sass. Se o Sass acusar que `typography: 'Space Grotesk'` (string) não é aceite, trocar para a forma de mapa:
```scss
typography: (
  plain-family: 'Space Grotesk',
  brand-family: 'Space Grotesk',
),
```
e voltar a correr o build.

- [ ] **Step 3: Suite existente**

Run: `cd frontend/biblia-teologica-web && npm test`
Expected: 66/66 PASS.

- [ ] **Step 4: Prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx prettier --write src/styles/material.scss
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/styles/material.scss
git commit -m "$(cat <<'EOF'
feat(web): re-tema do Angular Material (quente + Space Grotesk)

Sai azure+Roboto, entra um primário quente (orange) + tipografia Space
Grotesk. O Material passa a viver só no /admin (M2b), que herda este tema.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 3: `bt-botao`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/botao/botao.component.ts`
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/botao/botao.component.scss`
- Test: `frontend/biblia-teologica-web/src/app/shared/ui/botao/botao.component.spec.ts`

**Interfaces:**
- Consumes: tokens da Task 1.
- Produces: `BotaoComponent` (`selector: 'bt-botao'`). Inputs: `variante: 'primario' | 'fantasma' | 'texto'` (default `'primario'`), `type: 'button' | 'submit'` (default `'button'`), `disabled: boolean` (default `false`), `carregando: boolean` (default `false`). Renderiza um `<button>` interno. Usado pelas Tasks 7 e pelo M2d-b.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/shared/ui/botao/botao.component.spec.ts`:

```ts
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BotaoComponent } from './botao.component';

@Component({
  standalone: true,
  imports: [BotaoComponent],
  template: `
    <form (ngSubmit)="submeteu = true">
      <bt-botao [variante]="variante" [type]="type" [carregando]="carregando" [disabled]="disabled">
        Enviar
      </bt-botao>
    </form>
  `,
})
class HostComponent {
  variante: 'primario' | 'fantasma' | 'texto' = 'primario';
  type: 'button' | 'submit' = 'button';
  carregando = false;
  disabled = false;
  submeteu = false;
}

describe('BotaoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('aplica a classe da variante', () => {
    const fixture = criar();
    fixture.componentInstance.variante = 'fantasma';
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector('button');
    expect(btn.className).toContain('botao--fantasma');
  });

  it('em carregamento fica disabled e aria-busy', () => {
    const fixture = criar();
    fixture.componentInstance.carregando = true;
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(btn.disabled).toBe(true);
    expect(btn.getAttribute('aria-busy')).toBe('true');
  });

  it('respeita disabled', () => {
    const fixture = criar();
    fixture.componentInstance.disabled = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(true);
  });

  it('type submit dispara o submit do formulário', () => {
    const fixture = criar();
    fixture.componentInstance.type = 'submit';
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    expect(fixture.componentInstance.submeteu).toBe(true);
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/botao/botao.component.spec.ts`
Expected: FAIL — `Cannot find module './botao.component'`.

- [ ] **Step 3: Criar o componente**

`src/app/shared/ui/botao/botao.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'bt-botao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      [type]="type()"
      [class]="classe()"
      [disabled]="disabled() || carregando()"
      [attr.aria-busy]="carregando() ? 'true' : null"
    >
      @if (carregando()) {
        <span class="botao__spinner" aria-hidden="true"></span>
      }
      <ng-content />
    </button>
  `,
  styleUrl: './botao.component.scss',
})
export class BotaoComponent {
  readonly variante = input<'primario' | 'fantasma' | 'texto'>('primario');
  readonly type = input<'button' | 'submit'>('button');
  readonly disabled = input(false);
  readonly carregando = input(false);

  readonly classe = computed(() => `botao botao--${this.variante()}`);
}
```

`src/app/shared/ui/botao/botao.component.scss`:

```scss
@use '../../../../styles/variables' as *;

:host {
  display: inline-block;
}

.botao {
  display: inline-flex;
  align-items: center;
  gap: $sp-2;
  padding: 10px 18px;
  border: 1px solid transparent;
  border-radius: $raio;
  font-family: $fonte-display;
  font-size: 0.8125rem;
  font-weight: 500;
  line-height: 1;
  cursor: pointer;
  transition: background 0.15s ease, color 0.15s ease, border-color 0.15s ease;

  &:disabled {
    opacity: 0.55;
    cursor: not-allowed;
  }
}

.botao--primario {
  background: $ink;
  color: #fff;

  &:not(:disabled):hover {
    background: #000;
  }
}

.botao--fantasma {
  background: transparent;
  border-color: $acc;
  color: $acc;

  &:not(:disabled):hover {
    background: $acc-tint;
  }
}

.botao--texto {
  background: transparent;
  color: $acc;
  padding-left: 4px;
  padding-right: 4px;

  &:not(:disabled):hover {
    color: $acc-dk;
  }
}

.botao__spinner {
  width: 12px;
  height: 12px;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: botao-gira 0.7s linear infinite;
}

@keyframes botao-gira {
  to {
    transform: rotate(360deg);
  }
}
```

- [ ] **Step 4: Correr — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/botao/botao.component.spec.ts`
Expected: PASS (4 testes).

- [ ] **Step 5: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/shared/ui/botao/**/*.{ts,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/shared/ui/botao
git commit -m "$(cat <<'EOF'
feat(web): primitivo bt-botao (variantes + carregamento)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 4: `bt-alerta` + `bt-cartao`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/alerta/alerta.component.ts` / `.scss` / `.spec.ts`
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/cartao/cartao.component.ts` / `.scss` / `.spec.ts`

**Interfaces:**
- Consumes: tokens da Task 1.
- Produces:
  - `AlertaComponent` (`selector: 'bt-alerta'`), input `tipo: 'erro' | 'sucesso'` (default `'erro'`).
  - `CartaoComponent` (`selector: 'bt-cartao'`), input `titulo: string` (default `''`).
  Ambos usados pelas Tasks 7 e pelo M2d-b.

- [ ] **Step 1: Escrever os testes que falham**

`src/app/shared/ui/alerta/alerta.component.spec.ts`:

```ts
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AlertaComponent } from './alerta.component';

@Component({
  standalone: true,
  imports: [AlertaComponent],
  template: `<bt-alerta [tipo]="tipo">Mensagem</bt-alerta>`,
})
class HostComponent {
  tipo: 'erro' | 'sucesso' = 'erro';
}

describe('AlertaComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('usa role="alert" quando tipo erro', () => {
    const el = criar().nativeElement.querySelector('.alerta');
    expect(el.getAttribute('role')).toBe('alert');
  });

  it('usa role="status" quando tipo sucesso', () => {
    const fixture = criar();
    fixture.componentInstance.tipo = 'sucesso';
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.alerta').getAttribute('role')).toBe('status');
  });

  it('projeta o conteúdo', () => {
    expect(criar().nativeElement.textContent).toContain('Mensagem');
  });
});
```

`src/app/shared/ui/cartao/cartao.component.spec.ts`:

```ts
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CartaoComponent } from './cartao.component';

@Component({
  standalone: true,
  imports: [CartaoComponent],
  template: `<bt-cartao [titulo]="titulo">corpo</bt-cartao>`,
})
class HostComponent {
  titulo = '';
}

describe('CartaoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('não rende h1 quando titulo vazio', () => {
    expect(criar().nativeElement.querySelector('h1')).toBeNull();
  });

  it('rende h1 com o titulo', () => {
    const fixture = criar();
    fixture.componentInstance.titulo = 'Entrar';
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent.trim()).toBe('Entrar');
  });

  it('projeta o conteúdo', () => {
    expect(criar().nativeElement.textContent).toContain('corpo');
  });
});
```

- [ ] **Step 2: Correr — devem falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include "src/app/shared/ui/{alerta,cartao}/*.spec.ts"`
Expected: FAIL — módulos não encontrados.

- [ ] **Step 3: Criar `bt-alerta`**

`src/app/shared/ui/alerta/alerta.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'bt-alerta',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [class]="classe()" [attr.role]="tipo() === 'erro' ? 'alert' : 'status'">
      <ng-content />
    </div>
  `,
  styleUrl: './alerta.component.scss',
})
export class AlertaComponent {
  readonly tipo = input<'erro' | 'sucesso'>('erro');
  readonly classe = computed(() => `alerta alerta--${this.tipo()}`);
}
```

`src/app/shared/ui/alerta/alerta.component.scss`:

```scss
@use '../../../../styles/variables' as *;

.alerta {
  padding: $sp-3 $sp-4;
  border-radius: $raio;
  border: 1px solid;
  font-size: 0.875rem;
  line-height: 1.45;
}

.alerta--erro {
  background: $acc-tint;
  border-color: rgba(166, 55, 28, 0.28);
  color: $acc-dk;
}

.alerta--sucesso {
  background: #eef3ee;
  border-color: #cddccd;
  color: #37503a;
}
```

- [ ] **Step 4: Criar `bt-cartao`**

`src/app/shared/ui/cartao/cartao.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-cartao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="cartao">
      @if (titulo()) {
        <h1 class="cartao__titulo">{{ titulo() }}</h1>
      }
      <ng-content />
    </section>
  `,
  styleUrl: './cartao.component.scss',
})
export class CartaoComponent {
  readonly titulo = input<string>('');
}
```

`src/app/shared/ui/cartao/cartao.component.scss`:

```scss
@use '../../../../styles/variables' as *;

.cartao {
  width: 100%;
  max-width: 400px;
  margin: 0 auto;
  padding: $sp-8;
  background: $superficie;
  border: 1px solid $hair;
  border-radius: $raio;
}

.cartao__titulo {
  margin: 0 0 $sp-6;
  font-family: $fonte-display;
  font-weight: 700;
  font-size: $t-h3;
  letter-spacing: $tracking-tight;
  color: $ink;
}
```

- [ ] **Step 5: Correr — devem passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include "src/app/shared/ui/{alerta,cartao}/*.spec.ts"`
Expected: PASS (3 + 3 testes).

- [ ] **Step 6: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/shared/ui/alerta/**/*.{ts,scss}" "src/app/shared/ui/cartao/**/*.{ts,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/shared/ui/alerta frontend/biblia-teologica-web/src/app/shared/ui/cartao
git commit -m "$(cat <<'EOF'
feat(web): primitivos bt-alerta e bt-cartao

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 5: `bt-campo` + diretiva `btInput`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/bt-input.directive.ts`
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/campo/campo.component.ts` / `.scss` / `.spec.ts`

**Interfaces:**
- Consumes: tokens da Task 1.
- Produces:
  - `BtInputDirective` (`selector: 'input[btInput], textarea[btInput]'`) — signals públicos `id`, `invalido`, `describedBy` (o `bt-campo` empurra-os).
  - `CampoComponent` (`selector: 'bt-campo'`) — inputs `label` (required), `erro: string | null` (default `null`), `hint: string` (default `''`), `obrigatorio: boolean` (default `false`). Consome `contentChild(BtInputDirective)`.
  Ambos usados pela Task 6, Task 7 e pelo M2d-b. O consumidor importa **ambos**.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/shared/ui/campo/campo.component.spec.ts`:

```ts
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BtInputDirective } from '../bt-input.directive';
import { CampoComponent } from './campo.component';

@Component({
  standalone: true,
  imports: [CampoComponent, BtInputDirective],
  template: `
    <bt-campo [label]="label" [erro]="erro" [hint]="hint">
      <input btInput />
    </bt-campo>
  `,
})
class HostComponent {
  label = 'Email';
  erro: string | null = null;
  hint = '';
}

describe('CampoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('associa o label ao input pelo id', () => {
    const el = criar().nativeElement as HTMLElement;
    const label = el.querySelector('label') as HTMLLabelElement;
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.id).toBeTruthy();
    expect(label.getAttribute('for')).toBe(input.id);
  });

  it('mostra o hint quando não há erro', () => {
    const fixture = criar();
    fixture.componentInstance.hint = 'Mínimo 8 caracteres';
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Mínimo 8 caracteres');
  });

  it('erro: role alert, aria-invalid e aria-describedby ligados', () => {
    const fixture = criar();
    fixture.componentInstance.erro = 'Email inválido';
    fixture.componentInstance.hint = 'ignorado quando há erro';
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const erro = el.querySelector('[role="alert"]') as HTMLElement;
    const input = el.querySelector('input') as HTMLInputElement;
    expect(erro.textContent?.trim()).toBe('Email inválido');
    expect(input.getAttribute('aria-invalid')).toBe('true');
    expect(input.getAttribute('aria-describedby')).toBe(erro.id);
    expect(el.textContent).not.toContain('ignorado quando há erro');
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/campo/campo.component.spec.ts`
Expected: FAIL — módulos não encontrados.

- [ ] **Step 3: Criar a diretiva**

`src/app/shared/ui/bt-input.directive.ts`:

```ts
import { Directive, signal } from '@angular/core';

@Directive({
  selector: 'input[btInput], textarea[btInput]',
  standalone: true,
  host: {
    class: 'bt-input',
    '[id]': 'id()',
    '[attr.aria-invalid]': "invalido() ? 'true' : null",
    '[attr.aria-describedby]': 'describedBy()',
  },
})
export class BtInputDirective {
  readonly id = signal<string>('');
  readonly invalido = signal(false);
  readonly describedBy = signal<string | null>(null);
}
```

- [ ] **Step 4: Criar o componente**

`src/app/shared/ui/campo/campo.component.ts`:

```ts
import {
  ChangeDetectionStrategy,
  Component,
  contentChild,
  effect,
  input,
} from '@angular/core';
import { BtInputDirective } from '../bt-input.directive';

let contador = 0;

@Component({
  selector: 'bt-campo',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label class="campo__rotulo" [attr.for]="campoId">
      {{ label() }}@if (obrigatorio()) {<span class="campo__obrig" aria-hidden="true"> *</span>}
    </label>
    <ng-content />
    @if (hint() && !erro()) {
      <p class="campo__hint" [id]="hintId">{{ hint() }}</p>
    }
    @if (erro()) {
      <p class="campo__erro" role="alert" [id]="erroId">{{ erro() }}</p>
    }
  `,
  styleUrl: './campo.component.scss',
})
export class CampoComponent {
  readonly label = input.required<string>();
  readonly erro = input<string | null>(null);
  readonly hint = input<string>('');
  readonly obrigatorio = input(false);

  private readonly n = contador++;
  readonly campoId = `bt-campo-${this.n}`;
  readonly erroId = `bt-campo-${this.n}-erro`;
  readonly hintId = `bt-campo-${this.n}-hint`;

  private readonly controlo = contentChild(BtInputDirective);

  constructor() {
    effect(() => {
      const c = this.controlo();
      if (!c) return;
      c.id.set(this.campoId);
      c.invalido.set(!!this.erro());
      c.describedBy.set(this.erro() ? this.erroId : this.hint() ? this.hintId : null);
    });
  }
}
```

`src/app/shared/ui/campo/campo.component.scss`:

```scss
@use '../../../../styles/variables' as *;

:host {
  display: block;
  margin-bottom: $sp-4;
}

.campo__rotulo {
  display: block;
  margin-bottom: $sp-2;
  font-family: $fonte-display;
  font-size: 0.8125rem;
  font-weight: 500;
  color: $ink;
}

.campo__obrig {
  color: $acc;
}

::ng-deep .bt-input {
  width: 100%;
  padding: 9px 12px;
  border: 1px solid $hair;
  border-radius: $raio;
  background: $superficie;
  font-family: $fonte-display;
  font-size: 0.9375rem;
  color: $ink;
  outline: none;

  &:focus {
    border-color: $acc;
  }

  &[aria-invalid='true'] {
    border-color: $acc-dk;
  }
}

.campo__hint {
  margin: $sp-2 0 0;
  font-size: 0.75rem;
  color: $ink-mut;
}

.campo__erro {
  margin: $sp-2 0 0;
  font-size: 0.75rem;
  color: $acc-dk;
}
```

> `::ng-deep` é usado deliberadamente para estilar o `<input btInput>` projetado (a diretiva não tem folha de estilo própria). Escopo limitado ao `.bt-input` dentro do `:host` do `bt-campo`.

- [ ] **Step 5: Correr — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/campo/campo.component.spec.ts`
Expected: PASS (3 testes).

- [ ] **Step 6: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/shared/ui/bt-input.directive.ts" "src/app/shared/ui/campo/**/*.{ts,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/shared/ui/bt-input.directive.ts frontend/biblia-teologica-web/src/app/shared/ui/campo
git commit -m "$(cat <<'EOF'
feat(web): primitivo bt-campo + diretiva btInput

Wrapper de campo com label associado, hint e erro (role=alert +
aria-describedby + aria-invalid empurrados para o input projetado via
content query).

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 6: `bt-campo-senha`

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/campo-senha/campo-senha.component.ts` / `.scss` / `.spec.ts`

**Interfaces:**
- Consumes: `CampoComponent` + `BtInputDirective` (Task 5), tokens (Task 1).
- Produces: `CampoSenhaComponent` (`selector: 'bt-campo-senha'`), `ControlValueAccessor` (funciona com `formControlName`/`formControl`). Inputs: `label` (required), `hint: string` (default `''`), `erro: string | null` (default `null`), `autocomplete: string` (default `'current-password'`). Usado pela Task 7 e pelo M2d-b.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/shared/ui/campo-senha/campo-senha.component.spec.ts`:

```ts
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CampoSenhaComponent } from './campo-senha.component';

@Component({
  standalone: true,
  imports: [CampoSenhaComponent, ReactiveFormsModule],
  template: `<bt-campo-senha label="Senha" [formControl]="ctrl" />`,
})
class HostComponent {
  ctrl = new FormControl('');
}

describe('CampoSenhaComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('começa como password e o toggle alterna para text', () => {
    const fixture = criar();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    const botao = fixture.nativeElement.querySelector('.senha__toggle') as HTMLButtonElement;
    expect(input.getAttribute('type')).toBe('password');
    expect(botao.getAttribute('aria-pressed')).toBe('false');
    expect(botao.getAttribute('aria-label')).toBe('Mostrar senha');
    botao.click();
    fixture.detectChanges();
    expect(input.getAttribute('type')).toBe('text');
    expect(botao.getAttribute('aria-pressed')).toBe('true');
    expect(botao.getAttribute('aria-label')).toBe('Esconder senha');
  });

  it('propaga o valor para o FormControl', () => {
    const fixture = criar();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'segredo123';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.ctrl.value).toBe('segredo123');
  });

  it('escreve o valor vindo do FormControl no input', () => {
    const fixture = criar();
    fixture.componentInstance.ctrl.setValue('abc');
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('input') as HTMLInputElement).value).toBe('abc');
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/campo-senha/campo-senha.component.spec.ts`
Expected: FAIL — módulo não encontrado.

- [ ] **Step 3: Criar o componente**

`src/app/shared/ui/campo-senha/campo-senha.component.ts`:

```ts
import { ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BtInputDirective } from '../bt-input.directive';
import { CampoComponent } from '../campo/campo.component';

@Component({
  selector: 'bt-campo-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CampoComponent, BtInputDirective],
  template: `
    <bt-campo [label]="label()" [erro]="erro()" [hint]="hint()">
      <span class="senha__wrap">
        <input
          btInput
          [type]="visivel() ? 'text' : 'password'"
          [value]="valor()"
          [disabled]="desativado()"
          [attr.autocomplete]="autocomplete()"
          (input)="aoEscrever($any($event.target).value)"
          (blur)="aoTocarCb()"
        />
        <button
          type="button"
          class="senha__toggle"
          [attr.aria-pressed]="visivel()"
          [attr.aria-label]="visivel() ? 'Esconder senha' : 'Mostrar senha'"
          (click)="alternar()"
        >
          {{ visivel() ? 'Esconder' : 'Mostrar' }}
        </button>
      </span>
    </bt-campo>
  `,
  styleUrl: './campo-senha.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CampoSenhaComponent),
      multi: true,
    },
  ],
})
export class CampoSenhaComponent implements ControlValueAccessor {
  readonly label = input.required<string>();
  readonly hint = input<string>('');
  readonly erro = input<string | null>(null);
  readonly autocomplete = input<string>('current-password');

  readonly valor = signal('');
  readonly visivel = signal(false);
  readonly desativado = signal(false);

  private aoMudar: (v: string) => void = () => {};
  aoTocarCb: () => void = () => {};

  writeValue(v: string | null): void {
    this.valor.set(v ?? '');
  }
  registerOnChange(fn: (v: string) => void): void {
    this.aoMudar = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.aoTocarCb = fn;
  }
  setDisabledState(d: boolean): void {
    this.desativado.set(d);
  }

  aoEscrever(v: string): void {
    this.valor.set(v);
    this.aoMudar(v);
  }
  alternar(): void {
    this.visivel.update((x) => !x);
  }
}
```

`src/app/shared/ui/campo-senha/campo-senha.component.scss`:

```scss
@use '../../../../styles/variables' as *;

.senha__wrap {
  position: relative;
  display: block;
}

.senha__toggle {
  position: absolute;
  top: 50%;
  right: 8px;
  transform: translateY(-50%);
  border: 0;
  background: transparent;
  padding: 4px 6px;
  font-family: $fonte-mono;
  font-size: 0.7rem;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: $acc;
  cursor: pointer;

  &:hover {
    color: $acc-dk;
  }
}

::ng-deep .senha__wrap .bt-input {
  padding-right: 84px;
}
```

- [ ] **Step 4: Correr — deve passar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/shared/ui/campo-senha/campo-senha.component.spec.ts`
Expected: PASS (3 testes).

- [ ] **Step 5: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/shared/ui/campo-senha/**/*.{ts,scss}"
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/shared/ui/campo-senha
git commit -m "$(cat <<'EOF'
feat(web): primitivo bt-campo-senha (CVA + toggle mostrar/esconder)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 7: Rota `/ui` dev-only (kitchen-sink)

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/features/ui-provas/ui-provas.component.ts` / `.scss` / `.spec.ts`
- Modify: `frontend/biblia-teologica-web/src/app/app.routes.ts`

**Interfaces:**
- Consumes: os 5 primitivos + `BtInputDirective` (Tasks 3-6), `environment` (`src/environments/environment.ts`).
- Produces: rota `/ui` só quando `!environment.production`. Nada consumido por tasks posteriores.

- [ ] **Step 1: Escrever o teste que falha**

`src/app/features/ui-provas/ui-provas.component.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { UiProvasComponent } from './ui-provas.component';

describe('UiProvasComponent', () => {
  it('monta com todos os primitivos sem erros', () => {
    TestBed.configureTestingModule({ imports: [UiProvasComponent] });
    const fixture = TestBed.createComponent(UiProvasComponent);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Primitivos');
    expect(fixture.nativeElement.querySelectorAll('bt-botao').length).toBeGreaterThanOrEqual(3);
    expect(fixture.nativeElement.querySelector('bt-campo')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-campo-senha')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-alerta')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-cartao')).toBeTruthy();
  });
});
```

- [ ] **Step 2: Correr — deve falhar**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/ui-provas/ui-provas.component.spec.ts`
Expected: FAIL — módulo não encontrado.

- [ ] **Step 3: Criar o componente**

`src/app/features/ui-provas/ui-provas.component.ts`:

```ts
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { AlertaComponent } from '../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../shared/ui/campo/campo.component';
import { CampoSenhaComponent } from '../../shared/ui/campo-senha/campo-senha.component';
import { CartaoComponent } from '../../shared/ui/cartao/cartao.component';

@Component({
  selector: 'bt-ui-provas',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    AlertaComponent,
    BotaoComponent,
    BtInputDirective,
    CampoComponent,
    CampoSenhaComponent,
    CartaoComponent,
  ],
  templateUrl: './ui-provas.component.html',
  styleUrl: './ui-provas.component.scss',
})
export class UiProvasComponent {
  readonly senha = new FormControl('');
}
```

`src/app/features/ui-provas/ui-provas.component.html`:

```html
<div class="provas">
  <h1>Primitivos do design system</h1>

  <section>
    <h2>bt-botao</h2>
    <bt-botao variante="primario">Primário</bt-botao>
    <bt-botao variante="fantasma">Fantasma</bt-botao>
    <bt-botao variante="texto">Texto</bt-botao>
    <bt-botao variante="primario" [carregando]="true">A carregar</bt-botao>
    <bt-botao variante="primario" [disabled]="true">Desativado</bt-botao>
  </section>

  <section>
    <h2>bt-campo</h2>
    <bt-campo label="Email" hint="Usamos para te identificar">
      <input btInput type="email" />
    </bt-campo>
    <bt-campo label="Email" erro="Email inválido">
      <input btInput type="email" value="xxx" />
    </bt-campo>
    <bt-campo label="Nome" [obrigatorio]="true">
      <input btInput type="text" />
    </bt-campo>
  </section>

  <section>
    <h2>bt-campo-senha</h2>
    <bt-campo-senha label="Senha" hint="Mínimo 8 caracteres" [formControl]="senha" />
  </section>

  <section>
    <h2>bt-alerta</h2>
    <bt-alerta tipo="erro">Não foi possível criar a conta.</bt-alerta>
    <bt-alerta tipo="sucesso">Senha alterada.</bt-alerta>
  </section>

  <section>
    <h2>bt-cartao</h2>
    <bt-cartao titulo="Entrar">
      <p>Conteúdo do cartão.</p>
      <bt-botao variante="primario" type="button">Ação</bt-botao>
    </bt-cartao>
  </section>
</div>
```

`src/app/features/ui-provas/ui-provas.component.scss`:

```scss
@use '../../../styles/variables' as *;
@use '../../../styles/mixins' as *;

.provas {
  max-width: $largura-conteudo;
  margin: 0 auto;
  padding: $sp-12 $sp-6;
}

h1 {
  font-family: $fonte-display;
  font-weight: 700;
  font-size: $t-h2;
  letter-spacing: $tracking-tight;
}

section {
  margin: $sp-8 0;
  padding-top: $sp-6;
  border-top: 1px solid $hair;
  display: flex;
  flex-wrap: wrap;
  gap: $sp-4;
  align-items: flex-start;
}

section h2 {
  @include eyebrow;
  flex-basis: 100%;
  margin: 0 0 $sp-2;
}
```

- [ ] **Step 4: Registar a rota dev-only** — `src/app/app.routes.ts`

Adicionar o import no topo:

```ts
import { environment } from '../environments/environment';
```

Dentro do array `routes`, **antes** da entrada `{ path: '**', redirectTo: '' }`, inserir:

```ts
...(environment.production
  ? []
  : [
      {
        path: 'ui',
        loadComponent: () =>
          import('./features/ui-provas/ui-provas.component').then((m) => m.UiProvasComponent),
      },
    ]),
```

- [ ] **Step 5: Correr o teste + build de produção**

Run: `cd frontend/biblia-teologica-web && npx ng test --watch=false --include src/app/features/ui-provas/ui-provas.component.spec.ts && npx ng build --configuration production`
Expected: teste PASS; build de produção PASS (a rota `/ui` é excluída via `environment.production` no `environment.prod.ts`).

- [ ] **Step 6: Suite completa**

Run: `cd frontend/biblia-teologica-web && npm test`
Expected: PASS (66 + os novos).

- [ ] **Step 7: Tipos + prettier + commit**

```bash
cd frontend/biblia-teologica-web && npx tsc --noEmit -p tsconfig.app.json && npx prettier --write "src/app/features/ui-provas/**/*.{ts,html,scss}" src/app/app.routes.ts
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add frontend/biblia-teologica-web/src/app/features/ui-provas frontend/biblia-teologica-web/src/app/app.routes.ts
git commit -m "$(cat <<'EOF'
feat(web): rota /ui dev-only — kitchen-sink dos primitivos

Só registada quando !environment.production. Serve de documentação viva
dos primitivos para o M2d-b/-c.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

---

## Task 8: Roadmap + verificação final

**Files:**
- Modify: `docs/ROADMAP.md`

**Interfaces:**
- Consumes: nada.
- Produces: nada.

- [ ] **Step 1: Atualizar `docs/ROADMAP.md`**

Na lista de Marcos, entre a entrada `M2c` e a entrada `M3`, inserir:

```
- M2d — Repaginação visual (design pass). Direção "editorial moderno": organização
  "portal", Space Grotesk + Source Serif 4 + IBM Plex Mono, sotaque terracota #A6371C,
  off-white quase-monocromático. Angular Material sai das páginas públicas (primitivos
  à mão em src/app/shared/ui/) e fica só no /admin. Só frontend. Entregue em 3 PRs:
  - M2d-a — fundações + design system (tokens, tipografia, primitivos, re-tema do
    Material, .gitattributes). Spec: docs/superpowers/specs/2026-08-31-m2d-a-fundacoes-design.md
    Plano: docs/superpowers/plans/2026-08-31-m2d-a-fundacoes.md
  - M2d-b — páginas públicas re-skin (auth, conta, header/footer, em-breve, 404, titles).
  - M2d-c — homepage "portal" + re-skin do fichário.
```

- [ ] **Step 2: Verificação final completa**

Run:
```bash
cd frontend/biblia-teologica-web && npm ci && npm test && npx ng build && npx ng build --configuration production && npx tsc --noEmit -p tsconfig.app.json && npx prettier --check "src/**/*.{ts,html,scss}"
```
Expected: tudo PASS. `prettier --check` limpo nos ficheiros criados/alterados por este plano (se acusar ficheiros que este plano NÃO tocou, é dívida pré-existente — **não corrigir aqui**, mencionar no relatório).

- [ ] **Step 3: Commit**

```bash
cd C:/Users/elker/RiderProjects/BibliaTeologica
git add docs/ROADMAP.md
git commit -m "$(cat <<'EOF'
docs(roadmap): marco M2d — repaginação visual (3 sub-marcos)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_013WnkyKqz7UA7HW63AmXn1B
EOF
)"
```

- [ ] **Step 4: Teste manual (utilizador — guardrail de qualidade)**

O utilizador corre `docker compose up --build` e verifica, com o diff já revisto:
1. **Shell + fichário** — o header, a homepage e as fichas dos livros aparecem na paleta nova (terracota/near-black/off-white) e com Space Grotesk nos títulos. Layout inalterado.
2. **`/entrar`, `/registar`, `/conta`** — continuam a funcionar; o Material aparece agora quente + Space Grotesk (ainda com forma Material — o re-skin é o M2d-b).
3. **`/ui`** (só em dev; abrir com `npm start` em `http://localhost:4200/ui`, porque a imagem Docker usa o build de produção e não tem esta rota) — mostra os 5 primitivos em todos os estados; o toggle de senha alterna; o botão "a carregar" mostra o spinner.
4. Sem erros na consola; sem 404 de fontes (Space Grotesk carrega).

---

## Self-Review

**1. Spec coverage:**
- §3 "só frontend" → Global Constraints + todas as tasks. ✅
- §4.1 `_variables.scss` reescrita + alias → Task 1 Step 2 (valores verbatim da spec). ✅
- §4.2 `styles.scss` → Task 1 Step 3. ✅
- §4.3 `_mixins.scss` (`tablet`, `eyebrow`) → Task 1 Step 4. ✅
- §4.4 `index.html` fontes → Task 1 Step 5. ✅
- §4.5 `material.scss` re-tema → Task 2. ✅
- §4.6 primitivos (`bt-campo`+`btInput`, `bt-campo-senha`, `bt-botao`, `bt-cartao`, `bt-alerta`) → Tasks 3-6, com o contrato a11y nos testes. ✅
- §4.7 rota `/ui` dev-only → Task 7. ✅
- §4.8 `.gitattributes` → Task 1 Step 1. ✅
- §6 testes (cada primitivo + regressão + build prod) → Tasks 3-7 + Task 8 Step 2. ✅
- §8 roadmap → Task 8 Step 1. ✅

**2. Placeholder scan:** nenhum "TBD"/"TODO"/"handle edge cases". Todo o código de componente/teste/SCSS está inline. A Task 2 Step 2 tem uma ramificação condicional concreta (forma de string vs mapa para a config de tipografia do Material) com o código exato de ambos os lados — não é placeholder.

**3. Type consistency:**
- `BtInputDirective` — signals `id`, `invalido`, `describedBy` definidos na Task 5, empurrados pelo `bt-campo` (Task 5) e reutilizados na Task 6. ✅
- `CampoComponent` inputs `label`/`erro`/`hint`/`obrigatorio` — mesmos nomes na Task 5 (def), Task 6 (`<bt-campo [label] [erro] [hint]>`) e Task 7. ✅
- `CampoSenhaComponent` inputs `label`/`hint`/`erro`/`autocomplete` + CVA — Task 6 def, Task 7 uso (`[formControl]`). ✅
- `BotaoComponent` inputs `variante`/`type`/`disabled`/`carregando` — Task 3 def, Task 7 uso. ✅
- `AlertaComponent` `tipo`, `CartaoComponent` `titulo` — Task 4 def, Task 7 uso. ✅
- Tokens `$acc`/`$acc-dk`/`$acc-tint`/`$ink`/`$ink-mut`/`$hair`/`$superficie`/`$raio`/`$sp-*`/`$t-*`/`$tracking-*` — criados na Task 1, usados nos SCSS das Tasks 3-7. ✅
- `environment.production` — usado na Task 7 Step 4; `environment` importado de `../environments/environment`. ✅
