// Date: 2026-11-26
// Purpose: Security middleware and DI extensions (ForwardedHeaders, CORS, JWT Bearer + SecurityStamp validation, tiered RateLimiting, and baseline security headers).

using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Common.Security;

namespace TeamUp.Api.Extensions;

public static class SecurityPipelineExtensions
{
    #region Service Registration

    public static IServiceCollection AddTeamUpSecurityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment) =>
        services
            .ConfigureForwardedHeaders()
            .ConfigureCors(configuration, environment)
            .ConfigureJwtAuthentication()
            .ConfigureAuthorizationPolicies()
            .ConfigureRateLimiting();

    private static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        // STRIDE THR-S-02: trust X-Forwarded-For / X-Forwarded-Proto ONLY from explicitly configured proxies/networks.
        services.AddOptions<ForwardedHeadersOptions>()
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

        return services;
    }

    private static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // STRIDE THR-I-02: strict explicit CORS policy (exact origins, explicit HTTP methods and headers).
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var allowedOrigins = configuredOrigins is { Length: > 0 }
            ? configuredOrigins
            : (environment.IsDevelopment()
                ? ["http://localhost:5173", "http://localhost:4173", "http://127.0.0.1:5173", "http://127.0.0.1:4173"]
                : []);

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
                        .WithExposedHeaders("Content-Disposition")
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOpt) =>
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
                    // STRIDE THR-E-02: immediately reject tokens whose SecurityStamp, active status, or role no longer match persistence.
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        if (principal is null)
                        {
                            context.Fail("Invalid token principal.");
                            return;
                        }

                        var studentId = principal.StudentId;
                        var tokenStamp = principal.SecurityStamp;
                        var tokenRole = principal.UserRole;

                        if (studentId <= 0 || tokenStamp is null)
                        {
                            context.Fail("Token is missing required security claims.");
                            return;
                        }

                        var validator = context.HttpContext.RequestServices
                            .GetRequiredService<ISecurityStampValidator>();
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
                        await WriteJsonErrorAsync(
                            context.Response,
                            StatusCodes.Status401Unauthorized,
                            "Authentication is required. Please supply a valid Bearer token.");
                    },
                    OnForbidden = context =>
                        WriteJsonErrorAsync(
                            context.Response,
                            StatusCodes.Status403Forbidden,
                            "Access denied. You do not have permission to access this resource.")
                };
            });

        return services;
    }

    private static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("PlatformAdminOnly", p => p.RequireRole("PlatformAdmin"));
            options.AddPolicy("OrgAdminOrPlatformAdmin", p => p.RequireRole("OrgAdmin", "PlatformAdmin"));
        });

        return services;
    }

    private static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {
        // STRIDE THR-S-02 / THR-D-01 / THR-I-01: global + tiered rate limiting using sanitized RemoteIpAddress ONLY.
        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingOptions>>((options, rlOpt) =>
            {
                var rateLimits = rlOpt.Value;
                var rateWindow = TimeSpan.FromSeconds(Math.Max(1, rateLimits.WindowSeconds));

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, ct) =>
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                            ? ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString()
                            : rateLimits.WindowSeconds.ToString();

                    await WriteJsonErrorAsync(
                        context.HttpContext.Response,
                        StatusCodes.Status429TooManyRequests,
                        "Rate limit exceeded. Please slow down and try again shortly.",
                        ct);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    if (httpContext.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
                        return RateLimitPartition.GetNoLimiter("health");

                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        var studentId = httpContext.User.StudentId;
                        var userPartition = studentId > 0 ? $"user:{studentId}" : $"ip:{ResolveClientIp(httpContext)}";
                        return CreateFixedWindowPartition(userPartition, rateLimits.AuthenticatedPermitLimit, rateWindow);
                    }

                    return CreateFixedWindowPartition($"anon:{ResolveClientIp(httpContext)}", rateLimits.AnonymousPermitLimit, rateWindow);
                });

                options.AddPolicy("auth-policy", httpContext =>
                    CreateFixedWindowPartition($"auth:{ResolveClientIp(httpContext)}", rateLimits.AuthPermitLimit, rateWindow));

                options.AddPolicy("directory-policy", httpContext =>
                {
                    var studentId = httpContext.User.StudentId;
                    var partitionKey = studentId > 0
                        ? $"dir:user:{studentId}"
                        : $"dir:ip:{ResolveClientIp(httpContext)}";
                    return CreateFixedWindowPartition(partitionKey, rateLimits.DirectoryPermitLimit, rateWindow);
                });

                options.AddPolicy("upload-policy", httpContext =>
                {
                    var studentId = httpContext.User.StudentId;
                    var partitionKey = studentId > 0
                        ? $"upload:user:{studentId}"
                        : $"upload:ip:{ResolveClientIp(httpContext)}";
                    return CreateFixedWindowPartition(partitionKey, Math.Max(1, rateLimits.UploadPermitLimit), rateWindow);
                });
            });

        return services;
    }

    #endregion

    #region Helpers

    private static string ResolveClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static RateLimitPartition<string> CreateFixedWindowPartition(string key, int permitLimit, TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0
        });

    private static Task WriteJsonErrorAsync(
        HttpResponse response,
        int statusCode,
        string message,
        CancellationToken ct = default)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json; charset=utf-8";
        var payload = BaseResponse.Fail(message, statusCode);
        return response.WriteAsJsonAsync(payload, ct);
    }

    #endregion

    #region Middleware Pipeline

    public static WebApplication UseTeamUpSecurityPipeline(this WebApplication app)
    {
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
            ConnectionString: resolvedCs,
            EmailSmtpHost: app.Configuration["Email:SmtpHost"]));

        app.UseForwardedHeaders();
        app.UseExceptionHandler();
        app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;
            if (response.ContentType is null || !response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var msg = response.StatusCode switch
                {
                    401 => "Authentication is required. Please supply a valid Bearer token.",
                    403 => "Access denied. You do not have permission to access this resource.",
                    404 => $"The requested route '{context.HttpContext.Request.Path}' was not found.",
                    405 => "HTTP method not allowed on this endpoint.",
                    429 => "Rate limit exceeded. Please slow down and try again shortly.",
                    _ => $"HTTP error {response.StatusCode}."
                };
                await WriteJsonErrorAsync(response, response.StatusCode, msg);
            }
        });

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
            var path = context.Request.Path.Value;
            var isHtmlPreview = path?.EndsWith("/report", StringComparison.OrdinalIgnoreCase) == true
                || path?.Contains("/templates/preview/", StringComparison.OrdinalIgnoreCase) == true;
            var isStaticAsset = context.Request.Path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments("/logos", StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.StartsWithSegments("/api/v1/images", StringComparison.OrdinalIgnoreCase);
            context.Response.Headers["Content-Security-Policy"] = isHtmlPreview
                ? "default-src 'none'; style-src 'unsafe-inline'; img-src 'self' https:; frame-ancestors 'none'"
                : isStaticAsset
                    ? "default-src 'none'; img-src 'self'; frame-ancestors 'none'"
                    : "default-src 'none'; frame-ancestors 'none'";
            await next();
        });

        return app;
    }

    #endregion
}
