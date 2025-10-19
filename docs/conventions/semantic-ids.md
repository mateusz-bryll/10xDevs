# Strongly Typed IDs in .NET with StronglyTypedId

This document provides production-ready patterns for replacing primitive obsession in entity identifiers with strongly typed IDs. Instead of using raw `Guid`, `int`, or `string` values throughout your codebase, strongly typed IDs enforce compile-time type safety while maintaining simple serialization and persistence. Prefer using StronglyTypedId NuGet package instead custom implementation.

Goals:
- Enforce type safety for entity identifiers (avoid primitive obsession).
- Keep persistence and serialization simple and predictable.
- Provide copy‑paste‑ready code for EF Core, ASP.NET Core (System.Text.Json / Newtonsoft.Json), Redis, and Cosmos DB.
- Prevent common antipatterns when generating or refactoring code.
- Eliminate ID-related bugs at compile time
- Domain-driven design with rich entity models
- Microservices with clear API contracts
- Applications requiring type safety without performance overhead

---

## 1) Rules

### 1. Always model IDs as dedicated types, not `Guid`/`int`/`string` directly.

### 2. Use StronglyTypedId source generator to avoid boilerplate and ensure consistent equality, parsing, and JSON conversion.

### 3. Default backing type = `Guid`, unless requirements dictate `int`, `long`, or `string`.

### 4. Expose ID properties as the strongly typed type in domain, EF entities, DTOs, commands, queries, and API contracts.

### 5. Serialize IDs as their underlying primitive (string/number/Guid) in JSON. Do **not** leak nested structures.

### 6. Map IDs to a single column in EF Core using a value converter (template-provided or custom).

### 7. Store IDs in Redis/Cosmos as primitives (keys/fields/JSON), do not nest custom object payloads.

### 8. Prefer System.Text.Json; fall back to Newtonsoft.Json only when required by dependencies or policies.

### 9. Keep ID types immutable and comparable; never add domain behavior to an ID type.

### 10. Do not depend on runtime reflection for `[StronglyTypedId]` detection (attribute usage is conditional and not emitted).

---

## 2) Packages & Setup

### 2.1 Required NuGet packages

- **Core generator (required):**
  - `StronglyTypedId` (source generator & attributes)
- **Optional templates & integrations:**
  - `StronglyTypedId.Templates` (adds ready‑made templates such as `*-efcore`, `*-newtonsoftjson`, `*-dapper`)
  - Newtonsoft integration if needed (via templates): `*-newtonsoftjson`
  - EF Core integration (via templates): `*-efcore`
  - Dapper integration (via templates): `*-dapper`

> Note: System.Text.Json support ships with `StronglyTypedId` out of the box via a `JsonConverter` for the strongly-typed ID.

### 2.2 csproj reference (recommended)

```xml
<ItemGroup>
  <PackageReference Include="StronglyTypedId" Version="1.*" PrivateAssets="all" ExcludeAssets="runtime" />
  <!-- Optional: community templates package -->
  <PackageReference Include="StronglyTypedId.Templates" Version="1.*" PrivateAssets="all" />
</ItemGroup>
```

**Why `PrivateAssets` & `ExcludeAssets`?**
- Prevents transitive package flow to referencing projects.
- Avoids copying StronglyTypedId attribute DLLs to runtime output (not required at runtime).

### 2.3 SDK requirement

- Build with **.NET 7+ SDK** (can target older frameworks if needed).

### 2.4 Embedding attributes (optional)

If you want the attributes embedded in your assembly, set:
```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);STRONGLY_TYPED_ID_EMBED_ATTRIBUTES</DefineConstants>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="StronglyTypedId" Version="1.*"
                    PrivateAssets="all" ExcludeAssets="runtime;compile" />
</ItemGroup>
```

---

## 3) Defining Strongly Typed IDs

### 3.1 Default (Guid) ID

```csharp
using StronglyTypedIds;

[StronglyTypedId] // defaults to Guid
public partial struct OrderId {}

```

### 3.2 Choose a different backing type

```csharp
using StronglyTypedIds;

[StronglyTypedId(Template.Int)]
public partial struct ProductId {}

[StronglyTypedId(Template.Long)]
public partial struct InvoiceId {}

[StronglyTypedId(Template.String)]
public partial struct ExternalRefId {}
```

### 3.3 Project-wide defaults

```csharp
// Assembly-level default to Int for all IDs
[assembly: StronglyTypedIdDefaults(Template.Int)]

[StronglyTypedId] public partial struct UserId {}
[StronglyTypedId] public partial struct RoleId {}

// Override per type
[StronglyTypedId(Template.Guid)] public partial struct SessionId {}
```

### 3.4 Add extra capabilities via templates

```csharp
// Combine a built-in template with custom ones (from StronglyTypedId.Templates)
[StronglyTypedId(Template.Guid, "guid-efcore", "guid-newtonsoftjson")]

public partial struct CustomerId {}
```

---

## 4) Using IDs in Your Domain

- Treat ID types as **value objects** that only wrap the primitive value.
- **Do not** add domain logic; keep them thin and immutable.

**Example domain entity**
```csharp
public sealed class Customer
{
    public CustomerId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = Email.Empty; // example other value object

    private Customer() { } // EF Core

    public Customer(CustomerId id, string name, Email email)
    {
        Id = id; Name = name;
        Email = email;
    }
}

```

---

## 5) EF Core Integration

### 5.1 Value converter via template

If you added `"*-efcore"` templates (e.g., `guid-efcore`), each ID type gets a nested `EfCoreValueConverter`.

**DbContext configuration**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Map ID to a single column of the underlying type
    modelBuilder.Entity<Customer>(b =>
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
         .HasConversion(new CustomerId.EfCoreValueConverter())
         .ValueGeneratedNever(); // typical for assigned Guid
    });
}

```

### 5.2 Generic convention (no template)

```csharp
public class StronglyTypedIdConverter<TId, TPrimitive> : ValueConverter<TId, TPrimitive>
    where TId : struct
{
    public StronglyTypedIdConverter(
        Expression<Func<TId, TPrimitive>> to,
        Expression<Func<TPrimitive, TId>> from)
        : base(to, from) { }
}

protected override void ConfigureConventions(ModelConfigurationBuilder b)
{
    // Example for Guid-backed IDs repeatedly used
    b.Properties<OrderId>().HaveConversion(
        new StronglyTypedIdConverter<OrderId, Guid>(x => x.Value, v => new OrderId(v)));
}
```

### 5.3 Scalar configuration guidance

- Map IDs as PKs/FKs to the corresponding primitive database type.
- Avoid shadow properties; define FK properties as the strongly typed ID.
- For **owned types**, continue to map the ID as a scalar.

---

## 6) ASP.NET Core JSON (System.Text.Json / Newtonsoft.Json)

### 6.1 System.Text.Json (default)

`StronglyTypedId` ships a `JsonConverter<TId>` for each ID; serialization is seamless.

**Controller example**
```csharp
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<CustomerDto> Get(CustomerId id)
        => Ok(new CustomerDto(id, "Alice"));
}

public record CustomerDto(CustomerId Id, string Name);
```

**Result JSON**
```json
{ "id": "e9d7a9a7-88b1-4b03-9a98-3fd1a2a4d0a1", "name": "Alice" }
```

**Note:** If you use custom naming (camelCase), configure `JsonOptions` as usual; the ID converter still writes the primitive value.

### 6.2 Newtonsoft.Json

If your pipeline requires Newtonsoft, include the `*-newtonsoftjson` template for each ID.
```csharp
services.AddControllers()
        .AddNewtonsoftJson();
```

IDs will serialize to their primitive values (string/number/Guid) using the generated converter.

---

## 7) REST API Contracts

- Accept and return IDs **as their strongly typed type** in action signatures and DTOs.
- **Model binding** works via the generated `TypeConverter` and `IParsable<T>`, so route parameters like `/api/orders/{id}` bind to your ID type.
- **Never** expose `{ value: "..." }` wrapper objects in JSON; keep IDs flat (string/guid/number).

**Minimal API example**
```csharp
app.MapGet("/orders/{id}", (OrderId id) => Results.Ok(new { id }));
```

---

## 8) Redis Usage (StackExchange.Redis)

### 8.1 Keys & fields

- Use the primitive string representation of your ID for keys/fields.
- Prefer namespaces: `customer:{id}`, `order:{id}:lines`.

**Examples**
```csharp
var key = $"customer:{customerId}"; // relies on ToString() of the ID
await db.StringSetAsync(key, JsonSerializer.Serialize(customer));

// Hash field example
await db.HashSetAsync("orders", new HashEntry[] { new(orderId.ToString(), orderJson) });
```

### 8.2 Recommendations

- Keep keys reasonably short but descriptive (`entity:{id}`), avoid whitespace; use `:` as a separator.
- For GUIDs, prefer **lowercase, no braces**.
- Avoid storing IDs as nested objects in Redis—store the primitive.

---

## 9) Azure Cosmos DB Usage

### 9.1 Basics

- Cosmos items require an `id` property (lowercase). Ensure your DTO/entity serializes the ID to a string when using GUID/string backing.
- For System.Text.Json or Newtonsoft, the ID converter emits the primitive; prefer a string `id` for consistency.

**Example item**
```csharp
public sealed class CustomerDocument
{
    public string id { get; init; } = default!; // Cosmos requires lower-case id
    public CustomerId CustomerId { get; init; } // duplicated domain ID if needed
    public string Name { get; init; } = string.Empty;
}

// Creating document
var doc = new CustomerDocument { id = customerId.ToString(), CustomerId = customerId, Name = "Alice" };
```

### 9.2 SDK considerations

- The v3 SDK defaults to Newtonsoft; v4 uses System.Text.Json. Both work—ensure your serializer is configured consistently if you customize.
- Partition keys: if you use an ID as the partition key, use its primitive value (usually string).

---

## 10) Testing & Tooling

- Unit-test parsing/formatting boundaries (e.g., invalid GUIDs) using `IParsable` and `TryParse`.
- Verify JSON round-trips for both System.Text.Json and Newtonsoft (if used).
- Generate custom templates (`*.typedid`) for repeated policies (e.g., EF Core value converters, Dapper handlers).

---

## 11) Antipatterns to Avoid

### 1. Using raw primitives (e.g., `Guid` PKs) in public APIs or domain entities.

### 2. Wrapping IDs in nested JSON objects like `{ "id": { "value": "..." } }`.

### 3. Losing value semantics: adding mutable state or behavior to ID types.

### 4. Custom `ToString()` formatting that isn’t reversible (stick to default formats).

### 5. Manual JSON converters when the generator already provides them (avoid duplication).

### 6. Multiple columns for one ID in EF Core (should be one scalar column).

### 7. Casting or reflection hacks to extract underlying values—use generated APIs and converters instead.

### 8. Inconsistent casing of Cosmos `id` property or forgetting it entirely.

### 9. Using IDs as Redis values when you meant keys (be explicit about key naming conventions).

### 10. Relying on attribute reflection at runtime (attributes are conditional and trimmed).

---

## 12) End-to-End Example

### 12.1 ID definitions

```csharp
using StronglyTypedIds;

[StronglyTypedId(Template.Guid, "guid-efcore", "guid-newtonsoftjson")]
public partial struct CustomerId {}

[StronglyTypedId(Template.Int, "int-efcore")]
public partial struct OrderId {}

```

### 12.2 Entity + DbContext

```csharp
public sealed class Order

{
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
}

public sealed class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasConversion(new OrderId.EfCoreValueConverter());
            b.Property(x => x.CustomerId).HasConversion(new CustomerId.EfCoreValueConverter());
            b.Property(x => x.PlacedAt);
        });
    }
}

```

### 12.3 Minimal API + JSON

```csharp
var app = WebApplication.Create(args);
app.MapPost("/orders", (OrderId id, CustomerId customerId) =>
{
    return Results.Ok(new { id, customerId });
});

app.Run();
```

**JSON response**

```json
{ "id": 42, "customerId": "a3a8e42a-85d6-4c47-bc2f-1c8d7b9a6d0c" }
```

### 12.4 Redis

```csharp
var key = $"order:{order.Id}"; // e.g., order:42
await db.StringSetAsync(key, JsonSerializer.Serialize(orderDto));
```

### 12.5 Cosmos DB (DTO)

```csharp
public sealed class OrderDocument
{
    public string id { get; init; } = default!; // required by Cosmos
    public OrderId OrderId { get; init; }
    public CustomerId CustomerId { get; init; }
}

var doc = new OrderDocument
{
    id = order.Id.ToString(),
    OrderId = order.Id,
    CustomerId = order.CustomerId
};

```

---

## 13) Assistant Prompts & Checklists (for AI tools)

### When creating an entity: define `{Name}Id`, choose backing type, add `[StronglyTypedId(...)]`, ensure EF converter mapping.

### When exposing APIs: accept `{Name}Id` in routes, ensure JSON options consistent, return IDs as primitives.

### When persisting: configure EF conversions (or use template converters), ensure PK/FK types are the strongly typed IDs.

### When caching: use `entity:{id}` key naming; store primitives in fields.

### When using Cosmos: set lowercase `id` string; keep partition keys simple and stable.
