import { ChangeDetectionStrategy, Component, ElementRef, computed, inject, input } from '@angular/core';
import type { Nota } from '../../../core/models/conteudo.model';
import { ReferenciaComponent } from '../referencia/referencia.component';
import { idNota } from '../texto-notas/texto-notas.component';

const DESTAQUE = 'is-destaque';

/**
 * Assinatura do site: cada parágrafo ou item é uma linha de grade, com as próprias referências
 * e notas ao lado, como a coluna central de uma Bíblia de estudo. No celular, a coluna desce.
 */
@Component({
  selector: 'bt-linha-estudo',
  standalone: true,
  imports: [ReferenciaComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './linha-estudo.component.html',
  styleUrl: './linha-estudo.component.scss',
  host: {
    '(mouseover)': 'destacar($event)',
    '(focusin)': 'destacar($event)',
    '(mouseout)': 'soltarMouse()',
    '(focusout)': 'limpar()',
  },
})
export class LinhaEstudoComponent {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly referencias = input<string[]>([]);
  readonly notas = input<Nota[]>([]);
  readonly prefixo = input<string>('');
  /** Título da coluna, mostrado só onde ajuda a ler (em geral, a primeira linha). */
  readonly titulo = input<string | null>(null);

  protected readonly temColuna = computed(
    () => this.referencias().length > 0 || this.notas().length > 0,
  );

  protected id(numero: number): string {
    return idNota(numero, this.prefixo());
  }

  /** Liga a chamada à nota, nos dois sentidos, mesmo quando a nota está em outra linha do bloco. */
  protected destacar(evento: Event): void {
    this.limpar();
    this.marcar(evento.target as Element | null);
  }

  /** O mouse saiu: o destaque só continua no que ainda tem foco. */
  protected soltarMouse(): void {
    this.limpar();
    this.marcar(this.el.nativeElement.ownerDocument.activeElement);
  }

  private marcar(alvo: Element | null): void {
    const chamada = alvo?.closest?.('a[href^="#nota-"]');
    const nota = alvo?.closest?.('li[id^="nota-"]');
    const id = chamada?.getAttribute('href')?.slice(1) ?? nota?.id;
    if (!id) return;
    const doc = this.el.nativeElement.ownerDocument;
    doc.getElementById(id)?.classList.add(DESTAQUE);
    doc.querySelectorAll(`a[href="#${CSS.escape(id)}"]`).forEach((a) => a.classList.add(DESTAQUE));
  }

  protected limpar(): void {
    this.el.nativeElement.ownerDocument
      .querySelectorAll(`.${DESTAQUE}`)
      .forEach((e) => e.classList.remove(DESTAQUE));
  }
}
