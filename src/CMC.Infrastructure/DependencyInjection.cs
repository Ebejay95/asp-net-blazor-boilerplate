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
        // Revisions/Soft-Delete Support (IHttpContextAccessor + Interceptor)
        services.AddRevisionsSupport();

        // Database mit Interceptor
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.UseRevisionsInterceptor(sp);
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILibraryFrameworkRepository, LibraryFrameworkRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<UserService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<LibraryFrameworkService>();

        return services;
    }
}
