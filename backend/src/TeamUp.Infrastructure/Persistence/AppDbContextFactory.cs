using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeamUp.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI migrations and script generation in CI/headless environments.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__TeamUpDb");

        if (string.IsNullOrWhiteSpace(cs))
        {
            var apiDir = Path.Combine(Directory.GetCurrentDirectory(), "backend", "src", "TeamUp.Api");
            if (!Directory.Exists(apiDir))
            {
                apiDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "TeamUp.Api");
            }

            var devSettings = Path.Combine(apiDir, "appsettings.Development.json");
            if (File.Exists(devSettings))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(devSettings));
                    if (doc.RootElement.TryGetProperty("ConnectionStrings", out var csElement) &&
                        csElement.TryGetProperty("TeamUpDb", out var teamUpDbElement))
                    {
                        cs = teamUpDbElement.GetString();
                    }
                }
                catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException)
                {
                    Console.Error.WriteLine($"[TeamUp] Could not read '{devSettings}' ({ex.Message}); using default SQLEXPRESS connection.");
                }
            }
        }

        cs ??= "Server=.\\SQLEXPRESS;Database=TeamUpDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cs, sql => sql.UseCompatibilityLevel(160))
            .Options;

        return new AppDbContext(options);
    }
}
