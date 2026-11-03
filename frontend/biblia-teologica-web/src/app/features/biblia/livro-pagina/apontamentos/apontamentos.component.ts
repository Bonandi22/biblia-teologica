import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Apontamento } from '../../../../core/models/conteudo.model';
import { BibliografiaComponent } from '../../../../shared/ui/bibliografia/bibliografia.component';
import { ReferenciaComponent } from '../../../../shared/ui/referencia/referencia.component';
import { TextoNotasComponent } from '../../../../shared/ui/texto-notas/texto-notas.component';

/** Profecias ou promessas: só aponta o trecho, na ordem do livro, sem categoria nem cumprimento. */
@Component({
  selector: 'bt-apontamentos',
  standalone: true,
  imports: [ReferenciaComponent, TextoNotasComponent, BibliografiaComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './apontamentos.component.html',
  styleUrl: './apontamentos.component.scss',
})
export class ApontamentosComponent {
  readonly itens = input.required<Apontamento[]>();
  /** Distingue a numeração das notas entre abas e itens: id `nota-<prefixo>-<i>-<n>`. */
  readonly prefixo = input.required<string>();
}
