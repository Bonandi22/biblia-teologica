import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PrivacidadeComponent } from './privacidade.component';
import { RESPONSAVEL_DADOS, VERSAO_TERMO } from './responsavel-dados';

describe('PrivacidadeComponent', () => {
  const criar = () => {
    TestBed.configureTestingModule({
      imports: [PrivacidadeComponent],
      providers: [provideRouter([])],
    });
    const fixture = TestBed.createComponent(PrivacidadeComponent);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('privacidade_mostraResponsavelEContato', () => {
    const el = criar();
    expect(el.querySelector('h1')?.textContent).toContain('Termo e privacidade');
    expect(el.textContent).toContain(RESPONSAVEL_DADOS.nome);
    const mail = el.querySelector(`a[href="mailto:${RESPONSAVEL_DADOS.email}"]`);
    expect(mail?.textContent).toContain(RESPONSAVEL_DADOS.email);
  });

  it('privacidade_listaDadosEDireitos', () => {
    const texto = criar().textContent ?? '';
    for (const trecho of [
      'nome',
      'e-mail',
      'telefone',
      'Gmail',
      '7 dias',
      'apagamento',
      'autoridade de controle',
      'Meu perfil',
      'Apagar conta',
    ]) {
      expect(texto).toContain(trecho);
    }
  });

  it('privacidade_mostraVersaoEData', () => {
    expect(VERSAO_TERMO).toBe('2026-11-05');
    const texto = criar().textContent;
    expect(texto).toContain('Versão 2026-11-05');
    expect(texto).toContain('5 de novembro de 2026');
  });

  it('privacidade_temFundoEAlturaMinima', () => {
    const estilo = getComputedStyle(criar());
    expect(estilo.minHeight).not.toBe('');
    expect(estilo.minHeight).not.toBe('0px');
    expect(estilo.backgroundColor).not.toBe('');
  });
});
