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
- Falhas de autenticação ou de autorização (`/api/auth/*`, `/api/conta`; a área
  `/admin` e o upload de capa chegam no M2b)
- Injeção (SQL, comando, template), SSRF, path traversal
- XSS no frontend (conteúdo não sanitizado, `bypassSecurityTrust*`)
- Configuração insegura de CORS, cabeçalhos ou Docker

## Boas práticas já aplicadas

- Autenticação por JWT em cookies HttpOnly + Secure + SameSite=Strict; access curto
  (15 min) e refresh token rotativo (14 dias) com hash SHA-256 na BD e deteção de reutilização
- 2FA obrigatório por código de email em cada login (token do ASP.NET Core Identity)
- Lockout após 5 tentativas falhadas (15 min); rate limit de 10 req/min/IP em `/api/auth/*`
- CSRF: double-submit (cookie `bt_csrf` não-HttpOnly + header `X-CSRF-Token`) em toda a escrita
- Autorização: `/api/conta` opera sempre sobre o utilizador do token, nunca sobre um id
  do cliente; `[Authorize(Roles="Admin")]` em `/api/admin/**` chega no M2b
- Segredos (`Jwt:SigningKey`, `AdminSeed`, SMTP) só em env / `appsettings.Development.json`
  (git-ignored) / `.env` (git-ignored) — nunca versionados
- CORS com origens explícitas de configuração (nunca `AllowAnyOrigin`)
- Cabeçalhos de segurança e rate limiting na API
- DTOs como contrato — entidades e campos sensíveis (hash, refresh token) nunca saem numa resposta
