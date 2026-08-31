import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'bt-footer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  readonly ano = new Date().getFullYear();

  private readonly _mensagem = signal('');
  readonly mensagem = this._mensagem.asReadonly();

  readonly redes = [
    { nome: 'Facebook', url: '#' },
    { nome: 'Instagram', url: '#' },
    { nome: 'YouTube', url: '#' },
  ];

  submeterNewsletter(evento: Event): void {
    evento.preventDefault();
    this._mensagem.set('A newsletter ainda não está disponível — em breve.');
  }
}
