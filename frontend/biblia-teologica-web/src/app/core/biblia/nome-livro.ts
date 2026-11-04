const CONECTIVOS = new Set(['de', 'do', 'da', 'dos', 'das', 'e']);

/** A API devolve os nomes em caixa alta ("ATOS DOS APÓSTOLOS"), e o contrato não muda. */
export function nomeDeExibicao(nome: string): string {
  return nome
    .toLocaleLowerCase('pt-BR')
    .split(' ')
    .map((p, i) =>
      i > 0 && CONECTIVOS.has(p) ? p : p.charAt(0).toLocaleUpperCase('pt-BR') + p.slice(1),
    )
    .join(' ');
}

export function rotuloDeSecao(rotulo: string): string {
  const minusculo = rotulo.toLocaleLowerCase('pt-BR');
  return minusculo.charAt(0).toLocaleUpperCase('pt-BR') + minusculo.slice(1);
}
