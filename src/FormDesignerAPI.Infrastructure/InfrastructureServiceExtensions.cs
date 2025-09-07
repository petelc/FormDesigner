using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Infrastructure.Data.Queries;
using FormDesignerAPI.UseCases.Contributors.List;


namespace FormDesignerAPI.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    string? connectionString = config.GetConnectionString("SqliteConnection");
    Guard.Against.Null(connectionString);

    services.AddDbContext<AppDbContext>(options =>
      options.UseSqlite(connectionString));

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
        .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
        .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
        .AddScoped<IDeleteContributorService, DeleteContributorService>()
        .AddScoped<FormDesignerAPI.UseCases.Forms.List.IListFormsQueryService, FormDesignerAPI.Infrastructure.Data.Queries.ListFormsQueryService>()
        .AddScoped<FormDesignerAPI.Core.Interfaces.IDeleteFormService, FormDesignerAPI.Core.Services.FormDeletedService>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
