import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RESPONSAVEL_DADOS, VERSAO_TERMO } from './responsavel-dados';

// RASCUNHO: texto escrito sem parecer jurídico. Precisa de revisão por quem
// entenda de RGPD antes de abrir o site ao público.
@Component({
  selector: 'bt-privacidade',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './privacidade.component.html',
  styleUrl: './privacidade.component.scss',
})
export class PrivacidadeComponent {
  readonly responsavel = RESPONSAVEL_DADOS;
  readonly versao = VERSAO_TERMO;
  readonly dataVersao = '5 de novembro de 2026';
}
