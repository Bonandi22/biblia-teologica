import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-badge-secao',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge-secao">{{ rotulo() }}</span>`,
  styleUrl: './badge-secao.component.scss',
})
export class BadgeSecaoComponent {
  readonly rotulo = input.required<string>();
}
