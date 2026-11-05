import { caminhoInterno } from './caminho-interno';

describe('caminhoInterno', () => {
  it('caminhoInterno_aceita_quandoCaminhoComUmaBarra', () => {
    expect(caminhoInterno('/livros/genesis?x=1')).toBe('/livros/genesis?x=1');
  });

  it('caminhoInterno_recusa_quandoDuasBarrasOuUrlAbsoluta', () => {
    for (const v of ['//evil.com', '/\\evil.com', 'https://evil.com', 'javascript:alert(1)', 'livros', '', null]) {
      expect(caminhoInterno(v)).toBeNull();
    }
  });
});
