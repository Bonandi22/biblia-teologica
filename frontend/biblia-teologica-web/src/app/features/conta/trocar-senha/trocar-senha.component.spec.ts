import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
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
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'Nova12345', confirmar: 'Outra12345' });
    c.submeter();
    http.expectNone('/api/conta/senha');
    expect(c.form.hasError('diferentes')).toBe(true);
  });

  it('trocarSenha_exigeSenhaForte', () => {
    const c = criar();
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'tudominusculo1', confirmar: 'tudominusculo1' });
    c.submeter();
    http.expectNone('/api/conta/senha');
    expect(c.form.controls.novaSenha.hasError('senhaForte')).toBe(true);

    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'Forte12345', confirmar: 'Forte12345' });
    expect(c.form.controls.novaSenha.valid).toBe(true);
  });

  it('no sucesso navega para /conta (não-admin)', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'Nova12345', confirmar: 'Nova12345' });
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
    c.form.setValue({ senhaAtual: 'atual123', novaSenha: 'Nova12345', confirmar: 'Nova12345' });
    c.submeter();
    http.expectOne('/api/conta/senha').flush({
      usuario: { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Admin'], deveTrocarSenha: false },
    });
    expect(nav).toHaveBeenCalledWith('/admin');
  });

  it('mostra o detalhe do erro em falha', () => {
    const c = criar();
    c.form.setValue({ senhaAtual: 'errada12', novaSenha: 'Nova12345', confirmar: 'Nova12345' });
    c.submeter();
    http
      .expectOne('/api/conta/senha')
      .flush({ detail: 'Senha atual incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.erro()).toBe('Senha atual incorreta.');
  });
});

describe('TrocarSenhaComponent na tela', () => {
  const montar = () => {
    TestBed.configureTestingModule({
      imports: [TrocarSenhaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(TrocarSenhaComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('usa os campos próprios', () => {
    const el = montar().nativeElement as HTMLElement;
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.querySelectorAll('bt-campo-senha').length).toBe(3);
  });

  it('diz quando as senhas não coincidem, no campo de confirmação', () => {
    const fixture = montar();
    const c = fixture.componentInstance;
    c.form.setValue({ senhaAtual: 'atual', novaSenha: 'Segredo123', confirmar: 'outra' });
    const el = fixture.nativeElement as HTMLElement;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(el.textContent).toContain('As senhas não coincidem.');
    TestBed.inject(HttpTestingController).expectNone('/api/conta/senha');
  });
});
