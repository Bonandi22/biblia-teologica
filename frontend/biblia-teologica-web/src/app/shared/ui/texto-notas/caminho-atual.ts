import { Signal, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs';

/**
 * URL da página atual sem o fragmento, para compor o href das chamadas de nota.
 * Com `<base href="/">`, um href só com `#nota-1` resolve para a home: Ctrl+clique,
 * botão do meio e "copiar link" levavam para lá. Precisa de contexto de injeção.
 */
export function caminhoAtual(): Signal<string> {
  const router = inject(Router);
  const ler = () => router.url.split('#')[0];
  return toSignal(
    router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(ler),
    ),
    { initialValue: ler() },
  );
}
