import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import type { Bloco } from '../../../core/models/conteudo.model';
import { BlocoConteudoComponent } from './bloco-conteudo.component';

const item = { nome: 'Isaías', texto: 'Profeta.', saberMais: null, estudoSlug: null, referencias: [] };

@Component({
  standalone: true,
  imports: [BlocoConteudoComponent],
  template: `<bt-bloco-conteudo [bloco]="bloco()" rotuloPersonagens="Quem aparece" rotuloAcontecimentos="O que acontece" />`,
})
class HostComponent {
  readonly bloco = signal<Bloco>({ explicacao: 'Texto.', personagens: [], acontecimentos: [], notas: [] });
}

describe('BlocoConteudoComponent', () => {
  const criar = (parcial: Partial<Bloco>) => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.bloco.update((b) => ({ ...b, ...parcial }));
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('esconde as seções quando personagens e acontecimentos estão vazios', () => {
    const el = criar({});
    expect(el.querySelector('section')).toBeNull();
    expect(el.textContent).toContain('Texto.');
  });

  it('mostra só personagens, com o rótulo recebido, quando não há acontecimentos', () => {
    const el = criar({ personagens: [item] });
    const titulos = Array.from(el.querySelectorAll('h3')).map((h) => h.textContent?.trim());
    expect(titulos).toEqual(['Quem aparece']);
  });

  it('mostra só acontecimentos quando não há personagens', () => {
    const el = criar({ acontecimentos: [{ quando: '701 a.C.', item: { ...item, nome: '' } }] });
    const titulos = Array.from(el.querySelectorAll('h3')).map((h) => h.textContent?.trim());
    expect(titulos).toEqual(['O que acontece']);
    expect(el.textContent).toContain('701 a.C.');
  });
});

@Component({
  standalone: true,
  imports: [BlocoConteudoComponent],
  template: `<bt-bloco-conteudo [bloco]="bloco" prefixo="pro-1" />`,
})
class BlocoPrefixadoComponent {
  readonly bloco: Bloco = {
    explicacao: 'Intro[^1]',
    personagens: [{ ...item, texto: 'Texto[^1]', saberMais: 'Mais[^1]' }],
    acontecimentos: [],
    notas: [{ numero: 1, citacao: 'Obra', url: null }],
  };
}

describe('BlocoConteudoComponent com prefixo', () => {
  it('repassa o prefixo ao texto, aos itens e à bibliografia', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(BlocoPrefixadoComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const hrefs = Array.from(el.querySelectorAll('sup a')).map((a) => a.getAttribute('href'));
    expect(hrefs).toEqual(['#nota-pro-1-1', '#nota-pro-1-1', '#nota-pro-1-1']);
    expect(el.querySelector('li#nota-pro-1-1')).not.toBeNull();
  });
});
