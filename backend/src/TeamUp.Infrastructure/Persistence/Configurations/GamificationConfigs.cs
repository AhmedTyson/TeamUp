using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence.Configurations;

public sealed class ScoreTransactionConfig : IEntityTypeConfiguration<ScoreTransaction>
{
    public void Configure(EntityTypeBuilder<ScoreTransaction> builder)
    {
        builder.HasKey(x => x.TransactionId);
        builder.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasIndex(x => new { x.StudentId, x.CreatedAt });
        builder.HasIndex(x => new { x.StudentId, x.SourceType, x.SourceReferenceId })
            .IsUnique()
            .HasFilter("[SourceReferenceId] IS NOT NULL");
        // Ledger has no DeletedAt of its own; hide rows whose student was soft-deleted.
        builder.HasQueryFilter(QueryFilterNames.SoftDelete, x => x.Student.DeletedAt == null);
    }
}

public sealed class BadgeConfig : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.HasKey(x => x.BadgeId);
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(250).IsRequired();
        builder.Property(x => x.IconUrl).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasFilter(Sql.NotDeleted);
    }
}

public sealed class StudentBadgeConfig : IEntityTypeConfiguration<StudentBadge>
{
    public void Configure(EntityTypeBuilder<StudentBadge> builder)
    {
        builder.HasKey(x => new { x.StudentId, x.BadgeId });
        builder.Property(x => x.AwardedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasOne(x => x.Badge).WithMany().HasForeignKey(x => x.BadgeId);
        builder.HasIndex(x => x.BadgeId);
    }
}

public sealed class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.NotificationId);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsRead).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId);
        builder.HasIndex(x => new { x.StudentId, x.IsRead, x.CreatedAt });
    }
}
