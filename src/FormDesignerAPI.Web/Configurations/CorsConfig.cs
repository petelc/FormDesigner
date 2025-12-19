namespace FormDesignerAPI.Web.Configurations;

public static class CorsConfig
{
  public const string AllowReactAppPolicy = "AllowReactApp";

  public static IServiceCollection AddCorsConfigs(this IServiceCollection services, IConfiguration configuration)
  {
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                         ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Default React dev servers

    services.AddCors(options =>
    {
      options.AddPolicy(AllowReactAppPolicy, policy =>
      {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
      });

      // Optional: Add a policy for development that allows any origin
      options.AddPolicy("AllowAll", policy =>
      {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
      });
    });

    return services;
  }
}
