using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using FormDesignerAPI.Infrastructure;
using FormDesignerAPI.Infrastructure.Email;

namespace FormDesignerAPI.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger, WebApplicationBuilder builder)
  {
    services.AddMediatrConfigs();

    // Add authentication and authorization
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();


    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      //services.AddScoped<IEmailSender, MimeKitEmailSender>();

      // Otherwise use this:
      builder.Services.AddScoped<IEmailSender, FakeEmailSender>();

    }
    else
    {
      services.AddScoped<IEmailSender, MimeKitEmailSender>();
    }

    services.AddScoped<IFormDefinitionService, FormDefinitionService>();
    services.AddScoped<IHtmlFormElementParser, HtmlFormElementParser>();

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    return services;
  }


}
