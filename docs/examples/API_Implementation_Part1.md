# API Layer Implementation Guide

## Overview

The API layer (Presentation layer) exposes the application functionality through REST endpoints using ASP.NET Core MVC controllers.

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

Continued in Part 2... [API Implementation (part 2)](API_Implementation_Part2.md)

