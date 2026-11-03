import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './home.component';

const estudo = { slug: 'cerco', titulo: 'O cerco de Jerusalém', resumo: 'Resumo do estudo.' };

describe('HomeComponent', () => {
  const montar = async (opcoes: { estudos?: unknown[] | null; panoramas?: string[] } = {}) => {
    TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([
      { slug: 'isaias', nome: 'ISAÍAS', ordem: 23, secao: 'PM', secaoRotulo: 'PROFETAS MAIORES' },
    ]);
    http.expectOne('/api/panoramas/livros').flush(opcoes.panoramas ?? ['isaias']);
    const reqEstudos = http.expectOne('/api/estudos');
    if (opcoes.estudos === null) reqEstudos.error(new ProgressEvent('error'));
    else reqEstudos.flush(opcoes.estudos ?? [estudo]);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('abre com o nome do site, a frase e o índice dos livros', async () => {
    const el = await montar();
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Bíblia Teológica');
    expect(el.textContent).toContain(
      'Cada livro da Bíblia situado: quem escreveu, para quem e o que acontecia ao redor, com referência para conferir.',
    );
    expect(el.querySelector('bt-indice-canon a[href="/biblia/isaias"]')).not.toBeNull();
    expect(TestBed.inject(Title).getTitle()).toBe('Bíblia Teológica');
  });

  it('não mostra o que ainda não existe', async () => {
    const texto = (await montar()).textContent!;
    for (const ausente of ['Notícias', 'Curiosidades', 'membros', 'newsletter', 'Cristo aparece', 'Génesis']) {
      expect(texto).not.toContain(ausente);
    }
  });

  it('não usa Criar conta como chamada principal', async () => {
    expect((await montar()).querySelector('a[href="/registar"]')).toBeNull();
  });

  it('lista os estudos publicados', async () => {
    const el = await montar();
    const link = el.querySelector('a[href="/estudos/cerco"]');
    expect(link?.textContent).toContain('O cerco de Jerusalém');
    expect(el.textContent).toContain('Resumo do estudo.');
  });

  it('esconde a seção de estudos quando não há estudos', async () => {
    expect((await montar({ estudos: [] })).querySelector('.home__estudos')).toBeNull();
  });

  it('esconde a seção de estudos, sem erro, quando a lista falha', async () => {
    const el = await montar({ estudos: null });
    expect(el.querySelector('.home__estudos')).toBeNull();
    expect(el.querySelector('bt-indice-canon a')).not.toBeNull();
  });

  it('a guarda leva ao primeiro livro com panorama', async () => {
    const el = await montar();
    expect(el.querySelector('.home__guarda a[href="/biblia/isaias"]')).not.toBeNull();
  });

  it('a guarda não oferece link quando ainda não há panorama', async () => {
    const el = await montar({ panoramas: [] });
    expect(el.querySelector('.home__guarda')).not.toBeNull();
    expect(el.querySelector('.home__guarda a')).toBeNull();
  });
});
