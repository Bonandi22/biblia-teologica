import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './home.component';
import { AuthStore } from '../../core/auth/auth.store';

describe('HomeComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o título do hero e os CTA', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('Conhecer a Deus pela Sua Palavra');
    expect(el.querySelector('a[href="/biblia"]')).toBeTruthy();
    expect(el.querySelector('a[href="/registar"]')).toBeTruthy();
  });

  it('mostra os três cards de secção futura', () => {
    const texto = criar().nativeElement.textContent;
    expect(texto).toContain('Estudos');
    expect(texto).toContain('Notícias');
    expect(texto).toContain('Curiosidades');
    expect(criar().nativeElement.querySelectorAll('bt-secao-card').length).toBe(3);
  });

  it('mostra o teaser de membros quando anónimo', () => {
    expect(criar().nativeElement.textContent).toContain('Alguns conteúdos serão só para membros');
  });

  it('esconde o teaser de membros quando autenticado', () => {
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    expect(criar().nativeElement.textContent).not.toContain(
      'Alguns conteúdos serão só para membros',
    );
  });
});
