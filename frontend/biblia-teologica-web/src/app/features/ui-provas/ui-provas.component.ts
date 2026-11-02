import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { AlertaComponent } from '../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../shared/ui/campo/campo.component';
import { CampoSenhaComponent } from '../../shared/ui/campo-senha/campo-senha.component';
import { CartaoComponent } from '../../shared/ui/cartao/cartao.component';

@Component({
  selector: 'bt-ui-provas',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    AlertaComponent,
    BotaoComponent,
    BtInputDirective,
    CampoComponent,
    CampoSenhaComponent,
    CartaoComponent,
  ],
  templateUrl: './ui-provas.component.html',
  styleUrl: './ui-provas.component.scss',
})
export class UiProvasComponent {
  readonly senha = new FormControl('');
}
