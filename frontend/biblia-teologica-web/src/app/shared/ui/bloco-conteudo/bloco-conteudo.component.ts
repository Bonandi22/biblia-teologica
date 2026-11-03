import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { Bloco, Item, Nota } from '../../../core/models/conteudo.model';
import { ItemEstudoComponent } from '../item-estudo/item-estudo.component';
import { LinhaEstudoComponent } from '../linha-estudo/linha-estudo.component';
import { notasDoTexto } from '../texto-notas/notas-do-texto';
import { TextoNotasComponent } from '../texto-notas/texto-notas.component';

@Component({
  selector: 'bt-bloco-conteudo',
  standalone: true,
  imports: [ItemEstudoComponent, LinhaEstudoComponent, TextoNotasComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="bloco">
      <bt-linha-estudo
        class="bloco__explicacao"
        [notas]="distribuicao().explicacao"
        [prefixo]="prefixo()"
        [titulo]="distribuicao().explicacao.length > 0 ? 'Referências' : null"
      >
        <p class="bloco__texto"><bt-texto-notas [texto]="bloco().explicacao" [prefixo]="prefixo()" /></p>
      </bt-linha-estudo>
      @if (bloco().personagens.length > 0) {
        <section class="bloco__secao">
          <h3 class="bloco__titulo">{{ rotuloPersonagens() }}</h3>
          @for (p of bloco().personagens; track $index) {
            <bt-item-estudo [item]="p" [prefixo]="prefixo()" [notas]="distribuicao().personagens[$index]" />
          }
        </section>
      }
      @if (bloco().acontecimentos.length > 0) {
        <section class="bloco__secao">
          <h3 class="bloco__titulo">{{ rotuloAcontecimentos() }}</h3>
          @for (a of bloco().acontecimentos; track $index) {
            <bt-item-estudo
              [item]="a.item"
              [quando]="a.quando"
              [prefixo]="prefixo()"
              [notas]="distribuicao().acontecimentos[$index]"
            />
          }
        </section>
      }
      @if (distribuicao().restantes.length > 0) {
        <bt-linha-estudo class="bloco__restantes" [notas]="distribuicao().restantes" [prefixo]="prefixo()" />
      }
    </div>
  `,
  styleUrl: './bloco-conteudo.component.scss',
})
export class BlocoConteudoComponent {
  readonly bloco = input.required<Bloco>();
  readonly prefixo = input<string>('');
  readonly rotuloPersonagens = input<string>('Personagens');
  readonly rotuloAcontecimentos = input<string>('Acontecimentos');

  /** Cada nota vai para a primeira linha que a chama, porque seu id só pode existir uma vez. */
  protected readonly distribuicao = computed(() => {
    const b = this.bloco();
    const usadas = new Set<number>();
    const tomar = (texto: string): Nota[] => {
      const novas = notasDoTexto(texto, b.notas).filter((n) => !usadas.has(n.numero));
      novas.forEach((n) => usadas.add(n.numero));
      return novas;
    };
    const doItem = (i: Item) => tomar(`${i.texto} ${i.saberMais ?? ''}`);
    const explicacao = tomar(b.explicacao);
    const personagens = b.personagens.map(doItem);
    const acontecimentos = b.acontecimentos.map((a) => doItem(a.item));
    const restantes = b.notas.filter((n) => !usadas.has(n.numero));
    return { explicacao, personagens, acontecimentos, restantes };
  });
}
