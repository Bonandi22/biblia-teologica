import { TestBed } from '@angular/core/testing';
import { ReferenciaComponent } from './referencia.component';

describe('ReferenciaComponent', () => {
  const montar = (texto: string) => {
    const fixture = TestBed.createComponent(ReferenciaComponent);
    fixture.componentRef.setInput('texto', texto);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('abre a ARC em nova aba e avisa no nome acessível', () => {
    const a = montar('Isaías 6:1–8').querySelector('a')!;
    expect(a.textContent!.trim()).toBe('Isaías 6:1–8');
    expect(a.getAttribute('href')).toBe('https://www.bibliaonline.com.br/arc/is/6/1-8');
    expect(a.getAttribute('target')).toBe('_blank');
    expect(a.getAttribute('rel')).toBe('noopener noreferrer');
    expect(a.getAttribute('aria-label')).toBe('Isaías 6:1–8 na ARC, abre em nova aba');
  });

  it('mostra texto simples, sem link, quando a referência não pode ser lida', () => {
    const el = montar('Eis o meu servo');
    expect(el.querySelector('a')).toBeNull();
    expect(el.textContent!.trim()).toBe('Eis o meu servo');
  });
});
