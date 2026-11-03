import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Citacao } from '../../../../core/models/conteudo.model';
import { LinhaEstudoComponent } from '../../../../shared/ui/linha-estudo/linha-estudo.component';
import { ReferenciaComponent } from '../../../../shared/ui/referencia/referencia.component';
import { TextoNotasComponent } from '../../../../shared/ui/texto-notas/texto-notas.component';

@Component({
  selector: 'bt-citacoes',
  standalone: true,
  imports: [LinhaEstudoComponent, ReferenciaComponent, TextoNotasComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './citacoes.component.html',
  styleUrl: './citacoes.component.scss',
})
export class CitacoesComponent {
  readonly citacoes = input.required<Citacao[]>();
}
