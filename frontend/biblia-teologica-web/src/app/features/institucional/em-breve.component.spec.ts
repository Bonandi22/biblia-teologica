import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { EmBreveComponent } from './em-breve.component';
import { routes } from '../../app.routes';

describe('EmBreveComponent', () => {
  const montar = (data: Record<string, unknown>) => {
    TestBed.configureTestingModule({
      imports: [EmBreveComponent],
      providers: [provideRouter([]), { provide: ActivatedRoute, useValue: { snapshot: { data } } }],
    });
    const fixture = TestBed.createComponent(EmBreveComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('mostra o título e a descrição vindos do data da rota', () => {
    const el = montar({ titulo: 'Estudos', descricao: 'Descrição de teste.' }).nativeElement;
    expect(el.querySelector('h1')?.textContent).toContain('Estudos');
    expect(el.textContent).toContain('Descrição de teste.');
    expect(el.textContent).toContain('Em breve');
  });
});

describe('rotas "Em breve"', () => {
  it('noticias e curiosidades apontam para EmBreveComponent com data', () => {
    for (const [path, titulo] of [
      ['noticias', 'Notícias'],
      ['curiosidades', 'Curiosidades'],
    ]) {
      const rota = routes.find((r) => r.path === path);
      expect(rota?.component).toBe(EmBreveComponent);
      expect((rota?.data as { titulo: string }).titulo).toBe(titulo);
      expect(typeof (rota?.data as { descricao: string }).descricao).toBe('string');
    }
  });
});
