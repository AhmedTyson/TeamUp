using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeamUp.Application;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<TeamUp.Api.Filters.ValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TeamUp.Api.Middleware.GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<TeamUp.Application.Common.Security.JwtOptions>>((options, jwtOpt) =>
    {
        var opt = jwtOpt.Value;
        var key = Encoding.UTF8.GetBytes(opt.Key);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = opt.Issuer,
            ValidAudience = opt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdminOnly", p => p.RequireRole("PlatformAdmin"));
    options.AddPolicy("OrgAdminOrPlatformAdmin", p => p.RequireRole("OrgAdmin", "PlatformAdmin"));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth-policy", opt =>
    {
        opt.PermitLimit = 60; // 60 requests per minute per window
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

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
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

public partial class Program;
