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

  it('liga Estudos a /estudos e mantém Notícias e Curiosidades como em breve', () => {
    const el = criar().nativeElement as HTMLElement;
    const link = el.querySelector('.crescer a[href="/estudos"]');
    expect(link?.textContent).toContain('Estudos');
    expect(link?.textContent).not.toContain('Em breve');
    const emBreve = Array.from(el.querySelectorAll('.crescer .entrada--breve'));
    expect(emBreve.map((e) => e.querySelector('h3')?.firstChild?.textContent?.trim())).toEqual([
      'Notícias',
      'Curiosidades',
    ]);
    for (const e of emBreve) {
      expect(e.textContent).toContain('Em breve');
      expect(e.querySelector('a')).toBeNull();
    }
  });

  it('não usa cartões iguais na seção A crescer', () => {
    expect(criar().nativeElement.querySelectorAll('.crescer bt-secao-card').length).toBe(0);
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
