using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        b.Property(x => x.Severity).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => new { x.UserId, x.Status });
        b.HasIndex(x => x.CreatedAt);
    }
}
