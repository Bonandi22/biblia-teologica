import { AuthStore } from './auth.store';

describe('AuthStore', () => {
  it('computeds refletem o utilizador', () => {
    const s = new AuthStore();
    expect(s.estaAutenticado()).toBe(false);
    s.definir({ email: 'a@b.c', nomeExibicao: 'A', roles: ['Membro'], deveTrocarSenha: true });
    expect(s.estaAutenticado()).toBe(true);
    expect(s.ehAdmin()).toBe(false);
    expect(s.deveTrocarSenha()).toBe(true);
    s.limpar();
    expect(s.usuario()).toBeNull();
  });
});
