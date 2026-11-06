import { agruparReferencias } from './agrupar-referencias';

describe('agruparReferencias', () => {
  it('mostra o livro uma vez quando as referências seguidas são do mesmo livro', () => {
    expect(agruparReferencias(['Isaías 1:1', 'Isaías 7:3', 'Isaías 37:2'])).toEqual([
      [
        { texto: 'Isaías 1:1', rotulo: 'Isaías 1:1' },
        { texto: 'Isaías 7:3', rotulo: '7:3' },
        { texto: 'Isaías 37:2', rotulo: '37:2' },
      ],
    ]);
  });

  it('abre grupo novo quando o livro muda, inclusive com número no nome', () => {
    expect(agruparReferencias(['Romanos 9:33', 'Romanos 10:11', '1 Pedro 2:6'])).toEqual([
      [
        { texto: 'Romanos 9:33', rotulo: 'Romanos 9:33' },
        { texto: 'Romanos 10:11', rotulo: '10:11' },
      ],
      [{ texto: '1 Pedro 2:6', rotulo: '1 Pedro 2:6' }],
    ]);
  });

  it('não junta o mesmo livro quando há outro no meio, para manter a ordem do autor', () => {
    const grupos = agruparReferencias(['2 Reis 18:1', 'Isaías 37:1–7', '2 Reis 20:1']);
    expect(grupos.map((g) => g.map((r) => r.rotulo))).toEqual([
      ['2 Reis 18:1'],
      ['Isaías 37:1–7'],
      ['2 Reis 20:1'],
    ]);
  });

  it('mantém inteira a referência que não segue o formato livro capítulo', () => {
    expect(agruparReferencias(['Isaías', 'Isaías 1:1'])).toEqual([
      [{ texto: 'Isaías', rotulo: 'Isaías' }],
      [{ texto: 'Isaías 1:1', rotulo: 'Isaías 1:1' }],
    ]);
  });

  it('devolve lista vazia sem referências', () => {
    expect(agruparReferencias([])).toEqual([]);
  });
});
