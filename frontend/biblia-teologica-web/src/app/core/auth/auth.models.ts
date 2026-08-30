export interface Usuario {
  email: string;
  nomeExibicao: string;
  roles: string[];
  deveTrocarSenha: boolean;
}

export interface DesafioLogin {
  desafio2fa: true;
  tokenDesafio: string;
}
