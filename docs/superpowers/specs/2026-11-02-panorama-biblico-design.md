# Panorama Bíblico (design)

**Data:** 2026-11-02
**Estado:** rascunho, aguardando aprovação do utilizador
**Protótipos de referência:** `.superpowers/brainstorm/1254-1793621583/content/` (`panorama-abas.html`, `saber-mais.html`; pasta git-ignored)

---

## 1. Intenção

Transformar o portal em uma plataforma de estudo bíblico. A leitora ou o leitor está
lendo a Bíblia, para em um trecho e quer entender melhor o contexto cultural, histórico
e profético. Precisa primeiro entender o que se passa no livro; o contexto existe para
servir a essa compreensão.

O diferencial é ver cada livro situado: quem escreveu, para quem e por quê, o que
acontecia em Israel e no mundo, e quais profecias o livro contém.

O fichário atual dos 66 livros é o ponto de partida. Notícias, Curiosidades e Vídeos
deixam de ser prioridade.

**Decidido pelo utilizador:**

- Abas por livro, em vez de página corrida ou linha do tempo.
- Cada aba traz explicação, principais personagens e o que acontecia, na Bíblia e no mundo.
- "Saber mais" abre um trecho curto no próprio cartão, com link "Estudo completo" para
  uma página própria.
- Panorama também por divisão da Bíblia, porque a divisão explica o livro. Exemplo do
  utilizador: o Pentateuco foi, culturalmente, basicamente para Israel, e é preciso
  entender por quê.
- Profecias: apontar a profecia e como Cristo a cumpriu, sem discutir as diferentes
  linhas teológicas.
- Datas aproximadas ("por volta de..."), sem falsa precisão.
- O utilizador escreve com ajuda de IA e revisa tudo. Perplexity ou outro MCP pode
  ajudar na pesquisa.

## 2. Objetivos, não objetivos e antiobjetivos

**Objetivos**

1. Validar formato, qualidade e esforço por livro com uma primeira entrega completa.
2. Permitir produzir os demais livros repetindo o mesmo processo, sem novo código.
3. Fazer com que qualquer afirmação publicada possa ser conferida na Bíblia ou em uma fonte.

**Não objetivos desta etapa**

- Texto bíblico integral ou leitor de capítulos.
- Entrada direta pelo capítulo. Fica para depois, mas o conteúdo já nasce ligado a
  capítulos para permitir isso.
- Linha do tempo geral da Bíblia. O conteúdo já guarda um ano de referência para permitir isso.
- Tela de administração, comentários, conteúdo só para membros, notícias, vídeos.
- IA conversando com quem lê. A IA só atua na produção do conteúdo.
- Debate entre escolas de interpretação profética.

**Antiobjetivo (o que tornaria tudo um fracasso)**

Conteúdo raso ou com cara de texto genérico de IA, mesmo que o software funcione.

## 3. Padrão de qualidade do conteúdo

Este padrão vale tanto quanto os testes de software. Um texto não está pronto se falhar:

- **Específico:** cada parágrafo deve dizer algo que seria falso ou sem sentido se
  aplicado a outro livro. Tem nomes, lugares, épocas e referências.
- **Útil para a leitura:** cada fato histórico responde "o que isso muda na compreensão
  do texto?". Fato solto não entra.
- **Conferível:** referência bíblica para o que vem da Bíblia, e fonte registrada para o
  que vem de fora, inclusive citações de registros antigos.
- **Claro:** linguagem acessível para quem não estudou teologia. Termos difíceis são
  explicados; o aprofundamento técnico fica no Estudo completo.
- **Sem vícios de IA:** sem frases genéricas, fórmulas de transição, listas de adjetivos
  ou conclusões vazias. Um guia de estilo no repositório registra isso, com exemplos bons
  e ruins tirados da revisão de Isaías.
- **Honesto quanto às divergências:** segue a voz do site definida em 4.3. A posição
  tradicional é a do texto; quando autoria ou data são discutidas, a posição crítica é
  mencionada brevemente e atribuída aos seus defensores.

Cuidado conhecido: o texto não pode atribuir a um registro antigo o que ele não diz. Toda
citação de fonte antiga é conferida em uma fonte de referência antes da revisão final.

### 3.1 Bibliografia e citações (exigência do utilizador)

- Todo conteúdo histórico e cultural publicado **sempre mostra a bibliografia usada**:
  as abas "Contexto histórico e cultural" e "O mundo da época", as páginas de divisão e os
  Estudos completos.
- **Toda citação identifica sua origem**: autor, obra e, quando houver, edição e página
  ou passagem. Citação bíblica indica livro, capítulo, versículo e tradução.
- Só entra na bibliografia uma obra realmente consultada, e cada referência é conferida
  (autor, título, edição e existência) antes da revisão final. IA pode inventar
  referências e citações, então nada é aceito só porque uma IA o apresentou.
- A importação rejeita conteúdo histórico ou cultural sem bibliografia, e citação sem
  origem identificada.

## 4. A experiência

### 4.1 Divisão

Página para cada divisão da Bíblia, usando as 10 de `SecaoCanonica`. Explica por que a
divisão existe, para quem foi escrita, o contexto cultural e histórico e o que esperar ao
ler seus livros; lista os livros e leva até eles. A página do livro leva de volta à sua
divisão.

*Decisão do construtor sugerida:* Atos e Apocalipse são divisões de um único livro; a
divisão pode redirecionar para o livro, com o conteúdo de divisão incorporado ao contexto.

### 4.2 Livro

Cabeçalho (nome, divisão, tema) e abas no topo, cada uma com endereço próprio para poder
ser compartilhada:

| Aba | Explicação | Personagens | O que acontecia |
|---|---|---|---|
| **Panorama do livro** | Resumo; quem escreveu, para quem e por quê | Do próprio livro | Momentos principais, por capítulo |
| **Contexto histórico e cultural** | Religião, sociedade, política | De Israel e Judá na época | Fatos com data aproximada em Israel e Judá |
| **O mundo da época** | Impérios e outros povos | De fora de Israel | Fatos com data aproximada em outros povos |
| **Profecias** | Ver 4.3 | | |
| **Citações** | Ver 4.3.1 | | |

As abas Profecias e Citações só aparecem se o livro tiver itens registrados nelas. As abas
precisam funcionar bem no celular.

Um livro pode tratar de mais de um período. Isaías é o exemplo: os capítulos 1–39
respondem à ameaça assíria, e os capítulos 40–66 têm em vista o exílio e o retorno. O
conteúdo deve deixar isso claro, seguindo a voz do site em 4.3, sem sugerir, por exemplo,
que o povo rebelde do início já está cativo na Babilônia.

### 4.3 Profecias

Cada item traz a referência, o que anuncia e uma de três categorias (aprovadas pelo
utilizador):

- **Cumprida em Cristo:** o próprio Novo Testamento faz a ligação, com a referência.
- **Cumprida na história:** há um fato histórico ligado à profecia.
- **Promessa ainda aguardada:** o Novo Testamento a trata como futura; só as referências,
  sem discutir como ou quando se cumprirá.

O que não tiver base clara em nenhuma categoria fica fora da aba e pode ser tratado em um
Estudo completo.

Exemplos de Isaías: 7:14 → Mt 1:22–23; 40:3 → Mt 3:3; 53 → At 8:32–35 e 1 Pe 2:22–25;
61:1–2 → Lc 4:17–21 (cumprida em Cristo); cap. 13 e 44:28–45:1 (cumpridas na história);
65:17 → Ap 21:1 (promessa ainda aguardada).

**Voz do site (aprovada pelo utilizador):** a posição é a cristã tradicional, dita
abertamente. Outras leituras aparecem de forma resumida e respeitosa, sem debate, em um
pequeno trecho "outras leituras" no Estudo completo. 

**Salvaguardas contra contradições (aprovadas pelo utilizador):**

- O site nunca afirma como fato algo que contradiga a Bíblia. O texto principal segue o
  relato bíblico.
- O que vem de fora é atribuído à fonte ("os registros assírios dizem..."), deixando claro
  que é a versão daquele povo.
- Nos Estudos completos, as divergências aparecem de forma breve em "outras leituras", sem
  debate.
- Nenhum fato é alterado para parecer que combina com a Bíblia, e a bibliografia reflete o
  que as fontes realmente dizem.

### 4.3.1 Citações (pedido do utilizador)

Mostra onde o livro é citado no Novo Testamento: o trecho do livro, onde é citado e, se
ajudar, uma frase sobre como a citação é usada. A mesma passagem pode aparecer aqui e em
Profecias, porque as abas têm propósitos diferentes.

*Decisão do autor deste documento, a confirmar:* só entram citações diretas, não alusões
ou ecos, porque estes são muito numerosos e sua identificação é discutível. A lista é
conferida em uma obra de referência registrada na bibliografia.

Exemplos de Isaías: 6:9–10 → Mt 13:14–15; 29:13 → Mt 15:8–9; 40:3 → Mt 3:3; 53:1 → Jo 12:38;
61:1–2 → Lc 4:18–19.

### 4.4 Saber mais e Estudo completo

Personagens e fatos podem ter um **Saber mais**, que abre um trecho curto no próprio
cartão com referências. Se existir um estudo, aparece o link **Estudo completo**.

O **Estudo completo** é uma página própria, com endereço estável, em Markdown, com o
relato bíblico, o que outras fontes dizem quando houver, as fontes usadas e ligações para
temas relacionados. Um estudo não pertence a um livro: o de Senaqueribe, por exemplo, é
ligado a Isaías, 2 Reis e Miqueias sem repetir o texto. Reaproveita o marco M3 (Estudos)
do roadmap.

Em qualquer livro, a ligação entre conteúdos deve ser feita por identificador estável,
não por repetição de texto.

## 5. Como o conteúdo é produzido

1. **Pesquisa:** a IA levanta os fatos e registra as fontes. Perplexity é opcional.
2. **Rascunho:** o conteúdo vira arquivos no repositório, com estado `rascunho`.
3. **Revisão:** o utilizador edita, confere e muda o estado para `revisado`.
4. **Publicação:** só o conteúdo `revisado` chega a quem lê.

**Os arquivos no repositório são a fonte da verdade**, com histórico no Git. Uma
importação grava o conteúdo no banco ao subir a aplicação e deve ser idempotente:
correções nos arquivos precisam chegar ao banco existente. Hoje `LivroSeeder` não serve
para isso, porque não faz nada se a tabela já tiver dados. O formato dos arquivos fica a
cargo do construtor, desde que seja confortável de editar à mão e que um arquivo inválido
gere erro claro, sem publicar conteúdo pela metade.

Validações automáticas sobre o conteúdo: campos obrigatórios, identificadores existentes,
referências bíblicas com livro existente e fontes presentes nos itens que as exigem.

**Datas:** cada data tem o texto mostrado ("por volta de 740 a.C.") e um ano de
referência usado só para ordenar. Datas bem aceitas, como 586 a.C., podem aparecer sem o
"por volta de".

**Direitos autorais:** a tradução escolhida pelo utilizador é a Almeida Revista e Corrigida
(ARC), da Sociedade Bíblica do Brasil, protegida por direitos autorais. Não foi encontrada
regra oficial da SBB; um site de terceiros cita um limite de 500 versículos, sem
confirmação. Antes de abrir ao público, é preciso confirmação por escrito da SBB. Até lá,
citações curtas, com a tradução identificada. As fontes pesquisadas são usadas para
informar o texto, nunca copiadas.

## 6. Primeira entrega

**Decidido pelo utilizador:** avançar somente com Isaías. Os demais livros só começam depois
da aprovação de Isaías, inclusive da fonte tipográfica.

- **Isaías** completo, com as cinco abas.
- **Estudos completos dos principais personagens e fatos históricos do contexto de Isaías**
  (decidido pelo utilizador). Lista inicial proposta pelo autor deste documento, a confirmar
  depois de escritas as abas: Isaías; Acaz e a guerra contra Israel e Síria; a queda de Samaria e
  o avanço assírio; Senaqueribe, Ezequias e o cerco de Jerusalém; Nabucodonosor e a queda de
  Jerusalém; Ciro e o retorno; o Servo sofredor. Os demais itens não têm obrigação de estudo.
  O estudo de Senaqueribe é o piloto: valida o formato antes dos demais.
- Guia de estilo e processo de produção documentados, para repetir nos demais livros.
- Ao final, registrar o tempo gasto, para estimar o restante.

**Depois da aprovação, em plano próprio:** páginas de divisão (seção 4.1) e os demais livros.

Os outros 65 livros continuam acessíveis: mostram a ficha atual na aba Panorama e não
mostram as demais abas, até terem conteúdo revisado.

A ficha atual é aproveitada como ponto de partida do Panorama; os textos podem ser
reescritos.

**Pronto significa:** testes passando, revisão do diff pelo utilizador, teste manual na
stack Docker e todo o conteúdo publicado revisado pelo utilizador contra o padrão da seção 3.

## 7. Restrições

Seguem valendo as regras do `CLAUDE.md`: Clean Architecture, DTOs como contrato, EF Core
com SQLite, Angular standalone com signals, sem `innerHTML` de dado bruto. O Markdown dos
estudos passa por sanitização (DOMPurify, já previsto no M3). Endpoints novos são de
leitura e públicos, com comentário justificando, e têm testes do caminho feliz e de rejeição.

## 8. Decisões do autor deste documento (a confirmar pelo utilizador)

1. **Ordem de trabalho:** concluir apenas o M2d-a (fundações visuais, já em andamento e com
   um teste falhando) e construir o panorama sobre esse design system. M2d-b e M2d-c
   ficam depois.
2. **M2b em espera:** o conteúdo nasce em arquivos, então o admin não é necessário agora.
   A branch `feature/BT-4-m2b-admin-upload` é preservada; seu `Livro.Atualizar` pode ser
   aproveitado.
3. **Quarta aba** para Profecias, em vez de encaixar no Panorama.
4. **Acesso aberto:** panoramas e estudos públicos, sem exigir conta.
5. **Português do Brasil** no conteúdo, como já em `livros.json` e na escrita do utilizador.
   A interface hoje mistura formas de Portugal ("registar", "Cria uma conta").
6. **Sem páginas de divisão** até a aprovação de Isaías, e estudo de Senaqueribe como piloto.
7. **Escopo do M2d-c:** o panorama reconstrói boa parte da ficha, então o re-skin de
   `biblia-ficha`, `campo-ficha` e `badge-secao` sai do M2d-c. As telas novas usam os
   primitivos do M2d-a, nunca Material.
8. **Apresentação da bibliografia** (a exigência é do utilizador; ver seção 3.1): notas
   numeradas no texto, ligadas à lista "Bibliografia" no fim da aba ou da página.
9. **O que exige fonte:** o que vem só da Bíblia exige a referência bíblica. Fato externo à
   Bíblia e citação de registro antigo exigem fonte registrada. Datas tradicionais amplamente
   conhecidas, como a dos primeiros Jogos Olímpicos, exigem fonte só uma vez no conteúdo.
10. **Fonte de referência:** obra publicada, museu ou instituição acadêmica. Respostas de
    IA e buscadores com IA servem para encontrar a fonte, nunca como a própria fonte.
    Traduções modernas de textos antigos também têm direitos, então a citação deve ser curta
    e creditada.
11. **Arquivos como fonte da verdade:** valem para todo o conteúdo do panorama e dos
    estudos. Um admin futuro, se vier, precisará de um desenho próprio que não entre em
    conflito com a importação; isso não é resolvido agora. O `ROADMAP.md` deve registrar
    que o admin do M2b e do M3 está em espera.
12. **Textos de interface em português de Portugal:** as páginas novas nascem em português
    do Brasil. A correção das telas existentes fica para o M2d-b, sem aumentar esta entrega.

## 9. Fora deste projeto: preparar a publicação

Projeto separado, necessário antes de abrir o site ao público:

- **Limite de acessos:** a API não lê `X-Forwarded-For`, então, atrás do nginx, todos os
  visitantes contam como o mesmo IP.
- **HTTPS:** `docker-compose.prod.yml` é só um exemplo, sem HTTPS, e os cookies de login
  exigem `Secure`.
- **Busca:** o site é renderizado só no navegador. Conteúdo de estudo se beneficia de
  pré-renderização, títulos e descrições por página.
- **Conta:** falta recuperar a senha, e o cadastro marca o e-mail como confirmado sem verificação.
- **Backup:** não há rotina de backup do SQLite na configuração atual.
- **Homepage:** hoje promete "como Cristo aparece no livro" e fala em "portal"; deve ser
  reescrita para a nova proposta.

## 10. A cargo do construtor

Modelo de dados e formato de arquivos; desenho das rotas (incluindo a convivência da
lista com a ficha em `biblia.routes.ts` e a substituição do "Em breve" de `/estudos`);
extração da base comum de conteúdo sugerida na "regra de três" do roadmap; como distribuir
a entrega em PRs, seguindo a convenção do repositório; atualização do `ROADMAP.md`.
