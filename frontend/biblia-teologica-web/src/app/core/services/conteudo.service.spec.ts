import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ConteudoService } from './conteudo.service';

describe('ConteudoService', () => {
  let service: ConteudoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ConteudoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('obterLivro faz GET em /api/panoramas/livros/:slug', () => {
    service.obterLivro('isaias').subscribe();
    const req = http.expectOne('/api/panoramas/livros/isaias');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('listarEstudos faz GET em /api/estudos', () => {
    service.listarEstudos().subscribe();
    http.expectOne('/api/estudos').flush([]);
  });

  it('obterEstudo faz GET em /api/estudos/:slug', () => {
    service.obterEstudo('servo').subscribe();
    http.expectOne('/api/estudos/servo').flush({});
  });
});
