import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Citacao } from '../../../../core/models/conteudo.model';
import { BibliografiaComponent } from '../../../../shared/ui/bibliografia/bibliografia.component';
import { TextoNotasComponent } from '../../../../shared/ui/texto-notas/texto-notas.component';

@Component({
  selector: 'bt-citacoes',
  standalone: true,
  imports: [TextoNotasComponent, BibliografiaComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './citacoes.component.html',
  styleUrl: './citacoes.component.scss',
})
export class CitacoesComponent {
  readonly citacoes = input.required<Citacao[]>();
}
