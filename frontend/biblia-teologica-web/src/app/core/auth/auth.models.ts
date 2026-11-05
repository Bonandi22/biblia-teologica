export interface Usuario {
  email: string;
  nomeExibicao: string;
  roles: string[];
  deveTrocarSenha: boolean;
}

export interface NovoCadastro {
  nomeExibicao: string;
  email: string;
  senha: string;
  aceitouTermo: boolean;
  telefone: string | null;
  /** Campo-isca: pessoas deixam vazio, robôs preenchem. */
  site: string;
}

export interface DesafioLogin {
  desafio2fa: true;
  tokenDesafio: string;
}
