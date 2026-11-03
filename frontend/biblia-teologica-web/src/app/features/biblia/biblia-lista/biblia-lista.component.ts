import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  DestroyRef,
  signal,
} from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { LivroStore } from '../../../core/services/livro.store';
import type { LivroListItem } from '../../../core/models/livro.model';

const ORDEM_SECOES = [
  'Pentateuco',
  'LivrosHistoricos',
  'LivrosPoeticos',
  'ProfetasMaiores',
  'ProfetasMenores',
  'Evangelhos',
  'Atos',
  'CartasPaulinas',
  'CartasGerais',
  'Apocalipse',
];

const semAcento = (s: string) => s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();

@Component({
  selector: 'bt-biblia-lista',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './biblia-lista.component.html',
  styleUrl: './biblia-lista.component.scss',
})
export class BibliaListaComponent {
  private readonly store = inject(LivroStore);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  /** Só tem efeito visual no celular; no computador a lista está sempre visível. */
  readonly aberta = signal(false);
  private readonly urlAtual = signal(this.router.url);
  readonly livroAtual = computed(() => {
    const slug = this.urlAtual().split(/[?#]/)[0].split('/')[2];
    return this.store.livros().find((l) => l.slug === slug) ?? null;
  });

  readonly busca = signal('');
  readonly carregando = this.store.carregando;

  readonly livrosFiltrados = computed(() => {
    const termo = semAcento(this.busca().trim());
    const livros = this.store.livros();
    return termo ? livros.filter((l) => semAcento(l.nome).includes(termo)) : livros;
  });

  readonly secoes = computed(() => {
    const grupos = new Map<string, { rotulo: string; livros: LivroListItem[] }>();
    for (const livro of this.livrosFiltrados()) {
      const grupo = grupos.get(livro.secao) ?? { rotulo: livro.secaoRotulo, livros: [] };
      grupo.livros.push(livro);
      grupos.set(livro.secao, grupo);
    }
    return ORDEM_SECOES.filter((s) => grupos.has(s)).map((s) => ({ chave: s, ...grupos.get(s)! }));
  });

  alternar(): void {
    this.aberta.update((v) => !v);
  }

  fechar(): void {
    this.aberta.set(false);
  }

  constructor() {
    this.store.carregar();

    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((e) => this.urlAtual.set(e.urlAfterRedirects));

    effect(() => {
      const livros = this.store.livros();
      if (livros.length === 0) return;
      const caminho = this.router.url.split(/[?#]/)[0].replace(/\/+$/, '');
      if (caminho === '/biblia') {
        this.router.navigate(['/biblia', livros[0].slug]).catch(() => {});
      }
    });
  }
}
