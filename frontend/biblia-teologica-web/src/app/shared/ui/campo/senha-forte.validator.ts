import type { ValidatorFn } from '@angular/forms';

/** Mesma política do Identity no servidor: 8 ou mais caracteres, maiúscula, minúscula e dígito. */
export function senhaForte(): ValidatorFn {
  return (controle) => {
    const v = String(controle.value ?? '');
    if (!v) return null; // vazio é assunto do Validators.required
    const forte = v.length >= 8 && /\p{Lu}/u.test(v) && /\p{Ll}/u.test(v) && /\d/.test(v);
    return forte ? null : { senhaForte: true };
  };
}
