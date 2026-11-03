import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Title } from '@angular/platform-browser';
import { LugarAtualService } from '../../../core/layout/lugar-atual.service';
import { EstudosListaComponent } from './estudos-lista.component';

describe('EstudosListaComponent', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  const el = () => harness.routeNativeElement as HTMLElement;

  const abrir = async () => {
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/estudos');
    harness.detectChanges();
  };
  const responder = async (corpo: object | null, status = 200) => {
    const req = http.expectOne('/api/estudos');
    if (status === 200) req.flush(corpo);
    else req.flush(null, { status, statusText: 'erro' });
    harness.detectChanges();
    await harness.fixture.whenStable();
    harness.detectChanges();
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'estudos', component: EstudosListaComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lista título e resumo de cada estudo, com link para o estudo', async () => {
    await abrir();
    await responder([
      { slug: 'servo-sofredor', titulo: 'O servo sofredor', resumo: 'Quem é o servo de Isaías 53.' },
      { slug: 'remanescente', titulo: 'O remanescente', resumo: 'Um resto que volta.' },
    ]);
    const texto = el().textContent!;
    expect(texto).toContain('O servo sofredor');
    expect(texto).toContain('Quem é o servo de Isaías 53.');
    expect(texto).toContain('O remanescente');
    expect(el().querySelector('a[href="/estudos/servo-sofredor"]')).not.toBeNull();
  });

  it('diz que ainda não há estudos e leva a Livros quando a lista vem vazia', async () => {
    await abrir();
    await responder([]);
    expect(el().textContent).toContain('Ainda não há estudos publicados.');
    expect(el().querySelector('a[href="/biblia"]')?.textContent).toContain('Ver os livros');
  });

  it('define o título da aba e o lugar atual, e limpa o lugar ao sair', async () => {
    await abrir();
    await responder([]);
    expect(TestBed.inject(Title).getTitle()).toBe('Estudos | Bíblia Teológica');
    const lugar = TestBed.inject(LugarAtualService);
    expect(lugar.texto()).toBe('Estudos');
    harness.fixture.destroy();
    expect(lugar.texto()).toBe('');
  });

  it('mostra erro com nova tentativa quando a requisição falha', async () => {
    await abrir();
    await responder(null, 500);
    expect(el().querySelector('[role="alert"]')).not.toBeNull();
    (el().querySelector('button') as HTMLButtonElement).click();
    http.expectOne('/api/estudos');
  });
});
