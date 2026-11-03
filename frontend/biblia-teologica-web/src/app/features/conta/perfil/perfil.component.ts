import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { BtInputDirective } from '../../../shared/ui/bt-input.directive';
import { CampoComponent } from '../../../shared/ui/campo/campo.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { ContaService } from '../../../core/services/conta.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'bt-conta-perfil',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
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
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly perfil = toSignal(this.conta.obter());
  readonly msgNome = signal<string | null>(null);
  readonly msgEmail = signal<string | null>(null);

  readonly formNome = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
  });
  readonly formEmail = this.fb.nonNullable.group({
    novoEmail: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });

  constructor() {
    inject(Title).setTitle('Minha conta | Bíblia Teológica');
    effect(() => {
      const p = this.perfil();
      if (p) this.formNome.patchValue({ nomeExibicao: p.nomeExibicao });
    });
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  guardarNome(): void {
    this.formNome.markAllAsTouched();
    if (this.formNome.invalid) return;
    this.conta
      .atualizarNome(this.formNome.getRawValue().nomeExibicao)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.msgNome.set('Nome salvo.'),
        error: () => this.msgNome.set('Não foi possível salvar o nome. Tente de novo.'),
      });
  }

  guardarEmail(): void {
    this.formEmail.markAllAsTouched();
    if (this.formEmail.invalid) return;
    this.conta
      .trocarEmail(this.formEmail.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.msgEmail.set('E-mail alterado. Use-o no próximo login.'),
        error: (e) => this.msgEmail.set(e.error?.detail ?? 'Não foi possível alterar o e-mail.'),
      });
  }

  sair(): void {
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => void this.router.navigate(['/']));
  }
}
