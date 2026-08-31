export interface ItemNav {
  rotulo: string;
  rota: string;
  /** true → só visível quando autenticado. Ponto de extensão para secções reservadas a membros. */
  membros?: boolean;
}

export const NAV_PRINCIPAL: ItemNav[] = [
  { rotulo: 'Início', rota: '/' },
  { rotulo: 'Fichário', rota: '/biblia' },
  { rotulo: 'Estudos', rota: '/estudos' },
  { rotulo: 'Notícias', rota: '/noticias' },
  { rotulo: 'Curiosidades', rota: '/curiosidades' },
];
