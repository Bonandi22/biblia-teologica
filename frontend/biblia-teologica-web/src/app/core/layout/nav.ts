export interface ItemNav {
  rotulo: string;
  rota: string;
  /** true → só visível quando autenticado. Ponto de extensão para seções reservadas a membros. */
  membros?: boolean;
}

export const NAV_PRINCIPAL: ItemNav[] = [
  { rotulo: 'Livros', rota: '/biblia' },
  { rotulo: 'Estudos', rota: '/estudos' },
];
