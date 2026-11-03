import type { UrlMatchResult, UrlSegment } from '@angular/router';

/**
 * Casa `:slug` e `:slug/:aba` na MESMA rota. Duas rotas separadas recriariam o componente
 * a cada troca de aba e refariam a chamada HTTP do mesmo livro.
 */
export function livroMatcher(segmentos: UrlSegment[]): UrlMatchResult | null {
  if (segmentos.length < 1 || segmentos.length > 2) return null;
  const posParams: Record<string, UrlSegment> = { slug: segmentos[0] };
  if (segmentos.length === 2) posParams['aba'] = segmentos[1];
  return { consumed: segmentos, posParams };
}
