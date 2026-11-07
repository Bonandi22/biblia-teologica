import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { caminhoAtual } from '../texto-notas/caminho-atual';
import { idNota, irParaNota } from '../texto-notas/texto-notas.component';
import { renderizarMarkdown } from './markdown';

const HREF_NOTA = /#nota-(\d+)$/;

const escaparAtributo = (s: string) => s.replace(/&/g, '&amp;').replace(/"/g, '&quot;');

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
  private readonly caminho = caminhoAtual();
  // O HTML sanitizado traz href="#nota-n"; o caminho da página entra depois (ver caminhoAtual).
  protected readonly html = computed(() =>
    renderizarMarkdown(this.conteudo()).replaceAll(
      'href="#nota-',
      `href="${escaparAtributo(this.caminho())}#nota-`,
    ),
  );

  protected aoClicar(evento: Event): void {
    const link = (evento.target as Element | null)?.closest?.('a');
    // O sanitizador do Angular remove data-*; o número vem do href, restrito a terminar em #nota-<dígitos>.
    const m = HREF_NOTA.exec(link?.getAttribute('href') ?? '');
    if (!m) return;
    evento.preventDefault();
    irParaNota(idNota(Number(m[1]), this.prefixo()));
  }
}
