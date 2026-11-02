import { TestBed } from '@angular/core/testing';
import { UiProvasComponent } from './ui-provas.component';

describe('UiProvasComponent', () => {
  it('monta com todos os primitivos sem erros', () => {
    TestBed.configureTestingModule({ imports: [UiProvasComponent] });
    const fixture = TestBed.createComponent(UiProvasComponent);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Primitivos');
    expect(fixture.nativeElement.querySelectorAll('bt-botao').length).toBeGreaterThanOrEqual(3);
    expect(fixture.nativeElement.querySelector('bt-campo')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-campo-senha')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-alerta')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('bt-cartao')).toBeTruthy();
  });
});
