import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Title } from '@angular/platform-browser';
import { EntrarComponent } from './entrar.component';
import { AuthStore } from '../../../core/auth/auth.store';

const USUARIO = { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], deveTrocarSenha: false };

describe('EntrarComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.useRealTimers();
  });

  function criar() {
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    return { fixture, el, c: fixture.componentInstance };
  }

  function irParaCodigo(ctx: ReturnType<typeof criar>) {
    ctx.c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    ctx.c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });
    ctx.fixture.detectChanges();
  }

  function enviarCodigo(ctx: ReturnType<typeof criar>, codigo = '123456') {
    const campo = ctx.el.querySelector<HTMLInputElement>('bt-verificar-codigo input')!;
    campo.value = codigo;
    campo.dispatchEvent(new Event('input'));
    ctx.el.querySelector('bt-verificar-codigo form')!.dispatchEvent(new Event('submit'));
    ctx.fixture.detectChanges();
  }

  it('passa da fase de credenciais para a de código', () => {
    const ctx = criar();
    irParaCodigo(ctx);
    expect(ctx.c.fase()).toBe('codigo');
    expect(ctx.el.querySelector('bt-verificar-codigo')).not.toBeNull();
  });

  it('mostra erro quando o login falha', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(c.erro()).toContain('Credenciais');
  });

  it('submeterCredenciais_AvisaBloqueio_QuandoStatus423', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 423, statusText: 'Locked' });
    expect(c.erro()).toBe('Conta bloqueada temporariamente.');
  });

  it('entrar_mostraMensagem_quando503', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 503, statusText: 'Service Unavailable' });
    expect(c.erro()).toBe('Não conseguimos enviar o código agora. Tente de novo em alguns minutos.');
  });

  it('submeterCredenciais_NaoCulpaASenha_QuandoServidorOuRedeFalha', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 500, statusText: 'Erro' });
    expect(c.erro()).toBe('Não foi possível entrar agora. Tente de novo.');

    c.submeterCredenciais();
    http.expectOne('/api/auth/login').error(new ProgressEvent('error'));
    expect(c.erro()).toBe('Não foi possível entrar agora. Tente de novo.');
  });

  it('submeterCredenciais_PedeParaAguardar_QuandoLimiteDeTentativas', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 429, statusText: 'Too Many Requests' });
    expect(c.erro()).toBe('Muitas tentativas. Aguarde um minuto e tente de novo.');
  });

  it('submeterCredenciais_NaoCulpaASenha_QuandoCsrfOuOutroErro', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush(null, { status: 403, statusText: 'Forbidden' });
    expect(c.erro()).toBe('Não foi possível entrar agora. Tente de novo.');
  });

  it('submeterCodigo_PedeParaAguardar_QuandoLimiteDeTentativas', () => {
    const ctx = criar();
    irParaCodigo(ctx);
    enviarCodigo(ctx);
    http.expectOne('/api/auth/login/2fa').flush(null, { status: 429, statusText: 'Too Many Requests' });
    expect(ctx.c.erro()).toBe('Muitas tentativas. Aguarde um minuto e tente de novo.');
  });

  it('voltar_RetornaAsCredenciais_QuandoClicaVoltar', () => {
    const ctx = criar();
    irParaCodigo(ctx);
    enviarCodigo(ctx);
    http.expectOne('/api/auth/login/2fa').flush(null, { status: 400, statusText: 'Bad Request' });
    expect(ctx.c.erro()).toBe('Código inválido ou expirado.');
    ctx.fixture.detectChanges();

    ctx.el.querySelector<HTMLButtonElement>('bt-verificar-codigo .voltar button')!.click();

    expect(ctx.c.fase()).toBe('credenciais');
    expect(ctx.c.erro()).toBeNull();
  });

  it('entrar_voltaACredenciaisComAviso_quandoDesafioVence', () => {
    vi.useFakeTimers();
    const ctx = criar();
    irParaCodigo(ctx);
    vi.advanceTimersByTime(5 * 60 * 1000);
    ctx.fixture.detectChanges();

    expect(ctx.c.fase()).toBe('credenciais');
    expect(ctx.c.erro()).toContain('Entre de novo');
  });

  it('não submete credenciais inválidas', () => {
    const { c } = criar();
    c.formCredenciais.setValue({ email: 'invalido', senha: '' });
    c.submeterCredenciais();
    http.expectNone('/api/auth/login');
  });

  it('entrar_navegaParaRaiz_quandoSemRetorno_mesmoSendoAdmin', () => {
    const ctx = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    irParaCodigo(ctx);
    enviarCodigo(ctx);
    http.expectOne('/api/auth/login/2fa').flush({ usuario: { ...USUARIO, roles: ['Admin'] } });

    expect(TestBed.inject(AuthStore).ehAdmin()).toBe(true);
    expect(nav).toHaveBeenCalledWith('/');
  });

  it('após 2fa com deveTrocarSenha vai para /conta/trocar-senha', () => {
    const ctx = criar();
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    irParaCodigo(ctx);
    enviarCodigo(ctx);
    http.expectOne('/api/auth/login/2fa').flush({ usuario: { ...USUARIO, deveTrocarSenha: true } });

    expect(nav).toHaveBeenCalledWith(['/conta/trocar-senha']);
  });
});

describe('EntrarComponent com retorno', () => {
  const abrir = async (url: string) => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'entrar', component: EntrarComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    const harness = await RouterTestingHarness.create();
    const c = await harness.navigateByUrl(url, EntrarComponent);
    const http = TestBed.inject(HttpTestingController);
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    http.expectOne('/api/auth/login').flush({ desafio2fa: true, tokenDesafio: 'T' });
    harness.detectChanges();
    const el = harness.routeNativeElement as HTMLElement;
    const campo = el.querySelector<HTMLInputElement>('bt-verificar-codigo input')!;
    campo.value = '123456';
    campo.dispatchEvent(new Event('input'));
    el.querySelector('bt-verificar-codigo form')!.dispatchEvent(new Event('submit'));
    http.expectOne('/api/auth/login/2fa').flush({ usuario: USUARIO });
    return nav;
  };

  it('entrar_voltaParaORetorno_quandoCaminhoInterno', async () => {
    const nav = await abrir('/entrar?retorno=%2Flivros%2Fgenesis');
    expect(nav).toHaveBeenCalledWith('/livros/genesis');
  });

  it('entrar_ignoraRetorno_quandoExterno', async () => {
    const nav = await abrir('/entrar?retorno=%2F%2Fevil.com');
    expect(nav).toHaveBeenCalledWith('/');
  });
});

describe('EntrarComponent depois de um cadastro sem código', () => {
  const abrir = async (url: string) => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'entrar', component: EntrarComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl(url, EntrarComponent);
    return harness.routeNativeElement as HTMLElement;
  };

  it('entrar_mostraTextoNeutro_quandoVemDoCadastroVencido', async () => {
    const el = await abrir('/entrar?aviso=cadastro');
    expect(el.querySelector('.aviso')?.textContent).toContain('O tempo do código acabou.');
  });

  it('entrar_naoMostraAviso_quandoValorDesconhecido', async () => {
    const el = await abrir('/entrar?aviso=qualquer%20coisa');
    expect(el.querySelector('.aviso')).toBeNull();
  });
});

describe('EntrarComponent na tela', () => {
  const montar = () => {
    TestBed.configureTestingModule({
      imports: [EntrarComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(EntrarComponent);
    fixture.detectChanges();
    return fixture;
  };

  it('usa os campos próprios, sem Material, e define o título da aba', () => {
    const el = montar().nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Entrar');
    expect(el.querySelectorAll('.bt-input').length).toBe(2);
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.querySelector('bt-botao button[type="submit"]')?.textContent).toContain('Continuar');
    expect(TestBed.inject(Title).getTitle()).toBe('Entrar | Bíblia Teológica');
  });

  it('oferece esqueci a senha e criar conta', () => {
    const el = montar().nativeElement as HTMLElement;
    expect(el.querySelector('a[href="/esqueci-senha"]')?.textContent).toContain('Esqueci minha senha');
    expect(el.querySelector('.abaixo a[href="/registar"]')?.textContent).toContain('Criar conta');
  });

  it('não mostra aviso de conta criada', () => {
    const el = montar().nativeElement as HTMLElement;
    expect(el.querySelector('[role="status"]')).toBeNull();
  });

  it('ao enviar vazio, diz o que falta em cada campo', () => {
    const fixture = montar();
    const el = fixture.nativeElement as HTMLElement;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    const erros = Array.from(el.querySelectorAll('.campo__erro')).map((e) => e.textContent!.trim());
    expect(erros).toEqual(['Preencha este campo.', 'Preencha este campo.']);
  });

  it('avisa em português do Brasil que o código foi enviado', () => {
    const fixture = montar();
    const c = fixture.componentInstance;
    c.formCredenciais.setValue({ email: 'a@b.c', senha: 'x' });
    c.submeterCredenciais();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/auth/login')
      .flush({ desafio2fa: true, tokenDesafio: 'T' });
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Enviamos um código para o seu e-mail.');
  });
});
