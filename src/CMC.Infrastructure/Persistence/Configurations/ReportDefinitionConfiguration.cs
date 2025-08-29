using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations;

public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.CustomerId).IsRequired();
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Kind).IsRequired().HasMaxLength(64);
        e.Property(x => x.WindowDays).IsRequired();
        e.Property(x => x.Sections).HasColumnType("text");
        e.Property(x => x.CreatedAt).IsRequired();
        e.Property(x => x.UpdatedAt).IsRequired();
        e.Property(x => x.IsDeleted).HasDefaultValue(false);
        e.Property(x => x.DeletedBy).HasMaxLength(320);

        e.HasIndex(x => new { x.CustomerId, x.Name });
    }
}
