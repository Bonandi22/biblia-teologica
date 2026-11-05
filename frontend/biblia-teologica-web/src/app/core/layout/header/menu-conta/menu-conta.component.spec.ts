import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MenuContaComponent } from './menu-conta.component';
import { AuthStore } from '../../../auth/auth.store';

const usuario = (nomeExibicao: string) => ({
  email: 'a@b.c',
  nomeExibicao,
  roles: ['Membro'],
  deveTrocarSenha: false,
});

describe('MenuContaComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MenuContaComponent],
      providers: [
        provideRouter([
          { path: 'conta', children: [] },
          { path: '', children: [] },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    TestBed.inject(AuthStore).definir(usuario('Maria Souza'));
  });

  const criar = () => {
    const fixture = TestBed.createComponent(MenuContaComponent);
    document.body.appendChild(fixture.nativeElement);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const botao = el.querySelector('.avatar') as HTMLButtonElement;
    return { fixture, el, botao };
  };

  afterEach(() => {
    document.body.innerHTML = '';
  });

  it('menuConta_abreEFecha_comEscDevolvendoFoco', () => {
    const { fixture, el, botao } = criar();
    expect(botao.getAttribute('aria-expanded')).toBe('false');
    botao.click();
    fixture.detectChanges();
    expect(botao.getAttribute('aria-expanded')).toBe('true');
    expect(el.querySelector('.menu')).not.toBeNull();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(botao.getAttribute('aria-expanded')).toBe('false');
    expect(el.querySelector('.menu')).toBeNull();
    expect(document.activeElement).toBe(botao);
  });

  it('menuConta_fecha_quandoCliqueFora', () => {
    const { fixture, botao } = criar();
    botao.click();
    fixture.detectChanges();
    document.body.click();
    fixture.detectChanges();
    expect(botao.getAttribute('aria-expanded')).toBe('false');
  });

  it('menuConta_mostraNomeEIniciais_quandoAberto', () => {
    const { fixture, el, botao } = criar();
    expect(botao.textContent?.trim()).toBe('MS');
    botao.click();
    fixture.detectChanges();
    expect(el.querySelector('.menu__nome')?.textContent?.trim()).toBe('Maria Souza');
  });

  it('menuConta_navegaParaPerfil_eFecha', async () => {
    const { fixture, el, botao } = criar();
    botao.click();
    fixture.detectChanges();
    (el.querySelector('a[href="/conta"]') as HTMLAnchorElement).click();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(TestBed.inject(Router).url).toBe('/conta');
    expect(botao.getAttribute('aria-expanded')).toBe('false');
  });

  it('menuConta_saiENavegaParaRaiz_quandoClicaSair', async () => {
    const { fixture, el, botao } = criar();
    const http = TestBed.inject(HttpTestingController);
    await TestBed.inject(Router).navigateByUrl('/conta');
    botao.click();
    fixture.detectChanges();
    (el.querySelector('.menu button') as HTMLButtonElement).click();
    http.expectOne('/api/auth/logout').flush(null);
    await fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/');
    expect(TestBed.inject(AuthStore).estaAutenticado()).toBe(false);
  });

  it('menuConta_mantemSessaoEMostraErro_quandoLogoutFalha', () => {
    const { fixture, el, botao } = criar();
    const http = TestBed.inject(HttpTestingController);
    botao.click();
    fixture.detectChanges();
    (el.querySelector('.menu button') as HTMLButtonElement).click();
    http.expectOne('/api/auth/logout').flush(null, { status: 500, statusText: 'Erro' });
    fixture.detectChanges();
    expect(TestBed.inject(AuthStore).estaAutenticado()).toBe(true);
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível sair');
  });

  it('menuConta_atualizaIniciais_quandoNomeMudaNaStore', () => {
    const { fixture, botao } = criar();
    TestBed.inject(AuthStore).definir(usuario('João Pedro Alves'));
    fixture.detectChanges();
    expect(botao.textContent?.trim()).toBe('JA');
  });
});
