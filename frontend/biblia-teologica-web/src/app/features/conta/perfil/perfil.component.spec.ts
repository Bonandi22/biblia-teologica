import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { PerfilComponent } from './perfil.component';

const PERFIL = {
  email: 'a@b.c',
  nomeExibicao: 'Ana',
  roles: ['Membro'],
  criadoEm: '2026-01-01T00:00:00Z',
};

describe('PerfilComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
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

  it('mostra o email do perfil', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('a@b.c');
  });

  it('pré-preenche o form do nome com o perfil', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(fixture.componentInstance.formNome.getRawValue().nomeExibicao).toBe('Ana');
  });

  it('guardarNome faz PUT e mostra confirmação', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();

    const c = fixture.componentInstance;
    c.formNome.setValue({ nomeExibicao: 'Ana Maria' });
    c.guardarNome();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nomeExibicao: 'Ana Maria' });
    req.flush(null);
    expect(c.msgNome()).toContain('guardado');
  });

  it('guardarEmail mostra o detalhe do erro em falha', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();

    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'x' });
    c.guardarEmail();
    http
      .expectOne('/api/conta/email')
      .flush({ detail: 'Senha incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.msgEmail()).toBe('Senha incorreta.');
  });
});
