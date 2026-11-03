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
  perfil: [
    { rotulo: 'Origem e família', texto: 'Filho de Amoz.', referencias: ['Isaías 1:1'] },
    { rotulo: 'Ofício', texto: 'Profeta.', referencias: [] },
  ],
  panorama: bloco('Texto do panorama.'),
  contexto: bloco('Texto do contexto.'),
  mundo: bloco('Texto do mundo.'),
  profecias: [],
  promessas: [],
  citacoes: [],
  ...extra,
});

const profecias = [
  {
    referencia: 'Isaías 7:14',
    anuncia: 'Uma virgem conceberá.',
    notas: [],
  },
];
const promessas = [{ referencia: 'Isaías 65:17', anuncia: 'Novos céus e nova terra.', notas: [] }];
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
    Array.from(el().querySelectorAll('nav.abas a')).map((a) => a.textContent!.trim());

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

  it('mostra a aba Promessas entre Profecias e Citações quando há promessas', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama({ profecias, promessas, citacoes }));
    expect(rotulosAbas()).toEqual([
      'Panorama',
      'Contexto',
      'Mundo',
      'Profecias',
      'Promessas',
      'Citações',
    ]);
  });

  it('mostra as promessas na aba Promessas', async () => {
    await abrir('/biblia/isaias/promessas');
    await responder(panorama({ promessas }));
    expect(el().textContent).toContain('Isaías 65:17');
    expect(el().textContent).toContain('Novos céus e nova terra.');
  });

  it('redireciona para panorama quando a aba promessas não tem itens', async () => {
    await abrir('/biblia/isaias/promessas');
    await responder(panorama());
    expect(router().url).toBe('/biblia/isaias/panorama');
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

  it('mostra cada item do perfil com rótulo e texto só na aba panorama', async () => {
    await abrir('/biblia/isaias/panorama');
    await responder(panorama());
    const itens = Array.from(el().querySelectorAll('.perfil__item'));
    const rotulos = itens.map((p) => p.querySelector('.perfil__rotulo')!.textContent!.trim());
    expect(rotulos).toEqual(['Origem e família', 'Ofício']);
    expect(itens[0].querySelector('.perfil__texto')!.textContent).toContain('Filho de Amoz.');
    expect(itens[0].querySelector('.linha__coluna')!.textContent).toContain('Isaías 1:1');
    expect(itens[1].querySelector('.perfil__texto')!.textContent).toContain('Profeta.');
    // O rótulo fica na mesma linha de grade, para a coluna de referências começar na altura dele.
    expect(itens[0].querySelector('.linha__texto .perfil__rotulo')).not.toBeNull();

    await harness.navigateByUrl('/biblia/isaias/contexto');
    await assentar();
    expect(el().textContent).not.toContain('Filho de Amoz.');
    expect(el().textContent).toContain('Texto do contexto.');
  });

  it('marca a aba ativa com aria-current e troca o conteúdo sem nova chamada HTTP', async () => {
    await abrir('/biblia/isaias/panorama');
    await responder(panorama());
    const ativa = () =>
      Array.from(el().querySelectorAll('nav.abas a[aria-current="page"]')).map((a) =>
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

  it('busca o novo livro ao trocar de slug, limpa o anterior e redireciona pelo livro novo', async () => {
    await abrir('/biblia/isaias/profecias');
    await responder(panorama({ profecias }));
    expect(el().textContent).toContain('Isaías 7:14');

    await harness.navigateByUrl('/biblia/jeremias/profecias');
    await assentar();

    // (a) nova requisição ao outro livro; (b) carregando, sem conteúdo do anterior;
    // (c) sem redirecionar durante o carregamento, mesmo a aba existindo no livro antigo.
    const req = http.expectOne('/api/panoramas/livros/jeremias');
    expect(el().textContent).not.toContain('Isaías 7:14');
    expect(el().querySelector('nav')).toBeNull();
    expect(el().querySelector('[aria-busy="true"]')).not.toBeNull();
    expect(router().url).toBe('/biblia/jeremias/profecias');

    // o livro novo não tem profecias: redireciona para o panorama do livro NOVO.
    req.flush(panorama({ livroSlug: 'jeremias' }));
    await assentar();
    expect(router().url).toBe('/biblia/jeremias/panorama');
    expect(el().textContent).toContain('Texto do panorama.');
    http.expectNone('/api/panoramas/livros/isaias');
  });

  it('não redireciona ao trocar de livro quando a aba existe só no livro novo', async () => {
    await abrir('/biblia/isaias/panorama');
    await responder(panorama());

    await harness.navigateByUrl('/biblia/jeremias/profecias');
    await assentar();
    expect(router().url).toBe('/biblia/jeremias/profecias');

    http.expectOne('/api/panoramas/livros/jeremias').flush(panorama({ profecias }));
    await assentar();
    expect(router().url).toBe('/biblia/jeremias/profecias');
    expect(el().textContent).toContain('Isaías 7:14');
  });

  it('trata a rota sem aba como panorama', async () => {
    await abrir('/biblia/isaias');
    await responder(panorama());
    expect(el().textContent).toContain('Texto do panorama.');
    expect(router().url).toBe('/biblia/isaias');
  });

  const carregarLivros = async () => {
    http
      .expectOne('/api/livros')
      .flush([{ slug: 'isaias', nome: 'Isaías', ordem: 23, secao: 'ProfetasMaiores', secaoRotulo: 'Profetas maiores' }]);
    await assentar();
  };

  it('define o título da página por aba', async () => {
    await abrir('/biblia/isaias/contexto');
    await responder(panorama());
    await carregarLivros();
    expect(TestBed.inject(Title).getTitle()).toBe('Isaías: Contexto | Bíblia Teológica');
  });

  it('não põe o slug no título enquanto a lista de livros não carregou', async () => {
    await abrir('/biblia/isaias/contexto');
    await responder(panorama());
    expect(TestBed.inject(Title).getTitle()).not.toContain('isaias');
    await carregarLivros();
    expect(TestBed.inject(Title).getTitle()).toBe('Isaías: Contexto | Bíblia Teológica');
  });

  it('define o título com o nome do livro na ficha sem panorama', async () => {
    await abrir('/biblia/isaias');
    await responder(null, 404);
    await carregarLivros();
    expect(TestBed.inject(Title).getTitle()).toBe('Isaías | Bíblia Teológica');
  });

  it('define um título de erro quando o panorama falha', async () => {
    await abrir('/biblia/isaias');
    http.expectOne('/api/panoramas/livros/isaias').error(new ProgressEvent('error'));
    await assentar();
    expect(TestBed.inject(Title).getTitle()).toBe('Erro ao carregar o livro | Bíblia Teológica');
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
