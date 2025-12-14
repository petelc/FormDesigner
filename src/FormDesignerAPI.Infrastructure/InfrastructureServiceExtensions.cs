using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Infrastructure.Data.Queries;
using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.Infrastructure.DocumentIntelligence;
// using FormDesignerAPI.UseCases.Contributors.List; // Not yet implemented
using FormDesignerAPI.UseCases.Forms.List;
using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.AspNetCore.Identity;


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

    services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<AppDbContext>()
      .AddDefaultTokenProviders();

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
        .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
        // .AddScoped<IListContributorsQueryService, ListContributorsQueryService>() // Not yet implemented
        // .AddScoped<IDeleteContributorService, DeleteContributorService>() // Not yet implemented
        .AddScoped<IListFormsQueryService, ListFormsQueryService>()
        // .AddScoped<IDeleteFormService, FormDeletedService>() // Not yet implemented
        .AddScoped<ITokenClaimService, IdentityTokenClaimService>();

    // FormContext services
    services.AddScoped<IFormRepository, FormRepository>()
        .AddScoped<IFormExtractor, MockFormExtractorService>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
