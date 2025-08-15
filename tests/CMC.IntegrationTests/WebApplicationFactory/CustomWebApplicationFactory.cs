using CMC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CMC.Application.Services;
using CMC.Contracts.Users;
using CMC.Application.Ports;

namespace CMC.IntegrationTests.WebApplicationFactory;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class {
  protected override void ConfigureWebHost(IWebHostBuilder builder) {
    builder.ConfigureServices(services => {
      // Remove the app's ApplicationDbContext registration
      var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

      if (descriptor != null)
        services.Remove(descriptor);

      // Add ApplicationDbContext using an in-memory database for testing
      services.AddDbContext<AppDbContext>(options => {
        options.UseInMemoryDatabase("InMemoryDbForTesting");
      });

      // Build the service provider
      var sp = services.BuildServiceProvider();

      // Create a scope to obtain a reference to the database context
      using var scope = sp.CreateScope();
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();
      var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

      try {
        // Ensure the database is created
        db.Database.EnsureCreated();

        // Seed test data
        SeedTestData(scopedServices).GetAwaiter().GetResult();
      } catch (Exception ex) {
        logger.LogError(ex, "An error occurred seeding the database with test data.");
      }
    });

    // Configure test environment
    builder.UseEnvironment("Testing");
  }

  private static async Task SeedTestData(IServiceProvider serviceProvider) {
    var userService = serviceProvider.GetRequiredService<UserService>();
    var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

    // Check if test user already exists
    var existingUser = await userRepository.GetByEmailAsync("test@example.com");

    if (existingUser == null) {
      // Create test user
      await userService.RegisterAsync(new RegisterUserRequest {
        Email = "test@example.com",
        Password = "password123",
        FirstName = "Test",
        LastName = "User"
      });
    }
  }
}
