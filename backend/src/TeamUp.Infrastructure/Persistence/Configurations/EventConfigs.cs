using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Infrastructure.Persistence.Configurations;

public class OrganizationConfig : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.HasKey(x => x.OrganizationId);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Description).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50).IsRequired();
        b.Property(x => x.LogoUrl).HasMaxLength(300);
        b.Property(x => x.Scope).HasDefaultValue(OrganizationScope.UniversityBranch);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.CoverImageUrl).HasMaxLength(500);
        b.Property(x => x.WebsiteUrl).HasMaxLength(300);
        b.Property(x => x.IsRecruiting).HasDefaultValue(false);
        b.Property(x => x.CurrentSeasonName).HasMaxLength(80);
        b.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).IsRequired(false);
        b.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
        b.HasIndex(x => new { x.Category, x.Name }).HasFilter(Sql.NotDeleted);
        b.HasIndex(x => new { x.Scope, x.IsRecruiting }).HasFilter(Sql.NotDeleted);
        b.HasIndex(x => x.UniversityId).HasFilter(Sql.NotDeleted);
    }
}

public class OrganizationUnitConfig : IEntityTypeConfiguration<OrganizationUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationUnit> b)
    {
        b.HasKey(x => x.OrganizationUnitId);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Description).HasMaxLength(600);
        b.Property(x => x.UnitType).HasDefaultValue(OrganizationUnitType.TechnicalChapter);
        b.Property(x => x.IsRecruiting).HasDefaultValue(false);
        b.HasOne(x => x.Organization).WithMany(o => o.Units).HasForeignKey(x => x.OrganizationId);
        b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class OrganizationMemberConfig : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> b)
    {
        b.HasKey(x => new { x.OrganizationId, x.StudentId });
        b.Property(x => x.Role).HasMaxLength(60).IsRequired();
        b.Property(x => x.HierarchyTier)
            .HasDefaultValue(OrganizationHierarchyTier.OrganizingCrew)
            .HasSentinel((OrganizationHierarchyTier)(-1));
        b.Property(x => x.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        b.HasIndex(x => x.StudentId);
        b.HasIndex(x => x.OrganizationUnitId);
        b.HasIndex(x => new { x.OrganizationId, x.Role, x.JoinedAt }).HasFilter(Sql.NotDeleted);
    }
}

public class MembershipApplicationConfig : IEntityTypeConfiguration<MembershipApplication>
{
    public void Configure(EntityTypeBuilder<MembershipApplication> b)
    {
        b.HasKey(x => x.ApplicationId);
        b.Property(x => x.Message).HasMaxLength(600);
        b.Property(x => x.AppliedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        b.Property(x => x.SeasonName).HasMaxLength(80);
        b.Property(x => x.Stage).HasDefaultValue(RecruitmentStage.Applied);
        b.Property(x => x.EvaluationScore).HasPrecision(4, 2);
        b.Property(x => x.InterviewNotes).HasMaxLength(1000);
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        b.HasIndex(x => new { x.OrganizationId, x.StudentId }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        b.HasIndex(x => x.StudentId);
        b.HasIndex(x => new { x.OrganizationId, x.Stage }).HasFilter(Sql.NotDeleted);
        b.HasIndex(x => new { x.OrganizationId, x.AppliedAt }).HasFilter(Sql.NotDeleted);
    }
}

public class EventConfig : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.HasKey(x => x.EventId);
        b.Property(x => x.Title).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).IsRequired();
        b.Property(x => x.Location).HasMaxLength(200).IsRequired();
        b.Property(x => x.DeliveryMode).HasDefaultValue(EventDeliveryMode.InPerson);
        b.Property(x => x.IsPublic).HasDefaultValue(true);
        b.Property(x => x.MeetingUrl).HasMaxLength(500);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.OrganizationUnitId).IsRequired(false);
        b.HasIndex(x => x.OrganizationId);
        b.HasIndex(x => x.StartDate);
        b.HasIndex(x => new { x.StartDate, x.Title }).HasFilter(Sql.NotDeleted);
        b.HasIndex(x => new { x.OrganizationId, x.StartDate }).HasFilter(Sql.NotDeleted);
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Event_Dates", "[EndDate] >= [StartDate]");
            t.HasCheckConstraint("CK_Event_Capacity", "[Capacity] >= 1");
        });
    }
}

public class EventRegistrationConfig : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> b)
    {
        b.HasKey(x => new { x.EventId, x.StudentId });
        b.Property(x => x.RegisteredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.Status).HasDefaultValue(RegistrationStatus.Confirmed);
        b.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => x.StudentId);
        b.HasIndex(x => new { x.EventId, x.Status, x.RegisteredAt });
    }
}

public class EventSessionConfig : IEntityTypeConfiguration<EventSession>
{
    public void Configure(EntityTypeBuilder<EventSession> b)
    {
        b.HasKey(x => x.SessionId);
        b.Property(x => x.Title).HasMaxLength(120).IsRequired();
        b.Property(x => x.Room).HasMaxLength(60);
        b.HasOne(x => x.Event).WithMany(e => e.Sessions).HasForeignKey(x => x.EventId);
        b.HasIndex(x => new { x.EventId, x.SessionNo }).IsUnique().HasFilter(Sql.NotDeleted);
        b.ToTable(t => t.HasCheckConstraint("CK_EventSession_Times", "[EndTime] >= [StartTime]"));
    }
}

public class SessionAttendanceConfig : IEntityTypeConfiguration<SessionAttendance>
{
    public void Configure(EntityTypeBuilder<SessionAttendance> b)
    {
        b.HasKey(x => new { x.SessionId, x.StudentId });
        b.Property(x => x.CheckInTime).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.RecordedByStudent).WithMany().HasForeignKey(x => x.RecordedByStudentId).IsRequired(false);
        b.HasIndex(x => x.StudentId);
        b.HasIndex(x => x.RecordedByStudentId);
        b.HasIndex(x => new { x.SessionId, x.CheckInTime }).HasFilter(Sql.NotDeleted);
    }
}

public class OrganizationCreationRequestConfig : IEntityTypeConfiguration<OrganizationCreationRequest>
{
    public void Configure(EntityTypeBuilder<OrganizationCreationRequest> b)
    {
        b.HasKey(x => x.RequestId);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50).IsRequired();
        b.Property(x => x.Scope).HasDefaultValue(OrganizationScope.UniversityBranch);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.LogoUrl).HasMaxLength(300);
        b.Property(x => x.CoverImageUrl).HasMaxLength(500);
        b.Property(x => x.WebsiteUrl).HasMaxLength(300);
        b.Property(x => x.CurrentSeasonName).HasMaxLength(80);
        b.Property(x => x.Justification).HasMaxLength(1000);
        b.Property(x => x.Status).HasDefaultValue(ApplicationStatus.Pending);
        b.Property(x => x.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.ReviewNotes).HasMaxLength(1000);

        b.HasOne(x => x.RequesterStudent).WithMany().HasForeignKey(x => x.RequesterStudentId);
        b.HasOne(x => x.University).WithMany().HasForeignKey(x => x.UniversityId).IsRequired(false);
        b.HasOne(x => x.ReviewedByStudent).WithMany().HasForeignKey(x => x.ReviewedByStudentId).IsRequired(false);
        b.HasOne(x => x.CreatedOrganization).WithMany().HasForeignKey(x => x.CreatedOrganizationId).IsRequired(false);

        b.HasIndex(x => new { x.RequesterStudentId, x.Name }).IsUnique()
            .HasFilter("[DeletedAt] IS NULL AND [Status] = 'Pending'");
        b.HasIndex(x => new { x.Status, x.RequestedAt }).HasFilter(Sql.NotDeleted);
        b.HasIndex(x => x.RequesterStudentId);
    }
}
