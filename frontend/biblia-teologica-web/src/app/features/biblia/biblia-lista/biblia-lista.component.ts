import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
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

  constructor() {
    this.store.carregar();

    effect(() => {
      const livros = this.store.livros();
      if (livros.length === 0) return;
      const temSlug = /\/biblia\/[^/]+$/.test(this.router.url);
      if (!temSlug) {
        this.router.navigate(['/biblia', livros[0].slug]).catch(() => {});
      }
    });
  }
}
