import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { senhaForte } from '../../../shared/ui/campo/senha-forte.validator';
import { ContaService } from '../../../core/services/conta.service';
import { AuthStore } from '../../../core/auth/auth.store';
import { mensagemDeErro } from '../../../core/auth/erros-auth';

function iguais(g: AbstractControl): { diferentes: true } | null {
  return g.get('novaSenha')!.value === g.get('confirmar')!.value ? null : { diferentes: true };
}

@Component({
  selector: 'bt-trocar-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, AlertaComponent, BotaoComponent, CampoSenhaComponent],
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
  readonly aEnviar = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      senhaAtual: ['', Validators.required],
      novaSenha: ['', [Validators.required, senhaForte()]],
      confirmar: ['', Validators.required],
    },
    { validators: iguais },
  );

  constructor() {
    inject(Title).setTitle('Trocar senha | Bíblia Teológica');
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  protected erroConfirmar(): string | null {
    const confirmar = this.form.controls.confirmar;
    return (
      erroCampo(confirmar) ??
      (confirmar.touched && this.form.hasError('diferentes') ? 'As senhas não coincidem.' : null)
    );
  }

  submeter(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    const { senhaAtual, novaSenha } = this.form.getRawValue();
    this.conta
      .trocarSenha({ senhaAtual, novaSenha })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => void this.router.navigateByUrl(this.store.ehAdmin() ? '/admin' : '/conta'),
        error: (e: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível trocar a senha.'));
          this.aEnviar.set(false);
        },
      });
  }
}
