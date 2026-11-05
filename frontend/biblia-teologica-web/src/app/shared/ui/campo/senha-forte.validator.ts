import type { ValidatorFn } from '@angular/forms';

/** Mesma política do Identity no servidor: 8 ou mais caracteres, maiúscula, minúscula e dígito (ASCII, como o Identity). */
export function senhaForte(): ValidatorFn {
  return (controle) => {
    const v = String(controle.value ?? '');
    if (!v) return null; // vazio é assunto do Validators.required
    const forte = v.length >= 8 && /[A-Z]/.test(v) && /[a-z]/.test(v) && /\d/.test(v);
    return forte ? null : { senhaForte: true };
  };
}

