# Infrastructure Layer Implementation Guide - Part 2

## Repositories, CodeGeneration Configuration, Events, and DI Setup

---

## Entity Configurations (Continued)

### CodeGenerationRequestConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for CodeGenerationRequest entity
/// </summary>
public class CodeGenerationRequestConfiguration : IEntityTypeConfiguration<CodeGenerationRequest>
{
    public void Configure(EntityTypeBuilder<CodeGenerationRequest> builder)
    {
        builder.ToTable("CodeGenerationRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FormId)
            .IsRequired();

        builder.Property(r => r.FormName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.FormPurpose)
            .HasMaxLength(1000);

        builder.Property(r => r.RequestedAt)
            .IsRequired();

        builder.Property(r => r.CompletedAt);

        builder.Property(r => r.FailedAt);

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        // Enum - Status
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Value object - CodeNamespace
        builder.OwnsOne(r => r.TargetNamespace, ns =>
        {
            ns.Property(n => n.Value)
                .HasColumnName("TargetNamespace")
                .HasMaxLength(500)
                .IsRequired();
        });

        // Value object - CodeAnalysis (optional)
        builder.OwnsOne(r => r.Analysis, a =>
        {
            a.Property(x => x.DetectedPurpose)
                .HasColumnName("AnalysisDetectedPurpose")
                .HasMaxLength(500);

            a.Property(x => x.Complexity)
                .HasColumnName("AnalysisComplexity")
                .HasMaxLength(50);

            a.Property(x => x.HasRepeatingData)
                .HasColumnName("AnalysisHasRepeatingData");

            a.Property(x => x.RecommendedApproach)
                .HasColumnName("AnalysisRecommendedApproach")
                .HasMaxLength(1000);
        });

        // Collection - GeneratedCodes
        builder.HasMany(r => r.GeneratedCodes)
            .WithOne()
            .HasForeignKey("RequestId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);

        // Indexes
        builder.HasIndex(r => r.FormId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.RequestedAt);
    }
}
```

### GeneratedCodeConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for GeneratedCode entity
/// </summary>
public class GeneratedCodeConfiguration : IEntityTypeConfiguration<GeneratedCode>
{
    public void Configure(EntityTypeBuilder<GeneratedCode> builder)
    {
        builder.ToTable("GeneratedCodes");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.CodeType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(g => g.Language)
            .IsRequired()
            .HasMaxLength(50);

        // Index
        builder.HasIndex(g => g.CodeType);
    }
}
```

---

## Repositories

### FormRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for Form aggregate
/// </summary>
public class FormRepository : IFormRepository
{
    private readonly ApplicationDbContext _context;

    public FormRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Form?> GetByFileNameAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .FirstOrDefaultAsync(f => f.FileName == fileName, cancellationToken);
    }

    public async Task<List<Form>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Form form, CancellationToken cancellationToken = default)
    {
        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Form form, CancellationToken cancellationToken = default)
    {
        _context.Forms.Update(form);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var form = await GetByIdAsync(id, cancellationToken);
        if (form != null)
        {
            _context.Forms.Remove(form);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### CodeGenerationRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CodeGenerationRequest aggregate
/// </summary>
public class CodeGenerationRepository : ICodeGenerationRepository
{
    private readonly ApplicationDbContext _context;

    public CodeGenerationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CodeGenerationRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetByFormIdAsync(
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .Where(r => r.FormId == formId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .Where(r => r.Status == CodeGenerationStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CodeGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _context.CodeGenerationRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CodeGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        _context.CodeGenerationRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await GetByIdAsync(id, cancellationToken);
        if (request != null)
        {
            _context.CodeGenerationRequests.Remove(request);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

---

## Missing Domain Code

Before we can use the repositories, we need to add the missing CodeGenerationRequest aggregate. Let me create that:

### CodeGenerationRequest.cs (Domain Layer)

```csharp
using FormCodeGenerator.Domain.Events;
using FormCodeGenerator.Domain.ValueObjects;

namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// CodeGenerationRequest aggregate root
/// </summary>
public class CodeGenerationRequest
{
    public Guid Id { get; private set; }
    public Guid FormId { get; private set; }
    public string FormName { get; private set; }
    public string? FormPurpose { get; private set; }
    public CodeNamespace TargetNamespace { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public CodeGenerationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<GeneratedCode> _generatedCodes;
    public IReadOnlyList<GeneratedCode> GeneratedCodes => _generatedCodes.AsReadOnly();

    public CodeAnalysis? Analysis { get; private set; }

    // Domain events
    private readonly List<object> _domainEvents;
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private CodeGenerationRequest()
    {
        // Required by EF Core
        FormName = string.Empty;
        TargetNamespace = new CodeNamespace("MyApp");
        _generatedCodes = new List<GeneratedCode>();
        _domainEvents = new List<object>();
        Status = CodeGenerationStatus.Pending;
    }

    public CodeGenerationRequest(
        Guid formId,
        string formName,
        string? formPurpose,
        CodeNamespace targetNamespace)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty", nameof(formId));

        if (string.IsNullOrWhiteSpace(formName))
            throw new ArgumentException("Form name cannot be empty", nameof(formName));

        Id = Guid.NewGuid();
        FormId = formId;
        FormName = formName;
        FormPurpose = formPurpose;
        TargetNamespace = targetNamespace ?? throw new ArgumentNullException(nameof(targetNamespace));
        RequestedAt = DateTime.UtcNow;
        Status = CodeGenerationStatus.Pending;
        
        _generatedCodes = new List<GeneratedCode>();
        _domainEvents = new List<object>();
    }

    public void AddGeneratedCode(string codeType, string code, string language)
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Cannot add code to a completed or failed request");

        var generatedCode = new GeneratedCode(codeType, code, language);
        _generatedCodes.Add(generatedCode);
    }

    public void SetAnalysis(CodeAnalysis analysis)
    {
        Analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
    }

    public void MarkAsCompleted()
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Request is not in pending state");

        if (!_generatedCodes.Any())
            throw new InvalidOperationException("Cannot complete request without generated code");

        Status = CodeGenerationStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        // Raise domain event
        _domainEvents.Add(new CodeGeneratedEvent(
            Id,
            FormId,
            FormName,
            TargetNamespace.Value,
            _generatedCodes.Count));
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Request is not in pending state");

        Status = CodeGenerationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### GeneratedCode.cs (Entity)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Generated code entity
/// </summary>
public class GeneratedCode
{
    public Guid Id { get; private set; }
    public string CodeType { get; private set; }
    public string Code { get; private set; }
    public string Language { get; private set; }

    private GeneratedCode()
    {
        // Required by EF Core
        CodeType = string.Empty;
        Code = string.Empty;
        Language = string.Empty;
    }

    public GeneratedCode(string codeType, string code, string language)
    {
        if (string.IsNullOrWhiteSpace(codeType))
            throw new ArgumentException("Code type cannot be empty", nameof(codeType));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty", nameof(language));

        Id = Guid.NewGuid();
        CodeType = codeType;
        Code = code;
        Language = language;
    }
}
```

### CodeGenerationStatus.cs (Enum)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Status of code generation request
/// </summary>
public enum CodeGenerationStatus
{
    Pending,
    Completed,
    Failed
}
```

### CodeAnalysis.cs (Value Object)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Analysis of generated code
/// </summary>
public sealed class CodeAnalysis : IEquatable<CodeAnalysis>
{
    public string DetectedPurpose { get; }
    public string Complexity { get; }
    public bool HasRepeatingData { get; }
    public string RecommendedApproach { get; }

    public CodeAnalysis(
        string detectedPurpose,
        string complexity,
        bool hasRepeatingData,
        string recommendedApproach)
    {
        if (string.IsNullOrWhiteSpace(detectedPurpose))
            throw new ArgumentException("Detected purpose cannot be empty", nameof(detectedPurpose));

        if (string.IsNullOrWhiteSpace(complexity))
            throw new ArgumentException("Complexity cannot be empty", nameof(complexity));

        if (string.IsNullOrWhiteSpace(recommendedApproach))
            throw new ArgumentException("Recommended approach cannot be empty", nameof(recommendedApproach));

        DetectedPurpose = detectedPurpose;
        Complexity = complexity;
        HasRepeatingData = hasRepeatingData;
        RecommendedApproach = recommendedApproach;
    }

    public bool Equals(CodeAnalysis? other)
    {
        if (other is null) return false;
        return DetectedPurpose == other.DetectedPurpose &&
               Complexity == other.Complexity &&
               HasRepeatingData == other.HasRepeatingData &&
               RecommendedApproach == other.RecommendedApproach;
    }

    public override bool Equals(object? obj) => Equals(obj as CodeAnalysis);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(DetectedPurpose, Complexity, HasRepeatingData, RecommendedApproach);
    }
}
```

### ICodeGenerationRepository.cs (Domain Interface)

```csharp
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Domain.Interfaces;

/// <summary>
/// Repository for CodeGenerationRequest aggregate
/// </summary>
public interface ICodeGenerationRepository
{
    Task<CodeGenerationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetByFormIdAsync(Guid formId, CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CodeGenerationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(CodeGenerationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

---

## Configuration Options

### AzureAIOptions.cs

```csharp
namespace FormCodeGenerator.Infrastructure.Options;

/// <summary>
/// Azure AI service configuration options
/// </summary>
public class AzureAIOptions
{
    public const string SectionName = "Azure:AI";

    public DocumentIntelligenceOptions DocumentIntelligence { get; set; } = new();
    public OpenAIOptions OpenAI { get; set; } = new();
}

public class DocumentIntelligenceOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAIOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4";
    public float Temperature { get; set; } = 0.3f;
    public int MaxTokens { get; set; } = 4000;
}
```

### PersistenceOptions.cs

```csharp
namespace FormCodeGenerator.Infrastructure.Options;

/// <summary>
/// Persistence configuration options
/// </summary>
public class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}
```

---

## Event Publishing

### IEventPublisher.cs (Application Interface)

```csharp
namespace FormCodeGenerator.Application.Interfaces;

/// <summary>
/// Publishes domain events
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
```

### EventPublisher.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Infrastructure.Events;

/// <summary>
/// Simple in-memory event publisher
/// For production, replace with Azure Service Bus, RabbitMQ, etc.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventType = @event.GetType().Name;
        
        _logger.LogInformation("Publishing event: {EventType}", eventType);
        
        // In production, publish to message queue/event bus
        // For now, just log it
        _logger.LogDebug("Event data: {@Event}", @event);

        return Task.CompletedTask;
    }
}
```

### Event Handlers (Examples)

#### FormAnalyzedEventHandler.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Events;

namespace FormCodeGenerator.Infrastructure.Events.EventHandlers;

/// <summary>
/// Handles FormAnalyzedEvent
/// </summary>
public class FormAnalyzedEventHandler
{
    private readonly ILogger<FormAnalyzedEventHandler> _logger;

    public FormAnalyzedEventHandler(ILogger<FormAnalyzedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FormAnalyzedEvent @event)
    {
        _logger.LogInformation(
            "Form analyzed: {FileName} (ID: {FormId}), {FieldCount} fields, {TableCount} tables",
            @event.FileName,
            @event.FormId,
            @event.FieldCount,
            @event.TableCount);

        // Could trigger notifications, analytics, etc.

        return Task.CompletedTask;
    }
}
```

#### CodeGeneratedEventHandler.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Events;

namespace FormCodeGenerator.Infrastructure.Events.EventHandlers;

/// <summary>
/// Handles CodeGeneratedEvent
/// </summary>
public class CodeGeneratedEventHandler
{
    private readonly ILogger<CodeGeneratedEventHandler> _logger;

    public CodeGeneratedEventHandler(ILogger<CodeGeneratedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CodeGeneratedEvent @event)
    {
        _logger.LogInformation(
            "Code generated: {FormName} (Request: {RequestId}), {FileCount} files",
            @event.FormName,
            @event.RequestId,
            @event.GeneratedFilesCount);

        // Could send notifications, save to blob storage, etc.

        return Task.CompletedTask;
    }
}
```

---

## Dependency Injection Setup

### InfrastructureServiceExtensions.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Application.Interfaces;
using FormCodeGenerator.Domain.Interfaces;
using FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;
using FormCodeGenerator.Infrastructure.AI.AzureOpenAI;
using FormCodeGenerator.Infrastructure.Events;
using FormCodeGenerator.Infrastructure.Events.EventHandlers;
using FormCodeGenerator.Infrastructure.Options;
using FormCodeGenerator.Infrastructure.Persistence;
using FormCodeGenerator.Infrastructure.Persistence.Repositories;

namespace FormCodeGenerator.Infrastructure.Configuration;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration options
        services.Configure<AzureAIOptions>(
            configuration.GetSection(AzureAIOptions.SectionName));

        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            // Enable sensitive data logging only in development
            if (configuration.GetValue<bool>("Persistence:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Repositories
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<ICodeGenerationRepository, CodeGenerationRepository>();

        // Azure AI Services
        services.AddScoped<IFormExtractor, AzureFormExtractor>();
        services.AddScoped<ICodeGenerator, AzureCodeGenerator>();

        // Event Publishing
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<FormAnalyzedEventHandler>();
        services.AddSingleton<CodeGeneratedEventHandler>();

        return services;
    }

    public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
}
```

---

## appsettings.json Example

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
  }
}
```

---

## Database Migrations

### Creating Initial Migration

```bash
# From solution root
dotnet ef migrations add InitialCreate \
  --project src/FormCodeGenerator.Infrastructure \
  --startup-project src/FormCodeGenerator.API \
  --output-dir Persistence/Migrations
```

### Applying Migrations

```bash
# Apply to database
dotnet ef database update \
  --project src/FormCodeGenerator.Infrastructure \
  --startup-project src/FormCodeGenerator.API

# Or programmatically in Program.cs:
# await app.Services.MigrateDatabase();
```

### Migration File Example

The generated migration will create:
- Forms table
- FormFields table
- FormTables table
- CodeGenerationRequests table
- GeneratedCodes table
- All necessary indexes and foreign keys

---

## Testing Infrastructure

### Integration Test Example

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Infrastructure.Persistence;
using FormCodeGenerator.Infrastructure.Configuration;

namespace FormCodeGenerator.Infrastructure.IntegrationTests;

public class InfrastructureTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public ApplicationDbContext DbContext { get; }

    public InfrastructureTestFixture()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        // Add infrastructure services
        services.AddInfrastructureServices(configuration);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
```

---

## Summary

The Infrastructure layer provides:

✅ **Azure Document Intelligence** - PDF form extraction  
✅ **Azure OpenAI** - Code generation  
✅ **Entity Framework Core** - Persistence  
✅ **Repositories** - Data access  
✅ **Event Publishing** - Domain events  
✅ **Configuration** - Options pattern  
✅ **Dependency Injection** - Service registration  

Next: API layer with controllers! [API Implementation (part 1)](API_Implementation_Part1.md)

