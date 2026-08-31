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

  readonly titulo: string = this.rota.snapshot.data['titulo'] ?? 'Em breve';
  readonly descricao: string = this.rota.snapshot.data['descricao'] ?? '';
}
