/** Uma referência da coluna: `texto` é a referência completa (link e leitor de tela); `rotulo` é o que aparece. */
export interface ReferenciaRotulada {
  texto: string;
  rotulo: string;
}

// "1 Pedro 2:6" -> livro "1 Pedro", resto "2:6". O livro termina no espaço antes do capítulo.
const LIVRO_E_RESTO = /^(.+?)\s+(\d[\d:–\-;, ]*)$/;

/**
 * Agrupa referências seguidas do mesmo livro, para o nome do livro aparecer uma vez só
 * ("Isaías 1:1; 7:3; 37:2"). A ordem do autor é mantida: o livro só se junta ao vizinho.
 */
export function agruparReferencias(referencias: string[]): ReferenciaRotulada[][] {
  const grupos: ReferenciaRotulada[][] = [];
  let livroAnterior: string | null = null;

  for (const texto of referencias) {
    const m = LIVRO_E_RESTO.exec(texto.trim());
    const livro = m?.[1] ?? null;
    const atual = grupos.at(-1);
    if (livro !== null && livro === livroAnterior && atual) {
      atual.push({ texto, rotulo: m![2] });
    } else {
      grupos.push([{ texto, rotulo: texto }]);
    }
    livroAnterior = livro;
  }
  return grupos;
}
