import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { Item } from '../../../core/models/conteudo.model';
import { TextoNotasComponent } from '../texto-notas/texto-notas.component';

@Component({
  selector: 'bt-item-estudo',
  standalone: true,
  imports: [RouterLink, TextoNotasComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="item">
      @if (quando()) {
        <p class="item__quando">{{ quando() }}</p>
      }
      @if (item().nome) {
        <h4 class="item__nome">{{ item().nome }}</h4>
      }
      <p class="item__texto"><bt-texto-notas [texto]="item().texto" [prefixo]="prefixo()" /></p>
      @if (item().referencias.length > 0) {
        <p class="item__refs">{{ item().referencias.join('; ') }}</p>
      }
      @if (item().saberMais; as mais) {
        <details class="item__mais">
          <summary>Saber mais</summary>
          <p><bt-texto-notas [texto]="mais" [prefixo]="prefixo()" /></p>
        </details>
      }
      @if (item().estudoSlug; as slug) {
        <a class="item__estudo" [routerLink]="['/estudos', slug]">Estudo completo</a>
      }
    </article>
  `,
  styleUrl: './item-estudo.component.scss',
})
export class ItemEstudoComponent {
  readonly item = input.required<Item>();
  readonly quando = input<string>('');
  readonly prefixo = input<string>('');
}
