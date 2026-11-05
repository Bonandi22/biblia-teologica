import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PerfilComponent } from './perfil.component';
import { AuthStore } from '../../../core/auth/auth.store';

const PERFIL = {
  email: 'a@b.c',
  nomeExibicao: 'Ana',
  roles: ['Membro'],
  criadoEm: '2026-01-01T00:00:00Z',
  telefone: '+5511999990000',
};
const USUARIO = { email: 'a@b.c', nomeExibicao: 'Ana', roles: ['Membro'], deveTrocarSenha: false };
const SEM_CONTEUDO = { status: 204, statusText: 'No Content' };

describe('PerfilComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function criar() {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    return fixture;
  }

  it('perfil_preencheNomeETelefone_quandoCarrega', () => {
    const c = criar().componentInstance;
    expect(c.formPerfil.getRawValue()).toEqual({ nomeExibicao: 'Ana', telefone: '+5511999990000' });
  });

  it('perfil_naoMostraPerfis', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.textContent).not.toContain('Perfis');
    expect(el.textContent).not.toContain('Membro');
    expect(el.textContent).toContain('a@b.c');
  });

  it('perfil_naoTemSair_naTela', () => {
    const el = criar().nativeElement as HTMLElement;
    expect(el.textContent).not.toContain('Sair');
  });

  it('perfil_atualizaAvatar_quandoSalvaNome', () => {
    const fixture = criar();
    const store = TestBed.inject(AuthStore);
    store.definir(USUARIO);
    const c = fixture.componentInstance;
    c.formPerfil.patchValue({ nomeExibicao: 'Ana Maria' });
    c.salvar();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nomeExibicao: 'Ana Maria', telefone: '+5511999990000' });
    req.flush(null, SEM_CONTEUDO);
    expect(store.usuario()?.nomeExibicao).toBe('Ana Maria');
    expect(c.avisoPerfil()).toEqual({ tipo: 'sucesso', texto: 'Dados salvos.' });
  });

  it('perfil_limpaTelefone_quandoCampoVazio', () => {
    const c = criar().componentInstance;
    c.formPerfil.patchValue({ telefone: '   ' });
    c.salvar();
    const req = http.expectOne('/api/conta');
    expect(req.request.body).toEqual({ nomeExibicao: 'Ana', telefone: null });
    req.flush(null, SEM_CONTEUDO);
  });

  it('perfil_mostraMensagemDoServidor_quandoSalvarFalha', () => {
    const c = criar().componentInstance;
    c.salvar();
    http
      .expectOne('/api/conta')
      .flush({ detail: 'Telefone inválido.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.avisoPerfil()).toEqual({ tipo: 'erro', texto: 'Telefone inválido.' });
    expect(c.aSalvar()).toBe(false);
  });

  it('carregar_MostraErroETentaDeNovo_QuandoPerfilFalha', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    http.expectOne('/api/conta').flush(null, { status: 500, statusText: 'Erro' });
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível carregar');
    el.querySelector<HTMLButtonElement>('.tentar button')!.click();
    http.expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(el.querySelector('[role="alert"]')).toBeNull();
    expect(el.textContent).toContain('a@b.c');
  });

  it('perfil_pedeCodigo_depoisDeSolicitarTrocaEmail', () => {
    const fixture = criar();
    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'Segredo1' });
    c.pedirCodigo();
    const req = http.expectOne('/api/conta/email/solicitar');
    expect(req.request.body).toEqual({ novoEmail: 'novo@b.c', senha: 'Segredo1' });
    req.flush(null, SEM_CONTEUDO);
    fixture.detectChanges();
    expect(c.faseEmail()).toBe('codigo');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('novo@b.c');
    expect(c.formEmail.getRawValue().senha).toBe('');
  });

  it('perfil_mostraErro_quandoSenhaErradaAoSolicitar', () => {
    const c = criar().componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'x' });
    c.pedirCodigo();
    http
      .expectOne('/api/conta/email/solicitar')
      .flush({ detail: 'Senha incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.faseEmail()).toBe('dados');
    expect(c.erroEmail()).toBe('Senha incorreta.');
  });

  it('perfil_mostraEmailNovo_quandoConfirmaCodigo', () => {
    const fixture = criar();
    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'Segredo1' });
    c.pedirCodigo();
    http.expectOne('/api/conta/email/solicitar').flush(null, SEM_CONTEUDO);
    c.formCodigo.setValue({ codigo: '123456' });
    c.confirmarCodigo();
    const req = http.expectOne('/api/conta/email/confirmar');
    expect(req.request.body).toEqual({ novoEmail: 'novo@b.c', codigo: '123456' });
    req.flush({ usuario: { ...USUARIO, email: 'novo@b.c' } });
    fixture.detectChanges();
    expect(c.faseEmail()).toBe('dados');
    expect((fixture.nativeElement as HTMLElement).querySelector('.email-atual')?.textContent).toContain(
      'novo@b.c',
    );
    expect(TestBed.inject(AuthStore).usuario()?.email).toBe('novo@b.c');
  });

  it('perfil_mostraErro_quandoCodigoInvalido', () => {
    const fixture = criar();
    const c = fixture.componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'Segredo1' });
    c.pedirCodigo();
    http.expectOne('/api/conta/email/solicitar').flush(null, SEM_CONTEUDO);
    c.formCodigo.setValue({ codigo: '000000' });
    c.confirmarCodigo();
    http
      .expectOne('/api/conta/email/confirmar')
      .flush({ detail: 'Código inválido ou vencido.' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(c.faseEmail()).toBe('codigo');
    expect(c.erroEmail()).toBe('Código inválido ou vencido.');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Código inválido ou vencido.');
  });

  it('perfil_voltaAosDados_quandoCancelaCodigo', () => {
    const c = criar().componentInstance;
    c.formEmail.setValue({ novoEmail: 'novo@b.c', senha: 'Segredo1' });
    c.pedirCodigo();
    http.expectOne('/api/conta/email/solicitar').flush(null, SEM_CONTEUDO);
    c.cancelarEmail();
    expect(c.faseEmail()).toBe('dados');
  });

  it('perfil_pedeConfirmacaoEmTela_antesDeApagar', () => {
    const confirmar = vi.spyOn(window, 'confirm');
    const fixture = criar();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.apagar input')).toBeNull();
    el.querySelector<HTMLButtonElement>('.apagar button')!.click();
    fixture.detectChanges();
    expect(confirmar).not.toHaveBeenCalled();
    expect(fixture.componentInstance.apagando()).toBe(true);
    expect(el.querySelector('.apagar input')).not.toBeNull();
  });

  it('perfil_naoApaga_quandoSenhaVazia', () => {
    const c = criar().componentInstance;
    c.pedirApagar();
    c.apagar();
    http.expectNone((r) => r.method === 'DELETE');
  });

  it('perfil_limpaSessaoENavegaParaRaiz_quandoApagada', () => {
    const c = criar().componentInstance;
    const store = TestBed.inject(AuthStore);
    store.definir(USUARIO);
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    c.pedirApagar();
    c.formApagar.setValue({ senha: 'Segredo1' });
    c.apagar();
    const req = http.expectOne('/api/conta');
    expect(req.request.method).toBe('DELETE');
    expect(req.request.body).toEqual({ senha: 'Segredo1' });
    req.flush(null, SEM_CONTEUDO);
    expect(store.usuario()).toBeNull();
    expect(nav).toHaveBeenCalledWith(['/']);
  });

  it('perfil_explicaMotivo_quandoUltimoAdmin', () => {
    const fixture = criar();
    const c = fixture.componentInstance;
    const store = TestBed.inject(AuthStore);
    store.definir(USUARIO);
    const nav = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    c.pedirApagar();
    c.formApagar.setValue({ senha: 'Segredo1' });
    c.apagar();
    http
      .expectOne('/api/conta')
      .flush({ detail: 'O último administrador não pode apagar a conta.' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();
    expect(c.erroApagar()).toBe('O último administrador não pode apagar a conta.');
    expect(nav).not.toHaveBeenCalled();
    expect(store.usuario()).not.toBeNull();
    expect(c.aApagar()).toBe(false);
  });

  it('perfil_mostraErro_quandoSenhaErradaAoApagar', () => {
    const c = criar().componentInstance;
    c.pedirApagar();
    c.formApagar.setValue({ senha: 'x' });
    c.apagar();
    http.expectOne('/api/conta').flush({ detail: 'Senha incorreta.' }, { status: 400, statusText: 'Bad Request' });
    expect(c.erroApagar()).toBe('Senha incorreta.');
  });
});

describe('PerfilComponent na tela', () => {
  it('usa os componentes próprios, com textos em português do Brasil', () => {
    TestBed.configureTestingModule({
      imports: [PerfilComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    const fixture = TestBed.createComponent(PerfilComponent);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent?.trim()).toBe('Meu perfil');
    expect(el.textContent).toContain('Carregando');
    TestBed.inject(HttpTestingController).expectOne('/api/conta').flush(PERFIL);
    fixture.detectChanges();
    expect(el.querySelector('[class*="mat-"]')).toBeNull();
    expect(el.textContent).toContain('Salvar');
    expect(el.textContent).toContain('Apagar conta');
    expect(el.querySelector('a[href$="trocar-senha"]')?.textContent).toContain('Trocar senha');
  });
});
