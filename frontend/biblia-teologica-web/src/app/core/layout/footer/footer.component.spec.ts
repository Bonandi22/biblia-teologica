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
    return fixture.nativeElement as HTMLElement;
  };

  it('tem links para Livros e Estudos, sem Notícias nem Curiosidades', () => {
    const el = criar();
    expect(el.querySelector('a[href="/biblia"]')?.textContent?.trim()).toBe('Livros');
    expect(el.querySelector('a[href="/estudos"]')?.textContent?.trim()).toBe('Estudos');
    expect(el.querySelector('a[href="/noticias"]')).toBeNull();
    expect(el.querySelector('a[href="/curiosidades"]')).toBeNull();
  });

  it('não tem formulário de newsletter nem redes sociais', () => {
    const el = criar();
    expect(el.querySelector('form')).toBeNull();
    for (const rede of ['Facebook', 'Instagram', 'YouTube']) {
      expect(el.textContent).not.toContain(rede);
    }
  });

  it('explica para onde as referências bíblicas levam', () => {
    expect(criar().textContent).toContain(
      'As referências bíblicas abrem a tradução Almeida Revista e Corrigida na Bíblia Online.',
    );
  });

  it('rodape_linkaPrivacidade', () => {
    const el = criar();
    expect(el.querySelector('a[href="/privacidade"]')?.textContent?.trim()).toBe('Privacidade');
  });

  it('mostra o ano corrente', () => {
    expect(criar().textContent).toContain(String(new Date().getFullYear()));
  });
});
