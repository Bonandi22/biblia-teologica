import { iniciais } from './iniciais';

describe('iniciais', () => {
  const casos: [string, string, string][] = [
    ['Maria Souza', 'MS', 'DuasPalavras'],
    ['  maria   da   silva ', 'MS', 'PrimeiroEUltimoComEspacosEMinusculas'],
    ['Ana', 'A', 'UmaPalavra'],
    ['Élida Órfão', 'ÉÓ', 'Acentuadas'],
    ['😀 Ana', '😀A', 'ComEmoji'],
    ['', '?', 'Vazio'],
  ];
  casos.forEach(([nome, esperado, caso]) => {
    it(`iniciais_Devolve${esperado.length}Simbolo_Quando${caso}`, () => {
      expect(iniciais(nome)).toBe(esperado);
    });
  });
});
