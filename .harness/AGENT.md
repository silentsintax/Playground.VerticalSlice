# Instruções para o agente — desenvolvimento de histórias .NET

Este projeto usa um harness local, sem dependência de vendor específico. As ferramentas
de build/test/formatação/estado estão disponíveis via MCP (servidor `dotnet-story-harness`
definido em `.mcp.json`). Use essas ferramentas em vez de rodar comandos dotnet você mesmo
fora do container — elas já rodam isoladas no Docker sandbox.

## Stack
.NET 10, C# 13/14, xUnit + FluentAssertions, Coverlet, StyleCop.Analyzers.
Ajuste este bloco para refletir a stack real do seu projeto (versão do .NET,
framework de teste, ORM, etc.) — o agente lê isso literalmente.

## Ferramentas disponíveis (via MCP)
- `dotnet_build` — build com `-warnaserror`, retorna só os erros relevantes
- `dotnet_test` — retorna só o trecho de falhas relevante
- `dotnet_format_check` / `dotnet_format_apply`
- `state_read(slug)` / `state_write(slug, content)` — memória externa em arquivo
- `scope_check(storyFiles)` — detecta sobreposição de escopo entre histórias

## Contexto brownfield
Este projeto é existente (brownfield). Antes de qualquer história:
1. Leia `PROJECT-NOTES.md` (gerado no onboarding) para convenções reais do repositório.
2. Leia `stories/.baseline.md` para saber quantos testes já passavam/falhavam ANTES
   desta história — isso é o piso de não-regressão, não o objetivo a atingir.
3. Nunca "aproveite" para refatorar, renomear ou modernizar código fora do escopo da
   história, mesmo que pareça uma melhoria óbvia. Registre a observação no resumo
   final em vez de agir sobre ela.

## Definition of Done (ajustado para brownfield)
Uma história só está pronta quando, nessa ordem:
1. `dotnet_build` retorna ok
2. `dotnet_test` retorna ok — **e o número de testes passando é maior ou igual ao
   registrado em `stories/.baseline.md`** (não pode haver regressão em testes que já
   passavam, mesmo em código que a história não pretendia tocar)
3. `dotnet_format_check` retorna ok
4. Cobertura de linha ≥ 80% **apenas nos arquivos novos ou efetivamente alterados
   pela história** — não se exige retroativamente cobertura em código legado que a
   história não tocou
5. Todos os critérios de aceite mapeados para pelo menos um teste

## Processo
1. Chame `state_read` para o slug da história. Se não existir, é história nova — crie o
   estado inicial com a lista de critérios de aceite como checklist.
2. TDD critério por critério: escreva teste → rode `dotnet_test` (deve falhar pelo motivo
   certo) → implemente → `dotnet_build` → `dotnet_test` → corrija se falhar.
3. Após cada critério fechado, chame `state_write` atualizando o checklist e o resumo da
   última tentativa — isso é o que permite retomar depois de perder contexto de conversa.
4. Ao fechar todos os critérios, rode `dotnet_format_check`; se falhar, `dotnet_format_apply`.
5. Pare após 3 iterações sem progresso no mesmo erro e reporte o bloqueio.

## Escopo
Nunca altere arquivos fora da seção "## Escopo" da história. Se precisar, pare e reporte
em vez de alterar — outra história pode depender daquele arquivo permanecer intocado.

## Commits
Um commit por critério de aceite fechado. Nunca `git push` sem confirmação explícita.
