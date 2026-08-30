import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'bt-entrar',
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
  templateUrl: './entrar.component.html',
  styleUrl: './entrar.component.scss',
})
export class EntrarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  readonly fase = signal<'credenciais' | 'codigo'>('credenciais');
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);
  private tokenDesafio = '';

  readonly formCredenciais = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });
  readonly formCodigo = this.fb.nonNullable.group({
    codigo: ['', [Validators.required, Validators.minLength(4)]],
  });

  submeterCredenciais(): void {
    if (this.formCredenciais.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { email, senha } = this.formCredenciais.getRawValue();
    this.auth.login(email, senha).subscribe({
      next: (r) => {
        this.tokenDesafio = r.tokenDesafio;
        this.fase.set('codigo');
        this.aEnviar.set(false);
      },
      error: (e) => {
        this.erro.set(
          e.status === 423 ? 'Conta bloqueada temporariamente.' : 'Credenciais inválidas.',
        );
        this.aEnviar.set(false);
      },
    });
  }

  submeterCodigo(): void {
    if (this.formCodigo.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth.verificar2fa(this.tokenDesafio, this.formCodigo.getRawValue().codigo).subscribe({
      next: () => {
        this.aEnviar.set(false);
        if (this.store.deveTrocarSenha()) {
          void this.router.navigate(['/conta/trocar-senha']);
          return;
        }
        const retorno = this.rota.snapshot.queryParamMap.get('retorno');
        void this.router.navigateByUrl(retorno ?? (this.store.ehAdmin() ? '/admin' : '/conta'));
      },
      error: () => {
        this.erro.set('Código inválido ou expirado.');
        this.aEnviar.set(false);
      },
    });
  }
}
