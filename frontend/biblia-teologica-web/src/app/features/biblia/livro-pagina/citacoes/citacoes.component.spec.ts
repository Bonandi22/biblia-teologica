import { TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { CitacoesComponent } from './citacoes.component';
import type { Citacao } from '../../../../core/models/conteudo.model';

const citacao = (extra: Partial<Citacao> = {}): Citacao => ({
  trecho: 'Eis que uma virgem conceberá.',
  citadoEm: ['Mateus 1:23', 'Lucas 1:31'],
  comentario: 'Mateus lê o sinal como cumprido em Jesus.[^1]',
  notas: [{ numero: 1, citacao: 'Obra A', url: null }],
  ...extra,
});

@Component({
  imports: [CitacoesComponent],
  template: `<bt-citacoes [citacoes]="citacoes()" />`,
})
class HostComponent {
  readonly citacoes = signal<Citacao[]>([]);
}

describe('CitacoesComponent', () => {
  const montar = (citacoes: Citacao[]) => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.citacoes.set(citacoes);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('mostra o trecho e todos os lugares em citadoEm', () => {
    const el = montar([citacao()]);
    expect(el.textContent).toContain('Eis que uma virgem conceberá.');
    expect(el.textContent).toContain('Mateus 1:23');
    expect(el.textContent).toContain('Lucas 1:31');
  });

  it('mostra o comentário quando há', () => {
    expect(montar([citacao()]).querySelector('.citacao__comentario')).not.toBeNull();
  });

  it('não mostra o comentário quando não há', () => {
    expect(montar([citacao({ comentario: null })]).querySelector('.citacao__comentario')).toBeNull();
  });

  it('mostra a bibliografia', () => {
    const el = montar([citacao()]);
    expect(el.textContent).toContain('Bibliografia');
    expect(el.textContent).toContain('Obra A');
  });

  it('usa ids de nota distintos por citação', () => {
    const el = montar([citacao(), citacao()]);
    const ids = Array.from(el.querySelectorAll('li[id]')).map((li) => li.id);
    expect(ids).toEqual(['nota-cit-0-1', 'nota-cit-1-1']);
  });
});
