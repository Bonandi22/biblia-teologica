import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Component, signal } from '@angular/core';
import { ApontamentosComponent } from './apontamentos.component';
import type { Apontamento } from '../../../../core/models/conteudo.model';

const apontamento = (extra: Partial<Apontamento> = {}): Apontamento => ({
  referencia: 'Isaías 7:14',
  anuncia: 'Uma virgem conceberá.[^1]',
  notas: [{ numero: 1, citacao: 'Obra A', url: null }],
  ...extra,
});

@Component({
  imports: [ApontamentosComponent],
  template: `
    <bt-apontamentos [itens]="profecias()" prefixo="pro" />
    <bt-apontamentos [itens]="promessas()" prefixo="prm" />
  `,
})
class HostComponent {
  readonly profecias = signal<Apontamento[]>([]);
  readonly promessas = signal<Apontamento[]>([]);
}

describe('ApontamentosComponent', () => {
  const montar = (profecias: Apontamento[], promessas: Apontamento[] = []) => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.profecias.set(profecias);
    fixture.componentInstance.promessas.set(promessas);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('lista os itens na ordem recebida', () => {
    const el = montar([
      apontamento({ referencia: 'Isaías 53:7' }),
      apontamento({ referencia: 'Isaías 7:14' }),
    ]);
    const refs = Array.from(el.querySelectorAll('.apontamento__ref')).map((h) =>
      h.textContent!.trim(),
    );
    expect(refs).toEqual(['Isaías 53:7', 'Isaías 7:14']);
  });

  it('não mostra categoria nem cumprimento', () => {
    const texto = montar([apontamento()]).textContent!;
    expect(texto).not.toContain('Cumprida');
    expect(texto).not.toContain('Cumprimento');
    expect(texto).not.toContain('aguardada');
    expect(texto).not.toContain('Fato histórico');
  });

  it('gera ids de nota distintos entre itens, e cada link aponta para a própria nota', () => {
    const el = montar([apontamento(), apontamento({ referencia: 'Isaías 9:6' })]);
    const ids = Array.from(el.querySelectorAll('li[id]')).map((li) => li.id);
    expect(ids).toEqual(['nota-pro-0-1', 'nota-pro-1-1']);
    const hrefs = Array.from(el.querySelectorAll('sup a')).map((a) => a.getAttribute('href'));
    expect(hrefs).toEqual(['/#nota-pro-0-1', '/#nota-pro-1-1']);
    for (const h of hrefs) expect(el.querySelector(h!.slice(h!.indexOf('#')))).not.toBeNull();
  });

  it('prefixos de profecias e promessas geram ids de nota distintos', () => {
    const el = montar([apontamento()], [apontamento({ referencia: 'Isaías 65:17' })]);
    const ids = Array.from(el.querySelectorAll('li[id]')).map((li) => li.id);
    expect(ids).toEqual(['nota-pro-0-1', 'nota-prm-0-1']);
  });
});
