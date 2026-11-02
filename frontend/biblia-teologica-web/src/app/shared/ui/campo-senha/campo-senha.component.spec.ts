import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CampoSenhaComponent } from './campo-senha.component';

@Component({
  standalone: true,
  imports: [CampoSenhaComponent, ReactiveFormsModule],
  template: `<bt-campo-senha label="Senha" [formControl]="ctrl" />`,
})
class HostComponent {
  ctrl = new FormControl('');
}

describe('CampoSenhaComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('começa como password e o toggle alterna para text', () => {
    const fixture = criar();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    const botao = fixture.nativeElement.querySelector('.senha__toggle') as HTMLButtonElement;
    expect(input.getAttribute('type')).toBe('password');
    expect(botao.getAttribute('aria-pressed')).toBe('false');
    expect(botao.getAttribute('aria-label')).toBe('Mostrar senha');
    botao.click();
    fixture.detectChanges();
    expect(input.getAttribute('type')).toBe('text');
    expect(botao.getAttribute('aria-pressed')).toBe('true');
    expect(botao.getAttribute('aria-label')).toBe('Esconder senha');
  });

  it('propaga o valor para o FormControl', () => {
    const fixture = criar();
    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'segredo123';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.componentInstance.ctrl.value).toBe('segredo123');
  });

  it('escreve o valor vindo do FormControl no input', () => {
    const fixture = criar();
    fixture.componentInstance.ctrl.setValue('abc');
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('input') as HTMLInputElement).value).toBe('abc');
  });
});
