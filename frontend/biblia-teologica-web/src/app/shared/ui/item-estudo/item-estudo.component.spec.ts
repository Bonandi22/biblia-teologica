import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import type { Item } from '../../../core/models/conteudo.model';
import { ItemEstudoComponent } from './item-estudo.component';

@Component({
  standalone: true,
  imports: [ItemEstudoComponent],
  template: `<bt-item-estudo [item]="item()" />`,
})
class HostComponent {
  readonly item = signal<Item>({ nome: '', texto: '', saberMais: null, estudoSlug: null, referencias: [] });
}

describe('ItemEstudoComponent', () => {
  const criar = (parcial: Partial<Item>) => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.item.update((i) => ({ ...i, nome: 'Ezequias', texto: 'Rei de Judá.', ...parcial }));
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('mostra Saber mais como details fechado', () => {
    const d = criar({ saberMais: 'Mais detalhe.' }).querySelector('details') as HTMLDetailsElement;
    expect(d).not.toBeNull();
    expect(d.open).toBe(false);
    expect(d.querySelector('summary')?.textContent?.trim()).toBe('Saber mais');
  });

  it('não mostra Saber mais sem saberMais', () => {
    expect(criar({}).querySelector('details')).toBeNull();
  });

  it('leva o link Estudo completo a /estudos/slug', () => {
    const a = criar({ estudoSlug: 'servo-sofredor' }).querySelector('a.item__estudo');
    expect(a?.textContent?.trim()).toBe('Estudo completo');
    expect(a?.getAttribute('href')).toBe('/estudos/servo-sofredor');
  });

  it('não mostra Estudo completo sem estudoSlug', () => {
    expect(criar({}).querySelector('a.item__estudo')).toBeNull();
  });
});
