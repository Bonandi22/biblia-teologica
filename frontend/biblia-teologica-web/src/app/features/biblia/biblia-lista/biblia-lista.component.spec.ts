import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { BibliaListaComponent } from './biblia-lista.component';
import { AuthStore } from '../../../core/auth/auth.store';
import type { LivroListItem } from '../../../core/models/livro.model';

const item = (slug: string, ordem: number, secao = 'Pentateuco', rotulo = 'PENTATEUCO'): LivroListItem =>
  ({ slug, nome: slug, ordem, secao, secaoRotulo: rotulo });

describe('BibliaListaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BibliaListaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lista os livros retornados pela API na sidebar', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent.toLowerCase();
    expect(texto).toContain('genesis');
    expect(texto).toContain('exodo');
  });

  it('filtra pela busca', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([
      { ...item('genesis', 1), nome: 'GÊNESIS' },
      { ...item('exodo', 2), nome: 'ÊXODO' },
    ]);
    fixture.componentInstance.busca.set('genes');
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('GÊNESIS');
    expect(texto).not.toContain('ÊXODO');
  });

  it('mostra "Entrar" no cabeçalho quando anónimo', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1)]);
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('.sidebar__conta a');
    expect(link.textContent.trim()).toBe('Entrar');
    expect(link.getAttribute('href')).toBe('/entrar');
  });

  it('mostra "A minha conta" no cabeçalho quando autenticado', () => {
    TestBed.inject(AuthStore).definir({
      email: 'a@b.c',
      nomeExibicao: 'Ana',
      roles: ['Membro'],
      deveTrocarSenha: false,
    });
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1)]);
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('.sidebar__conta a');
    expect(link.textContent.trim()).toBe('A minha conta');
    expect(link.getAttribute('href')).toBe('/conta');
  });
});
