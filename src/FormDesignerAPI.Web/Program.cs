using Ardalis.ListStartupServices;
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Core.Services;
using FormDesignerAPI.Infrastructure;
using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.UseCases.Forms.Create;
using FormDesignerAPI.UseCases.Identity.Register;
using FormDesignerAPI.UseCases.Interfaces;
using FormDesignerAPI.Web.Configurations;
using FormDesignerAPI.Web.Services;


var builder = WebApplication.CreateBuilder(args);
// Register AuthSettings for options pattern
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
builder.Services.AddServiceConfigs(appLogger, builder);

// Register services from other projects
builder.AddServiceDefaults();
builder.Services.AddInfrastructureServices(builder.Configuration, appLogger);
builder.Services.AddMediatrConfigs();

// Add CORS configuration
builder.Services.AddCorsConfigs(builder.Configuration);

// Add JWT Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add HttpContextAccessor and CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUser, CurrentUserService>();

builder.Services.AddFastEndpoints()
  .SwaggerDocument(o =>
  {
    o.ShortSchemaNames = true;
    
    // Add JWT Bearer authentication to Swagger
    o.DocumentSettings = s =>
    {
      s.Title = "FormDesigner API";
      s.Version = "v1";
      s.Description = "API for form analysis and code generation";
      
      // Add JWT Bearer security definition
      s.AddAuth("Bearer", new()
      {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token in the format: Bearer {your token}"
      });
    };
  });
 

builder.Services.AddTransient<ICommandHandler<RegisterUserCommand, Result<string>>, RegisterUserHandler>();

builder.Services.AddTransient<IIdentityService, IdentityService>();



var app = builder.Build();

// Register all synchronous middleware here
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
}
else
{
  app.UseDefaultExceptionHandler(); // from FastEndpoints
  app.UseHsts();
}

// IMPORTANT: Middleware order matters!
// 1. CORS must be before authentication
app.UseCors(CorsConfig.AllowReactAppPolicy);

// 2. Authentication must be before authorization
app.UseAuthentication();

// 3. Authorization must be before endpoints
app.UseAuthorization();

// 4. FastEndpoints (includes routing and endpoint mapping)
app.UseFastEndpoints();

// 5. Swagger
app.UseSwaggerGen(); // Includes AddFileServer and static files middleware

// 6. HTTPS redirection (Note: This may drop Authorization headers in some scenarios)
// app.UseHttpsRedirection(); // Commented out - enable only if needed

// Now run async startup tasks (database seeding)
await app.UseAppMiddlewareAndSeedDatabase();


app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
