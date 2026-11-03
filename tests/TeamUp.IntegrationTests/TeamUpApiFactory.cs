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
    public int SeededUniversityId { get; private set; }

    public string CreateToken(int studentId, string email, string role)
    {
        using var scope = Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<TeamUp.Application.Common.Security.IJwtTokenService>();
        var (token, _) = jwt.GenerateAccessToken(studentId, email, role, "Test User");
        return token;
    }

    public string CreateAdminToken() => CreateToken(9999, "admin@teamup.test", "PlatformAdmin");
    public string CreateStudentToken(int studentId = 1) => CreateToken(studentId, "student@teamup.test", "Student");

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

            SeededUniversityId = uni.UniversityId;
            SeededFacultyId = faculty.FacultyId;
        }
        else
        {
            SeededUniversityId = db.Universities.First().UniversityId;
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
