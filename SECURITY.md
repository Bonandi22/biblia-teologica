# Política de Segurança

## Reportar uma vulnerabilidade

**Não abras uma issue pública** para vulnerabilidades de segurança.

Usa o canal privado do GitHub:
[Security → Advisories → Report a vulnerability](https://github.com/Bonandi22/biblia-teologica/security/advisories/new)

Inclui, se possível:

- Descrição da vulnerabilidade e do impacto
- Passos para reproduzir (ou prova de conceito)
- Versão / commit afetado

Resposta esperada em alguns dias. Depois de confirmada e corrigida, a
divulgação é coordenada.

## Âmbito

Este é um projeto pessoal / peça de portfólio, sem dados reais de utilizadores.
Ainda assim, interessam relatos sobre:

- Exposição de dados ou de configuração sensível
- Falhas de autorização (quando o M2 introduzir a área `/admin`)
- Injeção (SQL, comando, template), SSRF, path traversal
- XSS no frontend (conteúdo não sanitizado, `bypassSecurityTrust*`)
- Configuração insegura de CORS, cabeçalhos ou Docker

## Boas práticas já aplicadas

- Segredos só em env vars ou `appsettings.Development.json` (git-ignored) — nunca versionados
- API pública é read-only no M1; endpoints de escrita chegam no M2 com `[Authorize(Roles=...)]`
- CORS com origens explícitas de configuração (nunca `AllowAnyOrigin`)
- Cabeçalhos de segurança e rate limiting na API
- DTOs como contrato — entidades e campos sensíveis nunca saem numa resposta
