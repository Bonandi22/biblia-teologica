import type { Nota } from '../../../core/models/conteudo.model';
import { segmentarNotas } from './texto-notas.component';

/** Notas cujo marcador `[^n]` aparece no texto, na ordem do texto e sem repetir. */
export function notasDoTexto(texto: string, notas: Nota[]): Nota[] {
  const numeros = segmentarNotas(texto).flatMap((s) => ('nota' in s ? [s.nota] : []));
  return [...new Set(numeros)].flatMap((n) => notas.filter((nota) => nota.numero === n));
}
