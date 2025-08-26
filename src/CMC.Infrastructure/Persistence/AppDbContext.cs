using CMC.Domain.Entities;
using CMC.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CMC.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- ValueConverter & ValueComparer für Email (VO <-> string) ---
        var emailComparer = new ValueComparer<Email>(
            (l, r) => l.Value == r.Value,
            v => v.Value.GetHashCode(),
            v => new Email(v.Value)
        );

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            // Email als ValueObject -> DB-Spalte string(255), unique
            entity.Property(u => u.Email)
                  .HasConversion(
                      toProvider => toProvider.Value,
                      fromProvider => new Email(fromProvider)
                  )
                  .Metadata.SetValueComparer(emailComparer);
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
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Customer Entity Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Industry).IsRequired().HasMaxLength(100);
            entity.Property(c => c.EmployeeCount).IsRequired();
            entity.Property(c => c.RevenuePerYear).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).IsRequired();
        });

        // Zusätzliche Indizes
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
