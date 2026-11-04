import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
import { BibliaListaComponent } from './biblia-lista.component';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';

const LIVROS = [
  { slug: 'genesis', nome: 'GÊNESIS', ordem: 1, secao: 'P', secaoRotulo: 'PENTATEUCO' },
  { slug: 'exodo', nome: 'ÊXODO', ordem: 2, secao: 'P', secaoRotulo: 'PENTATEUCO' },
];

describe('BibliaListaComponent', () => {
  const montar = async () => {
    TestBed.configureTestingModule({
      imports: [BibliaListaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigate');
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush(LIVROS);
    http.expectOne('/api/panoramas/livros').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, navegar, el: fixture.nativeElement as HTMLElement };
  };

  it('mostra o índice e não redireciona para o primeiro livro', async () => {
    const { el, navegar } = await montar();
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Livros');
    expect(el.textContent).toContain('Gênesis');
    expect(navegar).not.toHaveBeenCalled();
    expect(TestBed.inject(Title).getTitle()).toBe('Livros | Bíblia Teológica');
  });

  it('filtra pela busca, com rótulo acessível', async () => {
    const { fixture, el } = await montar();
    const busca = el.querySelector('input[type="search"]') as HTMLInputElement;
    expect(busca.getAttribute('aria-label')).toBe('Buscar livro');
    busca.value = 'genes';
    busca.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(el.textContent).toContain('Gênesis');
    expect(el.textContent).not.toContain('Êxodo');
  });

  it('define o lugar atual e o limpa ao sair', async () => {
    const { fixture } = await montar();
    const lugar = TestBed.inject(LugarAtualService);
    expect(lugar.texto()).toBe('Os 66 livros');
    fixture.destroy();
    expect(lugar.texto()).toBe('');
  });
});
