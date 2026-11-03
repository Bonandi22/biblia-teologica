import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { BibliaListaComponent } from './biblia-lista.component';
import type { LivroListItem } from '../../../core/models/livro.model';

const item = (
  slug: string,
  ordem: number,
  secao = 'Pentateuco',
  rotulo = 'PENTATEUCO',
): LivroListItem => ({ slug, nome: slug, ordem, secao, secaoRotulo: rotulo });

describe('BibliaListaComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BibliaListaComponent],
      providers: [provideRouter([{ path: '**', children: [] }]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lista os livros retornados pela API na sidebar', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent.toLowerCase();
    expect(texto).toContain('genesis');
    expect(texto).toContain('exodo');
  });

  it('filtra pela busca', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([
      { ...item('genesis', 1), nome: 'GÊNESIS' },
      { ...item('exodo', 2), nome: 'ÊXODO' },
    ]);
    fixture.componentInstance.busca.set('genes');
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Gênesis');
    expect(texto).not.toContain('Êxodo');
  });

  describe('redirecionamento', () => {
    const montar = async (url: string) => {
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        imports: [BibliaListaComponent],
        providers: [
          provideRouter([{ path: 'biblia', children: [{ path: '**', component: BibliaListaComponent }] }]),
          provideHttpClient(),
          provideHttpClientTesting(),
        ],
      });
      const httpLocal = TestBed.inject(HttpTestingController);
      const harness = await RouterTestingHarness.create();
      const navegar = vi.spyOn(TestBed.inject(Router), 'navigate');
      await harness.navigateByUrl(url);
      httpLocal.expectOne('/api/livros').flush([item('genesis', 1)]);
      harness.detectChanges();
      await harness.fixture.whenStable();
      return navegar;
    };

    it('não redireciona em /biblia/isaias', async () => {
      expect(await montar('/biblia/isaias')).not.toHaveBeenCalled();
    });

    it('não redireciona em /biblia/isaias/mundo', async () => {
      expect(await montar('/biblia/isaias/mundo')).not.toHaveBeenCalled();
    });

    it('redireciona só em /biblia', async () => {
      expect(await montar('/biblia')).toHaveBeenCalledWith(['/biblia', 'genesis']);
    });
  });

  it('não repete a marca do site na sidebar (vem do header global)', () => {
    const fixture = TestBed.createComponent(BibliaListaComponent);
    fixture.detectChanges();
    http.expectOne('/api/livros').flush([item('genesis', 1)]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.sidebar__brand')).toBeNull();
    expect(fixture.nativeElement.querySelector('.sidebar__conta')).toBeNull();
  });

  describe('lista recolhível (celular)', () => {
    const montar = () => {
      const fixture = TestBed.createComponent(BibliaListaComponent);
      fixture.detectChanges();
      http.expectOne('/api/livros').flush([item('genesis', 1), item('exodo', 2)]);
      fixture.detectChanges();
      return fixture;
    };
    const botao = (f: ReturnType<typeof montar>) =>
      f.nativeElement.querySelector('.sidebar__trocar') as HTMLButtonElement;

    it('começa recolhida, com aria-controls apontando para a lista', () => {
      const f = montar();
      expect(botao(f).getAttribute('aria-expanded')).toBe('false');
      const alvo = botao(f).getAttribute('aria-controls')!;
      expect(f.nativeElement.querySelector('#' + alvo)).not.toBeNull();
    });

    it('alterna aria-expanded ao clicar no botão', () => {
      const f = montar();
      botao(f).click();
      f.detectChanges();
      expect(botao(f).getAttribute('aria-expanded')).toBe('true');
      botao(f).click();
      f.detectChanges();
      expect(botao(f).getAttribute('aria-expanded')).toBe('false');
    });

    it('fecha a lista ao escolher um livro', () => {
      const f = montar();
      botao(f).click();
      f.detectChanges();
      (f.nativeElement.querySelector('a.item') as HTMLAnchorElement).click();
      f.detectChanges();
      expect(botao(f).getAttribute('aria-expanded')).toBe('false');
    });

    it('mantém a busca e os livros no DOM com a lista recolhida (computador)', () => {
      const f = montar();
      expect(f.nativeElement.querySelector('.sidebar__busca')).not.toBeNull();
      expect(f.nativeElement.querySelectorAll('a.item').length).toBe(2);
    });

    it('mostra o livro atual no botão', async () => {
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        imports: [BibliaListaComponent],
        providers: [
          provideRouter([{ path: 'biblia', children: [{ path: '**', component: BibliaListaComponent }] }]),
          provideHttpClient(),
          provideHttpClientTesting(),
        ],
      });
      const httpLocal = TestBed.inject(HttpTestingController);
      const harness = await RouterTestingHarness.create();
      await harness.navigateByUrl('/biblia/exodo');
      httpLocal.expectOne('/api/livros').flush([item('genesis', 1), { ...item('exodo', 2), nome: 'Êxodo' }]);
      harness.detectChanges();
      expect(harness.routeNativeElement!.querySelector('.sidebar__trocar-atual')?.textContent).toContain('Êxodo');
    });
  });
});
