# Estudos completos de Isaías — design

Data: 2026-11-07. Issue #28. Continua `2026-11-02-panorama-biblico-design.md` (seção 6) depois
do estudo-piloto de Senaqueribe, aprovado e publicado em `706fef9`.

## Intenção

Quem lê Isaías no site e clica em "Estudo completo" encontra um panorama do tema: o que a
Bíblia conta, o que as fontes antigas abertas dizem quando existem, e onde os relatos
diferem, sem tomar partido. Palavras do utilizador: todos os estudos "podem ser iguais,
dando um panorama geral sobre o tema para o usuário". Nenhum é mais aprofundado que outro.

## Os estudos (decisão do utilizador)

Seis, na ordem de produção abaixo. A ordem segue a cronologia de Isaías e deixa por último
os dois temas que não são um episódio. A ordem é sugestão do autor deste documento, confirmada
pelo utilizador ("isso").

| # | Tema | Slug sugerido | Onde se liga em `isaias.yaml` |
|---|------|---------------|-------------------------------|
| 1 | Acaz e a guerra contra Israel e Síria | `acaz-guerra-siria-israel` | personagem Acaz; acontecimentos "Nos dias de Acaz" (panorama) e "No reinado de Tiglate-Pileser…" (contexto) |
| 2 | A queda de Samaria e o avanço assírio | `queda-de-samaria` | acontecimento "Por volta de 724 a 722 a.C." (contexto) |
| 3 | Nabucodonosor e a queda de Jerusalém | `nabucodonosor-queda-jerusalem` | acontecimento "Por volta de 597 a.C." (mundo) |
| 4 | Ciro e o retorno | `ciro-e-o-retorno` | personagem Ciro; acontecimento "Por volta de 539 a.C." (mundo) |
| 5 | O Servo sofredor | `servo-sofredor` | profecia "Isaías 52:13–53:12" (depois da tarefa de código do link) |
| 6 | Isaías, o profeta e o livro | `isaias-profeta-e-livro` | perfil "Quem era" (depois da tarefa de código do link) |

Os estudos se ligam entre si por `relacionados` quando um continua o outro (Acaz → Samaria
→ Senaqueribe → Nabucodonosor → Ciro); Servo e Isaías se ligam um ao outro. Quando um
vizinho fica `revisado`, os `relacionados` do estudo já publicado (inclusive o de
Senaqueribe) são atualizados no mesmo commit (decisão do autor). Os slugs são sugestões;
o slug final fica no arquivo e não muda depois de publicado.

## O molde (igual ao piloto)

- **Tamanho:** cerca de 1.200 a 1.500 palavras.
- **Estrutura:** abertura com data e as passagens bíblicas do tema; "O relato bíblico";
  "O que dizem as fontes antigas" quando houver fonte aberta conferida; comparação breve
  entre os relatos, sem decidir quem tem razão. Seção de fonte antiga só entra se ajuda a
  ler o texto bíblico (o utilizador tirou Heródoto do piloto por isso).
- **Fontes:** só abertas, abertas na URL e conferidas (`content/GUIA-DE-ESTILO.md`, seção
  2). Já no catálogo: ARC, RINAP Senaqueribe 17, Livius (prisma, ABC 1, ABC 5, ABC 7,
  Cilindro de Ciro) e cinco verbetes da ISBE. Fonte nova entra com comentário de
  conferência na `bibliografia.yaml`, um registro por verbete quando for enciclopédia.
  O catálogo pode crescer a cada estudo; preferência pela ordem do guia (Oracc/RINAP,
  Livius, British Museum, Perseus), e Josefo na tradução aberta do Perseus para 586 a.C.
  Quando nenhuma fonte antiga aberta ajuda a ler o texto, o estudo fica só com o relato
  bíblico e a comparação entre os livros bíblicos (decisão do autor).
- **Termos difíceis:** explicados na primeira vez, com nota para fonte aberta (decisão do
  utilizador no piloto).
- **Voz e neutralidade:** guia, seções 3 a 5. Nada de "cumpre/cumprida"; o que vem de fora
  é atribuído à fonte; divergência entre tradições aparece breve e atribuída.
- **Layout:** o da página de estudo, já padrão (coluna única centrada, justificado, notas
  só no fim). Nenhuma mudança de tela prevista.

## Cuidados por tema

- **Servo sofredor:** as tradições divergem sobre quem é o Servo. O texto mostra os
  cânticos (Isaías 42, 49, 50, 52–53) e onde o Novo Testamento os cita (a aba Citações já
  tem esses trechos). Leituras diferentes entram só atribuídas a uma fonte aberta e
  conferida. Antes do rascunho, procurar fonte aberta para a leitura judaica (por
  exemplo Rashi no Sefaria ou a Jewish Encyclopedia de 1906); se não houver, levar ao
  utilizador antes de escrever, porque um estudo só com as citações do Novo Testamento
  penderia para uma leitura (decisão do autor).
- **Isaías, o profeta e o livro:** a posição tradicional é a do texto. A discussão sobre
  autoria e datas dos capítulos 40–66 entra em uma frase, atribuída a quem a defende, com
  fonte aberta. Isaías já decidiu a formulação "a Jerusalém e a um povo cativo na
  Babilônia" (`3f91322`); o estudo não pode contradizê-la.
- **Nabucodonosor:** a ABC 5 cobre 597 a.C.; a queda de 586 a.C. não está nela. O texto
  não atribui à crônica o que ela não diz.

## Processo por estudo (o do piloto)

1. Rascunho (`estado: rascunho`) com fontes abertas conferidas.
2. Conferência independente por um subagente revisor disparado pela sessão que escreve,
   sem o contexto da escrita (como no piloto), só com os critérios
   1 a 3 (exatidão, neutralidade, afirmação sem fonte) mais vícios de IA, para gastar
   menos que no piloto.
3. Leitura do utilizador na stack com rascunhos visíveis.
4. O utilizador marca `revisado`; então o estudo é ligado em `isaias.yaml` e nos
   `relacionados`.

Um estudo de cada vez. O próximo só começa depois do anterior estar `revisado`, para que o
que o utilizador pedir num valha para os seguintes. Mudança de molde pedida num estudo vale
também para os já publicados, inclusive o de Senaqueribe, como tarefa curta de retoque
(decisão do autor). Ao fim de cada estudo, registrar em
`docs/superpowers/plans/2026-11-02-panorama-medicao.md` o tempo entre o primeiro commit e o
`revisado`, as rodadas com o utilizador e o custo do revisor (decisão do autor).

## Fora do escopo

- Outros livros e páginas de divisão (plano próprio, depois de Isaías).
- Mudanças de layout da página de estudo.
- Estudos sobre personagens menores (Uzias, Sebna, Merodaque-Baladã): ficam sem estudo.

## Critério de fracasso

Mesmo publicado, um estudo fracassa se:
- for raso ou com cara de IA;
- tiver uma fonte que não abre ou não diz o que o texto afirma;
- tomar partido onde as tradições divergem.

## Fechamento

Depois do sexto estudo: medição de tempo e custo consolidada (plano do panorama, Step 8) e
aprovação final de Isaías por inteiro (Step 6.2). O conteúdo já terá entrado na `main`
estudo a estudo.

## Link "Estudo completo" em Profecias e Perfil (decisão do utilizador)

O formato de conteúdo só aceita `estudo` em personagens e acontecimentos, e o Servo (aba
Profecias) e o próprio Isaías (Perfil) não tinham onde receber o link. Decisão do
utilizador: mudar o código. Os itens de `profecias`, `promessas` e `perfil` ganham
`estudo` opcional (slug), com a mesma regra 8 do validador (aponta para estudo publicado
na mesma importação), passam pela API e mostram "Estudo completo" nas duas telas, com
testes de caminho feliz e de rejeição. O contrato de `/api/livros` não muda; o de
`/api/panoramas/livros/{slug}` só ganha um campo opcional. É uma tarefa de código separada,
feita antes dos estudos 5 e 6.

## Git (decisão do utilizador)

- Agora: PR da branch `feature/BT-28-conteudo-isaias` com o que já está feito (Isaías
  revisado, piloto de Senaqueribe, correções de tela e este documento).
- Depois: cada estudo em sua própria branch a partir da `main` e seu próprio PR; a tarefa
  de código do link também tem branch e PR próprios.
