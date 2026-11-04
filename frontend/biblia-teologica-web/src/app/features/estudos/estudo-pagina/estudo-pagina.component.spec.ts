import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Title } from '@angular/platform-browser';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';
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

  it('põe cada nota na linha do parágrafo que a chama, e os links batem com os ids', async () => {
    await abrir();
    await responder(estudo());
    const linhas = Array.from(el().querySelectorAll('.corpo bt-linha-estudo'));
    const comNotas = linhas.filter((l) => l.querySelector('li[id]'));
    expect(comNotas).toHaveLength(1);
    expect(comNotas[0].querySelector('bt-markdown')?.textContent).toContain('O texto fala de alguém');
    expect(comNotas[0].querySelectorAll('li[id]')).toHaveLength(2);
    const hrefs = Array.from(el().querySelectorAll('bt-markdown sup a')).map((a) =>
      a.getAttribute('href'),
    );
    expect(hrefs).toEqual(['#nota-1', '#nota-2']);
    for (const h of hrefs) expect(el().querySelector(`li[id="${h!.slice(1)}"]`)).not.toBeNull();
  });

  it('mostra o título da coluna só na primeira linha com notas', async () => {
    await abrir();
    await responder(estudo());
    const titulos = Array.from(el().querySelectorAll('.corpo .linha__titulo'));
    expect(titulos.map((t) => t.textContent!.trim())).toEqual(['Referências']);
  });

  it('mantém a bibliografia completa no fim, sem repetir ids', async () => {
    await abrir();
    await responder(estudo());
    const bib = el().querySelector('.bibliografia') as HTMLElement;
    expect(bib.querySelector('h2')?.textContent).toContain('Bibliografia');
    expect(bib.querySelectorAll('li')).toHaveLength(2);
    expect(bib.querySelector('[id]')).toBeNull();
    expect(bib.querySelector('a[href="https://exemplo.org/b"]')?.textContent).toContain('Abrir fonte');
  });

  it('mantém visível a nota que nenhum parágrafo chama', async () => {
    await abrir();
    await responder(estudo({ notas: [...estudo().notas, { numero: 9, citacao: 'Obra sem chamada.', url: null }] }));
    expect(el().querySelector('.corpo li[id="nota-9"]')?.textContent).toContain('Obra sem chamada.');
  });

  it('define o lugar atual e o limpa ao sair', async () => {
    await abrir();
    await responder(estudo());
    const lugar = TestBed.inject(LugarAtualService);
    expect(lugar.texto()).toBe('Estudo');
    harness.fixture.destroy();
    expect(lugar.texto()).toBe('');
  });

  it('mostra o nome do livro na grafia de exibição', async () => {
    await abrir();
    await responder(estudo());
    http.expectOne('/api/livros').flush([
      { slug: 'isaias', nome: 'ISAÍAS', ordem: 23, secao: 'PM', secaoRotulo: 'PROFETAS MAIORES' },
    ]);
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
    expect(el().querySelector('a[href="/biblia/isaias"]')?.textContent).toContain('Isaías');
  });

  it('lista os estudos relacionados em Continue estudando', async () => {
    await abrir();
    await responder(estudo());
    const secao = el().querySelector('.continue') as HTMLElement;
    expect(secao.textContent).toContain('Continue estudando');
    expect(secao.textContent).toContain('O remanescente');
    expect(secao.querySelector('a[href="/estudos/remanescente"]')).not.toBeNull();
  });

  it('clicar na chamada leva o foco à nota e só rola o mínimo necessário', async () => {
    await abrir();
    await responder(estudo());
    const rolar = vi.fn();
    Element.prototype.scrollIntoView = rolar;
    (el().querySelector('bt-markdown sup a') as HTMLElement).click();
    expect(rolar).toHaveBeenCalledWith({ block: 'nearest' });
    expect(document.activeElement?.id).toBe('nota-1');
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
    expect(TestBed.inject(Title).getTitle()).toBe('Estudo não encontrado | Bíblia Teológica');
  });

  it('mostra erro de conexão com nova tentativa, distinto do 404', async () => {
    await abrir();
    await responder(null, 500);
    expect(el().querySelector('[role="alert"]')).not.toBeNull();
    expect(el().textContent).not.toContain('Estudo não encontrado');
    expect(TestBed.inject(Title).getTitle()).toBe('Erro ao carregar o estudo | Bíblia Teológica');
    (el().querySelector('button') as HTMLButtonElement).click();
    harness.detectChanges();
    http.expectOne('/api/estudos/servo-sofredor');
  });
});
