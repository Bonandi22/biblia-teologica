const BASE = 'https://www.bibliaonline.com.br/arc';

/** Sigla na Bíblia Online e número de capítulos. Siglas conferidas no site em 2026-11-03. */
export const LIVROS_ARC: Record<string, { sigla: string; capitulos: number }> = {
  genesis: { sigla: 'gn', capitulos: 50 },
  exodo: { sigla: 'ex', capitulos: 40 },
  levitico: { sigla: 'lv', capitulos: 27 },
  numeros: { sigla: 'nm', capitulos: 36 },
  deuteronomio: { sigla: 'dt', capitulos: 34 },
  josue: { sigla: 'js', capitulos: 24 },
  juizes: { sigla: 'jz', capitulos: 21 },
  rute: { sigla: 'rt', capitulos: 4 },
  '1-samuel': { sigla: '1sm', capitulos: 31 },
  '2-samuel': { sigla: '2sm', capitulos: 24 },
  '1-reis': { sigla: '1rs', capitulos: 22 },
  '2-reis': { sigla: '2rs', capitulos: 25 },
  '1-cronicas': { sigla: '1cr', capitulos: 29 },
  '2-cronicas': { sigla: '2cr', capitulos: 36 },
  esdras: { sigla: 'ed', capitulos: 10 },
  neemias: { sigla: 'ne', capitulos: 13 },
  ester: { sigla: 'et', capitulos: 10 },
  jo: { sigla: 'job', capitulos: 42 },
  salmos: { sigla: 'sl', capitulos: 150 },
  proverbios: { sigla: 'pv', capitulos: 31 },
  eclesiastes: { sigla: 'ec', capitulos: 12 },
  'cantares-de-salomao': { sigla: 'ct', capitulos: 8 },
  isaias: { sigla: 'is', capitulos: 66 },
  jeremias: { sigla: 'jr', capitulos: 52 },
  lamentacoes: { sigla: 'lm', capitulos: 5 },
  ezequiel: { sigla: 'ez', capitulos: 48 },
  daniel: { sigla: 'dn', capitulos: 12 },
  oseias: { sigla: 'os', capitulos: 14 },
  joel: { sigla: 'jl', capitulos: 3 },
  amos: { sigla: 'am', capitulos: 9 },
  obadias: { sigla: 'ob', capitulos: 1 },
  jonas: { sigla: 'jn', capitulos: 4 },
  miqueias: { sigla: 'mq', capitulos: 7 },
  naum: { sigla: 'na', capitulos: 3 },
  habacuque: { sigla: 'hc', capitulos: 3 },
  sofonias: { sigla: 'sf', capitulos: 3 },
  ageu: { sigla: 'ag', capitulos: 2 },
  zacarias: { sigla: 'zc', capitulos: 14 },
  malaquias: { sigla: 'ml', capitulos: 4 },
  mateus: { sigla: 'mt', capitulos: 28 },
  marcos: { sigla: 'mc', capitulos: 16 },
  lucas: { sigla: 'lc', capitulos: 24 },
  joao: { sigla: 'jo', capitulos: 21 },
  'atos-dos-apostolos': { sigla: 'atos', capitulos: 28 },
  romanos: { sigla: 'rm', capitulos: 16 },
  '1-corintios': { sigla: '1co', capitulos: 16 },
  '2-corintios': { sigla: '2co', capitulos: 13 },
  galatas: { sigla: 'gl', capitulos: 6 },
  efesios: { sigla: 'ef', capitulos: 6 },
  filipenses: { sigla: 'fp', capitulos: 4 },
  colossenses: { sigla: 'cl', capitulos: 4 },
  '1-tessalonicenses': { sigla: '1ts', capitulos: 5 },
  '2-tessalonicenses': { sigla: '2ts', capitulos: 3 },
  '1-timoteo': { sigla: '1tm', capitulos: 6 },
  '2-timoteo': { sigla: '2tm', capitulos: 4 },
  tito: { sigla: 'tt', capitulos: 3 },
  filemom: { sigla: 'fm', capitulos: 1 },
  hebreus: { sigla: 'hb', capitulos: 13 },
  tiago: { sigla: 'tg', capitulos: 5 },
  '1-pedro': { sigla: '1pe', capitulos: 5 },
  '2-pedro': { sigla: '2pe', capitulos: 3 },
  '1-joao': { sigla: '1jo', capitulos: 5 },
  '2-joao': { sigla: '2jo', capitulos: 1 },
  '3-joao': { sigla: '3jo', capitulos: 1 },
  judas: { sigla: 'jd', capitulos: 1 },
  apocalipse: { sigla: 'ap', capitulos: 22 },
};

// Espelha ReferenciaBiblica.cs: mesmo formato e mesmos apelidos.
const APELIDOS: Record<string, string> = {
  cantares: 'cantares-de-salomao',
  salmo: 'salmos',
  atos: 'atos-dos-apostolos',
};
const FORMATO = /^(.+?)\s+([1-9][0-9]*)(?::([1-9][0-9]*))?(?:\s?[–-]\s?(?:([1-9][0-9]*):)?([1-9][0-9]*))?$/;

function slugDoNome(nome: string): string {
  const slug = nome
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
    .replace(/\s+/g, '-');
  return APELIDOS[slug] ?? slug;
}

export function slugDaReferencia(ref: string): string | null {
  const m = FORMATO.exec(ref.trim());
  if (!m) return null;
  const slug = slugDoNome(m[1]);
  return slug in LIVROS_ARC ? slug : null;
}

export function urlCapitulo(slug: string, capitulo: number): string | null {
  const livro = LIVROS_ARC[slug];
  if (!livro || capitulo < 1 || capitulo > livro.capitulos) return null;
  return `${BASE}/${livro.sigla}/${capitulo}`;
}

/** URL da referência na ARC; `null` quando não pode ser lida, para nunca gerar link quebrado. */
export function urlReferencia(ref: string): string | null {
  const slug = slugDaReferencia(ref);
  const m = FORMATO.exec(ref.trim());
  if (!slug || !m) return null;

  const capitulo = Number(m[2]);
  const inicio = urlCapitulo(slug, capitulo);
  if (!inicio) return null;

  const versiculo = m[3] ? Number(m[3]) : null;
  const capituloFinal = m[4] ? Number(m[4]) : null;
  const fim = m[5] ? Number(m[5]) : null;

  if (versiculo === null) {
    if (capituloFinal !== null || (fim !== null && fim <= capitulo)) return null;
    return inicio; // "Isaías 53" ou "Isaías 36–37": abre o primeiro capítulo.
  }
  if (fim === null) return `${inicio}/${versiculo}`;
  if (capituloFinal !== null && capituloFinal !== capitulo) {
    return capituloFinal > capitulo ? `${inicio}/${versiculo}` : null;
  }
  return fim >= versiculo ? `${inicio}/${versiculo}-${fim}` : null;
}
