using CMC.Domain.Entities;
using CMC.Domain.Entities.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CMC.Infrastructure.Persistence.Extensions;

namespace CMC.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Core sets
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Framework> Frameworks => Set<Framework>();
    public DbSet<Revision> Revisions => Set<Revision>();

    // Library & taxonomy
    public DbSet<LibraryControl> LibraryControls => Set<LibraryControl>();
    public DbSet<LibraryScenario> LibraryScenarios => Set<LibraryScenario>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<Tag> Tags => Set<Tag>();

    // Joins (explicit entities)
    public DbSet<LibraryControlFramework> LibraryControlFrameworks => Set<LibraryControlFramework>();
    public DbSet<LibraryControlScenario> LibraryControlScenarios => Set<LibraryControlScenario>();
    public DbSet<LibraryControlIndustry> LibraryControlIndustries => Set<LibraryControlIndustry>();
    public DbSet<FrameworkIndustry> FrameworkIndustries => Set<FrameworkIndustry>();
    public DbSet<CustomerIndustry> CustomerIndustries => Set<CustomerIndustry>();
    public DbSet<LibraryScenarioIndustry> LibraryScenarioIndustries => Set<LibraryScenarioIndustry>();
    public DbSet<LibraryScenarioTag> LibraryScenarioTags => Set<LibraryScenarioTag>();
    public DbSet<LibraryControlTag> LibraryControlTags => Set<LibraryControlTag>();
    public DbSet<ScenarioTag> ScenarioTags => Set<ScenarioTag>();
    public DbSet<ControlScenario>         ControlScenarios         => Set<ControlScenario>();


// 👇 NEU – Provisionierungs-Maps
    public DbSet<ProvisionedScenarioMap> ProvisionedScenarioMaps => Set<ProvisionedScenarioMap>();
    public DbSet<ProvisionedControlMap> ProvisionedControlMaps => Set<ProvisionedControlMap>();

    // Customer data
    public DbSet<Control> Controls => Set<Control>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<Evidence> Evidence => Set<Evidence>();
    public DbSet<ToDo> ToDos => Set<ToDo>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<RiskAcceptance> RiskAcceptances => Set<RiskAcceptance>();

    protected override void ConfigureConventions(ModelConfigurationBuilder cb)
    {
        cb.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
        cb.Properties<DateTimeOffset?>().HaveColumnType("timestamp with time zone");
        cb.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // globaler Soft-Delete-Filter
        modelBuilder.ApplySoftDeleteFilters();

        // ggf. Revisionsmodel
        CMC.Infrastructure.RevisionsRegistration.ConfigureRevisionsModel(modelBuilder);
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

    private static void SetIfExists(EntityEntry entry, string propName, DateTimeOffset value)
    {
        var property = entry.Entity.GetType().GetProperty(propName);
        if (property != null && property.CanWrite)
            property.SetValue(entry.Entity, value);
    }
}
