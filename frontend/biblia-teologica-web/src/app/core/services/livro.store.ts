import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { nomeDeExibicao, rotuloDeSecao } from '../biblia/nome-livro';
import { LivroService } from './livro.service';
import type { LivroListItem } from '../models/livro.model';

@Injectable({ providedIn: 'root' })
export class LivroStore {
  private readonly service = inject(LivroService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _livros = signal<LivroListItem[]>([]);
  private readonly _carregando = signal(false);
  private carregado = false;

  readonly livros = this._livros.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly total = computed(() => this._livros().length);

  carregar(): void {
    if (this.carregado || this._carregando()) return;
    this._carregando.set(true);
    this.service
      .listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (livros) => {
          this._livros.set(
            livros.map((l) => ({
              ...l,
              nome: nomeDeExibicao(l.nome),
              secaoRotulo: rotuloDeSecao(l.secaoRotulo),
            })),
          );
          this.carregado = true;
          this._carregando.set(false);
        },
        error: () => this._carregando.set(false),
      });
  }

  vizinhos(slug: string): { anterior?: LivroListItem; proximo?: LivroListItem } {
    const lista = this._livros();
    const i = lista.findIndex((l) => l.slug === slug);
    if (i === -1) return {};
    return { anterior: lista[i - 1], proximo: lista[i + 1] };
  }
}
