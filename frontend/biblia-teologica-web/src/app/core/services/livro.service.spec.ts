import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LivroService } from './livro.service';

describe('LivroService', () => {
  let service: LivroService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), LivroService],
    });
    service = TestBed.inject(LivroService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('listar() faz GET em /api/livros', () => {
    service.listar().subscribe();
    const req = http.expectOne('/api/livros');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('obterPorSlug() faz GET em /api/livros/:slug', () => {
    service.obterPorSlug('genesis').subscribe();
    const req = http.expectOne('/api/livros/genesis');
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
