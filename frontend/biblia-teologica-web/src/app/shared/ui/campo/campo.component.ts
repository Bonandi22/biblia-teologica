import { ChangeDetectionStrategy, Component, contentChild, effect, input } from '@angular/core';
import { BtInputDirective } from '../bt-input.directive';

let contador = 0;

@Component({
  selector: 'bt-campo',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label class="campo__rotulo" [attr.for]="campoId">
      {{ label() }}
      @if (obrigatorio()) {
        <span class="campo__obrig" aria-hidden="true"> *</span>
      }
    </label>
    <ng-content />
    @if (hint() && !erro()) {
      <p class="campo__hint" [id]="hintId">{{ hint() }}</p>
    }
    @if (erro()) {
      <p class="campo__erro" role="alert" [id]="erroId">{{ erro() }}</p>
    }
  `,
  styleUrl: './campo.component.scss',
})
export class CampoComponent {
  readonly label = input.required<string>();
  readonly erro = input<string | null>(null);
  readonly hint = input<string>('');
  readonly obrigatorio = input(false);

  private readonly n = contador++;
  readonly campoId = `bt-campo-${this.n}`;
  readonly erroId = `bt-campo-${this.n}-erro`;
  readonly hintId = `bt-campo-${this.n}-hint`;

  private readonly controlo = contentChild(BtInputDirective);

  constructor() {
    effect(() => {
      const c = this.controlo();
      if (!c) return;
      c.id.set(this.campoId);
      c.invalido.set(!!this.erro());
      c.obrigatorio.set(this.obrigatorio());
      c.describedBy.set(this.erro() ? this.erroId : this.hint() ? this.hintId : null);
    });
  }
}
