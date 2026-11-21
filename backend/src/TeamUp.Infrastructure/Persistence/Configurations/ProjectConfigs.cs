using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Infrastructure.Persistence.Configurations;

public class SkillConfig : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> b)
    {
        b.HasKey(x => x.SkillId);
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50).IsRequired();
        b.Property(x => x.Description).HasMaxLength(250);
        b.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class StudentSkillConfig : IEntityTypeConfiguration<StudentSkill>
{
    public void Configure(EntityTypeBuilder<StudentSkill> b)
    {
        b.HasKey(x => new { x.StudentId, x.SkillId });
        b.Property(x => x.YearsOfExperience).HasColumnType("decimal(3,1)").HasDefaultValue(0m);
        b.HasOne(x => x.Student).WithMany(s => s.Skills).HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.Skill).WithMany().HasForeignKey(x => x.SkillId);
        b.ToTable(t => t.HasCheckConstraint("CK_StudentSkill_Years", "[YearsOfExperience] >= 0"));
    }
}

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.HasKey(x => x.ProjectId);
        b.Property(x => x.Title).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).IsRequired(); // nvarchar(max)
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.Status).HasDefaultValue(ProjectStatus.Recruiting);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatorId);
        b.HasIndex(x => x.CreatorId);
        b.ToTable(t => t.HasCheckConstraint("CK_Project_MaxTeamSize", "[MaxTeamSize] >= 1"));
    }
}

public class ProjectRequiredSkillConfig : IEntityTypeConfiguration<ProjectRequiredSkill>
{
    public void Configure(EntityTypeBuilder<ProjectRequiredSkill> b)
    {
        b.HasKey(x => new { x.ProjectId, x.SkillId });
        b.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
        b.HasOne(x => x.Skill).WithMany().HasForeignKey(x => x.SkillId);
    }
}

public class ProjectMemberConfig : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> b)
    {
        b.HasKey(x => new { x.ProjectId, x.StudentId });
        b.Property(x => x.Role).HasMaxLength(60).IsRequired();
        b.Property(x => x.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Project).WithMany(p => p.Members).HasForeignKey(x => x.ProjectId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => x.StudentId);
    }
}

public class ProjectApplicationConfig : IEntityTypeConfiguration<ProjectApplication>
{
    public void Configure(EntityTypeBuilder<ProjectApplication> b)
    {
        b.HasKey(x => x.ApplicationId);
        b.Property(x => x.Message).HasMaxLength(500);
        b.Property(x => x.AppliedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        b.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        // One open application per student per project; re-applying after a rejection is allowed.
        b.HasIndex(x => new { x.ProjectId, x.StudentId }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        b.HasIndex(x => x.StudentId);
    }
}

public class ProjectTaskConfig : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> b)
    {
        b.HasKey(x => x.TaskId);
        b.Property(x => x.Title).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Status).HasDefaultValue(ProjectTaskStatus.Todo);
        b.HasOne(x => x.Project).WithMany(p => p.Tasks).HasForeignKey(x => x.ProjectId);
        b.HasOne(x => x.AssignedStudent).WithMany().HasForeignKey(x => x.AssignedStudentId).IsRequired(false);
        b.HasIndex(x => x.ProjectId);
        b.HasIndex(x => x.AssignedStudentId);
    }
}
