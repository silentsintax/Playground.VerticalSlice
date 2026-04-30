# Maybe Monad — Documentation

A functional, monadic `Maybe<T>` implementation for C# that encodes optional values and eliminates null reference errors through composition.

---

## Table of Contents

- [Core Types](#core-types)
- [Creating Maybe](#creating-maybe)
- [Checking State](#checking-state)
- [Accessing Values](#accessing-values)
- [Map](#map)
- [FlatMap / Bind](#flatmap--bind)
- [Where — Filtering](#where--filtering)
- [Match](#match)
- [Tap / TapNone](#tap--tapnone)
- [ToEnumerable](#toenumerable)
- [Implicit Conversions](#implicit-conversions)
- [Chaining Examples](#chaining-examples)
- [Integration with Services](#integration-with-services)
- [Quick Reference](#quick-reference)

---

## Core Types

```csharp
// Represents a value that may or may not exist
Maybe<T>
```

---

## Creating Maybe

### Some — with a value

```csharp
// Explicit factory method
var maybeName = Maybe<string>.Some("Alice");

// Via static helper
var maybeAge = Maybe.Some(25);
```

### None — empty

```csharp
// Explicit
var noValue = Maybe<string>.None();

// Via static helper
var empty = Maybe.None<string>();
```

### FromNullable — from nullable values

```csharp
string? name = GetNameFromDatabase();

var maybeName = Maybe.FromNullable(name);

// If name is null, maybeName is None
// If name has a value, maybeName is Some(value)
```

---

## Checking State

```csharp
var maybeValue = Maybe.Some(42);

if (maybeValue.IsSome)
    Console.WriteLine("Has a value!");

if (maybeValue.IsNone)
    Console.WriteLine("Empty.");
```

---

## Accessing Values

### Value — only on Some

```csharp
var maybeValue = Maybe.Some(42);

// Safe — always check first
if (maybeValue.IsSome)
    Console.WriteLine(maybeValue.ToNullable()); // 42
```

### GetOrElse — with default value

```csharp
var maybeValue = Maybe.Some(42);

int result = maybeValue.GetOrElse(0);
Console.WriteLine(result); // 42

var empty = Maybe.None<int>();
int fallback = empty.GetOrElse(0);
Console.WriteLine(fallback); // 0
```

### GetOrElse — with function

```csharp
var maybeValue = Maybe<string>.None();

string result = maybeValue.GetOrElse(() => "default");
Console.WriteLine(result); // "default"

// Useful for lazy evaluation
string expensive = maybeValue.GetOrElse(() => ExpensiveComputation());
```

### GetOrThrow — extract or fail

```csharp
var maybeValue = Maybe.Some(42);

int value = maybeValue.GetOrThrow(); // 42

var empty = Maybe.None<int>();
int willThrow = empty.GetOrThrow(); 
// throws InvalidOperationException: "Cannot get value from None"
```

### GetOrThrow — with custom exception

```csharp
var maybeValue = Maybe.None<int>();

int value = maybeValue.GetOrThrow(() => 
    new ArgumentException("Value was not provided."));
// throws ArgumentException
```

### ToNullable — convert to nullable

```csharp
var maybeValue = Maybe.Some("hello");

string? result = maybeValue.ToNullable();
Console.WriteLine(result); // "hello"

var empty = Maybe.None<string>();
string? empty_result = empty.ToNullable();
Console.WriteLine(empty_result); // null
```

---

## Map

Transforms the inner value without changing the `Maybe` wrapper.  
If the `Maybe` is `None`, `Map` is skipped and `None` is forwarded.

```csharp
Maybe<int> maybeId = Maybe.Some(42);

// int → string
Maybe<string> maybeLabel = maybeId.Map(id => $"User #{id}");

Console.WriteLine(maybeLabel.GetOrElse("N/A")); // "User #42"
```

```csharp
// None is forwarded — map function is never called
Maybe<int> empty = Maybe.None<int>();

Maybe<string> stillEmpty = empty.Map(id => $"User #{id}");

Console.WriteLine(stillEmpty.IsNone); // True
```

```csharp
// Real-world: entity → response DTO
Maybe<User> maybeUser = repository.FindById(userId);

Maybe<UserResponse> response = maybeUser.Map(user => 
    new UserResponse 
    { 
        Id = user.Id, 
        Name = user.Name 
    });
```

```csharp
// Chaining maps
Maybe<int> result = Maybe.Some(5)
    .Map(x => x * 2)      // 10
    .Map(x => x + 3)      // 13
    .Map(x => x / 2);     // 6 (integer division)

Console.WriteLine(result.GetOrElse(0)); // 6
```

---

## FlatMap / Bind

Chains operations that themselves return a `Maybe<T>`.  
Use `FlatMap` or `Bind` when the next step can return `None`.

### FlatMap (sync)

```csharp
Maybe<string> GetUsername(int userId)
    => userId > 0
        ? Maybe.Some("alice")
        : Maybe.None<string>();

Maybe<int> GetUserId()
    => Maybe.Some(42);

// Chain: Maybe<int> → Maybe<string>
Maybe<string> result = GetUserId()
    .FlatMap(id => GetUsername(id));

Console.WriteLine(result.GetOrElse("unknown")); // "alice"
```

```csharp
// If any step returns None, the chain stops
Maybe<int> maybeId = Maybe.None<int>();

Maybe<string> result = maybeId
    .FlatMap(id => GetUsername(id));

Console.WriteLine(result.IsNone); // True
```

### Bind (alias for FlatMap)

```csharp
// Bind is an alias for FlatMap — use whichever reads better in your code
Maybe<string> result = GetUserId()
    .Bind(id => GetUsername(id));
```

### Real-world: retrieving related data

```csharp
public Maybe<Order> GetOrderWithCustomer(int orderId)
{
    return GetOrder(orderId)
        .Bind(order => GetCustomer(order.CustomerId)
            .Map(customer => new { order, customer })
            .Map(x => new Order 
            { 
                Id = x.order.Id,
                CustomerName = x.customer.Name
            }));
}
```

---

## Where — Filtering

Filters a `Maybe` based on a predicate.  
Returns `Some(value)` if the predicate is true, otherwise `None`.

```csharp
Maybe<int> maybeAge = Maybe.Some(25);

// Keep only if age >= 18
Maybe<int> maybeAdult = maybeAge.Where(age => age >= 18);

Console.WriteLine(maybeAdult.IsSome); // True
```

```csharp
Maybe<int> maybeAge = Maybe.Some(16);

Maybe<int> maybeAdult = maybeAge.Where(age => age >= 18);

Console.WriteLine(maybeAdult.IsNone); // True (filtered out)
```

```csharp
// Chaining filters
Maybe<string> maybeEmail = Maybe.Some("user@example.com");

Maybe<string> validEmail = maybeEmail
    .Where(email => email.Contains("@"))
    .Where(email => email.Length > 5);

Console.WriteLine(validEmail.IsSome); // True
```

```csharp
// Real-world: validation
public Maybe<User> GetActiveUser(int id)
{
    return repository.FindById(id)
        .Where(user => user.IsActive)
        .Where(user => !user.IsDeleted);
}
```

---

## Match

Handles both branches — always returns a value.  
The most explicit way to consume a `Maybe<T>`.

### Match with return value

```csharp
Maybe<int> maybeValue = Maybe.Some(42);

string message = maybeValue.Match(
    onSome: value => $"Found: {value}",
    onNone: () => "No value");

Console.WriteLine(message); // "Found: 42"
```

```csharp
Maybe<int> empty = Maybe.None<int>();

string message = empty.Match(
    onSome: value => $"Found: {value}",
    onNone: () => "No value");

Console.WriteLine(message); // "No value"
```

### Match with side effects

```csharp
Maybe<User> maybeUser = repository.FindById(userId);

maybeUser.Match(
    onSome: user => logger.LogInformation("Found user: {Name}", user.Name),
    onNone: () => logger.LogWarning("User not found"));
```

### Real-world: HTTP response mapping

```csharp
return maybeUser.Match(
    onSome: user => Results.Ok(user),
    onNone: () => Results.NotFound());
```

```csharp
// Different response types based on Some/None
return maybeProduct.Match(
    onSome: product => Results.Ok(new ProductResponse { Id = product.Id, Name = product.Name }),
    onNone: () => Results.NotFound(new { message = "Product not found" }));
```

---

## Tap / TapNone

Runs side effects without changing the `Maybe`.  
Useful for logging, metrics, or validation.

### Tap — runs on Some

```csharp
Maybe<int> maybeValue = Maybe.Some(42);

maybeValue
    .Tap(value => logger.LogInformation("Value is {V}", value))
    .Tap(value => metrics.RecordValue(value));

// Maybe is unchanged — Tap returns the same Maybe<T>
```

### TapNone — runs on None

```csharp
Maybe<User> maybeUser = repository.FindById(999);

maybeUser
    .TapNone(() => logger.LogWarning("User not found"));
```

### Chaining Tap with other operations

```csharp
Maybe<Order> result = repository.FindOrder(orderId)
    .Tap(order => logger.LogInformation("Found order: {Id}", order.Id))
    .TapNone(() => logger.LogWarning("Order not found"))
    .Map(order => order.ToResponse());
```

---

## ToEnumerable

Converts the `Maybe` to an `IEnumerable<T>` for LINQ queries.  
`Some(value)` yields one element; `None` yields nothing.

```csharp
Maybe<int> maybeValue = Maybe.Some(42);

var list = maybeValue.ToEnumerable().ToList();
Console.WriteLine(list.Count); // 1
Console.WriteLine(list[0]);    // 42
```

```csharp
Maybe<int> empty = Maybe.None<int>();

var list = empty.ToEnumerable().ToList();
Console.WriteLine(list.Count); // 0
```

### Using in LINQ queries

```csharp
var users = new[] { user1, user2, user3 };

var primaryEmails = users
    .Select(u => repository.GetPrimaryEmail(u.Id)) // Maybe<string>[]
    .SelectMany(m => m.ToEnumerable())             // IEnumerable<string>
    .ToList();

// Only the Some emails are included; None values are skipped
```

### Filtering collections

```csharp
var maybeValues = new[] 
{ 
    Maybe.Some(1), 
    Maybe.None<int>(), 
    Maybe.Some(3) 
};

var onlyValues = maybeValues
    .SelectMany(m => m.ToEnumerable())
    .ToList();

Console.WriteLine(onlyValues.Count); // 2
Console.WriteLine(string.Join(", ", onlyValues)); // "1, 3"
```

---

## Implicit Conversions

Allows returning values and `None` directly without explicit wrapping.

```csharp
// Value → Maybe<T> (implicit Some)
public Maybe<int> GetPositiveNumber(int n)
{
    if (n <= 0)
        return Maybe.None<int>(); // implicit None

    return n; // implicit Some — no need for Maybe.Some(n)
}
```

```csharp
// Useful in service methods
public Maybe<User> FindById(int id)
{
    var user = _db.Users.FirstOrDefault(u => u.Id == id);

    return user is null
        ? Maybe.None<User>() // implicit None
        : user;              // implicit Some
}
```

```csharp
// In one-liners
public Maybe<string> ValidateEmail(string email)
    => email.Contains("@") ? email : Maybe.None<string>();
```

---

## Chaining Examples

### Full service method

```csharp
public Maybe<UserResponse> GetUserById(int userId)
{
    return repository.FindById(userId)                    // Maybe<User>
        .Where(u => u.IsActive)                           // Filter
        .Tap(u => logger.LogInformation("Found: {Id}", u.Id))
        .Map(u => new UserResponse                        // Maybe<UserResponse>
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        });
}
```

### Composing multiple steps

```csharp
public Maybe<Order> ProcessOrder(int orderId)
{
    return repository.GetOrder(orderId)                    // Maybe<Order>
        .Where(o => o.Status == OrderStatus.Pending)       // Only pending
        .TapNone(() => logger.LogWarning("Order not found"))
        .Bind(order => repository.GetCustomer(order.CustomerId)  // Maybe<Customer>
            .Map(customer => EnrichOrder(order, customer))) // Enrich
        .Tap(order => logger.LogInformation("Order processed"))
        .Where(o => ValidateOrder(o));                     // Final validation
}
```

### Handling nested maybes

```csharp
// Get user, then their primary address, then validate the postcode
public Maybe<Address> GetValidUserAddress(int userId)
{
    return GetUser(userId)                   // Maybe<User>
        .FlatMap(u => GetPrimaryAddress(u.Id))  // Maybe<Address>
        .Where(a => ValidatePostcode(a.Postcode));
}
```

### Aggregating multiple maybes

```csharp
var maybeUser = repository.FindUser(userId);
var maybeSettings = repository.FindSettings(userId);

// Combine two maybes into one result
var combined = maybeUser
    .FlatMap(user => maybeSettings
        .Map(settings => new UserWithSettings { User = user, Settings = settings }));
```

---

## Integration with Services

### Repository pattern

```csharp
public interface IUserRepository
{
    Maybe<User> GetById(int id);
    Maybe<User> GetByEmail(string email);
    Maybe<IEnumerable<User>> GetByCompanyId(int companyId);
}

public class UserRepository : IUserRepository
{
    private readonly DbContext _db;

    public Maybe<User> GetById(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        return Maybe.FromNullable(user);
    }

    public Maybe<User> GetByEmail(string email)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);
        return Maybe.FromNullable(user);
    }

    public Maybe<IEnumerable<User>> GetByCompanyId(int companyId)
    {
        var users = _db.Users.Where(u => u.CompanyId == companyId).ToList();
        return users.Count > 0 ? Maybe.Some<IEnumerable<User>>(users) : Maybe.None<IEnumerable<User>>();
    }
}
```

### Service layer

```csharp
public interface IUserService
{
    Maybe<UserResponse> GetUserById(int id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public Maybe<UserResponse> GetUserById(int id)
    {
        return _repository.GetById(id)
            .Where(u => u.IsActive)
            .Tap(u => _logger.LogInformation("Retrieved user {Id}", u.Id))
            .TapNone(() => _logger.LogWarning("User {Id} not found", id))
            .Map(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });
    }
}
```

### Minimal API integration

```csharp
app.MapGet("/users/{id:int}", async (
    int id,
    IUserService service) =>
{
    return service.GetUserById(id)
        .Match(
            onSome: user => Results.Ok(user),
            onNone: () => Results.NotFound(new { message = "User not found" }));
});
```

```csharp
app.MapGet("/users/email/{email}", async (
    string email,
    IUserRepository repository) =>
{
    return repository.GetByEmail(email)
        .Map(u => new { u.Id, u.Name })
        .Match(
            onSome: dto => Results.Ok(dto),
            onNone: () => Results.NotFound());
});
```

### Error recovery with fallbacks

```csharp
public Maybe<UserSettings> GetUserSettings(int userId)
{
    return repository.GetSettings(userId)
        .Bind(settings => 
            // If the stored settings are incomplete, use defaults
            ValidateSettings(settings)
                ? Maybe.Some(settings)
                : GetDefaultSettings(userId));
}
```

---

## Quick Reference

| Method | Input | Output | When to use |
|---|---|---|---|
| `Some(value)` | `T` | `Maybe<T>` | Wrapping a value |
| `None()` | — | `Maybe<T>` | Creating an empty Maybe |
| `FromNullable(value)` | `T?` | `Maybe<T>` | Converting nullable to Maybe |
| `IsSome` | — | `bool` | Checking if value exists |
| `IsNone` | — | `bool` | Checking if empty |
| `Map(fn)` | `T → TOut` | `Maybe<TOut>` | Transform value |
| `FlatMap(fn)` | `T → Maybe<TOut>` | `Maybe<TOut>` | Chain step that can return None |
| `Bind(fn)` | `T → Maybe<TOut>` | `Maybe<TOut>` | Alias for FlatMap |
| `Where(predicate)` | `T → bool` | `Maybe<T>` | Filter based on condition |
| `Match(onSome, onNone)` | Two funcs | `TOut` | Consume both branches |
| `Tap(action)` | `T → void` | `Maybe<T>` | Side effect on Some |
| `TapNone(action)` | `() → void` | `Maybe<T>` | Side effect on None |
| `ToEnumerable()` | — | `IEnumerable<T>` | Use in LINQ queries |
| `GetOrElse(value)` | `T` | `T` | Extract with default |
| `GetOrElse(fn)` | `() → T` | `T` | Lazy default evaluation |
| `GetOrThrow()` | — | `T` | Extract or throw |
| `GetOrThrow(fn)` | `() → Exception` | `T` | Extract or throw custom |
| `ToNullable()` | — | `T?` | Convert to nullable |
