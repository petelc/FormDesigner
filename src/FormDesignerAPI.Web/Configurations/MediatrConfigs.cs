using Ardalis.SharedKernel;
using FormDesignerAPI.Core.ContributorAggregate;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Contributors.Create;
using FormDesignerAPI.UseCases.Forms.Create;
using MediatR;
using System.Reflection;

namespace FormDesignerAPI.Web.Configurations;

public static class MediatrConfigs
{
  public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
  {
    var mediatRAssemblies = new[]
      {
        Assembly.GetAssembly(typeof(Contributor)), // Core
        Assembly.GetAssembly(typeof(CreateContributorCommand)), // UseCases
        Assembly.GetAssembly(typeof (Form)), // Core
        Assembly.GetAssembly(typeof(CreateFormCommand)) // UseCases
      };

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    return services;
  }
}
