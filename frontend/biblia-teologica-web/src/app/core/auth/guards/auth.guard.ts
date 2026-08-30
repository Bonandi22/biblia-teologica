import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth.service';
import { AuthStore } from '../auth.store';

// `inject()` só funciona no tick síncrono do guard — resolver tudo aqui e
// passar as instâncias, para não chamar `inject()` depois de um `await`.
async function garantirCarregado(store: AuthStore, service: AuthService): Promise<void> {
  if (!store.carregado()) {
    await firstValueFrom(service.carregarUsuario());
  }
}

export const authGuard: CanActivateFn = async () => {
  const store = inject(AuthStore);
  const service = inject(AuthService);
  const router = inject(Router);

  await garantirCarregado(store, service);
  return store.estaAutenticado() ? true : router.parseUrl('/entrar');
};

export const adminGuard: CanActivateFn = async () => {
  const store = inject(AuthStore);
  const service = inject(AuthService);
  const router = inject(Router);

  await garantirCarregado(store, service);
  if (!store.estaAutenticado()) return router.parseUrl('/entrar');
  if (store.deveTrocarSenha()) return router.parseUrl('/conta/trocar-senha');
  return store.ehAdmin() ? true : router.parseUrl('/');
};
