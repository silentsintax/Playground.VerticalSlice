# Result Monad — Documentation

A railway-oriented, monadic `Result<T>` implementation for C# that replaces exceptions with explicit, composable error handling.

---

## Table of Contents

- [Core Types](#core-types)
- [Creating Results](#creating-results)
- [Checking State](#checking-state)
- [Accessing Values](#accessing-values)
- [Map](#map)
- [Bind / BindAsync](#bind--bindasync)
- [Match](#match)
- [Tap / TapError](#tap--taperror)
- [Recover / RecoverWhen](#recover--recoverwhen)
- [Implicit Conversions](#implicit-conversions)
- [Result.Of / Result.OfAsync](#resultof--resultofasync)
- [Error — Single vs Multiple Messages](#error--single-vs-multiple-messages)
- [Error.Append](#errorappend)
- [Chaining Examples](#chaining-examples)
- [Integration with Minimal API](#integration-with-minimal-api)

---

## Core Types

```csharp
// Represents success or failure
Result<T>

// Describes why something failed
Error

// Categorizes the kind of failure
ErrorType { NotFound, Validation, Unauthorized, Conflict, InternalError }
```

---

## Creating Results

### Success

```csharp
// Explicit
var result = Result<int>.Success(42);

// Via static helper
var result = Result<string>.Success("hello");
```

### Failure — single message

```csharp
var result = Result<int>.Failure(Error.NotFound("Security with Id 99 not found."));
var result = Result<int>.Failure(Error.Validation("Rate must be positive."));
var result = Result<int>.Failure(Error.Conflict("ISIN already registered."));
var result = Result<int>.Failure(Error.Unauthorized("User does not have access."));
var result = Result<int>.Failure(Error.InternalError("Unexpected failure.", ex));
```

### Failure — multiple messages

```csharp
var result = Result<int>.Failure(Error.Validation([
    "Rate must be positive.",
    "Maturity date must be after issue date.",
    "ISIN must follow BR format."
]));
```

---

## Checking State

```csharp
var result = Result<int>.Success(42);

if (result.IsSuccess)
    Console.WriteLine("It worked!");

if (result.IsFailure)
    Console.WriteLine("It failed.");
```

---

## Accessing Values

### Value — only on success

```csharp
var result = Result<int>.Success(42);

// Safe — always check first
if (result.IsSuccess)
    Console.WriteLine(result.Value); // 42

// Throws InvalidOperationException if called on failure
var value = result.Value;
```

### Error — only on failure

```csharp
var result = Result<int>.Failure(Error.NotFound("Not found."));

if (result.IsFailure)
{
    Console.WriteLine(result.Error.Type);       // NotFound
    Console.WriteLine(result.Error.Message);    // "Not found."  (first message)

    foreach (var msg in result.Error.Messages)  // all messages
        Console.WriteLine(msg);
}
```

---

## Map

Transforms the success value without changing the `Result` wrapper.  
If the result is a failure, `Map` is skipped and the error is forwarded.

```csharp
Result<int> idResult = Result<int>.Success(42);

// int → string
Result<string> labelResult = idResult.Map(id => $"Security #{id}");

Console.WriteLine(labelResult.Value); // "Security #42"
```

```csharp
// Failure is forwarded — map function is never called
Result<int> failed = Error.NotFound("Not found.");

Result<string> stillFailed = failed.Map(id => $"Security #{id}");

Console.WriteLine(stillFailed.IsFailure); // True
Console.WriteLine(stillFailed.Error.Message); // "Not found."
```

```csharp
// Real-world: entity → response DTO
Result<FixedIncomeSecurity> securityResult = await service.GetByIdAsync(id, ct);

Result<SecurityResponse> response = securityResult.Map(s => s.ToResponse());
```

---

## Bind / BindAsync

Chains operations that themselves return a `Result<T>`.  
Use `Bind` when the next step can also fail.

### Bind (sync)

```csharp
Result<string> GetIsin(int id)
    => id > 0
        ? Result<string>.Success("BRCDBXYZ0001")
        : Error.Validation("Id must be positive.");

Result<FixedIncomeSecurity> GetBySecurity(string isin)
    => isin.StartsWith("BR")
        ? Result<FixedIncomeSecurity>.Success(new FixedIncomeSecurity { ISIN = isin })
        : Error.NotFound($"Security {isin} not found.");

// Chain two operations — if either fails, the chain stops
Result<FixedIncomeSecurity> result = Result<int>.Success(42)
    .Bind(id   => GetIsin(id))
    .Bind(isin => GetBySecurity(isin));
```

### BindAsync (async)

```csharp
Result<int> idResult = Result<int>.Success(42);

Result<FixedIncomeSecurity> result = await idResult
    .BindAsync(id => repository.GetByIdAsync(id, ct));
```

```csharp
// Full async chain
Result<RegisterSecurityResponse> result = await ValidateSecurity(security)
    .BindAsync(s  => repository.InsertAsync(s, ct))
    .BindAsync(id => repository.GetByIdAsync(id, ct))
    .Map(s         => s.ToRegisterResponse(s.Id));
```

---

## Match

Handles both branches — always returns a value.  
The most explicit way to consume a `Result<T>`.

```csharp
Result<int> result = await service.InsertAsync(security, ct);

string message = result.Match(
    onSuccess: id    => $"Inserted with Id {id}.",
    onFailure: error => $"Failed: {error.Message}");

Console.WriteLine(message);
```

```csharp
// In a Minimal API endpoint
return result.Match(
    onSuccess: id    => Results.Created($"/api/fixed-income/{id}", id),
    onFailure: error => error.ToProblem());
```

```csharp
// Branching on error type
return result.Match(
    onSuccess: value => Results.Ok(value),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound   => Results.NotFound(),
        ErrorType.Validation => Results.BadRequest(error.Messages),
        _                    => Results.StatusCode(500)
    });
```

---

## Tap / TapError

Runs a side effect without changing the result.  
Useful for logging, metrics, or auditing.

### Tap — runs on success

```csharp
Result<int> result = await service.InsertAsync(security, ct);

result.Tap(id => logger.LogInformation("Security {Id} inserted.", id));

// Result is unchanged — Tap returns the same Result<T>
```

### TapError — runs on failure

```csharp
Result<int> result = await service.InsertAsync(security, ct);

result.TapError(error =>
    logger.LogWarning("Insert failed [{Type}]: {Message}", error.Type, error.Message));
```

### Chaining Tap with other operations

```csharp
Result<SecurityResponse> result = await service
    .InsertAsync(security, ct)
    .Tap(id    => logger.LogInformation("Inserted: {Id}", id))
    .TapError(e => logger.LogError("Failed: {Msg}", e.Message))
    .BindAsync(id => service.GetByIdAsync(id, ct))
    .Map(s         => s.ToResponse());
```

---

## Recover / RecoverWhen

Converts a failure into a success by providing a fallback value.

### Recover — always recovers from any failure

```csharp
Result<FixedIncomeSecurity> result = await repository.GetByIdAsync(99, ct);

// If not found, return a default instead of propagating the error
Result<FixedIncomeSecurity> recovered = result.Recover(_ => FixedIncomeSecurity.Default);

Console.WriteLine(recovered.IsSuccess); // True
```

### RecoverWhen — recovers only from a specific ErrorType

```csharp
Result<FixedIncomeSecurity> result = await repository.GetByIdAsync(99, ct);

// Only recover from NotFound — let Validation and others propagate
Result<FixedIncomeSecurity> recovered = result.RecoverWhen(
    ErrorType.NotFound,
    _ => FixedIncomeSecurity.Default);
```

```csharp
// Real-world: return cached value if DB fails
Result<decimal> rateResult = await rateRepository.GetLatestAsync(ct);

Result<decimal> withFallback = rateResult.RecoverWhen(
    ErrorType.InternalError,
    _ => _cache.GetLastKnownRate());
```

---

## Implicit Conversions

Allows returning values and errors directly without wrapping manually.

```csharp
// Value → Result<T> (implicit Success)
public Result<int> GetPositiveNumber(int n)
{
    if (n <= 0)
        return Error.Validation("Number must be positive."); // implicit Failure

    return n; // implicit Success — no need for Result<int>.Success(n)
}
```

```csharp
// Error → Result<T> (implicit Failure)
public Result<FixedIncomeSecurity> GetById(int id)
{
    var security = _db.Find(id);

    return security is null
        ? Error.NotFound($"Security {id} not found.") // implicit Failure
        : security;                                    // implicit Success
}
```

---

## Result.Of / Result.OfAsync

Wraps code that might throw an exception, converting it into a `Result<T>`.  
Catches all exceptions and wraps them as `InternalError`.

### Result.Of (sync)

```csharp
Result<decimal> result = Result.Of(() =>
{
    // Any exception here becomes Error.InternalError
    return decimal.Parse("not-a-number");
});

Console.WriteLine(result.IsFailure);          // True
Console.WriteLine(result.Error.Type);         // InternalError
Console.WriteLine(result.Error.Message);      // "Input string was not in a correct format."
```

### Result.OfAsync (async)

```csharp
Result<int> result = await Result.OfAsync(async () =>
{
    return await repository.InsertAsync(security, ct);
});
```

```csharp
// Catching SQL exceptions inside OfAsync
Result<int> result = await Result.OfAsync(async () =>
{
    try
    {
        return await repository.InsertAsync(security, ct);
    }
    catch (SqlException ex) when (ex.Number == 2627)
    {
        throw new InvalidOperationException("ISIN already registered.");
    }
});
```

---

## Error — Single vs Multiple Messages

### Single message

```csharp
var error = Error.Validation("Rate must be positive.");

Console.WriteLine(error.Message);               // "Rate must be positive."
Console.WriteLine(error.Messages.Count);        // 1
Console.WriteLine(error.HasMultipleMessages);   // False
Console.WriteLine(error.ToString());            // "[Validation] Rate must be positive."
```

### Multiple messages

```csharp
var error = Error.Validation([
    "Rate must be positive.",
    "Maturity date must be after issue date.",
    "ISIN must follow BR format."
]);

Console.WriteLine(error.Message);               // "Rate must be positive."  (first)
Console.WriteLine(error.Messages.Count);        // 3
Console.WriteLine(error.HasMultipleMessages);   // True

foreach (var msg in error.Messages)
    Console.WriteLine(msg);
// Rate must be positive.
// Maturity date must be after issue date.
// ISIN must follow BR format.
```

### Building from FluentValidation errors

```csharp
var validation = await _validator.ValidateAsync(entity, ct);

if (!validation.IsValid)
{
    var messages = validation.Errors.Select(e => e.ErrorMessage);
    return Error.Validation(messages);
}
```

---

## Error.Append

Adds messages to an existing error without mutating it (returns a new `Error`).

### Append single message

```csharp
var error = Error.Validation("Rate must be positive.");

var enriched = error.Append("Maturity date must be after issue date.");

Console.WriteLine(enriched.Messages.Count); // 2
```

### Append multiple messages

```csharp
var error = Error.Validation("Rate must be positive.");

var enriched = error.Append([
    "Maturity date must be after issue date.",
    "ISIN must follow BR format."
]);

Console.WriteLine(enriched.Messages.Count); // 3
```

### Chaining appends

```csharp
var error = Error.Validation("Rate must be positive.")
    .Append("Maturity date must be after issue date.")
    .Append("ISIN must follow BR format.");
```

---

## Chaining Examples

### Full service method

```csharp
public async Task<Result<int>> InsertAsync(
    FixedIncomeSecurity security,
    CancellationToken   ct = default)
{
    // Validate
    var validation = await _validator.ValidateAsync(security, ct);
    if (!validation.IsValid)
        return Error.Validation(validation.Errors.Select(e => e.ErrorMessage));

    // Persist
    return await Result.OfAsync(async () =>
    {
        try   { return await _repository.InsertAsync(security, ct); }
        catch (SqlException ex) when (ex.Number == 2627)
        {
            throw new InvalidOperationException("ISIN already registered.");
        }
    });
}
```

### Composing multiple async steps

```csharp
public async Task<Result<SecurityResponse>> RegisterAndFetchAsync(
    RegisterSecurityRequest request,
    CancellationToken       ct)
{
    var entity = MapToEntity(request);

    return await service.InsertAsync(entity, ct)        // Result<int>
        .Tap(id    => logger.LogInformation("Inserted {Id}", id))
        .TapError(e => logger.LogWarning("Failed: {Msg}", e.Message))
        .BindAsync(id => service.GetByIdAsync(id, ct))  // Result<FixedIncomeSecurity>
        .Map(s         => s.ToResponse());               // Result<SecurityResponse>
}
```

### Aggregating multiple results

```csharp
var results = await Task.WhenAll(
    service.GetByIdAsync(1, ct),
    service.GetByIdAsync(2, ct),
    service.GetByIdAsync(3, ct));

var failures = results.Where(r => r.IsFailure).Select(r => r.Error).ToList();
var values   = results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
```

---

## Integration with Minimal API

### Using extension methods from `ResultExtensions`

```csharp
// 200 OK — entity as-is
group.MapGet("/{id:int}", async (
    int id,
    [FromServices] IFixedIncomeSecurityService service,
    CancellationToken ct) =>
{
    return await service
        .GetByIdAsync(id, ct)
        .ToOk();
});

// 201 Created — raw value
group.MapPost("/", async (
    RegisterSecurityRequest request,
    [FromServices] IFixedIncomeSecurityService service,
    CancellationToken ct) =>
{
    return await service
        .InsertAsync(MapToEntity(request), ct)
        .ToCreated(id => $"/api/fixed-income/{id}");
});

// 201 Created — with DTO projection
group.MapPost("/", async (
    RegisterSecurityRequest request,
    [FromServices] IFixedIncomeSecurityService service,
    CancellationToken ct) =>
{
    var entity = MapToEntity(request);

    return await service
        .InsertAsync(entity, ct)
        .ToCreated(
            id     => $"/api/fixed-income/{id}",
            id     => entity.ToRegisterResponse(id));  // ← custom DTO
});

// 204 No Content
group.MapDelete("/{id:int}", async (
    int id,
    [FromServices] IFixedIncomeSecurityService service,
    CancellationToken ct) =>
{
    return await service
        .DeleteAsync(id, ct)
        .ToNoContent();
});

// Full control — custom success + failure handling
group.MapGet("/{id:int}", async (
    int id,
    [FromServices] IFixedIncomeSecurityService service,
    CancellationToken ct) =>
{
    return await service
        .GetByIdAsync(id, ct)
        .ToHttpResult(
            onSuccess: s     => TypedResults.Ok(s.ToResponse()),
            onFailure: error => error.ToProblem());
});
```

### Error → HTTP status mapping (via `ToProblem`)

| `ErrorType` | HTTP Status | `title` |
|---|---|---|
| `NotFound` | `404` | Resource Not Found |
| `Validation` | `400` | Validation Error |
| `Unauthorized` | `401` | Unauthorized |
| `Conflict` | `409` | Conflict |
| `InternalError` | `500` | Internal Server Error |

### Problem response shape — single error

```json
{
  "title": "Validation Error",
  "status": 400,
  "detail": "Rate must be positive.",
  "errorType": "Validation",
  "errors": ["Rate must be positive."]
}
```

### Problem response shape — multiple errors

```json
{
  "title": "Validation Error",
  "status": 400,
  "detail": "3 errors occurred.",
  "errorType": "Validation",
  "errors": [
    "Rate must be positive.",
    "Maturity date must be after issue date.",
    "ISIN must follow BR format."
  ]
}
```

---

## Quick Reference

| Method | Input | Output | When to use |
|---|---|---|---|
| `Success(value)` | `T` | `Result<T>` | Wrapping a successful value |
| `Failure(error)` | `Error` | `Result<T>` | Wrapping a failure |
| `Map(fn)` | `T → TOut` | `Result<TOut>` | Transform value, can't fail |
| `Bind(fn)` | `T → Result<TOut>` | `Result<TOut>` | Chain step that can also fail |
| `BindAsync(fn)` | `T → Task<Result<TOut>>` | `Task<Result<TOut>>` | Async chain step |
| `Match(onSuccess, onFailure)` | Two funcs | `TOut` | Consume both branches |
| `Tap(action)` | `T → void` | `Result<T>` | Side effect on success |
| `TapError(action)` | `Error → void` | `Result<T>` | Side effect on failure |
| `Recover(fn)` | `Error → T` | `Result<T>` | Fallback for any failure |
| `RecoverWhen(type, fn)` | `ErrorType, Error → T` | `Result<T>` | Fallback for specific error |
| `Result.Of(fn)` | `() → T` | `Result<T>` | Wrap sync code that may throw |
| `Result.OfAsync(fn)` | `() → Task<T>` | `Task<Result<T>>` | Wrap async code that may throw |
| `Error.Append(msg)` | `string` | `Error` | Add message to existing error |
| `error.ToProblem()` | — | `IResult` | Convert to HTTP ProblemDetails |
| `.ToOk()` | — | `IResult` | 200 OK extension |
| `.ToCreated(loc)` | `T → string` | `IResult` | 201 Created extension |
| `.ToCreated(loc, map)` | `T → string, T → TRes` | `IResult` | 201 Created with DTO |
| `.ToNoContent()` | — | `IResult` | 204 No Content extension |