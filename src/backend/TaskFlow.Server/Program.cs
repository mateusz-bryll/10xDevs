using Microsoft.EntityFrameworkCore;
using TaskFlow.Modules.Users;
using TaskFlow.Modules.WorkItems;
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

builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddWorkItemsModule(builder.Configuration);

var app = builder.Build();

app.UseWorkItemsModule();

app.Run();