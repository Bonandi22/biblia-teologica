import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { LivroListItem } from '../../../core/models/livro.model';
import { LivroStore } from '../../../core/services/livro.store';

interface Divisao {
  rotulo: string;
  livros: LivroListItem[];
}

const PRIMEIRA_ORDEM_NT = 40;
const semAcento = (s: string) =>
  s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLocaleLowerCase('pt-BR');

function agrupar(livros: LivroListItem[]): Divisao[] {
  const divisoes: Divisao[] = [];
  for (const livro of livros) {
    const ultima = divisoes.at(-1);
    if (ultima?.rotulo === livro.secaoRotulo) ultima.livros.push(livro);
    else divisoes.push({ rotulo: livro.secaoRotulo, livros: [livro] });
  }
  return divisoes;
}

/** O índice de uma Bíblia: os 66 livros de uma vez, por testamento e divisão, na ordem do cânon. */
@Component({
  selector: 'bt-indice-canon',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './indice-canon.component.html',
  styleUrl: './indice-canon.component.scss',
})
export class IndiceCanonComponent {
  private readonly store = inject(LivroStore);

  readonly filtro = input<string>('');

  protected readonly falhou = this.store.falhou;
  protected readonly carregando = this.store.carregando;

  private readonly comPanorama = computed(() => new Set(this.store.comPanorama()));

  private readonly filtrados = computed(() => {
    const termo = semAcento(this.filtro().trim());
    const livros = [...this.store.livros()].sort((a, b) => a.ordem - b.ordem);
    return termo ? livros.filter((l) => semAcento(l.nome).includes(termo)) : livros;
  });

  protected readonly testamentos = computed(() => {
    const livros = this.filtrados();
    return [
      {
        nome: 'Antigo Testamento',
        divisoes: agrupar(livros.filter((l) => l.ordem < PRIMEIRA_ORDEM_NT)),
      },
      {
        nome: 'Novo Testamento',
        divisoes: agrupar(livros.filter((l) => l.ordem >= PRIMEIRA_ORDEM_NT)),
      },
    ].filter((t) => t.divisoes.length > 0);
  });

  protected readonly semResultado = computed(
    () => this.store.livros().length > 0 && this.filtrados().length === 0,
  );

  constructor() {
    this.store.carregar();
    this.store.carregarPanoramas();
  }

  protected temPanorama(slug: string): boolean {
    return this.comPanorama().has(slug);
  }

  protected tentarNovamente(): void {
    this.store.carregar();
  }
}
