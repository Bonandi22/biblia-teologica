import { nomeDeExibicao, rotuloDeSecao } from './nome-livro';

describe('nomeDeExibicao', () => {
  it('passa o nome da API para caixa mista, preservando acentos', () => {
    expect(nomeDeExibicao('ISAÍAS')).toBe('Isaías');
    expect(nomeDeExibicao('ÊXODO')).toBe('Êxodo');
    expect(nomeDeExibicao('JÓ')).toBe('Jó');
  });

  it('mantém o número dos livros numerados', () => {
    expect(nomeDeExibicao('1 REIS')).toBe('1 Reis');
    expect(nomeDeExibicao('2 CORÍNTIOS')).toBe('2 Coríntios');
  });

  it('deixa conectivos em minúscula', () => {
    expect(nomeDeExibicao('CANTARES DE SALOMÃO')).toBe('Cantares de Salomão');
    expect(nomeDeExibicao('ATOS DOS APÓSTOLOS')).toBe('Atos dos Apóstolos');
  });

  it('não altera um nome que já está em caixa mista', () => {
    expect(nomeDeExibicao('Isaías')).toBe('Isaías');
  });
});

describe('rotuloDeSecao', () => {
  it('passa o rótulo da API para maiúscula só na primeira letra', () => {
    expect(rotuloDeSecao('PROFETAS MAIORES')).toBe('Profetas maiores');
    expect(rotuloDeSecao('LIVROS HISTÓRICOS')).toBe('Livros históricos');
    expect(rotuloDeSecao('Evangelhos')).toBe('Evangelhos');
  });
});
