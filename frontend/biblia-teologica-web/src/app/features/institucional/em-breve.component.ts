import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'bt-em-breve',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './em-breve.component.html',
  styleUrl: './em-breve.component.scss',
})
export class EmBreveComponent {
  private readonly rota = inject(ActivatedRoute);

  // Pressuposto: as 3 rotas ("Em breve") são estáticas, sem params.
  // Se virarem uma rota paramétrica (ex.: :secao), migrar de snapshot para toSignal(data).

  readonly titulo: string = this.rota.snapshot.data['titulo'] ?? 'Em breve';
  readonly descricao: string = this.rota.snapshot.data['descricao'] ?? '';
}
