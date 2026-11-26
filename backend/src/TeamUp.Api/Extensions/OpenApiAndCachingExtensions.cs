// Date: 2026-11-26
// Purpose: OpenAPI 3.2 document configuration, Response Compression, and Output Caching for global read-only catalog endpoints.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;

namespace TeamUp.Api.Extensions;

public static class CatalogOutputCacheTags
{
    public const string Universities = "catalog-universities";
    public const string Skills = "catalog-skills";
    public const string OrganizationCategories = "catalog-org-categories";
    public const string Badges = "catalog-badges";

    public static string? ForRestoredEntityType(string entityType) =>
        entityType.Trim().ToLowerInvariant() switch
        {
            "university" or "universities" or "faculty" or "faculties" => Universities,
            "skill" or "skills" => Skills,
            "organization" or "organizations" => OrganizationCategories,
            "badge" or "badges" => Badges,
            _ => null
        };
}

public static class OpenApiAndCachingExtensions
{
    public const string CatalogOutputCachePolicyName = "Catalog";

    #region DI Registration

    extension(IServiceCollection services)
    {
        public IServiceCollection AddTeamUpOpenApiAndCaching() =>
            services
                .AddTeamUpOpenApi()
                .AddTeamUpResponseCompression()
                .AddTeamUpOutputCaching();

        private IServiceCollection AddTeamUpOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2;
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info.Title = "TeamUp API";
                    document.Info.Version = "v1";
                    document.Info.Description = "TeamUp Student Collaboration, Organizations, Events & Gamification REST API (.NET 11).";
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

                options.AddOperationTransformer((operation, context, _) =>
                {
                    var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                    var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                    var requiresAuth = metadata.OfType<IAuthorizeData>().Any();

                    if (requiresAuth && !allowsAnonymous && context.Document is not null)
                    {
                        operation.Security ??= [];
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                        });
                    }

                    return Task.CompletedTask;
                });
            });

            return services;
        }

        private IServiceCollection AddTeamUpResponseCompression()
        {
            services.AddResponseCompression(options =>
            {
                // Keep EnableForHttps=false by default to mitigate BREACH side-channel on authenticated JSON over TLS.
                options.EnableForHttps = false;
                options.Providers.Add<ZstandardCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/json", "text/csv"]);
            });

            return services;
        }

        private IServiceCollection AddTeamUpOutputCaching()
        {
            services.AddOutputCache(options =>
            {
                options.AddPolicy(CatalogOutputCachePolicyName, CatalogOutputCachePolicy.Instance);
            });

            return services;
        }
    }

    #endregion

    #region Output Cache Policy

    private sealed class CatalogOutputCachePolicy : IOutputCachePolicy
    {
        public static readonly CatalogOutputCachePolicy Instance = new();

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var request = context.HttpContext.Request;
            var isCacheableMethod = HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);

            // Explicitly allow caching global non-tenant catalog endpoints (e.g. /skills, /badges) even when
            // Authorization is present, because response content is identical across authenticated users.
            context.EnableOutputCaching = isCacheableMethod;
            context.AllowCacheLookup = isCacheableMethod;
            context.AllowCacheStorage = isCacheableMethod;
            context.AllowLocking = true;
            context.ResponseExpirationTimeSpan = TimeSpan.FromMinutes(5);
            context.CacheVaryByRules.QueryKeys = "*";

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
            ValueTask.CompletedTask;

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var response = context.HttpContext.Response;
            if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie) || response.StatusCode != StatusCodes.Status200OK)
            {
                context.AllowCacheStorage = false;
            }

            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
