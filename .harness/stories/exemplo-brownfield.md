# História: Cancelamento de pedido (OrderService legado)

## Contexto
`OrderService` existe desde 2019, em `src/Legacy.Orders/OrderService.cs`. Não possui
testes unitários (ver `PROJECT-NOTES.md`, seção "Zonas de risco"). Usa EF Core 6 com
DbContext injetado diretamente (sem repositório) — é o padrão do projeto, não mudar isso
mesmo que pareça datado.

## Escopo (mais restrito que em projeto novo, de propósito)
- Pode alterar: `src/Legacy.Orders/OrderService.cs` (apenas adicionar o método
  `CancelOrder`, não tocar métodos existentes), `src/Legacy.Orders.Tests/*` (criar,
  já que não existe pasta de testes ainda — confirmar isso no onboarding antes de criar)
- Não pode alterar: qualquer método já existente em `OrderService.cs` além de
  adicionar o novo método; `Legacy.Orders.Infrastructure` (fora do escopo mesmo que
  pareça relacionado)
- Se o método `CancelOrder` precisar de algo que hoje não existe (ex: um evento de
  domínio `OrderCancelled` que não existe no projeto), PARE e reporte — não crie
  infraestrutura nova de eventos sem alinhamento, isso é decisão arquitetural maior
  que o escopo desta história.

## Critérios de aceite
1. **Given** um pedido com status `Pending` ou `Confirmed`
   **When** `CancelOrder(orderId)` é chamado
   **Then** o status muda para `Cancelled` e o método retorna sucesso

2. **Given** um pedido com status `Shipped` ou `Delivered`
   **When** `CancelOrder(orderId)` é chamado
   **Then** lança `InvalidOperationException` com mensagem clara, sem alterar o pedido

## Definition of Done específico desta história
- Cobertura de 100% apenas no método `CancelOrder` (novo) — não se exige cobertura
  retroativa do resto de `OrderService.cs`
- Nenhum teste hoje passando pode passar a falhar (ver `stories/.baseline.md`)
- Se não existir projeto de testes para `Legacy.Orders`, criar seguindo o mesmo padrão
  de nomenclatura/estrutura de outro projeto de teste já existente no repositório
  (verificar em `PROJECT-NOTES.md` qual usar como referência)
