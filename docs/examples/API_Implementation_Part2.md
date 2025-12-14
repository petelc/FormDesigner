# API Layer Implementation Guide - Part 2

## API Documentation, Configuration, and Testing

---

## Swagger/OpenAPI Configuration

### Enhanced Swagger Setup

Add to Program.cs or create SwaggerConfiguration.cs:

```csharp
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace FormCodeGenerator.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Form Code Generator API",
                Version = "v1",
                Description = @"
## Overview
API for analyzing PDF forms and generating C# code using Azure AI services.

## Features
- **Form Analysis**: Extract structure from AcroForm, XFA, Hybrid, and Static PDFs
- **Code Generation**: Generate Model, Controller, View, and Migration code
- **Batch Processing**: Process multiple forms in a single request

## Authentication
Currently no authentication required. Add JWT/OAuth2 for production.

## Rate Limits
- Form Analysis: 15 requests/second
- Code Generation: 5 requests/second
- Batch Processing: 1 request/minute

## Support
For issues or questions, contact support@example.com
",
                Contact = new OpenApiContact
                {
                    Name = "Form Code Generator Team",
                    Email = "support@example.com",
                    Url = new Uri("https://github.com/yourorg/form-code-generator")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Include XML comments from all projects
            var xmlFiles = new[]
            {
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                "FormCodeGenerator.Application.xml",
                "FormCodeGenerator.Domain.xml"
            };

            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // Add file upload support
            options.OperationFilter<FileUploadOperationFilter>();

            // Add authorization (if implementing authentication)
            // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            // {
            //     Description = "JWT Authorization header using the Bearer scheme",
            //     Name = "Authorization",
            //     In = ParameterLocation.Header,
            //     Type = SecuritySchemeType.ApiKey,
            //     Scheme = "Bearer"
            // });
        });

        return services;
    }
}

/// <summary>
/// Operation filter to support file uploads in Swagger UI
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadMime = "multipart/form-data";
        
        if (operation.RequestBody == null || 
            !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.OrdinalIgnoreCase)))
            return;

        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                       p.ParameterType == typeof(List<IFormFile>));

        operation.RequestBody.Content[fileUploadMime].Schema.Properties =
            fileParams.ToDictionary(
                k => k.Name!,
                v => new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                });
    }
}
```

---

## appsettings Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "FormCodeGenerator": "Debug"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FormCodeGeneratorDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },

  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-DOCUMENT-INTELLIGENCE-KEY"
      },
      "OpenAI": {
        "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
        "ApiKey": "YOUR-OPENAI-KEY",
        "DeploymentName": "gpt-4",
        "Temperature": 0.3,
        "MaxTokens": 4000
      }
    }
  },

  "Persistence": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false
  },

  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "FormCodeGenerator": "Debug"
    }
  },

  "Persistence": {
    "EnableSensitiveDataLogging": true
  },

  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "FormCodeGenerator": "Information"
    }
  },

  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "${AZURE_DOC_INTEL_ENDPOINT}",
        "ApiKey": "${AZURE_DOC_INTEL_KEY}"
      },
      "OpenAI": {
        "Endpoint": "${AZURE_OPENAI_ENDPOINT}",
        "ApiKey": "${AZURE_OPENAI_KEY}",
        "DeploymentName": "${AZURE_OPENAI_DEPLOYMENT}"
      }
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "${SQL_CONNECTION_STRING}"
  }
}
```

---

## launchSettings.json

Configure for development:

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  },
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:48392",
      "sslPort": 44309
    }
  }
}
```

---

## Health Checks

### HealthCheckController.cs (Optional)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FormCodeGenerator.API.Controllers;

/// <summary>
/// Health check endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Get health status
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(503, response);
    }
}
```

---

## API Testing

### Functional Tests Setup

#### WebApplicationFactory Setup

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Infrastructure.Persistence;

namespace FormCodeGenerator.API.FunctionalTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}
```

#### Controller Tests

```csharp
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using FormCodeGenerator.Application.Commands.AnalyzeForm;

namespace FormCodeGenerator.API.FunctionalTests.Controllers;

public class FormsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public FormsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AnalyzeForm_WithValidPdf_ReturnsOk()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var pdfBytes = CreateTestPdf(); // Helper method to create test PDF
        var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "test.pdf");

        // Act
        var response = await _client.PostAsync("/api/forms/analyze", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AnalyzeFormResult>();
        result.Should().NotBeNull();
        result!.FormId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeForm_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/forms/analyze", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFormAnalysis_WithValidId_ReturnsOk()
    {
        // Arrange
        var formId = await CreateTestForm();

        // Act
        var response = await _client.GetAsync($"/api/forms/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFormAnalysis_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/forms/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> CreateTestForm()
    {
        var content = new MultipartFormDataContent();
        var pdfBytes = CreateTestPdf();
        var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "test.pdf");

        var response = await _client.PostAsync("/api/forms/analyze", content);
        var result = await response.Content.ReadFromJsonAsync<AnalyzeFormResult>();
        
        return result!.FormId;
    }

    private byte[] CreateTestPdf()
    {
        // Create a minimal valid PDF for testing
        // In real tests, use a proper PDF library or test file
        return new byte[] { /* minimal PDF bytes */ };
    }
}
```

---

## Integration Tests

### CodeGenerationController Tests

```csharp
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using FormCodeGenerator.Application.Commands.GenerateCode;
using FormCodeGenerator.API.Controllers;

namespace FormCodeGenerator.API.FunctionalTests.Controllers;

public class CodeGenerationControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CodeGenerationControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateCode_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var formId = await CreateTestFormAsync();
        var request = new GenerateCodeRequest
        {
            FormId = formId,
            FormName = "TestForm",
            FormPurpose = "Testing",
            TargetNamespace = "MyApp.Tests"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/codegeneration/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<GenerateCodeResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.GeneratedFiles.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGeneratedCode_WithValidId_ReturnsOk()
    {
        // Arrange
        var requestId = await GenerateTestCodeAsync();

        // Act
        var response = await _client.GetAsync($"/api/codegeneration/{requestId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DownloadGeneratedCode_WithValidId_ReturnsZipFile()
    {
        // Arrange
        var requestId = await GenerateTestCodeAsync();

        // Act
        var response = await _client.GetAsync($"/api/codegeneration/{requestId}/download");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/zip");
    }

    private async Task<Guid> CreateTestFormAsync()
    {
        // Helper method implementation
        return Guid.NewGuid();
    }

    private async Task<Guid> GenerateTestCodeAsync()
    {
        // Helper method implementation
        return Guid.NewGuid();
    }
}
```

---

## Docker Support

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FormCodeGenerator.API/FormCodeGenerator.API.csproj", "src/FormCodeGenerator.API/"]
COPY ["src/FormCodeGenerator.Application/FormCodeGenerator.Application.csproj", "src/FormCodeGenerator.Application/"]
COPY ["src/FormCodeGenerator.Domain/FormCodeGenerator.Domain.csproj", "src/FormCodeGenerator.Domain/"]
COPY ["src/FormCodeGenerator.Infrastructure/FormCodeGenerator.Infrastructure.csproj", "src/FormCodeGenerator.Infrastructure/"]
RUN dotnet restore "src/FormCodeGenerator.API/FormCodeGenerator.API.csproj"
COPY . .
WORKDIR "/src/src/FormCodeGenerator.API"
RUN dotnet build "FormCodeGenerator.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FormCodeGenerator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FormCodeGenerator.API.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    image: formcodegenerator-api
    build:
      context: .
      dockerfile: src/FormCodeGenerator.API/Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=FormCodeGeneratorDb;User=sa;Password=YourStrong@Password;TrustServerCertificate=True
      - Azure__AI__DocumentIntelligence__Endpoint=${AZURE_DOC_INTEL_ENDPOINT}
      - Azure__AI__DocumentIntelligence__ApiKey=${AZURE_DOC_INTEL_KEY}
      - Azure__AI__OpenAI__Endpoint=${AZURE_OPENAI_ENDPOINT}
      - Azure__AI__OpenAI__ApiKey=${AZURE_OPENAI_KEY}
      - Azure__AI__OpenAI__DeploymentName=${AZURE_OPENAI_DEPLOYMENT}
    depends_on:
      - sqlserver
    networks:
      - formcodegen-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - formcodegen-network

networks:
  formcodegen-network:
    driver: bridge

volumes:
  sqlserver-data:
```

---

## CI/CD

### GitHub Actions Workflow

`.github/workflows/dotnet.yml`:

```yaml
name: .NET

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal
    
    - name: Publish
      run: dotnet publish src/FormCodeGenerator.API/FormCodeGenerator.API.csproj -c Release -o ./publish
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: api-artifact
        path: ./publish

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Download artifact
      uses: actions/download-artifact@v3
      with:
        name: api-artifact
        path: ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'your-app-name'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## NuGet Packages Required

### FormCodeGenerator.API.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\FormCodeGenerator.Application\FormCodeGenerator.Application.csproj" />
    <ProjectReference Include="..\FormCodeGenerator.Infrastructure\FormCodeGenerator.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="Microsoft.AspNetCore.HealthChecks.EntityFrameworkCore" Version="8.0.0" />
  </ItemGroup>
</Project>
```

---

## Summary

The API layer provides:

✅ **REST Controllers** - Forms, CodeGeneration, Batch  
✅ **Middleware** - Exception handling, Request logging  
✅ **Filters** - Automatic validation  
✅ **Swagger/OpenAPI** - Interactive documentation  
✅ **Health Checks** - Monitoring endpoints  
✅ **Configuration** - All environments  
✅ **Testing** - Functional and integration tests  
✅ **Docker** - Container support  
✅ **CI/CD** - GitHub Actions workflow  

Your complete DDD solution is now ready!
