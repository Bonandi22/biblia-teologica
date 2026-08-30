import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TrocarSenhaComponent } from './trocar-senha.component';
import { AuthStore } from '../../../core/auth/auth.store';

describe('TrocarSenhaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TrocarSenhaComponent],
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
    const fixture = TestBed.createComponent(TrocarSenhaComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('não submete quando as senhas não coincidem', () => {
    const c = criar();
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'nova12345', confirmar: 'outra12345' });
    c.submeter();
    http.expectNone('/api/conta/senha');
    expect(c.form.hasError('diferentes')).toBe(true);
  });

  it('no sucesso navega para /conta (não-admin)', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'nova12345', confirmar: 'nova12345' });
    c.submeter();
    http.expectOne('/api/conta/senha').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], deveTrocarSenha: false },
    });
    expect(nav).toHaveBeenCalledWith('/conta');
    expect(TestBed.inject(AuthStore).usuario()?.deveTrocarSenha).toBe(false);
  });

  it('no sucesso navega para /admin quando é admin', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'nova12345', confirmar: 'nova12345' });
    c.submeter();
    http.expectOne('/api/conta/senha').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Admin'], deveTrocarSenha: false },
    });
    expect(nav).toHaveBeenCalledWith('/admin');
  });

  it('mostra o detalhe do erro em falha', () => {
    const c = criar();
    c.form.setValue({ senhaAtual: 'errada12', novaSenha: 'nova12345', confirmar: 'nova12345' });
    c.submeter();
    http
      .expectOne('/api/conta/senha')
      .flush({ detail: 'Senha atual incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.erro()).toBe('Senha atual incorreta.');
  });
});
