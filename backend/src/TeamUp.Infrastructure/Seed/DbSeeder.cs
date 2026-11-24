using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Common;
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
        (
            "Helwan University",
            "Cairo",
            "Egypt",
            "https://helwan.edu.eg",
            "https://upload.wikimedia.org/wikipedia/en/6/60/Helwan_University_logo.png",
            [
                ("Faculty of Engineering (Helwan)", "ENG_HU"),
                ("Faculty of Computers and Artificial Intelligence", "FCAI_HU"),
                ("Faculty of Commerce and Business Administration", "COM_HU"),
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

    private static readonly (
        string Email,
        string FirstName,
        string LastName,
        int AcademicYear,
        string FacultyCode,
        int Points,
        string PhotoUrl,
        string Bio,
        string[] Skills
    )[] ShowcaseStudents =
    [
        (
            "salma@cu.edu.eg",
            "Salma",
            "Khaled",
            4,
            "FCAI",
            2840,
            "/images/people/salma-khaled.jpg",
            "Building cross-faculty AI & React squads at Cairo University FCAI.",
            ["Python", "React", "Machine Learning"]
        ),
        (
            "omar@capu.edu.eg",
            "Omar",
            "Mostafa",
            3,
            "ENG_CAPU",
            2490,
            "/images/people/omar-mostafa.jpg",
            "Embedded systems & full-stack clubs lead at Capital University.",
            ["TypeScript", "Docker", "C#"]
        ),
        (
            "youssef@asu.edu.eg",
            "Youssef",
            "Adel",
            3,
            "CIS",
            2310,
            "/images/people/youssef-adel.jpg",
            "Backend architect & campus workshop mentor at Ain Shams CIS.",
            ["ASP.NET Core", "PostgreSQL", "Docker"]
        ),
        (
            "hana@alexu.edu.eg",
            "Hana",
            "El-Sayed",
            2,
            "CDS_ALEX",
            1920,
            "/images/people/hana-el-sayed.jpg",
            "Data science & computer vision researcher at Alexandria CDS.",
            ["Python", "Data Analysis", "Figma"]
        ),
        (
            "karim@guc.edu.eg",
            "Karim",
            "Mansour",
            4,
            "MET_GUC",
            1780,
            "/images/people/karim-mansour.jpg",
            "Systems engineer & design-systems builder at GUC MET.",
            ["React", "TypeScript", "UI/UX Design"]
        ),
        (
            "mariam@mans.edu.eg",
            "Mariam",
            "Nabil",
            2,
            "FCI_MANS",
            1540,
            "/images/people/mariam-nabil.jpg",
            "Applied ML & UX researcher at Mansoura University FCI.",
            ["Machine Learning", "Python", "UI/UX Design"]
        ),
    ];

    /// <summary>
    /// Seeds reference data (universities, skills, badges) and a bootstrap platform admin.
    /// Outside Development (<paramref name="isDevelopment"/> false) it never overwrites the password of an existing
    /// account, does not create the OrgAdmin demo account, and gives showcase profiles an unusable random password so
    /// they cannot be logged into.
    /// </summary>
    public static async Task SeedAsync(
        AppDbContext db, string adminPasswordHash, bool isDevelopment = false, CancellationToken ct = default)
    {
        var existingUniversities = await db.Universities
            .IgnoreQueryFilters()
            .Include(u => u.Faculties)
            .ToDictionaryAsync(u => u.Name, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var uniDef in Universities)
        {
            if (existingUniversities.TryGetValue(uniDef.Name, out var existingUni))
            {
                existingUni.LogoUrl = uniDef.LogoUrl;
                existingUni.Website = uniDef.Website;
                foreach (var (facName, facCode) in uniDef.Faculties)
                {
                    if (!existingUni.Faculties.Any(f => string.Equals(f.Code, facCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        existingUni.Faculties.Add(new Faculty { Name = facName, Code = facCode });
                    }
                }
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
        else if (isDevelopment)
        {
            existingAdmin.PasswordHash = adminPasswordHash;
            await db.SaveChangesAsync(ct);
        }

        if (!isDevelopment)
        {
            await SeedShowcaseStudentsAsync(db, CreateUnusablePasswordHash(), resetExistingPasswords: false, ct);
            return;
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

        await SeedShowcaseStudentsAsync(db, adminPasswordHash, resetExistingPasswords: true, ct);
        await SeedEgyptianActivitiesEcosystemAsync(db, ct);
    }

    private static async Task SeedEgyptianActivitiesEcosystemAsync(AppDbContext db, CancellationToken ct)
    {
        var unisByName = await db.Universities
            .IgnoreQueryFilters()
            .ToDictionaryAsync(u => u.Name, StringComparer.OrdinalIgnoreCase, ct);

        var studentsByEmail = await db.Students
            .IgnoreQueryFilters()
            .ToDictionaryAsync(s => s.Email, StringComparer.OrdinalIgnoreCase, ct);

        var specs = new (
            string Name,
            string Description,
            string Category,
            OrganizationScope Scope,
            string? UniversityName,
            string? City,
            string? WebsiteUrl,
            bool IsRecruiting,
            string? SeasonName,
            (string Name, string Code, string Description, OrganizationUnitType UnitType, bool IsRecruiting, int OpenSeats)[] Units
        )[]
        {
            (
                "IEEE Helwan Student Branch",
                "Pioneer engineering & computing student branch at Helwan University empowering students with technical workshops, robotics competitions, research bootcamps, and career readiness.",
                "Engineering",
                OrganizationScope.UniversityBranch,
                "Helwan University",
                "Helwan, Cairo",
                "https://ieee-helwan.org",
                true,
                "Season 2026 — Fall Recruitment",
                [
                    ("Computer Society", "CS", "Full-stack web, AI, algorithms, and competitive programming tracks.", OrganizationUnitType.TechnicalChapter, true, 25),
                    ("Robotics & Automation", "RAS", "Autonomous robots, ROS2, embedded firmware, and Arduino hardware.", OrganizationUnitType.TechnicalChapter, true, 18),
                    ("Power & Energy Society", "PES", "Smart grids, renewable energy systems, and power electronics.", OrganizationUnitType.TechnicalChapter, true, 15),
                    ("Communications Society", "ComSoc", "5G networks, IoT telemetry, RF hardware, and SDR labs.", OrganizationUnitType.TechnicalChapter, false, 10),
                    ("Women in Engineering", "WIE", "Empowering women engineers through mentorship and leadership labs.", OrganizationUnitType.AffinityGroup, true, 20),
                    ("Human Resources", "HR", "Recruitment screening, member experience, and season evaluations.", OrganizationUnitType.OperationalCommittee, true, 8),
                    ("PR & Fundraising", "PR", "Corporate sponsorships, partner outreach, and speaker invitations.", OrganizationUnitType.OperationalCommittee, true, 10),
                    ("Marketing & Media", "MEDIA", "Visual identity, photography, social campaigns, and event branding.", OrganizationUnitType.OperationalCommittee, true, 12),
                    ("Logistics & OC", "OC", "Venue operations, stage tech, registration desks, and event execution.", OrganizationUnitType.OperationalCommittee, true, 14),
                ]
            ),
            (
                "IEEE Cairo University SB",
                "Student branch at Cairo University advancing electrical engineering, AI systems, and cross-faculty innovation projects.",
                "Engineering",
                OrganizationScope.UniversityBranch,
                "Cairo University",
                "Giza",
                "https://ieeecusb.org",
                true,
                "2026 Technical Track",
                [
                    ("Computer Society", "CS", "Systems engineering, distributed backends, and cloud labs.", OrganizationUnitType.TechnicalChapter, true, 20),
                    ("Robotics & Automation", "RAS", "Mechatronics, computer vision, and UAV flight stacks.", OrganizationUnitType.TechnicalChapter, true, 15),
                    ("Human Resources", "HR", "Member onboarding, KPI tracking, and technical committee pairing.", OrganizationUnitType.OperationalCommittee, true, 6),
                ]
            ),
            (
                "Enactus Helwan",
                "Entrepreneurship action community turning student innovation into sustainable social enterprises across Helwan.",
                "Entrepreneurship",
                OrganizationScope.UniversityBranch,
                "Helwan University",
                "Helwan, Cairo",
                "https://enactus-helwan.org",
                true,
                "Social Impact 2026",
                [
                    ("Project Development", "PD", "Business modeling, field validation, and green impact ventures.", OrganizationUnitType.TechnicalChapter, true, 20),
                    ("Human Resources", "HR", "Talent sourcing, interview panels, and crew engagement.", OrganizationUnitType.OperationalCommittee, true, 8),
                    ("PR & Partnerships", "PR", "NGO alliances, incubator pitch days, and community sponsors.", OrganizationUnitType.OperationalCommittee, true, 8),
                ]
            ),
            (
                "GDG Helwan",
                "Independent Google Developer Group serving Helwan, Maadi, and South Cairo with Android, Flutter, Cloud, and Gemini AI study jams.",
                "Technology",
                OrganizationScope.Independent,
                null,
                "Helwan & Maadi, Cairo",
                "https://gdg.community.dev/gdg-helwan/",
                true,
                "DevFest Crew 2026",
                [
                    ("Cloud & AI Study Jam", "AI", "Hands-on Google Cloud and applied LLM engineering sessions.", OrganizationUnitType.WorkingGroup, true, 30),
                    ("Flutter Mobile Builders", "FLT", "Cross-platform mobile apps for local startups and open source.", OrganizationUnitType.WorkingGroup, true, 20),
                ]
            ),
            (
                "GDG Cairo",
                "Greater Cairo's flagship developer community uniting engineers, founders, and university builders around modern web and AI tools.",
                "Technology",
                OrganizationScope.Independent,
                null,
                "Greater Cairo",
                "https://gdg.community.dev/gdg-cairo/",
                false,
                null,
                [
                    ("Web Platform Chapter", "WEB", "Modern TypeScript, performance, and design systems talks.", OrganizationUnitType.WorkingGroup, false, 0),
                    ("Applied AI Circle", "GENAI", "Open-weights inference, RAG architectures, and agents.", OrganizationUnitType.WorkingGroup, false, 0),
                ]
            ),
            (
                "STP Egypt",
                "Cross-university leadership, academic coaching, and career development organization preparing Egyptian students for tech and management careers.",
                "Leadership",
                OrganizationScope.CrossUniversity,
                null,
                "Cairo, Egypt",
                "https://stp-egypt.org",
                true,
                "Season 21",
                [
                    ("Technical Track", "TECH", "Software engineering and problem-solving workshops.", OrganizationUnitType.TechnicalChapter, true, 24),
                    ("Academic & Juniors", "AC", "Career guidance and STEM readiness camps for freshmen.", OrganizationUnitType.AffinityGroup, true, 16),
                    ("Human Resources", "HR", "Structured evaluation rubrics and mentoring follow-ups.", OrganizationUnitType.OperationalCommittee, true, 8),
                ]
            ),
            (
                "Egypt AI & Robotics Collective",
                "Open independent builder collective uniting university students, fresh graduates, and self-taught researchers on open-source AI and robotics.",
                "AI & Robotics",
                OrganizationScope.Independent,
                null,
                "Cairo & Remote",
                "https://egypt-ai.org",
                true,
                "Open Research Cohort",
                [
                    ("Open LLM Lab", "LLM", "Arabic & bilingual fine-tuning, evaluation, and tooling.", OrganizationUnitType.WorkingGroup, true, 15),
                    ("Edge Robotics Guild", "EDGE", "On-device vision and embedded ROS navigation.", OrganizationUnitType.WorkingGroup, true, 12),
                ]
            ),
        };

        var seededOrgs = new Dictionary<string, Organization>(StringComparer.OrdinalIgnoreCase);

        foreach (var spec in specs)
        {
            var uniId = spec.UniversityName is not null && unisByName.TryGetValue(spec.UniversityName, out var u)
                ? u.UniversityId
                : (int?)null;

            var org = await db.Organizations.IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Name == spec.Name, ct);

            if (org is null)
            {
                org = new Organization
                {
                    Name = spec.Name,
                    Description = spec.Description,
                    Category = spec.Category,
                    Scope = spec.Scope,
                    UniversityId = uniId,
                    City = spec.City,
                    WebsiteUrl = spec.WebsiteUrl,
                    IsRecruiting = spec.IsRecruiting,
                    CurrentSeasonName = spec.SeasonName,
                };
                db.Organizations.Add(org);
                await db.SaveChangesAsync(ct);
            }
            else if (org.DeletedAt is null)
            {
                org.Scope = spec.Scope;
                org.UniversityId = uniId;
                org.City = spec.City;
                org.WebsiteUrl = spec.WebsiteUrl;
                org.IsRecruiting = spec.IsRecruiting;
                org.CurrentSeasonName = spec.SeasonName;
                await db.SaveChangesAsync(ct);
            }

            if (org.DeletedAt is null)
            {
                seededOrgs[org.Name] = org;

                var existingCodes = await db.OrganizationUnits.IgnoreQueryFilters()
                    .Where(unit => unit.OrganizationId == org.OrganizationId)
                    .Select(unit => unit.Code)
                    .ToListAsync(ct);

                foreach (var unitDef in spec.Units)
                {
                    if (!existingCodes.Contains(unitDef.Code, StringComparer.OrdinalIgnoreCase))
                    {
                        db.OrganizationUnits.Add(new OrganizationUnit
                        {
                            OrganizationId = org.OrganizationId,
                            Name = unitDef.Name,
                            Code = unitDef.Code,
                            Description = unitDef.Description,
                            UnitType = unitDef.UnitType,
                            IsRecruiting = unitDef.IsRecruiting,
                            OpenSeats = unitDef.OpenSeats,
                        });
                    }
                }
                await db.SaveChangesAsync(ct);
            }
        }

        // Wire authentic 4-tier hierarchy and flagship events for IEEE Helwan Student Branch + Open Collective
        if (seededOrgs.TryGetValue("IEEE Helwan Student Branch", out var ieeeHelwan))
        {
            var unitsByCode = await db.OrganizationUnits
                .Where(u => u.OrganizationId == ieeeHelwan.OrganizationId)
                .ToDictionaryAsync(u => u.Code, StringComparer.OrdinalIgnoreCase, ct);

            var memberSpecs = new (string Email, string Role, string? UnitCode, OrganizationHierarchyTier Tier)[]
            {
                ("nour@cu.edu.eg", OrganizationRoles.Chair, null, OrganizationHierarchyTier.HighBoard),
                ("salma@cu.edu.eg", OrganizationRoles.ViceChair, null, OrganizationHierarchyTier.HighBoard),
                ("omar@capu.edu.eg", "Head of HR", "HR", OrganizationHierarchyTier.CommitteeLeadership),
                ("youssef@asu.edu.eg", "CS Chapter Chair", "CS", OrganizationHierarchyTier.CommitteeLeadership),
                ("karim@guc.edu.eg", "RAS Chapter Head", "RAS", OrganizationHierarchyTier.CommitteeLeadership),
                ("hana@alexu.edu.eg", "Media & Marketing Lead", "MEDIA", OrganizationHierarchyTier.CommitteeLeadership),
                ("mariam@mans.edu.eg", OrganizationRoles.Member, "OC", OrganizationHierarchyTier.OrganizingCrew),
            };

            foreach (var mSpec in memberSpecs)
            {
                if (!studentsByEmail.TryGetValue(mSpec.Email, out var stu) || stu.DeletedAt is not null)
                    continue;

                int? unitId = mSpec.UnitCode is not null && unitsByCode.TryGetValue(mSpec.UnitCode, out var unit)
                    ? unit.OrganizationUnitId
                    : null;

                var existingMember = await db.OrganizationMembers.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.OrganizationId == ieeeHelwan.OrganizationId && m.StudentId == stu.StudentId, ct);

                if (existingMember is null)
                {
                    db.OrganizationMembers.Add(new OrganizationMember
                    {
                        OrganizationId = ieeeHelwan.OrganizationId,
                        StudentId = stu.StudentId,
                        Role = mSpec.Role,
                        OrganizationUnitId = unitId,
                        HierarchyTier = mSpec.Tier,
                        JoinedAt = DateTime.UtcNow.AddDays(-30)
                    });
                }
            }
            await db.SaveChangesAsync(ct);

            var bootcamp = await db.Events.FirstOrDefaultAsync(
                e => e.OrganizationId == ieeeHelwan.OrganizationId && e.Title == "Auxilio Engineering & AI Bootcamp 2026", ct);

            if (bootcamp is null)
            {
                var start = DateTime.UtcNow.Date.AddDays(5).AddHours(10);
                bootcamp = new Event
                {
                    OrganizationId = ieeeHelwan.OrganizationId,
                    OrganizationUnitId = unitsByCode.TryGetValue("CS", out var csUnit) ? csUnit.OrganizationUnitId : null,
                    Title = "Auxilio Engineering & AI Bootcamp 2026",
                    Description = "Three-day flagship hands-on engineering & AI bootcamp hosted by IEEE Helwan Student Branch covering clean backends, ROS robotics, and applied machine learning.",
                    Type = EventType.Workshop,
                    DeliveryMode = EventDeliveryMode.Hybrid,
                    IsPublic = true,
                    MeetingUrl = "https://meet.teamup.eg/ieee-helwan-auxilio",
                    Location = "Helwan Faculty of Engineering Main Hall · Cairo",
                    StartDate = start,
                    EndDate = start.AddDays(2).AddHours(8),
                    Capacity = 120
                };
                db.Events.Add(bootcamp);
                await db.SaveChangesAsync(ct);

                var s1 = new EventSession
                {
                    EventId = bootcamp.EventId,
                    SessionNo = 1,
                    Title = "Opening Keynote & System Architecture",
                    StartTime = start,
                    EndTime = start.AddHours(3),
                    Room = "Auditorium A1"
                };
                var s2 = new EventSession
                {
                    EventId = bootcamp.EventId,
                    SessionNo = 2,
                    Title = "Computer Society: Full-Stack AI Lab",
                    StartTime = start.AddDays(1),
                    EndTime = start.AddDays(1).AddHours(3),
                    Room = "Lab 3B / Stream"
                };
                var s3 = new EventSession
                {
                    EventId = bootcamp.EventId,
                    SessionNo = 3,
                    Title = "RAS & PES Prototype Demo Day",
                    StartTime = start.AddDays(2),
                    EndTime = start.AddDays(2).AddHours(4),
                    Room = "Main Hall"
                };
                db.EventSessions.AddRange(s1, s2, s3);
                await db.SaveChangesAsync(ct);

                // Register Salma & Karim and check in one participant so Participants tab & Digital Pass work out-of-the-box
                if (studentsByEmail.TryGetValue("salma@cu.edu.eg", out var salma))
                {
                    db.EventRegistrations.Add(new EventRegistration
                    {
                        EventId = bootcamp.EventId,
                        StudentId = salma.StudentId,
                        Status = RegistrationStatus.Confirmed,
                        RegisteredAt = DateTime.UtcNow.AddDays(-2)
                    });
                    db.SessionAttendances.Add(new SessionAttendance
                    {
                        SessionId = s1.SessionId,
                        StudentId = salma.StudentId,
                        VerificationMethod = VerificationMethod.QRCode,
                        CheckInTime = DateTime.UtcNow.AddHours(-3),
                        RecordedByStudentId = salma.StudentId
                    });
                }

                if (studentsByEmail.TryGetValue("mariam@mans.edu.eg", out var mariam))
                {
                    db.EventRegistrations.Add(new EventRegistration
                    {
                        EventId = bootcamp.EventId,
                        StudentId = mariam.StudentId,
                        Status = RegistrationStatus.Confirmed,
                        RegisteredAt = DateTime.UtcNow.AddDays(-1)
                    });
                }
                await db.SaveChangesAsync(ct);
            }
        }

        if (seededOrgs.TryGetValue("GDG Helwan", out var gdgHelwan)
            && !await db.Events.AnyAsync(e => e.OrganizationId == gdgHelwan.OrganizationId, ct))
        {
            var start = DateTime.UtcNow.Date.AddDays(10).AddHours(14);
            var gdgEvt = new Event
            {
                OrganizationId = gdgHelwan.OrganizationId,
                Title = "DevFest Helwan Prep Workshop",
                Description = "Community meetup in Helwan & Maadi exploring Kotlin Multiplatform, Gemini APIs, and Cloud Run deployments.",
                Type = EventType.Session,
                DeliveryMode = EventDeliveryMode.InPerson,
                IsPublic = true,
                Location = "Creativa Innovation Hub · Helwan",
                StartDate = start,
                EndDate = start.AddHours(4),
                Capacity = 80
            };
            db.Events.Add(gdgEvt);
            await db.SaveChangesAsync(ct);
            db.EventSessions.Add(new EventSession
            {
                EventId = gdgEvt.EventId,
                SessionNo = 1,
                Title = "Gemini Agents & Cloud Run Workshop",
                StartTime = start,
                EndTime = start.AddHours(3),
                Room = "Hub Stage"
            });
            await db.SaveChangesAsync(ct);
        }

        if (seededOrgs.TryGetValue("Egypt AI & Robotics Collective", out var aiCollective)
            && !await db.Events.AnyAsync(e => e.OrganizationId == aiCollective.OrganizationId, ct))
        {
            var start = DateTime.UtcNow.Date.AddDays(14).AddHours(18);
            var aiEvt = new Event
            {
                OrganizationId = aiCollective.OrganizationId,
                Title = "Egypt Open LLM & Embedded AI Summit",
                Description = "Open national hackathon and speaker series bringing university students and independent learners together.",
                Type = EventType.Hackathon,
                DeliveryMode = EventDeliveryMode.Online,
                IsPublic = true,
                MeetingUrl = "https://meet.teamup.eg/ai-summit",
                Location = "Online Live Stage · Cairo Time (EET)",
                StartDate = start,
                EndDate = start.AddDays(1).AddHours(4),
                Capacity = 250
            };
            db.Events.Add(aiEvt);
            await db.SaveChangesAsync(ct);
            db.EventSessions.Add(new EventSession
            {
                EventId = aiEvt.EventId,
                SessionNo = 1,
                Title = "Arabic Evaluation Benchmarks Keynote",
                StartTime = start,
                EndTime = start.AddHours(2),
                Room = "Main Stream"
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static string CreateUnusablePasswordHash() =>
        new Microsoft.AspNetCore.Identity.PasswordHasher<Student>().HashPassword(
            null!, Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)));

    private static async Task SeedShowcaseStudentsAsync(
        AppDbContext db,
        string defaultPasswordHash,
        bool resetExistingPasswords,
        CancellationToken ct)
    {
        var facultiesByCode = await db.Faculties
            .IgnoreQueryFilters()
            .ToDictionaryAsync(f => f.Code, StringComparer.OrdinalIgnoreCase, ct);

        var skillsByName = await db.Skills
            .IgnoreQueryFilters()
            .ToDictionaryAsync(s => s.Name, StringComparer.OrdinalIgnoreCase, ct);

        var thresholdBadges = await db.Badges
            .IgnoreQueryFilters()
            .Where(b => b.CriteriaType == BadgeCriteriaType.PointsThreshold)
            .ToListAsync(ct);

        var fallbackFaculty = await db.Faculties.OrderBy(f => f.FacultyId).FirstAsync(ct);

        foreach (var def in ShowcaseStudents)
        {
            var facultyId = facultiesByCode.TryGetValue(def.FacultyCode, out var faculty)
                ? faculty.FacultyId
                : fallbackFaculty.FacultyId;

            var student = await db.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Email == def.Email, ct);

            if (student is null)
            {
                student = new Student
                {
                    Email = def.Email,
                    PasswordHash = defaultPasswordHash,
                    FirstName = def.FirstName,
                    LastName = def.LastName,
                    AcademicYear = def.AcademicYear,
                    Role = UserRole.Student,
                    FacultyId = facultyId,
                    SecurityStamp = Guid.NewGuid(),
                };
                db.Students.Add(student);
                await db.SaveChangesAsync(ct);
            }
            else if (student.DeletedAt is null)
            {
                if (resetExistingPasswords)
                    student.PasswordHash = defaultPasswordHash;
                if (student.FacultyId != facultyId)
                    student.FacultyId = facultyId;
                await db.SaveChangesAsync(ct);
            }

            if (student.DeletedAt is not null)
                continue;

            var profile = await db.StudentProfiles
                .FirstOrDefaultAsync(p => p.StudentId == student.StudentId, ct);

            if (profile is null)
            {
                db.StudentProfiles.Add(new StudentProfile
                {
                    StudentId = student.StudentId,
                    PhotoUrl = def.PhotoUrl,
                    Bio = def.Bio,
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(profile.PhotoUrl))
                    profile.PhotoUrl = def.PhotoUrl;
                if (string.IsNullOrWhiteSpace(profile.Bio))
                    profile.Bio = def.Bio;
            }

            var existingSkillIds = await db.StudentSkills
                .IgnoreQueryFilters()
                .Where(ss => ss.StudentId == student.StudentId)
                .Select(ss => ss.SkillId)
                .ToListAsync(ct);

            foreach (var skillName in def.Skills)
            {
                if (skillsByName.TryGetValue(skillName, out var skill) && !existingSkillIds.Contains(skill.SkillId))
                {
                    db.StudentSkills.Add(new StudentSkill
                    {
                        StudentId = student.StudentId,
                        SkillId = skill.SkillId,
                        ProficiencyLevel = ProficiencyLevel.Advanced,
                        YearsOfExperience = 2,
                    });
                }
            }

            var hasScoreHistory = await db.ScoreTransactions
                .AnyAsync(t => t.StudentId == student.StudentId, ct);

            if (!hasScoreHistory)
            {
                db.ScoreTransactions.Add(new ScoreTransaction
                {
                    StudentId = student.StudentId,
                    Points = def.Points,
                    Reason = "Verified campus collaboration & workshop contributions",
                    SourceType = ScoreSourceType.Manual,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            var existingBadgeIds = await db.StudentBadges
                .IgnoreQueryFilters()
                .Where(sb => sb.StudentId == student.StudentId)
                .Select(sb => sb.BadgeId)
                .ToListAsync(ct);

            foreach (var badge in thresholdBadges)
            {
                if (def.Points >= badge.CriteriaValue && !existingBadgeIds.Contains(badge.BadgeId))
                {
                    db.StudentBadges.Add(new StudentBadge
                    {
                        StudentId = student.StudentId,
                        BadgeId = badge.BadgeId,
                        AwardedAt = DateTime.UtcNow,
                    });
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
