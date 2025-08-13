using CMC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMC.Infrastructure.Persistence;

public class AppDbContext: DbContext {
  public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

  public DbSet<User> Users => Set<User>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<User>(entity => {
      entity.HasKey(u => u.Id);
      entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
      entity.HasIndex(u => u.Email).IsUnique();
      entity.Property(u => u.PasswordHash).IsRequired();
      entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
      entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
      entity.Property(u => u.CreatedAt).IsRequired();
    });

    // Zukünftige Entities hier hinzufügen:
    // modelBuilder.Entity<Assessment>(entity => {
    //     entity.HasKey(u => u.Id);
    // });
  }
}
