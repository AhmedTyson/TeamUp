// Date: 2026-11-26
// Purpose: ASP.NET Core 11 API bootstrap wiring modular security, OpenAPI 3.2, output caching, health probes, and database lifecycle extensions.

using TeamUp.Api.Extensions;
using TeamUp.Api.Filters;
using TeamUp.Api.Middleware;
using TeamUp.Api.Services;
using TeamUp.Application;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

#region Configuration & Service Registration

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();
builder.Services.AddScoped<IProfilePhotoStorage, LocalProfilePhotoStorage>();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTeamUpSecurityServices(builder.Configuration, builder.Environment);
builder.Services.AddTeamUpOpenApiAndCaching();
builder.Services.AddTeamUpHealthChecks();

#endregion

var app = builder.Build();

#region HTTP Pipeline & Endpoints

app.UseTeamUpSecurityPipeline();

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

await app.InitializeTeamUpDatabaseAsync();

app.UseResponseCompression();
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
app.UseOutputCache();
app.UseRateLimiter();
app.MapControllers();
app.MapTeamUpHealthEndpoints();

#endregion

app.Run();

public partial class Program;
