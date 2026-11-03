import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Nota } from '../../../core/models/conteudo.model';
import { idNota } from '../texto-notas/texto-notas.component';

@Component({
  selector: 'bt-bibliografia',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (notas().length > 0) {
      <section class="biblio">
        <h3 class="biblio__titulo">Bibliografia</h3>
        <ol class="biblio__lista">
          @for (n of notas(); track n.numero) {
            <li class="biblio__item" [id]="id(n.numero)" [attr.value]="n.numero">
              {{ n.citacao }}
              @if (n.url) {
                <a [href]="n.url" target="_blank" rel="noopener noreferrer">Abrir fonte</a>
              }
            </li>
          }
        </ol>
      </section>
    }
  `,
  styleUrl: './bibliografia.component.scss',
})
export class BibliografiaComponent {
  readonly notas = input<Nota[]>([]);
  readonly prefixo = input<string>('');

  protected id(numero: number): string {
    return idNota(numero, this.prefixo());
  }
}
