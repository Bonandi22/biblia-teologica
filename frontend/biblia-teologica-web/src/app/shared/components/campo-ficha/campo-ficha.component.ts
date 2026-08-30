import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'bt-campo-ficha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="campo">
      <span class="campo__rotulo">{{ rotulo() }}</span>
      <div class="campo__valor"><ng-content /></div>
    </div>
  `,
  styleUrl: './campo-ficha.component.scss',
})
export class CampoFichaComponent {
  readonly rotulo = input.required<string>();
}
