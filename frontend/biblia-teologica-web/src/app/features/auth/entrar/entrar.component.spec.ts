import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { EntrarComponent } from './entrar.component';
import { AuthStore } from '../../../core/auth/auth.store';

describe('EntrarComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function criar() {
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('passa da fase de credenciais para a de código', () => {
    const c = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });
    expect(c.fase()).toBe('codigo');
  });

  it('mostra erro quando o login falha', () => {
    const c = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(c.erro()).toContain('Credenciais');
  });

  it('não submete credenciais inválidas', () => {
    const c = criar();
    c.formCredenciais.setValue({ email: 'invalido', senha: '' });
    c.submeterCredenciais();
    http.expectNone('/api/auth/login');
  });

  it('após 2fa navega para /admin quando é admin', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);

    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });

    c.formCodigo.setValue({ codigo: '123456' });
    c.submeterCodigo();
    http.expectOne('/api/auth/login/2fa').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Admin'], deveTrocarSenha: false },
    });

    expect(TestBed.inject(AuthStore).ehAdmin()).toBe(true);
    expect(nav).toHaveBeenCalledWith('/admin');
  });

  it('após 2fa com deveTrocarSenha vai para /conta/trocar-senha', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });

    c.formCodigo.setValue({ codigo: '123456' });
    c.submeterCodigo();
    http.expectOne('/api/auth/login/2fa').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], deveTrocarSenha: true },
    });

    expect(nav).toHaveBeenCalledWith(['/conta/trocar-senha']);
  });
});
