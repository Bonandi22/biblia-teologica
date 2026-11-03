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
