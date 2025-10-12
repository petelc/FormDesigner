﻿using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Infrastructure.Data.Queries;
using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.UseCases.Contributors.List;
using FormDesignerAPI.UseCases.Forms.List;
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
        .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
        .AddScoped<IDeleteContributorService, DeleteContributorService>()
        .AddScoped<IListFormsQueryService, ListFormsQueryService>()
        .AddScoped<IDeleteFormService, FormDeletedService>()
        .AddScoped<ITokenClaimService, IdentityTokenClaimService>();


    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
