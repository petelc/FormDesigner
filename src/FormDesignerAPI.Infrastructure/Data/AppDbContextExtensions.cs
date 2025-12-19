namespace FormDesignerAPI.Infrastructure.Data;

/// <summary>
/// Extension methods for configuring AppDbContext with SQL Server
/// </summary>
public static class AppDbContextExtensions
{
  /// <summary>
  /// Adds AppDbContext with SQL Server configuration
  /// </summary>
  public static void AddApplicationDbContext(this IServiceCollection services, string connectionString) =>
    services.AddDbContext<AppDbContext>(options =>
         options.UseSqlServer(connectionString));
}
