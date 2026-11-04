import { ViewportScroller } from '@angular/common';
import { inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, NavigationStart, Router } from '@angular/router';

/** As abas do livro são a mesma página: `/biblia/isaias/contexto` conta como `/biblia/isaias`. */
function chaveDaPagina(url: string): string {
  const caminho = url.split(/[?#]/)[0];
  return /^\/biblia\/[^/]+/.exec(caminho)?.[0] ?? caminho;
}

/**
 * Leva ao topo ao abrir outra página, sem mexer na troca de aba do mesmo livro nem no botão voltar,
 * que fica com a restauração do navegador. O `scrollPositionRestoration` do Angular não permite
 * essa exceção das abas.
 */
export function rolarAoTopoAoTrocarDePagina(): void {
  const scroller = inject(ViewportScroller);
  let paginaAtual: string | null = null;
  let voltando = false;

  inject(Router)
    .events.pipe(takeUntilDestroyed())
    .subscribe((evento) => {
      if (evento instanceof NavigationStart) {
        voltando = evento.navigationTrigger === 'popstate';
      } else if (evento instanceof NavigationEnd) {
        const pagina = chaveDaPagina(evento.urlAfterRedirects);
        if (paginaAtual !== null && pagina !== paginaAtual && !voltando) {
          scroller.scrollToPosition([0, 0]);
        }
        paginaAtual = pagina;
      }
    });
}
