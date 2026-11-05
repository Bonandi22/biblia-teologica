import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { AuthStore } from './auth.store';

describe('AuthService', () => {
  let svc: AuthService;
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    svc = TestBed.inject(AuthService);
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  it('login devolve o desafio', () => {
    let td: string | undefined;
    svc.login('a@b.c', 'x').subscribe((r) => (td = r.tokenDesafio));
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T1' });
    expect(td).toBe('T1');
  });

  it('verificar2fa popula o store', () => {
    svc.verificar2fa('T1', '123456').subscribe();
    http.expectOne('/api/auth/login/2fa').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Admin'], deveTrocarSenha: false },
    });
    expect(store.usuario()?.email).toBe('a@b.c');
    expect(store.ehAdmin()).toBe(true);
  });

  it('carregarUsuario com 401 limpa o store e marca carregado', () => {
    svc.carregarUsuario().subscribe();
    http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(store.usuario()).toBeNull();
    expect(store.carregado()).toBe(true);
  });

  describe('carregarUsuario com o acesso vencido', () => {
    const USUARIO = { email: 'a@b.c', nomeExibicao: 'Ana', roles: [], deveTrocarSenha: false };
    const comSessao = () => {
      document.cookie = 'bt_csrf=abc; path=/';
    };

    afterEach(() => {
      document.cookie = 'bt_csrf=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
      http.verify();
    });

    it('carregarUsuario_RenovaASessao_QuandoRefreshValido', () => {
      comSessao();
      let devolvido: unknown;
      svc.carregarUsuario().subscribe((u) => (devolvido = u));
      http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
      http.expectOne('/api/auth/refresh').flush({ usuario: USUARIO });
      expect(store.usuario()?.email).toBe('a@b.c');
      expect(store.carregado()).toBe(true);
      expect(devolvido).toEqual(USUARIO);
    });

    it('carregarUsuario_DaPorDeslogado_QuandoRefreshTambemFalha', () => {
      comSessao();
      let devolvido: unknown = 'nada';
      svc.carregarUsuario().subscribe((u) => (devolvido = u));
      http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
      http.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
      expect(store.usuario()).toBeNull();
      expect(store.carregado()).toBe(true);
      expect(devolvido).toBeNull();
    });

    it('carregarUsuario_NaoTentaRenovar_QuandoVisitanteSemSessao', () => {
      svc.carregarUsuario().subscribe();
      http.expectOne('/api/auth/eu').flush(null, { status: 401, statusText: 'Unauthorized' });
      http.expectNone('/api/auth/refresh');
      expect(store.carregado()).toBe(true);
    });

    it('carregarUsuario_NaoTentaRenovar_QuandoFalhaNaoE401', () => {
      comSessao();
      svc.carregarUsuario().subscribe();
      http.expectOne('/api/auth/eu').flush(null, { status: 500, statusText: 'Erro' });
      http.expectNone('/api/auth/refresh');
      expect(store.carregado()).toBe(true);
    });
  });

  it('reenviarCodigo_postaOToken_quandoChamado', () => {
    svc.reenviarCodigo('T1').subscribe();
    const req = http.expectOne('/api/auth/login/2fa/reenviar');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ tokenDesafio: 'T1' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('registar_postaOCadastroEDevolveODesafio_quandoChamado', () => {
    const dto = {
      nomeExibicao: 'Ana',
      email: 'a@b.c',
      senha: 'Segredo123',
      aceitouTermo: true,
      telefone: null,
      site: '',
    };
    let td: string | undefined;
    svc.registar(dto).subscribe((r) => (td = r.tokenDesafio));
    const req = http.expectOne('/api/auth/registar');
    expect(req.request.body).toEqual(dto);
    req.flush({ tokenDesafio: 'T9' }, { status: 202, statusText: 'Accepted' });
    expect(td).toBe('T9');
  });

  it('esqueciSenha_postaEmailESite_quandoChamado', () => {
    svc.esqueciSenha('a@b.c', 'https://x').subscribe();
    const req = http.expectOne('/api/auth/senha/esqueci');
    expect(req.request.body).toEqual({ email: 'a@b.c', site: 'https://x' });
    req.flush(null, { status: 202, statusText: 'Accepted' });
  });

  it('redefinirSenha_postaOsDados_quandoChamado', () => {
    svc.redefinirSenha({ email: 'a@b.c', token: 'tk', novaSenha: 'Nova#123' }).subscribe();
    const req = http.expectOne('/api/auth/senha/redefinir');
    expect(req.request.body).toEqual({ email: 'a@b.c', token: 'tk', novaSenha: 'Nova#123' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('logout limpa o store', () => {
    store.definir({ email: 'a@b.c', nomeExibicao: 'A', roles: [], deveTrocarSenha: false });
    svc.logout().subscribe();
    http.expectOne('/api/auth/logout').flush(null);
    expect(store.usuario()).toBeNull();
  });
});
