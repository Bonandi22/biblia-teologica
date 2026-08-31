import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { HeaderComponent } from './header.component';
import { AuthStore } from '../../auth/auth.store';

describe('HeaderComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra os itens de navegação principais', () => {
    const texto = criar().nativeElement.textContent;
    for (const rotulo of ['Início', 'Fichário', 'Estudos', 'Notícias', 'Curiosidades']) {
      expect(texto).toContain(rotulo);
    }
  });

  it('mostra "Entrar" e "Criar conta" quando anónimo', () => {
    const el = criar().nativeElement as HTMLElement;
    const entrar = el.querySelector('a[href="/entrar"]');
    const criarConta = el.querySelector('a[href="/registar"]');
    expect(entrar?.textContent?.trim()).toBe('Entrar');
    expect(criarConta?.textContent?.trim()).toBe('Criar conta');
    expect(el.querySelector('a[href="/conta"]')).toBeNull();
  });

  it('mostra "A minha conta" quando autenticado', () => {
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/conta"]')?.textContent?.trim()).toBe('A minha conta');
    expect(el.querySelector('a[href="/entrar"]')).toBeNull();
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
      providers: [provideRouter([{ path: 'x', children: [] }])],
    });
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    fixture.componentInstance.alternarMenu();
    await TestBed.inject(Router).navigateByUrl('/x');
    fixture.detectChanges();
    expect(fixture.componentInstance.menuAberto()).toBe(false);
  });
});
