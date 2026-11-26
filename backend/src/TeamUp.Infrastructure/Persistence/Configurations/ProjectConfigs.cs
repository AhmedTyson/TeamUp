using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Infrastructure.Persistence.Configurations;

#region Skills

public sealed class SkillConfig : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(x => x.SkillId);
        builder.Property(x => x.Name).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public sealed class StudentSkillConfig : IEntityTypeConfiguration<StudentSkill>
{
    public void Configure(EntityTypeBuilder<StudentSkill> builder)
    {
        builder.HasKey(x => new { x.StudentId, x.SkillId });
        builder.Property(x => x.YearsOfExperience).HasColumnType("decimal(3,1)").HasDefaultValue(0m);
        builder.HasOne(x => x.Student).WithMany(s => s.Skills).HasForeignKey(x => x.StudentId);
        builder.HasOne(x => x.Skill).WithMany().HasForeignKey(x => x.SkillId);
        builder.ToTable(t => t.HasCheckConstraint("CK_StudentSkill_Years", "[YearsOfExperience] >= 0"));
    }
}

#endregion

#region Projects, Membership & Tasks

public sealed class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.ProjectId);
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).IsRequired(); // nvarchar(max)
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.Status).HasDefaultValue(ProjectStatus.Recruiting);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatorId);
        builder.HasIndex(x => x.CreatorId);
        builder.ToTable(t => t.HasCheckConstraint("CK_Project_MaxTeamSize", "[MaxTeamSize] >= 1"));
    }
}

public sealed class ProjectRequiredSkillConfig : IEntityTypeConfiguration<ProjectRequiredSkill>
{
    public void Configure(EntityTypeBuilder<ProjectRequiredSkill> builder)
    {
        builder.HasKey(x => new { x.ProjectId, x.SkillId });
        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
        builder.HasOne(x => x.Skill).WithMany().HasForeignKey(x => x.SkillId);
    }
}

public sealed class ProjectMemberConfig : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(x => new { x.ProjectId, x.StudentId });
        builder.Property(x => x.Role).HasMaxLength(60).IsRequired();
        builder.Property(x => x.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Project).WithMany(p => p.Members).HasForeignKey(x => x.ProjectId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasIndex(x => x.StudentId);
    }
}

public sealed class ProjectApplicationConfig : IEntityTypeConfiguration<ProjectApplication>
{
    public void Configure(EntityTypeBuilder<ProjectApplication> builder)
    {
        builder.HasKey(x => x.ApplicationId);
        builder.Property(x => x.Message).HasMaxLength(500);
        builder.Property(x => x.AppliedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        // One open application per student per project; re-applying after a rejection is allowed.
        builder.HasIndex(x => new { x.ProjectId, x.StudentId }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        builder.HasIndex(x => x.StudentId);
    }
}

public sealed class ProjectTaskConfig : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.HasKey(x => x.TaskId);
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Status).HasDefaultValue(ProjectTaskStatus.Todo);
        builder.HasOne(x => x.Project).WithMany(p => p.Tasks).HasForeignKey(x => x.ProjectId);
        builder.HasOne(x => x.AssignedStudent).WithMany().HasForeignKey(x => x.AssignedStudentId).IsRequired(false);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.AssignedStudentId);
    }
}

#endregion
