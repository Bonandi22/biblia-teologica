import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { EntrarComponent } from './entrar.component';
import { AuthStore } from '../../../core/auth/auth.store';
import { Title } from '@angular/platform-browser';

describe('EntrarComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
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

describe('EntrarComponent na tela', () => {
  const montar = () => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('usa os campos próprios, sem Material, e define o título da aba', () => {
    const el = montar().nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Entrar');
    expect(el.querySelectorAll('.bt-input').length).toBe(2);
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.querySelector('bt-botao button[type="submit"]')?.textContent).toContain('Continuar');
    expect(el.querySelector('a[href="/registar"]')?.textContent).toContain('Criar conta');
    expect(TestBed.inject(Title).getTitle()).toBe('Entrar | Bíblia Teológica');
  });

  it('ao enviar vazio, diz o que falta em cada campo', () => {
    const fixture = montar();
    const el = fixture.nativeElement as HTMLElement;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    const erros = Array.from(el.querySelectorAll('.campo__erro')).map((e) => e.textContent!.trim());
    expect(erros).toEqual(['Preencha este campo.', 'Preencha este campo.']);
  });

  it('avisa em português do Brasil que o código foi enviado', () => {
    const fixture = montar();
    const c = fixture.componentInstance;
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/auth/login')
      .flush({ desafio2fa: true, tokenDesafio: 'T' });
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Enviamos um código para o seu e-mail.');
  });
});
