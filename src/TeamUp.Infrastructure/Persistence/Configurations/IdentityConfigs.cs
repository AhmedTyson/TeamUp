using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence.Configurations;

internal static class Sql
{
    public const string NotDeleted = "[DeletedAt] IS NULL";
}

public class UniversityConfig : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> b)
    {
        b.HasKey(x => x.UniversityId);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.Country).HasMaxLength(100).IsRequired();
        b.Property(x => x.Website).HasMaxLength(200);
        b.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class FacultyConfig : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> b)
    {
        b.HasKey(x => x.FacultyId);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.HasOne(x => x.University).WithMany(u => u.Faculties).HasForeignKey(x => x.UniversityId);
        b.HasIndex(x => new { x.UniversityId, x.Code }).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class StudentConfig : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.HasKey(x => x.StudentId);
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        b.Property(x => x.Role).HasDefaultValue(Domain.Enums.UserRole.Student);
        b.Property(x => x.RefreshToken).HasMaxLength(256);
        b.HasOne(x => x.Faculty).WithMany(f => f.Students).HasForeignKey(x => x.FacultyId);
        b.HasIndex(x => x.Email).IsUnique().HasFilter(Sql.NotDeleted);
        b.ToTable(t => t.HasCheckConstraint("CK_Student_AcademicYear", "[AcademicYear] BETWEEN 1 AND 7"));
    }
}

public class StudentInterestConfig : IEntityTypeConfiguration<StudentInterest>
{
    public void Configure(EntityTypeBuilder<StudentInterest> b)
    {
        b.HasKey(x => new { x.StudentId, x.Interest });
        b.Property(x => x.Interest).HasMaxLength(50).IsRequired();
        b.HasOne(x => x.Student).WithMany(s => s.Interests).HasForeignKey(x => x.StudentId);
    }
}

public class StudentProfileConfig : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> b)
    {
        b.HasKey(x => x.ProfileId);
        b.Property(x => x.Bio).HasMaxLength(1000);
        b.Property(x => x.PhotoUrl).HasMaxLength(300);
        b.Property(x => x.GitHubUrl).HasMaxLength(200);
        b.Property(x => x.LinkedInUrl).HasMaxLength(200);
        b.HasOne(x => x.Student).WithOne(s => s.Profile).HasForeignKey<StudentProfile>(x => x.StudentId);
        b.HasIndex(x => x.StudentId).IsUnique().HasFilter(Sql.NotDeleted);
    }
}
