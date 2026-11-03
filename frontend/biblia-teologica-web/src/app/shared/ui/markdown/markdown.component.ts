import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { idNota } from '../texto-notas/texto-notas.component';
import { renderizarMarkdown } from './markdown';

const HREF_NOTA = /^#nota-(\d+)$/;

@Component({
  standalone: true,
  selector: 'bt-markdown',
  changeDetection: ChangeDetectionStrategy.OnPush,
  // HTML já sanitizado por DOMPurify; o sanitizador do Angular segue ativo em [innerHTML].
  template: `<div class="markdown" [innerHTML]="html()" (click)="aoClicar($event)"></div>`,
  styleUrl: './markdown.component.scss',
})
export class MarkdownComponent {
  readonly conteudo = input.required<string>();
  /** Mesmo papel que em bt-texto-notas: id do destino `nota-<prefixo>-<n>`. */
  readonly prefixo = input<string>('');
  protected readonly html = computed(() => renderizarMarkdown(this.conteudo()));

  protected aoClicar(evento: Event): void {
    const link = (evento.target as Element | null)?.closest?.('a');
    // O sanitizador do Angular remove data-*; o número vem do href, restrito a #nota-<dígitos>.
    const m = HREF_NOTA.exec(link?.getAttribute('href') ?? '');
    if (!m) return;
    evento.preventDefault();
    document.getElementById(idNota(Number(m[1]), this.prefixo()))?.scrollIntoView({ block: 'center' });
  }
}
