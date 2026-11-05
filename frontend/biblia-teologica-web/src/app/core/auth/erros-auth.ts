import { HttpErrorResponse } from '@angular/common/http';

export const MUITAS_TENTATIVAS = 'Muitas tentativas. Aguarde um minuto e tente de novo.';

/** Só 400, 409 e 423 trazem texto escrito por nós; qualquer outro status (500, rede) usa o padrão. */
const STATUS_COM_DETAIL = new Set([400, 409, 423]);

export function mensagemDeErro(e: HttpErrorResponse, padrao: string): string {
  if (e.status === 429) return MUITAS_TENTATIVAS;
  if (STATUS_COM_DETAIL.has(e.status)) {
    const detail = (e.error as { detail?: unknown } | null)?.detail;
    if (typeof detail === 'string' && detail.trim()) return detail;
  }
  return padrao;
}
