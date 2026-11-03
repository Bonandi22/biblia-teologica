import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Title } from '@angular/platform-browser';
import { EstudoPaginaComponent } from './estudo-pagina.component';
import type { Estudo } from '../../../core/models/conteudo.model';

const estudo = (extra: Partial<Estudo> = {}): Estudo => ({
  slug: 'servo-sofredor',
  titulo: 'O servo sofredor',
  resumo: 'Quem é o servo de Isaías 53.',
  corpoMarkdown:
    '## Quem é o servo\n\nO texto fala de alguém[^1] que carrega a dor.[^2]\n\n> Ele foi traspassado.',
  livros: ['isaias'],
  relacionados: [{ slug: 'remanescente', titulo: 'O remanescente', resumo: 'Um resto que volta.' }],
  notas: [
    { numero: 1, citacao: 'Obra A, p. 10.', url: null },
    { numero: 2, citacao: 'Obra B, p. 20.', url: 'https://exemplo.org/b' },
  ],
  ...extra,
});

describe('EstudoPaginaComponent', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  const el = () => harness.routeNativeElement as HTMLElement;

  const abrir = async () => {
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/estudos/servo-sofredor');
    harness.detectChanges();
  };
  const responder = async (corpo: Estudo | null, status = 200) => {
    const req = http.expectOne('/api/estudos/servo-sofredor');
    if (corpo) req.flush(corpo);
    else req.flush(null, { status, statusText: 'erro' });
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'estudos/:slug', component: EstudoPaginaComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('renderiza o corpo por bt-markdown, com título e resumo', async () => {
    await abrir();
    await responder(estudo());
    expect(el().querySelector('h1')?.textContent).toContain('O servo sofredor');
    expect(el().textContent).toContain('Quem é o servo de Isaías 53.');
    expect(el().querySelector('bt-markdown h2')?.textContent).toContain('Quem é o servo');
    expect(el().querySelector('bt-markdown blockquote')).not.toBeNull();
    expect(TestBed.inject(Title).getTitle()).toBe('O servo sofredor | Bíblia Teológica');
  });

  it('mostra a bibliografia e os links das notas batem com os ids dela', async () => {
    await abrir();
    await responder(estudo());
    expect(el().querySelectorAll('bt-bibliografia li').length).toBe(2);
    expect(el().textContent).toContain('Obra A, p. 10.');
    const hrefs = Array.from(el().querySelectorAll('bt-markdown sup a')).map((a) =>
      a.getAttribute('href'),
    );
    expect(hrefs).toEqual(['#nota-1', '#nota-2']);
    for (const h of hrefs) {
      expect(el().querySelector(`bt-bibliografia li[id="${h!.slice(1)}"]`)).not.toBeNull();
    }
  });

  it('lista os estudos relacionados em Continue estudando', async () => {
    await abrir();
    await responder(estudo());
    const secao = el().querySelector('.continue') as HTMLElement;
    expect(secao.textContent).toContain('Continue estudando');
    expect(secao.textContent).toContain('O remanescente');
    expect(secao.querySelector('a[href="/estudos/remanescente"]')).not.toBeNull();
  });

  it('não mostra Continue estudando sem relacionados', async () => {
    await abrir();
    await responder(estudo({ relacionados: [] }));
    expect(el().querySelector('.continue')).toBeNull();
  });

  it('linka os livros do estudo', async () => {
    await abrir();
    await responder(estudo());
    expect(el().querySelector('a[href="/biblia/isaias"]')).not.toBeNull();
  });

  it('mostra Estudo não encontrado com link para a lista quando a API retorna 404', async () => {
    await abrir();
    await responder(null, 404);
    expect(el().textContent).toContain('Estudo não encontrado');
    expect(el().querySelector('a[href="/estudos"]')).not.toBeNull();
    expect(el().querySelector('[role="alert"]')).toBeNull();
  });

  it('mostra erro de conexão com nova tentativa, distinto do 404', async () => {
    await abrir();
    await responder(null, 500);
    expect(el().querySelector('[role="alert"]')).not.toBeNull();
    expect(el().textContent).not.toContain('Estudo não encontrado');
    (el().querySelector('button') as HTMLButtonElement).click();
    harness.detectChanges();
    http.expectOne('/api/estudos/servo-sofredor');
  });
});
