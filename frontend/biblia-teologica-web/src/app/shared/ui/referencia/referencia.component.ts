import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { urlReferencia } from '../../../core/biblia/arc';

/** Referência bíblica: abre a ARC em nova aba; sem link quando não pode ser lida. */
@Component({
  selector: 'bt-referencia',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (url(); as u) {<a
      class="referencia"
      [href]="u"
      target="_blank"
      rel="noopener noreferrer"
      [attr.aria-label]="texto() + ' na ARC, abre em nova aba'"
      >{{ rotulo() ?? texto() }}</a
    >} @else {<span>{{ rotulo() ?? texto() }}</span>}`,
  styleUrl: './referencia.component.scss',
})
export class ReferenciaComponent {
  readonly texto = input.required<string>();
  /** Texto visível, quando difere da referência completa (ex.: "7:3" depois de "Isaías 1:1"). */
  readonly rotulo = input<string | null>(null);
  protected readonly url = computed(() => urlReferencia(this.texto()));
}
