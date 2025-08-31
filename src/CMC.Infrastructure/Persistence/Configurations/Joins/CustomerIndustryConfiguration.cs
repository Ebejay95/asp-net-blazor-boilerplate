using CMC.Domain.Entities;
using CMC.Domain.Entities.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations.Joins
{
    public class CustomerIndustryConfiguration : IEntityTypeConfiguration<CustomerIndustry>
    {
        public void Configure(EntityTypeBuilder<CustomerIndustry> b)
        {
            b.ToTable("CustomerIndustries");
            b.HasKey(x => new { x.CustomerId, x.IndustryId });

            b.HasOne(x => x.Customer)
             .WithMany(c => c.CustomerIndustries)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Industry)
             .WithMany(i => i.CustomerIndustries)
             .HasForeignKey(x => x.IndustryId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.IndustryId);
        }
    }
}
