# Panorama Bíblico Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Entregar a página completa de Isaías, com cinco abas, "Saber mais" e Estudos completos dos principais personagens e fatos históricos do seu contexto, alimentada por arquivos revisados no repositório. Os demais livros e as páginas de divisão só entram em plano próprio, depois da aprovação do utilizador, inclusive da fonte tipográfica.

**Architecture:** Os arquivos em `content/` são a fonte da verdade. A cada subida, a Application valida os arquivos `revisado`, monta o conteúdo no formato dos DTOs e substitui atomicamente a tabela `DocumentosConteudo`, que funciona como cópia de leitura (JSON por documento). Endpoints públicos só de leitura servem esses documentos; o Angular os mostra em rotas com abas e renderiza Markdown sanitizado.

**Tech Stack:** .NET 8, EF Core + SQLite, YamlDotNet (novo), xUnit + Moq + AwesomeAssertions; Angular 22 standalone + signals, marked + DOMPurify (novos), Vitest.

**Spec:** `docs/superpowers/specs/2026-11-02-panorama-biblico-design.md`

**Pré-requisito:** M2d-a concluído e em `main`, conforme `docs/superpowers/plans/2026-08-31-m2d-a-fundacoes.md`. Hoje a branch `feature/BT-x-m2d-a-fundacoes` está na Task 3 desse plano, com `botao.component.spec.ts` falhando ("type submit dispara o submit do formulário"). Este plano começa de `main` depois desse merge, com Issue e branch `feature/BT-<n>-panorama`. Os arquivos da spec e deste plano entram no primeiro commit dessa branch.

## Global Constraints

- **Regras do repositório:** todas as do `CLAUDE.md` valem. Testes: `Metodo_ResultadoEsperado_QuandoCondicao`. Commits terminam com os trailers exigidos no `CLAUDE.md`, com a URL da sessão em execução.
- **Regras trazidas do projeto ICM** (Task 1 as grava no `CLAUDE.md`):
  - Nunca alterar o contrato de endpoint existente sem plano de versionamento. `/api/livros` e `/api/livros/{slug}` não mudam.
  - Endpoint novo só de leitura traz comentário justificando por que é público.
  - Verificar staged, inclusive arquivos de nome inocente, antes de cada commit.
  - Toda página de topo define fundo e altura mínima próprios e é conferida visualmente.
  - Revisão: no máximo 3 rondas por task. Se não convergir, ou se o achado for decisão de produto ou segurança, levar ao utilizador.
  - Ao trabalhar em worktree, indicar o caminho logo no início para o utilizador testar por partes.
- **Nenhum implementador muda `estado` para `revisado`.** Só o utilizador faz isso.
- **Conteúdo publicado:** português do Brasil; citações bíblicas na ARC, curtas, com a tradução identificada. Nunca copiar fontes; toda obra da bibliografia foi realmente consultada e conferida.
- **Telas novas:** usam os primitivos do M2d-a e tokens de `_variables.scss`; nunca Angular Material. `standalone`, `OnPush`, seletor `bt-`.
- **Visual sem cara de IA (pedido do utilizador):** antes de desenhar ou reformular qualquer tela, carregar o skill `frontend-design:frontend-design` e seguir sua orientação dentro da direção visual já decidida no M2d-a. Subagentes que tocam telas recebem esta instrução. Uma tela não está pronta se parecer modelo genérico: grades de cartões iguais, cartões com ícone e gradientes decorativos não entram. Texto longo tem medida, ritmo e hierarquia de leitura pensados para estudo.
- **Segurança de conteúdo:** nenhum `bypassSecurityTrust*`; HTML só de Markdown passado por DOMPurify com a allowlist da Task 7; URLs de obras só `https://`.
- **Fora deste plano:** projeto de publicação (spec, seção 9), inclusive limite de acessos por IP e confirmação da SBB. O site não abre ao público sem ele.

## Review Focus

- **Arquivo editado no Windows:** YAML e frontmatter com CRLF ou BOM devem ser lidos normalmente (Task 3).
- **Correção ou remoção de arquivo:** ao subir de novo, o banco reflete os arquivos, sem duplicar nem manter conteúdo removido (Task 4).
- **Endereço direto:** abrir ou recarregar `/biblia/isaias/mundo` mantém a página, sem voltar a Gênesis (Task 8).
- **Livro sem panorama revisado ou aba inexistente:** Gênesis mostra a ficha; `/biblia/isaias/xyz` e as abas Profecias e Citações vazias vão para o Panorama (Task 8).
- **Texto com marcadores e referências reais:** `[^12]` em qualquer posição, "1 Pedro 2:22–25" com travessão, "Cantares" e demais nomes dos 66 livros, sem falso erro (Tasks 2 e 7).

---

## Formato do conteúdo

Os implementadores das Tasks 2, 3 e 10 usam este formato. Nomes de propriedades em YAML: camelCase.

```text
content/
  README.md               processo (não importado)
  GUIA-DE-ESTILO.md       padrão de qualidade (não importado)
  bibliografia.yaml       catálogo de obras
  livros/<slug-do-livro>.yaml
  estudos/<slug>.md       frontmatter YAML + corpo Markdown
```

```yaml
# bibliografia.yaml
- id: arc
  autor: Sociedade Bíblica do Brasil
  titulo: Bíblia Sagrada, Almeida Revista e Corrigida
  detalhes: "Edição consultada a confirmar pelo autor do texto"
  url: null

# livros/isaias.yaml
estado: rascunho            # rascunho | revisado
livro: isaias
resumo: "..."
quem: "..."
paraQuem: "..."
porque: "..."
panorama:                   # o mesmo formato vale para contexto e mundo
  explicacao: "Texto do bloco.[^1]"
  personagens:
    - nome: Ezequias
      texto: "..."
      saberMais: "..."      # opcional
      estudo: null          # slug opcional
      referencias: ["Isaías 36:1"]
  acontecimentos:
    - quando: "Por volta de 701 a.C."
      ano: -701             # obrigatório em contexto e mundo; só para ordenar
      texto: "..."
      saberMais: null
      estudo: senaqueribe-cerco-jerusalem
      referencias: ["Isaías 36–37"]
  notas:
    - { numero: 1, obra: arc, local: "Isaías 36:1" }
contexto: {}
mundo: {}
profecias:
  - referencia: "Isaías 7:14"
    anuncia: "..."
    categoria: cumpridaEmCristo  # cumpridaEmCristo | cumpridaNaHistoria | promessaAguardada
    cumprimento: ["Mateus 1:22–23"]
    fatoHistorico: null     # obrigatório em cumpridaNaHistoria
    notas: []
citacoes:                   # só citações diretas; alusões não entram
  - trecho: "Isaías 40:3"
    citadoEm: ["Mateus 3:3"]
    comentario: null        # opcional
    notas: [{ numero: 1, obra: id-da-obra-de-referencia, local: "..." }]

# estudos/<slug>.md (frontmatter)
estado: rascunho
titulo: "..."
resumo: "..."
livros: [isaias, 2-reis, miqueias]
relacionados: []            # slugs de estudos
notas: [{ numero: 1, obra: arc, local: "..." }]
```

**Regras de validação para arquivos `revisado`** (`ValidadorConteudo`, todas com arquivo e caminho na mensagem):

1. Campos obrigatórios presentes e slug do arquivo igual ao identificador interno, quando houver.
2. `livro` e `livros` existem entre os 66 livros.
3. Referência bíblica no formato `Livro cap[:vers][–fim]`, com livro existente.
4. Todo `[^n]` em um bloco tem nota; toda nota é usada no bloco; números únicos; obra existe no catálogo.
5. `contexto`, `mundo` e cada estudo têm ao menos uma nota. Acontecimentos de `contexto` e `mundo` têm `ano`.
6. Texto com aspas (`"`, `“`, `«`) tem nota `[^n]` ou `referencias` no mesmo item.
7. `cumpridaEmCristo` e `promessaAguardada` têm `cumprimento` com ao menos uma referência do Novo Testamento (Mateus a Apocalipse). `cumpridaNaHistoria` tem `fatoHistorico`, com nota se o fato não vier só da Bíblia.
8. `estudo` e `relacionados` apontam para estudo publicado na mesma importação.
9. `url` de obra, quando existe, começa com `https://`.
10. Corpo de estudo não contém tag HTML bruta.
11. Em `citacoes`, `trecho` pertence ao próprio livro do arquivo; `citadoEm` tem ao menos uma referência do Novo Testamento; cada item tem ao menos uma nota, com a obra usada na conferência.

Arquivo `rascunho` é ignorado, mesmo com YAML inválido (só um aviso em log), exceto com `Conteudo:IncluirRascunhos=true`, quando é tratado como `revisado` para a validação. A opção é ignorada em Production.

---

### Task 1: Regras do ICM e fundação de dados

**Files:**
- Modify: `CLAUDE.md`, `SECURITY.md`, `Directory.Packages.props`, `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj`, `src/BibliaTeologica.Infrastructure/Persistence/AppDbContext.cs`
- Create: `src/BibliaTeologica.Domain/Enums/TipoConteudo.cs`, `src/BibliaTeologica.Domain/Entities/DocumentoConteudo.cs`, `src/BibliaTeologica.Infrastructure/Persistence/Configurations/DocumentoConteudoConfiguration.cs`, migração `Panorama_DocumentosConteudo`
- Test: `tests/BibliaTeologica.Domain.Tests/Entities/DocumentoConteudoTests.cs`, `tests/BibliaTeologica.Infrastructure.Tests/Persistence/AppDbContextTests.cs`

**Interfaces:**
- Produces: `enum TipoConteudo { Livro, Estudo }`, armazenado como string para aceitar novos tipos sem migração de dados; `DocumentoConteudo.Criar(TipoConteudo tipo, string slug, string titulo, string resumo, string json)` com propriedades `Id, Tipo, Slug, Titulo, Resumo, Json, ImportadoEm`.

- [ ] **Step 1: Gravar as regras** da seção "Regras trazidas do projeto ICM" em `CLAUDE.md` (Guardrails e Regras de ouro). Em `SECURITY.md`, acrescentar XSS por conteúdo Markdown ao âmbito.
- [ ] **Step 2: Escrever testes que falham**: `Criar_LancaDomainException_QuandoSlugVazio`, `Criar_LancaDomainException_QuandoJsonVazio`, `Criar_DefineImportadoEmUtc_QuandoValido`; e em `AppDbContextTests`, `SaveChanges_LancaExcecao_QuandoTipoESlugRepetidos`.
- [ ] **Step 3: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~DocumentoConteudo|FullyQualifiedName~AppDbContextTests"`. Esperado: falha de compilação.
- [ ] **Step 4: Implementar** a entidade sem setters públicos; tabela `DocumentosConteudo`; índice único `(Tipo, Slug)`; `Tipo` como string (máx. 20); `Slug` máx. 120; `Titulo` máx. 200; `Resumo` e `Json` TEXT. Adicionar `YamlDotNet` ao CPM, na versão estável atual, e referência na Infrastructure. Gerar a migração com o comando do `CLAUDE.md`.
- [ ] **Step 5: Rodar os mesmos testes.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(panorama): fundacao de dados e regras do ICM`.

### Task 2: Referências e validação

**Files:**
- Create: `src/BibliaTeologica.Domain/Biblia/ReferenciaBiblica.cs`, `src/BibliaTeologica.Application/Conteudo/Registros.cs`, `src/BibliaTeologica.Application/Conteudo/ValidadorConteudo.cs`
- Test: `tests/BibliaTeologica.Domain.Tests/Biblia/ReferenciaBiblicaTests.cs`, `tests/BibliaTeologica.Application.Tests/Conteudo/ValidadorConteudoTests.cs`

**Interfaces:**
- Produces:
  - `ReferenciaBiblica.TentarLer(string texto, out string slugLivro): bool`, usando `Slug.From` no nome do livro.
  - POCOs de Application com setters públicos, no formato da seção anterior: `RegistroObra`, `RegistroBloco`, `RegistroItem`, `RegistroNota`, `RegistroProfecia`, `RegistroCitacao`, `RegistroLivro`, `RegistroEstudo` (com `CorpoMarkdown`), cada documento com `Arquivo` e `Estado`; `ConjuntoRegistros(Obras, Livros, Estudos)`.
  - `record LivroCanonico(string Slug, int Ordem)`.
  - `ValidadorConteudo.Validar(ConjuntoRegistros c, IReadOnlyList<LivroCanonico> livros): IReadOnlyList<string>`; Novo Testamento é `Ordem >= 40`.

- [ ] **Step 1: Escrever testes de referência que falham**: aceita `"Isaías 7:14"`, `"1 Pedro 2:22–25"`, `"Isaías 36–37"`; aceita os apelidos `"Cantares 2:1"` (slug `cantares-de-salomao`) e `"Salmo 23"` (slug `salmos`), que são os únicos dois fixados no código; rejeita `"Hezequias 1"`, `"Isaías"`, `""`. Teoria adicional cobrindo os nomes dos 66 livros de `livros.json`, usando as formas exatas do arquivo.
- [ ] **Step 2: Escrever um teste que falha para cada uma das 11 regras**, com um caso rejeitado e outro aceito, no padrão `Validar_RetornaErroComArquivo_QuandoNotaSemObra`. Inclui `Validar_AcumulaTodosOsErros_QuandoVariosArquivosInvalidos`.
- [ ] **Step 3: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~ReferenciaBiblica|FullyQualifiedName~ValidadorConteudo"`. Esperado: falha de compilação.
- [ ] **Step 4: Implementar** com regex `\[\^(\d+)\]` para marcadores. O validador recebe só os registros já escolhidos para publicar.
- [ ] **Step 5: Rodar os mesmos testes.** Esperado: PASS.
- [ ] **Step 6: Commit** `feat(panorama): validacao de conteudo e referencias biblicas`.

### Task 3: Leitura dos arquivos

**Files:**
- Create: `src/BibliaTeologica.Application/Common/Interfaces/IFonteConteudo.cs`, `src/BibliaTeologica.Infrastructure/Conteudo/FonteConteudoArquivos.cs`, `src/BibliaTeologica.Infrastructure/Configuracao/ConteudoOptions.cs`, `content/bibliografia.yaml`
- Modify: `src/BibliaTeologica.Infrastructure/BibliaTeologica.Infrastructure.csproj`, `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`, `src/BibliaTeologica.Api/Dockerfile`
- Test: `tests/BibliaTeologica.Infrastructure.Tests/Conteudo/FonteConteudoArquivosTests.cs`

**Interfaces:**
- Consumes: registros da Task 2.
- Produces: `IFonteConteudo.LerAsync(bool incluirRascunhos, CancellationToken ct): Task<ConjuntoRegistros>`; `class ConteudoInvalidoException : Exception` com `IReadOnlyList<string> Erros`, em `Application/Conteudo`; `ConteudoOptions { string Pasta = "content"; bool IncluirRascunhos = false }`, seção `Conteudo`.

- [ ] **Step 1: Escrever testes que falham**, com pasta temporária:
  - `LerAsync_LeTodosOsTipos_QuandoArquivosRevisados`
  - `LerAsync_LeNormalmente_QuandoArquivoTemCrlfEBom`
  - `LerAsync_IgnoraRascunhoInvalido_QuandoNaoIncluiRascunhos`
  - `LerAsync_LancaConteudoInvalido_QuandoRevisadoTemYamlInvalido`, com o nome do arquivo no erro
  - `LerAsync_LancaConteudoInvalido_QuandoArquivoMaiorQue1Mb`
  - `LerAsync_NaoSegueLink_QuandoArquivoAponteParaForaDaPasta`, ignorado no Windows sem privilégio de symlink
  - `LerAsync_IgnoraReadmeEGuia_QuandoNaRaiz`
- [ ] **Step 2: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~FonteConteudoArquivos"`. Esperado: falha de compilação.
- [ ] **Step 3: Implementar** com `CamelCaseNamingConvention` do YamlDotNet. Ler só `bibliografia.yaml`, `livros/*.yaml`, `estudos/*.md`, sem recursão, ignorando entradas com `LinkTarget`, limite de 1 MB. Frontmatter delimitado por linhas `---`, depois de normalizar CRLF.
- [ ] **Step 4: Empacotar**: `<Content Include="..\..\content\**\*" LinkBase="content" CopyToOutputDirectory="PreserveNewest" />` na Infrastructure; `COPY content/ content/` no Dockerfile, antes do `dotnet publish`. Pasta relativa resolve contra `AppContext.BaseDirectory`. Criar `content/bibliografia.yaml` como lista vazia.
- [ ] **Step 5: Rodar os testes** e `docker compose build api`. Esperado: PASS e build concluído.
- [ ] **Step 6: Commit** `feat(panorama): leitura dos arquivos de conteudo`.

### Task 4: Importação

**Files:**
- Create: `src/BibliaTeologica.Application/Conteudo/Dtos/ConteudoDtos.cs`, `src/BibliaTeologica.Application/Conteudo/ConstrutorDocumentos.cs`, `src/BibliaTeologica.Application/Conteudo/ImportadorConteudo.cs`, `src/BibliaTeologica.Application/Common/Interfaces/IDocumentoConteudoRepository.cs`, `src/BibliaTeologica.Infrastructure/Repositories/DocumentoConteudoRepository.cs`
- Modify: `src/BibliaTeologica.Application/DependencyInjection.cs`, `src/BibliaTeologica.Infrastructure/DependencyInjection.cs`, `src/BibliaTeologica.Api/Program.cs`, `docker-compose.override.yml`
- Test: `tests/BibliaTeologica.Application.Tests/Conteudo/ConstrutorDocumentosTests.cs`, `tests/BibliaTeologica.Infrastructure.Tests/Conteudo/ImportacaoConteudoTests.cs`

**Interfaces:**
- Consumes: Tasks 1–3; `ILivroRepository.ListarPublicadosAsync`.
- Produces:
  - DTOs: `NotaDto(int Numero, string Citacao, string? Url)`; `ItemDto(string Nome, string Texto, string? SaberMais, string? EstudoSlug, IReadOnlyList<string> Referencias)`, com `Nome` vazio em acontecimento; `AcontecimentoDto(string Quando, ItemDto Item)`; `BlocoDto(string Explicacao, IReadOnlyList<ItemDto> Personagens, IReadOnlyList<AcontecimentoDto> Acontecimentos, IReadOnlyList<NotaDto> Notas)`; `ProfeciaDto(string Referencia, string Anuncia, string Categoria, IReadOnlyList<string> Cumprimento, string? FatoHistorico, IReadOnlyList<NotaDto> Notas)`; `CitacaoDto(string Trecho, IReadOnlyList<string> CitadoEm, string? Comentario, IReadOnlyList<NotaDto> Notas)`; `LivroPanoramaDto(string LivroSlug, string Resumo, string Quem, string ParaQuem, string Porque, BlocoDto Panorama, BlocoDto Contexto, BlocoDto Mundo, IReadOnlyList<ProfeciaDto> Profecias, IReadOnlyList<CitacaoDto> Citacoes)`; `EstudoResumoDto(string Slug, string Titulo, string Resumo)`; `EstudoDto(string Slug, string Titulo, string Resumo, string CorpoMarkdown, IReadOnlyList<string> Livros, IReadOnlyList<EstudoResumoDto> Relacionados, IReadOnlyList<NotaDto> Notas)`.
  - `IDocumentoConteudoRepository`: `SubstituirTudoAsync(IReadOnlyList<DocumentoConteudo>, CancellationToken)`, em transação; `ObterAsync(TipoConteudo, string slug, CancellationToken)`; `ListarAsync(TipoConteudo, CancellationToken)`.
  - `ImportadorConteudo.ImportarAsync(bool incluirRascunhos, CancellationToken)`.

- [ ] **Step 1: Escrever testes que falham** do construtor:
  - `Construir_FormataCitacaoCompleta_QuandoNotaTemObraELocal` ("Autor. Título. Detalhes, local.")
  - `Construir_OrdenaAcontecimentosPorAno_QuandoContextoEMundo`
  - `Construir_MantemOrdemDoArquivo_QuandoPanorama`
- [ ] **Step 2: Escrever testes que falham** da importação, com SQLite real em memória:
  - `ImportarAsync_AtualizaDocumento_QuandoArquivoCorrigido`
  - `ImportarAsync_RemoveDocumento_QuandoArquivoApagado`
  - `ImportarAsync_NaoAlteraBanco_QuandoHaArquivoInvalido`
  - `ImportarAsync_NaoAlteraLivros_QuandoImportaPanorama`
- [ ] **Step 3: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~ConstrutorDocumentos|FullyQualifiedName~ImportacaoConteudo"`. Esperado: falha de compilação.
- [ ] **Step 4: Implementar.** `ImportarAsync`: lê, valida e, se houver erro, lança `ConteudoInvalidoException` com todos; senão serializa os DTOs com `JsonSerializerDefaults.Web` e substitui tudo. A substituição usa `BeginTransactionAsync` + `ExecuteDeleteAsync` + `AddRange`.
- [ ] **Step 5: Ligar à subida.** Em `Program.cs`, depois dos seeders: `importar(incluirRascunhos: config && !app.Environment.IsProduction())`. Em `docker-compose.override.yml`: `Conteudo__IncluirRascunhos: "true"`. Erro de importação registra via `ILogger` e impede a subida.
- [ ] **Step 6: Acrescentar teste de CI** `ConteudoReal_ImportaSemErros_QuandoRepositorioAtual` em `ImportacaoConteudoTests`, lendo o `content/` copiado para a saída, sem e com rascunhos.
- [ ] **Step 7: Rodar** `dotnet test BibliaTeologica.sln`. Esperado: todos passam.
- [ ] **Step 8: Commit** `feat(panorama): importacao idempotente do conteudo`.

### Task 5: Endpoints públicos

**Files:**
- Create: `src/BibliaTeologica.Application/Conteudo/IConteudoQueryService.cs`, `src/BibliaTeologica.Application/Conteudo/ConteudoQueryService.cs`, `src/BibliaTeologica.Api/Controllers/PanoramasController.cs`, `src/BibliaTeologica.Api/Controllers/EstudosController.cs`
- Test: `tests/BibliaTeologica.Application.Tests/Conteudo/ConteudoQueryServiceTests.cs`, `tests/BibliaTeologica.Api.Tests/PanoramaEndpointsTests.cs`

**Interfaces:**
- Consumes: Task 4.
- Produces, todos `GET`, públicos e com a mesma política de limite global:
  - `/api/panoramas/livros/{slug}` → `LivroPanoramaDto` | 404
  - `/api/estudos` → `EstudoResumoDto[]`
  - `/api/estudos/{slug}` → `EstudoDto` | 404
  - `slug` com a restrição de rota `regex(^[a-z0-9-]{{1,120}}$)`.

- [ ] **Step 1: Escrever testes que falham**: um caminho feliz por rota; `GET_..._Retorna404_QuandoDocumentoNaoExiste`; `GET_..._Retorna404_QuandoSlugForaDoFormato` (`Isaias%20'`); `POST_ApiEstudos_Retorna405`; `GET_ApiLivros_MantemContrato_QuandoPanoramaExiste`, com as mesmas propriedades de hoje. O teste de API aponta `Conteudo:Pasta` para uma pasta temporária via `AppFactory.ConfiguracaoExtra`.
- [ ] **Step 2: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~PanoramaEndpoints|FullyQualifiedName~ConteudoQueryService"`. Esperado: FAIL.
- [ ] **Step 3: Implementar** controllers finos, com comentário justificando o acesso público como em `LivrosController`; repositório com `AsNoTracking()`; desserializar com `JsonSerializerDefaults.Web`. `CancellationToken` em tudo.
- [ ] **Step 4: Rodar os mesmos testes.** Esperado: PASS.
- [ ] **Step 5: Commit** `feat(panorama): endpoints publicos de leitura`.

### Task 6: Serviço e componentes de conteúdo no frontend

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/core/models/conteudo.model.ts`, `src/app/core/services/conteudo.service.ts`, `src/app/shared/ui/texto-notas/`, `src/app/shared/ui/bibliografia/`, `src/app/shared/ui/item-estudo/`, `src/app/shared/ui/bloco-conteudo/` (cada um com `.ts`, `.scss` e `.spec.ts`)

**Interfaces:**
- Consumes: DTOs da Task 4.
- Produces:
  - Interfaces TypeScript iguais aos DTOs, em camelCase.
  - `ConteudoService`: `obterLivro(slug)`, `listarEstudos()`, `obterEstudo(slug)`, retornando `Observable`.
  - `segmentarNotas(texto: string): ({ texto: string } | { nota: number })[]`.
  - `bt-texto-notas [texto]`; `bt-bibliografia [notas]`; `bt-item-estudo [item]`; `bt-bloco-conteudo [bloco] [rotuloPersonagens] [rotuloAcontecimentos]`.

- [ ] **Step 1: Escrever testes que falham**:
  - `segmentarNotas` com marcador no início, no meio, no fim, `[^12]` e `[texto comum]` sem nota.
  - `bt-texto-notas`: renderiza `<sup>` com link e, ao clicar, chama `scrollIntoView` no elemento `nota-<n>`; não renderiza HTML presente no texto.
  - `bt-item-estudo`: "Saber mais" é um `<details>` fechado; sem `saberMais` não aparece; o link "Estudo completo" leva a `/estudos/<slug>`; sem `estudoSlug` não aparece.
  - `bt-bibliografia`: lista os itens com `id="nota-<n>"`; título "Bibliografia"; não renderiza sem notas; link com `rel="noopener noreferrer"` quando há URL.
  - `bt-bloco-conteudo`: esconde a seção de personagens ou de acontecimentos quando vazia.
- [ ] **Step 2: Rodar** `npx ng test --watch=false --include "src/app/shared/ui/**/*.spec.ts"` em `frontend/biblia-teologica-web`. Esperado: FAIL.
- [ ] **Step 3: Carregar `frontend-design:frontend-design` e implementar** com interpolação, sem `innerHTML`. Cartões usam `<details>/<summary>` nativos e tokens do design system.
- [ ] **Step 4: Rodar os mesmos testes** e `npx tsc --noEmit -p tsconfig.app.json`. Esperado: PASS.
- [ ] **Step 5: Commit** `feat(panorama): componentes de conteudo com notas e bibliografia`.

### Task 7: Markdown seguro e cabeçalhos do site

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/shared/ui/markdown/markdown.ts`, `src/app/shared/ui/markdown/markdown.component.ts`, `.spec.ts`
- Modify: `frontend/biblia-teologica-web/package.json`, `frontend/biblia-teologica-web/nginx.conf`

**Interfaces:**
- Produces: `renderizarMarkdown(md: string): string`; `bt-markdown [conteudo]`, que converte `[^n]` em links de nota e mantém o mesmo comportamento de rolagem.

- [ ] **Step 1: Escrever testes que falham** de `renderizarMarkdown`:
  - Remove `<script>`, `<img onerror=...>`, `<iframe>`, `style`.
  - Remove `href="javascript:..."`, `data:` e `vbscript:`.
  - Mantém `## título`, listas, ênfase, citação e `https://` com `rel="noopener noreferrer"`.
  - Transforma `[^12]` em `<sup><a data-nota="12" ...>12</a></sup>`, inclusive em parágrafo, e não dentro de bloco de código.
  - Em `bt-markdown`: `Clique_RolaAteNota_QuandoLinkDeNota`.
- [ ] **Step 2: Rodar** `npx ng test --watch=false --include "src/app/shared/ui/markdown/**/*.spec.ts"`. Esperado: FAIL.
- [ ] **Step 3: Instalar** as versões estáveis atuais de `marked` e `dompurify`, revisando o diff do lockfile.
- [ ] **Step 4: Implementar.** Marked primeiro, depois DOMPurify com `ALLOWED_TAGS: p,h2,h3,h4,ul,ol,li,strong,em,blockquote,a,sup,code,pre,hr`, `ALLOWED_ATTR: href,rel,target,data-nota`, `ALLOWED_URI_REGEXP` só para `https:`. O resultado vai em `[innerHTML]`, deixando o sanitizador do Angular ativo. Nunca `bypassSecurityTrustHtml`.
- [ ] **Step 5: Reforçar `nginx.conf`** no nível de `server`, com `always`: `X-Content-Type-Options: nosniff`; `X-Frame-Options: DENY`; `Referrer-Policy: strict-origin-when-cross-origin`; `Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src https://fonts.gstatic.com; img-src 'self' data:; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'`.
- [ ] **Step 6: Verificar.** `npm test`, `npm run build`, `docker compose up --build`, `curl -I http://localhost:4300/`. Esperado: testes passam, os cabeçalhos aparecem e `/` carrega sem violação de CSP no console. Se o build gerar script inline, ajustar `inlineCritical` em `angular.json` em vez de relaxar a CSP.
- [ ] **Step 7: Commit** `feat(panorama): markdown sanitizado e cabecalhos de seguranca do site`.

### Task 8: Página do livro com abas

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/features/biblia/livro-pagina/livro-pagina.component.{ts,html,scss,spec.ts}`, `src/app/features/biblia/livro-pagina/profecias/profecias.component.{ts,html,scss,spec.ts}`
- Modify: `src/app/features/biblia/biblia.routes.ts`, `src/app/features/biblia/biblia-lista/biblia-lista.component.ts`, `src/app/features/biblia/biblia-lista/biblia-lista.component.spec.ts`

**Interfaces:**
- Consumes: Task 6; `BibliaFichaComponent`, sem alteração.
- Produces: rotas filhas, nesta ordem: `:slug`, `:slug/:aba`. Abas: `panorama`, `contexto`, `mundo`, `profecias`, `citacoes`.

- [ ] **Step 1: Escrever testes que falham** de `LivroPaginaComponent`:
  - `MostraFicha_QuandoApiRetorna404`.
  - `MostraTresAbas_QuandoSemProfeciasESemCitacoes`, `MostraAbaProfecias_QuandoHaProfecias` e `MostraAbaCitacoes_QuandoHaCitacoes`.
  - `RedirecionaParaPanorama_QuandoAbaInexistente`, `RedirecionaParaPanorama_QuandoAbaProfeciasSemItens` e `RedirecionaParaPanorama_QuandoAbaCitacoesSemItens`.
  - Quem/para quem/por quê aparecem só na aba Panorama.
  - Cada aba é um link com `aria-current="page"` na ativa; muda de conteúdo ao mudar a rota, sem nova chamada HTTP para o mesmo livro.
  - `MostraMensagemETentarNovamente_QuandoErroDeRede`, sem redirecionar nem mostrar a ficha em erro 500 ou de rede.
- [ ] **Step 2: Escrever testes que falham** de `ProfeciasComponent`: mostra o rótulo exato "Cumprida em Cristo", "Cumprida na história" ou "Promessa ainda aguardada" conforme a categoria; mostra as referências de cumprimento; mostra o fato histórico quando há. E de `CitacoesComponent`, em `livro-pagina/citacoes/`: mostra o trecho e todos os lugares em `citadoEm`; mostra o comentário só quando há; mostra a bibliografia.
- [ ] **Step 3: Escrever testes que falham** em `biblia-lista.component.spec.ts`: não redireciona em `/biblia/isaias` e `/biblia/isaias/mundo`; redireciona só em `/biblia`.
- [ ] **Step 4: Rodar** `npx ng test --watch=false --include "src/app/features/biblia/**/*.spec.ts"`. Esperado: FAIL.
- [ ] **Step 5: Carregar `frontend-design:frontend-design` e implementar.** Trocar o teste de `temSlug` em `biblia-lista` para só redirecionar quando a URL, sem query e fragmento, for exatamente `/biblia`. Abas em `<nav>` com rolagem horizontal no celular, sem barra quebrar a página. Título da página por aba. Subscrições com `takeUntilDestroyed(this.destroyRef)`.
- [ ] **Step 6: Rodar** `npm test`, `npx tsc --noEmit -p tsconfig.app.json`, `npm run build`. Esperado: tudo passa.
- [ ] **Step 7: Commit** `feat(panorama): pagina do livro com abas e profecias`.

### Task 9: Estudos

**Files:**
- Create: `frontend/biblia-teologica-web/src/app/features/estudos/estudos.routes.ts`, `src/app/features/estudos/estudos-lista/estudos-lista.component.{ts,html,scss,spec.ts}`, `src/app/features/estudos/estudo-pagina/estudo-pagina.component.{ts,html,scss,spec.ts}`
- Modify: `src/app/app.routes.ts`, `docs/ROADMAP.md`

**Interfaces:**
- Consumes: Tasks 6 e 7.
- Produces: `/estudos`, `/estudos/:slug`.

- [ ] **Step 1: Escrever testes que falham**:
  - `EstudosLista`: lista título e resumo; lista vazia mostra "Os estudos estão sendo preparados.".
  - `EstudoPagina`: renderiza por `bt-markdown`; mostra bibliografia; "Continue estudando" lista relacionados; links para os livros; 404 mostra "Estudo não encontrado" com link para `/estudos`.
- [ ] **Step 2: Rodar** `npx ng test --watch=false --include "src/app/features/**/*.spec.ts"`. Esperado: FAIL.
- [ ] **Step 3: Carregar `frontend-design:frontend-design`, implementar** e trocar `EmBreveComponent` em `/estudos` por `estudos.routes.ts`, com lazy loading. Atualizar a descrição de "Em breve" apenas onde for necessário para os testes existentes passarem.
- [ ] **Step 4: Atualizar `docs/ROADMAP.md`**: novo marco do panorama, começando por Isaías; divisões, demais estudos e demais livros depois da aprovação; M2b e admin do M3 em espera; M2d-c sem o re-skin da ficha.
- [ ] **Step 5: Rodar** `dotnet test BibliaTeologica.sln`, `npm test`, `npm run build`. Esperado: tudo passa.
- [ ] **Step 6: Commit** `feat(panorama): estudos`.
- [ ] **Step 7: Teste manual do utilizador**, com conteúdo fictício em uma pasta temporária via `Conteudo__Pasta`: celular e computador, recarregar em cada endereço, abrir "Saber mais", clicar nas notas, conferir o console sem violação de CSP e julgar se as telas têm cara de site genérico de IA. Abrir PR do software só depois dessa confirmação.

### Task 10: Produção do conteúdo

Esta task não cabe em um único subagente. Cada item é uma rodada: rascunho, conferência, revisão do utilizador. Trabalhar em branch própria depois do merge da Task 9.

**Files:**
- Create: `content/README.md`, `content/GUIA-DE-ESTILO.md`, `content/livros/isaias.yaml`, `content/estudos/senaqueribe-cerco-jerusalem.md`, os demais `content/estudos/<slug>.md` definidos no Step 3, `docs/superpowers/plans/2026-11-02-panorama-medicao.md`
- Modify: `content/bibliografia.yaml`

- [ ] **Step 1: Escrever o guia e o processo.** `GUIA-DE-ESTILO.md` traz as seções 3 e 3.1 e as salvaguardas da 4.3 da spec, em forma de lista de conferência. `README.md` traz as quatro etapas e o formato.
- [ ] **Step 2: Combinar as obras com o utilizador.** Levantar candidatas com a IA, conferir que existem e confirmar quais o utilizador consegue consultar. Só essas entram em `bibliografia.yaml`. Se um fato só tiver fonte inacessível, ele fica de fora ou aguarda o utilizador.
- [ ] **Step 3: Fazer a página de Isaías como `rascunho`, aba por aba**, seguindo os exemplos e os três períodos da seção 4.2 da spec. Cada citação de registro antigo é conferida na obra listada; o que não puder ser conferido não entra. Ao final, confirmar com o utilizador a lista de estudos dos principais personagens e fatos, partindo da lista inicial da seção 6 da spec. Itens fora da lista podem ter só "Saber mais" ou nenhum aprofundamento.
- [ ] **Step 4: Conferência independente.** Um revisor sem o contexto da escrita confere o rascunho contra o guia, marcando frases genéricas, fato sem utilidade para a leitura, nota sem correspondência e contradições com as salvaguardas. Corrigir.
- [ ] **Step 5: Revisão do utilizador.** O utilizador lê em `docker compose up --build`, edita e muda para `revisado`. Registrar no guia os exemplos bons e ruins encontrados.
- [ ] **Step 6: Fazer o estudo-piloto de Senaqueribe**, repetindo os Steps 3–5. **Parar até o utilizador aprovar o formato de estudo.** Depois, repetir os Steps 3–5 para cada estudo da lista aprovada, um de cada vez.
- [x] **Step 6.1: Comparar a fonte.** Decidido pelo utilizador em 2026-11-07: fica a fonte da repaginação (Gelasio + Alegreya Sans SC), sem comparação com a Space Grotesk.
- [ ] **Step 6.2: Aprovação final.** **Parar aqui até o utilizador aprovar Isaías por inteiro, incluindo conteúdo, formato e fonte.** Nenhum outro livro ou divisão começa antes disso, e esse trabalho terá plano próprio, com base na medição do Step 8.
- [ ] **Step 7: Rodar** `dotnet test BibliaTeologica.sln --filter "FullyQualifiedName~ConteudoReal"`. Esperado: PASS, sem e com rascunhos.
- [ ] **Step 8: Registrar a medição** em `2026-11-02-panorama-medicao.md`: tempo por etapa, tempo por aba e estimativa para os outros 65 livros. Incluir o guardrail de Custo do `CLAUDE.md`.
- [ ] **Step 9: Commit e PR do conteúdo**, depois da aprovação do utilizador. Verificar staged.
