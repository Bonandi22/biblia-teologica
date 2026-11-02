import { ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BtInputDirective } from '../bt-input.directive';
import { CampoComponent } from '../campo/campo.component';

@Component({
  selector: 'bt-campo-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CampoComponent, BtInputDirective],
  template: `
    <bt-campo [label]="label()" [erro]="erro()" [hint]="hint()">
      <span class="senha__wrap">
        <input
          btInput
          [type]="visivel() ? 'text' : 'password'"
          [value]="valor()"
          [disabled]="desativado()"
          [attr.autocomplete]="autocomplete()"
          (input)="aoEscrever($any($event.target).value)"
          (blur)="aoTocarCb()"
        />
        <button
          type="button"
          class="senha__toggle"
          [attr.aria-pressed]="visivel()"
          [attr.aria-label]="visivel() ? 'Esconder senha' : 'Mostrar senha'"
          (click)="alternar()"
        >
          {{ visivel() ? 'Esconder' : 'Mostrar' }}
        </button>
      </span>
    </bt-campo>
  `,
  styleUrl: './campo-senha.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CampoSenhaComponent),
      multi: true,
    },
  ],
})
export class CampoSenhaComponent implements ControlValueAccessor {
  readonly label = input.required<string>();
  readonly hint = input<string>('');
  readonly erro = input<string | null>(null);
  readonly autocomplete = input<string>('current-password');

  readonly valor = signal('');
  readonly visivel = signal(false);
  readonly desativado = signal(false);

  private aoMudar: (v: string) => void = () => {};
  aoTocarCb: () => void = () => {};

  writeValue(v: string | null): void {
    this.valor.set(v ?? '');
  }
  registerOnChange(fn: (v: string) => void): void {
    this.aoMudar = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.aoTocarCb = fn;
  }
  setDisabledState(d: boolean): void {
    this.desativado.set(d);
  }

  aoEscrever(v: string): void {
    this.valor.set(v);
    this.aoMudar(v);
  }
  alternar(): void {
    this.visivel.update((x) => !x);
  }
}
