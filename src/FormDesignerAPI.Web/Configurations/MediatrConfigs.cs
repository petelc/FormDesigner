using Traxs.SharedKernel;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.UseCases.Forms.Create;
using FormDesignerAPI.UseCases.Identity.GetUserProfile;
using FormDesignerAPI.UseCases.FormContext.Mappers;
using FormDesignerAPI.Infrastructure.Data;
using MediatR;
using System.Reflection;

namespace FormDesignerAPI.Web.Configurations;

public static class MediatrConfigs
{
  public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
  {
    var mediatRAssemblies = new[]
      {
        Assembly.GetAssembly(typeof (Form)), // Core
        Assembly.GetAssembly(typeof(CreateFormCommand)),
        Assembly.GetAssembly(typeof(GetUserProfileCommand)), // UseCases
      };

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>()
            .AddScoped<FormDefinitionMapper>();

    return services;
  }
}
