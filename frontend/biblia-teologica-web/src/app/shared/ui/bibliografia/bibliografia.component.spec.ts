import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import type { Nota } from '../../../core/models/conteudo.model';
import { BibliografiaComponent } from './bibliografia.component';

@Component({
  standalone: true,
  imports: [BibliografiaComponent],
  template: `<bt-bibliografia [notas]="notas()" />`,
})
class HostComponent {
  readonly notas = signal<Nota[]>([]);
}

describe('BibliografiaComponent', () => {
  const criar = (notas: Nota[]) => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.notas.set(notas);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('não renderiza nada sem notas', () => {
    const el = criar([]);
    expect(el.querySelector('section')).toBeNull();
    expect(el.textContent).not.toContain('Bibliografia');
  });

  it('lista os itens com id nota-n e o título Bibliografia', () => {
    const el = criar([
      { numero: 1, citacao: 'Obra A', url: null },
      { numero: 3, citacao: 'Obra B', url: null },
    ]);
    expect(el.querySelector('h3')?.textContent?.trim()).toBe('Bibliografia');
    expect(el.querySelector('#nota-1')?.textContent).toContain('Obra A');
    expect(el.querySelector('#nota-3')?.textContent).toContain('Obra B');
  });

  it('só cria link, com rel seguro, quando há URL', () => {
    const el = criar([
      { numero: 1, citacao: 'Com link', url: 'https://exemplo.org/x' },
      { numero: 2, citacao: 'Sem link', url: null },
    ]);
    const links = el.querySelectorAll('a');
    expect(links.length).toBe(1);
    expect(links[0].getAttribute('rel')).toBe('noopener noreferrer');
    expect(links[0].getAttribute('href')).toBe('https://exemplo.org/x');
  });
});

@Component({
  standalone: true,
  imports: [BibliografiaComponent],
  template: `<bt-bibliografia [notas]="notas" prefixo="p1" /><bt-bibliografia [notas]="notas" prefixo="p2" />`,
})
class DoisBlocosComponent {
  readonly notas: Nota[] = [{ numero: 1, citacao: 'Obra', url: null }];
}

describe('BibliografiaComponent com prefixo', () => {
  it('gera ids distintos para a mesma nota em blocos diferentes', () => {
    const fixture = TestBed.createComponent(DoisBlocosComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('#nota-p1-1')).not.toBeNull();
    expect(el.querySelector('#nota-p2-1')).not.toBeNull();
    expect(el.querySelector('#nota-1')).toBeNull();
  });
});
