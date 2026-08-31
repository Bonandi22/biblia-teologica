import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SecaoCardComponent } from './secao-card.component';

describe('SecaoCardComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SecaoCardComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = (disponivel: boolean) => {
    const fixture = TestBed.createComponent(SecaoCardComponent);
    fixture.componentRef.setInput('titulo', 'Estudos');
    fixture.componentRef.setInput('descricao', 'Artigos longos.');
    fixture.componentRef.setInput('rota', '/estudos');
    fixture.componentRef.setInput('disponivel', disponivel);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o selo "Em breve" quando não disponível', () => {
    expect(criar(false).nativeElement.textContent).toContain('Em breve');
  });

  it('esconde o selo quando disponível', () => {
    expect(criar(true).nativeElement.textContent).not.toContain('Em breve');
  });

  it('liga para a rota indicada', () => {
    const link = criar(false).nativeElement.querySelector('a') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/estudos');
    expect(link.textContent).toContain('Estudos');
  });
});
