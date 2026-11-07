import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { TextoNotasComponent, idNota, segmentarNotas } from './texto-notas.component';

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
    expect(link.getAttribute('href')).toBe('/#nota-1');
    expect(el.textContent).toContain('Texto');
  });

  it('Href_LevaOCaminhoDaPagina_QuandoForaDaRaiz', async () => {
    // Com <base href="/">, um href só com "#nota-1" resolve para a home.
    TestBed.configureTestingModule({
      providers: [provideRouter([{ path: 'biblia/:slug/:aba', component: HostComponent }])],
    });
    await TestBed.inject(Router).navigateByUrl('/biblia/isaias/contexto');
    const el: HTMLElement = criar('Texto[^1]').nativeElement;
    expect(el.querySelector('sup a')?.getAttribute('href')).toBe('/biblia/isaias/contexto#nota-1');
  });

  it('Href_AcompanhaANavegacao_QuandoAPaginaMuda', async () => {
    TestBed.configureTestingModule({
      providers: [provideRouter([{ path: 'biblia/:slug/:aba', component: HostComponent }])],
    });
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/biblia/isaias/contexto');
    const fixture = criar('Texto[^1]');
    await router.navigateByUrl('/biblia/isaias/mundo');
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('sup a')?.getAttribute('href')).toBe(
      '/biblia/isaias/mundo#nota-1',
    );
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

  it('leva o foco à nota e só rola o mínimo necessário', () => {
    const alvo = document.createElement('li');
    alvo.id = 'nota-1';
    alvo.tabIndex = -1;
    alvo.scrollIntoView = vi.fn();
    document.body.appendChild(alvo);
    const el: HTMLElement = criar('Texto[^1]').nativeElement;
    (el.querySelector('sup a') as HTMLElement).click();
    expect(alvo.scrollIntoView).toHaveBeenCalledWith({ block: 'nearest' });
    expect(document.activeElement).toBe(alvo);
    alvo.remove();
  });

  it('não renderiza HTML presente no texto', () => {
    const el: HTMLElement = criar('<b>negrito</b><img src=x>').nativeElement;
    expect(el.querySelector('b')).toBeNull();
    expect(el.querySelector('img')).toBeNull();
    expect(el.textContent).toContain('<b>negrito</b>');
  });
});

@Component({
  standalone: true,
  imports: [TextoNotasComponent],
  template: `<bt-texto-notas texto="A[^1]" prefixo="a" /><bt-texto-notas texto="B[^1]" prefixo="b" />`,
})
class DoisPrefixosComponent {}

describe('TextoNotasComponent com prefixo', () => {
  it('gera hrefs distintos e rola para a nota da própria instância', () => {
    const alvos = ['a', 'b'].map((p) => {
      const li = document.createElement('li');
      li.id = `nota-${p}-1`;
      li.scrollIntoView = vi.fn();
      document.body.appendChild(li);
      return li;
    });
    const fixture = TestBed.createComponent(DoisPrefixosComponent);
    fixture.detectChanges();
    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('sup a')) as HTMLElement[];
    expect(links.map((l) => l.getAttribute('href'))).toEqual(['/#nota-a-1', '/#nota-b-1']);
    links[1].click();
    expect(alvos[1].scrollIntoView).toHaveBeenCalled();
    expect(alvos[0].scrollIntoView).not.toHaveBeenCalled();
    alvos.forEach((a) => a.remove());
  });

  it('mantém nota-n quando não há prefixo', () => {
    expect(idNota(4)).toBe('nota-4');
    expect(idNota(4, 'x')).toBe('nota-x-4');
  });
});
