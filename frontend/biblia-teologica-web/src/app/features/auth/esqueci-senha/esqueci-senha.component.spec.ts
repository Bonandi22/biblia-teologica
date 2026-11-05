import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { EsqueciSenhaComponent } from './esqueci-senha.component';

const MENSAGEM = 'Se houver uma conta com esse e-mail, enviamos um link válido por 1 hora.';

describe('EsqueciSenhaComponent', () => {
  let http: HttpTestingController;

  function criar() {
    TestBed.configureTestingModule({
      imports: [EsqueciSenhaComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(EsqueciSenhaComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement, c: fixture.componentInstance };
  }

  afterEach(() => http.verify());

  it('esqueci_mostraMesmaMensagem_quando202', () => {
    const { fixture, el, c } = criar();
    c.form.setValue({ email: 'a@b.c', site: '' });
    c.submeter();
    const req = http.expectOne('/api/auth/senha/esqueci');
    expect(req.request.body).toEqual({ email: 'a@b.c', site: '' });
    req.flush(null, { status: 202, statusText: 'Accepted' });
    fixture.detectChanges();
    expect(el.textContent).toContain(MENSAGEM);
    expect(el.querySelector('form')).toBeNull();
  });

  it('esqueci_mostraEspera_quando429', () => {
    const { fixture, el, c } = criar();
    c.form.setValue({ email: 'a@b.c', site: '' });
    c.submeter();
    http.expectOne('/api/auth/senha/esqueci').flush(null, { status: 429, statusText: 'Too Many' });
    fixture.detectChanges();
    expect(el.textContent).toContain('Muitas tentativas');
    expect(el.textContent).not.toContain(MENSAGEM);
    expect(c.aEnviar()).toBe(false);
  });

  it('esqueci_naoEnvia_quandoEmailInvalido', () => {
    const { c } = criar();
    c.form.setValue({ email: 'x', site: '' });
    c.submeter();
    http.expectNone('/api/auth/senha/esqueci');
  });

  it('esqueci_campoSiteFicaForaDaTabulacao', () => {
    const { el } = criar();
    const site = el.querySelector<HTMLInputElement>('input[name="site"]')!;
    expect(site.getAttribute('tabindex')).toBe('-1');
    expect(site.getAttribute('autocomplete')).toBe('off');
    expect(site.closest('[aria-hidden="true"]')).not.toBeNull();
  });
});
