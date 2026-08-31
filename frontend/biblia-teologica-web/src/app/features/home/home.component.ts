import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { SecaoCardComponent } from '../../shared/components/secao-card/secao-card.component';

interface CardSecao {
  titulo: string;
  descricao: string;
  rota: string;
}

@Component({
  selector: 'bt-home',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, SecaoCardComponent],
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
      descricao: 'Artigos longos sobre teologia, história da igreja e vida cristã.',
      rota: '/estudos',
    },
    {
      titulo: 'Notícias',
      descricao: 'O que se passa na igreja e no mundo, lido à luz da Bíblia.',
      rota: '/noticias',
    },
    {
      titulo: 'Curiosidades',
      descricao: 'Descobertas sobre o texto bíblico, línguas originais e contexto.',
      rota: '/curiosidades',
    },
  ];
}
