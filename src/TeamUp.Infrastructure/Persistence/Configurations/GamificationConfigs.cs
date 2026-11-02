using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence.Configurations;

public class ScoreTransactionConfig : IEntityTypeConfiguration<ScoreTransaction>
{
    public void Configure(EntityTypeBuilder<ScoreTransaction> b)
    {
        b.HasKey(x => x.TransactionId);
        b.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => new { x.StudentId, x.CreatedAt });
        // Ledger has no DeletedAt of its own; hide rows whose student was soft-deleted.
        b.HasQueryFilter(x => x.Student.DeletedAt == null);
    }
}

public class BadgeConfig : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> b)
    {
        b.HasKey(x => x.BadgeId);
        b.Property(x => x.Name).HasMaxLength(80).IsRequired();
        b.Property(x => x.Description).HasMaxLength(250).IsRequired();
        b.Property(x => x.IconUrl).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public class StudentBadgeConfig : IEntityTypeConfiguration<StudentBadge>
{
    public void Configure(EntityTypeBuilder<StudentBadge> b)
    {
        b.HasKey(x => new { x.StudentId, x.BadgeId });
        b.Property(x => x.AwardedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasOne(x => x.Badge).WithMany().HasForeignKey(x => x.BadgeId);
        b.HasIndex(x => x.BadgeId);
    }
}

public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.HasKey(x => x.NotificationId);
        b.Property(x => x.Title).HasMaxLength(120).IsRequired();
        b.Property(x => x.Message).HasMaxLength(500).IsRequired();
        b.Property(x => x.IsRead).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        b.HasIndex(x => new { x.StudentId, x.IsRead, x.CreatedAt });
    }
}
