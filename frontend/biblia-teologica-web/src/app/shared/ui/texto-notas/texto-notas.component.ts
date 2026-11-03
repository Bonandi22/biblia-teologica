import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type SegmentoTexto = { texto: string } | { nota: number };

/** Divide o texto nos marcadores `[^n]`; qualquer outro colchete segue como texto comum. */
export function segmentarNotas(texto: string): SegmentoTexto[] {
  const segmentos: SegmentoTexto[] = [];
  const re = /\[\^(\d+)\]/g;
  let ultimo = 0;
  let m: RegExpExecArray | null;
  while ((m = re.exec(texto)) !== null) {
    if (m.index > ultimo) segmentos.push({ texto: texto.slice(ultimo, m.index) });
    segmentos.push({ nota: Number(m[1]) });
    ultimo = m.index + m[0].length;
  }
  if (ultimo < texto.length) segmentos.push({ texto: texto.slice(ultimo) });
  return segmentos;
}

@Component({
  selector: 'bt-texto-notas',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@for (s of segmentos(); track $index) {@if ('nota' in s) {<sup class="nota"><a
            [href]="'#nota-' + s.nota"
            [attr.aria-label]="'Nota ' + s.nota"
            (click)="ir($event, s.nota)"
            >{{ s.nota }}</a></sup>} @else {{{ s.texto }}}}`,
  styleUrl: './texto-notas.component.scss',
})
export class TextoNotasComponent {
  readonly texto = input.required<string>();
  protected readonly segmentos = computed(() => segmentarNotas(this.texto()));

  protected ir(evento: Event, numero: number): void {
    evento.preventDefault();
    document.getElementById(`nota-${numero}`)?.scrollIntoView({ block: 'center' });
  }
}
