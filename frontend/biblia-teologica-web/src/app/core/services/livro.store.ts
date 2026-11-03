import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { nomeDeExibicao, rotuloDeSecao } from '../biblia/nome-livro';
import { ConteudoService } from './conteudo.service';
import { LivroService } from './livro.service';
import type { LivroListItem } from '../models/livro.model';

@Injectable({ providedIn: 'root' })
export class LivroStore {
  private readonly service = inject(LivroService);
  private readonly conteudo = inject(ConteudoService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _livros = signal<LivroListItem[]>([]);
  private readonly _carregando = signal(false);
  private readonly _falhou = signal(false);
  private readonly _comPanorama = signal<string[]>([]);
  private panoramasPedidos = false;
  private carregado = false;

  readonly livros = this._livros.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly falhou = this._falhou.asReadonly();
  /** Slugs dos livros com panorama publicado; vazio se a lista falhar, porque é só uma marca. */
  readonly comPanorama = this._comPanorama.asReadonly();
  readonly total = computed(() => this._livros().length);

  carregar(): void {
    if (this.carregado || this._carregando()) return;
    this._carregando.set(true);
    this._falhou.set(false);
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
        error: () => {
          this._carregando.set(false);
          this._falhou.set(true);
        },
      });
  }

  carregarPanoramas(): void {
    if (this.panoramasPedidos) return;
    this.panoramasPedidos = true;
    this.conteudo
      .listarLivrosComPanorama()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (slugs) => this._comPanorama.set(slugs),
        error: () => {
          this.panoramasPedidos = false;
        },
      });
  }

  vizinhos(slug: string): { anterior?: LivroListItem; proximo?: LivroListItem } {
    const lista = this._livros();
    const i = lista.findIndex((l) => l.slug === slug);
    if (i === -1) return {};
    return { anterior: lista[i - 1], proximo: lista[i + 1] };
  }
}
