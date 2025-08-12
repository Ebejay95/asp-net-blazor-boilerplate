using CMC.Application.Ports;
using CMC.Application.Services;
using CMC.Infrastructure.Persistence;
using CMC.Infrastructure.Repositories;
using CMC.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMC.Infrastructure;

public static class DependencyInjection {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
    // Database
    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();

    // Services
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<UserService>();

    return services;
  }
}
