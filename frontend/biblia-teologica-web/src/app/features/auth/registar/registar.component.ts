import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'bt-registar',
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
  templateUrl: './registar.component.html',
  styleUrl: './registar.component.scss',
})
export class RegistarComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);

  readonly form = this.fb.nonNullable.group({
    nomeExibicao: ['', [Validators.required, Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(8)]],
  });

  submeter(): void {
    if (this.form.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth.registar(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigate(['/entrar'], { queryParams: { registado: 1 } }),
      error: (e) => {
        this.erro.set(e.error?.detail ?? 'Não foi possível criar a conta.');
        this.aEnviar.set(false);
      },
    });
  }
}
