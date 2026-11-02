import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'bt-botao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      [type]="type()"
      [class]="classe()"
      [disabled]="disabled() || carregando()"
      [attr.aria-busy]="carregando() ? 'true' : null"
    >
      @if (carregando()) {
        <span class="botao__spinner" aria-hidden="true"></span>
      }
      <ng-content />
    </button>
  `,
  styleUrl: './botao.component.scss',
})
export class BotaoComponent {
  readonly variante = input<'primario' | 'fantasma' | 'texto'>('primario');
  readonly type = input<'button' | 'submit'>('button');
  readonly disabled = input(false);
  readonly carregando = input(false);

  readonly classe = computed(() => `botao botao--${this.variante()}`);
}
