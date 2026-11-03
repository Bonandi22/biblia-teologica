---
version: 1
slug: "frontend-biblia-teologica-web-src-app"
primary_target: "frontend/biblia-teologica-web/src/app"
related_targets: []
---

Escopo: todo o frontend (início, livros, panorama, estudos, conta, institucional). Modo: Read.
Público: quem estuda a Bíblia, ao lado da leitura, em computador e celular.
Evitar: cara de IA ou template, frio, religioso caricato, denso.
Decisões: sem Angular Material; só mostrar o que existe; textos em pt-BR; rotas e contratos de API não mudam.

## Direction contract

THESIS: A página do site é uma Bíblia de estudo aberta: texto no centro e cada afirmação com sua referência ao lado. Recusa a página de portal com herói, cartões e seções empilhadas.

OWN-WORLD: Papel-bíblia frio #FBFBF8, tinta #16161A, rubrica #9E1B1B só para o que leva a uma fonte e para a posição atual, filete #C9C8C0, capa em tecido azul #2A3550 em guarda e rodapé. Gelasio para título e texto; Alegreya Sans SC para títulos correntes e navegação. Sem cartões, sombras, cantos arredondados decorativos ou ornamentos religiosos.

STORY: Quem lê entende de imediato onde está no cânon, lê o livro situado e confere cada afirmação na hora, abrindo a ARC em nova aba.

FIRST VIEWPORT: Na página do livro, título corrente no topo com navegação à esquerda, livro e capítulos ao centro, marca e Entrar à direita. Três colunas: à esquerda os livros da divisão; ao centro o nome do livro em 112px, frase em rubrica, abas e texto com 65 a 75 caracteres; à direita a coluna de referências, com linha alinhada à altura da chamada. A ação principal é ler e conferir.

FORM: Bíblia de Estudo, candidata 1 da lista, escolhida pelo utilizador. seed:e458d5a7c39d7caa. Assinatura: coluna de referências sincronizada com o texto; passar ou focar uma chamada destaca a linha correspondente, e no celular as referências ficam ao fim de cada bloco.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
