using Ardalis.ListStartupServices;
using FormDesignerAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FormDesignerAPI.Web.Configurations;

public static class MiddlewareConfig
{
  public static async Task<IApplicationBuilder> UseAppMiddlewareAndSeedDatabase(this WebApplication app)
  {
    await SeedDatabase(app);
    return app;
  }

  static async Task SeedDatabase(WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
      logger.LogInformation("Starting database seeding process...");

      var context = services.GetRequiredService<AppDbContext>();

      // Apply any pending migrations
      logger.LogInformation("Applying pending migrations...");
      await context.Database.MigrateAsync();
      logger.LogInformation("Migrations applied successfully.");

      // Ensure database is created (if migrations haven't created it)
      logger.LogInformation("Ensuring database is created...");
      await context.Database.EnsureCreatedAsync();
      logger.LogInformation("Database created/verified.");

      // Seed the database with initial data
      logger.LogInformation("Seeding database with initial data...");
      await SeedData.InitializeAsync(context);
      logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
      throw;
    }
  }
}
