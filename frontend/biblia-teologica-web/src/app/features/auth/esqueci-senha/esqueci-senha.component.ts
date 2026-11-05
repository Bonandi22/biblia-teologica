import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../../shared/ui/campo/campo.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { AuthService } from '../../../core/auth/auth.service';
import { mensagemDeErro } from '../../../core/auth/erros-auth';

const MENSAGEM = 'Se houver uma conta com esse e-mail, enviamos um link válido por 1 hora.';

@Component({
  selector: 'bt-esqueci-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    AlertaComponent,
    BotaoComponent,
    BtInputDirective,
    CampoComponent,
  ],
  templateUrl: './esqueci-senha.component.html',
  styleUrl: './esqueci-senha.component.scss',
})
export class EsqueciSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly mensagem = MENSAGEM;
  readonly enviado = signal(false);
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    site: [''],
  });

  constructor() {
    inject(Title).setTitle('Esqueci minha senha | Bíblia Teológica');
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  submeter(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { email, site } = this.form.getRawValue();
    this.auth
      .esqueciSenha(email.trim(), site)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.enviado.set(true);
          this.aEnviar.set(false);
        },
        // Só o 429 é dito: o servidor responde 202 igual com ou sem conta.
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível enviar agora. Tente de novo.'));
          this.aEnviar.set(false);
        },
      });
  }
}
