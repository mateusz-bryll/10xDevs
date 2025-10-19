# Structured, Semantic Logging in .NET with Serilog

Produce correct, safe, and maintainable structured logging code for .NET apps (Console, Worker, ASP.NET Core, Minimal APIs), using Serilog.

---

## 1) Rules

### 1. Prefer structured events over strings
    - Use **message templates** with named properties: `Log.Information("Processed {OrderId} in {Elapsed:0.000}s", orderId, elapsed)`.
    - Never use string concatenation or interpolation for variable data.
        
### 2. Consistent property names
    - Pick domain names once and reuse: `OrderId`, `UserId`, `TenantId`, `CorrelationId`.
        
### 3. Right level, right place
    - `Verbose`/`Debug` for diagnostics; `Information` for business milestones; `Warning` for abnormal but handled; `Error` for failures; `Fatal` for process-terminating conditions.
    
### 4. Separation of concerns
    - Configure sinks/formatting centrally (Program.cs/appsettings.json). Do not set up loggers ad-hoc in random classes.
        
### 5. Performance
    - Use asynchronous sinks where appropriate and avoid blocking I/O in hot paths.
        
### 6. Privacy & security
    - Never log secrets, tokens, full PII, or payloads unless explicitly allowed and scrubbed.
        
### 7. Observability
    - Include correlation/causation identifiers and high-value metadata via enrichers or scopes.
        

---

## 2) Required NuGet Packages (choose per scenario)

| Scenario                             | Package                                                                                  | Purpose                                                                        |
| ------------------------------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Core Serilog API                     | `Serilog`                                                                                | Base logging API & message templates                                           |
| ASP.NET Core integration             | `Serilog.AspNetCore`                                                                     | Routes **Microsoft.Extensions.Logging** to Serilog; request logging middleware |
| Configuration via `appsettings.json` | `Serilog.Settings.Configuration`                                                         | Reads `Serilog` section from configuration                                     |
| Console sink                         | `Serilog.Sinks.Console`                                                                  | Write to console (with output template or JSON)                                |
| File sink                            | `Serilog.Sinks.File`                                                                     | Write to rolling files                                                         |
| Seq (local/hosted)                   | `Serilog.Sinks.Seq`                                                                      | Ship events to Seq                                                             |
| Async wrapper                        | `Serilog.Sinks.Async`                                                                    | Offload sink writes to background thread                                       |
| Compact JSON format                  | `Serilog.Formatting.Compact`                                                             | Dense JSON (`CompactJsonFormatter`)                                            |
| Expressions (filters/format)         | `Serilog.Expressions`                                                                    | Expression-based filtering/formatting                                          |
| Exception details                    | `Serilog.Exceptions`                                                                     | Enrich events with structured exception data                                   |
| Enrichers (examples)                 | `Serilog.Enrichers.Environment`, `Serilog.Enrichers.Process`, `Serilog.Enrichers.Thread` | Host, process, thread info                                                     |
| Correlation Id (optional)            | `Serilog.Enrichers.CorrelationId` or custom middleware                                   | Adds `CorrelationId` property                                                  |

> Add sinks/enrichers only when actually used.

---

## 3) App Startup Patterns (DI-first)

### 3.1 Modern hosting (ASP.NET Core/.NET 8+)

**Program.cs**
```csharp
using Serilog;
using Serilog.Exceptions; // if using Serilog.Exceptions
  
var builder = WebApplication.CreateBuilder(args);

// 1) Bootstrap logger for very early startup errors (optional but recommended)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

// 2) Read full configuration from appsettings.json
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services) // optional: pull enrichers from DI
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails() // if Serilog.Exceptions installed
);

// Add services
builder.Services.AddControllers();

var app = builder.Build();

// Request logging summary event (ASP.NET Core middleware)
app.UseSerilogRequestLogging(options =>
{
    // example: add extra properties to request completion event
    options.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("RequestHost", http.Request.Host.Value);
        diag.Set("UserAgent", http.Request.Headers["User-Agent"].ToString());
    };
});

app.MapControllers();
app.Run();
```

### 3.2 Generic host / worker services

```csharp
using Serilog;

Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext())
    .ConfigureServices(services =>
     {
        services.AddHostedService<Worker>();
     })
    .Build()
    .Run();
```

> Prefer `HostBuilder.UseSerilog()` or `IServiceCollection.AddSerilog()` over obsolete `IWebHostBuilder.UseSerilog()`.

---

## 4) Configuration via `appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ 
      "FromLogContext", 
      "WithMachineName", 
      "WithProcessId", 
      "WithThreadId" 
    ],
    "WriteTo": [
      {
        "Name": "Async",
        "Args": {
          "configure": [
            { 
              "Name": "Console", 
              "Args": { 
                "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}" 
              }
            },
            { 
              "Name": "File", 
              "Args": { 
                "path": "logs/app-.log", 
                "rollingInterval": "Day" 
              }
            },
            { 
              "Name": "Seq", 
              "Args": { 
                "serverUrl": "http://localhost:5341" 
              } 
            }
          ]
        }
      }
    ]
  }
}
```

**Notes**
- Wrap sinks with `Async` to reduce contention in busy apps.
- For compact JSON files, use `"formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"`.

---

## 5) Using `ILogger<T>` via DI (recommended)

```csharp
public class PaymentService
{
    private readonly ILogger<PaymentService> logger;

    public PaymentService(ILogger<PaymentService> logger) => this.logger = logger;

    public async Task HandleAsync(Guid orderId, decimal amount)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["OrderId"] = orderId,
            ["Component"] = nameof(PaymentService)
        });

        logger.LogInformation("Starting payment of {Amount}", amount);
        try
        {
            // ... do work
            logger.LogInformation("Payment completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Payment failed");
            throw;
        }
    }
}
```

> Scopes flow to Serilog as structured properties when Serilog is configured with `Enrich.FromLogContext()` (and when using `Serilog.AspNetCore`/`Serilog.Extensions.Logging`).

---

## 6) Serilog API (direct use when DI not available)

```csharp
using Serilog;

Log.Information("Processed {Items} items", count);

try
{
    // ...
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled failure while processing {JobId}", jobId);
}
```

> Prefer `ILogger<T>` from DI in app code. Reserve static `Log` for early bootstrapping or infrastructure utilities.

---

## 7) Correlation & Context

### 7.1 Include correlation ID in all events

- Option A: **Middleware +** `**LogContext**`
```csharp
app.Use(async (ctx, next) =>
{
    var correlationId = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        ctx.Response.Headers["X-Correlation-ID"] = correlationId;
        await next();
    }
});
```
- Option B: **Package**
    - Use a correlation-id enricher package to avoid custom code; then call `loggerConfiguration.Enrich.WithCorrelationId()` and ensure the middleware is added.
        
### 7.2 Enrich request completion event

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("EndpointName", http.GetEndpoint()?.DisplayName);
        diag.Set("ContentType", http.Response.ContentType);
    };
});
```

---

## 8) Filtering and Routing

- Use **minimum level overrides** to silence noisy namespaces (e.g., `Microsoft`, `System`).
- Use `**Serilog.Expressions**` to filter to sinks:
```json
{
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "logs/requests-.json",
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      },
      "Filter": [
        {
          "Name": "ByIncludingOnly",
          "Args": {
            "expression": "SourceContext = 'Serilog.AspNetCore.RequestLoggingMiddleware'"
          }
        }
      ]
    }
  ]
}
```

---

## 9) Formatting Output

- Text sinks accept **output templates**:
```json
{
    "WriteTo": [
        { "Name": "Console", "Args": { "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}" } }
    ]
}
```

- For machine-readability and lower bandwidth, prefer **compact JSON** for files/shipments.
    
---

## 10) Error Handling Best Practices

- Always pass the `Exception` as the first argument: `Log.Error(ex, "Message")`.
- Add relevant **structured properties** alongside the exception.
- Consider `Serilog.Exceptions` to capture exception object graphs into properties.
- Avoid swallowing exceptions after logging; rethrow when appropriate.
    
---

## 11) Antipatterns to Avoid (with fixes)

1. **Interpolated strings**
    - ❌ `Log.Information($"User {userId} logged in")`     
    - ✅ `Log.Information("User {UserId} logged in", userId)`
        
2. **Concatenation**
    - ❌ `"Processed: " + id`
    - ✅ `Log.Information("Processed {Id}", id)`
        
3. **Logging sensitive data**
    - ❌ Passwords, tokens, full payloads/body by default
    - ✅ Redact or hash; opt-in logging; truncate large values
        
4. **Heavy synchronous I/O in hot paths**
    - ❌ File sink without buffering under high load
    - ✅ Wrap sinks with `Async`; prefer batching/network sinks
        
5. **No correlation**
    - ❌ Missing `CorrelationId`/`RequestId`
    - ✅ Push via `LogContext` or scopes; include in templates
        
6. **Over-logging**
    - ❌ Verbose in production, duplicate logs (e.g., request logging + global handler logging the same exception)
    - ✅ Tune levels; use filters; deduplicate pipelines
        
7. **Creating loggers manually per class**
    - ❌ `new LoggerConfiguration().CreateLogger()` sprinkled across code
    - ✅ Use DI `ILogger<T>` everywhere
        
8. **Using** `**ToString()**` **for complex objects**
    - ❌ `Log.Information("{@Order}", order.ToString())`
    - ✅ Use destructuring: `Log.Information("{@Order}", order)`
        
9. **Forgetting** `**Enrich.FromLogContext()**`
    - ❌ Scopes/correlation not appearing
    - ✅ Add `Enrich.FromLogContext()` in configuration
        
---

## 12) End-to-end Examples

### 12.1 Minimal API with structured logging, correlation, and compact JSON

**Program.cs**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails());

var app = builder.Build();

app.Use(async (http, next) =>
{
    var cid = http.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    using (LogContext.PushProperty("CorrelationId", cid))
    {
        http.Response.Headers["X-Correlation-ID"] = cid;
        await next();
    }
});
app.UseSerilogRequestLogging();

app.MapGet("/orders/{id}", (ILoggerFactory lf, int id) =>
{
    var log = lf.CreateLogger("Orders");
    log.LogInformation("Fetching order {OrderId}", id);

    return Results.Ok(new { Id = id, Status = "Processing" });
});

app.Run();
```

**appsettings.json (logging part)**
```json
{
  "Serilog": {
    "MinimumLevel": { 
      "Default": "Information", 
      "Override": { 
        "Microsoft": "Warning" 
      } 
    },
    "Enrich": [
      "FromLogContext", 
      "WithMachineName"
    ],
    "WriteTo": [
      { 
        "Name": "Async", 
        "Args": { 
          "configure": [
            { 
              "Name": "Console", 
              "Args": { 
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact" 
              } 
            },
            { 
              "Name": "File", 
              "Args": { 
                "path": "logs/app-.clef", 
                "rollingInterval": "Day", 
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact" 
              } 
            }
          ]
        }
      }
    ]
  }
}
```

### 12.2 Worker service with DI scopes

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    public Worker(ILogger<Worker> logger) => this.logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (logger.BeginScope(new { JobId = Guid.NewGuid() }))
            {
                logger.LogInformation("Heartbeat");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

---

## 13) Troubleshooting Tips

- Exceptions logged twice? Check request logging + global exception handler duplication; deduplicate.
- Scope properties missing? Confirm `Enrich.FromLogContext()` and that logs are written through Serilog (not a different provider).
- JSON formatter type names in config must be fully-qualified (assembly-qualified type).
- High CPU / blocked threads? Use `Serilog.Sinks.Async` or batch-capable sinks.

---

## 14) At-a-glance Snippets

**Install packages (example)**
```bash
dotnet add package Serilog

dotnet add package Serilog.AspNetCore

dotnet add package Serilog.Settings.Configuration

dotnet add package Serilog.Sinks.Console

dotnet add package Serilog.Sinks.File

dotnet add package Serilog.Sinks.Seq

dotnet add package Serilog.Sinks.Async

dotnet add package Serilog.Formatting.Compact

// optional

dotnet add package Serilog.Exceptions
```

**Message template dos & don’ts**
```csharp
// Do
logger.LogInformation("Shipped {OrderId} with {ItemCount} items", orderId, items.Count);

// Don’t
logger.LogInformation($"Shipped {orderId} with {items.Count} items");
```

**Destructuring complex objects**
```csharp
logger.LogInformation("Order details {@Order}", order); // use @ to capture object graph
```