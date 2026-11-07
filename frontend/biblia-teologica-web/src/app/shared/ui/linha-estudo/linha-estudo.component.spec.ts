import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { LinhaEstudoComponent } from './linha-estudo.component';
import { TextoNotasComponent } from '../texto-notas/texto-notas.component';

@Component({
  imports: [LinhaEstudoComponent, TextoNotasComponent],
  template: `
    <bt-linha-estudo
      [referencias]="['Isaías 1:1']"
      [notas]="[{ numero: 1, citacao: 'Obra A', url: 'https://exemplo.org' }]"
      prefixo="ctx"
      titulo="Referências"
    >
      <p><bt-texto-notas texto="Texto com chamada.[^1]" prefixo="ctx" /></p>
    </bt-linha-estudo>
  `,
})
class HostComponent {}

@Component({
  imports: [LinhaEstudoComponent],
  template: `<bt-linha-estudo><p>Só texto.</p></bt-linha-estudo>`,
})
class HostSemColunaComponent {}

@Component({
  imports: [LinhaEstudoComponent],
  template: `
    <bt-linha-estudo [referencias]="['Isaías 1:1', 'Isaías 7:3', 'Romanos 9:33']">
      <p>Texto.</p>
    </bt-linha-estudo>
  `,
})
class HostVariasRefsComponent {}

@Component({
  imports: [LinhaEstudoComponent],
  template: `
    <bt-linha-estudo [notas]="[{ numero: 1, citacao: 'Isaías 40:1', url: null }]" prefixo="pan">
      <p>Texto.</p>
    </bt-linha-estudo>
  `,
})
class HostNotaBibliaComponent {}

describe('LinhaEstudoComponent', () => {
  const montar = () => {
    const f = TestBed.createComponent(HostComponent);
    f.detectChanges();
    return f.nativeElement as HTMLElement;
  };

  it('mostra referências e notas na coluna, com título', () => {
    const coluna = montar().querySelector('.linha__coluna')!;
    expect(coluna.textContent).toContain('Referências');
    expect(coluna.querySelector('a')?.getAttribute('href')).toContain('/arc/is/1/1');
    expect(coluna.textContent).toContain('Obra A');
    expect(coluna.querySelector('a[href="https://exemplo.org"]')?.textContent).toContain('Abrir fonte');
  });

  it('escreve o livro uma vez por grupo, e cada referência continua sendo link para a ARC', () => {
    const f = TestBed.createComponent(HostVariasRefsComponent);
    f.detectChanges();
    const refs = (f.nativeElement as HTMLElement).querySelector('.linha__refs')!;
    const grupos = Array.from(refs.querySelectorAll(':scope > li')).map((li) =>
      li.textContent!.replace(/\s+/g, ' ').trim(),
    );
    expect(grupos).toEqual(['Isaías 1:1; 7:3', 'Romanos 9:33']);
    const links = refs.querySelectorAll('a');
    expect(links.length).toBe(3);
    expect(links[1].textContent).toBe('7:3');
    expect(links[1].getAttribute('href')).toContain('/arc/is/7/3');
    expect(links[1].getAttribute('aria-label')).toContain('Isaías 7:3');
  });

  it('nota que é só referência bíblica vira link para o versículo, sem "Abrir fonte"', () => {
    const f = TestBed.createComponent(HostNotaBibliaComponent);
    f.detectChanges();
    const nota = (f.nativeElement as HTMLElement).querySelector('#nota-pan-1')!;
    const link = nota.querySelector('a')!;
    expect(link.textContent).toBe('Isaías 40:1');
    expect(link.getAttribute('href')).toContain('/arc/is/40/1');
    expect(nota.textContent).not.toContain('Abrir fonte');
  });

  it('coluna é um grupo rotulado, não um landmark repetido por parágrafo', () => {
    const el = montar();
    expect(el.querySelector('aside')).toBeNull();
    const coluna = el.querySelector('.linha__coluna')!;
    expect(coluna.getAttribute('role')).toBe('group');
    expect(coluna.getAttribute('aria-label')).toBe('Referências');
  });

  it('mantém o id da nota para a chamada encontrar a âncora', () => {
    const el = montar();
    const href = el.querySelector('sup a')!.getAttribute('href')!;
    expect(href).toBe('/#nota-ctx-1');
    expect(el.querySelector(href.slice(href.indexOf('#')))?.textContent).toContain('Obra A');
  });

  it('focar a chamada destaca a nota', () => {
    const el = montar();
    const chamada = el.querySelector('sup a') as HTMLElement;
    chamada.dispatchEvent(new FocusEvent('focusin', { bubbles: true }));
    expect(el.querySelector('#nota-ctx-1')!.classList).toContain('is-destaque');

    chamada.dispatchEvent(new FocusEvent('focusout', { bubbles: true }));
    expect(el.querySelector('#nota-ctx-1')!.classList).not.toContain('is-destaque');
  });

  it('passar o mouse sobre a nota destaca a chamada', () => {
    const el = montar();
    el.querySelector('#nota-ctx-1')!.dispatchEvent(new MouseEvent('mouseover', { bubbles: true }));
    expect(el.querySelector('sup a')!.classList).toContain('is-destaque');
  });

  it('mantém o destaque ao tirar o mouse enquanto a nota tem foco', () => {
    const el = montar();
    const nota = el.querySelector('#nota-ctx-1') as HTMLElement;
    nota.focus();
    expect(nota.classList).toContain('is-destaque');

    el.querySelector('sup a')!.dispatchEvent(new MouseEvent('mouseout', { bubbles: true }));
    expect(nota.classList).toContain('is-destaque');
    expect(el.querySelector('sup a')!.classList).toContain('is-destaque');
  });

  it('não mostra coluna quando não há referências nem notas', () => {
    const f = TestBed.createComponent(HostSemColunaComponent);
    f.detectChanges();
    const el = f.nativeElement as HTMLElement;
    expect(el.querySelector('.linha__coluna')).toBeNull();
    expect(el.textContent).toContain('Só texto.');
  });
});
