using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data.Entities;

namespace TeamUp.Api.Data;

public class TeamUpDbContext : DbContext
{
    public TeamUpDbContext(DbContextOptions<TeamUpDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<SessionAttendance> SessionAttendances => Set<SessionAttendance>();
    public DbSet<ScoreTransaction> ScoreTransactions => Set<ScoreTransaction>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<StudentBadge> StudentBadges => Set<StudentBadge>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite Keys
        modelBuilder.Entity<OrganizationMember>()
            .HasKey(om => new { om.OrganizationID, om.StudentID });

        modelBuilder.Entity<StudentBadge>()
            .HasKey(sb => new { sb.StudentID, sb.BadgeID });

        // Unique Constraints
        modelBuilder.Entity<EventRegistration>()
            .HasIndex(r => new { r.EventID, r.StudentID })
            .IsUnique();

        modelBuilder.Entity<SessionAttendance>()
            .HasIndex(a => new { a.SessionID, a.StudentID })
            .IsUnique();

        modelBuilder.Entity<Badge>()
            .HasIndex(b => b.Name)
            .IsUnique();

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.IdempotencyKey)
            .IsUnique();

        // Foreign key behaviors
        modelBuilder.Entity<EventSession>()
            .HasOne(s => s.Event)
            .WithMany(e => e.Sessions)
            .HasForeignKey(s => s.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventRegistration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionAttendance>()
            .HasOne(a => a.Session)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.SessionID)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void SeedInitialData()
    {
        var now = DateTime.UtcNow;

        if (!Organizations.Any(o => o.OrganizationID == "org-ieee"))
        {
            Organizations.Add(new Organization
            {
                OrganizationID = "org-ieee",
                Name = "IEEE Student Branch",
                Description = "IEEE Helwan Student Branch",
                IsActive = true,
                CreatedAt = now
            });
        }

        if (!Organizations.Any(o => o.OrganizationID == "org-robotics"))
        {
            Organizations.Add(new Organization
            {
                OrganizationID = "org-robotics",
                Name = "Robotics Club",
                Description = "Autonomous Robotics & AI Club",
                IsActive = true,
                CreatedAt = now
            });
        }

        // Students
        if (!Students.Any(s => s.StudentID == "1"))
        {
            Students.Add(new Student
            {
                StudentID = "1",
                Email = "student1@teamup.edu",
                FirstName = "Student",
                LastName = "One",
                Role = "Student",
                CreatedAt = now
            });
        }

        if (!Students.Any(s => s.StudentID == "std-sarah"))
        {
            Students.Add(new Student
            {
                StudentID = "std-sarah",
                Email = "sarah@teamup.edu",
                FirstName = "Sarah",
                LastName = "Student",
                Role = "Student",
                CreatedAt = now
            });
        }

        if (!Students.Any(s => s.StudentID == "std-ali"))
        {
            Students.Add(new Student
            {
                StudentID = "std-ali",
                Email = "ali@teamup.edu",
                FirstName = "Ali",
                LastName = "Student",
                Role = "Student",
                CreatedAt = now
            });
        }

        // OrgAdmins
        if (!Students.Any(s => s.StudentID == "std-ibrahim"))
        {
            Students.Add(new Student
            {
                StudentID = "std-ibrahim",
                Email = "ibrahim@teamup.edu",
                FirstName = "Ibrahim",
                LastName = "OrgAdmin",
                Role = "Student",
                CreatedAt = now
            });
        }

        if (!Students.Any(s => s.StudentID == "orgadmin-1"))
        {
            Students.Add(new Student
            {
                StudentID = "orgadmin-1",
                Email = "orgadmin1@teamup.edu",
                FirstName = "OrgAdmin",
                LastName = "One",
                Role = "Student",
                CreatedAt = now
            });
        }

        // PlatformAdmins
        if (!Students.Any(s => s.StudentID == "admin-master"))
        {
            Students.Add(new Student
            {
                StudentID = "admin-master",
                Email = "admin@teamup.edu",
                FirstName = "Super",
                LastName = "Admin",
                Role = "PlatformAdmin",
                CreatedAt = now
            });
        }

        if (!Students.Any(s => s.StudentID == "platformadmin-1"))
        {
            Students.Add(new Student
            {
                StudentID = "platformadmin-1",
                Email = "platformadmin1@teamup.edu",
                FirstName = "Platform",
                LastName = "Admin",
                Role = "PlatformAdmin",
                CreatedAt = now
            });
        }

        SaveChanges();

        // OrgAdmin Memberships
        if (!OrganizationMembers.Any(om => om.OrganizationID == "org-ieee" && om.StudentID == "std-ibrahim"))
        {
            OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationID = "org-ieee",
                StudentID = "std-ibrahim",
                Role = "OrgAdmin",
                JoinedAt = now
            });
        }

        if (!OrganizationMembers.Any(om => om.OrganizationID == "org-ieee" && om.StudentID == "orgadmin-1"))
        {
            OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationID = "org-ieee",
                StudentID = "orgadmin-1",
                Role = "OrgAdmin",
                JoinedAt = now
            });
        }

        SaveChanges();
    }
}
