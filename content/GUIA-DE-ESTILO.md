# Guia de estilo do conteúdo

Lista de conferência para todo texto de `livros/` e `estudos/`. Um texto não está pronto
se falhar em qualquer item. Origem: seções 3, 3.1 e 4.3 de
`docs/superpowers/specs/2026-11-02-panorama-biblico-design.md`, com as mudanças de
`2026-11-03-repaginacao-biblia-de-estudo-design.md` (perfil, profecias e promessas
sem categoria, citações em qualquer livro).

## 1. Padrão de qualidade

- [ ] **Específico.** Cada parágrafo diz algo que seria falso ou sem sentido se aplicado a
  outro livro. Tem nomes, lugares, épocas e referências.
- [ ] **Útil para a leitura.** Cada fato histórico responde "o que isso muda na
  compreensão do texto?". Fato solto não entra.
- [ ] **Conferível.** O que vem da Bíblia tem referência. O que vem de fora tem nota
  `[^n]` com obra da `bibliografia.yaml` e local (página, capítulo ou linha).
- [ ] **Claro.** Quem não estudou teologia entende. Termo difícil é explicado na primeira
  vez; o aprofundamento técnico fica no Estudo completo.
- [ ] **Sem vícios de IA.** Ver a seção 5.
- [ ] **Datas aproximadas.** "Por volta de 701 a.C.", nunca precisão que as fontes não dão.
  O campo `ano` só ordena e não aparece na tela.
- [ ] **Números e datas divergentes viram faixa** (decisão do utilizador, 2026-11-06).
  Quando as fontes conferidas dão valores diferentes, o texto usa "cerca de 52 a 55
  anos" ou "por volta de 740 a 735 a.C." e cita as referências que sustentam cada ponta.

## 2. Bibliografia e citações

- [ ] Só entra na `bibliografia.yaml` obra que o autor do texto realmente consultou.
  Autor, título, edição e existência conferidos antes da revisão final.
- [ ] Toda citação identifica a origem: autor, obra e, quando houver, edição e página ou
  passagem. Citação bíblica indica livro, capítulo, versículo e tradução (ARC).
- [ ] Toda citação de registro antigo (assírio, babilônico, persa, Josefo etc.) foi
  conferida na obra de referência listada. O texto não atribui a um registro o que ele
  não diz. O que não puder ser conferido não entra.
- [ ] Nada é aceito só porque uma IA o apresentou. Obra, página e citação sugeridas por IA
  são conferidas na fonte antes de entrar.
- [ ] `contexto`, `mundo` e todo estudo têm ao menos uma nota.
- [ ] **Wikipedia e Perplexity são ferramentas de busca, não fontes** (decisão do
  utilizador, 2026-11-06). Elas servem para achar a fonte original que citam. Essa fonte
  entra na bibliografia só se estiver aberta ao público, se for aberta no endereço
  registrado em `url` e se disser o que o texto afirma. Fonte fechada ou que não se
  consegue abrir não entra.
- [ ] Preferir edições de referência abertas: textos antigos em tradução acadêmica
  (Oracc/RINAP, Livius.org, British Museum, Perseus) e a ARC na Bíblia Online.

## 3. Neutralidade e voz

- [ ] **Profecias e promessas** são só apontadas, na ordem do livro: referência e o que o
  trecho anuncia, com as palavras do próprio texto. Sem categoria, sem "cumprida",
  "a se cumprir", "aponta para Cristo" ou indicação de quando e como se cumpre.
- [ ] Nenhum rótulo ou título decide uma interpretação em que as teologias divergem.
  Quando a divergência importa para a leitura, o texto mostra o trecho e, se preciso,
  atribui cada posição à sua fonte da bibliografia.
- [ ] **Citações** mostram onde o livro é citado em outro livro (inclusive do Antigo
  Testamento). O `comentario`, quando existe, descreve como o texto que cita usa o
  trecho, com as palavras dele, sem acrescentar leitura própria. Citações não levam nota:
  cada referência de `citadoEm` abre o texto na ARC e já é a fonte (utilizador,
  2026-11-06).
- [ ] Autoria e data: a posição tradicional é a do texto. Quando são discutidas, a
  posição crítica aparece em uma frase, atribuída a quem a defende.

## 4. Salvaguardas contra contradições

- [ ] O site nunca afirma como fato algo que contradiga a Bíblia. O texto principal segue
  o relato bíblico.
- [ ] O que vem de fora é atribuído à fonte ("os anais de Senaqueribe dizem..."), deixando
  claro que é a versão daquele povo.
- [ ] Nos Estudos completos, divergências aparecem de forma breve em "Outras leituras",
  sem debate.
- [ ] Nenhum fato é ajustado para parecer que combina com a Bíblia. A nota reflete o que a
  fonte realmente diz.

## 5. Vícios a cortar

| Vício | Ruim | Melhor |
|---|---|---|
| Frase que serve a qualquer livro | "Isaías é um livro rico e profundo, cheio de ensinamentos." | "Isaías profetizou em Jerusalém sob quatro reis de Judá, de Uzias a Ezequias (Isaías 1:1)." |
| Fórmula de transição | "Além disso, vale ressaltar que..." | Começar direto pelo fato. |
| Lista de adjetivos | "Um período turbulento, complexo e decisivo." | Dizer o que aconteceu: quem atacou quem, quando. |
| Conclusão vazia | "Assim, vemos a importância deste contexto." | Cortar, ou dizer qual versículo fica mais claro. |
| Fato solto | "A Assíria tinha um exército numeroso." | Ligar ao texto: o que isso explica em Isaías 36–37. |
| Interpretação embutida | "Isaías 7:14, cumprida no nascimento de Jesus." | "Isaías 7:14" + o que o versículo anuncia, nas palavras dele. |

Os exemplos bons e ruins encontrados na revisão de Isaías entram aqui (seção 6).

Regras de escrita em português (de "Como fazer o Claude escrever como humano",
mestredoclaude.com.br):

- Sem conectivos de redação: "além disso", "em suma", "por conseguinte", "vale
  ressaltar", "nesse sentido", "no mundo de hoje".
- Começar direto no assunto, nunca por uma frase que serviria a qualquer livro.
- Variar o tamanho das frases e dos parágrafos. Simetria demais denuncia.
- Lista só quando o conteúdo é mesmo uma lista; o resto é prosa.
- Sem fechamento que recapitula e sem entusiasmo vazio ("incrível", "poderoso",
  "riquíssimo").
- Exemplo concreto (nome, lugar, versículo) no lugar de adjetivo.
- O tom se calibra com textos reais do autor do site, não com adjetivos. Os
  exemplos de referência ficam na seção 6.

Lista completa de padrões para a passada final: o método e os 26 padrões da skill
[humanizer](https://github.com/blader/humanizer) (MIT), baseada em "Signs of AI
writing" da Wikipedia. Os que mais aparecem neste conteúdo: preparação antes do ponto
("A missão é dura: ..."), frase curta de efeito ("O tom muda."), voz passiva sem
sujeito ("são lidos à luz de"), texto que fala de si mesmo ("prepara a leitura") e
fecho de uma linha que repete o já dito. Na reescrita, nenhum fato, número, data ou
citação é acrescentado ou retirado.

## 6. Exemplos da revisão de Isaías

Casos reais do rascunho de Isaías (2026-11-06 e 2026-11-07): o trecho original, o
problema e a versão que ficou.

### 6.1 Interpretar em vez de mostrar (contexto, Acaz)

- Antes: "Saber disso ajuda a ler o capítulo 7 como um conflito entre confiar no Senhor
  e comprar a proteção da Assíria."
- Problema: diz ao leitor como ler o capítulo; é uma leitura, não o texto (seção 3).
- Depois: "2 Reis conta que Acaz se declarou "teu servo e teu filho" a
  Tiglate-Pileser, rei da Assíria, pediu socorro contra os mesmos dois reis e mandou-lhe
  a prata e o ouro da casa do Senhor e do palácio."
- Regra: dar o fato com a fonte e deixar a conclusão com o leitor.

### 6.2 Paráfrase que fecha o sentido (profecias, Isaías 53)

- Antes: "Foi ferido pelas transgressões de outros, …"
- Problema: "de outros" decide quem são os transgressores; o texto diz "nossas".
- Depois: ""Foi ferido pelas nossas transgressões", e "pelas suas pisaduras, fomos
  sarados"."
- Regra: onde as tradições divergem, citar a ARC entre aspas em vez de parafrasear.

### 6.3 Frase de função em vez de conteúdo (perfil, o chamado)

- Antes: "O capítulo 6 explica por que a pregação dele encontrará resistência."
- Problema: anuncia o que o capítulo faz, mas não diz o que ele diz.
- Depois: "Isaías tem a visão do trono e é enviado a um povo que vai ouvir e não
  entender, até que as cidades fiquem assoladas."

### 6.4 Promessa sem a condição (promessas, Isaías 58)

- Antes: ""O Senhor te guiará continuamente", e quem o segue será como um jardim
  regado …"
- Problema: some a condição do versículo anterior e entra um "quem o segue" que o texto
  não tem.
- Depois: "Se o povo abrir a alma ao faminto e fartar a alma aflita, "o Senhor te
  guiará continuamente", e será "como um jardim regado" …"
- Regra: promessa condicional leva a condição junto.

### 6.5 Frase emendada que esconde a fonte (mundo, Merodaque-Baladã)

- Antes: "A Babilônia aparece primeiro como reino rebelde contra a Assíria, com
  Merodaque-Baladã, e depois como o império para onde iriam os tesouros …"
- Problema: mistura o que diz a crônica com o que diz Isaías, sem dizer quem afirma o
  quê.
- Depois: "Merodaque-Baladã, que a crônica babilônica mostra lutando contra Sargão e
  Senaqueribe,[^1] manda enviados a Ezequias, e Isaías anuncia …"
- Regra: cada afirmação com o seu dono (o texto bíblico ou a fonte numerada).

### 6.6 Número que parece certeza (perfil, Uzias)

- Antes: "Reinou cerca de 52 a 55 anos em Jerusalém …"
- Problema: o texto bíblico dá um número só (2 Reis 15:2: 52 anos); a faixa sugeria uma
  divergência de fontes que o conteúdo não mostrava.
- Depois: "Reinou 52 anos em Jerusalém …"
- Regra: faixa aproximada só quando as fontes de fato divergem, com a referência de cada
  ponta (seção 2).

### 6.7 Nota longa

- Antes: `local: "Col. iii, linhas 38–75 (terceira campanha, trecho sobre Ezequias)"`;
  notas da ARC com explicação.
- Problema: a nota repete o que o texto já disse.
- Depois: `local: "col. iii, linhas 38–75"`; nota da ARC só com a referência e o link.
- Regra: nota localiza, não comenta.
