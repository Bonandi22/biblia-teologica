import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { CampoFichaComponent } from './campo-ficha.component';

@Component({
  standalone: true,
  imports: [CampoFichaComponent],
  template: `<bt-campo-ficha rotulo="Tema central">Deus criador</bt-campo-ficha>`,
})
class HostComponent {}

it('projeta rótulo e valor', () => {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const texto = fixture.nativeElement.textContent;
  expect(texto).toContain('Tema central');
  expect(texto).toContain('Deus criador');
});
