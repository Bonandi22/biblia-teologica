---
name: Bíblia Teológica
description: Portal de estudo bíblico composto como uma Bíblia de estudo aberta, com cada afirmação conferível ao lado do texto.
colors:
  papel: "#fbfbf8"
  tinta: "#16161a"
  tinta-mut: "#55555c"
  rubrica: "#9e1b1b"
  rubrica-escura: "#7c1414"
  filete: "#c9c8c0"
  capa: "#2a3550"
  sobre-capa: "#e9e7dd"
  erro: "#7a2d12"
  borda-controlo: "#6f6f76"
  papel-realce: "#f1f1ec"
  sobre-tinta: "#ffffff"
  tinta-funda: "#000000"
typography:
  display-livro:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "clamp(3.25rem, 8vw, 7rem)"
    fontWeight: 700
    lineHeight: 1
    letterSpacing: "-0.02em"
  display-pagina:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "clamp(2.75rem, 6vw, 6rem)"
    fontWeight: 700
    lineHeight: 1
    letterSpacing: "-0.02em"
  display-apoio:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "clamp(2.25rem, 4vw, 4rem)"
    fontWeight: 700
    lineHeight: 1.05
    letterSpacing: "-0.02em"
  headline:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "clamp(1.6rem, 3vw, 2.1rem)"
    fontWeight: 700
    lineHeight: 1.15
  title:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "1.375rem"
    fontWeight: 700
    lineHeight: 1.25
  lead:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "1.25rem"
    fontWeight: 400
    lineHeight: 1.45
  body:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "1.1875rem"
    fontWeight: 400
    lineHeight: 1.6
  body-celular:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "1.0625rem"
    fontWeight: 400
    lineHeight: 1.6
  nota:
    fontFamily: "Gelasio, Georgia, 'Times New Roman', serif"
    fontSize: "0.9375rem"
    fontWeight: 400
    lineHeight: 1.5
  corrente:
    fontFamily: "'Alegreya Sans SC', 'Gill Sans', 'Trebuchet MS', sans-serif"
    fontSize: "1.0625rem"
    fontWeight: 500
  corrente-rotulo:
    fontFamily: "'Alegreya Sans SC', 'Gill Sans', 'Trebuchet MS', sans-serif"
    fontSize: "1.0625rem"
    fontWeight: 700
    lineHeight: 1.4
  corrente-aba:
    fontFamily: "'Alegreya Sans SC', 'Gill Sans', 'Trebuchet MS', sans-serif"
    fontSize: "1.125rem"
    fontWeight: 500
rounded:
  raio: "2px"
spacing:
  sp-1: "4px"
  sp-2: "8px"
  sp-3: "12px"
  sp-4: "16px"
  sp-6: "24px"
  sp-8: "32px"
  sp-12: "48px"
  sp-16: "64px"
  coluna-divisao: "12.5rem"
  coluna-leitura: "40rem"
  coluna-referencias-min: "14rem"
  vao-colunas: "clamp(32px, 4vw, 64px)"
  largura-folha: "90rem"
  margem-folha: "clamp(16px, 4vw, 4.5rem)"
  altura-header: "64px"
  altura-header-mobile: "56px"
components:
  botao-primario:
    backgroundColor: "{colors.tinta}"
    textColor: "{colors.sobre-tinta}"
    typography: "{typography.corrente}"
    rounded: "{rounded.raio}"
    padding: "10px 18px"
    height: "44px"
  botao-primario-hover:
    backgroundColor: "{colors.tinta-funda}"
  botao-fantasma:
    backgroundColor: "transparent"
    textColor: "{colors.tinta}"
    typography: "{typography.corrente}"
    rounded: "{rounded.raio}"
    padding: "10px 18px"
    height: "44px"
  botao-fantasma-hover:
    backgroundColor: "{colors.papel-realce}"
  botao-texto:
    backgroundColor: "transparent"
    textColor: "{colors.tinta}"
    typography: "{typography.corrente}"
    padding: "10px 4px"
    height: "44px"
  campo:
    backgroundColor: "{colors.papel}"
    textColor: "{colors.tinta}"
    rounded: "{rounded.raio}"
    padding: "9px 12px"
  referencia:
    textColor: "{colors.rubrica}"
  referencia-hover:
    textColor: "{colors.rubrica-escura}"
  aba:
    textColor: "{colors.tinta}"
    typography: "{typography.corrente-aba}"
    padding: "8px 0"
  aba-ativa:
    textColor: "{colors.rubrica}"
  titulo-corrente:
    backgroundColor: "{colors.papel}"
    textColor: "{colors.tinta}"
    typography: "{typography.corrente}"
    height: "{spacing.altura-header}"
  rodape:
    backgroundColor: "{colors.capa}"
    textColor: "{colors.sobre-capa}"
    padding: "48px clamp(16px, 4vw, 4.5rem)"
---

# Design System: Bíblia Teológica

## Overview

**Creative North Star: "A Bíblia de Estudo Aberta"**

A página é uma Bíblia de estudo aberta sobre a mesa: o texto no centro, cada afirmação com sua referência ao lado, um título corrente fino em cima e o tecido da capa aparecendo no fim. Quem lê entende onde está no cânon, lê o livro situado e confere a passagem na hora, abrindo a ARC em nova aba. O sistema só tem valor se essa conferência estiver sempre a um gesto de distância.

A densidade é a de um livro de referência, não a de um portal. As divisões vêm de filetes, de versalete e da grade; não há caixas, sombras nem camadas. A hierarquia nasce do contraste entre um título muito grande e um corpo de leitura confortável, com poucos pesos e uma única cor de acento com função.

Recusas confirmadas: herói, cartões e seções empilhadas de portal; Angular Material (decisão do utilizador em 2026-11-03, todas as telas usam componentes próprios); cruzes, pergaminho, douração, texturas envelhecidas e qualquer outro ornamento religioso; aparência fria de produto de tecnologia ou de modelo pronto.

**Key Characteristics:**
- Papel levemente frio, tinta quase preta, uma rubrica com significado e um único campo de cor cheia: a capa.
- Gelasio para título e texto; Alegreya Sans SC para tudo o que orienta a leitura.
- Coluna de referências sincronizada com o texto, linha a linha, como assinatura.
- Plano por inteiro: nenhuma sombra e cantos de 2px só onde há controle.
- Só um efeito de leitura: o destaque que liga a chamada à nota. Nenhuma animação de entrada.

## Colors

Uma paleta de livro impresso: neutros de papel e tinta, uma cor de rubrica e uma de encadernação.

### Primary
- **Rubrica** (#9e1b1b): referências bíblicas, links para fontes, chamadas e marcadores de nota, os títulos "Referências" e "Capítulos", a marca "panorama" no índice, a divisão e o lugar atuais na navegação, e o anel de foco. No título do livro, é também a cor da frase de resumo.
- **Rubrica Escura** (#7c1414): hover das referências e dos links do corpo.

### Secondary
- **Capa em Tecido Azul** (#2a3550): rodapé e guarda da página inicial, seleção de texto e, como cor de texto, as mensagens de sucesso.
- **Texto sobre Capa** (#e9e7dd): texto, links e anel de foco sobre a capa, e texto da seleção.

### Tertiary
- **Ferrugem de Erro** (#7a2d12): somente estados de erro e o sinal de campo obrigatório. É outra cor de propósito, porque erro não aponta para fonte.

### Neutral
- **Papel-Bíblia Frio** (#fbfbf8): fundo de todas as páginas e dos campos; levemente frio, não creme.
- **Tinta** (#16161a): texto, títulos, filete do título corrente, divisões de abertura de seção e o botão primário.
- **Tinta Mutada** (#55555c): rótulos de metadado, notas, resumos de lista e livros não atuais; cerca de 7:1 sobre o papel.
- **Filete** (#c9c8c0): coluna de referências, separadores internos, citações e barra de rolagem.
- **Borda de Controle** (#6f6f76): contorno dos campos em repouso, mais escuro que o filete para o controle se distinguir do papel.
- **Papel Realçado** (#f1f1ec): hover do botão fantasma e fundo de código.
- **Sobre Tinta** (#ffffff) e **Tinta Funda** (#000000): texto e hover do botão primário, e nada mais.

### Named Rules
**The Rubrica Rule.** Vermelho significa referência, nota, fonte ou lugar atual. A única exceção é a frase de resumo do livro, herdada da direção aprovada. Pergunta de auditoria: este vermelho leva a uma fonte ou diz onde a pessoa está? Se não, é tinta.

**The One Field Rule.** A capa é o único campo de cor cheia. Ela aparece no fim da página, nunca atrás de conteúdo de leitura.

## Typography

**Display Font:** Gelasio (com Georgia, 'Times New Roman', serif)
**Body Font:** Gelasio (com Georgia, 'Times New Roman', serif)
**Label/Mono Font:** Alegreya Sans SC (com 'Gill Sans', 'Trebuchet MS', sans-serif)

**Character:** Uma serifa de livro carrega título e texto; um versalete humanista faz o papel do título corrente impresso. O versalete de caixa mista substitui a caixa alta: não há transformação para maiúsculas em lugar nenhum.

### Hierarchy
- **Display do livro** (700, clamp até 7rem, entrelinha 1, -0.02em): só o nome do livro. Decisão deliberada: 112px a partir de 1400px de largura, 52px no celular, com recuo ótico de -0.03em para as hastes alinharem com o texto.
- **Display de página** (700, clamp até 6rem, entrelinha 1, -0.02em): títulos da página inicial, de Livros e de Estudos, com o mesmo recuo ótico.
- **Display de apoio** (700, clamp até 4rem, entrelinha 1,05, -0.02em): conta e páginas de apoio.
- **Headline** (700, clamp até 2,1rem, entrelinha 1,15): títulos de seção no markdown e na guarda.
- **Title** (700, 1.375rem, entrelinha 1,25): testamentos, Bibliografia, títulos de item em listas.
- **Lead** (400 itálico, 1.25rem, entrelinha 1,45): frase de abertura sob o título. O itálico é o que a distingue do corpo.
- **Body** (400, 1.1875rem, entrelinha 1,6; 1.0625rem até 640px): texto de leitura em coluna de 40rem. Numerais alinhados em todo o site.
- **Nota** (400, 0.9375rem, entrelinha 1,5): notas, citações e bibliografia.
- **Corrente** (500, 1.0625rem): navegação, botões, rótulos de campo. **Rótulo** (700, mesmo tamanho, 1,4): títulos de metadado. **Aba** (500, 1.125rem; 700 quando ativa): abas e links de ação isolados.

### Named Rules
**The Small Caps Rule.** Rótulos e navegação usam Alegreya Sans SC em caixa mista, sem `text-transform` e sem espaçamento positivo entre letras.

**The Two Voices Rule.** Gelasio lê; Alegreya orienta. Texto corrido nunca vai em versalete, navegação nunca vai em Gelasio.

## Layout

A folha tem no máximo 90rem, centrada, com margem fluida de 16px a 4,5rem. A largura vem de colunas nomeadas: divisão (12.5rem), leitura (40rem) e referências (mínimo de 14rem, indo até a margem direita da folha), com vão fluido de 32px a 64px.

A grade de leitura é a unidade do sistema: cada parágrafo ou item ocupa uma linha com texto e referências na mesma altura, de modo que as referências nunca se sobrepõem. O cabeçalho do livro usa a mesma grade, com os capítulos na coluna de referências, começando na altura das maiúsculas do título. A coluna da divisão fica fixa ao rolar.

O Estudo completo é a exceção: não tem coluna de referências (as notas vão todas para a bibliografia do fim). É uma coluna única de 54rem, centrada na folha, com o corpo crescendo de 19px a 21px conforme a tela, entrelinha 1,7 e texto justificado com hifenização, para a página não parecer espremida (decisão do utilizador, 2026-11-07). Até 640px vale a regra geral de 17px.

O índice do cânon pesa 3:2 entre Antigo e Novo Testamento, com a ordem do cânon sempre mantida. Conta e apoio ficam alinhados à margem da folha, em coluna de 26rem, sem caixa em volta. Toda página de topo tem fundo e altura mínima próprios.

Ritmo: a escala é 4, 8, 12, 16, 24, 32, 48, 64. O título corrente tem 64px de altura, 56px no celular.

Responsivo: até 900px tudo vira uma coluna. A ordem do livro passa a título, outros livros da divisão (recolhidos), capítulos, conteúdo; as referências descem para o fim de cada bloco, sob um filete; capítulos e abas rolam em uma linha, com a borda direita esmaecida por máscara. O lugar atual sai do título corrente e a navegação entra num menu. Até 640px o corpo cai para 17px e o índice para duas colunas.

### Named Rules
**The Same Height Rule.** O alinhamento é por parágrafo ou item, nunca por chamada. Texto e aparato começam na mesma linha de grade.

## Elevation & Depth

O sistema é plano. Não existe `box-shadow` em nenhum componente. A profundidade vem de filetes e da mudança de campo entre papel e capa. A camada suspensa do menu no celular é separada por um filete de tinta de 2px, não por sombra.

### Named Rules
**The Printed Page Rule.** Nada flutua sobre a folha. Se um elemento precisa se destacar, ele ganha filete, peso ou rubrica.

## Shapes

Cantos quase retos (2px) só em controles e blocos de estado: botões, campos, alertas, blocos de código. Texto de leitura e seções não têm canto porque não têm caixa. As únicas curvas restantes são funcionais: o giro de carregamento do botão.

Os filetes têm uma hierarquia: tinta de 2px sob o título corrente, tinta de 1px para abrir seções e cercar as abas, filete de 1px para o aparato e separadores internos.

## Components

### Buttons
Discretos e firmes, em versalete.
- **Shape:** quase reto (2px), altura mínima de 44px.
- **Primary:** tinta com texto branco, 10px 18px.
- **Hover / Focus:** o primário escurece até o preto, o fantasma recebe papel realçado, o de texto engrossa o sublinhado; 0.15s ease. Foco com anel de 2px em rubrica e 2px de afastamento.
- **Ghost / Text:** fantasma com borda de tinta de 1px e fundo transparente; texto sublinhado, com 4px laterais. Desabilitado a 55% de opacidade. Carregando, mostra giro de 12px em 0.7s.

### Inputs / Fields
- **Style:** borda de controle de 1px, fundo de papel, 2px de canto, Gelasio em 1rem; rótulo acima em versalete. A busca de Livros segue o mesmo material.
- **Focus:** borda passa a tinta e o anel em rubrica aparece a 1px.
- **Error / Disabled:** borda e mensagem na cor de erro, com o texto de erro abaixo do campo. Campo de senha tem alternância "mostrar" em versalete, dentro do campo.

### Alertas
Caixa fina sem fundo: 1px e texto na mesma cor, ferrugem para erro e capa para sucesso, em tamanho de nota.

### Navigation
- **Título corrente:** navegação à esquerda, lugar atual ao centro (700, 1.25rem), marca e conta à direita, sobre filete de tinta de 2px. Sublinha no hover, rubrica na seção ativa.
- **Avatar da conta:** quadrado de 44px, cantos de 2px, fundo tinta, com as iniciais (primeira letra do primeiro e do último nome) em papel, Alegreya Sans SC 700. Visitante vê só "Entrar". O menu abre sem sombra, com filete de tinta de 2px no alto e de 1px nos outros lados, e lista o nome, "Meu perfil" e "Sair". Fecha com Esc, clique fora e ao escolher um item, devolvendo o foco ao avatar. No celular, os mesmos itens entram direto no painel do menu, sem segundo nível.
- **Abas:** faixa entre dois filetes de tinta de 1px; a ativa fica em rubrica e peso 700, sem fundo e sem marcador. Rolam dentro do próprio trilho e nunca alargam a página.
- **Divisão:** nome da divisão em rubrica; livros em tinta mutada, com o atual em tinta e peso 700.
- **Rodapé:** campo de capa com marca, links, nota sobre a ARC e ano. No celular, empilhado.

### Referência
Link em rubrica, sublinhado de 1px com afastamento de 3px, sempre em nova aba para a ARC. Referência que não pode ser lida vira texto simples, sem link quebrado.

### Chamada de nota
Número em versalete a 0.7em, em rubrica e sem sublinhado, com alvo de toque de 24x24px criado por pseudo-elemento para não mexer na entrelinha.

### Coluna de Referências (assinatura)
Linha de grade com o texto à esquerda e, à direita, um filete vertical de 1px com referências lado a lado e notas numeradas em tamanho de nota. Passar o mouse ou focar uma chamada destaca a nota, e o inverso também, mesmo entre linhas do mesmo bloco: rubrica a 8% no fundo da nota e 14% na chamada, em 0.15s. No celular o filete vira horizontal e o aparato desce para o fim do bloco, na mesma ordem.

### Grade de Capítulos
Números em versalete, em células de pelo menos 26px, na coluna ao lado do título. Levam à ARC. Só um livro longo como Salmos ganha rolagem interna, limitada a 15rem. No celular rolam em uma linha.

### Índice do Cânon
Testamentos sob filete de tinta, divisões como rótulos em tinta mutada, livros em lista de texto puro. Livro com panorama recebe a palavra "panorama" em rubrica, nunca um ícone.

### Bibliografia do estudo
Lista numerada em tamanho de nota no fim do estudo, sob filete de tinta. Deliberado e provisório: ela repete as notas do texto, aceito pelo utilizador até haver estudo real.

### Estados
Carregando, erro, vazio e não encontrado são texto simples na coluna de leitura, dizendo o que aconteceu e o que fazer, com botão fantasma "Tentar novamente" quando cabe.

## Do's and Don'ts

### Do:
- **Do** colocar toda referência bíblica em link para a ARC, em nova aba, em rubrica.
- **Do** usar a grade de leitura sempre que um texto trouxer referências ou notas.
- **Do** manter 40rem como coluna de leitura e 1,6 de entrelinha no corpo.
- **Do** usar a capa só no rodapé e na guarda, com texto sobre capa e anel de foco na mesma cor.
- **Do** garantir 44px em botões e pelo menos 24px em qualquer alvo de toque.
- **Do** respeitar `prefers-reduced-motion`, que desliga transições e animações.
- **Do** usar a cor de erro, nunca a rubrica, em estados de erro.

### Don't:
- **Don't** usar Angular Material nem outra biblioteca de componentes visuais.
- **Don't** usar sombras nem cartões para agrupar conteúdo; `bt-cartao` não existe em tela publicada.
- **Don't** usar gradiente como efeito visual; a máscara que esmaece a borda de uma faixa rolável é a única exceção, porque avisa que há mais conteúdo.
- **Don't** usar cruzes, pergaminho, douração, texturas envelhecidas ou outro ornamento religioso.
- **Don't** usar rubrica como enfeite, nem para decidir uma interpretação: nenhum rótulo classifica uma profecia ou promessa.
- **Don't** criar caixa alta com `text-transform` para rótulos.
- **Don't** adicionar animação de entrada.
- **Don't** arredondar cantos além de 2px, salvo o giro de carregamento.
