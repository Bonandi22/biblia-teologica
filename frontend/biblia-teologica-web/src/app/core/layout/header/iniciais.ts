/** Primeira letra do primeiro e do último nome, em maiúscula. Array.from não parte emoji. */
export function iniciais(nome: string): string {
  const palavras = nome.trim().split(/\s+/).filter(Boolean);
  if (palavras.length === 0) return '?';
  const primeira = (p: string) => Array.from(p)[0].toUpperCase();
  if (palavras.length === 1) return primeira(palavras[0]);
  return primeira(palavras[0]) + primeira(palavras[palavras.length - 1]);
}
