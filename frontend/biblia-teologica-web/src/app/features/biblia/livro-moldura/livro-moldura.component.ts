import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { LIVROS_ARC, urlCapitulo } from '../../../core/biblia/arc';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';
import { LivroStore } from '../../../core/services/livro.store';

/** Moldura comum da página do livro e da ficha: divisão, nome, frase e capítulos na ARC. */
@Component({
  selector: 'bt-livro-moldura',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './livro-moldura.component.html',
  styleUrl: './livro-moldura.component.scss',
})
export class LivroMolduraComponent {
  private readonly store = inject(LivroStore);
  private readonly lugar = inject(LugarAtualService);

  readonly slug = input.required<string>();
  readonly resumo = input<string | null>(null);

  protected readonly divisaoAberta = signal(false);

  private readonly livro = computed(
    () => this.store.livros().find((l) => l.slug === this.slug()) ?? null,
  );
  /** Sem a lista de livros, um rótulo neutro: o slug nunca aparece para quem lê. */
  protected readonly nome = computed(() => this.livro()?.nome ?? 'Livro');
  protected readonly secao = computed(() => this.livro()?.secaoRotulo ?? null);
  protected readonly daDivisao = computed(() =>
    this.store.livros().filter((l) => l.secaoRotulo === this.secao()),
  );
  protected readonly capitulos = computed(() => {
    const total = LIVROS_ARC[this.slug()]?.capitulos ?? 0;
    return Array.from({ length: total }, (_, i) => ({
      numero: i + 1,
      url: urlCapitulo(this.slug(), i + 1),
    }));
  });

  constructor() {
    this.store.carregar();

    effect(() => {
      const livro = this.livro();
      const total = this.capitulos().length;
      if (!livro) this.lugar.limpar();
      else this.lugar.definir(total > 1 ? `${livro.nome} 1–${total}` : livro.nome);
    });

    effect(() => {
      this.slug();
      this.divisaoAberta.set(false);
    });

    inject(DestroyRef).onDestroy(() => this.lugar.limpar());
  }

  protected alternarDivisao(): void {
    this.divisaoAberta.update((v) => !v);
  }
}
