import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AlertaComponent } from './alerta.component';

@Component({
  standalone: true,
  imports: [AlertaComponent],
  template: `<bt-alerta [tipo]="tipo()">Mensagem</bt-alerta>`,
})
class HostComponent {
  readonly tipo = signal<'erro' | 'sucesso'>('erro');
}

describe('AlertaComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('usa role="alert" quando tipo erro', () => {
    const el = criar().nativeElement.querySelector('.alerta');
    expect(el.getAttribute('role')).toBe('alert');
  });

  it('usa role="status" quando tipo sucesso', () => {
    const fixture = criar();
    fixture.componentInstance.tipo.set('sucesso');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.alerta').getAttribute('role')).toBe('status');
  });

  it('projeta o conteúdo', () => {
    expect(criar().nativeElement.textContent).toContain('Mensagem');
  });
});
