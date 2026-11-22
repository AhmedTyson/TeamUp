using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Seed;

/// <summary>Idempotent, development-only reference data.</summary>
public static class DbSeeder
{
    private static readonly (string Name, string City, string Country, string Website, string LogoUrl, (string Name, string Code)[] Faculties)[] Universities =
    [
        (
            "Cairo University",
            "Giza",
            "Egypt",
            "https://cu.edu.eg",
            "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png",
            [
                ("Faculty of Computers and Artificial Intelligence", "FCAI"),
                ("Faculty of Engineering", "ENG"),
                ("Faculty of Science", "SCI"),
            ]
        ),
        (
            "Ain Shams University",
            "Cairo",
            "Egypt",
            "https://asu.edu.eg",
            "https://upload.wikimedia.org/wikipedia/en/3/3d/Ain_Shams_logo.png",
            [
                ("Faculty of Computer and Information Sciences", "CIS"),
                ("Faculty of Engineering", "ENG_ASU"),
            ]
        ),
        (
            "Alexandria University",
            "Alexandria",
            "Egypt",
            "https://alexu.edu.eg",
            "https://upload.wikimedia.org/wikipedia/ar/6/63/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D8%A5%D8%B3%D9%83%D9%86%D8%AF%D8%B1%D9%8A%D8%A9.png",
            [
                ("Faculty of Engineering", "ENG_ALEX"),
                ("Faculty of Computing and Data Science", "CDS_ALEX"),
            ]
        ),
        (
            "Mansoura University",
            "Mansoura",
            "Egypt",
            "https://mans.edu.eg",
            "https://upload.wikimedia.org/wikipedia/ar/5/5e/%D8%B4%D8%B9%D8%A7%D8%B1_%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D9%85%D9%86%D8%B5%D9%88%D8%B1%D8%A9.png",
            [
                ("Faculty of Computers and Information", "FCI_MANS"),
                ("Faculty of Engineering", "ENG_MANS"),
            ]
        ),
        (
            "Assiut University",
            "Assiut",
            "Egypt",
            "https://aun.edu.eg",
            "https://www.aun.edu.eg/main/themes/gavias_colin/aun-logo.png",
            [
                ("Faculty of Computers and Information", "FCI_AUN"),
                ("Faculty of Engineering", "ENG_AUN"),
            ]
        ),
        (
            "Capital University",
            "Cairo",
            "Egypt",
            "https://capu.edu.eg",
            "https://upload.wikimedia.org/wikipedia/commons/c/c1/Capital_University_Logo.png",
            [
                ("Faculty of Computers and Artificial Intelligence", "FCAI_CAPU"),
                ("Faculty of Engineering", "ENG_CAPU"),
            ]
        ),
        (
            "Zagazig University",
            "Zagazig",
            "Egypt",
            "https://zu.edu.eg",
            "https://www.zu.edu.eg/uploads/2017/07/ZULogo.png",
            [
                ("Faculty of Computers and Informatics", "FCI_ZU"),
                ("Faculty of Engineering", "ENG_ZU"),
            ]
        ),
        (
            "Tanta University",
            "Tanta",
            "Egypt",
            "https://tanta.edu.eg",
            "https://tanta.edu.eg/tantaedu/img/unv-logo.png",
            [
                ("Faculty of Computers and Informatics", "FCI_TU"),
                ("Faculty of Engineering", "ENG_TU"),
            ]
        ),
        (
            "Benha University",
            "Benha",
            "Egypt",
            "https://bu.edu.eg",
            "https://upload.wikimedia.org/wikipedia/en/d/dc/Benha_University_Logo.png",
            [
                ("Faculty of Computers and Artificial Intelligence", "FCAI_BU"),
                ("Shoubra Faculty of Engineering", "ENG_BU"),
            ]
        ),
        (
            "Suez Canal University",
            "Ismailia",
            "Egypt",
            "https://suez.edu.eg",
            "https://suez.edu.eg/ar/wp-content/uploads/2022/01/266177708_300900118547240_420708396794634738_n-1.png",
            [
                ("Faculty of Computers and Informatics", "FCI_SCU"),
                ("Faculty of Engineering", "ENG_SCU"),
            ]
        ),
        (
            "German University in Cairo (GUC)",
            "New Cairo",
            "Egypt",
            "https://guc.edu.eg",
            "https://www.guc.edu.eg/img/guc_logo_og.png",
            [
                ("Faculty of Media Engineering and Technology", "MET_GUC"),
                ("Faculty of Information Engineering and Technology", "IET_GUC"),
            ]
        ),
        (
            "The American University in Cairo (AUC)",
            "New Cairo",
            "Egypt",
            "https://aucegypt.edu",
            "https://upload.wikimedia.org/wikipedia/commons/1/1b/AUC_Arabic_Logo_2021.png",
            [
                ("School of Sciences and Engineering", "SSE_AUC"),
                ("School of Business", "BUS_AUC"),
            ]
        ),
    ];

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
        var legacyHelwan = await db.Universities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Name == "Helwan University", ct);
        if (legacyHelwan is not null)
        {
            legacyHelwan.Name = "Capital University";
            legacyHelwan.Website = "https://capu.edu.eg";
            legacyHelwan.LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c1/Capital_University_Logo.png";
            await db.SaveChangesAsync(ct);
        }

        var existingUniversities = await db.Universities
            .IgnoreQueryFilters()
            .ToDictionaryAsync(u => u.Name, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var uniDef in Universities)
        {
            if (existingUniversities.TryGetValue(uniDef.Name, out var existingUni))
            {
                existingUni.LogoUrl = uniDef.LogoUrl;
                existingUni.Website = uniDef.Website;
                continue;
            }

            var uni = new University
            {
                Name = uniDef.Name,
                City = uniDef.City,
                Country = uniDef.Country,
                Website = uniDef.Website,
                LogoUrl = uniDef.LogoUrl,
            };

            foreach (var (facName, facCode) in uniDef.Faculties)
            {
                uni.Faculties.Add(new Faculty { Name = facName, Code = facCode });
            }

            db.Universities.Add(uni);
        }

        var existingSkills = await db.Skills.Select(s => s.Name).ToListAsync(ct);
        db.Skills.AddRange(Skills.Where(s => !existingSkills.Contains(s.Name))
            .Select(s => new Skill { Name = s.Name, Category = s.Category }));

        var existingBadges = await db.Badges.Select(b => b.Name).ToListAsync(ct);
        db.Badges.AddRange(Badges.Where(b => !existingBadges.Contains(b.Name)).Select(b => new Badge
        {
            Name = b.Name,
            Description = b.Desc,
            IconUrl = $"/badges/{b.Name.ToLowerInvariant().Replace(' ', '-')}.png",
            CriteriaType = b.Type,
            CriteriaValue = b.Value,
        }));
        await db.SaveChangesAsync(ct);

        var firstFaculty = await db.Faculties.OrderBy(f => f.FacultyId).FirstAsync(ct);

        var existingAdmin = await db.Students.FirstOrDefaultAsync(s => s.Email == "admin@teamup.local", ct);
        if (existingAdmin is null)
        {
            var admin = new Student
            {
                Email = "admin@teamup.local",
                PasswordHash = adminPasswordHash,
                FirstName = "Platform",
                LastName = "Admin",
                AcademicYear = 4,
                Role = UserRole.PlatformAdmin,
                FacultyId = firstFaculty.FacultyId,
                SecurityStamp = Guid.NewGuid(),
            };
            db.Students.Add(admin);
            await db.SaveChangesAsync(ct);

            db.AuditLogs.Add(new AuditLog
            {
                ActorStudentId = admin.StudentId,
                ActorEmail = admin.Email,
                Action = "SeedInitialized",
                EntityType = "University",
                EntityId = "1",
                OldValues = null,
                NewValues = "{\"SeededUniversities\":3,\"AdminEmail\":\"admin@teamup.local\"}",
                ClientIp = "127.0.0.1",
            });
            await db.SaveChangesAsync(ct);
        }
        else
        {
            existingAdmin.PasswordHash = adminPasswordHash;
            await db.SaveChangesAsync(ct);
        }

        var existingOrgAdmin = await db.Students.FirstOrDefaultAsync(s => s.Email == "nour@cu.edu.eg", ct);
        if (existingOrgAdmin is null)
        {
            db.Students.Add(new Student
            {
                Email = "nour@cu.edu.eg",
                PasswordHash = adminPasswordHash,
                FirstName = "Nour",
                LastName = "Hassan",
                AcademicYear = 3,
                Role = UserRole.OrgAdmin,
                FacultyId = firstFaculty.FacultyId,
                SecurityStamp = Guid.NewGuid(),
            });
            await db.SaveChangesAsync(ct);
        }
        else
        {
            existingOrgAdmin.PasswordHash = adminPasswordHash;
            await db.SaveChangesAsync(ct);
        }
    }
}
