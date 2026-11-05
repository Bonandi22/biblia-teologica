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
import { VerificarCodigoComponent } from '../../../shared/ui/verificar-codigo/verificar-codigo.component';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';
import { caminhoInterno } from '../../../core/auth/caminho-interno';
import { MUITAS_TENTATIVAS } from '../../../core/auth/erros-auth';

/** Só o 401 fala da senha: dizer "credenciais inválidas" num 429 faz a pessoa duvidar de dados corretos. */
function mensagemDeFalhaNoLogin(status: number): string {
  if (status === 401) return 'Credenciais inválidas.';
  if (status === 423) return 'Conta bloqueada temporariamente.';
  if (status === 429) return MUITAS_TENTATIVAS;
  if (status === 503) return 'Não conseguimos enviar o código agora. Tente de novo em alguns minutos.';
  return 'Não foi possível entrar agora. Tente de novo.';
}

const AVISOS = new Map<string, string>([
  [
    'cadastro',
    'O tempo do código acabou. Para concluir o cadastro, use “Esqueci minha senha” com o e-mail que você informou.',
  ],
  ['senha', 'Senha alterada. Entre com a nova senha.'],
]);

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
    VerificarCodigoComponent,
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
  readonly tokenDesafio = signal('');
  /** Só caminhos internos; qualquer outro valor é descartado. */
  protected readonly retorno = caminhoInterno(this.rota.snapshot.queryParamMap.get('retorno'));

  /** Texto fixo para um valor conhecido de `?aviso=`; nunca ecoa o que veio na URL. */
  protected readonly aviso = AVISOS.get(this.rota.snapshot.queryParamMap.get('aviso') ?? '') ?? null;
  protected readonly avisoComLinkDeSenha = this.rota.snapshot.queryParamMap.get('aviso') === 'cadastro';

  readonly formCredenciais = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
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
        this.tokenDesafio.set(r.tokenDesafio);
        this.fase.set('codigo');
        this.aEnviar.set(false);
      },
      error: (e) => {
        this.erro.set(mensagemDeFalhaNoLogin(e.status));
        this.aEnviar.set(false);
      },
    });
  }

  submeterCodigo(codigo: string): void {
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth
      .verificar2fa(this.tokenDesafio(), codigo)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.aEnviar.set(false);
          if (this.store.deveTrocarSenha()) {
            void this.router.navigate(['/conta/trocar-senha']);
            return;
          }
          // A rota /admin ainda não existe: o admin segue a mesma regra até a etapa 3.
          void this.router.navigateByUrl(this.retorno ?? '/');
        },
        error: (e) => {
          this.erro.set(e.status === 429 ? MUITAS_TENTATIVAS : 'Código inválido ou expirado.');
          this.aEnviar.set(false);
        },
      });
  }

  /** Pedir outro código é entrar com as credenciais de novo. */
  voltar(): void {
    this.tokenDesafio.set('');
    this.erro.set(null);
    this.fase.set('credenciais');
  }

  desafioVenceu(): void {
    this.voltar();
    this.erro.set('O tempo do código acabou. Entre de novo para receber outro.');
  }
}
