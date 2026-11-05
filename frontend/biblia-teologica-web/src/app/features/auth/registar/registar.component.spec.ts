import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute, convertToParamMap } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RegistarComponent } from './registar.component';

const USUARIO = { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], deveTrocarSenha: false };

describe('RegistarComponent', () => {
  let http: HttpTestingController;

  function preparar(retorno: string | null = null) {
    TestBed.configureTestingModule({
      imports: [RegistarComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap(retorno ? { retorno } : {}) } },
        },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  }

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  function criar() {
    const fixture = TestBed.createComponent(RegistarComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement, c: fixture.componentInstance };
  }

  const valido = {
    nomeExibicao: 'Ana',
    email: 'a@b.c',
    telefone: '',
    senha: 'Segredo123',
    aceitouTermo: true,
    site: '',
  };

  function enviarValido(ctx: ReturnType<typeof criar>, extra: Partial<typeof valido> = {}) {
    ctx.c.form.setValue({ ...valido, ...extra });
    ctx.c.submeter();
  }

  function irParaCodigo(ctx: ReturnType<typeof criar>) {
    enviarValido(ctx);
    http.expectOne('/api/auth/registar').flush({ tokenDesafio: 'T' }, { status: 202, statusText: 'Accepted' });
    ctx.fixture.detectChanges();
  }

  it('registar_naoEnvia_quandoTermoDesmarcado', () => {
    preparar();
    const ctx = criar();
    const caixa = ctx.el.querySelector<HTMLInputElement>('input[type="checkbox"]')!;
    expect(caixa.checked).toBe(false);
    expect(ctx.c.form.controls.aceitouTermo.value).toBe(false);
    enviarValido(ctx, { aceitouTermo: false });
    ctx.fixture.detectChanges();
    http.expectNone('/api/auth/registar');
    expect(ctx.el.querySelector('.termo__erro')?.textContent).toContain('Aceite o termo');
  });

  it('registar_enviaTelefoneNulo_quandoVazio', () => {
    preparar();
    const ctx = criar();
    enviarValido(ctx);
    const req = http.expectOne('/api/auth/registar');
    expect(req.request.body).toEqual({
      nomeExibicao: 'Ana',
      email: 'a@b.c',
      senha: 'Segredo123',
      aceitouTermo: true,
      telefone: null,
      site: '',
    });
  });

  it('registar_mostraTelaDeCodigo_quando202', () => {
    preparar();
    const ctx = criar();
    irParaCodigo(ctx);
    expect(ctx.el.querySelector('bt-verificar-codigo')).not.toBeNull();
    expect(ctx.el.querySelector('bt-campo-senha')).toBeNull();
  });

  it('registar_abreSessaoENavega_quandoCodigoCerto', () => {
    preparar('/estudos');
    const ctx = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    irParaCodigo(ctx);
    const campo = ctx.el.querySelector<HTMLInputElement>('bt-verificar-codigo input')!;
    campo.value = '123456';
    campo.dispatchEvent(new Event('input'));
    ctx.el.querySelector('bt-verificar-codigo form')!.dispatchEvent(new Event('submit'));
    const req = http.expectOne('/api/auth/login/2fa');
    expect(req.request.body).toEqual({ tokenDesafio: 'T', codigo: '123456' });
    req.flush({ usuario: USUARIO });
    expect(nav).toHaveBeenCalledWith('/estudos');
  });

  it('registar_voltaParaInicio_quandoNaoHaRetorno', () => {
    preparar();
    const ctx = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    irParaCodigo(ctx);
    ctx.c.submeterCodigo('123456');
    http.expectOne('/api/auth/login/2fa').flush({ usuario: USUARIO });
    expect(nav).toHaveBeenCalledWith('/');
  });

  it('registar_levaAEntrarComTextoNeutro_quandoDesafioVence', () => {
    vi.useFakeTimers();
    preparar('/estudos');
    const ctx = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    irParaCodigo(ctx);
    vi.advanceTimersByTime(5 * 60 * 1000 + 1);
    expect(nav).toHaveBeenCalledTimes(1);
    const [cmds, extras] = nav.mock.calls[0] as [string[], { queryParams: Record<string, string> }];
    expect(cmds).toEqual(['/entrar']);
    expect(extras.queryParams['retorno']).toBe('/estudos');
    expect(extras.queryParams['aviso']).toBe('cadastro');
  });

  it('registar_mostraErroDoServidor_quando400', () => {
    preparar();
    const ctx = criar();
    enviarValido(ctx);
    http
      .expectOne('/api/auth/registar')
      .flush({ detail: 'Senha fraca.' }, { status: 400, statusText: 'Bad Request' });
    expect(ctx.c.erro()).toBe('Senha fraca.');
    expect(ctx.c.aEnviar()).toBe(false);
  });

  it('registar_naoMostraDetalheInterno_quando500', () => {
    preparar();
    const ctx = criar();
    enviarValido(ctx);
    http
      .expectOne('/api/auth/registar')
      .flush({ detail: 'stack interna' }, { status: 500, statusText: 'Server Error' });
    expect(ctx.c.erro()).toBe('Não foi possível criar a conta agora. Tente de novo.');
  });

  it('registar_recusaSenhaFraca_semChamarOServidor', () => {
    preparar();
    const ctx = criar();
    enviarValido(ctx, { senha: 'segredo123' });
    http.expectNone('/api/auth/registar');
  });

  it('registar_campoSiteFicaForaDaTabulacao', () => {
    preparar();
    const { el } = criar();
    const site = el.querySelector<HTMLInputElement>('input[name="site"]')!;
    expect(site.getAttribute('tabindex')).toBe('-1');
    expect(site.getAttribute('autocomplete')).toBe('off');
    expect(site.closest('[aria-hidden="true"]')).not.toBeNull();
  });

  it('registar_linkDoTermoAbreEmNovaAba', () => {
    preparar();
    const { el } = criar();
    const a = el.querySelector<HTMLAnchorElement>('a[href="/privacidade"]')!;
    expect(a.getAttribute('target')).toBe('_blank');
    expect(a.getAttribute('rel')).toContain('noopener');
  });

  it('registar_usaOsCamposProprios_eDizOQueFaltaAoEnviarVazio', () => {
    preparar();
    const { fixture, el } = criar();
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.textContent).toContain('Pelo menos 8 caracteres, com maiúscula, minúscula e número.');
    expect(el.textContent).toContain('Li e aceito o termo de proteção de dados e autorizo guardar os meus dados');
    expect(el.querySelector('a[href="/entrar"]')?.textContent).toContain('Entrar');
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(el.querySelectorAll('.campo__erro').length).toBe(3);
  });
});
