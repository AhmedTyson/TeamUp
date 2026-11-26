// Date: 2026-11-26
// Purpose: Startup database initialization, SQLite dev schema patching, idempotent campus ecosystem seeding, and default cover image seeding.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Services;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

namespace TeamUp.Api.Extensions;

public static partial class DatabaseStartupExtensions
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Default cover seeding skipped; SeedImages disk fallback will serve default covers.")]
    private static partial void LogDefaultCoverSeedingSkipped(ILogger logger, Exception ex);
    extension(WebApplication app)
    {
        public async Task InitializeTeamUpDatabaseAsync()
        {
            var seedEnabled = app.Configuration.GetValue("Seed:Enabled", false);
            var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", app.Environment.IsDevelopment() && seedEnabled);
            var syncEcosystemCatalog = app.Configuration.GetValue("Seed:SyncEcosystemCatalog", autoMigrate);

            if (autoMigrate || seedEnabled || syncEcosystemCatalog)
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (db.Database.IsSqlite())
                {
                    await db.Database.EnsureCreatedAsync();
                    await SqliteDevSchemaPatcher.PatchAsync(db);
                }
                else
                {
                    await db.Database.MigrateAsync();
                }

                if (seedEnabled)
                {
                    var password = app.ResolveSeedAdminPassword();
                    var hash = new PasswordHasher<Student>().HashPassword(null!, password);
                    await DbSeeder.SeedAsync(db, hash, app.Environment.IsDevelopment());
                }
                else if (syncEcosystemCatalog)
                {
                    await DbSeeder.SyncEcosystemCatalogAsync(db, isDevelopment: false);
                }
            }

            await app.SeedDefaultCoversAsync();
        }

        private string ResolveSeedAdminPassword()
        {
            var password = app.Configuration["Seed:AdminPassword"];
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (!app.Environment.IsDevelopment())
                {
                    SecurityStartupValidator.ValidateSeedAdminPassword(password);
                }

                return password;
            }

            if (app.Environment.IsDevelopment())
            {
                return "DevelopmentSeedPassword2026!";
            }

            throw new InvalidOperationException(
                "CRITICAL: Seed:AdminPassword must be explicitly configured (at least 12 characters) when Seed:Enabled is true outside Development.");
        }

        private async Task SeedDefaultCoversAsync()
        {
            // Generated default covers live in the database (StoredImages) so cards never render image-less.
            try
            {
                using var imageScope = app.Services.CreateScope();
                var db = imageScope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dataSource = db.Database.GetDbConnection().DataSource;
                var isUnbornSqliteFile = db.Database.IsSqlite()
                    && !string.IsNullOrEmpty(dataSource)
                    && !dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
                    && !File.Exists(dataSource);
                if (isUnbornSqliteFile)
                    return;

                var baseSeedImages = Path.Combine(AppContext.BaseDirectory, "SeedImages");
                var seedImagesDirectory = Directory.Exists(baseSeedImages)
                    ? baseSeedImages
                    : Path.Combine(app.Environment.ContentRootPath, "SeedImages");

                var imageService = imageScope.ServiceProvider.GetRequiredService<IImageService>();
                await imageService.EnsureDefaultCoversAsync(seedImagesDirectory);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogDefaultCoverSeedingSkipped(app.Logger, ex);
            }
        }
    }
}
