import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RedefinirSenhaComponent } from './redefinir-senha.component';

describe('RedefinirSenhaComponent', () => {
  let http: HttpTestingController;

  function criar(fragmento: string | null) {
    TestBed.configureTestingModule({
      imports: [RedefinirSenhaComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: fragmento } } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(RedefinirSenhaComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement, c: fixture.componentInstance };
  }

  afterEach(() => {
    http.verify();
    vi.restoreAllMocks();
  });

  it('redefinir_mostraLinkInvalido_quandoSemFragmento', () => {
    const { el } = criar(null);
    expect(el.textContent).toContain('Este link é inválido ou venceu.');
    expect(el.querySelector('form')).toBeNull();
    expect(el.querySelector('a[href="/esqueci-senha"]')).not.toBeNull();
  });

  it('redefinir_mostraLinkInvalido_quandoFragmentoMalformado', () => {
    for (const f of ['email=a%40b.c', 'token=abc', 'email=a%40b.c&token=%E0%A4%A', '']) {
      TestBed.resetTestingModule();
      const { el } = criar(f);
      expect(el.textContent).toContain('Este link é inválido ou venceu.');
      expect(el.querySelector('form')).toBeNull();
    }
  });

  it('redefinir_limpaFragmentoDaBarra_aoCarregar', () => {
    const espiao = vi.spyOn(history, 'replaceState');
    criar('email=a%40b.c&token=T');
    expect(espiao).toHaveBeenCalledTimes(1);
    expect(String(espiao.mock.calls[0][2])).not.toContain('#');
  });

  it('redefinir_navegaParaEntrarComAviso_quandoSucesso', () => {
    const { c } = criar('email=a%40b.c&token=T%2Bx');
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    c.form.setValue({ senha: 'Segredo123' });
    c.submeter();
    const req = http.expectOne('/api/auth/senha/redefinir');
    expect(req.request.body).toEqual({ email: 'a@b.c', token: 'T+x', novaSenha: 'Segredo123' });
    req.flush(null, { status: 204, statusText: 'No Content' });
    expect(nav).toHaveBeenCalledWith(['/entrar'], { queryParams: { aviso: 'senha' } });
  });

  it('redefinir_mostraInvalidoComLinkParaPedirOutro_quando400DeToken', () => {
    const { fixture, el, c } = criar('email=a%40b.c&token=T');
    c.form.setValue({ senha: 'Segredo123' });
    c.submeter();
    http
      .expectOne('/api/auth/senha/redefinir')
      .flush({ detail: 'Este link é inválido ou venceu.' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(el.textContent).toContain('Este link é inválido ou venceu.');
    expect(el.querySelector('form')).toBeNull();
    expect(el.querySelector('a[href="/esqueci-senha"]')).not.toBeNull();
  });

  it('redefinir_mantemFormulario_quando400DeSenhaFraca', () => {
    const { fixture, el, c } = criar('email=a%40b.c&token=T');
    c.form.setValue({ senha: 'Segredo123' });
    c.submeter();
    http
      .expectOne('/api/auth/senha/redefinir')
      .flush({ detail: 'A senha não atende à regra.' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(el.querySelector('form')).not.toBeNull();
    expect(el.textContent).toContain('A senha não atende à regra.');
  });

  it('redefinir_naoEnvia_quandoSenhaFraca', () => {
    const { c } = criar('email=a%40b.c&token=T');
    c.form.setValue({ senha: 'segredo123' });
    c.submeter();
    http.expectNone('/api/auth/senha/redefinir');
  });
});
