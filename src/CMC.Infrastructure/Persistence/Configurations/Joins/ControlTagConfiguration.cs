using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class ControlTagConfiguration : IEntityTypeConfiguration<ControlTag>
    {
        public void Configure(EntityTypeBuilder<ControlTag> e)
        {
            e.ToTable("ControlTags");
            e.HasKey(x => new { x.ControlId, x.TagId });

            e.HasOne(x => x.Control)
             .WithMany(c => c.TagLinks)
             .HasForeignKey(x => x.ControlId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Tag)
             .WithMany(t => t.ControlTags)
             .HasForeignKey(x => x.TagId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.TagId);
        }
    }
}
