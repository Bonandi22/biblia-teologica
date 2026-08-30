import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { RegistarComponent } from './registar.component';

describe('RegistarComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RegistarComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function criar() {
    const fixture = TestBed.createComponent(RegistarComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  const valido = { nomeExibicao: 'Ana', email: 'a@b.c', senha: 'segredo123' };

  it('no sucesso navega para /entrar com aviso de registo', () => {
    const c = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    c.form.setValue(valido);
    c.submeter();
    http.expectOne('/api/auth/registar').flush(null, { status: 201, statusText: 'Created' });
    expect(nav).toHaveBeenCalledWith(['/entrar'], { queryParams: { registado: 1 } });
  });

  it('mostra o detalhe do erro em 400', () => {
    const c = criar();
    c.form.setValue(valido);
    c.submeter();
    http
      .expectOne('/api/auth/registar')
      .flush({ detail: 'Email já registado.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.erro()).toBe('Email já registado.');
  });

  it('não submete formulário inválido', () => {
    const c = criar();
    c.form.setValue({ nomeExibicao: '', email: 'x', senha: '123' });
    c.submeter();
    http.expectNone('/api/auth/registar');
  });
});
