using CMC.Application.Ports;
using CMC.Application.Services;
using CMC.Infrastructure.Persistence;
using CMC.Infrastructure.Repositories;
using CMC.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Revisions/Soft-Delete Support
        services.AddRevisionsSupport();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.UseRevisionsInterceptor(sp);
        });

        // Repositories – existing
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IFrameworkRepository, FrameworkRepository>();

        // Repositories – new
        services.AddScoped<IControlRepository, ControlRepository>();
        services.AddScoped<IScenarioRepository, ScenarioRepository>();
        services.AddScoped<IEvidenceRepository, EvidenceRepository>();
        services.AddScoped<IToDoRepository, ToDoRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportDefinitionRepository, ReportDefinitionRepository>();
        services.AddScoped<ILibraryControlRepository, LibraryControlRepository>();
        services.AddScoped<ILibraryScenarioRepository, LibraryScenarioRepository>();
        services.AddScoped<IIndustryRepository, IndustryRepository>();
        services.AddScoped<IRiskAcceptanceRepository, RiskAcceptanceRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<UserService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<FrameworkService>();
        services.AddScoped<RevisionService>();
        services.AddScoped<RecycleBinService>();
        services.AddScoped<IndustryService>();

        services.AddScoped<LibraryProvisioningService>();

        return services;
    }
}
