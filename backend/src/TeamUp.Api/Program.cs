// Date: 2026-11-21
// Purpose: ASP.NET Core 10 API bootstrap enforcing STRIDE security controls (startup secret validation, proxy hardening, global/tiered rate limiting, SecurityStamp JWT validation, strict CORS, security headers, and health probes).

using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TeamUp.Api.Middleware;
using TeamUp.Application;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// STRIDE THR-S-02: trust X-Forwarded-For / X-Forwarded-Proto ONLY from explicitly configured proxies/networks.
// Default loopback proxies are cleared so untrusted callers cannot spoof client IPs to bypass rate limiting.
builder.Services.AddOptions<ForwardedHeadersOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        options.ForwardLimit = config.GetValue("ForwardedHeaders:ForwardLimit", 1);

        var knownProxies = config.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        foreach (var proxy in knownProxies)
        {
            if (IPAddress.TryParse(proxy, out var ip))
                options.KnownProxies.Add(ip);
        }

        var knownNetworks = config.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [];
        foreach (var cidr in knownNetworks)
        {
            if (System.Net.IPNetwork.TryParse(cidr, out var network))
                options.KnownIPNetworks.Add(network);
        }

        options.ForwardedHeaders = (options.KnownProxies.Count > 0 || options.KnownIPNetworks.Count > 0)
            ? ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            : ForwardedHeaders.None;
    });

// STRIDE THR-I-02: strict explicit CORS policy (exact origins, explicit HTTP methods and headers)
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var allowedOrigins = configuredOrigins is { Length: > 0 }
    ? configuredOrigins
    : (builder.Environment.IsDevelopment()
        ? ["http://localhost:5173", "http://localhost:4173", "http://127.0.0.1:5173", "http://127.0.0.1:4173"]
        : []);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
                .AllowCredentials();
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();
builder.Services.AddScoped<TeamUp.Application.Common.Interfaces.IProfilePhotoStorage, TeamUp.Api.Services.LocalProfilePhotoStorage>();

builder.Services.AddControllers(options => options.Filters.Add<TeamUp.Api.Filters.ValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "TeamUp API";
        document.Info.Version = "v1";
        document.Info.Description = "TeamUp Student Collaboration, Organizations, Events & Gamification REST API (.NET 10).";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Supply a valid JWT access token obtained from POST /api/v1/auth/login."
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((options, jwtOpt) =>
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
            // STRIDE THR-E-02: immediately reject tokens whose SecurityStamp, active status, or role no longer match persistence
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                if (principal is null)
                {
                    context.Fail("Invalid token principal.");
                    return;
                }

                var studentId = principal.GetStudentId();
                var tokenStamp = principal.GetSecurityStamp();
                var tokenRole = principal.GetUserRole();

                if (studentId <= 0 || tokenStamp is null)
                {
                    context.Fail("Token is missing required security claims.");
                    return;
                }

                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<TeamUp.Application.Common.Caching.ISecurityStampValidator>();
                var currentState = await validator.GetActiveStudentStateAsync(studentId, context.HttpContext.RequestAborted);
                if (currentState is null
                    || currentState.SecurityStamp != tokenStamp.Value
                    || !string.Equals(currentState.Role, tokenRole, StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("Session has been revoked or account permissions changed. Please sign in again.");
                }
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = BaseResponse.Fail(
                    "Authentication is required. Please supply a valid Bearer token.",
                    StatusCodes.Status401Unauthorized);
                await context.Response.WriteAsJsonAsync(payload);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = BaseResponse.Fail(
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

// STRIDE THR-S-02 / THR-D-01 / THR-I-01: global + tiered rate limiting using sanitized RemoteIpAddress ONLY (never raw X-Forwarded-For)
builder.Services.AddRateLimiter();
builder.Services.AddOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>()
    .Configure<Microsoft.Extensions.Options.IOptions<RateLimitingOptions>>((options, rlOpt) =>
    {
        var rateLimits = rlOpt.Value;
        var rateWindow = TimeSpan.FromSeconds(Math.Max(1, rateLimits.WindowSeconds));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, ct) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
            }
            else
            {
                context.HttpContext.Response.Headers.RetryAfter = rateLimits.WindowSeconds.ToString();
            }

            var payload = BaseResponse.Fail(
                "Rate limit exceeded. Please slow down and try again shortly.",
                StatusCodes.Status429TooManyRequests);
            await context.HttpContext.Response.WriteAsJsonAsync(payload, ct);
        };

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
                return RateLimitPartition.GetNoLimiter("health");

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var studentId = httpContext.User.GetStudentId();
                var userPartition = studentId > 0 ? $"user:{studentId}" : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
                return RateLimitPartition.GetFixedWindowLimiter(userPartition, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimits.AuthenticatedPermitLimit,
                    Window = rateWindow,
                    QueueLimit = 0
                });
            }

            var anonIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter($"anon:{anonIp}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimits.AnonymousPermitLimit,
                Window = rateWindow,
                QueueLimit = 0
            });
        });

        options.AddPolicy("auth-policy", httpContext =>
        {
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter($"auth:{clientIp}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimits.AuthPermitLimit,
                Window = rateWindow,
                QueueLimit = 0
            });
        });

        options.AddPolicy("directory-policy", httpContext =>
        {
            var studentId = httpContext.User.GetStudentId();
            var partitionKey = studentId > 0
                ? $"dir:user:{studentId}"
                : $"dir:ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimits.DirectoryPermitLimit,
                Window = rateWindow,
                QueueLimit = 0
            });
        });
    });

var app = builder.Build();

// STRIDE THR-S-01 / THR-S-03 / THR-I-03: fail fast if insecure secrets, email sink, or SQL TLS settings are used outside Development
var resolvedCs = app.Configuration.GetConnectionString("TeamUpDb")
    ?? app.Configuration.GetConnectionString("DefaultConnection");

SecurityStartupValidator.Validate(new SecurityStartupSettings(
    IsDevelopment: app.Environment.IsDevelopment(),
    IsProduction: app.Environment.IsProduction(),
    JwtKey: app.Configuration["Jwt:Key"],
    JwtIssuer: app.Configuration["Jwt:Issuer"],
    JwtAudience: app.Configuration["Jwt:Audience"],
    EmailProvider: app.Configuration["Email:Provider"],
    DatabaseProvider: app.Configuration["DatabaseProvider"],
    ConnectionString: resolvedCs));

app.UseForwardedHeaders();
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
        var payload = BaseResponse.Fail(msg, response.StatusCode);
        await response.WriteAsJsonAsync(payload);
    }
});

// Baseline HTTP security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    var isReport = context.Request.Path.Value?.EndsWith("/report", StringComparison.OrdinalIgnoreCase) == true;
    var isStaticAsset = context.Request.Path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
        || context.Request.Path.StartsWithSegments("/logos", StringComparison.OrdinalIgnoreCase);
    context.Response.Headers["Content-Security-Policy"] = isReport
        ? "default-src 'none'; style-src 'unsafe-inline'; frame-ancestors 'none'"
        : isStaticAsset
            ? "default-src 'none'; img-src 'self'; frame-ancestors 'none'"
            : "default-src 'none'; frame-ancestors 'none'";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
    if (app.Configuration.GetValue("Hosting:EnableHttpsRedirection", true))
    {
        app.UseHttpsRedirection();
    }
}

var seedEnabled = app.Configuration.GetValue("Seed:Enabled", false);
var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", app.Environment.IsDevelopment() && seedEnabled);

if (autoMigrate || seedEnabled)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlite())
    {
        await db.Database.EnsureCreatedAsync();
        var sqlitePatchStatements = new[]
        {
            "ALTER TABLE University ADD COLUMN LogoUrl TEXT NULL;",
            "ALTER TABLE Student ADD COLUMN FailedLoginCount INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE Student ADD COLUMN LockoutEndUtc TEXT NULL;",
            "ALTER TABLE Student ADD COLUMN PreviousRefreshToken TEXT NULL;",
            "ALTER TABLE Student ADD COLUMN PreviousRefreshTokenRotatedAt TEXT NULL;",
            "ALTER TABLE ScoreTransaction ADD COLUMN SourceReferenceId INTEGER NULL;",
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId ON ScoreTransaction (StudentId, SourceType, SourceReferenceId) WHERE SourceReferenceId IS NOT NULL;",
            "ALTER TABLE Student ADD COLUMN EmailNotificationsEnabled INTEGER NOT NULL DEFAULT 1;",
            "ALTER TABLE Student ADD COLUMN ReceiveLeaderboardDigests INTEGER NOT NULL DEFAULT 1;",
            "ALTER TABLE Student ADD COLUMN ReceiveProjectMatchEmails INTEGER NOT NULL DEFAULT 1;"
        };
        foreach (var sql in sqlitePatchStatements)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // Column already exists in local SQLite dev database
            }
        }
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    if (seedEnabled)
    {
        var password = app.Configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            if (app.Environment.IsDevelopment())
            {
                password = "DevelopmentSeedPassword2026!";
            }
            else
            {
                throw new InvalidOperationException(
                    "CRITICAL: Seed:AdminPassword must be explicitly configured (at least 12 characters) when Seed:Enabled is true outside Development.");
            }
        }
        else if (!app.Environment.IsDevelopment())
        {
            SecurityStartupValidator.ValidateSeedAdminPassword(password);
        }

        var hash = new PasswordHasher<Student>().HashPassword(null!, password);
        await DbSeeder.SeedAsync(db, hash, app.Environment.IsDevelopment());
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public,max-age=86400";
        ctx.Context.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
    }
});

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapGet("/health", async (AppDbContext? db, CancellationToken ct) =>
{
    if (db is null)
        return Results.Ok(new { status = "Healthy", database = "NotConfigured" });

    var canConnect = await db.Database.CanConnectAsync(ct);
    return canConnect
        ? Results.Ok(new { status = "Healthy", database = "Connected" })
        : Results.Json(new { status = "Unhealthy", database = "Disconnected" }, statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program;
