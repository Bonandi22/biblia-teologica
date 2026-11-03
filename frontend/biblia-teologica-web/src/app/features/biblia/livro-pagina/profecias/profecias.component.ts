import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { Profecia } from '../../../../core/models/conteudo.model';
import { BibliografiaComponent } from '../../../../shared/ui/bibliografia/bibliografia.component';
import { TextoNotasComponent } from '../../../../shared/ui/texto-notas/texto-notas.component';

const ROTULOS: Record<string, string> = {
  cumpridaEmCristo: 'Cumprida em Cristo',
  cumpridaNaHistoria: 'Cumprida na história',
  promessaAguardada: 'Promessa ainda aguardada',
};

@Component({
  selector: 'bt-profecias',
  standalone: true,
  imports: [TextoNotasComponent, BibliografiaComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profecias.component.html',
  styleUrl: './profecias.component.scss',
})
export class ProfeciasComponent {
  readonly profecias = input.required<Profecia[]>();

  protected rotulo(categoria: string): string {
    return ROTULOS[categoria] ?? categoria;
  }
}
