using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CMC.Infrastructure.Persistence.Configurations;


public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
public void Configure(EntityTypeBuilder<Customer> entity)
{
entity.HasKey(c => c.Id);
entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
entity.HasIndex(c => c.Name).IsUnique();


entity.Property(c => c.EmployeeCount).IsRequired();
entity.Property(c => c.RevenuePerYear).HasPrecision(18, 2).IsRequired();
entity.Property(c => c.IsActive).HasDefaultValue(true).IsRequired();
entity.Property(c => c.CreatedAt).IsRequired();
entity.Property(c => c.UpdatedAt).IsRequired();


entity.HasIndex(c => c.IsActive).HasDatabaseName("IX_Customers_IsActive");
}
}
