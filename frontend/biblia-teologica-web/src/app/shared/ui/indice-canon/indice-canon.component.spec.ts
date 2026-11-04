import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { IndiceCanonComponent } from './indice-canon.component';

@Component({
  imports: [IndiceCanonComponent],
  template: `<bt-indice-canon [filtro]="filtro()" />`,
})
class HostComponent {
  readonly filtro = signal('');
}

const livro = (slug: string, nome: string, ordem: number, secaoRotulo: string) => ({
  slug,
  nome,
  ordem,
  secao: secaoRotulo,
  secaoRotulo,
});
const LIVROS = [
  livro('genesis', 'GÊNESIS', 1, 'PENTATEUCO'),
  livro('exodo', 'ÊXODO', 2, 'PENTATEUCO'),
  livro('isaias', 'ISAÍAS', 23, 'PROFETAS MAIORES'),
  livro('mateus', 'MATEUS', 40, 'EVANGELHOS'),
];

describe('IndiceCanonComponent', () => {
  const montar = async (panoramas: string[] | null = ['isaias'], livros: unknown[] | null = LIVROS) => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const reqLivros = http.expectOne('/api/livros');
    if (livros) reqLivros.flush(livros);
    else reqLivros.error(new ProgressEvent('error'));
    const reqPanoramas = http.expectOne('/api/panoramas/livros');
    if (panoramas) reqPanoramas.flush(panoramas);
    else reqPanoramas.error(new ProgressEvent('error'));
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, http, el: fixture.nativeElement as HTMLElement };
  };
  const nomes = (el: HTMLElement) =>
    Array.from(el.querySelectorAll('.indice__nome')).map((n) => n.textContent!.trim());

  it('separa os testamentos e agrupa por divisão, na ordem do cânon', async () => {
    const { el } = await montar();
    const titulos = Array.from(el.querySelectorAll('h2')).map((h) => h.textContent!.trim());
    expect(titulos).toEqual(['Antigo Testamento', 'Novo Testamento']);
    const divisoes = Array.from(el.querySelectorAll('h3')).map((h) => h.textContent!.trim());
    expect(divisoes).toEqual(['Pentateuco', 'Profetas maiores', 'Evangelhos']);
    expect(nomes(el)).toEqual(['Gênesis', 'Êxodo', 'Isaías', 'Mateus']);
  });

  it('liga cada livro à sua página', async () => {
    const { el } = await montar();
    expect(el.querySelector('a[href="/biblia/isaias"]')?.textContent).toContain('Isaías');
  });

  it('marca só os livros com panorama', async () => {
    const { el } = await montar();
    const marcas = el.querySelectorAll('.indice__marca');
    expect(marcas).toHaveLength(1);
    expect(marcas[0].closest('a')?.getAttribute('href')).toBe('/biblia/isaias');
  });

  it('continua sem marca e sem erro quando a lista de panoramas falha', async () => {
    const { el } = await montar(null);
    expect(el.querySelectorAll('.indice__marca')).toHaveLength(0);
    expect(el.querySelector('[role="alert"]')).toBeNull();
    expect(nomes(el)).toContain('Isaías');
  });

  it('filtra sem acento e mantém a ordem do cânon', async () => {
    const { fixture, el } = await montar();
    fixture.componentInstance.filtro.set('e');
    fixture.detectChanges();
    expect(nomes(el)).toEqual(['Gênesis', 'Êxodo', 'Mateus']);
    fixture.componentInstance.filtro.set('exo');
    fixture.detectChanges();
    expect(nomes(el)).toEqual(['Êxodo']);
    expect(Array.from(el.querySelectorAll('h2')).map((h) => h.textContent!.trim())).toEqual([
      'Antigo Testamento',
    ]);
  });

  it('diz quando a busca não encontra nada', async () => {
    const { fixture, el } = await montar();
    fixture.componentInstance.filtro.set('zzz');
    fixture.detectChanges();
    expect(el.textContent).toContain('Nenhum livro encontrado para "zzz".');
  });

  it('avisa e permite tentar de novo quando a lista de livros falha', async () => {
    const { fixture, http, el } = await montar(['isaias'], null);
    expect(el.querySelector('[role="alert"]')?.textContent).toContain(
      'Não foi possível carregar a lista de livros.',
    );
    (el.querySelector('[role="alert"] button') as HTMLButtonElement).click();
    http.expectOne('/api/livros').flush(LIVROS);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(el.querySelector('[role="alert"]')).toBeNull();
    expect(nomes(el)).toContain('Isaías');
  });
});
