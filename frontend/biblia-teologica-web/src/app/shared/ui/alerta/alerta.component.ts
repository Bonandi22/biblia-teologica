import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'bt-alerta',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [class]="classe()" [attr.role]="tipo() === 'erro' ? 'alert' : 'status'">
      <ng-content />
    </div>
  `,
  styleUrl: './alerta.component.scss',
})
export class AlertaComponent {
  readonly tipo = input<'erro' | 'sucesso'>('erro');
  readonly classe = computed(() => `alerta alerta--${this.tipo()}`);
}
