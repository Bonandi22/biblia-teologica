import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ContaService } from './conta.service';
import { AuthStore } from '../auth/auth.store';

const USUARIO = { email: 'n@b.c', nomeExibicao: 'Ana', roles: [], deveTrocarSenha: false };

describe('ContaService', () => {
  let svc: ContaService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    svc = TestBed.inject(ContaService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('atualizarPerfil_enviaNomeETelefone_quandoChamado', () => {
    svc.atualizarPerfil({ nomeExibicao: 'Ana', telefone: null }).subscribe();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nomeExibicao: 'Ana', telefone: null });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('solicitarTrocaEmail_postaNovoEmailESenha_quandoChamado', () => {
    svc.solicitarTrocaEmail({ novoEmail: 'n@b.c', senha: 'x' }).subscribe();
    const req = http.expectOne('/api/conta/email/solicitar');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ novoEmail: 'n@b.c', senha: 'x' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('confirmarTrocaEmail_atualizaOStore_quandoSucesso', () => {
    let devolvido: unknown;
    svc.confirmarTrocaEmail({ novoEmail: 'n@b.c', codigo: '123456' }).subscribe((u) => (devolvido = u));
    http.expectOne('/api/conta/email/confirmar').flush({ usuario: USUARIO });
    expect(devolvido).toEqual(USUARIO);
    expect(TestBed.inject(AuthStore).usuario()?.email).toBe('n@b.c');
  });

  it('apagar_enviaASenhaNoCorpo_quandoChamado', () => {
    svc.apagar('segredo').subscribe();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('DELETE');
    expect(req.request.body).toEqual({ senha: 'segredo' });
    req.flush(null, { status: 204, statusText: 'No Content' });
  });
});
