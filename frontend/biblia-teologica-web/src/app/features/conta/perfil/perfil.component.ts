import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ContaService } from '../../../core/services/conta.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'bt-conta-perfil',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
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
    effect(() => {
      const p = this.perfil();
      if (p) this.formNome.patchValue({ nomeExibicao: p.nomeExibicao });
    });
  }

  guardarNome(): void {
    if (this.formNome.invalid) return;
    this.conta
      .atualizarNome(this.formNome.getRawValue().nomeExibicao)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.msgNome.set('Nome guardado.'),
        error: () => this.msgNome.set('Erro ao guardar.'),
      });
  }

  guardarEmail(): void {
    if (this.formEmail.invalid) return;
    this.conta
      .trocarEmail(this.formEmail.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.msgEmail.set('Email alterado. Use-o no próximo login.'),
        error: (e) => this.msgEmail.set(e.error?.detail ?? 'Não foi possível alterar o email.'),
      });
  }

  sair(): void {
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => void this.router.navigate(['/']));
  }
}
