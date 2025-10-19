# Entity Framework Core

Scope: .NET > 8 apps using the official Microsoft Entity Framework Core packages and first‑party/provider docs. This ruleset is written to produce correct, modern EF Core code, with safe defaults, anti‑patterns to avoid, provider notes (SQL Server, PostgreSQL, MySQL), and domain‑model guidance.

---
## 1) Rules

- Install the **right provider** (`Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Pomelo.EntityFrameworkCore.MySql`, etc.) and **design tools** (`Microsoft.EntityFrameworkCore.Design`).
- Register `DbContext` in DI with the correct `Use{Provider}()` and connection string.
- Keep **DbContext lifetime** *scoped* (web) and **never share** a single instance across threads.
- For **read‑only** queries, prefer `AsNoTracking()`; for complex `Include` graphs, consider `AsSplitQuery()`.
- Use **eager loading** (`Include/ThenInclude`) to prevent **N+1**; avoid implicit lazy loading unless you fully understand its impact.
- Use **migrations** with proper naming; consider a **separate migrations project** and wire `MigrationsAssembly`.
- Extract mapping with `IEntityTypeConfiguration<TEntity>` and `ApplyConfigurationsFromAssembly(...)`.
- For **hot paths**, consider **compiled queries** (`EF.CompileQuery`) and **projection** to DTOs.
- Use **value converters** for custom types, and **concurrency tokens** for optimistic concurrency.
- Prefer EF Core directly over layering “generic repository + unit of work” unless adding real value.

---

## 2) NuGet Packages by Scenario

### Core
- `Microsoft.EntityFrameworkCore` — core EF Core APIs.
- `Microsoft.EntityFrameworkCore.Design` — required for **migrations** & design tooling.

### Providers (choose one or more)
- **SQL Server/Azure SQL**: `Microsoft.EntityFrameworkCore.SqlServer` (+ optional `Microsoft.Data.SqlClient` features).
- **PostgreSQL**: `Npgsql.EntityFrameworkCore.PostgreSQL` (JSON/JSONB, arrays, etc.).
- **MySQL/MariaDB**: `Pomelo.EntityFrameworkCore.MySql` (uses `MySqlConnector`).
- **SQLite (local/dev)**: `Microsoft.EntityFrameworkCore.Sqlite`.

### Optional integrations
- **Proxies for lazy loading**: `Microsoft.EntityFrameworkCore.Proxies` (use sparingly).
- **Spatial**: provider‑specific (e.g., `Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite`, `Pomelo.EntityFrameworkCore.MySql.NetTopologySuite`).

### Install examples

```bash
# Core + tools
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design

# SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# MySQL (Pomelo)
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

---
## 3) Registering the DbContext (Program.cs)

### SQL Server
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer"),
        sql => sql.MigrationsAssembly("YourProject.Migrations") // if using separate migrations project
    ));

var app = builder.Build();
app.Run();
```

### PostgreSQL (Npgsql)
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres"),
        npgsql => npgsql.MigrationsAssembly("YourProject.Migrations")
    ));
```

### MySQL (Pomelo)
```csharp
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")),
        my => my.MigrationsAssembly("YourProject.Migrations"))
);
```

**Rules**
- Web apps: register context as **Scoped** (default for `AddDbContext`).
- Worker/console: create scopes for background tasks; do **not** share a single `DbContext` across threads.
- Prefer **connection strings** via configuration (`appsettings.json`, secrets) and **parameterized** access (EF does this by default).

---
## 4) Model Configuration with `IEntityTypeConfiguration<TEntity>`

**Why**: Keep `OnModelCreating` small; centralize each entity’s mapping.

```csharp
public sealed class Order
{
    public int Id { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.Total).HasPrecision(18,2);
        b.HasMany(x => x.Lines).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
        // Examples: value converters, indexes, concurrency tokens, owned types, etc.
    }
}

public sealed class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

---
## 5) Migrations in a **Separate Project**

**Wire up migrations assembly** (see Program.cs examples above) and/or in `OnConfiguring`:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlServer(connString,
        sql => sql.MigrationsAssembly("YourProject.Migrations"));
```

**Design‑time factory** (for `dotnet ef` to create the context without the app running):
```csharp
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("<design-time-connection>",
                sql => sql.MigrationsAssembly("YourProject.Migrations"))
            .Options;
        return new AppDbContext(opts);
    }
}
```

**Commands**
```bash
# Add a migration into the migrations project (‑p) using the API project to resolve config (‑s)
dotnet ef migrations add Init --project YourProject.Migrations --startup-project YourProject.Api

# Update database using the same pairing
dotnet ef database update --project YourProject.Migrations --startup-project YourProject.Api
```

**Naming**: Use descriptive migration names (e.g., `AddOrderIndexes`, `RenameCustomerFullName`). Commit migration files to source control.

---

## 6) Querying Rules & Performance

### Tracking
- Default tracking is **On**. For read‑only flows: `AsNoTracking()`.
- If you need identity resolution without change tracking: `AsNoTracking().AsNoTrackingWithIdentityResolution()` is available, or set `QueryTrackingBehavior.NoTracking` globally if most queries are read‑only.

### Related data
- Prefer **eager loading**: `Include/ThenInclude` to avoid **N+1**.
- For large graphs or multiple collections, consider `AsSplitQuery()` to avoid row multiplication (“cartesian explosion”).
- Avoid global lazy loading; if you must enable proxies, be explicit and verify SQL.

### Projection
- Project to lightweight DTOs with `Select` to reduce materialization costs and payload size.

### Compiled queries (hot paths)
```csharp
static readonly Func<AppDbContext, int, Task<Order?>> getOrder =
    EF.CompileAsyncQuery((AppDbContext db, int id) =>
        db.Orders.Include(o => o.Lines).FirstOrDefault(o => o.Id == id));

var order = await getOrder(db, 123);
```
> Only for frequently executed, parameter‑only queries—benchmark first.

### Concurrency
- Configure **optimistic concurrency** via a token (e.g., `RowVersion` byte[] / `xmin` in Postgres):
```csharp
b.Property<byte[]>("RowVersion").IsRowVersion(); // SQL Server
// or
b.Property<uint>("xmin").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate(); // PostgreSQL system column
```
- Catch `DbUpdateConcurrencyException` and implement a merge/retry strategy.

### Value conversions & comparers
- Map enums as strings, custom value objects, strongly typed IDs, or encrypted values:
```csharp
b.Property(p => p.Currency)
 .HasConversion<string>();

b.Property(p => p.Tags) // List<string>
 .HasConversion(
     to => JsonSerializer.Serialize(to, (JsonSerializerOptions?)null),
     from => JsonSerializer.Deserialize<List<string>>(from)!)
 .Metadata.SetValueComparer(new ValueComparer<List<string>>(
     (a,b) => a!.SequenceEqual(b!), a => a!.Aggregate(0,(h,v)=>HashCode.Combine(h,v.GetHashCode())), a=>a!.ToList()));
```

---
## 7) Saving & Transactions
- Batch multiple changes and call `SaveChangesAsync()` **once** per unit of work.
- For multi‑context or cross‑resource operations, use **database transactions** or `IDbContextTransaction`.
- Avoid calling `SaveChangesAsync()` in tight loops; accumulate and save in batches.

---
## 8) Anti‑patterns (and Better Alternatives)
- ❌ **Generic repository + UoW wrappers** that only proxy `DbContext`/`DbSet` add little value and can hide EF features.
  - ✅ Use `DbContext` directly in application services. Introduce repositories **only** for cross‑aggregate abstractions or non‑EF data sources.
- ❌ **Long‑lived** or **shared** `DbContext` across threads.
  - ✅ Scoped lifetime per request/unit of work.
- ❌ Relying on **lazy loading** by default.
  - ✅ Be explicit with `Include` / `ThenInclude` / `AsSplitQuery()`.
- ❌ **Client evaluation** of heavy filters/joins.
  - ✅ Ensure expressions translate to SQL; use `AsEnumerable()` only after server‑side filtering.
- ❌ Over‑eager tracking.
  - ✅ `AsNoTracking()` for read‑only.
- ❌ Raw string concatenation in SQL.
  - ✅ `FromSqlInterpolated` / parameters; let EF parameterize for you.

---
## 9) Provider‑Specific Guidance

### PostgreSQL (Npgsql)
- Prefer **JSONB** for flexible, semi‑structured data; map via owned types or `JsonDocument`:
```csharp
b.OwnsOne(o => o.Metadata, nb => nb.ToJson()); // Npgsql owned‑type JSON mapping
```
- Use **array** and **range** types where appropriate; leverage GIN indexes for JSONB/arrays.
- Consider **materialized views** for expensive read‑models; refresh via scheduled jobs:
```sql
REFRESH MATERIALIZED VIEW CONCURRENTLY app.read_orders;
```
- Connection pooling is built‑in (ADO.NET); tune max pool size via connection string if needed.

### MySQL/MariaDB (Pomelo)
- Ensure **InnoDB** (FKs, transactions).
- Always specify **server version** (or use `ServerVersion.AutoDetect`).
- Design **indexes** for common predicates/sorts; watch out for case sensitivity/collations.
- Pooling is built‑in; prefer a single, shared `DbContext` per scope (not per query).

### SQL Server
- EF Core parameterizes queries by default; prefer `FromSqlInterpolated` over string concatenation for raw SQL.
- Use **filtered indexes**/**include columns** for heavy read scenarios.
- Stored procedures: use `FromSql` for result‑sets or `Database.ExecuteSqlRaw` for non‑query operations.

---
## 10) Patterns for Domain Modeling (DDD‑friendly)
- Treat EF Core as **persistence**; keep domain logic inside aggregates/entities.
- Use **owned types** for value objects (e.g., `Money`, `Address`).
- Prefer **strongly typed IDs** (via value converters) over raw `Guid`/`int` in the domain.
- Keep entities’ constructors valid; expose behavior methods (not only settable properties).
- Query with **DTO projections** for APIs; avoid exposing EF entities beyond the domain boundary.

---
## 11) Examples

### 11.1 Minimal API + EF Core with best‑practice defaults
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt
    .UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), npg => npg.MigrationsAssembly("YourProject.Migrations"))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // default read‑optimized; opt back in when needed

builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.MapGet("/orders/{id:int}", async (int id, OrderService svc, CancellationToken ct)
    => await svc.GetOrder(id, ct));

app.Run();
```

### 11.2 Service using compiled queries, projection, and split queries
```csharp
public sealed class OrderService
{
    private static readonly Func<AppDbContext, int, Task<OrderDto?>> GetOrderCompiled =
        EF.CompileAsyncQuery((AppDbContext db, int id) =>
            db.Orders
              .AsSplitQuery()
              .Where(o => o.Id == id)
              .Select(o => new OrderDto(
                  o.Id,
                  o.PlacedAt,
                  o.Lines.Select(l => new OrderLineDto(l.Sku, l.Quantity, l.Price)).ToList()))
              .FirstOrDefault());

    private readonly AppDbContext context;
    public OrderService(AppDbContext context) => this.context = context;

    public Task<OrderDto?> GetOrder(int id, CancellationToken ct)
        => GetOrderCompiled(context, id);
}
```

### 11.3 Stored procedure (read) and raw SQL (write)
```csharp
// Read via stored procedure returning entity shape
var recent = await db.Orders
    .FromSql($"EXEC dbo.GetRecentOrders @days={7}")
    .AsNoTracking()
    .ToListAsync(ct);

// Non‑query command (write)
await db.Database.ExecuteSqlRawAsync(
    "EXEC dbo.ApplyDiscounts @p0", parameters: new object[]{ 0.10m }, ct);
```

### 11.4 Concurrency token with retry
```csharp
try
{
    await db.SaveChangesAsync(ct);
}
catch (DbUpdateConcurrencyException ex)
{
    // Reload entries and retry or return a conflict to the caller
    foreach (var entry in ex.Entries)
    {
        await entry.ReloadAsync(ct);
    }
    // Decide whether to retry or surface a 409/merge result
}
```

---
## 12) Repository / Unit of Work — Pragmatic Guidance
- EF Core’s `DbContext` **is** a unit of work; `DbSet<T>` mirrors a repository.
- Only introduce repositories when:
  - You need to abstract across **multiple data sources**, or
  - You’re enforcing **aggregate boundaries** with custom operations, or
  - You target **ORM‑agnostic** tests/components.
- If you add repositories, don’t hide EF features (e.g., `Include`, `AsSplitQuery`, compiled queries). Prefer **specifications**/query objects that return `IQueryable<T>` when safe.

---
## 13) Testing & Migrations Hygiene
- Use **testcontainers** for provider‑faithful tests.
- Keep migrations **linear**; avoid rebasing history; add new migrations for model changes.
- Validate pending migrations at CI; run `dotnet ef migrations bundle` for deployment scripts if desired.

---
## 14) Copy‑Paste Snippets

**Apply all configurations**
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

**Global no‑tracking default**
```csharp
options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
```

**Value converter for strongly typed ID**
```csharp
b.Property(x => x.CustomerId)
 .HasConversion(id => id.Value, value => new CustomerId(value))
 .ValueGeneratedNever();
```

**Npgsql JSONB owned type mapping**
```csharp
b.OwnsOne(o => o.Metadata, nb => nb.ToJson());
```

**Split queries**
```csharp
var orders = await db.Orders
    .Include(o => o.Lines)
    .Include(o => o.Customer)
    .AsSplitQuery()
    .ToListAsync();
```

---
## 15) Naming & Conventions
- Tables: snake_case (pg) or PascalCase (SQL Server) — be consistent.
- Keys: `{Entity}Id`; shadow FKs only when necessary.
- Migrations: `YYYYMMDDHHmm_Description` (or semantic names like `AddOrderIndexes`).

---
## 16) Assistant Prompts (embed into AI tools)
- “Use `IEntityTypeConfiguration<T>` for all mapping, and call `ApplyConfigurationsFromAssembly`.”
- “For read endpoints, default to `AsNoTracking()`; project to DTOs; use `AsSplitQuery()` when including multiple collections.”
- “When recommending repositories, justify **why** (cross data source, aggregate invariants) or prefer direct `DbContext`.”
- “If user asks to store flexible data on Postgres, recommend JSONB with owned types and GIN indexes.”
- “Place migrations in a dedicated project and set `MigrationsAssembly` in both DI and design‑time factory.”
