# Repaginação do frontend: Bíblia de Estudo (design)

**Data:** 2026-11-03
**Issue:** #30
**Branch:** `feature/BT-30-repaginacao-biblia-de-estudo`, a partir de `feature/BT-28-panorama-isaias`
**Estado:** aprovada pelo utilizador em 2026-11-03, incluindo as propostas da seção 11. Isaías é o livro padrão; os demais só avançam depois da aprovação do padrão.
**Contexto de produto:** `PRODUCT.md`

Substitui o desenho "Mapa de capítulos" com Angular Material, registrado no fim de `.superpowers/sdd/2026-11-02-panorama-biblico/progress.md`.

## 1. Objetivo

Dar ao site inteiro um único mundo visual, a Bíblia de Estudo, e implementar as funções já decididas para o Panorama que ainda não existem no código. A pessoa deve entender onde está no cânon, ler com conforto e conferir cada afirmação na hora.

O resultado não pode parecer feito por IA nem template, frio, religioso caricato, denso ou difícil de ler.

## 2. Decisões do utilizador

- Visual do zero, no mundo Bíblia de Estudo, em todas as telas.
- Sem Angular Material. As 4 telas de conta passam a usar os componentes próprios.
- Mostrar só o que existe. Notícias, Curiosidades, conteúdo para membros e newsletter saem da navegação, da página inicial e do rodapé. As rotas `/noticias` e `/curiosidades` continuam.
- Textos da interface em português do Brasil. As URLs não mudam.
- As funções vêm antes do visual.

## 3. Etapa de funções

O Panorama ainda não foi para a `main` e não há conteúdo real, então o formato pode mudar sem migração de conteúdo nem plano de versão. `/api/livros` e `/api/livros/{slug}` não mudam.

### 3.1 Formato do conteúdo e API

| Item | Hoje | Depois |
|---|---|---|
| Perfil do autor | `quem`, `paraQuem`, `porque` | `perfil`: lista de `{ rotulo, texto, referencias }` |
| Profecias | `referencia`, `anuncia`, `categoria`, `cumprimento`, `fatoHistorico`, `notas` | `referencia`, `anuncia`, `notas` |
| Promessas | não existem | `promessas`, com a mesma forma de profecias |
| Citações | exigem referência do Novo Testamento | `citadoEm` aceita qualquer livro, exceto o próprio |

- `perfil` é uma lista com rótulos livres porque o perfil de um profeta não serve para um rei ou um apóstolo. Exige ao menos um item, com `rotulo` e `texto`.
- Profecias e promessas aparecem na ordem do arquivo, e o autor as mantém na ordem do livro.
- Saem a lista de categorias e as regras ligadas a elas no validador.
- Mudam juntos: `Registros.cs`, `ValidadorConteudo.cs`, `ConteudoDtos.cs`, `ConstrutorDocumentos.cs`, os testes, `conteudo.model.ts` e `formato.md`.

### 3.2 Abas

Panorama (perfil do autor e personagens), Contexto (situação em que o livro foi escrito), Mundo (o que acontecia ao redor), Profecias, Promessas e Citações. A rota nova é `/biblia/<slug>/promessas`. O selo de categoria sai da tela.

### 3.3 Links para a ARC

- Toda referência bíblica vira link para a ARC na Bíblia Online, em nova aba, com `rel="noopener noreferrer"`.
- Formato já usado no modelo anterior: `https://www.bibliaonline.com.br/arc/<sigla>/<capítulo>[/<versículo>[-<fim>]]`. **Conferir as 66 siglas e o formato antes de publicar**, abrindo uma amostra em cada grupo do cânon.
- A API não informa o número de capítulos. Uma tabela no frontend guarda sigla e capítulos dos 66 livros, com teste que confere a soma de 1.189 capítulos.
- Referência que não pode ser lida aparece como texto simples, sem link quebrado.
- O uso público da ARC continua dependendo da confirmação escrita da SBB.

## 4. Mundo visual

### 4.1 Tokens

| Papel | Valor | Uso |
|---|---|---|
| Papel | `#FBFBF8` | Fundo de leitura; levemente frio, não creme |
| Tinta | `#16161A` | Texto e títulos |
| Rubrica | `#9E1B1B` | Só o que leva a uma fonte e a posição atual |
| Filete | `#C9C8C0` | Divisões que carregam informação |
| Capa | `#2A3550` | Tecido da capa: rodapé e guarda da página inicial |
| Texto sobre capa | `#E9E7DD` | Conferir contraste AA |

A rubrica tem um significado: vermelho indica referência, nota ou lugar atual. Ela não serve de enfeite.

Tipos: **Gelasio** para títulos e texto, **Alegreya Sans SC** para títulos correntes e navegação. Sai Space Grotesk, Source Serif 4, IBM Plex Mono e Material Icons. Versalete em caixa mista substitui rótulos em caixa alta. Corpo com 65 a 75 caracteres por linha e entrelinha perto de 1,6.

Não entram: cartões, sombras, gradientes, cruzes, pergaminho, douração ou texturas envelhecidas.

### 4.2 Assinatura: coluna de referências

Como na coluna central de uma Bíblia de estudo, as referências acompanham o texto. Cada parágrafo ou item é uma linha de grade, com as próprias referências e notas à direita, na mesma altura. Passar o mouse ou focar uma chamada destaca a linha correspondente, e o inverso também. É o único efeito do site, sem animação de entrada.

No celular a coluna se recolhe: as referências aparecem ao fim de cada bloco, na mesma ordem.

### 4.3 Moldura comum

- **Título corrente:** navegação à esquerda, lugar atual ao centro (como "Isaías 1–66"), marca e Entrar à direita, com filete de tinta abaixo.
- **Rodapé:** campo de capa com marca, links para Livros e Estudos, e a nota sobre a tradução ARC.

## 5. Telas

### 5.1 Página do livro

```
Livros  Estudos          Isaías 1–66          Bíblia Teológica  Entrar
────────────────────────────────────────────────────────────────────────
Profetas maiores   Isaías                             
 Isaías            frase de resumo em rubrica          
 Jeremias          1 2 3 4 … 66 (capítulos na ARC)
 Lamentações       Panorama Contexto Mundo Profecias Promessas Citações
 Ezequiel          ───────────────────────────────    Referências
 Daniel            Texto em coluna de leitura ¹        1 Isaías 1:1
                   continua com a chamada seguinte ²   2 Isaías 6:1–8
```

Corrige o layout anterior: as margens ficam proporcionais e a coluna esquerda mostra só os livros da divisão, não os 66. No celular, a lista da divisão se recolhe e as abas rolam com indicação visual de que há mais.

### 5.2 Página inicial

Abre como o índice de uma Bíblia, sem herói nem cartões: "Bíblia Teológica", uma frase sobre o que o site faz e, logo abaixo, os 66 livros em duas colunas, Antigo e Novo Testamento, agrupados por divisão. Livros com panorama recebem marca em rubrica. Depois vêm os estudos publicados, e a guarda em cor de capa fecha a página.

### 5.3 Livros (`/biblia`) e ficha

- **Livros:** o mesmo índice, com busca. Filtrar não muda a ordem do cânon.
- **Ficha:** usa o mesmo layout da página do livro, sem abas, e mostra os campos que a API já entrega.

### 5.4 Estudos

- **Lista:** títulos e resumos em sequência, sem cartões.
- **Estudo:** coluna de leitura longa, notas na coluna de referências e bibliografia no fim.

### 5.5 Conta e institucional

Entrar, registrar, 2FA, perfil e trocar senha em uma coluna estreita, com `bt-botao`, `bt-campo` e `bt-campo-senha`. Mantêm toda a validação, os estados de erro e o fluxo atuais. As páginas "em breve" recebem o mesmo tratamento e seguem acessíveis pela URL.

## 6. Estados e acessibilidade

- Carregando, erro, vazio e 404 em cada tela, com texto que diz o que aconteceu e o que fazer.
- WCAG AA: contraste, foco visível, teclado, alvos de toque de pelo menos 24px e `prefers-reduced-motion`.
- Seleção de texto, sublinhado, barras de rolagem e numerais tematizados pela paleta.
- Toda página de topo tem fundo e altura mínima próprios e passa por conferência visual.

## 7. Etapas de entrega

1. Funções: formato, validação, API, abas, selos e ARC.
2. Fundação: tokens, fontes, base global, primitivos, título corrente e rodapé.
3. Página do livro e coluna de referências.
4. Página inicial, Livros e ficha.
5. Estudos.
6. Conta e institucional; remoção do Angular Material; revisão dos textos em pt-BR.
7. Revisão final de design, `DESIGN.md` e revisão de código da branch.

Depois da etapa 3, o utilizador confere a página do livro antes de o visual ser replicado nas demais telas.

## 8. Testes

- **Backend:** validador, construtor e endpoints, com caminho feliz e rejeição para as novas regras.
- **Frontend (Vitest):** abas, promessas, ausência de selos, links e tabela da ARC, recolhimento no celular e formulários sem Material.
- **Visual:** capturas em 375px, 1366px e 1920px por tela, em todos os estados.
- **Manual (utilizador):** stack em Docker com o conteúdo fictício do projeto de teste `biblia-teste`.

## 9. Fora do escopo

Leitor de capítulos, linha do tempo, painel de administração, conteúdo real de Isaías (Task 10) e demais livros. A marca "onde você parou" era do Atlas e também fica de fora.

## 10. Riscos

- **Parecer o visual editorial genérico:** papel claro, serifa e vermelho é uma combinação comum em interfaces geradas por IA. O contraponto é a coluna de referências, a rubrica com função e a cor de capa, e a revisão final verifica isso.
- **Links errados da ARC:** tabela conferida e referências ilegíveis sem link.
- **Coluna alinhada em conteúdo longo:** o alinhamento é por parágrafo ou item, não por chamada. Assim as referências nunca se sobrepõem, mesmo com chamadas próximas.

## 11. Pontos conferidos no código

Os dois pontos levantados na revisão se mostraram diferentes do que a primeira versão desta spec dizia:

1. "Como Cristo aparece no livro" não é um campo da API nem da ficha. É só uma frase na página inicial atual, e sai com a reescrita da página (5.2).
2. "Génesis" está escrito à mão em `home.component.ts`, e não nos dados dos livros. Sai com a mesma reescrita. `livros.json` não muda.

A marca de "livros com panorama" (5.2) exige um endpoint novo de leitura, `GET /api/panoramas/livros`, que devolve os slugs publicados. Ele é público, como os demais de leitura, e leva comentário e testes.
