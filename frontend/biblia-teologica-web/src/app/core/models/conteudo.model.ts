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

export interface Profecia {
  referencia: string;
  anuncia: string;
  categoria: string;
  cumprimento: string[];
  fatoHistorico: string | null;
  notas: Nota[];
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
  quem: string;
  paraQuem: string;
  porque: string;
  panorama: Bloco;
  contexto: Bloco;
  mundo: Bloco;
  profecias: Profecia[];
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
