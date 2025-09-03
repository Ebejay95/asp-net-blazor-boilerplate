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

            // 1:n
            // Users: darf losgelöst werden, daher SetNull
            b.HasMany(x => x.Users)
             .WithOne(u => u.Customer)
             .HasForeignKey(u => u.CustomerId)
             .OnDelete(DeleteBehavior.SetNull);

            // Controls: required FK (CustomerId nicht nullable) -> beim Löschen von Customer
            // müssen Controls mit gelöscht (soft-deleted) werden -> Cascade
            b.HasMany(x => x.Controls)
             .WithOne(c => c.Customer)
             .HasForeignKey(c => c.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
