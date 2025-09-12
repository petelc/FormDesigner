﻿using FormDesignerAPI.Infrastructure;
//using FormDesignerAPI.ServiceDefaults;
using FormDesignerAPI.UseCases.Contributors.Create;
using FormDesignerAPI.UseCases.Forms.Create;
using FormDesignerAPI.UseCases.Forms.Update;
using FormDesignerAPI.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddFastEndpoints()
  .SwaggerDocument(o =>
      {
        o.ShortSchemaNames = true;
      })
      .AddCommandMiddleware(c =>
      {
        c.Register(typeof(CommandLogger<,>));
      });

// wire up commands
builder.Services.AddTransient<ICommandHandler<CreateContributorCommand2, Result<int>>, CreateContributorCommandHandler2>();

builder.Services.AddTransient<ICommandHandler<CreateFormCommand2, Result<int>>, CreateFormCommandHandler2>();



var app = builder.Build();

// Register all synchronous middleware here
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  // app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
}
else
{
  app.UseDefaultExceptionHandler(); // from FastEndpoints
  app.UseHsts();
}

app.UseFastEndpoints();
app.UseSwaggerGen(); // Includes AddFileServer and static files middleware
app.UseHttpsRedirection(); // Note this will drop Authorization headers

// Now run async startup tasks (database seeding)
await app.UseAppMiddlewareAndSeedDatabase();


app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
