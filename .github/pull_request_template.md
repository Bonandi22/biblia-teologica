<!--
Fluxo do projeto: Issue → branch feature/BT-<n>-nome → commits → PR → checks → merge (squash).
Ver CLAUDE.md ("Git" e "Regras de ouro — nunca fazer").
-->

## Contexto

<!-- Que problema/issue isto resolve? Ligar a issue com "Closes #<n>". -->

Closes #

## Mudanças

<!-- Lista objetiva do que muda. Uma linha por alteração relevante. -->

-

## Risco

<!-- O que pode partir? O que não foi testado? Que parte do site/API depende do que foi tocado? -->

-

## Checklist

- [ ] `dotnet build BibliaTeologica.sln` sem erros/warnings novos
- [ ] `dotnet test` e/ou `npm test` a passar, com teste novo para endpoints/serviços novos (caminho feliz + rejeição)
- [ ] Endpoints de escrita têm `[Authorize(Roles=...)]` (ou comentário a justificar por que são públicos)
- [ ] Nenhum `_mapper.Map(dto, entity)` num Update sem ignorar campos de auditoria/propriedade
- [ ] Resposta da API não devolve entidade crua nem campos sensíveis
- [ ] CORS com origens explícitas (nunca `AllowAnyOrigin`)
- [ ] Sem segredos em código, commits, migrações ou logs
- [ ] `DateTime.UtcNow` (nunca `.Now`) em Domain/Application; async ponta a ponta com `CancellationToken`
- [ ] Frontend: sem `innerHTML` de dado bruto, sem `bypassSecurityTrust*`; `takeUntilDestroyed(this.destroyRef)` em subscrições novas
- [ ] Teste manual local (subir a stack em Docker e exercitar o fluxo) feito para mudanças com comportamento em runtime
