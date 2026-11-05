import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../../shared/ui/campo/campo.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { ContaService, Perfil } from '../../../core/services/conta.service';
import { AuthStore } from '../../../core/auth/auth.store';
import { mensagemDeErro } from '../../../core/auth/erros-auth';

interface Aviso {
  tipo: 'erro' | 'sucesso';
  texto: string;
}

const ULTIMO_ADMIN = 'O último administrador não pode apagar a própria conta.';

@Component({
  selector: 'bt-conta-perfil',
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
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.scss',
})
export class PerfilComponent {
  private readonly fb = inject(FormBuilder);
  private readonly conta = inject(ContaService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly perfil = signal<Perfil | null>(null);
  readonly falhouAoCarregar = signal(false);

  readonly avisoPerfil = signal<Aviso | null>(null);
  readonly aSalvar = signal(false);

  readonly faseEmail = signal<'dados' | 'codigo'>('dados');
  readonly emailPendente = signal('');
  readonly erroEmail = signal<string | null>(null);
  readonly avisoEmail = signal<string | null>(null);
  readonly aEnviarEmail = signal(false);

  readonly apagando = signal(false);
  readonly erroApagar = signal<string | null>(null);
  readonly aApagar = signal(false);

  readonly formPerfil = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
    telefone: [''],
  });
  readonly formEmail = this.fb.nonNullable.group({
    novoEmail: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });
  readonly formCodigo = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.minLength(4)]],
  });
  readonly formApagar = this.fb.nonNullable.group({
    senha: ['', Validators.required],
  });

  constructor() {
    inject(Title).setTitle('Meu perfil | Bíblia Teológica');
    this.carregar();
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  carregar(): void {
    this.falhouAoCarregar.set(false);
    this.conta
      .obter()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (p) => {
          this.perfil.set(p);
          this.formPerfil.patchValue({ nomeExibicao: p.nomeExibicao, telefone: p.telefone ?? '' });
        },
        error: () => this.falhouAoCarregar.set(true),
      });
  }

  salvar(): void {
    this.formPerfil.markAllAsTouched();
    if (this.formPerfil.invalid) return;
    this.avisoPerfil.set(null);
    this.aSalvar.set(true);
    const { nomeExibicao, telefone } = this.formPerfil.getRawValue();
    const dto = { nomeExibicao: nomeExibicao.trim(), telefone: telefone.trim() || null };
    this.conta
      .atualizarPerfil(dto)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          const u = this.store.usuario();
          if (u) this.store.definir({ ...u, nomeExibicao: dto.nomeExibicao });
          this.perfil.update((p) => p && { ...p, nomeExibicao: dto.nomeExibicao, telefone: dto.telefone });
          this.avisoPerfil.set({ tipo: 'sucesso', texto: 'Dados salvos.' });
          this.aSalvar.set(false);
        },
        error: (e: HttpErrorResponse) => {
          this.avisoPerfil.set({
            tipo: 'erro',
            texto: mensagemDeErro(e, 'Não foi possível salvar. Tente de novo.'),
          });
          this.aSalvar.set(false);
        },
      });
  }

  pedirCodigo(): void {
    this.formEmail.markAllAsTouched();
    if (this.formEmail.invalid) return;
    this.erroEmail.set(null);
    this.avisoEmail.set(null);
    this.aEnviarEmail.set(true);
    const dados = this.formEmail.getRawValue();
    this.conta
      .solicitarTrocaEmail({ novoEmail: dados.novoEmail.trim(), senha: dados.senha })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.emailPendente.set(dados.novoEmail.trim());
          this.formEmail.controls.senha.reset('');
          this.formCodigo.reset();
          this.faseEmail.set('codigo');
          this.aEnviarEmail.set(false);
        },
        error: (e: HttpErrorResponse) => {
          this.erroEmail.set(mensagemDeErro(e, 'Não foi possível pedir o código. Tente de novo.'));
          this.aEnviarEmail.set(false);
        },
      });
  }

  confirmarCodigo(): void {
    this.formCodigo.markAllAsTouched();
    if (this.formCodigo.invalid) return;
    this.erroEmail.set(null);
    this.aEnviarEmail.set(true);
    this.conta
      .confirmarTrocaEmail({
        novoEmail: this.emailPendente(),
        codigo: this.formCodigo.getRawValue().codigo.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (u) => {
          this.perfil.update((p) => p && { ...p, email: u.email });
          this.formEmail.reset();
          this.formCodigo.reset();
          this.faseEmail.set('dados');
          this.avisoEmail.set('E-mail alterado. Use o novo endereço no próximo login.');
          this.aEnviarEmail.set(false);
        },
        error: (e: HttpErrorResponse) => {
          this.erroEmail.set(mensagemDeErro(e, 'Não foi possível confirmar o código. Tente de novo.'));
          this.aEnviarEmail.set(false);
        },
      });
  }

  cancelarEmail(): void {
    this.erroEmail.set(null);
    this.formCodigo.reset();
    this.faseEmail.set('dados');
  }

  pedirApagar(): void {
    this.erroApagar.set(null);
    this.formApagar.reset();
    this.apagando.set(true);
  }

  cancelarApagar(): void {
    this.erroApagar.set(null);
    this.formApagar.reset();
    this.apagando.set(false);
  }

  apagar(): void {
    this.formApagar.markAllAsTouched();
    if (this.formApagar.invalid) return;
    this.erroApagar.set(null);
    this.aApagar.set(true);
    this.conta
      .apagar(this.formApagar.getRawValue().senha)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.store.limpar();
          void this.router.navigate(['/']);
        },
        error: (e: HttpErrorResponse) => {
          const padrao = e.status === 409 ? ULTIMO_ADMIN : 'Não foi possível apagar a conta. Tente de novo.';
          this.erroApagar.set(mensagemDeErro(e, padrao));
          this.aApagar.set(false);
        },
      });
  }
}
