import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PerfilComponent } from './perfil.component';
import { AuthStore } from '../../../core/auth/auth.store';

const PERFIL = {
  email: 'a@b.c',
  nomeExibicao: 'Ana',
  roles: ['Membro'],
  criadoEm: '2026-01-01T00:00:00Z',
};

describe('PerfilComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('mostra o email do perfil', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('a@b.c');
  });

  it('pré-preenche o form do nome com o perfil', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(fixture.componentInstance.formNome.getRawValue().nomeExibicao).toBe('Ana');
  });

  it('guardarNome faz PUT e mostra confirmação', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();

    const c = fixture.componentInstance;
    c.formNome.setValue({ nomeExibicao: 'Ana Maria' });
    c.guardarNome();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nomeExibicao: 'Ana Maria' });
    req.flush(null);
    expect(c.avisoNome()).toEqual({ tipo: 'sucesso', texto: 'Nome salvo.' });
  });

  it('carregar_MostraErroETentaDeNovo_QuandoPerfilFalha', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(null, { status: 500, statusText: 'Erro' });
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível carregar');
    expect(el.textContent).not.toContain('Carregando');

    el.querySelector<HTMLButtonElement>('.tentar button')!.click();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(el.querySelector('[role="alert"]')).toBeNull();
    expect(el.textContent).toContain('a@b.c');
  });

  it('sair_VaiParaInicial_QuandoLogoutPassa', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture.componentInstance.sair();
    http.expectOne('/api/auth/logout').flush(null);
    expect(nav).toHaveBeenCalledWith(['/']);
  });

  it('sair_AvisaEContinuaNaConta_QuandoLogoutFalha', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    const store = TestBed.inject(AuthStore);
    store.definir({ email: 'a@b.c', nomeExibicao: 'Ana', roles: [], deveTrocarSenha: false });
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture.componentInstance.sair();
    expect(fixture.componentInstance.aSair()).toBe(true);
    http.expectOne('/api/auth/logout').flush(null, { status: 401, statusText: 'Unauthorized' });
    fixture.detectChanges();

    expect(nav).not.toHaveBeenCalled();
    expect(store.usuario()).not.toBeNull();
    expect(fixture.componentInstance.aSair()).toBe(false);
    expect((fixture.nativeElement as HTMLElement).querySelector('[role="alert"]')?.textContent).toContain(
      'Não foi possível sair',
    );
  });

  it('guardarEmail_AtualizaEmailELimpaSenha_QuandoPassa', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();

    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'segredo' });
    c.guardarEmail();
    expect(c.aEnviarEmail()).toBe(true);
    http.expectOne('/api/conta/email').flush(null);
    fixture.detectChanges();

    expect(c.aEnviarEmail()).toBe(false);
    expect(c.formEmail.getRawValue()).toEqual({ novoEmail: '', senha: '' });
    expect((fixture.nativeElement as HTMLElement).querySelector('.dados')?.textContent).toContain('novo@b.c');
  });

  it('guardarNome_LimpaAvisoAnterior_QuandoReenvia', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);

    const c = fixture.componentInstance;
    c.formNome.setValue({ nomeExibicao: 'Ana Maria' });
    c.guardarNome();
    http.expectOne('/api/conta').flush(null);
    c.guardarNome();
    expect(c.avisoNome()).toBeNull();
    expect(c.aEnviarNome()).toBe(true);
    http.expectOne('/api/conta').flush(null, { status: 500, statusText: 'Erro' });
    expect(c.avisoNome()?.tipo).toBe('erro');
    expect(c.aEnviarNome()).toBe(false);
  });

  it('guardarEmail mostra o detalhe do erro em falha', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();

    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'x' });
    c.guardarEmail();
    http
      .expectOne('/api/conta/email')
      .flush({ detail: 'Senha incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.avisoEmail()).toEqual({ tipo: 'erro', texto: 'Senha incorreta.' });
  });
});

describe('PerfilComponent na tela', () => {
  it('usa os componentes próprios, com textos em português do Brasil', () => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Minha conta');
    expect(el.textContent).toContain('Carregando');
    TestBed.inject(HttpTestingController)
      .expectOne('/api/conta')
      .flush({ email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'] });
    fixture.detectChanges();
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.querySelectorAll('.bt-input').length).toBe(3);
    expect(el.textContent).toContain('Salvar nome');
    expect(el.textContent).not.toContain('Guardar');
    expect(el.querySelector('a[href$="trocar-senha"]')?.textContent).toContain('Trocar senha');
  });
});
