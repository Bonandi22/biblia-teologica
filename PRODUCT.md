# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Primário: quem estuda a Bíblia. A pessoa está lendo, para em um trecho e quer entender o que se passa no livro e o contexto cultural, histórico e profético dele. Pode estar preparando um estudo ou conferindo uma passagem, e não precisa ter formação em teologia. Quando as necessidades entrarem em conflito, esta pessoa vem primeiro.

Secundários: o próprio autor, no seu estudo, e quem avalia o projeto como portfólio de engenharia .NET.

## Product Purpose

Plataforma de estudo bíblico que mostra cada livro situado: quem escreveu, para quem e por quê, o que acontecia em Israel e no mundo, e quais profecias e promessas o livro contém. O contexto existe para servir à compreensão do texto.

Há sucesso quando quem lê entende melhor o livro e consegue conferir na Bíblia ou em uma fonte aquilo que o site afirma. O projeto fracassa se o conteúdo for raso ou tiver cara de texto genérico de IA, mesmo com o software funcionando.

## Positioning

Cada afirmação pode ser conferida: referência bíblica para o que vem da Bíblia, bibliografia real e visível para o que vem de fora. O site aponta o que o texto diz e não toma partido onde as teologias divergem.

## Operating Context

Uso ao lado da leitura da Bíblia, em computador e em celular, inclusive em sessões longas de estudo. As referências bíblicas levam à ARC na Bíblia Online, em nova aba; o site não hospeda o texto bíblico integral.

## Capabilities and Constraints

- Fichário dos 66 livros do cânon protestante, com ficha de cada livro.
- Panorama por livro, em abas. Isaías é o primeiro; os demais livros e as divisões da Bíblia avançam só depois da aprovação do utilizador.
- Abas decididas: Panorama (perfil do autor), Contexto (situação em que o livro foi escrito), Mundo (o que acontecia ao redor), Profecias, Promessas, e Citações em outros livros, inclusive no Antigo Testamento.
- "Saber mais" abre um trecho curto no próprio cartão, com link para o estudo completo.
- Estudos longos com notas e bibliografia.
- Conta de utilizador (entrar, registrar, 2FA). Notícias e Curiosidades mantêm as rotas "em breve", mas ficam fora da navegação e da página inicial até haver conteúdo.
- Monólito .NET 8 com frontend Angular 22 (standalone, signals, CSR). Redesign não altera contratos de API existentes.
- Fora do escopo atual: leitor de capítulos, linha do tempo geral, comentários, conteúdo só para membros, newsletter, IA conversando com quem lê.
- Sem Angular Material: decisão do utilizador em 2026-11-03. Componentes próprios em todas as telas.
- Textos da interface em português do Brasil. As URLs atuais não mudam nesta etapa.

## Brand Commitments

- Nome: Bíblia Teológica.
- Neutralidade: profecias e promessas são apontadas na ordem do livro, sem categoria nem indicação de cumprimento. Nenhum rótulo pode decidir uma interpretação.
- Voz: clara e específica, acessível a quem não estudou teologia. A posição tradicional é a do texto; posições críticas são breves e atribuídas a quem as defende.
- Datas aproximadas ("por volta de..."), sem falsa precisão.
- O resultado não pode parecer feito por IA nem template, frio como produto de tecnologia, religioso caricato, denso ou difícil de ler.

## Evidence on Hand

- Dados dos 66 livros, servidos por `/api/livros`.
- Conteúdo de Isaías: ainda não aprovado (Task 10 pendente). O conteúdo usado em testes é fictício e não pode ser apresentado como real.
- Sem depoimentos, números de uso, parcerias ou imprensa. Nada disso pode ser inventado.
- Bibliografia: só obras realmente consultadas e conferidas. Nunca inventar obras ou citações.
- Uso da ARC em público depende de confirmação escrita da SBB.

## Product Principles

1. O texto bíblico é o centro; o site serve à leitura dele.
2. Tudo o que se afirma pode ser conferido.
3. Apontar, não opinar, onde as teologias divergem.
4. Clareza para quem não estudou teologia, profundidade para quem quer continuar.
5. Só mostrar o que existe de verdade: nada de promessa vazia nem conteúdo fictício em público.

## Accessibility & Inclusion

Leitura longa confortável em computador e celular. Respeitar `prefers-reduced-motion`, garantir contraste legível, alvos de toque adequados e navegação por teclado (WCAG AA como piso).
