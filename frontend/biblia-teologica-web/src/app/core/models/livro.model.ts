export interface LivroListItem {
  slug: string;
  nome: string;
  ordem: number;
  secao: string;
  secaoRotulo: string;
}

export interface LivroDetail extends LivroListItem {
  testamento: string;
  quem: string;
  porque: string;
  paraQuem: string;
  estrutura: string;
  tema: string;
  ideias: string[];
  versiculo: string;
}
