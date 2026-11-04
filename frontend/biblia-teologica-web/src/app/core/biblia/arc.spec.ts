import { LIVROS_ARC, slugDaReferencia, urlCapitulo, urlReferencia } from './arc';

const BASE = 'https://www.bibliaonline.com.br/arc';

describe('arc', () => {
  it('tem os 66 livros, somando 1.189 capítulos', () => {
    expect(Object.keys(LIVROS_ARC)).toHaveLength(66);
    expect(Object.values(LIVROS_ARC).reduce((s, l) => s + l.capitulos, 0)).toBe(1189);
  });

  it('gera URL de versículo, intervalo e capítulo', () => {
    expect(urlReferencia('Isaías 53:7')).toBe(`${BASE}/is/53/7`);
    expect(urlReferencia('Isaías 6:1–8')).toBe(`${BASE}/is/6/1-8`);
    expect(urlReferencia('Isaías 6:1-8')).toBe(`${BASE}/is/6/1-8`);
    expect(urlReferencia('Isaías 53')).toBe(`${BASE}/is/53`);
  });

  it('abre o início quando o intervalo atravessa capítulos', () => {
    expect(urlReferencia('Isaías 52:13–53:12')).toBe(`${BASE}/is/52/13`);
    expect(urlReferencia('Isaías 36–37')).toBe(`${BASE}/is/36`);
  });

  it('reconhece apelidos e livros numerados', () => {
    expect(urlReferencia('Atos 8:32')).toBe(`${BASE}/atos/8/32`);
    expect(urlReferencia('Atos dos Apóstolos 8:32')).toBe(`${BASE}/atos/8/32`);
    expect(urlReferencia('Salmo 23')).toBe(`${BASE}/sl/23`);
    expect(urlReferencia('Cantares 2:1')).toBe(`${BASE}/ct/2/1`);
    expect(urlReferencia('1 Pedro 2:22–25')).toBe(`${BASE}/1pe/2/22-25`);
    expect(urlReferencia('Jó 1:1')).toBe(`${BASE}/job/1/1`);
    expect(urlReferencia('João 1:23')).toBe(`${BASE}/jo/1/23`);
  });

  it('devolve null quando a referência não pode ser lida', () => {
    expect(urlReferencia('Isaías')).toBeNull();
    expect(urlReferencia('Isaías 67')).toBeNull();
    expect(urlReferencia('Isaías 0')).toBeNull();
    expect(urlReferencia('Isaías 8:3–1')).toBeNull();
    expect(urlReferencia('Livro Inexistente 1:1')).toBeNull();
    expect(urlReferencia('Eis o meu servo')).toBeNull();
    expect(urlReferencia('')).toBeNull();
  });

  it('gera URL de capítulo só dentro do livro', () => {
    expect(urlCapitulo('isaias', 66)).toBe(`${BASE}/is/66`);
    expect(urlCapitulo('isaias', 67)).toBeNull();
    expect(urlCapitulo('isaias', 0)).toBeNull();
    expect(urlCapitulo('inexistente', 1)).toBeNull();
  });

  it('identifica o livro da referência', () => {
    expect(slugDaReferencia('1 Coríntios 13:4')).toBe('1-corintios');
    expect(slugDaReferencia('Atos 8:32')).toBe('atos-dos-apostolos');
    expect(slugDaReferencia('Nada aqui')).toBeNull();
  });
});
