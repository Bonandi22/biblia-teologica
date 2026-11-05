import { HttpErrorResponse } from '@angular/common/http';
import { mensagemDeErro } from './erros-auth';

const erro = (status: number, detail?: string) =>
  new HttpErrorResponse({ status, error: detail ? { detail } : null });

describe('mensagemDeErro', () => {
  it('mensagemDeErro_naoExpoeDetail_quando500', () => {
    expect(mensagemDeErro(erro(500, 'System.Exception: stack'), 'Falhou.')).toBe('Falhou.');
  });

  it('mensagemDeErro_usaDetail_quando400409Ou423', () => {
    for (const s of [400, 409, 423]) {
      expect(mensagemDeErro(erro(s, 'Texto nosso.'), 'Falhou.')).toBe('Texto nosso.');
    }
  });

  it('mensagemDeErro_usaPadrao_quando400SemDetail', () => {
    expect(mensagemDeErro(erro(400), 'Falhou.')).toBe('Falhou.');
  });

  it('mensagemDeErro_temTextoProprio_quando429', () => {
    expect(mensagemDeErro(erro(429, 'x'), 'Falhou.')).toBe(
      'Muitas tentativas. Aguarde um minuto e tente de novo.',
    );
  });
});
