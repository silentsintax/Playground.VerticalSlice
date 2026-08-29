# Onboarding brownfield (rodar uma vez, antes da primeira história)

Objetivo: mapear o projeto existente antes de tocar em qualquer código, para que o
agente trabalhe com as convenções reais do repositório, não com suposições de projeto
novo.

## O que o agente deve investigar e registrar em `PROJECT-NOTES.md`

1. **Estrutura real** — rodar `dotnet sln list` e `Glob **/*.csproj` para mapear os
   projetos existentes. Não assumir Clean Architecture, DDD ou qualquer padrão até
   confirmar olhando o código.
2. **Convenções em uso** — abrir 3-5 classes de domínio já existentes e documentar:
   estilo de nomenclatura, uso (ou não) de nullable reference types, padrão de injeção
   de dependência, framework de teste já usado (xUnit? NUnit? MSTest?), ORM (EF Core?
   Dapper?).
3. **Cobertura de testes existente** — rodar a suíte completa (`dotnet test`) e
   registrar: quantos testes existem, quantos passam, quantos já falham ou estão
   marcados `[Skip]`/`[Ignore]` HOJE, antes de qualquer alteração. Isso vira a
   **baseline** — ver `scripts/baseline.sh`.
4. **Zonas de risco** — identificar código sem nenhum teste que provavelmente será
   tocado (ex: `OrderService` no nosso exemplo). Registrar isso explicitamente: "esta
   área não tem rede de segurança, qualquer mudança aqui exige cautela extra".
5. **Dívida técnica conhecida** — se houver `TODO`, `FIXME`, ou warnings suprimidos
   (`#pragma warning disable`) na área de interesse, registrar — não é escopo da
   história corrigir, mas o agente precisa saber que aquilo existe e não é dele.

## Regra de ouro do brownfield

O agente NUNCA deve "aproveitar e melhorar" código legado fora do escopo da história
(renomear, refatorar, atualizar padrões) mesmo que veja algo claramente melhorável.
Isso é decisão humana, separada. Mudança de escopo não pedida em brownfield é a causa
nº 1 de PRs gigantes e impossíveis de revisar.
