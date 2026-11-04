export interface Nota {
  numero: number;
  citacao: string;
  url: string | null;
}

/** Personagem ou acontecimento; `nome` fica vazio em acontecimento. */
export interface Item {
  nome: string;
  texto: string;
  saberMais: string | null;
  estudoSlug: string | null;
  referencias: string[];
}

export interface Acontecimento {
  quando: string;
  item: Item;
}

export interface Bloco {
  explicacao: string;
  personagens: Item[];
  acontecimentos: Acontecimento[];
  notas: Nota[];
}

/** Profecia ou promessa: só aponta o trecho, sem categoria nem indicação de cumprimento. */
export interface Apontamento {
  referencia: string;
  anuncia: string;
  notas: Nota[];
}

export interface PerfilItem {
  rotulo: string;
  texto: string;
  referencias: string[];
}

export interface Citacao {
  trecho: string;
  citadoEm: string[];
  comentario: string | null;
  notas: Nota[];
}

export interface LivroPanorama {
  livroSlug: string;
  resumo: string;
  perfil: PerfilItem[];
  panorama: Bloco;
  contexto: Bloco;
  mundo: Bloco;
  profecias: Apontamento[];
  promessas: Apontamento[];
  citacoes: Citacao[];
}

export interface EstudoResumo {
  slug: string;
  titulo: string;
  resumo: string;
}

export interface Estudo {
  slug: string;
  titulo: string;
  resumo: string;
  corpoMarkdown: string;
  livros: string[];
  relacionados: EstudoResumo[];
  notas: Nota[];
}
