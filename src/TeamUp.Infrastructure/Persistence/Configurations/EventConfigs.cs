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
        b.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class OrganizationMemberConfig : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> b)
    {
        b.HasKey(x => new { x.OrganizationId, x.StudentId });
        b.Property(x => x.Role).HasMaxLength(60).IsRequired();
        b.Property(x => x.JoinedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => x.StudentId);
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
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => new { x.OrganizationId, x.StudentId }).IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Status] = 'Pending'");
        b.HasIndex(x => x.StudentId);
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
        b.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
        b.HasIndex(x => x.OrganizationId);
        b.HasIndex(x => x.StartDate);
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
        b.HasIndex(x => x.StudentId);
    }
}
