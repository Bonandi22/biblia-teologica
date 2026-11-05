import type { AbstractControl } from '@angular/forms';

/** Mensagem do campo depois de tocado: diz o problema e como resolver. */
export function erroCampo(controle: AbstractControl | null): string | null {
  if (!controle || !controle.touched || !controle.errors) return null;
  const e = controle.errors;
  if (e['required']) return 'Preencha este campo.';
  if (e['email']) return 'Digite um e-mail válido, como nome@exemplo.com.';
  if (e['minlength']) return `Use pelo menos ${e['minlength'].requiredLength} caracteres.`;
  if (e['maxlength']) return `Use no máximo ${e['maxlength'].requiredLength} caracteres.`;
  if (e['senhaForte']) return 'Use pelo menos 8 caracteres, com maiúscula, minúscula e número.';
  return 'Confira este campo.';
}
