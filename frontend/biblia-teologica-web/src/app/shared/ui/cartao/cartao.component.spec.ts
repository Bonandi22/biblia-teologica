import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { CartaoComponent } from './cartao.component';

@Component({
  standalone: true,
  imports: [CartaoComponent],
  template: `<bt-cartao [titulo]="titulo()">corpo</bt-cartao>`,
})
class HostComponent {
  readonly titulo = signal('');
}

describe('CartaoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('não rende h1 quando titulo vazio', () => {
    expect(criar().nativeElement.querySelector('h1')).toBeNull();
  });

  it('rende h1 com o titulo', () => {
    const fixture = criar();
    fixture.componentInstance.titulo.set('Entrar');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent.trim()).toBe('Entrar');
  });

  it('projeta o conteúdo', () => {
    expect(criar().nativeElement.textContent).toContain('corpo');
  });
});
