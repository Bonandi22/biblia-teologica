import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { AuthStore } from '../../auth/auth.store';
import { LugarAtualService } from '../lugar-atual.service';
import { NAV_PRINCIPAL } from '../nav';

@Component({
  selector: 'bt-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet, RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  host: {
    '(document:keydown.escape)': 'fecharMenu()',
  },
})
export class HeaderComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly autenticado = this.auth.estaAutenticado;
  readonly lugar = inject(LugarAtualService).texto;
  private readonly _menuAberto = signal(false);
  readonly menuAberto = this._menuAberto.asReadonly();

  readonly itens = computed(() =>
    NAV_PRINCIPAL.filter((item) => (item.membros ? this.autenticado() : true)),
  );

  constructor() {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this._menuAberto.set(false));
  }

  alternarMenu(): void {
    this._menuAberto.update((v) => !v);
  }

  fecharMenu(): void {
    this._menuAberto.set(false);
  }
}
