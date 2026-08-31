import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'bt-secao-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <a class="secao-card" [routerLink]="rota()">
      @if (!disponivel()) {
        <span class="secao-card__selo">Em breve</span>
      }
      <h3 class="secao-card__titulo">{{ titulo() }}</h3>
      <p class="secao-card__descricao">{{ descricao() }}</p>
    </a>
  `,
  styleUrl: './secao-card.component.scss',
})
export class SecaoCardComponent {
  readonly titulo = input.required<string>();
  readonly descricao = input.required<string>();
  readonly rota = input.required<string>();
  readonly disponivel = input<boolean>(false);
}
