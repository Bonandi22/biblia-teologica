import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Bloco } from '../../../core/models/conteudo.model';
import { BibliografiaComponent } from '../bibliografia/bibliografia.component';
import { ItemEstudoComponent } from '../item-estudo/item-estudo.component';
import { TextoNotasComponent } from '../texto-notas/texto-notas.component';

@Component({
  selector: 'bt-bloco-conteudo',
  standalone: true,
  imports: [BibliografiaComponent, ItemEstudoComponent, TextoNotasComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="bloco">
      <p class="bloco__explicacao"><bt-texto-notas [texto]="bloco().explicacao" [prefixo]="prefixo()" /></p>
      @if (bloco().personagens.length > 0) {
        <section class="bloco__secao">
          <h3 class="bloco__titulo">{{ rotuloPersonagens() }}</h3>
          @for (p of bloco().personagens; track $index) {
            <bt-item-estudo [item]="p" [prefixo]="prefixo()" />
          }
        </section>
      }
      @if (bloco().acontecimentos.length > 0) {
        <section class="bloco__secao">
          <h3 class="bloco__titulo">{{ rotuloAcontecimentos() }}</h3>
          @for (a of bloco().acontecimentos; track $index) {
            <bt-item-estudo [item]="a.item" [quando]="a.quando" [prefixo]="prefixo()" />
          }
        </section>
      }
      <bt-bibliografia [notas]="bloco().notas" [prefixo]="prefixo()" />
    </div>
  `,
  styleUrl: './bloco-conteudo.component.scss',
})
export class BlocoConteudoComponent {
  readonly bloco = input.required<Bloco>();
  readonly prefixo = input<string>('');
  readonly rotuloPersonagens = input<string>('Personagens');
  readonly rotuloAcontecimentos = input<string>('Acontecimentos');
}
