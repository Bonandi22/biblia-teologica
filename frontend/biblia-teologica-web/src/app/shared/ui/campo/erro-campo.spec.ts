import { FormControl, Validators } from '@angular/forms';
import { erroCampo } from './erro-campo';

describe('erroCampo', () => {
  const tocado = (c: FormControl) => {
    c.markAsTouched();
    return c;
  };

  it('não mostra erro antes de o campo ser tocado', () => {
    expect(erroCampo(new FormControl('', Validators.required))).toBeNull();
  });

  it('não mostra erro em campo válido', () => {
    expect(erroCampo(tocado(new FormControl('x', Validators.required)))).toBeNull();
  });

  it('nomeia o problema e a correção', () => {
    expect(erroCampo(tocado(new FormControl('', Validators.required)))).toBe('Preencha este campo.');
    expect(erroCampo(tocado(new FormControl('x', Validators.email)))).toBe(
      'Digite um e-mail válido, como nome@exemplo.com.',
    );
    expect(erroCampo(tocado(new FormControl('abc', Validators.minLength(8))))).toBe(
      'Use pelo menos 8 caracteres.',
    );
    expect(erroCampo(tocado(new FormControl('abcd', Validators.maxLength(3))))).toBe(
      'Use no máximo 3 caracteres.',
    );
  });

  it('aceita controle ausente', () => {
    expect(erroCampo(null)).toBeNull();
  });
});
