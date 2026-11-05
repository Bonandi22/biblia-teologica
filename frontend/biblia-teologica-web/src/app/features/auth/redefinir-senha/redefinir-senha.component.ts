import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AlertaComponent } from '../../../shared/ui/alerta/alerta.component';
import { BotaoComponent } from '../../../shared/ui/botao/botao.component';
import { erroCampo } from '../../../shared/ui/campo/erro-campo';
import { senhaForte } from '../../../shared/ui/campo/senha-forte.validator';
import { CampoSenhaComponent } from '../../../shared/ui/campo-senha/campo-senha.component';
import { AuthService } from '../../../core/auth/auth.service';
import { mensagemDeErro } from '../../../core/auth/erros-auth';

const LINK_INVALIDO = 'Este link é inválido ou venceu.';

/** Lê `email` e `token` do fragmento; qualquer defeito (falta, vazio, percent-encoding quebrado) é null. */
function lerFragmento(fragmento: string | null | undefined): { email: string; token: string } | null {
  try {
    const campos = new Map<string, string>();
    for (const par of (fragmento ?? '').split('&')) {
      const i = par.indexOf('=');
      if (i < 1) continue;
      campos.set(decodeURIComponent(par.slice(0, i)), decodeURIComponent(par.slice(i + 1)));
    }
    const email = campos.get('email')?.trim();
    const token = campos.get('token');
    return email && token ? { email, token } : null;
  } catch {
    return null;
  }
}

@Component({
  selector: 'bt-redefinir-senha',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, AlertaComponent, BotaoComponent, CampoSenhaComponent],
  templateUrl: './redefinir-senha.component.html',
  styleUrl: './redefinir-senha.component.scss',
})
export class RedefinirSenhaComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  // Guardados aqui, fora de qualquer signal ou template: o fragmento sai da barra já no carregamento.
  private readonly dados = lerFragmento(inject(ActivatedRoute).snapshot.fragment);

  protected readonly linkInvalido = signal(this.dados === null);
  protected readonly mensagemInvalido = LINK_INVALIDO;
  readonly erro = signal<string | null>(null);
  readonly aEnviar = signal(false);

  readonly form = this.fb.nonNullable.group({
    senha: ['', [Validators.required, senhaForte()]],
  });

  constructor() {
    inject(Title).setTitle('Nova senha | Bíblia Teológica');
    // O token não deve ficar no histórico nem na barra de endereços.
    history.replaceState(history.state, '', location.pathname + location.search);
  }

  protected erroDe(controle: AbstractControl): string | null {
    return erroCampo(controle);
  }

  submeter(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.dados) return;
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth
      .redefinirSenha({ email: this.dados.email, token: this.dados.token, novaSenha: this.form.getRawValue().senha })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.aEnviar.set(false);
          void this.router.navigate(['/entrar'], { queryParams: { aviso: 'senha' } });
        },
        error: (e) => {
          this.aEnviar.set(false);
          const mensagem = mensagemDeErro(e, 'Não foi possível trocar a senha agora. Tente de novo.');
          // Link ruim e senha fraca são ambos 400; o texto fixo do servidor separa os dois.
          if (mensagem === LINK_INVALIDO) this.linkInvalido.set(true);
          else this.erro.set(mensagem);
        },
      });
  }
}
