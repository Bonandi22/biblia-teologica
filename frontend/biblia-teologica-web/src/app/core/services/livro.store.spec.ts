import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LivroStore } from './livro.store';
import type { LivroListItem } from '../models/livro.model';

const item = (slug: string, ordem: number): LivroListItem =>
  ({ slug, nome: slug, ordem, secao: 'Pentateuco', secaoRotulo: 'PENTATEUCO' });

describe('LivroStore', () => {
  let store: LivroStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), LivroStore],
    });
    store = TestBed.inject(LivroStore);
    http = TestBed.inject(HttpTestingController);
  });

  it('carrega uma vez só', () => {
    store.carregar();
    store.carregar();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
    http.verify();
    expect(store.livros().length).toBe(2);
  });

  it('vizinhos() devolve anterior e próximo pela ordem', () => {
    store.carregar();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2), item('levitico', 3)]);
    expect(store.vizinhos('exodo')).toEqual({
      anterior: expect.objectContaining({ slug: 'genesis' }),
      proximo: expect.objectContaining({ slug: 'levitico' }),
    });
    expect(store.vizinhos('genesis').anterior).toBeUndefined();
  });
});
