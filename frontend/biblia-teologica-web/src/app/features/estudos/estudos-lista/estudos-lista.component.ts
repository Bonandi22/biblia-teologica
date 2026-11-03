import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import type { EstudoResumo } from '../../../core/models/conteudo.model';
import { ConteudoService } from '../../../core/services/conteudo.service';

type Estado = 'carregando' | 'pronto' | 'erro';

@Component({
  selector: 'bt-estudos-lista',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './estudos-lista.component.html',
  styleUrl: './estudos-lista.component.scss',
})
export class EstudosListaComponent {
  private readonly conteudo = inject(ConteudoService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly estado = signal<Estado>('carregando');
  protected readonly estudos = signal<EstudoResumo[]>([]);

  constructor() {
    this.carregar();
  }

  protected carregar(): void {
    this.estado.set('carregando');
    this.conteudo
      .listarEstudos()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (lista) => {
          this.estudos.set(lista);
          this.estado.set('pronto');
        },
        error: () => this.estado.set('erro'),
      });
  }
}
