import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../../shared/ui/campo/campo.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';

const MUITAS_TENTATIVAS = 'Muitas tentativas. Aguarde um minuto e tente de novo.';

/** Só o 401 fala da senha: dizer "credenciais inválidas" num 429 faz a pessoa duvidar de dados corretos. */
function mensagemDeFalhaNoLogin(status: number): string {
  if (status === 401) return 'Credenciais inválidas.';
  if (status === 423) return 'Conta bloqueada temporariamente.';
  if (status === 429) return MUITAS_TENTATIVAS;
  return 'Não foi possível entrar agora. Tente de novo.';
}

@Component({
  selector: 'bt-entrar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    AlertaComponent,
    BotaoComponent,
    BtInputDirective,
    CampoComponent,
    CampoSenhaComponent,
  ],
  templateUrl: './entrar.component.html',
  styleUrl: './entrar.component.scss',
})
export class EntrarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly fase = signal<'credenciais' | 'codigo'>('credenciais');
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);
  readonly contaCriada = this.rota.snapshot.queryParamMap.has('registado');
  private tokenDesafio = '';

  readonly formCredenciais = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });
  readonly formCodigo = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.minLength(4)]],
  });

  constructor() {
    inject(Title).setTitle('Entrar | Bíblia Teológica');
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  submeterCredenciais(): void {
    this.formCredenciais.markAllAsTouched();
    if (this.formCredenciais.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { email, senha } = this.formCredenciais.getRawValue();
    this.auth.login(email, senha).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (r) => {
        this.tokenDesafio = r.tokenDesafio;
        this.fase.set('codigo');
        this.aEnviar.set(false);
      },
      error: (e) => {
        this.erro.set(mensagemDeFalhaNoLogin(e.status));
        this.aEnviar.set(false);
      },
    });
  }

  submeterCodigo(): void {
    this.formCodigo.markAllAsTouched();
    if (this.formCodigo.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth
      .verificar2fa(this.tokenDesafio, this.formCodigo.getRawValue().codigo)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: () => {
        this.aEnviar.set(false);
        if (this.store.deveTrocarSenha()) {
          void this.router.navigate(['/conta/trocar-senha']);
          return;
        }
        const retorno = this.rota.snapshot.queryParamMap.get('retorno');
        void this.router.navigateByUrl(retorno ?? (this.store.ehAdmin() ? '/admin' : '/conta'));
      },
      error: (e) => {
        this.erro.set(e.status === 429 ? MUITAS_TENTATIVAS : 'Código inválido ou expirado.');
        this.aEnviar.set(false);
      },
    });
  }

  /** O desafio do 2FA expira e não há reenvio: pedir outro código é entrar com as credenciais de novo. */
  voltar(): void {
    this.tokenDesafio = '';
    this.erro.set(null);
    this.formCodigo.reset();
    this.fase.set('credenciais');
  }
}
