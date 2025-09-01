using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class LibraryControlIndustryConfiguration : IEntityTypeConfiguration<LibraryControlIndustry>
    {
        public void Configure(EntityTypeBuilder<LibraryControlIndustry> e)
        {
            e.ToTable("LibraryControlIndustries");
            e.HasKey(x => new { x.LibraryControlId, x.IndustryId });

            e.HasOne(x => x.LibraryControl)
                .WithMany(lc => lc.IndustryLinks)
                .HasForeignKey(x => x.LibraryControlId)
                .OnDelete(DeleteBehavior.Cascade);

            // ← Backref setzen (nutzt Industry.LibraryControlIndustries)
            e.HasOne(x => x.Industry)
                .WithMany(i => i.LibraryControlIndustries)
                .HasForeignKey(x => x.IndustryId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.IndustryId);
        }
    }
}
