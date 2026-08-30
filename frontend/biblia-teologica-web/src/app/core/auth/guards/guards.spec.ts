import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { adminGuard, authGuard } from './auth.guard';
import { AuthStore } from '../auth.store';

function run(guard: typeof authGuard) {
  return TestBed.runInInjectionContext(() =>
    guard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
  );
}

function url(r: unknown): string {
  return TestBed.inject(Router).serializeUrl(r as UrlTree);
}

describe('guards', () => {
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('authGuard: hidrata via /api/auth/eu e bloqueia anónimo', async () => {
    const p = run(authGuard);
    http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(url(await p)).toBe('/entrar');
  });

  it('authGuard: hidrata e deixa passar utilizador autenticado', async () => {
    const p = run(authGuard);
    http.expectOne('/api/auth/eu').flush({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    expect(await p).toBe(true);
  });

  it('authGuard: não refaz o pedido quando o store já está carregado', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Membro'], deveTrocarSenha: false });
    expect(await run(authGuard)).toBe(true);
    http.expectNone('/api/auth/eu');
  });

  it('adminGuard: membro vai para /', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Membro'], deveTrocarSenha: false });
    expect(url(await run(adminGuard))).toBe('/');
  });

  it('adminGuard: admin passa', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Admin'], deveTrocarSenha: false });
    expect(await run(adminGuard)).toBe(true);
  });

  it('adminGuard: deveTrocarSenha vai para /conta/trocar-senha', async () => {
    store.definir({ email: 'a', nomeExibicao: 'a', roles: ['Admin'], deveTrocarSenha: true });
    expect(url(await run(adminGuard))).toBe('/conta/trocar-senha');
  });

  it('adminGuard: anónimo vai para /entrar', async () => {
    const p = run(adminGuard);
    http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(url(await p)).toBe('/entrar');
  });
});
