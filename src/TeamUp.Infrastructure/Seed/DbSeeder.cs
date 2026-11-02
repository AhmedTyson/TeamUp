using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Seed;

/// <summary>Idempotent, development-only reference data.</summary>
public static class DbSeeder
{
    private static readonly (string Name, string Category)[] Skills =
    [
        ("C#", "Backend"), ("ASP.NET Core", "Backend"), ("Entity Framework Core", "Backend"), ("SQL Server", "Database"),
        ("PostgreSQL", "Database"), ("Python", "Backend"), ("Java", "Backend"), ("Node.js", "Backend"),
        ("JavaScript", "Frontend"), ("TypeScript", "Frontend"), ("React", "Frontend"), ("Angular", "Frontend"),
        ("Vue.js", "Frontend"), ("HTML/CSS", "Frontend"), ("Flutter", "Mobile"), ("Kotlin", "Mobile"),
        ("Docker", "DevOps"), ("Git", "DevOps"), ("CI/CD", "DevOps"), ("Azure", "Cloud"),
        ("AWS", "Cloud"), ("Machine Learning", "AI"), ("Data Analysis", "AI"), ("UI/UX Design", "Design"),
        ("Figma", "Design"), ("Project Management", "Soft Skills"), ("Public Speaking", "Soft Skills"),
        ("Teamwork", "Soft Skills"), ("Technical Writing", "Soft Skills"), ("Cybersecurity", "Security"),
    ];

    private static readonly (string Name, string Desc, BadgeCriteriaType Type, int Value)[] Badges =
    [
        ("First Steps", "Earn your first 100 points", BadgeCriteriaType.PointsThreshold, 100),
        ("Rising Star", "Reach 500 points", BadgeCriteriaType.PointsThreshold, 500),
        ("Champion", "Reach 2000 points", BadgeCriteriaType.PointsThreshold, 2000),
        ("Workshop Marathoner", "Attend 10 events", BadgeCriteriaType.EventCount, 10),
        ("Task Master", "Complete 25 tasks", BadgeCriteriaType.TaskCount, 25),
    ];

    public static async Task SeedAsync(AppDbContext db, string adminPasswordHash, CancellationToken ct = default)
    {
        if (!await db.Universities.AnyAsync(ct))
        {
            var uni = new University { Name = "Cairo University", City = "Giza", Country = "Egypt", Website = "https://cu.edu.eg" };
            uni.Faculties.Add(new Faculty { Name = "Faculty of Computers and Artificial Intelligence", Code = "FCAI" });
            uni.Faculties.Add(new Faculty { Name = "Faculty of Engineering", Code = "ENG" });
            db.Universities.Add(uni);
        }

        var existingSkills = await db.Skills.Select(s => s.Name).ToListAsync(ct);
        db.Skills.AddRange(Skills.Where(s => !existingSkills.Contains(s.Name))
            .Select(s => new Skill { Name = s.Name, Category = s.Category }));

        var existingBadges = await db.Badges.Select(b => b.Name).ToListAsync(ct);
        db.Badges.AddRange(Badges.Where(b => !existingBadges.Contains(b.Name)).Select(b => new Badge
        {
            Name = b.Name, Description = b.Desc, IconUrl = $"/badges/{b.Name.ToLowerInvariant().Replace(' ', '-')}.png",
            CriteriaType = b.Type, CriteriaValue = b.Value,
        }));
        await db.SaveChangesAsync(ct);

        if (!await db.Students.AnyAsync(s => s.Role == UserRole.PlatformAdmin, ct))
        {
            var faculty = await db.Faculties.OrderBy(f => f.FacultyId).FirstAsync(ct);
            db.Students.Add(new Student
            {
                Email = "admin@teamup.local", PasswordHash = adminPasswordHash, FirstName = "Platform", LastName = "Admin",
                AcademicYear = 1, Role = UserRole.PlatformAdmin, FacultyId = faculty.FacultyId,
            });
            await db.SaveChangesAsync(ct);
        }
    }
}
