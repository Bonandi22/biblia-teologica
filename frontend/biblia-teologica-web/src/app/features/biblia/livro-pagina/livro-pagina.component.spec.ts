import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Title } from '@angular/platform-browser';
import { LivroPaginaComponent } from './livro-pagina.component';
import { livroMatcher } from './livro.matcher';
import type { Bloco, LivroPanorama } from '../../../core/models/conteudo.model';

const bloco = (explicacao: string): Bloco => ({
  explicacao,
  personagens: [],
  acontecimentos: [],
  notas: [],
});

const panorama = (extra: Partial<LivroPanorama> = {}): LivroPanorama => ({
  livroSlug: 'isaias',
  resumo: 'Resumo do livro.',
  quem: 'Isaías, filho de Amoz',
  paraQuem: 'O reino de Judá',
  porque: 'Chamar o povo de volta',
  panorama: bloco('Texto do panorama.'),
  contexto: bloco('Texto do contexto.'),
  mundo: bloco('Texto do mundo.'),
  profecias: [],
  citacoes: [],
  ...extra,
});

const profecias = [
  {
    referencia: 'Isaías 7:14',
    anuncia: 'Uma virgem conceberá.',
    categoria: 'cumpridaEmCristo',
    cumprimento: ['Mateus 1:23'],
    fatoHistorico: null,
    notas: [],
  },
];
const citacoes = [
  { trecho: 'Eis o meu servo', citadoEm: ['Mateus 12:18'], comentario: null, notas: [] },
];

describe('LivroPaginaComponent', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  const router = () => TestBed.inject(Router);
  const el = () => harness.routeNativeElement as HTMLElement;

  const abrir = async (url: string) => {
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url);
    harness.detectChanges();
  };
  const assentar = async () => {
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
  };
  const responder = async (corpo: LivroPanorama | null, status = 200) => {
    const req = http.expectOne('/api/panoramas/livros/isaias');
    if (corpo) req.flush(corpo);
    else req.flush(null, { status, statusText: 'erro' });
    await assentar();
  };
  const rotulosAbas = () =>
    Array.from(el().querySelectorAll('nav a')).map((a) => a.textContent!.trim());

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'biblia', children: [{ matcher: livroMatcher, component: LivroPaginaComponent }] },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('mostra a ficha quando a API retorna 404', async () => {
    await abrir('/biblia/isaias');
    await responder(null, 404);
    expect(el().querySelector('bt-biblia-ficha')).not.toBeNull();
    expect(el().querySelector('nav')).toBeNull();
    http.expectOne('/api/livros/isaias');
  });

  it('mostra três abas quando não há profecias nem citações', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama());
    expect(rotulosAbas()).toEqual(['Panorama', 'Contexto', 'Mundo']);
  });

  it('mostra a aba Profecias quando há profecias', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama({ profecias }));
    expect(rotulosAbas()).toEqual(['Panorama', 'Contexto', 'Mundo', 'Profecias']);
  });

  it('mostra a aba Citações quando há citações', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama({ citacoes }));
    expect(rotulosAbas()).toEqual(['Panorama', 'Contexto', 'Mundo', 'Citações']);
  });

  it('redireciona para panorama quando a aba não existe', async () => {
    await abrir('/biblia/isaias/inexistente');
    await responder(panorama());
    expect(router().url).toBe('/biblia/isaias/panorama');
  });

  it('redireciona para panorama quando a aba profecias não tem itens', async () => {
    await abrir('/biblia/isaias/profecias');
    await responder(panorama());
    expect(router().url).toBe('/biblia/isaias/panorama');
  });

  it('redireciona para panorama quando a aba citacoes não tem itens', async () => {
    await abrir('/biblia/isaias/citacoes');
    await responder(panorama());
    expect(router().url).toBe('/biblia/isaias/panorama');
  });

  it('mostra quem, para quem e por quê só na aba panorama', async () => {
    await abrir('/biblia/isaias/panorama');
    await responder(panorama());
    expect(el().textContent).toContain('Isaías, filho de Amoz');
    expect(el().textContent).toContain('O reino de Judá');
    expect(el().textContent).toContain('Chamar o povo de volta');

    await harness.navigateByUrl('/biblia/isaias/contexto');
    await assentar();
    expect(el().textContent).not.toContain('Isaías, filho de Amoz');
    expect(el().textContent).toContain('Texto do contexto.');
  });

  it('marca a aba ativa com aria-current e troca o conteúdo sem nova chamada HTTP', async () => {
    await abrir('/biblia/isaias/panorama');
    await responder(panorama());
    const ativa = () =>
      Array.from(el().querySelectorAll('nav a[aria-current="page"]')).map((a) =>
        a.textContent!.trim(),
      );
    expect(ativa()).toEqual(['Panorama']);
    expect(el().textContent).toContain('Texto do panorama.');

    await harness.navigateByUrl('/biblia/isaias/mundo');
    await assentar();
    expect(ativa()).toEqual(['Mundo']);
    expect(el().textContent).toContain('Texto do mundo.');
    expect(el().textContent).not.toContain('Texto do panorama.');
    http.expectNone('/api/panoramas/livros/isaias');
  });

  it('trata a rota sem aba como panorama', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama());
    expect(el().textContent).toContain('Texto do panorama.');
    expect(router().url).toBe('/biblia/isaias');
  });

  it('define o título da página por aba', async () => {
    await abrir('/biblia/isaias/contexto');
    await responder(panorama());
    expect(TestBed.inject(Title).getTitle()).toContain('Contexto');
  });

  it('mostra mensagem e tenta novamente em erro de rede, sem redirecionar nem mostrar a ficha', async () => {
    await abrir('/biblia/isaias/contexto');
    http.expectOne('/api/panoramas/livros/isaias').error(new ProgressEvent('error'));
    await assentar();
    expect(router().url).toBe('/biblia/isaias/contexto');
    expect(el().querySelector('bt-biblia-ficha')).toBeNull();
    expect(el().querySelector('[role="alert"]')).not.toBeNull();

    (el().querySelector('button') as HTMLButtonElement).click();
    await assentar();
    await responder(panorama());
    expect(el().querySelector('[role="alert"]')).toBeNull();
    expect(el().textContent).toContain('Texto do contexto.');
  });

  it('não redireciona nem mostra a ficha em erro 500', async () => {
    await abrir('/biblia/isaias/mundo');
    await responder(null, 500);
    expect(router().url).toBe('/biblia/isaias/mundo');
    expect(el().querySelector('bt-biblia-ficha')).toBeNull();
    expect(el().querySelector('[role="alert"]')).not.toBeNull();
  });
});
