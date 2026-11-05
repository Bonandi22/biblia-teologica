import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { VerificarCodigoComponent } from './verificar-codigo.component';

describe('VerificarCodigoComponent', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [VerificarCodigoComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  function montar(cadastro = false) {
    const fixture = TestBed.createComponent(VerificarCodigoComponent);
    fixture.componentRef.setInput('tokenDesafio', 'T');
    fixture.componentRef.setInput('cadastro', cadastro);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    const vencido = vi.fn();
    fixture.componentInstance.vencido.subscribe(vencido);
    const reenviar = () => el.querySelector<HTMLButtonElement>('.reenviar button')!;
    return { fixture, el, vencido, reenviar };
  }

  it('verificarCodigo_explicaContaExistente_quandoCadastro', () => {
    const { el } = montar(true);
    const nota = el.querySelector('.conta-existente')!;
    expect(nota.textContent).toContain('Se este e-mail já tiver conta, nenhum código será enviado');
    const links = Array.from(nota.querySelectorAll('a')).map((a) => a.getAttribute('href'));
    expect(links).toEqual(['/entrar', '/esqueci-senha']);
  });

  it('verificarCodigo_naoFalaDeContaExistente_quandoLogin', () => {
    const { el } = montar(false);
    expect(el.querySelector('.conta-existente')).toBeNull();
  });

  it('verificarCodigo_reenviaEMostraAviso_quandoClicaReenviar', () => {
    const { fixture, el, reenviar } = montar();
    reenviar().click();
    fixture.detectChanges();
    expect(reenviar().disabled).toBe(true);

    const req = http.expectOne('/api/auth/login/2fa/reenviar');
    expect(req.request.body).toEqual({ tokenDesafio: 'T' });
    req.flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(reenviar().disabled).toBe(false);
    expect(el.querySelector('[role="status"]')?.textContent).toContain('tente de novo');
  });

  it('verificarCodigo_emiteVencido_depoisDe5Minutos', () => {
    vi.useFakeTimers();
    const { vencido } = montar();
    vi.advanceTimersByTime(5 * 60 * 1000 - 1);
    expect(vencido).not.toHaveBeenCalled();
    vi.advanceTimersByTime(1);
    expect(vencido).toHaveBeenCalledTimes(1);
  });

  it('verificarCodigo_emiteVencido_quandoReenvioResponde400', () => {
    const { vencido, reenviar } = montar();
    reenviar().click();
    http.expectOne('/api/auth/login/2fa/reenviar').flush(null, { status: 400, statusText: 'Bad Request' });
    expect(vencido).toHaveBeenCalledTimes(1);
  });

  it('verificarCodigo_mostraEspera_quandoReenvioResponde429', () => {
    const { fixture, el, vencido, reenviar } = montar();
    reenviar().click();
    http.expectOne('/api/auth/login/2fa/reenviar').flush(null, { status: 429, statusText: 'Too Many Requests' });
    fixture.detectChanges();
    expect(vencido).not.toHaveBeenCalled();
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('Aguarde');
  });

  it('verificarCodigo_emiteEnviar_quandoCodigoPreenchido', () => {
    const { fixture, el } = montar();
    const enviar = vi.fn();
    fixture.componentInstance.enviar.subscribe(enviar);
    const campo = el.querySelector<HTMLInputElement>('input')!;
    campo.value = '123456';
    campo.dispatchEvent(new Event('input'));
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    expect(enviar).toHaveBeenCalledWith('123456');
  });

  it('verificarCodigo_naoEmiteEnviar_quandoVazio', () => {
    const { fixture, el } = montar();
    const enviar = vi.fn();
    fixture.componentInstance.enviar.subscribe(enviar);
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    expect(enviar).not.toHaveBeenCalled();
  });
});
