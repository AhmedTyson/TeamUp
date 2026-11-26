using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence.Configurations;

internal static class Sql
{
    public const string NotDeleted = "[DeletedAt] IS NULL";
}

#region Academic Catalog

public sealed class UniversityConfig : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.HasKey(x => x.UniversityId);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public sealed class FacultyConfig : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.HasKey(x => x.FacultyId);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.University).WithMany(u => u.Faculties).HasForeignKey(x => x.UniversityId);
        builder.HasIndex(x => new { x.UniversityId, x.Code }).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

#endregion

#region Student Profiles & Governance

public sealed class StudentConfig : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(x => x.StudentId);
        builder.Property(x => x.Email).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Role).HasDefaultValue(Domain.Enums.UserRole.Student);
        builder.Property(x => x.SecurityStamp).IsRequired();
        builder.Property(x => x.ShowEmailInDirectory).HasDefaultValue(false);
        builder.Property(x => x.EmailNotificationsEnabled).HasDefaultValue(true).HasSentinel(true);
        builder.Property(x => x.ReceiveLeaderboardDigests).HasDefaultValue(true).HasSentinel(true);
        builder.Property(x => x.ReceiveProjectMatchEmails).HasDefaultValue(true).HasSentinel(true);
        builder.Property(x => x.RefreshToken).HasMaxLength(256);
        builder.Property(x => x.PreviousRefreshToken).HasMaxLength(256);
        builder.Property(x => x.PasswordResetToken).HasMaxLength(256);
        builder.Property(x => x.AffiliationType).HasDefaultValue(Domain.Enums.AffiliationType.UniversityStudent);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.HasOne(x => x.Faculty).WithMany(f => f.Students).HasForeignKey(x => x.FacultyId).IsRequired(false);
        builder.HasIndex(x => x.Email).IsUnique().HasFilter(Sql.NotDeleted);
        builder.ToTable(t => t.HasCheckConstraint("CK_Student_AcademicYear", "[AcademicYear] BETWEEN 1 AND 7"));
    }
}

public sealed class StudentInterestConfig : IEntityTypeConfiguration<StudentInterest>
{
    public void Configure(EntityTypeBuilder<StudentInterest> builder)
    {
        builder.HasKey(x => new { x.StudentId, x.Interest });
        builder.Property(x => x.Interest).HasMaxLength(50).IsRequired();
        builder.HasOne(x => x.Student).WithMany(s => s.Interests).HasForeignKey(x => x.StudentId);
    }
}

public sealed class StudentProfileConfig : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.HasKey(x => x.ProfileId);
        builder.Property(x => x.Bio).HasMaxLength(1000);
        builder.Property(x => x.PhotoUrl).HasMaxLength(300);
        builder.Property(x => x.GitHubUrl).HasMaxLength(200);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(200);
        builder.HasOne(x => x.Student).WithOne(s => s.Profile).HasForeignKey<StudentProfile>(x => x.StudentId);
        builder.HasIndex(x => x.StudentId).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.AuditLogId);
        builder.Property(x => x.ActorEmail).HasMaxLength(150);
        builder.Property(x => x.Action).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ClientIp).HasMaxLength(64);
        builder.Property(x => x.OccurredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.ActorStudent).WithMany().HasForeignKey(x => x.ActorStudentId).IsRequired(false);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.ActorStudentId);
    }
}

#endregion
