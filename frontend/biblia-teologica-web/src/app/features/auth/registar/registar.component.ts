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
import { senhaForte } from '../../../shared/ui/campo/senha-forte.validator';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { VerificarCodigoComponent } from '../../../shared/ui/verificar-codigo/verificar-codigo.component';
import { AuthService } from '../../../core/auth/auth.service';
import { caminhoInterno } from '../../../core/auth/caminho-interno';
import { mensagemDeErro, MUITAS_TENTATIVAS } from '../../../core/auth/erros-auth';

@Component({
  selector: 'bt-registar',
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
  templateUrl: './registar.component.html',
  styleUrl: './registar.component.scss',
})
export class RegistarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly fase = signal<'dados' | 'codigo'>('dados');
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);
  readonly tokenDesafio = signal('');
  /** Só caminhos internos; qualquer outro valor é descartado. */
  protected readonly retorno = caminhoInterno(this.rota.snapshot.queryParamMap.get('retorno'));

  readonly form = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email]],
    telefone: ['', [Validators.maxLength(30)]],
    senha: ['', [Validators.required, senhaForte()]],
    aceitouTermo: [false, [Validators.requiredTrue]],
    site: [''],
  });

  constructor() {
    inject(Title).setTitle('Criar conta | Bíblia Teológica');
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  protected erroDoTermo(): boolean {
    const c = this.form.controls.aceitouTermo;
    return c.touched && c.invalid;
  }

  submeter(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { telefone, ...resto } = this.form.getRawValue();
    this.auth
      .registar({ ...resto, telefone: telefone.trim() || null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (r) => {
          this.tokenDesafio.set(r.tokenDesafio);
          this.fase.set('codigo');
          this.aEnviar.set(false);
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível criar a conta agora. Tente de novo.'));
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
          void this.router.navigateByUrl(this.retorno ?? '/');
        },
        error: (e) => {
          this.erro.set(e.status === 429 ? MUITAS_TENTATIVAS : 'Código inválido ou expirado.');
          this.aEnviar.set(false);
        },
      });
  }

  /** Voltar refaz o cadastro: o desafio atual não serve para outro e-mail. */
  voltar(): void {
    this.tokenDesafio.set('');
    this.erro.set(null);
    this.fase.set('dados');
  }

  desafioVenceu(): void {
    void this.router.navigate(['/entrar'], {
      queryParams: { aviso: 'cadastro', ...(this.retorno ? { retorno: this.retorno } : {}) },
    });
  }
}
