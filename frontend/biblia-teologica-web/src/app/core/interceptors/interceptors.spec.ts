import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { csrfInterceptor } from './csrf.interceptor';
import { refreshInterceptor } from './refresh.interceptor';
import { withCredentialsInterceptor } from './with-credentials.interceptor';
import { AuthStore } from '../auth/auth.store';

describe('interceptors', () => {
  let http: HttpClient;
  let ctrl: HttpTestingController;

  beforeEach(() => {
    document.cookie = 'bt_csrf=CSRF123';
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(
          withInterceptors([withCredentialsInterceptor, csrfInterceptor, refreshInterceptor]),
        ),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    ctrl = TestBed.inject(HttpTestingController);
  });

  afterEach(() => ctrl.verify());

  it('withCredentials: liga withCredentials em pedidos /api', () => {
    http.get('/api/livros').subscribe();
    const req = ctrl.expectOne('/api/livros');
    expect(req.request.withCredentials).toBe(true);
    req.flush([]);
  });

  it('csrf: adiciona X-CSRF-Token em POST /api', () => {
    http.post('/api/x', {}).subscribe();
    const req = ctrl.expectOne('/api/x');
    expect(req.request.headers.get('X-CSRF-Token')).toBe('CSRF123');
    req.flush({});
  });

  it('csrf: não adiciona header em GET', () => {
    http.get('/api/x').subscribe();
    const req = ctrl.expectOne('/api/x');
    expect(req.request.headers.has('X-CSRF-Token')).toBe(false);
    req.flush({});
  });

  it('refresh: em 401 tenta refresh (com CSRF) e repete o pedido', () => {
    http.get('/api/admin/livros').subscribe({ next: () => {}, error: () => {} });
    ctrl.expectOne('/api/admin/livros').flush(null, { status: 401, statusText: 'Unauthorized' });

    const refresh = ctrl.expectOne('/api/auth/refresh');
    expect(refresh.request.headers.get('X-CSRF-Token')).toBe('CSRF123');
    refresh.flush({
      usuario: { email: 'a', nomeExibicao: 'a', roles: [], deveTrocarSenha: false },
    });

    ctrl.expectOne('/api/admin/livros').flush([]);
  });

  it('refresh: em 401 + refresh falhado limpa o store e navega para /entrar', () => {
    const store = TestBed.inject(AuthStore);
    store.definir({ email: 'a@b.c', nomeExibicao: 'A', roles: [], deveTrocarSenha: false });
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    http.get('/api/admin/livros').subscribe({ next: () => {}, error: () => {} });
    ctrl.expectOne('/api/admin/livros').flush(null, { status: 401, statusText: 'Unauthorized' });
    ctrl.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(nav).toHaveBeenCalledWith(['/entrar']);
    expect(store.usuario()).toBeNull();
  });

  it('refresh: não intercepta 401 de /api/auth/* (login inválido)', () => {
    let erro: number | undefined;
    http.post('/api/auth/login', {}).subscribe({
      next: () => {},
      error: (e) => (erro = e.status),
    });
    ctrl.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    ctrl.expectNone('/api/auth/refresh');
    expect(erro).toBe(401);
  });
});
