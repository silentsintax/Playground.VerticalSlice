# Skill: Endpoint Minimal API (Vertical Slice)

Quando usar: qualquer história que adicione um novo endpoint HTTP neste projeto.

## Padrão a seguir

Todo endpoint segue esta estrutura, sem exceção:

```csharp
group.MapGet("/rota", async (
        TipoDoParametro parametro,
        [FromServices] IAlgumService service,
        CancellationToken ct) =>
    {
        return await service
            .MetodoAsync(parametro, ct)
            .ToOk();
    })
    .WithName("NomeDoEndpoint")
    .WithSummary("Descrição curta do que o endpoint faz")
    .Produces<AlgumDto>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```

## Regras
- Injeção via `[FromServices]`, nunca via construtor de classe estática do grupo
- `CancellationToken ct` sempre presente e sempre repassado pro service
- Retorno sempre via `.ToOk()` (extension method já existente no projeto) — não
  construir `Results.Ok(...)` manualmente
- Toda combinação de status code documentada tem que bater com o que o service
  realmente pode retornar (não copiar `ProducesProblem` de outro endpoint sem
  confirmar que o cenário existe)
- Nome do endpoint (`WithName`) no padrão `Verbo + Substantivo + Critério`
  (ex: `GetSecurityByIsin`, `GetSecurityByName`)

## O que NÃO fazer
- Não criar abstrações novas (base class de endpoint, builder customizado) só
  porque parece repetitivo — repetição controlada é intencional nesse padrão,
  facilita achar/entender qualquer endpoint isoladamente
- Não misturar validação de entrada com lógica de negócio dentro do lambda do
  `MapGet` — validação simples (nulo/vazio) pode ficar ali; regra de negócio vai
  pro service
