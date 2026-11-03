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

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = TeamUp.Application.Common.Models.BaseResponse.Fail(
                    "Authentication is required. Please supply a valid Bearer token.",
                    StatusCodes.Status401Unauthorized);
                await context.Response.WriteAsJsonAsync(payload);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = TeamUp.Application.Common.Models.BaseResponse.Fail(
                    "Access denied. You do not have permission to access this resource.",
                    StatusCodes.Status403Forbidden);
                await context.Response.WriteAsJsonAsync(payload);
            }
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
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.ContentType is null || !response.ContentType.Contains("application/json"))
    {
        response.ContentType = "application/json; charset=utf-8";
        var msg = response.StatusCode switch
        {
            401 => "Authentication is required. Please supply a valid Bearer token.",
            403 => "Access denied. You do not have permission to access this resource.",
            404 => $"The requested route '{context.HttpContext.Request.Path}' was not found.",
            405 => "HTTP method not allowed on this endpoint.",
            429 => "Rate limit exceeded. Please slow down and try again shortly.",
            _ => $"HTTP error {response.StatusCode}."
        };
        var payload = TeamUp.Application.Common.Models.BaseResponse.Fail(msg, response.StatusCode);
        await response.WriteAsJsonAsync(payload);
    }
});

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
