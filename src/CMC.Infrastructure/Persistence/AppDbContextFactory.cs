using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CMC.Infrastructure.Persistence
{
    // Wird von "dotnet ef" benutzt, um AppDbContext zur Design-Time zu erstellen.
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Umgebung ermitteln
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var basePath = Directory.GetCurrentDirectory();

            // Konfiguration laden (erst lokale appsettings, dann die aus dem Web-Projekt, dann Env)
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddJsonFile(Path.Combine(basePath, "../CMC.Web/appsettings.json"), optional: true)
                .AddJsonFile(Path.Combine(basePath, $"../CMC.Web/appsettings.{env}.json"), optional: true)
                .AddEnvironmentVariables();

            var config = cfgBuilder.Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? "Host=localhost;Port=5432;Database=cmc;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(cs, npgsql =>
                {
                    // Falls du Migrations in CMC.Infrastructure hältst (standard):
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                })
                // Hilfreich bei Migrations/Fehlersuche (kannst du entfernen):
                .EnableSensitiveDataLogging()
                .Options;

            return new AppDbContext(options);
        }
    }
}
