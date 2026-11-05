# Etapa 2: autenticação e área logada

Data: 2026-11-05. Parte do programa de segurança, auth e admin. A etapa 1 (auditoria e .NET 10) está em main.

## Intenção

Abrir o site ao público com contas confiáveis. O site continua aberto: a conta existe, por agora, para a pessoa ter o próprio perfil. No futuro, parte do conteúdo será só para quem estiver logado, mas isso não entra aqui.

O sucesso é um visitante conseguir criar conta, entrar, recuperar o acesso e manter os próprios dados sem ajuda, sem que um terceiro consiga descobrir quem tem conta ou usar um e-mail que não é dele.

## Decisões do utilizador

- Todas as páginas continuam acessíveis sem login.
- No topo, o visitante vê só "Entrar". "Criar conta" fica dentro da tela de entrar.
- Depois de logada, a pessoa vê um avatar no topo, com um menu de "Meu perfil" e "Sair".
- O cadastro pede nome, e-mail, telefone (opcional) e senha. Morada não é recolhida.
- Em "Meu perfil" a pessoa muda os dados do cadastro: nome, telefone, e-mail e senha.
- **O código por e-mail no login vale para todas as contas**, não só para administradores.
- **O cadastro exige aceitar um termo de proteção de dados (RGPD)**, no qual a pessoa autoriza guardar os dados.
- As quatro telas do esboço foram aprovadas (`.superpowers/brainstorm/1120-1793880098/content/conta-fluxo.html`, fora do git). O funcionamento descrito em "Fluxos" também foi aprovado na conversa.

## Fora de escopo

- Envio de SMS e qualquer uso do telefone. O número é guardado, não é verificado nem usado.
- Bloquear conteúdo para quem não está logado.
- Foto de perfil e upload.
- Captcha de terceiros.
- Fila persistida com novas tentativas de envio (etapa 4).
- Painel de admin e registro de acessos (etapa 3).

## Recusas

Uma entrega que funcione, mas faça uma destas coisas, falhou:

- Revela por resposta ou tempo de resposta se um e-mail tem conta, no cadastro ou em "Esqueci minha senha".
- Mostra texto cru do Identity ou de exceção à pessoa.
- Troca o e-mail sem provar que a pessoa controla o novo endereço.
- Parece modelo pronto ou foge do `DESIGN.md`.

## Telas

Seguem o esboço aprovado, o `DESIGN.md` (coluna de 26rem alinhada à margem, sem caixa) e o skill frontend-design. Componentes próprios de `shared/ui`. Interface em português do Brasil.

| Rota | Tela | Observação |
|---|---|---|
| `/entrar` | Entrar | Link "Esqueci minha senha" e "Criar conta". |
| `/registar` | Criar conta | Mantém a rota atual. Mostra a regra de senha antes do envio. |
| `/entrar`, `/registar` (segunda fase) | Digite o código | Componente partilhado, com "Reenviar código" e "Voltar". |
| `/esqueci-senha` | Esqueci minha senha | Novo. |
| `/redefinir-senha` | Nova senha | Novo. Dados no fragmento da URL. |
| `/conta` | Meu perfil | Sai a linha "Perfis". Título e menu dizem "Meu perfil". |
| `/conta/trocar-senha` | Trocar senha | Já existe; atualizar para o visual novo. |
| `/privacidade` | Termo e política de privacidade | Nova, pública, com link também no rodapé. |

**Topo.** Visitante: só "Entrar". Logado: botão quadrado de 44px com as iniciais, cantos de 2px e fundo tinta. O menu abre sem sombra, com filete de tinta de 2px no alto, e mostra o nome, "Meu perfil" e "Sair". Fecha com Esc, clique fora e ao escolher um item, e devolve o foco ao botão. Usa `aria-expanded`. No celular, os itens entram direto no menu que já existe, sem segundo nível.

**Iniciais.** Primeira letra do primeiro e do último nome, em maiúscula; só uma letra se o nome tiver uma palavra.

**Depois de entrar.** A pessoa volta para `retorno`, se houver; senão, vai para `/`. Isso vale também para o admin, porque a rota `/admin` ainda não existe; o desvio e o item no menu chegam com a etapa 3. `/registar` preserva `retorno`, e o valor continua a aceitar só caminhos internos.

## Fluxos

Toda resposta usa os contratos abaixo. Toda mensagem de erro é escrita por nós.

### Criar conta

1. `POST /api/auth/registar` valida o formato e a política de senha **antes** de olhar se o e-mail existe. Senha fraca gera 400 nos dois casos.
2. Conta nova: cria com `EmailConfirmed = false`, papel Membro, e enfileira o código.
3. E-mail já cadastrado: não altera a conta e não envia nada. A tela do código diz a todos, com ou sem conta: "Se este e-mail já tiver conta, nenhum código será enviado: use Entrar ou Esqueci minha senha." (decisão do utilizador em 2026-11-05, issue #53, no lugar do antigo e-mail de aviso).
4. Campo invisível preenchido (bot): nada é criado ou enviado.
5. Os três casos respondem `202 { tokenDesafio }`. Nos casos 3 e 4, o token é válido na forma, mas ligado a um id aleatório que não existe, então o código sempre falha com a mensagem genérica.

A pessoa digita o código na fase seguinte. O acerto confirma o e-mail e abre a sessão, sem passo extra.

**Termo.** O formulário tem uma caixa obrigatória, nunca marcada de início, com o texto "Li e aceito o termo de proteção de dados e autorizo guardar os meus dados", e um link que abre `/privacidade` em nova aba. O servidor também exige o aceite: sem ele, responde 400, antes de olhar se o e-mail existe. A conta nova guarda a data do aceite e a versão do termo (`TermoAceitoEm`, `TermoVersao`), o que exige uma migração.

**Cadastro abandonado.** Quem deixa o desafio vencer vai para `/entrar?aviso=cadastro`, com o texto "O tempo do código acabou. Para concluir o cadastro, use “Esqueci minha senha” com o e-mail que você informou." e um link para `/esqueci-senha`. Redefinir a senha confirma o e-mail e é o caminho de recuperação; a senha criada no cadastro não entra pelo login. Quem volta por "Criar conta" cai no caso 3: a senha digitada é descartada, a conta não muda, e a frase da tela do código indica "Entrar" e "Esqueci minha senha". Esse caso não ganha tratamento próprio, porque um código real ali criaria um novo caminho a proteger contra a revelação de contas.

### Entrar

1. `POST /api/auth/login`: senha certa envia o código e responde com o desafio, como hoje. O envio continua síncrono, para a pessoa saber na hora se falhou.
2. Falha no SMTP: registra o erro e responde 503, com mensagem "Não conseguimos enviar o código agora. Tente de novo em alguns minutos." Hoje vira 500.
3. `POST /api/auth/login/2fa`: o acerto abre a sessão. Passa também a pôr `EmailConfirmed = true`.

Uma conta com `EmailConfirmed = false` não entra por aqui: com a senha certa, recebe o mesmo 401 "Credenciais inválidas" de uma senha errada. Nenhum código é enviado e nenhuma vaga de envio é gasta. A senha é conferida antes e a tentativa conta como falha de bloqueio, exatamente como uma senha errada, para que resposta e escrita no banco (logo, o tempo) sejam as mesmas; redefinir a senha zera o bloqueio. Assim, cadastrar e logar em seguida não revela se o e-mail já tinha conta. O desafio de `/registar`, `/login/2fa` e `/login/2fa/reenviar` continua valendo para a conta recém-criada: concluir o cadastro confirma o e-mail e abre a sessão.

**Risco aceite pelo utilizador:** enquanto a etapa 4 não existir, uma falha do Gmail impede todos de entrar, incluindo o admin.

### Reenviar código

`POST /api/auth/login/2fa/reenviar { tokenDesafio }` responde sempre 204, se o token for válido na forma e não estiver vencido. Só enfileira o envio se a conta existir e o limite permitir, porque um token obtido no cadastro não pode revelar a conta pelo tempo de resposta. Limite atingido responde 429. O token ligado a um id inexistente também passa pelo mesmo limite, para a resposta não diferir. A tela diz "Se o código não chegar em alguns minutos, tente de novo" e informa que o desafio vence em 5 minutos. Vencido, leva a `/entrar` em ambos os fluxos.

### Esqueci minha senha

1. `POST /api/auth/senha/esqueci { email, site }` responde sempre `202`, sem corpo.
2. Se a conta existir e o limite permitir, enfileira o e-mail com o link.
3. Link: `{Site:UrlPublica}/redefinir-senha#email=...&token=...`. O fragmento não chega a logs do nginx nem ao cabeçalho Referer. A URL vem da configuração, **nunca** do cabeçalho Host.
4. O token vale 1 hora.
5. `POST /api/auth/senha/redefinir { email, token, novaSenha }`: token inválido ou e-mail inexistente respondem o mesmo 400 genérico, "Este link é inválido ou venceu.". Senha fraca responde 400 com a regra, só depois de o token ser aceito.
6. No sucesso: troca a senha, põe `EmailConfirmed = true`, zera o bloqueio e `DeveTrocarSenha`, revoga todas as sessões, enfileira o aviso "sua senha foi alterada" e leva a `/entrar`. A pessoa ainda passa pelo código no login.

### Trocar e-mail

Substitui `PUT /api/conta/email`, que hoje troca sem provar nada.

1. `POST /api/conta/email/solicitar { novoEmail, senha }`, autenticado e com CSRF. Senha errada conta como falha de bloqueio e responde 400 "Senha incorreta."
2. Endereço livre: enfileira um código de 6 dígitos ao novo endereço, com finalidade ligada a esse endereço (`GenerateUserTokenAsync` do provedor de e-mail com a finalidade `TrocarEmail:{novoEmail}`), sem estado pendente no banco.
3. Endereço já usado: enfileira um aviso a esse endereço e responde igual. O código digitado depois falha com a mensagem genérica. Os dois casos usam a fila para que uma pessoa logada não descubra contas pelo tempo de resposta.
4. `POST /api/conta/email/confirmar { novoEmail, codigo }`: confere o código, troca `Email` e `UserName`, mantém `EmailConfirmed = true`, atualiza o security stamp, revoga as outras sessões e emite novos cookies para a atual. Erros contam para o bloqueio. Duplicado por corrida vira o mesmo erro genérico.
5. No sucesso, enfileira um aviso ao endereço **antigo**.
6. Na tela: um campo de novo e-mail e senha atual, seguido da fase de código.

### Trocar senha

Mantém `POST /api/conta/senha`, com a mesma resposta. Acrescenta o aviso por e-mail. As mensagens passam a ser traduzidas.

### Dados do perfil

- `PUT /api/conta` passa a receber `{ nomeExibicao, telefone? }`; `GET /api/conta` devolve também `telefone`. Campo vazio apaga o número.
- O telefone usa a coluna `PhoneNumber`, que já existe no Identity, então **não há migração** para ele.
- Validação: aceita dígitos, espaços, parênteses, hífen e `+` inicial, e guarda só `+` e dígitos, de 8 a 15. `PhoneNumberConfirmed` fica `false`.
- O telefone nunca vai para o JWT, logs ou `/api/auth/eu`.
- Depois de mudar o nome, o front atualiza o estado para o avatar refletir sem novo login, e o token antigo fica com o nome velho até o próximo refresh.

### Apagar conta

Sugerida por mim e aprovada pelo utilizador, porque quem autoriza guardar os dados precisa de um meio simples de retirar a autorização.

1. Em "Meu perfil", uma seção "Apagar conta" pede a senha atual e uma confirmação na própria tela.
2. `DELETE /api/conta { senha }`, autenticado e com CSRF: senha errada conta para o bloqueio e responde 400.
3. No sucesso: apaga a conta e seus dados, sem cópia oculta, revoga as sessões, limpa os cookies, enfileira um aviso de conclusão e leva a `/`.
4. O último administrador não pode apagar a própria conta, e a tela explica o motivo.

## Termo e privacidade

A página `/privacidade` diz, em linguagem simples: quem é o responsável e como contactá-lo; quais dados são guardados (nome, e-mail, telefone opcional, data do aceite) e para quê; que o Gmail entrega os e-mails; por quanto tempo ficam (até a conta ser apagada; 7 dias para conta não confirmada); e os direitos de acesso, correção, apagamento, retirada da autorização e queixa à autoridade de controle.

- **Responsável provisório:** o utilizador ainda não definiu nome e contato. A página mostra "Bíblia Teológica" e `privacidade@example.com`, domínio reservado que não recebe e-mail, ambos em constantes num único lugar do front.
- O telefone é explicado como opcional e ainda sem uso.
- A versão do termo é uma constante no código, mostrada na página com a data. Pedir novo aceite quando o termo mudar fica para quando houver mudança.
- As contas existentes (o admin do seed) não passam pelo aceite.
- O texto é um rascunho meu, não um parecer jurídico. Vale uma revisão por quem entenda de RGPD antes de abrir ao público.

## Proteção contra abuso

| Mecanismo | Regra |
|---|---|
| Campo invisível | Campo `site` em cadastro e "esqueci", fora da tela e da tabulação, com `autocomplete="off"`. |
| Limite por IP | Política `auth` atual, 10 por minuto. Aplica-se a todos os endpoints novos sem sessão. |
| Limite por endereço | No máximo 3 envios a cada 15 minutos, por par (endereço normalizado, finalidade). Configurável. |
| Bloqueio por falhas | Identity atual: 5 falhas, 15 minutos, contando senha e código. |

**Finalidades separadas.** Código de login, cadastro, redefinição, troca de e-mail e avisos têm contadores próprios. Sem isso, um atacante que pedisse "esqueci minha senha" em repetição impediria a vítima de receber o código de login.

**Limite atingido.** Login, reenvio e troca de e-mail respondem 429. Cadastro e "esqueci" descartam o envio em silêncio e respondem igual, porque o 429 ali revelaria a existência da conta.

O contador fica em memória (`IMemoryCache`), atrás de uma interface na Application. Há uma única instância da API; um reinício zera os contadores, o que é aceitável.

## E-mail em segundo plano

Enviar durante a requisição só em um dos caminhos revelaria a existência da conta pelo tempo de resposta. Por isso, cadastro, "esqueci", reenvio, troca de e-mail e todos os avisos vão para uma fila simples em memória (`Channel` lido por um `BackgroundService`). A Application fala com ela por uma interface.

- Sem novas tentativas nem persistência: falha é registrada em log, e quem esperava um código usa "Reenviar".
- Reinício perde o que estiver na fila, o que é aceitável até a etapa 4, que a substitui.
- Logs não contêm código, token, senha nem telefone. O SMTP e o `LogEmailSender` atuais continuam por baixo. O `LogEmailSender` de dev continua a escrever o código, como hoje, e nunca é usado em Production.
- Corpos de e-mail só interpolam códigos gerados e URLs com valores codificados, nunca o nome ou outro texto da pessoa.

Só o código do login continua síncrono: ele sai depois de a senha ser conferida, então não revela nada a quem não a conhece, e a pessoa fica sabendo na hora se o envio falhou.

## Contas não confirmadas

Um `BackgroundService` diário apaga contas com `EmailConfirmed = false` e mais de 7 dias, junto com seus dados. Assim, o site não guarda telefone de quem nunca confirmou, e um e-mail cadastrado por terceiros volta a ficar livre. O seed do admin e as contas existentes já têm `EmailConfirmed = true`.

`CriarMembroAsync` hoje grava `EmailConfirmed = true`; isso muda para `false`.

## Contrato da API

O guardrail pede plano de versionamento ao alterar um endpoint. O plano aqui: a API ainda não foi publicada, e o único cliente é o front deste repositório, entregue no mesmo PR. O PR registra as mudanças:

| Endpoint | Mudança |
|---|---|
| `POST /api/auth/registar` | 201 sem corpo passa a 202 com `{ tokenDesafio }`; entra `aceitouTermo`, `telefone?` e `site?`. |
| `DELETE /api/conta` | Novo. |
| `POST /api/auth/login` | Falha no envio passa de 500 a 503; limite de envio responde 429. |
| `POST /api/auth/login/2fa/reenviar` | Novo. |
| `POST /api/auth/senha/esqueci`, `/redefinir` | Novos. |
| `PUT /api/conta/email` | **Removido.** |
| `POST /api/conta/email/solicitar`, `/confirmar` | Novos. |
| `GET` e `PUT /api/conta` | Acrescentam `telefone`. |

`/api/livros` não muda. Os novos endpoints públicos levam `[AllowAnonymous]` com comentário justificando, conforme as regras do `CLAUDE.md`. Os autenticados exigem CSRF.

## Mensagens

Um `IdentityErrorDescriber` em português, na Infrastructure, cobre os códigos de senha e de e-mail. O que sobrar vira uma mensagem genérica. O front não exibe o `detail` de erros 5xx.

## Issue #31

Entra nesta etapa como investigação de tempo limitado, porque mexe na mesma tela: tentar reproduzir a falha na primeira tentativa com sessão vencida e, se reproduzir, corrigir a causa, com teste. A parte das mensagens por status já está feita em `mensagemDeFalhaNoLogin`.

**Não bloqueia o merge.** A falha foi vista uma vez, e a leitura do código não mostra a causa. Se não reproduzir, acrescenta-se um teste de regressão do login com cookies vencidos, e a issue fica aberta com o que foi apurado.

## Critérios de pronto

- Testes de backend, caminho feliz e rejeição, para cada endpoint novo ou alterado. Incluem: respostas iguais com e sem conta, limite por finalidade e troca de e-mail com código de outro endereço.
- Testes de frontend para as telas novas, o menu do avatar e o reenvio.
- `npm run build`, `npm test` e `dotnet test` passando.
- Conferência visual em celular e desktop, incluindo o espaçamento dos campos.
- `DESIGN.md` atualizado com o avatar e o menu.
- O utilizador revê o diff e testa na stack Docker antes do merge.

## A cargo de quem constrói

- Divisão em tasks e nomes internos de classes.
- Texto final dos e-mails e das telas, dentro das regras acima.
- Como garantir o tempo de resposta parecido no cadastro, dado que só a conta nova calcula o hash da senha.

## Decisões minhas, a confirmar na revisão

- Avatar de iniciais, sem foto; linha "Perfis" removida da tela.
- Destino depois do login, com `/` em vez de `/conta`, para todos até a etapa 3.
- Fila simples em memória antes da etapa 4, usada em tudo exceto o código do login. Falha nesses envios não aparece na tela; a pessoa vê "tente de novo".
- Quem refaz um cadastro abandonado não recebe um novo código; a tela do código o orienta para "Esqueci minha senha".
- Issue #31 com tempo limitado, sem bloquear o merge.
- Remoção de contas não confirmadas após 7 dias.
- Link de redefinição válido por 1 hora; limite de 3 envios por 15 minutos.
- Decisão do utilizador (revisão final, I2): conta não confirmada não entra pelo login e recebe o mesmo 401 de uma senha errada, para que cadastrar e logar em seguida não revele se o e-mail já tinha conta. Quem abandona o cadastro recupera o acesso por "Esqueci minha senha".
- A mensagem de conta bloqueada (423) continua a existir, embora indique que a conta existe, porque a pessoa legítima precisa saber o motivo. Só surge depois de 5 falhas num endereço existente.

## Pendências antes de abrir ao público

- Trocar o responsável e o contato provisórios por dados reais. Sem um contato que funcione, ninguém consegue exercer os seus direitos.
- Revisão jurídica do texto de `/privacidade`.
