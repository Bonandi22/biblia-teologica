import { Directive, signal } from '@angular/core';

@Directive({
  selector: 'input[btInput], textarea[btInput]',
  standalone: true,
  host: {
    class: 'bt-input',
    '[id]': 'id()',
    '[attr.aria-invalid]': "invalido() ? 'true' : null",
    '[attr.aria-describedby]': 'describedBy()',
  },
})
export class BtInputDirective {
  readonly id = signal<string>('');
  readonly invalido = signal(false);
  readonly describedBy = signal<string | null>(null);
}
