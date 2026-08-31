import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { FooterComponent } from './footer.component';

describe('FooterComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [FooterComponent],
      providers: [provideRouter([])],
    });
  });

  const criar = () => {
    const fixture = TestBed.createComponent(FooterComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra os links de navegação do rodapé', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/biblia"]')).toBeTruthy();
    expect(el.querySelector('a[href="/estudos"]')).toBeTruthy();
    expect(el.querySelector('a[href="/noticias"]')).toBeTruthy();
    expect(el.querySelector('a[href="/curiosidades"]')).toBeTruthy();
  });

  it('mostra as redes sociais com aria-label pelo nome', () => {
    const el = criar().nativeElement as HTMLElement;
    for (const rede of ['Facebook', 'Instagram', 'YouTube']) {
      const link = el.querySelector(`a[aria-label="${rede}"]`) as HTMLAnchorElement;
      expect(link).toBeTruthy();
      expect(link.getAttribute('aria-label')).toBe(rede);
    }
  });

  it('mostra o ano corrente no copyright', () => {
    expect(criar().nativeElement.textContent).toContain(String(new Date().getFullYear()));
  });

  it('submeter a newsletter não navega e mostra aviso de indisponível', () => {
    const fixture = criar();
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    const evento = new Event('submit', { cancelable: true });
    form.dispatchEvent(evento);
    fixture.detectChanges();
    expect(evento.defaultPrevented).toBe(true);
    expect(fixture.componentInstance.mensagem()).toContain('Obrigado');
    expect(fixture.componentInstance.mensagem()).toContain('ainda não está disponível');
    expect(fixture.nativeElement.textContent).toContain('ainda não está disponível');
  });
});
