import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { LivroMolduraComponent } from './livro-moldura.component';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';

@Component({
  imports: [LivroMolduraComponent],
  template: `
    @if (visivel()) {
      <bt-livro-moldura [slug]="slug()" resumo="Frase do livro."><p>Conteúdo</p></bt-livro-moldura>
    }
  `,
})
class HostComponent {
  readonly slug = signal('isaias');
  readonly visivel = signal(true);
}

const livro = (slug: string, nome: string, ordem: number, secaoRotulo: string) => ({
  slug,
  nome,
  ordem,
  secao: secaoRotulo,
  secaoRotulo,
});
const LIVROS = [
  livro('ezequias', 'Outro', 22, 'Poéticos'),
  livro('isaias', 'ISAÍAS', 23, 'PROFETAS MAIORES'),
  livro('jeremias', 'JEREMIAS', 24, 'PROFETAS MAIORES'),
  livro('obadias', 'Obadias', 31, 'Profetas menores'),
];

describe('LivroMolduraComponent', () => {
  const montar = async (responde = true) => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const req = TestBed.inject(HttpTestingController).expectOne('/api/livros');
    if (responde) req.flush(LIVROS);
    else req.error(new ProgressEvent('error'));
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  };
  const el = (f: { nativeElement: unknown }) => f.nativeElement as HTMLElement;

  it('mostra o nome, a frase e o conteúdo projetado', async () => {
    const e = el(await montar());
    expect(e.querySelector('h1')?.textContent?.trim()).toBe('Isaías');
    expect(e.textContent).toContain('Frase do livro.');
    expect(e.textContent).toContain('Conteúdo');
  });

  it('lista só os livros da divisão, com o atual marcado', async () => {
    const e = el(await montar());
    const links = Array.from(e.querySelectorAll('.moldura__divisao a'));
    expect(links.map((a) => a.textContent?.trim())).toEqual(['Isaías', 'Jeremias']);
    expect(links[0].getAttribute('aria-current')).toBe('page');
    expect(links[1].getAttribute('aria-current')).toBeNull();
    expect(e.querySelector('.moldura__divisao')?.textContent).toContain('Profetas maiores');
  });

  it('no celular, o controle da divisão diz o que faz', async () => {
    const e = el(await montar());
    expect(e.querySelector('.moldura__secao-botao')?.textContent?.trim()).toBe(
      'Outros livros de Profetas maiores',
    );
  });

  it('liga cada capítulo à ARC em nova aba', async () => {
    const e = el(await montar());
    const capitulos = Array.from(e.querySelectorAll<HTMLAnchorElement>('.moldura__capitulos a'));
    expect(capitulos).toHaveLength(66);
    expect(capitulos[52].textContent?.trim()).toBe('53');
    expect(capitulos[52].getAttribute('href')).toBe('https://www.bibliaonline.com.br/arc/is/53');
    expect(capitulos[52].getAttribute('target')).toBe('_blank');
    expect(capitulos[52].getAttribute('rel')).toBe('noopener noreferrer');
    expect(capitulos[52].getAttribute('aria-label')).toBe('Isaías 53 na ARC, abre em nova aba');
  });

  it('identifica os capítulos e os mantém fora do bloco do título', async () => {
    const e = el(await montar());
    const capitulos = e.querySelector('.moldura__capitulos')!;
    expect(capitulos.querySelector('.moldura__capitulos-titulo')?.textContent?.trim()).toBe(
      'Capítulos',
    );
    expect(e.querySelector('.moldura__titulos')?.contains(capitulos)).toBe(false);
    expect(e.querySelector('.moldura__titulos h1')).not.toBeNull();
  });

  it('só limita a altura dos capítulos no livro que passa de 66', async () => {
    const f = await montar();
    const capitulos = () => el(f).querySelector('.moldura__capitulos')!;
    expect(capitulos().classList).not.toContain('moldura__capitulos--longo');

    f.componentInstance.slug.set('salmos');
    f.detectChanges();
    expect(capitulos().querySelectorAll('a')).toHaveLength(150);
    expect(capitulos().classList).toContain('moldura__capitulos--longo');
  });

  it('define o lugar atual e o limpa ao sair', async () => {
    const f = await montar();
    const lugar = TestBed.inject(LugarAtualService);
    expect(lugar.texto()).toBe('Isaías 1–66');

    f.componentInstance.visivel.set(false);
    f.detectChanges();
    expect(lugar.texto()).toBe('');
  });

  it('mostra só o nome no lugar atual quando o livro tem um capítulo', async () => {
    const f = await montar();
    f.componentInstance.slug.set('obadias');
    f.detectChanges();
    expect(TestBed.inject(LugarAtualService).texto()).toBe('Obadias');
  });

  it('sem a lista de livros, não mostra o slug nem a divisão, e os capítulos seguem', async () => {
    const e = el(await montar(false));
    expect(e.querySelector('h1')?.textContent).not.toContain('isaias');
    expect(e.querySelector('.moldura__divisao')).toBeNull();
    expect(e.querySelectorAll('.moldura__capitulos a')).toHaveLength(66);
    expect(TestBed.inject(LugarAtualService).texto()).toBe('');
  });
});
