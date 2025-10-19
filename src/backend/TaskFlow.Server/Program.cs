using Microsoft.EntityFrameworkCore;
using TaskFlow.Modules.WorkItems.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register WorkItems DbContext
builder.Services.AddDbContext<WorkItemsDatabaseContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("TaskFlow.Server")
    )
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();