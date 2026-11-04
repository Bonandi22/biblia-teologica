import DOMPurify from 'dompurify';
import { Marked } from 'marked';

/** Só https: e o âncora exato de nota (`#nota-<dígitos>`); nada de javascript:, data:, vbscript: ou http:. */
const URI_PERMITIDA = /^(?:https:|#nota-\d+$)/i;

const purify = DOMPurify(window);
purify.addHook('afterSanitizeAttributes', (no) => {
  if (no.nodeName === 'A' && no.getAttribute('href')?.toLowerCase().startsWith('https:')) {
    no.setAttribute('rel', 'noopener noreferrer');
    no.setAttribute('target', '_blank');
  }
});

const marked = new Marked({
  async: false,
  extensions: [
    {
      name: 'nota',
      level: 'inline',
      start: (src: string) => src.indexOf('[^'),
      tokenizer(src: string) {
        const m = /^\[\^(\d+)\]/.exec(src);
        return m ? { type: 'nota', raw: m[0], numero: m[1] } : undefined;
      },
      renderer: (token) =>
        `<sup><a data-nota="${token['numero']}" href="#nota-${token['numero']}" aria-label="Nota ${token['numero']}">${token['numero']}</a></sup>`,
    },
  ],
});

/** Converte Markdown em HTML já sanitizado (allowlist fechada). Marcadores `[^n]` viram links de nota. */
export function renderizarMarkdown(md: string): string {
  const bruto = marked.parse(md ?? '', { async: false });
  return purify.sanitize(bruto, {
    ALLOWED_TAGS: ['p', 'h2', 'h3', 'h4', 'ul', 'ol', 'li', 'strong', 'em', 'blockquote', 'a', 'sup', 'code', 'pre', 'hr'],
    ALLOWED_ATTR: ['href', 'rel', 'target', 'data-nota', 'aria-label'],
    ALLOWED_URI_REGEXP: URI_PERMITIDA,
  });
}

/** Blocos de topo do texto, para cada um ser uma linha de grade com as próprias notas. */
export function blocosMarkdown(md: string): string[] {
  const tokens = marked.lexer(md ?? '');
  const definicoes = tokens.filter((t) => t.type === 'def').map((t) => t.raw.trim());
  return tokens
    .filter((t) => t.type !== 'space' && t.type !== 'def')
    .map((t) => [t.raw.trim(), ...definicoes].join('\n\n'));
}
