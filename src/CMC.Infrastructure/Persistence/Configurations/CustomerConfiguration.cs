using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMC.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> b)
        {
            b.ToTable("Customers");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.Name).IsUnique();

            b.Property(x => x.EmployeeCount).IsRequired();
            b.Property(x => x.RevenuePerYear).HasPrecision(18, 2).IsRequired();

            b.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();

            b.HasIndex(x => x.IsActive).HasDatabaseName("IX_Customers_IsActive");

            // Keine Skip-Navigationen mehr! Join wird separat über CustomerIndustryConfiguration gemappt.
            // 1:n
            b.HasMany(x => x.Users).WithOne(u => u.Customer).HasForeignKey(u => u.CustomerId).OnDelete(DeleteBehavior.SetNull);
            b.HasMany(x => x.Controls).WithOne(c => c.Customer).HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
