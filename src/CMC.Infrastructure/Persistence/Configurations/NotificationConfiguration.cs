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

        // Strings speichern, nicht int
        b.Property(x => x.Severity).HasMaxLength(16).IsRequired();
        b.Property(x => x.Status).HasMaxLength(16).IsRequired();

        // Optionale Guardrails
        b.HasCheckConstraint("CK_Notifications_Severity",
            "\"Severity\" IN ('notice','success','error')");
        b.HasCheckConstraint("CK_Notifications_Status",
            "\"Status\" IN ('unread','read')");

        b.HasIndex(x => new { x.UserId, x.Status });
        b.HasIndex(x => x.CreatedAt);
    }
}
