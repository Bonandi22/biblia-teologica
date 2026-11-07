# Conteúdo do Panorama Bíblico

Arquivos importados pela API na subida (`Conteudo:Pasta`). Este README e o
`GUIA-DE-ESTILO.md` não são importados.

```text
content/
  bibliografia.yaml       catálogo de obras consultadas
  livros/<slug>.yaml      página do livro (abas)
  estudos/<slug>.md       Estudo completo: frontmatter YAML + corpo Markdown
```

O formato de cada arquivo e as regras de validação estão em
`.superpowers/sdd/2026-11-02-panorama-biblico/formato.md`.

## Estados

- `rascunho`: ignorado pela importação (só um aviso em log). Para ver na tela em
  desenvolvimento, subir com `Conteudo__IncluirRascunhos=true`; a opção é ignorada em
  Production.
- `revisado`: publicado. **Só o utilizador muda um arquivo para `revisado`.**

## Processo, por arquivo

1. **Rascunho.** Escrito seguindo o `GUIA-DE-ESTILO.md`, com `estado: rascunho`. Cada
   citação de registro antigo é conferida na obra listada; o que não puder ser
   conferido não entra.
2. **Conferência independente.** Um revisor sem o contexto da escrita confere o
   rascunho contra o guia: frase genérica, fato sem utilidade para a leitura, nota sem
   correspondência, contradição com as salvaguardas, rótulo que decide interpretação.
3. **Revisão do utilizador.** Leitura na stack (`docker compose up --build`), edição e
   mudança para `revisado`. Os exemplos bons e ruins vão para a seção 6 do guia.
4. **Registro.** Tempo gasto por etapa e por aba, para estimar os demais livros.

## Bibliografia

Só entra obra que o autor do texto consegue consultar e conferiu (autor, título,
edição). Um fato que só tem fonte inacessível fica de fora até haver fonte conferida.
