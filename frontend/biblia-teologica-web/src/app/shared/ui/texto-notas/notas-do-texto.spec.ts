import { notasDoTexto } from './notas-do-texto';

const nota = (numero: number) => ({ numero, citacao: `Obra ${numero}`, url: null });

describe('notasDoTexto', () => {
  it('devolve as notas na ordem em que aparecem no texto', () => {
    const r = notasDoTexto('A.[^2] B.[^1]', [nota(1), nota(2)]);
    expect(r.map((n) => n.numero)).toEqual([2, 1]);
  });

  it('não repete a nota chamada duas vezes', () => {
    expect(notasDoTexto('A.[^1] B.[^1]', [nota(1)])).toHaveLength(1);
  });

  it('ignora marcador sem nota', () => {
    expect(notasDoTexto('A.[^9]', [nota(1)])).toEqual([]);
  });

  it('devolve vazio para texto sem marcador', () => {
    expect(notasDoTexto('Sem marcador.', [nota(1)])).toEqual([]);
  });
});
