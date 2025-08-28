using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CMC.Infrastructure.Persistence;
using CMC.Infrastructure.Persistence.Configurations;
using CMC.Infrastructure.Persistence.Interceptors;

namespace CMC.Infrastructure;

public static class RevisionsRegistration
{
    /// <summary>
    /// Ergänzt bestehende Infrastruktur-Registrierung um Revisions & Soft-Delete.
    /// Kann zusätzlich zu deinem bestehenden AddInfrastructure() aufgerufen werden.
    /// </summary>
    public static IServiceCollection AddRevisionsSupport(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<RevisionInterceptor>();
        return services;
    }

    /// <summary>
    /// Helper zum Einhängen in OnModelCreating deines DbContext.
    /// </summary>
    public static void ConfigureRevisionsModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RevisionConfiguration());
        modelBuilder.ApplySoftDeleteFilters();
    }

    /// <summary>
    /// Helper zum Einhängen in AddDbContext-Options: Interceptor aktivieren.
    /// </summary>
    public static void UseRevisionsInterceptor(this DbContextOptionsBuilder options, IServiceProvider sp)
    {
        options.AddInterceptors(sp.GetRequiredService<RevisionInterceptor>());
    }
}
