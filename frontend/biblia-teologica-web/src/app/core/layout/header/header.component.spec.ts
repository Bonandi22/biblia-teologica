import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { HeaderComponent } from './header.component';
import { AuthStore } from '../../auth/auth.store';
import { LugarAtualService } from '../lugar-atual.service';

describe('HeaderComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra Livros e Estudos, sem Notícias nem Curiosidades', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/biblia"]')?.textContent?.trim()).toBe('Livros');
    expect(el.querySelector('a[href="/estudos"]')?.textContent?.trim()).toBe('Estudos');
    expect(el.textContent).not.toContain('Notícias');
    expect(el.textContent).not.toContain('Curiosidades');
  });

  it('a marca leva à página inicial', () => {
    const marca = (criar().nativeElement as HTMLElement).querySelector('.header__marca');
    expect(marca?.getAttribute('href')).toBe('/');
    expect(marca?.textContent?.trim()).toBe('Bíblia Teológica');
  });

  it('mostra o lugar atual definido no serviço', () => {
    TestBed.inject(LugarAtualService).definir('Isaías 1–66');
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('.header__lugar')?.textContent?.trim()).toBe('Isaías 1–66');
  });

  it('header_mostraSoEntrar_quandoVisitante', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/entrar"]')?.textContent?.trim()).toBe('Entrar');
    expect(el.querySelector('a[href="/registar"]')).toBeNull();
    expect(el.textContent).not.toContain('Criar conta');
    expect(el.querySelector('.avatar')).toBeNull();
  });

  const entrarComoAna = () =>
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana Lima',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });

  it('header_mostraAvatar_quandoLogado', () => {
    entrarComoAna();
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('.header__acoes--barra .avatar')?.textContent?.trim()).toBe('AL');
    expect(el.querySelector('a[href="/entrar"]')).toBeNull();
  });

  it('header_listaItensDiretos_noMenuDoCelular', () => {
    entrarComoAna();
    const el = criar().nativeElement as HTMLElement;
    const painel = el.querySelector('.header__acoes--menu') as HTMLElement;
    expect(painel.querySelector('.avatar')).toBeNull();
    expect(painel.textContent).toContain('Ana Lima');
    expect(painel.querySelector('a[href="/conta"]')?.textContent?.trim()).toBe('Meu perfil');
    expect(painel.querySelector('button')?.textContent?.trim()).toBe('Sair');
  });

  it('alterna o menu mobile com o botão', () => {
    const fixture = criar();
    const cmp = fixture.componentInstance;
    const botao = fixture.nativeElement.querySelector('.header__toggle') as HTMLButtonElement;
    expect(cmp.menuAberto()).toBe(false);
    botao.click();
    fixture.detectChanges();
    expect(cmp.menuAberto()).toBe(true);
    expect(botao.getAttribute('aria-expanded')).toBe('true');
  });

  it('fecharMenu() fecha o menu', () => {
    const cmp = criar().componentInstance;
    cmp.alternarMenu();
    expect(cmp.menuAberto()).toBe(true);
    cmp.fecharMenu();
    expect(cmp.menuAberto()).toBe(false);
  });

  it('fecha o menu com Escape', () => {
    const fixture = criar();
    fixture.componentInstance.alternarMenu();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(fixture.componentInstance.menuAberto()).toBe(false);
  });

  it('fecha o menu ao navegar', async () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [
        provideRouter([{ path: 'x', children: [] }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    fixture.componentInstance.alternarMenu();
    await TestBed.inject(Router).navigateByUrl('/x');
    fixture.detectChanges();
    expect(fixture.componentInstance.menuAberto()).toBe(false);
  });
});
