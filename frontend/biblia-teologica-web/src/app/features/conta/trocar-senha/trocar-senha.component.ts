import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ContaService } from '../../../core/services/conta.service';
import { AuthStore } from '../../../core/auth/auth.store';

function iguais(g: AbstractControl): { diferentes: true } | null {
  return g.get('novaSenha')!.value === g.get('confirmar')!.value ? null : { diferentes: true };
}

@Component({
  selector: 'bt-trocar-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './trocar-senha.component.html',
  styleUrl: './trocar-senha.component.scss',
})
export class TrocarSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly conta = inject(ContaService);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      senhaAtual: ['', Validators.required],
      novaSenha: ['', [Validators.required, Validators.minLength(8)]],
      confirmar: ['', Validators.required],
    },
    { validators: iguais },
  );

  submeter(): void {
    if (this.form.invalid) return;
    const { senhaAtual, novaSenha } = this.form.getRawValue();
    this.conta
      .trocarSenha({ senhaAtual, novaSenha })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => void this.router.navigateByUrl(this.store.ehAdmin() ? '/admin' : '/conta'),
        error: (e) => this.erro.set(e.error?.detail ?? 'Não foi possível trocar a senha.'),
      });
  }
}
