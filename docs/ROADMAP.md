# Roadmap — Bíblia Teológica

Portal de conhecimento teológico. Cada vertical repete o mesmo padrão:
entidade (Domain) -> IXQueryService (Application) -> repositório (Infra) ->
controller (Api) -> feature lazy-loaded (Angular).

## Verticais previstas

| Vertical | Conteúdo | Nota |
|---|---|---|
| Livros | Ficha estruturada (campos curtos) | M1 |
| Estudos | Artigo long-form em Markdown | M3 — vira o template das próximas |
| Vídeos | Embed YouTube/Vimeo + metadados | M3.5 — entidade sem corpo |
| Teologia Sistemática | Séries encadeadas de Estudos (SerieId + Ordem) | M3 (decidir) |
| Doutrinas / Glossário / Cronologia | Entradas estruturadas | M5+ |

## Marcos

- M1 — Site público de Livros. Camadas .NET, EF Core/SQLite, Angular 22, Docker, CI.
  Spec: docs/superpowers/specs/2026-08-30-m1-site-publico-livros-design.md
- M2 — ASP.NET Core Identity + JWT + área /admin (role Admin) + CRUD de Livros + upload.
- M3 — Estudos (público + admin), corpo em Markdown, sanitização (DOMPurify).
- M3.5 — Vídeos.
- M4 — Cadastro público + confirmação de e-mail + captura de newsletter.
- M5+ — Doutrinas, Glossário, Cronologia, Teologia Sistemática, e-mail marketing, SSR/prerender, deploy real.

## Regra de três

No M3, ao surgir o 2º tipo long-form, avaliar extrair base comum Conteudo
(slug, título, publicado, SEO, timestamps, categoria, tags).
