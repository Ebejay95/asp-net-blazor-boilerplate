using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Persistence;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User Entity Configuration
        modelBuilder.Entity<User>(entity => {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).HasMaxLength(100);
            entity.Property(u => u.Department).HasMaxLength(100);
            entity.Property(u => u.CreatedAt).IsRequired();

            // Foreign Key Relationship to Customer
            entity.HasOne(u => u.Customer)
                  .WithMany(c => c.Users)
                  .HasForeignKey(u => u.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull); // When customer is deleted, set CustomerId to null
        });

        // Customer Entity Configuration
        modelBuilder.Entity<Customer>(entity => {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(c => c.Name).IsUnique(); // Ensure unique customer names
            entity.Property(c => c.Industry).IsRequired().HasMaxLength(100);
            entity.Property(c => c.EmployeeCount).IsRequired();
            entity.Property(c => c.RevenuePerYear).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).IsRequired();

            // One-to-Many Relationship with Users
            entity.HasMany(c => c.Users)
                  .WithOne(u => u.Customer)
                  .HasForeignKey(u => u.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Additional indexes for performance
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Industry)
            .HasDatabaseName("IX_Customers_Industry");

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Customers_IsActive");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.CustomerId)
            .HasDatabaseName("IX_Users_CustomerId");
    }
}
