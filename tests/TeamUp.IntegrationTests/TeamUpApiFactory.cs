using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class TeamUpApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public int SeededFacultyId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DatabaseProvider", "Sqlite");
        _connection.Open();

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:TeamUpDb"] = "",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>());
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Universities.Any())
        {
            var uni = new University
            {
                Name = "Cairo University Test",
                City = "Giza",
                Country = "Egypt"
            };
            var faculty = new Faculty
            {
                Name = "Faculty of Computers and AI",
                Code = "FCAI_TEST",
                University = uni
            };
            db.Universities.Add(uni);
            db.Faculties.Add(faculty);
            db.SaveChanges();

            SeededFacultyId = faculty.FacultyId;
        }
        else
        {
            SeededFacultyId = db.Faculties.First().FacultyId;
        }

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
