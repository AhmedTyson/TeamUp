// Date: 2026-11-25
// Purpose: EF Core mapping for StoredImage (DB-stored uploads): clustered surrogate key, required blob/content-type,
// restrictive delete so removing a student does not silently cascade-purge displayable images.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Domain.Entities;

namespace TeamUp.Infrastructure.Persistence.Configurations;

public sealed class StoredImageConfig : IEntityTypeConfiguration<StoredImage>
{
    public void Configure(EntityTypeBuilder<StoredImage> builder)
    {
        builder.HasKey(x => x.StoredImageId);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.UploadedByStudentId);
        builder.Property(x => x.UploadedByStudentId).IsRequired(false);
        builder.HasOne(x => x.UploadedBy)
            .WithMany()
            .HasForeignKey(x => x.UploadedByStudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
