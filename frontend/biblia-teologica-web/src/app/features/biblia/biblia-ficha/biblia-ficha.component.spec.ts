import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { BibliaFichaComponent } from './biblia-ficha.component';

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

  it('busca e renderiza a ficha do slug da rota', () => {
    const fixture = TestBed.createComponent(BibliaFichaComponent);
    fixture.detectChanges();

    http.expectOne('/api/livros/genesis').flush({
      slug: 'genesis', nome: 'GÊNESIS', ordem: 1, secao: 'Pentateuco', secaoRotulo: 'PENTATEUCO',
      testamento: 'Antigo', quem: 'Moisés', porque: 'Origem', paraQuem: 'Israel',
      estrutura: 'Duas partes', tema: 'Criação e aliança',
      ideias: ['Criação', 'Queda'], versiculo: 'Gênesis 1:1',
    });
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('GÊNESIS');
    expect(texto).toContain('Criação e aliança');
    expect(texto).toContain('Gênesis 1:1');
  });
});
