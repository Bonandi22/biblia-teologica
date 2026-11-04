import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';
import { IndiceCanonComponent } from '../../../shared/ui/indice-canon/indice-canon.component';

@Component({
  selector: 'bt-biblia-lista',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [IndiceCanonComponent],
  templateUrl: './biblia-lista.component.html',
  styleUrl: './biblia-lista.component.scss',
})
export class BibliaListaComponent {
  protected readonly busca = signal('');

  constructor() {
    inject(Title).setTitle('Livros | Bíblia Teológica');
    const lugar = inject(LugarAtualService);
    lugar.definir('Os 66 livros');
    inject(DestroyRef).onDestroy(() => lugar.limpar());
  }
}
