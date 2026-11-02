import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-cartao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="cartao">
      @if (titulo()) {
        <h1 class="cartao__titulo">{{ titulo() }}</h1>
      }
      <ng-content />
    </section>
  `,
  styleUrl: './cartao.component.scss',
})
export class CartaoComponent {
  readonly titulo = input<string>('');
}
