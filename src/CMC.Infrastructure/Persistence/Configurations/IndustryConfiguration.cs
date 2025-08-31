using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class IndustryConfiguration : IEntityTypeConfiguration<Industry>
    {
        public void Configure(EntityTypeBuilder<Industry> entity)
        {
            entity.ToTable("Industries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            // Soft delete properties
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.Property(x => x.DeletedAt);
            entity.Property(x => x.DeletedBy).HasMaxLength(320);

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique(false);
            entity.HasIndex(x => x.IsDeleted);
        }
    }
}
