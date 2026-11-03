import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Component, signal } from '@angular/core';
import { ProfeciasComponent } from './profecias.component';
import type { Profecia } from '../../../../core/models/conteudo.model';

const profecia = (extra: Partial<Profecia> = {}): Profecia => ({
  referencia: 'Isaías 7:14',
  anuncia: 'Uma virgem conceberá.[^1]',
  categoria: 'cumpridaEmCristo',
  cumprimento: ['Mateus 1:22-23'],
  fatoHistorico: null,
  notas: [{ numero: 1, citacao: 'Obra A', url: null }],
  ...extra,
});

@Component({
  imports: [ProfeciasComponent],
  template: `<bt-profecias [profecias]="profecias()" />`,
})
class HostComponent {
  readonly profecias = signal<Profecia[]>([]);
}

describe('ProfeciasComponent', () => {
  const montar = (profecias: Profecia[]) => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.profecias.set(profecias);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('mostra o rótulo "Cumprida em Cristo" para cumpridaEmCristo', () => {
    expect(montar([profecia()]).textContent).toContain('Cumprida em Cristo');
  });

  it('mostra o rótulo "Cumprida na história" para cumpridaNaHistoria', () => {
    const el = montar([
      profecia({ categoria: 'cumpridaNaHistoria', fatoHistorico: 'Queda de Babilônia em 539 a.C.' }),
    ]);
    expect(el.textContent).toContain('Cumprida na história');
  });

  it('mostra o rótulo "Promessa ainda aguardada" para promessaAguardada', () => {
    expect(montar([profecia({ categoria: 'promessaAguardada' })]).textContent).toContain(
      'Promessa ainda aguardada',
    );
  });

  it('mostra as referências de cumprimento', () => {
    const el = montar([profecia({ cumprimento: ['Mateus 1:22-23', 'Lucas 1:31'] })]);
    expect(el.textContent).toContain('Mateus 1:22-23');
    expect(el.textContent).toContain('Lucas 1:31');
  });

  it('mostra o fato histórico quando há', () => {
    const el = montar([
      profecia({ categoria: 'cumpridaNaHistoria', fatoHistorico: 'Queda de Babilônia em 539 a.C.' }),
    ]);
    expect(el.textContent).toContain('Queda de Babilônia em 539 a.C.');
  });

  it('não mostra fato histórico quando não há', () => {
    expect(montar([profecia()]).querySelector('.profecia__fato')).toBeNull();
  });

  it('gera ids de nota distintos para duas profecias com nota 1 e cada link aponta para a própria nota', () => {
    const el = montar([profecia(), profecia({ referencia: 'Isaías 9:6' })]);
    const ids = Array.from(el.querySelectorAll('li[id]')).map((li) => li.id);
    expect(ids).toEqual(['nota-pro-0-1', 'nota-pro-1-1']);
    const hrefs = Array.from(el.querySelectorAll('sup a')).map((a) => a.getAttribute('href'));
    expect(hrefs).toEqual(['#nota-pro-0-1', '#nota-pro-1-1']);
    for (const h of hrefs) expect(el.querySelector(h!)).not.toBeNull();
  });
});
