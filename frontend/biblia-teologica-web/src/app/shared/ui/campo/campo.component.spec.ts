import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BtInputDirective } from '../bt-input.directive';
import { CampoComponent } from './campo.component';

@Component({
  standalone: true,
  imports: [CampoComponent, BtInputDirective],
  template: `
    <bt-campo [label]="label()" [erro]="erro()" [hint]="hint()">
      <input btInput />
    </bt-campo>
  `,
})
class HostComponent {
  readonly label = signal('Email');
  readonly erro = signal<string | null>(null);
  readonly hint = signal('');
}

describe('CampoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('associa o label ao input pelo id', () => {
    const el = criar().nativeElement as HTMLElement;
    const label = el.querySelector('label') as HTMLLabelElement;
    const input = el.querySelector('input') as HTMLInputElement;
    expect(input.id).toBeTruthy();
    expect(label.getAttribute('for')).toBe(input.id);
  });

  it('mostra o hint quando não há erro', () => {
    const fixture = criar();
    fixture.componentInstance.hint.set('Mínimo 8 caracteres');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Mínimo 8 caracteres');
  });

  it('erro: role alert, aria-invalid e aria-describedby ligados', () => {
    const fixture = criar();
    fixture.componentInstance.erro.set('Email inválido');
    fixture.componentInstance.hint.set('ignorado quando há erro');
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const erro = el.querySelector('[role="alert"]') as HTMLElement;
    const input = el.querySelector('input') as HTMLInputElement;
    expect(erro.textContent?.trim()).toBe('Email inválido');
    expect(input.getAttribute('aria-invalid')).toBe('true');
    expect(input.getAttribute('aria-describedby')).toBe(erro.id);
    expect(el.textContent).not.toContain('ignorado quando há erro');
  });
});
