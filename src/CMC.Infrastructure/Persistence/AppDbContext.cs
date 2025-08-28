// src/CMC.Infrastructure/Persistence/AppDbContext.cs
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
    public DbSet<LibraryFramework> LibraryFrameworks => Set<LibraryFramework>();
    public DbSet<LibraryControl> LibraryControls => Set<LibraryControl>();
    public DbSet<Revision> Revisions => Set<Revision>();

protected override void ConfigureConventions(ModelConfigurationBuilder cb)
{
	// vorhandene:
	cb.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
	cb.Properties<DateTimeOffset?>().HaveColumnType("timestamp with time zone");
	cb.Properties<decimal>().HavePrecision(18, 2);

	// ❌ Entfernt: Email-Lambda-Conversions per HaveConversion(...),
	// die verursachen CS1660, weil hier nur Typ-/Converter-Overloads erlaubt sind.
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

	// Revisions-Modell registrieren
	CMC.Infrastructure.RevisionsRegistration.ConfigureRevisionsModel(modelBuilder);

	// ❌ Entfernt: Doppeltes Mapping als Owned Type kollidiert mit HasConversion in UserConfiguration
	// modelBuilder.Entity<User>(b =>
	// {
	// 	b.OwnsOne(u => u.Email, nb =>
	// 	{
	// 		nb.Property(e => e.Value)
	// 		  .HasColumnName("Email")
	// 		  .IsRequired();
	// 	});
	// });
}

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetIfExists(entry, "CreatedAt", now);
                SetIfExists(entry, "UpdatedAt", now);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetIfExists(entry, "UpdatedAt", now);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    // ACHTUNG: Signatur auf DateTimeOffset, damit wir zentral normalisieren können.
    private static void SetIfExists(EntityEntry entry, string propName, DateTimeOffset value)
    {
        var prop = entry.Properties
            .FirstOrDefault(p => string.Equals(p.Metadata.Name, propName, StringComparison.OrdinalIgnoreCase));
        if (prop is null) return;

        var t = prop.Metadata.ClrType;

        // Bevorzugt DateTimeOffset (timestamptz)
        if (t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?))
        {
            prop.CurrentValue = value;
            return;
        }

        // Legacy-Felder als DateTime (timestamp without time zone)
        if (t == typeof(DateTime) || t == typeof(DateTime?))
        {
            // explizit UTC-Kind setzen
            prop.CurrentValue = DateTime.SpecifyKind(value.UtcDateTime, DateTimeKind.Utc);
            return;
        }

        // Optional: String-Fallback, falls jemand CreatedAt als string gespeichert hat
        if (t == typeof(string))
        {
            prop.CurrentValue = value.UtcDateTime.ToString("O");
        }
    }
}
