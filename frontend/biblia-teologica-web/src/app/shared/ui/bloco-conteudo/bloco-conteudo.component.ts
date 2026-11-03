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
        [titulo]="distribuicao().tituloEm === 'explicacao' ? 'Referências' : null"
      >
        <p class="bloco__texto"><bt-texto-notas [texto]="bloco().explicacao" [prefixo]="prefixo()" /></p>
      </bt-linha-estudo>
      @if (bloco().personagens.length > 0) {
        <section class="bloco__secao">
          <h3 class="bloco__titulo">{{ rotuloPersonagens() }}</h3>
          @for (p of bloco().personagens; track $index) {
            <bt-item-estudo
              [item]="p"
              [prefixo]="prefixo()"
              [notas]="distribuicao().personagens[$index]"
              [tituloColuna]="distribuicao().tituloEm === p ? 'Referências' : null"
            />
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
              [tituloColuna]="distribuicao().tituloEm === a.item ? 'Referências' : null"
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
  /** Falso quando a aba já mostrou o título da coluna antes do bloco. */
  readonly comTituloColuna = input<boolean>(true);
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
    // O título da coluna aparece uma vez: na primeira linha que tem coluna.
    const itens = [
      ...b.personagens.map((i, n) => ({ i, notas: personagens[n] })),
      ...b.acontecimentos.map((a, n) => ({ i: a.item, notas: acontecimentos[n] })),
    ];
    const primeiroItem = itens.find((x) => x.notas.length > 0 || x.i.referencias.length > 0)?.i ?? null;
    const tituloEm: Item | 'explicacao' | null = !this.comTituloColuna()
      ? null
      : explicacao.length > 0
        ? 'explicacao'
        : primeiroItem;
    return { explicacao, personagens, acontecimentos, restantes, tituloEm };
  });
}
