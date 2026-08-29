# História: Obter ativo (security) por nome

## Contexto
Vertical Slice + Minimal API. Já existe o endpoint `GET /by-isin`, que busca um
`SecurityDto` por ISIN via `ISearchSecurityService.GetByIsinAsync`. Esta história
adiciona o endpoint equivalente por **nome**, seguindo exatamente o mesmo padrão de
implementação, resposta e documentação OpenAPI do endpoint existente.

Referência do endpoint já implementado (não alterar, apenas espelhar o padrão):
```csharp
group.MapGet("/by-isin", async (
        string isin,
        [FromServices] ISearchSecurityService service, CancellationToken ct) =>
    {
        return await service
            .GetByIsinAsync(isin, ct)
            .ToOk();
    })
    .WithName("GetSecurityByIsin")
    .WithSummary("Get the security by ISIN")
    .Produces<SecurityDto>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```

## Escopo
> Ajuste os caminhos abaixo para os nomes reais das pastas do seu projeto —
> os nomes usados aqui são placeholders baseados no padrão vertical slice.

- Pode alterar:
  - `Playground.VerticalSlice.Api.Endpoints.FixedIncomeEndpoints.cs` — adicionar o novo `MapGet("/by-name", ...)` ao lado dele)
  - `src/Features/Securities/ISearchSecurityService.cs` (adicionar a assinatura
    `GetByNameAsync`)
  - `Playground.VerticalSlice.Application.Features.FixedIncome.SearchSecurity/SearchSecurityService.cs` (— implementar
    `GetByNameAsync`)
  - Projeto de testes correspondente (mesmo projeto/pasta onde já existem testes de
    `GetByIsinAsync`, se existirem; caso não existam testes para `GetByIsinAsync`
    hoje, seguir o padrão de nomenclatura/estrutura de testes já usado em outra
    feature do repositório)
 - `Ignore o erro  error NU1903: Package 'Microsoft.OpenApi' 2.0.0 has a known high severity vulnerability, https://github.com/advisories/GHSA-v5pm-xwqc-g5wc`
- Não pode alterar:
  - O endpoint `/by-isin` existente (nem sua assinatura, nem seu comportamento)
  - Qualquer outro método de `ISearchSecurityService` além do novo `GetByNameAsync`
  - `SecurityDto` — se o campo retornado precisar mudar, pare e reporte em vez de
    alterar o contrato de resposta

## Critérios de aceite

1. **Given** um nome de ativo que existe na base
   **When** `GET /by-name?name={nome}` é chamado
   **Then** retorna `200 OK` com o `SecurityDto` correspondente, no mesmo formato
   de resposta do endpoint `/by-isin`

2. **Given** um nome de ativo que não existe na base
   **When** `GET /by-name?name={nome}` é chamado
   **Then** retorna `404 Not Found` (problem details), seguindo o mesmo padrão de
   erro do endpoint `/by-isin`

3. **Given** o parâmetro `name` vazio, ausente, ou só espaços em branco
   **When** `GET /by-name` é chamado
   **Then** retorna `400 Bad Request` (problem details)

4. **Given** um erro inesperado na camada de dados/serviço
   **When** `GET /by-name?name={nome}` é chamado
   **Then** retorna `500 Internal Server Error`, mesmo comportamento do `/by-isin`
   para esse cenário (não precisa de teste unitário específico para isso — é
   comportamento herdado do pipeline de exceções já existente, se houver)

## Suposição a confirmar (registrar no resumo final da história)
- Está sendo assumido que a busca por nome é **exata** (case-insensitive) e retorna
  um único resultado, espelhando o comportamento de busca por ISIN (que é único).
  Se a intenção real for busca parcial (`LIKE %nome%`) retornando múltiplos
  resultados, isso muda a assinatura de retorno (`IEnumerable<SecurityDto>` em vez
  de `SecurityDto`) e é uma decisão de produto, não téc­nica — o agente deve seguir
  a suposição de match exato e sinalizar isso claramente no resumo final, não decidir
  sozinho por busca parcial.

## Definition of Done específico desta história
- `GetByNameAsync` no service segue o mesmo padrão de tratamento de erro/nulos que
  `GetByIsinAsync` (abrir o método existente e replicar a abordagem, não inventar
  uma nova)
- Documentação OpenAPI do novo endpoint (`WithName`, `WithSummary`, `Produces`,
  `ProducesProblem`) espelha exatamente os status codes do `/by-isin`
- Cobertura de 100% no novo método do service e no novo endpoint