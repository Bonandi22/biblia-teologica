import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BotaoComponent } from './botao.component';

@Component({
  standalone: true,
  imports: [BotaoComponent],
  template: `
    <form (submit)="$event.preventDefault(); submeteu.set(true)">
      <bt-botao
        [variante]="variante()"
        [type]="type()"
        [carregando]="carregando()"
        [disabled]="disabled()"
      >
        Enviar
      </bt-botao>
    </form>
  `,
})
class HostComponent {
  readonly variante = signal<'primario' | 'fantasma' | 'texto'>('primario');
  readonly type = signal<'button' | 'submit'>('button');
  readonly carregando = signal(false);
  readonly disabled = signal(false);
  readonly submeteu = signal(false);
}

describe('BotaoComponent', () => {
  const criar = () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('aplica a classe da variante', () => {
    const fixture = criar();
    fixture.componentInstance.variante.set('fantasma');
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector('button');
    expect(btn.className).toContain('botao--fantasma');
  });

  it('em carregamento fica disabled e aria-busy', () => {
    const fixture = criar();
    fixture.componentInstance.carregando.set(true);
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(btn.disabled).toBe(true);
    expect(btn.getAttribute('aria-busy')).toBe('true');
  });

  it('respeita disabled', () => {
    const fixture = criar();
    fixture.componentInstance.disabled.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button').disabled).toBe(true);
  });

  it('type submit dispara o submit do formulário', () => {
    const fixture = criar();
    fixture.componentInstance.type.set('submit');
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    expect(fixture.componentInstance.submeteu()).toBe(true);
  });

  it('type button não dispara o submit do formulário', () => {
    const fixture = criar();
    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    expect(fixture.componentInstance.submeteu()).toBe(false);
  });
});
