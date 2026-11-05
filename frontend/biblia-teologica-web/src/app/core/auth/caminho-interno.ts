/** Devolve o valor só se for um caminho interno (`/x`); recusa `//x`, `/\x` e URLs absolutas. */
export function caminhoInterno(valor: string | null): string | null {
  if (!valor || !valor.startsWith('/')) return null;
  if (valor.startsWith('//') || valor.startsWith('/\\')) return null;
  return valor;
}
