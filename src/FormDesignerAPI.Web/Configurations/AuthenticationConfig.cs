using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FormDesignerAPI.Infrastructure.Identity;

namespace FormDesignerAPI.Web.Configurations;

public static class AuthenticationConfig
{
  public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var authSettings = configuration.GetSection("Auth").Get<AuthSettings>();
    
    if (authSettings == null || string.IsNullOrWhiteSpace(authSettings.JWT_SECRET_KEY))
    {
      throw new InvalidOperationException("JWT authentication is not properly configured. Please check Auth:JWT_SECRET_KEY in appsettings.json");
    }

    var key = Encoding.ASCII.GetBytes(authSettings.JWT_SECRET_KEY);

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.SaveToken = true;
      options.RequireHttpsMetadata = false; // Set to true in production
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = authSettings.JWT_ISSUER,
        ValidAudience = authSettings.JWT_AUDIENCE,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
      };

      // Add custom event handlers for debugging (optional)
      options.Events = new JwtBearerEvents
      {
        OnAuthenticationFailed = context =>
        {
          if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
          {
            context.Response.Headers.Append("Token-Expired", "true");
          }
          return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
          // Allow token to be sent via query string for SignalR/WebSocket connections
          var accessToken = context.Request.Query["access_token"];
          var path = context.HttpContext.Request.Path;
          
          if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
          {
            context.Token = accessToken;
          }
          return Task.CompletedTask;
        }
      };
    });

    services.AddAuthorization(options =>
    {
      // Add default policies here
      options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
      options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin"));
    });

    return services;
  }
}
