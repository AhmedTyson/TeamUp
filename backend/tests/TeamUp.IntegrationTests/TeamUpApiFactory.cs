// Date: 2026-11-21
// Purpose: Integration test WebApplicationFactory backing in-memory SQLite persistence and real SecurityStamp-backed test identities.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
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
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var stampValidator = scope.ServiceProvider.GetRequiredService<ISecurityStampValidator>();
        var orgAuth = scope.ServiceProvider.GetRequiredService<IOrganizationAuthorizationService>();

        var parsedRole = Enum.TryParse<UserRole>(role, true, out var r) ? r : UserRole.Student;
        var student = db.Students.IgnoreQueryFilters().FirstOrDefault(s => s.StudentId == studentId);

        if (student is null)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (db.Students.IgnoreQueryFilters().Any(s => s.Email == normalizedEmail))
                normalizedEmail = $"student_{studentId}_{Guid.NewGuid():N}@teamup.test";

            student = new Student
            {
                StudentId = studentId,
                Email = normalizedEmail,
                PasswordHash = "test-hash",
                FirstName = "Test",
                LastName = $"User{studentId}",
                AcademicYear = 2,
                Role = parsedRole,
                SecurityStamp = Guid.NewGuid(),
                FacultyId = SeededFacultyId
            };
            db.Students.Add(student);
            db.SaveChanges();
        }
        else if (student.DeletedAt is null)
        {
            var changed = false;
            if (student.Role != parsedRole)
            {
                student.Role = parsedRole;
                changed = true;
            }
            if (student.SecurityStamp == Guid.Empty)
            {
                student.SecurityStamp = Guid.NewGuid();
                changed = true;
            }
            if (changed)
            {
                db.SaveChanges();
                stampValidator.Invalidate(studentId);
            }
        }

        var adminOrgs = orgAuth.GetAdminOrganizationIdsAsync(student.StudentId).GetAwaiter().GetResult();
        var (token, _) = jwt.GenerateAccessToken(
            student.StudentId,
            student.Email,
            student.Role.ToString(),
            $"{student.FirstName} {student.LastName}",
            student.SecurityStamp,
            adminOrgs);
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
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
                ["Jwt:Key"] = "IntegrationTestSigningKeyForTeamUpMustBeAtLeast32BytesLong!",
                ["Email:Provider"] = "InMemory",
                ["RateLimiting:AuthPermitLimit"] = "1000",
                ["RateLimiting:AuthenticatedPermitLimit"] = "1000",
                ["RateLimiting:AnonymousPermitLimit"] = "1000",
                ["RateLimiting:DirectoryPermitLimit"] = "1000"
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
