import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { BibliaFichaComponent } from './biblia-ficha.component';

const ficha = {
  slug: 'genesis',
  nome: 'GÊNESIS',
  ordem: 1,
  secao: 'Pentateuco',
  secaoRotulo: 'PENTATEUCO',
  testamento: 'Antigo',
  quem: 'Moisés',
  porque: 'Origem',
  paraQuem: 'Israel',
  estrutura: 'Duas partes',
  tema: 'Criação e aliança',
  ideias: ['Criação', 'Queda'],
  versiculo: 'Gênesis 1:1',
};

describe('BibliaFichaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BibliaFichaComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'genesis' })) } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  const montar = () => {
    const fixture = TestBed.createComponent(BibliaFichaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([
      { slug: 'genesis', nome: 'GÊNESIS', ordem: 1, secao: 'P', secaoRotulo: 'PENTATEUCO' },
      { slug: 'exodo', nome: 'ÊXODO', ordem: 2, secao: 'P', secaoRotulo: 'PENTATEUCO' },
    ]);
    return fixture;
  };

  it('abre na moldura do livro, sem abas, com os campos da API', () => {
    const fixture = montar();
    http.expectOne('/api/livros/genesis').flush(ficha);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('bt-livro-moldura h1')?.textContent?.trim()).toBe('Gênesis');
    expect(el.querySelector('nav.abas')).toBeNull();
    for (const texto of ['Moisés', 'Origem', 'Israel', 'Duas partes', 'Criação e aliança', 'Queda', 'Gênesis 1:1']) {
      expect(el.textContent).toContain(texto);
    }
  });

  it('usa os rótulos em português correto', () => {
    const fixture = montar();
    http.expectOne('/api/livros/genesis').flush(ficha);
    fixture.detectChanges();
    const texto = (fixture.nativeElement as HTMLElement).textContent!;
    expect(texto).toContain('Para quem escreveu');
    expect(texto).toContain('Por que escreveu');
    expect(texto).not.toContain('Pra quem');
    expect(texto).not.toContain('Porque escreveu');
  });

  it('leva ao livro seguinte', () => {
    const fixture = montar();
    http.expectOne('/api/livros/genesis').flush(ficha);
    fixture.detectChanges();
    const link = (fixture.nativeElement as HTMLElement).querySelector('a[href="/biblia/exodo"].ficha__nav-link');
    expect(link?.textContent).toContain('Êxodo');
  });

  it('volta à lista de livros quando o livro não existe', () => {
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigate');
    montar();
    http.expectOne('/api/livros/genesis').flush(null, { status: 404, statusText: 'Not Found' });
    expect(navegar).toHaveBeenCalledWith(['/biblia']);
  });
});
