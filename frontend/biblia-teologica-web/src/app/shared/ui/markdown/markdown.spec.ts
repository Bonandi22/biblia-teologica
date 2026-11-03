import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { renderizarMarkdown } from './markdown';
import { MarkdownComponent } from './markdown.component';

const html = (md: string): HTMLElement => {
  const div = document.createElement('div');
  div.innerHTML = renderizarMarkdown(md);
  return div;
};

describe('renderizarMarkdown', () => {
  it('remove script, img com onerror, iframe e style', () => {
    const el = html(
      'texto\n\n<script>alert(1)</script>\n\n<img src=x onerror=alert(1)>\n\n<iframe src="https://x.com"></iframe>\n\n<p style="color:red">p</p>',
    );
    expect(el.querySelector('script')).toBeNull();
    expect(el.querySelector('img')).toBeNull();
    expect(el.querySelector('iframe')).toBeNull();
    expect(el.querySelector('[style]')).toBeNull();
    expect(el.innerHTML).not.toContain('onerror');
  });

  it('remove href javascript, data e vbscript', () => {
    for (const url of ['javascript:alert(1)', 'data:text/html,x', 'vbscript:msgbox(1)', 'JaVaScRiPt:alert(1)']) {
      const el = html(`[clique](${url})`);
      expect(el.querySelector('a[href]')).toBeNull();
    }
  });

  it('mantém título, lista, ênfase e citação', () => {
    const el = html('## Título\n\n- um\n- dois\n\n**forte** e *ênfase*\n\n> citação');
    expect(el.querySelector('h2')?.textContent).toBe('Título');
    expect(el.querySelectorAll('li').length).toBe(2);
    expect(el.querySelector('strong')).not.toBeNull();
    expect(el.querySelector('em')).not.toBeNull();
    expect(el.querySelector('blockquote')).not.toBeNull();
  });

  it('mantém link https com rel noopener noreferrer', () => {
    const a = html('[site](https://exemplo.com/x)').querySelector('a') as HTMLAnchorElement;
    expect(a.getAttribute('href')).toBe('https://exemplo.com/x');
    expect(a.getAttribute('rel')).toBe('noopener noreferrer');
  });

  it('remove link http sem TLS', () => {
    expect(html('[site](http://exemplo.com)').querySelector('a[href]')).toBeNull();
  });

  it('transforma [^12] em link de nota dentro de parágrafo', () => {
    const a = html('Texto[^12] fim').querySelector('p sup a') as HTMLAnchorElement;
    expect(a.getAttribute('data-nota')).toBe('12');
    expect(a.getAttribute('href')).toBe('#nota-12');
    expect(a.textContent).toBe('12');
  });

  it('não transforma [^n] dentro de bloco ou trecho de código', () => {
    const bloco = html('```\nx[^1]\n```');
    expect(bloco.querySelector('sup')).toBeNull();
    expect(bloco.querySelector('pre code')?.textContent).toContain('[^1]');
    const inline = html('use `[^2]` assim');
    expect(inline.querySelector('sup')).toBeNull();
    expect(inline.querySelector('code')?.textContent).toBe('[^2]');
  });
});

@Component({
  standalone: true,
  imports: [MarkdownComponent],
  template: `<bt-markdown [conteudo]="md()" />`,
})
class HostComponent {
  readonly md = signal('');
}

describe('MarkdownComponent', () => {
  it('Clique_RolaAteNota_QuandoLinkDeNota', () => {
    const alvo = document.createElement('li');
    alvo.id = 'nota-3';
    alvo.scrollIntoView = vi.fn();
    document.body.appendChild(alvo);
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.md.set('Texto[^3] fim');
    fixture.detectChanges();
    const link = (fixture.nativeElement as HTMLElement).querySelector('sup a') as HTMLElement;
    expect(link).not.toBeNull();
    link.click();
    expect(alvo.scrollIntoView).toHaveBeenCalled();
    alvo.remove();
  });

  it('não renderiza script nem handler pelo componente real', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.md.set('a <script>alert(1)</script> <img src=x onerror=alert(1)>');
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('script')).toBeNull();
    expect(el.querySelector('img')).toBeNull();
  });

  it('mantém o link de nota com href após o sanitizador do Angular, que remove data-nota', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.md.set('T[^3] [s](https://a.com)');
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('sup a')?.getAttribute('href')).toBe('#nota-3');
    expect(el.querySelector('a[href^="https:"]')?.getAttribute('rel')).toBe('noopener noreferrer');
  });
});
