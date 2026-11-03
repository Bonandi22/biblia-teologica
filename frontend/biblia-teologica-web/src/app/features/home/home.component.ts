import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import type { EstudoResumo } from '../../core/models/conteudo.model';
import { ConteudoService } from '../../core/services/conteudo.service';
import { LivroStore } from '../../core/services/livro.store';
import { IndiceCanonComponent } from '../../shared/ui/indice-canon/indice-canon.component';

@Component({
  selector: 'bt-home',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, IndiceCanonComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  private readonly store = inject(LivroStore);

  protected readonly estudos = signal<EstudoResumo[]>([]);

  /** Primeiro livro do cânon com panorama; sem panorama publicado, a guarda não oferece link. */
  protected readonly primeiroPanorama = computed(() => {
    const comPanorama = new Set(this.store.comPanorama());
    return this.store.livros().find((l) => comPanorama.has(l.slug)) ?? null;
  });

  constructor() {
    inject(Title).setTitle('Bíblia Teológica');
    this.store.carregar();
    this.store.carregarPanoramas();
    inject(ConteudoService)
      .listarEstudos()
      .pipe(takeUntilDestroyed(inject(DestroyRef)))
      .subscribe({
        next: (e) => this.estudos.set(e),
        // Sem a lista, a seção some: o índice continua sendo o conteúdo principal.
        error: () => this.estudos.set([]),
      });
  }
}
