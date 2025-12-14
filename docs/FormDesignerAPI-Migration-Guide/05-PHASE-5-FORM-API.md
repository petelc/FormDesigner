# Phase 5: Form Context - API

**Duration:** TBD  
**Complexity:** Medium-High  
**Prerequisites:** Previous phases complete

## Overview

The API layer (Presentation layer) exposes the application functionality through REST endpoints using ASP.NET Core MVC controllers. **evaluate using fast endpoints**

## Objectives

- [ ] Create Controllers folder structure (or fast endpoints folder structure)
- [ ] Create Forms Controller (or needed request/response classes)
- [ ] Create Health Checks Controller
- [ ] Create Middleware folder structure
- [ ] Create Exception Middleware
- [ ] Create Request Logging Middleware
- [ ] Create Filters folder structure
- [ ] Create Form Code Filter
- [ ] Update Program class
- [ ] Create Extensions folder structure
- [ ] Create Service Collection Extensions
- [ ] Create Application Builder Extensions
- [ ] Create Configurations folder structure
- [ ] Create Swagger Configuration
- [ ] Update appsettings
- [ ] Update launch settings if needed
- [ ] Write tests as needed

## Steps

---

## Table of Contents

1. [Controllers](#controllers)
2. [Middleware](#middleware)
3. [Filters](#filters)
4. [Program.cs Setup](#programcs-setup)
5. [API Documentation](#api-documentation)
6. [Error Handling](#error-handling)

---

## Controllers

### FormsController.cs

Handles form analysis operations.

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FormCodeGenerator.Application.Commands.AnalyzeForm;
using FormCodeGenerator.Application.Queries.GetFormAnalysis;

namespace FormCodeGenerator.API.Controllers;

/// <summary>
/// Form analysis endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FormsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FormsController> _logger;

    public FormsController(
        IMediator mediator,
        ILogger<FormsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Analyze a PDF form
    /// </summary>
    /// <param name="file">PDF file to analyze</param>
    /// <returns>Form analysis result</returns>
    /// <response code="200">Form analyzed successfully</response>
    /// <response code="400">Invalid file or validation error</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalyzeFormResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AnalyzeFormResult>> AnalyzeForm(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        _logger.LogInformation("Analyzing form: {FileName}, Size: {Size} bytes", 
            file.FileName, 
            file.Length);

        try
        {
            var command = new AnalyzeFormCommand(
                file.OpenReadStream(),
                file.FileName);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Form analyzed successfully: {FormId}", result.FormId);

            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for form: {FileName}", file.FileName);
            return BadRequest(new ValidationProblemDetails(
                ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing form: {FileName}", file.FileName);
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error analyzing form",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Get form analysis by ID
    /// </summary>
    /// <param name="id">Form ID</param>
    /// <returns>Form analysis details</returns>
    /// <response code="200">Form found</response>
    /// <response code="404">Form not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FormAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormAnalysisDto>> GetFormAnalysis(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting form analysis: {FormId}", id);

        var query = new GetFormAnalysisQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Form not found: {FormId}", id);
            return NotFound(new { error = $"Form with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all forms
    /// </summary>
    /// <returns>List of all forms</returns>
    /// <response code="200">Forms retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FormAnalysisDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FormAnalysisDto>>> GetAllForms(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all forms");

        // This would need a GetAllFormsQuery in the Application layer
        // For now, simplified version
        return Ok(new List<FormAnalysisDto>());
    }

    /// <summary>
    /// Get form fields
    /// </summary>
    /// <param name="id">Form ID</param>
    /// <returns>Form fields</returns>
    /// <response code="200">Fields retrieved</response>
    /// <response code="404">Form not found</response>
    [HttpGet("{id:guid}/fields")]
    [ProducesResponseType(typeof(List<FormFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FormFieldDto>>> GetFormFields(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetFormAnalysisQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = $"Form with ID {id} not found" });
        }

        return Ok(result.Fields);
    }

    /// <summary>
    /// Delete a form
    /// </summary>
    /// <param name="id">Form ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Form deleted</response>
    /// <response code="404">Form not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteForm(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting form: {FormId}", id);

        // This would need a DeleteFormCommand in the Application layer
        // For now, simplified version
        return NoContent();
    }
}
```

---

### CodeGenerationController.cs

Handles code generation operations.

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.IO.Compression;
using System.Text;
using FormCodeGenerator.Application.Commands.GenerateCode;
using FormCodeGenerator.Application.Queries.GetGeneratedCode;

namespace FormCodeGenerator.API.Controllers;

/// <summary>
/// Code generation endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CodeGenerationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CodeGenerationController> _logger;

    public CodeGenerationController(
        IMediator mediator,
        ILogger<CodeGenerationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generate code from a form
    /// </summary>
    /// <param name="request">Code generation request</param>
    /// <returns>Generated code result</returns>
    /// <response code="200">Code generated successfully</response>
    /// <response code="400">Invalid request or validation error</response>
    /// <response code="404">Form not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateCodeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenerateCodeResult>> GenerateCode(
        [FromBody] GenerateCodeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating code for form: {FormId}, FormName: {FormName}",
            request.FormId,
            request.FormName);

        try
        {
            var command = new GenerateCodeCommand(
                request.FormId,
                request.FormName,
                request.FormPurpose,
                request.TargetNamespace);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Code generated successfully: {RequestId}", result.RequestId);

            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for code generation");
            return BadRequest(new ValidationProblemDetails(
                ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Form not found: {FormId}", request.FormId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error generating code",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Get generated code by request ID
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <returns>Generated code details</returns>
    /// <response code="200">Code found</response>
    /// <response code="404">Code generation request not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GeneratedCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneratedCodeDto>> GetGeneratedCode(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting generated code: {RequestId}", id);

        var query = new GetGeneratedCodeQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Code generation request not found: {RequestId}", id);
            return NotFound(new { error = $"Code generation request with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Download generated code as ZIP file
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <returns>ZIP file with all generated code</returns>
    /// <response code="200">ZIP file</response>
    /// <response code="404">Code generation request not found</response>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadGeneratedCode(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading generated code: {RequestId}", id);

        var query = new GetGeneratedCodeQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = $"Code generation request with ID {id} not found" });
        }

        // Create ZIP file in memory
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var codeFile in result.CodeFiles)
            {
                var fileName = codeFile.CodeType switch
                {
                    "Model" => $"{result.FormName}.cs",
                    "Controller" => $"{result.FormName}Controller.cs",
                    "View" => $"{result.FormName}.cshtml",
                    "Migration" => $"Create{result.FormName}Table.cs",
                    _ => $"{codeFile.CodeType}.cs"
                };

                var entry = archive.CreateEntry(fileName);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                await writer.WriteAsync(codeFile.Code);
            }

            // Add README
            var readmeEntry = archive.CreateEntry("README.md");
            using var readmeStream = readmeEntry.Open();
            using var readmeWriter = new StreamWriter(readmeStream, Encoding.UTF8);
            await readmeWriter.WriteAsync($@"# Generated Code for {result.FormName}

Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Request ID: {result.RequestId}
Form ID: {result.FormId}
Namespace: {result.SuggestedNamespace}

## Files Included

{string.Join("\n", result.CodeFiles.Select(f => $"- {f.CodeType}.cs ({f.LineCount} lines)"))}

## Analysis

- Purpose: {result.Analysis?.DetectedPurpose}
- Complexity: {result.Analysis?.Complexity}
- Has Repeating Data: {result.Analysis?.HasRepeatingData}
- Recommended Approach: {result.Analysis?.RecommendedApproach}

## Usage

1. Copy the files to your project
2. Update namespaces if needed
3. Run the migration
4. Build and test
");
        }

        memoryStream.Position = 0;
        var fileBytes = memoryStream.ToArray();

        return File(fileBytes, "application/zip", $"{result.FormName}_Generated.zip");
    }

    /// <summary>
    /// Get generated code for a specific type
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="codeType">Type of code (Model, Controller, View, Migration)</param>
    /// <returns>Code content</returns>
    /// <response code="200">Code retrieved</response>
    /// <response code="404">Code not found</response>
    [HttpGet("{id:guid}/code/{codeType}")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetCodeByType(
        Guid id,
        string codeType,
        CancellationToken cancellationToken)
    {
        var query = new GetGeneratedCodeQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = $"Code generation request with ID {id} not found" });
        }

        var code = result.CodeFiles.FirstOrDefault(
            f => f.CodeType.Equals(codeType, StringComparison.OrdinalIgnoreCase));

        if (code == null)
        {
            return NotFound(new { error = $"Code type '{codeType}' not found" });
        }

        return Content(code.Code, "text/plain", Encoding.UTF8);
    }
}

/// <summary>
/// Request model for code generation
/// </summary>
public class GenerateCodeRequest
{
    public Guid FormId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string? FormPurpose { get; set; }
    public string? TargetNamespace { get; set; }
}
```

---

### BatchController.cs

Handles batch processing operations.

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FormCodeGenerator.Application.Commands.ProcessBatch;

namespace FormCodeGenerator.API.Controllers;

/// <summary>
/// Batch processing endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BatchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BatchController> _logger;

    public BatchController(
        IMediator mediator,
        ILogger<BatchController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Process multiple forms in batch
    /// </summary>
    /// <param name="files">PDF files to process</param>
    /// <param name="targetNamespace">Optional target namespace for all forms</param>
    /// <returns>Batch processing result</returns>
    /// <response code="200">Batch processing completed</response>
    /// <response code="400">Invalid request</response>
    [HttpPost("process")]
    [RequestSizeLimit(100_000_000)] // 100 MB limit
    [ProducesResponseType(typeof(ProcessBatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessBatchResult>> ProcessBatch(
        [FromForm] List<IFormFile> files,
        [FromForm] string? targetNamespace,
        CancellationToken cancellationToken)
    {
        if (files == null || !files.Any())
        {
            return BadRequest(new { error = "No files uploaded" });
        }

        if (files.Count > 100)
        {
            return BadRequest(new { error = "Maximum 100 files allowed per batch" });
        }

        _logger.LogInformation("Processing batch of {Count} forms", files.Count);

        try
        {
            var batchForms = files.Select(f => new BatchFormInput(
                f.OpenReadStream(),
                f.FileName,
                Path.GetFileNameWithoutExtension(f.FileName)
            )).ToList();

            var command = new ProcessBatchCommand(batchForms, targetNamespace);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "Batch processing complete: {Successful}/{Total} successful",
                result.SuccessfulForms,
                result.TotalForms);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            return StatusCode(500, new ProblemDetails
            {
                Title = "Error processing batch",
                Detail = ex.Message,
                Status = 500
            });
        }
    }

    /// <summary>
    /// Get batch processing status (placeholder)
    /// </summary>
    /// <param name="id">Batch ID</param>
    /// <returns>Batch status</returns>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchStatus(Guid id)
    {
        // This would need a GetBatchStatusQuery in the Application layer
        // For now, return placeholder
        return Ok(new
        {
            batchId = id,
            status = "Completed",
            message = "Batch status endpoint - to be implemented"
        });
    }
}
```

---

## Middleware

### ExceptionMiddleware.cs

Global exception handling middleware.

```csharp
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace FormCodeGenerator.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Validation Error";
                response.Status = 400;
                response.Errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Invalid Operation";
                response.Status = 400;
                response.Detail = invalidOpEx.Message;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized";
                response.Status = 401;
                response.Detail = "You are not authorized to access this resource";
                break;

            case KeyNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Title = "Not Found";
                response.Status = 404;
                response.Detail = notFoundEx.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                response.Status = 500;
                response.Detail = _env.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request";

                if (_env.IsDevelopment())
                {
                    response.StackTrace = exception.StackTrace;
                }
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }
}
```

---

### RequestLoggingMiddleware.cs

Logs all HTTP requests and responses.

```csharp
using System.Diagnostics;

namespace FormCodeGenerator.API.Middleware;

/// <summary>
/// Logs all HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log request
        _logger.LogInformation(
            "HTTP {Method} {Path} started",
            context.Request.Method,
            context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

---

## Filters

### ValidationFilter.cs

Automatic model validation filter.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FormCodeGenerator.API.Filters;

/// <summary>
/// Validates model state automatically
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Any() == true)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to do after action execution
    }
}
```

---

## Program.cs Setup

Complete Program.cs with all configurations.

```csharp
using Serilog;
using FormCodeGenerator.Application.Configuration;
using FormCodeGenerator.Infrastructure.Configuration;
using FormCodeGenerator.API.Middleware;
using FormCodeGenerator.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Add Application services (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplicationServices();

// Add Infrastructure services (Azure AI, EF Core, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Form Code Generator API",
        Version = "v1",
        Description = "API for analyzing PDF forms and generating C# code using Azure AI",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FormCodeGenerator.Infrastructure.Persistence.ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Form Code Generator API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Custom middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Apply database migrations
try
{
    Log.Information("Applying database migrations...");
    await app.Services.MigrateDatabase();
    Log.Information("Database migrations applied successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "Error applying database migrations");
}

try
{
    Log.Information("Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## Extensions

### ServiceCollectionExtensions.cs

```csharp
namespace FormCodeGenerator.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Any additional API-specific services can be registered here
        return services;
    }
}
```

### ApplicationBuilderExtensions.cs

```csharp
using FormCodeGenerator.API.Middleware;

namespace FormCodeGenerator.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
```

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


## Verification

- [ ] All tests pass
- [ ] Code compiles

## Next Steps

Continue to next phase.
