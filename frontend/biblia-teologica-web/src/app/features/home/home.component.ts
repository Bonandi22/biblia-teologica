import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';

interface CardSecao {
  titulo: string;
  descricao: string;
  rota: string | null;
}

@Component({
  selector: 'bt-home',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  private readonly auth = inject(AuthStore);
  readonly autenticado = this.auth.estaAutenticado;

  readonly livrosDestaque = [
    'Génesis',
    'Salmos',
    'Isaías',
    'Mateus',
    'João',
    'Romanos',
    'Hebreus',
    'Apocalipse',
  ];

  readonly secoes: CardSecao[] = [
    {
      titulo: 'Estudos',
      descricao:
        'Leituras longas, feitas para ir devagar, com as fontes consultadas listadas no fim de cada uma.',
      rota: '/estudos',
    },
    {
      titulo: 'Notícias',
      descricao: 'O que se passa na igreja e no mundo, lido à luz da Bíblia.',
      rota: null,
    },
    {
      titulo: 'Curiosidades',
      descricao: 'Descobertas sobre o texto bíblico, línguas originais e contexto.',
      rota: null,
    },
  ];
}
