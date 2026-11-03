import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { TextoNotasComponent, segmentarNotas } from './texto-notas.component';

describe('segmentarNotas', () => {
  it('reconhece marcador no início do texto', () => {
    expect(segmentarNotas('[^1] depois')).toEqual([{ nota: 1 }, { texto: ' depois' }]);
  });

  it('reconhece marcador no meio do texto', () => {
    expect(segmentarNotas('antes[^2]depois')).toEqual([{ texto: 'antes' }, { nota: 2 }, { texto: 'depois' }]);
  });

  it('reconhece marcador no fim do texto', () => {
    expect(segmentarNotas('fim[^3]')).toEqual([{ texto: 'fim' }, { nota: 3 }]);
  });

  it('reconhece marcador de dois dígitos', () => {
    expect(segmentarNotas('a[^12]')).toEqual([{ texto: 'a' }, { nota: 12 }]);
  });

  it('mantém colchetes comuns como texto', () => {
    expect(segmentarNotas('veja [texto comum] aqui')).toEqual([{ texto: 'veja [texto comum] aqui' }]);
  });
});

@Component({
  standalone: true,
  imports: [TextoNotasComponent],
  template: `<bt-texto-notas [texto]="texto()" />`,
})
class HostComponent {
  readonly texto = signal('');
}

describe('TextoNotasComponent', () => {
  const criar = (texto: string) => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.texto.set(texto);
    fixture.detectChanges();
    return fixture;
  };

  it('renderiza a nota como sup com link', () => {
    const el: HTMLElement = criar('Texto[^1] fim').nativeElement;
    const link = el.querySelector('sup a') as HTMLAnchorElement;
    expect(link.textContent?.trim()).toBe('1');
    expect(link.getAttribute('href')).toBe('#nota-1');
    expect(el.textContent).toContain('Texto');
  });

  it('rola até a nota ao clicar no link', () => {
    const alvo = document.createElement('li');
    alvo.id = 'nota-1';
    alvo.scrollIntoView = vi.fn();
    document.body.appendChild(alvo);
    const el: HTMLElement = criar('Texto[^1]').nativeElement;
    (el.querySelector('sup a') as HTMLElement).click();
    expect(alvo.scrollIntoView).toHaveBeenCalled();
    alvo.remove();
  });

  it('não renderiza HTML presente no texto', () => {
    const el: HTMLElement = criar('<b>negrito</b><img src=x>').nativeElement;
    expect(el.querySelector('b')).toBeNull();
    expect(el.querySelector('img')).toBeNull();
    expect(el.textContent).toContain('<b>negrito</b>');
  });
});
