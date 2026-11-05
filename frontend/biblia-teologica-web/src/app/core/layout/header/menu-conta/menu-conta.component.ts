import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/auth.service';
import { AuthStore } from '../../../auth/auth.store';
import { iniciais } from '../iniciais';

/**
 * Conta no topo. Em "menu" é o botão com iniciais e o menu que abre dele;
 * em "lista" os mesmos itens entram direto, para o painel do celular.
 */
@Component({
  selector: 'bt-menu-conta',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet, RouterLink],
  templateUrl: './menu-conta.component.html',
  styleUrl: './menu-conta.component.scss',
  host: {
    '(document:keydown.escape)': 'aoEscape()',
    '(document:click)': 'aoClicar($event)',
  },
})
export class MenuContaComponent {
  private readonly store = inject(AuthStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly raiz = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly botao = viewChild<ElementRef<HTMLButtonElement>>('botao');

  readonly modo = input<'menu' | 'lista'>('menu');

  readonly nome = computed(() => this.store.usuario()?.nomeExibicao ?? '');
  readonly iniciais = computed(() => iniciais(this.nome()));
  readonly aberto = signal(false);
  readonly aSair = signal(false);
  readonly erroAoSair = signal<string | null>(null);

  alternar(): void {
    this.aberto.update((v) => !v);
  }

  fechar(devolverFoco = true): void {
    if (!this.aberto()) return;
    this.aberto.set(false);
    if (devolverFoco) this.botao()?.nativeElement.focus();
  }

  aoEscape(): void {
    this.fechar();
  }

  aoClicar(e: Event): void {
    if (this.aberto() && !this.raiz.nativeElement.contains(e.target as Node)) {
      this.fechar(false);
    }
  }

  sair(): void {
    this.erroAoSair.set(null);
    this.aSair.set(true);
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.aSair.set(false);
          this.aberto.set(false);
          void this.router.navigate(['/']);
        },
        // A sessão só termina no servidor: limpar aqui faria parecer que saiu com os cookies ainda válidos.
        error: () => {
          this.erroAoSair.set('Não foi possível sair. Tente de novo.');
          this.aSair.set(false);
        },
      });
  }
}
