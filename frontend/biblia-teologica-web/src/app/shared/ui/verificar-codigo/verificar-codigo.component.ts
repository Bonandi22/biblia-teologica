import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { AlertaComponent } from '../alerta/alerta.component';
import { BotaoComponent } from '../botao/botao.component';
import { BtInputDirective } from '../bt-input.directive';
import { CampoComponent } from '../campo/campo.component';
import { erroCampo } from '../campo/erro-campo';

/** O desafio do servidor vale 5 minutos; passado isso, o código nunca mais serve. */
const VALIDADE_DESAFIO_MS = 5 * 60 * 1000;

/**
 * Fase "Digite o código", partilhada por entrar e criar conta. Quem a usa guarda o token e
 * decide o que fazer com o código (`enviar`), com o "Voltar" e com o desafio vencido.
 */
@Component({
  selector: 'bt-verificar-codigo',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, AlertaComponent, BotaoComponent, BtInputDirective, CampoComponent],
  templateUrl: './verificar-codigo.component.html',
  styleUrl: './verificar-codigo.component.scss',
})
export class VerificarCodigoComponent {
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tokenDesafio = input.required<string>();
  readonly aEnviar = input(false);
  readonly erro = input<string | null>(null);
  /** No cadastro, a mesma frase para todos explica o caso do e-mail que já tem conta, sem revelar nada. */
  readonly cadastro = input(false);

  readonly enviar = output<string>();
  readonly voltar = output<void>();
  readonly vencido = output<void>();

  protected readonly codigo = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(4)],
  });
  protected readonly form = new FormGroup({ codigo: this.codigo });
  protected readonly reenviando = signal(false);
  protected readonly aviso = signal<string | null>(null);
  protected readonly erroReenvio = signal<string | null>(null);

  constructor() {
    const temporizador = setTimeout(() => this.vencido.emit(), VALIDADE_DESAFIO_MS);
    this.destroyRef.onDestroy(() => clearTimeout(temporizador));
  }

  protected erroDoCampo(): string | null {
    return erroCampo(this.codigo);
  }

  protected submeter(): void {
    this.codigo.markAsTouched();
    if (this.codigo.invalid) return;
    this.enviar.emit(this.codigo.value.trim());
  }

  protected reenviar(): void {
    this.reenviando.set(true);
    this.aviso.set(null);
    this.erroReenvio.set(null);
    this.auth
      .reenviarCodigo(this.tokenDesafio())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reenviando.set(false);
          this.aviso.set('Enviamos outro código. Se ele não chegar em alguns minutos, tente de novo.');
        },
        error: (e: HttpErrorResponse) => {
          this.reenviando.set(false);
          if (e.status === 400) {
            this.vencido.emit();
            return;
          }
          this.erroReenvio.set(
            e.status === 429
              ? 'Muitos envios. Aguarde alguns minutos para pedir outro código.'
              : 'Não foi possível reenviar agora. Tente de novo.',
          );
        },
      });
  }
}
