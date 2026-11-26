using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Infrastructure.Persistence.Configurations;

#region Organizations, Units & Membership

public sealed class OrganizationConfig : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(x => x.OrganizationId);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(300);
        builder.Property(x => x.Scope).HasDefaultValue(OrganizationScope.UniversityBranch);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(300);
        builder.Property(x => x.IsRecruiting).HasDefaultValue(false);
        builder.Property(x => x.CurrentSeasonName).HasMaxLength(80);
        builder.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).IsRequired(false);
        builder.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => new { x.Category, x.Name }).HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => new { x.Scope, x.IsRecruiting }).HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => x.UniversityId).HasFilter(Sql.NotDeleted);
    }
}

public sealed class OrganizationUnitConfig : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
    {
        builder.HasKey(x => x.OrganizationUnitId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(600);
        builder.Property(x => x.UnitType).HasDefaultValue(OrganizationUnitType.TechnicalChapter);
        builder.Property(x => x.IsRecruiting).HasDefaultValue(false);
        builder.HasOne(x => x.Organization).WithMany(o => o.Units).HasForeignKey(x => x.OrganizationId);
        builder.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public sealed class OrganizationMemberConfig : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.HasKey(x => new { x.OrganizationId, x.StudentId });
        builder.Property(x => x.Role).HasMaxLength(60).IsRequired();
        builder.Property(x => x.HierarchyTier)
            .HasDefaultValue(OrganizationHierarchyTier.OrganizingCrew)
            .HasSentinel((OrganizationHierarchyTier)(-1));
        builder.Property(x => x.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.OrganizationUnitId);
        builder.HasIndex(x => new { x.OrganizationId, x.Role, x.JoinedAt }).HasFilter(Sql.NotDeleted);
    }
}

public sealed class MembershipApplicationConfig : IEntityTypeConfiguration<MembershipApplication>
{
    public void Configure(EntityTypeBuilder<MembershipApplication> builder)
    {
        builder.HasKey(x => x.ApplicationId);
        builder.Property(x => x.Message).HasMaxLength(600);
        builder.Property(x => x.AppliedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        builder.Property(x => x.SeasonName).HasMaxLength(80);
        builder.Property(x => x.Stage).HasDefaultValue(RecruitmentStage.Applied);
        builder.Property(x => x.EvaluationScore).HasPrecision(4, 2);
        builder.Property(x => x.InterviewNotes).HasMaxLength(1000);
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => new { x.OrganizationId, x.Stage }).HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => new { x.OrganizationId, x.AppliedAt }).HasFilter(Sql.NotDeleted);
    }
}

#endregion

#region Events, Sessions & Attendance

public sealed class EventConfig : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DeliveryMode).HasDefaultValue(EventDeliveryMode.InPerson);
        builder.Property(x => x.IsPublic).HasDefaultValue(true).HasSentinel(true);
        builder.Property(x => x.MeetingUrl).HasMaxLength(500);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => new { x.StartDate, x.Title }).HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => new { x.OrganizationId, x.StartDate }).HasFilter(Sql.NotDeleted);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Event_Dates", "[EndDate] >= [StartDate]");
            t.HasCheckConstraint("CK_Event_Capacity", "[Capacity] >= 1");
        });
    }
}

public sealed class EventRegistrationConfig : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(x => new { x.EventId, x.StudentId });
        builder.Property(x => x.RegisteredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.Status).HasDefaultValue(RegistrationStatus.Confirmed);
        builder.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => new { x.EventId, x.Status, x.RegisteredAt });
    }
}

public sealed class EventSessionConfig : IEntityTypeConfiguration<EventSession>
{
    public void Configure(EntityTypeBuilder<EventSession> builder)
    {
        builder.HasKey(x => x.SessionId);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Room).HasMaxLength(60);
        builder.HasOne(x => x.Event).WithMany(e => e.Sessions).HasForeignKey(x => x.EventId);
        builder.HasIndex(x => new { x.EventId, x.SessionNo }).IsUnique().HasFilter(Sql.NotDeleted);
        builder.ToTable(t => t.HasCheckConstraint("CK_EventSession_Times", "[EndTime] >= [StartTime]"));
    }
}

public sealed class SessionAttendanceConfig : IEntityTypeConfiguration<SessionAttendance>
{
    public void Configure(EntityTypeBuilder<SessionAttendance> builder)
    {
        builder.HasKey(x => new { x.SessionId, x.StudentId });
        builder.Property(x => x.CheckInTime).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionId);
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasOne(x => x.RecordedByStudent).WithMany().HasForeignKey(x => x.RecordedByStudentId).IsRequired(false);
        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.RecordedByStudentId);
        builder.HasIndex(x => new { x.SessionId, x.CheckInTime }).HasFilter(Sql.NotDeleted);
    }
}

#endregion

#region Organization Creation Proposals

public sealed class OrganizationCreationRequestConfig : IEntityTypeConfiguration<OrganizationCreationRequest>
{
    public void Configure(EntityTypeBuilder<OrganizationCreationRequest> builder)
    {
        builder.HasKey(x => x.RequestId);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Scope).HasDefaultValue(OrganizationScope.UniversityBranch);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.LogoUrl).HasMaxLength(300);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(300);
        builder.Property(x => x.CurrentSeasonName).HasMaxLength(80);
        builder.Property(x => x.Justification).HasMaxLength(1000);
        builder.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        builder.Property(x => x.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.ReviewNotes).HasMaxLength(1000);

        builder.HasOne(x => x.RequesterStudent).WithMany().HasForeignKey(x => x.RequesterStudentId);
        builder.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).IsRequired(false);
        builder.HasOne(x => x.ReviewedByStudent).WithMany().HasForeignKey(x => x.ReviewedByStudentId).IsRequired(false);
        builder.HasOne(x => x.CreatedOrganization).WithMany().HasForeignKey(x => x.CreatedOrganizationId).IsRequired(false);

        builder.HasIndex(x => new { x.RequesterStudentId, x.Name }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        builder.HasIndex(x => new { x.Status, x.RequestedAt }).HasFilter(Sql.NotDeleted);
        builder.HasIndex(x => x.RequesterStudentId);
    }
}

#endregion
