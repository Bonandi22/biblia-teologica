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

- M1 — Site público de Livros. Camadas .NET, EF Core/SQLite, Angular 22, Docker, CI. ✅ entregue.
  Spec: docs/superpowers/specs/2026-08-30-m1-site-publico-livros-design.md
- M2 — ASP.NET Core Identity + JWT (cookies HttpOnly, refresh rotativo, 2FA email) + área /admin
  (role Admin) + CRUD de Livros + upload de capa + área /conta.
  Spec: docs/superpowers/specs/2026-08-30-m2-auth-admin-livros-design.md
  Plano: docs/superpowers/plans/2026-08-30-m2-auth-admin-livros.md — entregue em 2 PRs:
  - M2a — autenticação + /conta (login 2FA, refresh, registo de Membro, perfil). ✅ entregue 2026-08-30.
  - M2b — admin de Livros (CRUD, publicar, upload de capa) + área /admin. Planeado.
  M2 antecipa do M4 o conceito de Membro (role + perfil) e a infra de email transacional.
  O M4 acrescenta: confirmação de email no registo, recuperação de password, captura/envio de newsletter.
- M2c — Casca do portal. Header/footer globais, homepage (hero + destaque do fichário +
  secções futuras + teaser de membros), páginas "Em breve" (Estudos/Notícias/Curiosidades),
  nova identidade visual "editorial" (tokens globais). Só frontend. Deixa o gancho de
  navegação/rotas para conteúdo reservado a membros, sem o implementar. ✅ entregue 2026-08-31.
  Spec: docs/superpowers/specs/2026-08-31-m2c-casca-portal-design.md
  Plano: docs/superpowers/plans/2026-08-31-m2c-casca-portal.md
- M2d — Repaginação visual (design pass). Direção "editorial moderno": organização
  "portal", Space Grotesk + Source Serif 4 + IBM Plex Mono, sotaque terracota #A6371C,
  off-white quase-monocromático. Angular Material sai das páginas públicas (primitivos
  à mão em src/app/shared/ui/) e fica só no /admin. Só frontend. Entregue em 3 PRs:
  - M2d-a — fundações + design system (tokens, tipografia, primitivos, re-tema do
    Material, .gitattributes). Spec: docs/superpowers/specs/2026-08-31-m2d-a-fundacoes-design.md
    Plano: docs/superpowers/plans/2026-08-31-m2d-a-fundacoes.md
  - M2d-b — páginas públicas re-skin (auth, conta, header/footer, em-breve, 404, titles).
  - M2d-c — homepage "portal" + re-skin do fichário.
- M3 — Estudos (público + admin), corpo em Markdown, sanitização (DOMPurify).
- M3.5 — Vídeos.
- M4 — Confirmação de e-mail no registo + recuperação de password ("esqueci-me") + captura de newsletter.
- M5+ — Doutrinas, Glossário, Cronologia, Teologia Sistemática, e-mail marketing, SSR/prerender, deploy real.

## Regra de três

No M3, ao surgir o 2º tipo long-form, avaliar extrair base comum Conteudo
(slug, título, publicado, SEO, timestamps, categoria, tags).
