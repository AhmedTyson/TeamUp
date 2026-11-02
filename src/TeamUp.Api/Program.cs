using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TeamUp.Api.Middleware.GlobalExceptionHandler>();

// Skip DB wiring when running under the test host without a connection string (health/smoke tests).
if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("TeamUpDb")))
    builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    if (app.Configuration.GetValue("Seed:Enabled", false))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var password = app.Configuration["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword is required when Seed:Enabled=true (use user-secrets).");
        var hash = new PasswordHasher<Student>().HashPassword(null!, password);
        await DbSeeder.SeedAsync(db, hash);
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

public partial class Program;
