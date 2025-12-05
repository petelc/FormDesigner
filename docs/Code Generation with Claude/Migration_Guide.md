# FormDesignerAPI Migration Guide
## Integrating DDD Bounded Contexts into Ardalis Clean Architecture

**Version:** 1.0  
**Date:** December 2024  
**Author:** Architecture Team  
**Target Solution:** FormDesignerAPI

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Prerequisites](#2-prerequisites)
3. [Understanding the Architecture](#3-understanding-the-architecture)
4. [Current State Analysis](#4-current-state-analysis)
5. [Target State Architecture](#5-target-state-architecture)
6. [Phase 1: Foundation (Week 1)](#phase-1-foundation-week-1)
7. [Phase 2: Form Context - Domain (Week 1-2)](#phase-2-form-context---domain-week-1-2)
8. [Phase 3: Form Context - Infrastructure (Week 2)](#phase-3-form-context---infrastructure-week-2)
9. [Phase 4: Form Context - Use Cases & API (Week 3)](#phase-4-form-context---use-cases--api-week-3)
10. [Phase 5: Claude API Integration (Week 3-4)](#phase-5-claude-api-integration-week-3-4)
11. [Phase 6: Import Context (Week 4-5)](#phase-6-import-context-week-4-5)
12. [Phase 7: Code Generation Context (Week 5-7)](#phase-7-code-generation-context-week-5-7)
13. [Phase 8: Integration & Events (Week 7-8)](#phase-8-integration--events-week-7-8)
14. [Phase 9: Testing & Documentation (Week 8)](#phase-9-testing--documentation-week-8)
15. [Troubleshooting Guide](#troubleshooting-guide)
16. [Appendix A: Complete Code Examples](#appendix-a-complete-code-examples)
17. [Appendix B: Glossary](#appendix-b-glossary)

---

## 1. Executive Summary

### 1.1 What This Guide Covers

This migration guide will help you transform your existing Ardalis CleanArchitecture template into a Domain-Driven Design (DDD) solution with three bounded contexts:

1. **Form Context** - Manages form definitions and revisions
2. **Import Context** - Handles PDF upload and AI-powered form extraction
3. **Code Generation Context** - Generates complete application code from forms

### 1.2 Migration Timeline

- **Estimated Duration:** 6-8 weeks
- **Complexity Level:** Intermediate to Advanced
- **Team Size Recommendation:** 2-3 developers

### 1.3 What You'll Achieve

By the end of this migration, you'll have:
- ✅ A fully functional AI-powered code generation system
- ✅ Three independently deployable bounded contexts
- ✅ Integration with Claude Sonnet 4 API
- ✅ Complete test coverage
- ✅ Production-ready CI/CD pipelines

### 1.4 Key Decisions Made

| Decision | Rationale |
|----------|-----------|
| Keep Ardalis structure | Proven, well-documented, follows Clean Architecture |
| Organize by Bounded Context | Clear separation of concerns, independent evolution |
| Single database initially | Simpler to start, can split later if needed |
| Event-driven communication | Loose coupling between contexts |
| PostgreSQL + JSONB | Flexible schema storage for form definitions |

---

## 2. Prerequisites

### 2.1 Required Tools

Check your environment:
```bash
# Check versions
dotnet --version        # Should be 8.0 or higher
node --version          # Should be 20.x or higher
npm --version           # Should be 10.x or higher
docker --version        # For local PostgreSQL

# Install EF Core tools
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

### 2.2 Required NuGet Packages

You'll add these as you progress through the phases:
```xml
<!-- FormDesignerAPI.SharedKernel -->
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />

<!-- FormDesignerAPI.Core -->
<PackageReference Include="Ardalis.GuardClauses" Version="4.5.0" />

<!-- FormDesignerAPI.Infrastructure -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- FormDesignerAPI.UseCases -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.0" />

<!-- FormDesignerAPI.UnitTests -->
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

### 2.3 Environment Setup

**PostgreSQL Database (Using Docker):**
```bash
# Start PostgreSQL container
docker run --name FormDesignerAPI-db \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=FormDesignerAPI \
  -p 5432:5432 \
  -d postgres:15

# Verify it's running
docker ps

# Connect to verify
docker exec -it FormDesignerAPI-db psql -U postgres -d FormDesignerAPI
```

**Anthropic API Key:**

1. Sign up at https://console.anthropic.com/
2. Navigate to API Keys section
3. Create a new API key
4. Store it securely (we'll add to configuration in Phase 5)

### 2.4 Backup Current Solution

Before starting, create a backup:
```bash
# Create a git branch
git checkout -b feature/ddd-migration

# Or create a full copy
cp -r FormDesignerAPI FormDesignerAPI.backup

# Commit current state
git add .
git commit -m "Checkpoint before DDD migration"
```

---

## 3. Understanding the Architecture

### 3.1 Current Ardalis Structure

Your solution currently follows this structure:
```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.Core/          # Domain entities & interfaces
│   ├── FormDesignerAPI.Infrastructure/ # Data access & external services
│   ├── FormDesignerAPI.UseCases/      # Application logic
│   └── FormDesignerAPI.Web/           # API endpoints
└── tests/
    ├── FormDesignerAPI.FunctionalTests/
    ├── FormDesignerAPI.IntegrationTests/
    └── FormDesignerAPI.UnitTests/
```

**Key Characteristics:**
- ✅ Clean separation of concerns
- ✅ Dependency inversion principle
- ✅ Testable architecture
- ❌ No bounded context separation
- ❌ Generic entity structure

### 3.2 Target DDD Structure

We'll reorganize to this:
```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.SharedKernel/        # NEW - Shared abstractions
│   ├── FormDesignerAPI.Core/                # REFACTOR - Organize by context
│   │   ├── FormContext/
│   │   ├── ImportContext/
│   │   └── CodeGenerationContext/
│   ├── FormDesignerAPI.UseCases/            # REFACTOR - Organize by context
│   ├── FormDesignerAPI.Infrastructure/      # REFACTOR - Organize by context
│   └── FormDesignerAPI.Web/                 # REFACTOR - Organize by context
└── tests/
```

### 3.3 Bounded Context Overview
```
┌──────────────────────────────────────────────────────────┐
│                   FORM CONTEXT                            │
│  Responsibilities:                                        │
│  - Manage form definitions                               │
│  - Track form revisions                                  │
│  - Store form metadata                                   │
│                                                          │
│  Publishes:                                              │
│  - FormCreated                                           │
│  - FormRevisionCreated                                   │
└────────────────┬─────────────────────────────────────────┘
                 │
                 │ Events
                 │
        ┌────────┴────────┐
        │                 │
        ▼                 ▼
┌─────────────────┐  ┌──────────────────────┐
│ IMPORT CONTEXT  │  │ CODE GEN CONTEXT     │
│                 │  │                      │
│ Publishes:      │  │ Subscribes to:       │
│ - FormCandidate │  │ - FormCreated        │
│   Approved      │  │                      │
│                 │  │ Publishes:           │
│                 │  │ - CodeArtifacts      │
│                 │  │   Generated          │
└─────────────────┘  └──────────────────────┘
        │
        │ Event
        ▼
┌─────────────────┐
│  FORM CONTEXT   │
│  (Creates Form) │
└─────────────────┘
```

### 3.4 Clean Architecture Layers

Each bounded context follows Clean Architecture:
```
┌─────────────────────────────────────┐
│     Presentation Layer              │ ← FormDesignerAPI.Web
│     (Controllers, APIs)             │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Application Layer               │ ← FormDesignerAPI.UseCases
│   (Use Cases, Commands, Queries)   │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Domain Layer                    │ ← FormDesignerAPI.Core
│  (Aggregates, Entities, Events)    │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Infrastructure Layer            │ ← FormDesignerAPI.Infrastructure
│ (Data Access, External Services)   │
└─────────────────────────────────────┘
```

**Dependency Rule:** Dependencies point inward. Domain has NO dependencies on outer layers.

---

## 4. Current State Analysis

### 4.1 Pre-Migration Checklist

Before starting, verify your current state:

- [ ] Solution builds successfully: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Database connection works (if applicable)
- [ ] You have recent backups
- [ ] Team is aware of migration plans
- [ ] Git branch created for migration

### 4.2 Identify Existing Code to Preserve

**Keep These:**
- ✅ Existing authentication/authorization setup
- ✅ Logging infrastructure (Serilog)
- ✅ API conventions (controllers, routing)
- ✅ Test infrastructure
- ✅ Configuration patterns

**Will Be Replaced:**
- ❌ Generic entity base classes (replace with SharedKernel)
- ❌ Flat domain model (organize by bounded context)
- ❌ Generic repositories (create context-specific)

### 4.3 Migration Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Database schema conflicts | Medium | High | Use migrations, test thoroughly |
| Breaking existing APIs | Low | Medium | Version APIs, maintain backward compatibility |
| Performance degradation | Low | Medium | Load test after each phase |
| Team learning curve | High | Low | Provide training, pair programming |
| Incomplete domain understanding | Medium | High | Domain modeling workshops before coding |

---

## 5. Target State Architecture

### 5.1 Final Folder Structure
```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.SharedKernel/
│   │   ├── Base/
│   │   │   ├── EntityBase.cs
│   │   │   ├── ValueObject.cs
│   │   │   └── DomainEventBase.cs
│   │   ├── Interfaces/
│   │   │   ├── IAggregateRoot.cs
│   │   │   ├── IRepository.cs
│   │   │   └── IDomainEvent.cs
│   │   └── Results/
│   │       └── Result.cs
│   │
│   ├── FormDesignerAPI.Core/
│   │   ├── FormContext/
│   │   │   ├── Aggregates/
│   │   │   │   ├── Form.cs
│   │   │   │   └── FormRevision.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── FormDefinition.cs
│   │   │   │   ├── OriginMetadata.cs
│   │   │   │   └── OriginType.cs
│   │   │   ├── Events/
│   │   │   │   ├── FormCreatedEvent.cs
│   │   │   │   └── FormRevisionCreatedEvent.cs
│   │   │   └── Interfaces/
│   │   │       └── IFormRepository.cs
│   │   │
│   │   ├── ImportContext/
│   │   │   ├── Aggregates/
│   │   │   │   ├── ImportBatch.cs
│   │   │   │   └── ImportedFormCandidate.cs
│   │   │   ├── ValueObjects/
│   │   │   ├── Services/
│   │   │   │   └── PdfExtractionService.cs
│   │   │   ├── Events/
│   │   │   └── Interfaces/
│   │   │
│   │   └── CodeGenerationContext/
│   │       ├── Aggregates/
│   │       │   └── CodeGenerationJob.cs
│   │       ├── ValueObjects/
│   │       ├── Templates/
│   │       │   ├── ICodeTemplate.cs
│   │       │   ├── CSharp/
│   │       │   ├── Sql/
│   │       │   ├── React/
│   │       │   └── CICD/
│   │       ├── Services/
│   │       ├── Events/
│   │       └── Interfaces/
│   │
│   ├── FormDesignerAPI.UseCases/
│   │   ├── FormContext/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── ImportContext/
│   │   └── CodeGenerationContext/
│   │
│   ├── FormDesignerAPI.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── FormContext/
│   │   │   ├── ImportContext/
│   │   │   └── CodeGenerationContext/
│   │   ├── Repositories/
│   │   ├── ExternalServices/
│   │   │   ├── ClaudeApiClient.cs
│   │   │   └── AnthropicSettings.cs
│   │   ├── BackgroundJobs/
│   │   └── Integration/
│   │
│   └── FormDesignerAPI.Web/
│       ├── Controllers/
│       │   ├── FormContext/
│       │   ├── ImportContext/
│       │   └── CodeGenerationContext/
│       ├── Configuration/
│       ├── appsettings.json
│       └── Program.cs
│
└── tests/
    ├── FormDesignerAPI.UnitTests/
    ├── FormDesignerAPI.IntegrationTests/
    └── FormDesignerAPI.FunctionalTests/
```

### 5.2 Database Schema Overview

**Forms Table:**
```sql
CREATE TABLE "Forms" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "DefinitionSchema" JSONB NOT NULL,
    "DefinitionFields" JSONB NOT NULL,
    "OriginType" VARCHAR(50) NOT NULL,
    "OriginReferenceId" VARCHAR(100),
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL
);

CREATE INDEX "IX_Forms_Name" ON "Forms"("Name");
CREATE INDEX "IX_Forms_CreatedAt" ON "Forms"("CreatedAt");
CREATE INDEX "IX_Forms_OriginType" ON "Forms"("OriginType");
```

**FormRevisions Table:**
```sql
CREATE TABLE "FormRevisions" (
    "Id" UUID PRIMARY KEY,
    "FormId" UUID NOT NULL,
    "Version" INT NOT NULL,
    "DefinitionSchema" JSONB NOT NULL,
    "DefinitionFields" JSONB NOT NULL,
    "Notes" VARCHAR(1000),
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL,
    FOREIGN KEY ("FormId") REFERENCES "Forms"("Id") ON DELETE CASCADE,
    UNIQUE ("FormId", "Version")
);

CREATE INDEX "IX_FormRevisions_FormId_Version" ON "FormRevisions"("FormId", "Version");
```

**Import Batches Table:**
```sql
CREATE TABLE "ImportBatches" (
    "Id" UUID PRIMARY KEY,
    "Status" VARCHAR(50) NOT NULL,
    "UploadedFiles" JSONB NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL,
    "CompletedAt" TIMESTAMP
);
```

**Code Generation Jobs Table:**
```sql
CREATE TABLE "CodeGenerationJobs" (
    "Id" UUID PRIMARY KEY,
    "FormDefinitionId" UUID NOT NULL,
    "FormRevisionId" UUID NOT NULL,
    "Version" VARCHAR(20) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "Options" JSONB NOT NULL,
    "ZipFilePath" VARCHAR(500),
    "RequestedAt" TIMESTAMP NOT NULL,
    "RequestedBy" VARCHAR(100) NOT NULL
);
```

---

## Phase 1: Foundation (Week 1)

### Step 1: Create SharedKernel Project

**1.1 Create the project:**
```bash
cd src
dotnet new classlib -n FormDesignerAPI.SharedKernel
dotnet sln ../FormDesignerAPI.sln add FormDesignerAPI.SharedKernel/FormDesignerAPI.SharedKernel.csproj
```

**1.2 Add NuGet packages:**
```bash
cd FormDesignerAPI.SharedKernel
dotnet add package MediatR.Contracts
```

**1.3 Create folder structure:**
```bash
mkdir Base
mkdir Interfaces
mkdir Results
```

**1.4 Create EntityBase:**

Create `Base/EntityBase.cs`:
```csharp
using FormDesignerAPI.SharedKernel.Interfaces;

namespace FormDesignerAPI.SharedKernel.Base;

/// <summary>
/// Base class for all entities with domain event support
/// </summary>
public abstract class EntityBase
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events that have occurred on this entity
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents.AsReadOnly();

    /// <summary>
    /// Register a domain event
    /// </summary>
    protected void RegisterDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clear all domain events (typically after publishing)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**1.5 Create IAggregateRoot:**

Create `Interfaces/IAggregateRoot.cs`:
```csharp
namespace FormDesignerAPI.SharedKernel.Interfaces;

/// <summary>
/// Marker interface for aggregate roots
/// An aggregate root is the entry point for all operations on an aggregate
/// </summary>
public interface IAggregateRoot
{
    // This is a marker interface - no methods required
}
```

**1.6 Create IDomainEvent:**

Create `Interfaces/IDomainEvent.cs`:
```csharp
using MediatR;

namespace FormDesignerAPI.SharedKernel.Interfaces;

/// <summary>
/// Base interface for all domain events
/// Domain events represent something that happened in the domain
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}
```

**1.7 Create DomainEventBase:**

Create `Base/DomainEventBase.cs`:
```csharp
using FormDesignerAPI.SharedKernel.Interfaces;

namespace FormDesignerAPI.SharedKernel.Base;

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
```

**1.8 Create ValueObject:**

Create `Base/ValueObject.cs`:
```csharp
namespace FormDesignerAPI.SharedKernel.Base;

/// <summary>
/// Base class for value objects
/// Value objects are immutable and equality is based on their properties
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
```

**1.9 Create IRepository:**

Create `Interfaces/IRepository.cs`:
```csharp
namespace FormDesignerAPI.SharedKernel.Interfaces;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

**1.10 Create Result:**

Create `Results/Result.cs`:
```csharp
namespace FormDesignerAPI.SharedKernel.Results;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation with a value
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private set; }

    protected Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => 
        new(value, true, null);

    public static new Result<T> Failure(string error) => 
        new(default, false, error);
}
```

**1.11 Add reference to Core project:**
```bash
cd ../FormDesignerAPI.Core
dotnet add reference ../FormDesignerAPI.SharedKernel/FormDesignerAPI.SharedKernel.csproj
```

**1.12 Build and verify:**
```bash
cd ../..
dotnet build
```

### Checkpoint 1: Verification

- [ ] SharedKernel project builds successfully
- [ ] All base classes compile without errors
- [ ] Core project can reference SharedKernel
- [ ] Solution builds: `dotnet build`

**Git Commit:**
```bash
git add .
git commit -m "Phase 1: Created SharedKernel with base classes and interfaces"
```

---

## Phase 2: Form Context - Domain (Week 1-2)

### Step 2: Create Form Context Domain Model

**2.1 Create folder structure:**
```bash
cd src/FormDesignerAPI.Core
mkdir -p FormContext/Aggregates
mkdir -p FormContext/ValueObjects
mkdir -p FormContext/Events
mkdir -p FormContext/Interfaces
```

**2.2 Create OriginType enum:**

Create `FormContext/ValueObjects/OriginType.cs`:
```csharp
namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents how a form was created
/// </summary>
public enum OriginType
{
    /// <summary>
    /// Manually created by a user
    /// </summary>
    Manual,
    
    /// <summary>
    /// Created from an imported PDF
    /// </summary>
    Import,
    
    /// <summary>
    /// Created via API
    /// </summary>
    API,
    
    /// <summary>
    /// Created from a template
    /// </summary>
    Template
}
```

**2.3 Create OriginMetadata:**

Create `FormContext/ValueObjects/OriginMetadata.cs`:
```csharp
namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Tracks how and when a form was created
/// </summary>
public record OriginMetadata
{
    public OriginType Type { get; init; }
    public string? ReferenceId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    
    /// <summary>
    /// Create origin metadata for a manually created form
    /// </summary>
    public static OriginMetadata Manual(string createdBy) => new()
    {
        Type = OriginType.Manual,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = createdBy
    };
    
    /// <summary>
    /// Create origin metadata for an imported form
    /// </summary>
    public static OriginMetadata Import(string candidateId, string approvedBy) => new()
    {
        Type = OriginType.Import,
        ReferenceId = candidateId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = approvedBy
    };
    
    /// <summary>
    /// Create origin metadata for an API-created form
    /// </summary>
    public static OriginMetadata Api(string createdBy) => new()
    {
        Type = OriginType.API,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = createdBy
    };
}
```

**2.4 Create FormDefinition:**

Create `FormContext/ValueObjects/FormDefinition.cs`:
```csharp
using System.Text.Json;

namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents the structure and fields of a form
/// </summary>
public record FormDefinition
{
    public string Schema { get; init; } = string.Empty;
    public List<FormField> Fields { get; init; } = new();

    /// <summary>
    /// Create a form definition from a JSON schema
    /// </summary>
    public static FormDefinition From(string jsonSchema)
    {
        return new FormDefinition
        {
            Schema = jsonSchema,
            Fields = ParseFields(jsonSchema)
        };
    }

    private static List<FormField> ParseFields(string jsonSchema)
    {
        try
        {
            // TODO: Implement proper JSON schema parsing
            // For now, return empty list
            return new List<FormField>();
        }
        catch
        {
            return new List<FormField>();
        }
    }
}

/// <summary>
/// Represents a single field in a form
/// </summary>
public record FormField
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Label { get; init; }
    public string? Placeholder { get; init; }
    public Dictionary<string, object>? ValidationRules { get; init; }
}
```

**2.5 Create Domain Events:**

Create `FormContext/Events/FormCreatedEvent.cs`:
```csharp
using FormDesignerAPI.SharedKernel.Base;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form is created
/// </summary>
public record FormCreatedEvent(
    Guid FormId,
    string Name,
    OriginMetadata Origin,
    string CreatedBy
) : DomainEventBase;
```

Create `FormContext/Events/FormRevisionCreatedEvent.cs`:
```csharp
using FormDesignerAPI.SharedKernel.Base;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form revision is created
/// </summary>
public record FormRevisionCreatedEvent(
    Guid FormId,
    Guid RevisionId,
    int Version,
    string CreatedBy
) : DomainEventBase;
```

**2.6 Create Form Aggregate:**

Create `FormContext/Aggregates/Form.cs`:
```csharp
using Ardalis.GuardClauses;
using FormDesignerAPI.SharedKernel.Base;
using FormDesignerAPI.SharedKernel.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Events;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// Form aggregate root - manages form definitions and their revisions
/// </summary>
public class Form : EntityBase, IAggregateRoot
{
    // Properties
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public FormDefinition Definition { get; private set; } = null!;
    public OriginMetadata Origin { get; private set; } = null!;
    
    private readonly List<FormRevision> _revisions = new();
    public IReadOnlyCollection<FormRevision> Revisions => _revisions.AsReadOnly();
    
    /// <summary>
    /// Get the most recent revision
    /// </summary>
    public FormRevision CurrentRevision => 
        _revisions.OrderByDescending(r => r.CreatedAt).First();

    // Private constructor for EF Core
    private Form() {}
    /// <summary>
    /// Factory method to create a new form
    /// </summary>
    public static Form Create(
        string name,
        FormDefinition definition,
        OriginMetadata origin,
        string createdBy)
    {
        // Guard clauses
        Guard.Against.NullOrEmpty(name, nameof(name));
        Guard.Against.Null(definition, nameof(definition));
        Guard.Against.Null(origin, nameof(origin));
        Guard.Against.NullOrEmpty(createdBy, nameof(createdBy));
        
        var form = new Form
        {
            Id = Guid.NewGuid(),
            Name = name,
            Definition = definition,
            Origin = origin
        };
        
        // Create initial revision
        var initialRevision = FormRevision.Create(
            form.Id, 
            definition, 
            "Initial version",
            createdBy,
            1
        );
        
        form._revisions.Add(initialRevision);
        
        // Raise domain event
        form.RegisterDomainEvent(new FormCreatedEvent(
            form.Id,
            form.Name,
            origin,
            createdBy
        ));
        
        return form;
    }
    
    /// <summary>
    /// Create a new revision of this form
    /// </summary>
    public void CreateRevision(
        FormDefinition newDefinition,
        string notes,
        string createdBy)
    {
        Guard.Against.Null(newDefinition, nameof(newDefinition));
        Guard.Against.NullOrEmpty(createdBy, nameof(createdBy));
        
        var nextVersion = _revisions.Max(r => r.Version) + 1;
        
        var revision = FormRevision.Create(
            Id,
            newDefinition,
            notes,
            createdBy,
            nextVersion
        );
        
        _revisions.Add(revision);
        Definition = newDefinition;
        
        RegisterDomainEvent(new FormRevisionCreatedEvent(
            Id,
            revision.Id,
            revision.Version,
            createdBy
        ));
    }
    
    /// <summary>
    /// Update the form name
    /// </summary>
    public void Rename(string newName)
    {
        Guard.Against.NullOrEmpty(newName, nameof(newName));
        Name = newName;
    }
}

/// <summary>
/// Form revision entity - represents a version of a form
/// </summary>
public class FormRevision : EntityBase
{
    public Guid Id { get; private set; }
    public Guid FormId { get; private set; }
    public int Version { get; private set; }
    public FormDefinition Definition { get; private set; } = null!;
    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    // Private constructor for EF Core
    private FormRevision() { }
    
    /// <summary>
    /// Factory method to create a new revision
    /// </summary>
    internal static FormRevision Create(
        Guid formId,
        FormDefinition definition,
        string notes,
        string createdBy,
        int version)
    {
        Guard.Against.Default(formId, nameof(formId));
        Guard.Against.Null(definition, nameof(definition));
        Guard.Against.NullOrEmpty(createdBy, nameof(createdBy));
        Guard.Against.NegativeOrZero(version, nameof(version));
        
        return new FormRevision
        {
            Id = Guid.NewGuid(),
            FormId = formId,
            Version = version,
            Definition = definition,
            Notes = notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}


**2.7 Create Repository Interface:**

Create `FormContext/Interfaces/IFormRepository.cs`:
```csharp
using FormDesignerAPI.SharedKernel.Interfaces;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Interfaces;

/// <summary>
/// Repository interface for Form aggregate
/// </summary>
public interface IFormRepository : IRepository<Form>
{
    /// <summary>
    /// Get form with all its revisions
    /// </summary>
    Task<Form?> GetByIdWithRevisionsAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get forms by origin type
    /// </summary>
    Task<IEnumerable<Form>> GetByOriginTypeAsync(
        ValueObjects.OriginType originType,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search forms by name
    /// </summary>
    Task<IEnumerable<Form>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);
}
```

**2.8 Build and verify:**
```bash
cd ../..
dotnet build
```

### Checkpoint 2: Verification

- [ ] All domain classes compile
- [ ] No dependencies on infrastructure
- [ ] Domain events are properly defined
- [ ] Aggregate enforces invariants (guard clauses)

**Git Commit:**
```bash
git add .
git commit -m "Phase 2: Created Form Context domain model with aggregates, value objects, and events"
```

---

## Phase 3: Form Context - Infrastructure (Week 2)

### Step 3: Create Infrastructure Layer

**3.1 Install required packages:**
```bash
cd src/FormDesignerAPI.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**3.2 Create folder structure:**
```bash
mkdir -p Data/FormContext
mkdir -p Repositories/FormContext
```

**3.3 Create EF Core Configuration:**

Create `Data/FormContext/FormConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.Data.FormContext;

/// <summary>
/// EF Core configuration for Form aggregate
/// </summary>
public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");
        
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        // Configure FormDefinition as owned type (stored as JSONB)
        builder.OwnsOne(f => f.Definition, def =>
        {
            def.Property(d => d.Schema)
                .HasColumnType("jsonb")
                .HasColumnName("DefinitionSchema")
                .IsRequired();
            
            def.Property(d => d.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("DefinitionFields")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<FormField>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });
        
        // Configure OriginMetadata as owned type
        builder.OwnsOne(f => f.Origin, origin =>
        {
            origin.Property(o => o.Type)
                .HasColumnName("OriginType")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            origin.Property(o => o.ReferenceId)
                .HasColumnName("OriginReferenceId")
                .HasMaxLength(100);
            
            origin.Property(o => o.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();
            
            origin.Property(o => o.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(100)
                .IsRequired();
        });
        
        // Configure Revisions relationship
        builder.HasMany(typeof(FormRevision))
            .WithOne()
            .HasForeignKey("FormId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Ignore domain events (not persisted)
        builder.Ignore(f => f.DomainEvents);
        
        // Indexes
        builder.HasIndex(f => f.Name);
        builder.HasIndex("CreatedAt");
        builder.HasIndex("OriginType");
    }
}
```

Create `Data/FormContext/FormRevisionConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.Data.FormContext;

/// <summary>
/// EF Core configuration for FormRevision entity
/// </summary>
public class FormRevisionConfiguration : IEntityTypeConfiguration<FormRevision>
{
    public void Configure(EntityTypeBuilder<FormRevision> builder)
    {
        builder.ToTable("FormRevisions");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.FormId)
            .IsRequired();
        
        builder.Property(r => r.Version)
            .IsRequired();
        
        builder.Property(r => r.Notes)
            .HasMaxLength(1000);
        
        builder.Property(r => r.CreatedAt)
            .IsRequired();
        
        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
        
        // Configure FormDefinition
        builder.OwnsOne(r => r.Definition, def =>
        {
            def.Property(d => d.Schema)
                .HasColumnType("jsonb")
                .HasColumnName("DefinitionSchema")
                .IsRequired();
            
            def.Property(d => d.Fields)
                .HasColumnType("jsonb")
                .HasColumnName("DefinitionFields")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<FormField>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });
        
        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
        
        // Unique constraint on FormId + Version
        builder.HasIndex(r => new { r.FormId, r.Version }).IsUnique();
        
        // Index for queries
        builder.HasIndex(r => r.CreatedAt);
    }
}
```

**3.4 Update AppDbContext:**

Update `Data/AppDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using FormDesignerAPI.Core.FormContext.Aggregates;
using System.Reflection;

namespace FormDesignerAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Form Context
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormRevision> FormRevisions => Set<FormRevision>();

    // TODO: Add other contexts as you implement them
    // public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    // public DbSet<CodeGenerationJob> CodeGenerationJobs => Set<CodeGenerationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    /// <summary>
    /// Override SaveChanges to dispatch domain events
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: In Phase 8, add domain event dispatching here
        // For now, just save
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

**3.5 Create Repository Implementation:**

Create `Repositories/FormContext/FormRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Infrastructure.Data;

namespace FormDesignerAPI.Infrastructure.Repositories.FormContext;

/// <summary>
/// Repository implementation for Form aggregate
/// </summary>
public class FormRepository : IFormRepository
{
    private readonly AppDbContext _context;

    public FormRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Form?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Form?> GetByIdWithRevisionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_revisions") // Include private field
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Form>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .OrderByDescending(f => EF.Property<DateTime>(f, "CreatedAt"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Form>> GetByOriginTypeAsync(
        OriginType originType,
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Where(f => EF.Property<OriginType>(f, "OriginType") == originType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Form>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(cancellationToken);

        return await _context.Forms
            .Where(f => f.Name.Contains(searchTerm))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Form> AddAsync(
        Form entity, 
        CancellationToken cancellationToken = default)
    {
        _context.Forms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(
        Form entity, 
        CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Form entity, 
        CancellationToken cancellationToken = default)
    {
        _context.Forms.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**3.6 Update Configuration Files:**

Update `FormDesignerAPI.Web/appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormDesignerAPI;Username=postgres;Password=your_password"
  },
  "AllowedHosts": "*"
}
```

Update `FormDesignerAPI.Web/appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormDesignerAPI_Dev;Username=postgres;Password=your_password"
  }
}
```

**3.7 Create DependencyInjection:**

Create `Infrastructure/DependencyInjection.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Infrastructure.Data;
using FormDesignerAPI.Infrastructure.Repositories.FormContext;

namespace FormDesignerAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        // Repositories - Form Context
        services.AddScoped<IFormRepository, FormRepository>();

        // TODO: Add repositories for other contexts as you implement them
        // services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        // services.AddScoped<ICodeGenerationJobRepository, CodeGenerationJobRepository>();

        return services;
    }
}
```

**3.8 Update Program.cs:**

Update `FormDesignerAPI.Web/Program.cs`:
```csharp
using FormDesignerAPI.Infrastructure;
using FormDesignerAPI.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FormDesignerAPI application");

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add Infrastructure services
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // TODO: Add UseCases services in Phase 4
    // builder.Services.AddUseCaseServices();

    var app = builder.Build();

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Apply migrations on startup (Development only)
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied");
    }

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

**3.9 Create and Apply Migration:**
```bash
cd src/FormDesignerAPI.Infrastructure

# Create migration
dotnet ef migrations add InitialCreate \
  --startup-project ../FormDesignerAPI.Web \
  --context AppDbContext

# Review the migration file in Migrations/ folder
# Then apply it:

dotnet ef database update \
  --startup-project ../FormDesignerAPI.Web \
  --context AppDbContext
```

**3.10 Verify Database:**
```bash
# Connect to PostgreSQL
psql -U postgres -d FormDesignerAPI

# List tables
\dt

# Should see: Forms, FormRevisions
# View Forms table structure
\d "Forms"

# Exit
\q
```

### Checkpoint 3: Verification

- [ ] Database migrations created successfully
- [ ] Tables exist in PostgreSQL
- [ ] Can connect to database from application
- [ ] Repository compiles without errors
- [ ] DependencyInjection registers all services

**Git Commit:**
```bash
git add .
git commit -m "Phase 3: Created Form Context infrastructure with EF Core, repositories, and migrations"
```

---

## Phase 4: Form Context - Use Cases & API (Week 3)

### Step 4: Create Application Layer

**4.1 Install MediatR:**
```bash
cd src/FormDesignerAPI.UseCases
dotnet add package MediatR
```

**4.2 Create folder structure:**
```bash
mkdir -p FormContext/Commands
mkdir -p FormContext/Queries
mkdir -p FormContext/DTOs
```

**4.3 Create DTOs:**

Create `FormContext/DTOs/FormDto.cs`:
```csharp
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.UseCases.FormContext.DTOs;

/// <summary>
/// Data transfer object for Form
/// </summary>
public record FormDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OriginType { get; init; } = string.Empty;
    public string? OriginReferenceId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public int CurrentVersion { get; init; }
    public int TotalRevisions { get; init; }

    /// <summary>
    /// Map from domain entity to DTO
    /// </summary>
    public static FormDto FromDomain(Form form)
    {
        return new FormDto
        {
            Id = form.Id,
            Name = form.Name,
            OriginType = form.Origin.Type.ToString(),
            OriginReferenceId = form.Origin.ReferenceId,
            CreatedAt = form.Origin.CreatedAt,
            CreatedBy = form.Origin.CreatedBy,
            CurrentVersion = form.CurrentRevision.Version,
            TotalRevisions = form.Revisions.Count
        };
    }
}
```

Create `FormContext/DTOs/FormDetailDto.cs`:
```csharp
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.UseCases.FormContext.DTOs;

/// <summary>
/// Detailed DTO including form definition
/// </summary>
public record FormDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OriginType { get; init; } = string.Empty;
    public string? OriginReferenceId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string DefinitionSchema { get; init; } = string.Empty;
    public List<FormFieldDto> Fields { get; init; } = new();
    public List<FormRevisionDto> Revisions { get; init; } = new();

    public static FormDetailDto FromDomain(Form form)
    {
        return new FormDetailDto
        {
            Id = form.Id,
            Name = form.Name,
            OriginType = form.Origin.Type.ToString(),
            OriginReferenceId = form.Origin.ReferenceId,
            CreatedAt = form.Origin.CreatedAt,
            CreatedBy = form.Origin.CreatedBy,
            DefinitionSchema = form.Definition.Schema,
            Fields = form.Definition.Fields.Select(f => new FormFieldDto
            {
                Name = f.Name,
                Type = f.Type,
                Required = f.Required,
                Label = f.Label,
                Placeholder = f.Placeholder
            }).ToList(),
            Revisions = form.Revisions.Select(r => new FormRevisionDto
            {
                Id = r.Id,
                Version = r.Version,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy
            }).ToList()
        };
    }
}

public record FormFieldDto
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Label { get; init; }
    public string? Placeholder { get; init; }
}

public record FormRevisionDto
{
    public Guid Id { get; init; }
    public int Version { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}
```

**4.4 Create Commands:**

Create `FormContext/Commands/CreateFormCommand.cs`:
```csharp
using MediatR;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Commands;

/// <summary>
/// Command to create a new form
/// </summary>
public record CreateFormCommand(
    string Name,
    string DefinitionSchema,
    string CreatedBy
) : IRequest<FormDto>;
```

Create `FormContext/Commands/CreateFormCommandHandler.cs`:
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Commands;

/// <summary>
/// Handler for CreateFormCommand
/// </summary>
public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, FormDto>
{
    private readonly IFormRepository _repository;
    private readonly ILogger<CreateFormCommandHandler> _logger;

    public CreateFormCommandHandler(
        IFormRepository repository,
        ILogger<CreateFormCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FormDto> Handle(
        CreateFormCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating new form: {FormName} by {CreatedBy}",
            request.Name,
            request.CreatedBy);

        try
        {
            // Create value objects
            var definition = FormDefinition.From(request.DefinitionSchema);
            var origin = OriginMetadata.Manual(request.CreatedBy);
            
            // Create aggregate
            var form = Form.Create(
                request.Name,
                definition,
                origin,
                request.CreatedBy
            );
            
            // Persist
            await _repository.AddAsync(form, cancellationToken);
            
            _logger.LogInformation(
                "Form created successfully: {FormId}",
                form.Id);

            // Return DTO
            return FormDto.FromDomain(form);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating form: {FormName}",
                request.Name);
            throw;
        }
    }
}
```

Create `FormContext/Commands/CreateFormRevisionCommand.cs`:
```csharp
using MediatR;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Commands;

/// <summary>
/// Command to create a new form revision
/// </summary>
public record CreateFormRevisionCommand(
    Guid FormId,
    string DefinitionSchema,
    string Notes,
    string CreatedBy
) : IRequest<FormDetailDto>;
```

Create `FormContext/Commands/CreateFormRevisionCommandHandler.cs`:
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Commands;

public class CreateFormRevisionCommandHandler 
    : IRequestHandler<CreateFormRevisionCommand, FormDetailDto>
{
    private readonly IFormRepository _repository;
    private readonly ILogger<CreateFormRevisionCommandHandler> _logger;

    public CreateFormRevisionCommandHandler(
        IFormRepository repository,
        ILogger<CreateFormRevisionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FormDetailDto> Handle(
        CreateFormRevisionCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating new revision for form: {FormId}",
            request.FormId);

        // Get form with revisions
        var form = await _repository.GetByIdWithRevisionsAsync(
            request.FormId,
            cancellationToken);

        if (form == null)
        {
            throw new InvalidOperationException($"Form not found: {request.FormId}");
        }

        // Create new definition
        var newDefinition = FormDefinition.From(request.DefinitionSchema);

        // Create revision
        form.CreateRevision(
            newDefinition,
            request.Notes,
            request.CreatedBy);

        // Update
        await _repository.UpdateAsync(form, cancellationToken);

        _logger.LogInformation(
            "Form revision created: {FormId}, Version: {Version}",
            form.Id,
            form.CurrentRevision.Version);

        return FormDetailDto.FromDomain(form);
    }
}
```

**4.5 Create Queries:**

Create `FormContext/Queries/GetFormByIdQuery.cs`:
```csharp
using MediatR;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Queries;

/// <summary>
/// Query to get a form by ID
/// </summary>
public record GetFormByIdQuery(Guid FormId) : IRequest<FormDetailDto?>;
```

Create `FormContext/Queries/GetFormByIdQueryHandler.cs`:
```csharp
using MediatR;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.UseCases.FormContext.DTOs;

namespace FormDesignerAPI.UseCases.FormContext.Queries;

public class GetFormByIdQueryHandler 
    : IRequestHandler<GetFormByIdQuery, FormDetailDto?>
{
    private readonly IFormRepository _repository;

    public GetFormByIdQueryHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<FormDetailDto?> Handle(
        GetFormByIdQuery request,
        CancellationToken cancellationToken)
    {
        var form = await _repository.GetByIdWithRevisionsAsync(
            request.FormId,
            cancellationToken);

        return form == null ? null : FormDetailDto.FromDomain(form);
    }
}
```

Create `FormContext/Queries/GetAllFormsQuery.cs`:
```csharp
using MediatR;
using FormDesignerAPI.UseCases.FormContext.DT
Os;

namespace FormGenAI.UseCases.FormContext.Queries;

/// <summary>
/// Query to get all forms
/// </summary>
public record GetAllFormsQuery() : IRequest<List<FormDto>>;
```

Create `FormContext/Queries/GetAllFormsQueryHandler.cs`:

```csharp
using MediatR;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Queries;

public class GetAllFormsQueryHandler 
    : IRequestHandler<GetAllFormsQuery, List<FormDto>>
{
    private readonly IFormRepository _repository;

    public GetAllFormsQueryHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FormDto>> Handle(
        GetAllFormsQuery request,
        CancellationToken cancellationToken)
    {
        var forms = await _repository.GetAllAsync(cancellationToken);
        
        return forms.Select(FormDto.FromDomain).ToList();
    }
}
```

**4.6 Create UseCases DependencyInjection:**

Create `UseCases/DependencyInjection.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FormGenAI.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCaseServices(
        this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
```

**4.7 Create API Controller:**

Create folder and controller:

```bash
cd src/FormGenAI.Web
mkdir -p Controllers/FormContext
```

Create `Controllers/FormContext/FormsController.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FormGenAI.UseCases.FormContext.Commands;
using FormGenAI.UseCases.FormContext.Queries;
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.Web.Controllers.FormContext;

/// <summary>
/// API endpoints for Form management
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
    /// Get all forms
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FormDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FormDto>>> GetAll()
    {
        var query = new GetAllFormsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get form by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FormDetailDto>> GetById(Guid id)
    {
        var query = new GetFormByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Create a new form
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormDto>> Create(
        [FromBody] CreateFormCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new revision for an existing form
    /// </summary>
    [HttpPost("{id}/revisions")]
    [ProducesResponseType(typeof(FormDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormDetailDto>> CreateRevision(
        Guid id,
        [FromBody] CreateRevisionRequest request)
    {
        try
        {
            var command = new CreateFormRevisionCommand(
                id,
                request.DefinitionSchema,
                request.Notes,
                request.CreatedBy
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form revision");
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for creating a revision
/// </summary>
public record CreateRevisionRequest(
    string DefinitionSchema,
    string Notes,
    string CreatedBy
);
```

**4.8 Update Program.cs:**

Update `FormGenAI.Web/Program.cs`:

```csharp
using FormGenAI.Infrastructure;
using FormGenAI.UseCases;  // ADD THIS
using FormGenAI.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FormGenAI application");

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "FormGenAI API",
            Version = "v1",
            Description = "AI-Powered Form Code Generation System"
        });
    });

    // Add Infrastructure services
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add UseCase services
    builder.Services.AddUseCaseServices();

    var app = builder.Build();

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Apply migrations on startup (Development only)
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied");
    }

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

**4.9 Test the API:**

Start the application:

```bash
cd src/FormGenAI.Web
dotnet run
```

Navigate to Swagger UI: `https://localhost:5001/swagger`

Test creating a form:

```bash
curl -X POST https://localhost:5001/api/forms \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Patient Intake Form",
    "definitionSchema": "{\"fields\":[{\"name\":\"firstName\",\"type\":\"text\"}]}",
    "createdBy": "admin@test.com"
  }'
```

Test getting all forms:

```bash
curl https://localhost:5001/api/forms
```

### Checkpoint 4: Verification

- [ ] API endpoints return 200 OK
- [ ] Can create forms via API
- [ ] Can retrieve forms via API
- [ ] Swagger UI displays all endpoints
- [ ] Data persists in PostgreSQL

**Git Commit:**
```bash
git add .
git commit -m "Phase 4: Created Form Context Use Cases and API with CQRS pattern"
```

---

## Phase 5: Claude API Integration (Week 3-4)

### Step 5: Integrate Claude Sonnet 4 API

This is the crucial phase where you integrate with Anthropic's Claude API for AI-powered code generation.

**5.1 Create folder structure:**

```bash
cd src/FormGenAI.Infrastructure
mkdir -p ExternalServices
```

**5.2 Create ClaudeApiClient:**

Create `ExternalServices/ClaudeApiClient.cs`:

```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormGenAI.Infrastructure.ExternalServices;

/// <summary>
/// Client for interacting with the Anthropic Claude API
/// </summary>
public class ClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<ClaudeApiClient> _logger;
    private const string ApiVersion = "2023-06-01";

    public ClaudeApiClient(
        HttpClient httpClient,
        IOptions<AnthropicSettings> settings,
        ILogger<ClaudeApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.Timeout = TimeSpan.FromMinutes(_settings.TimeoutMinutes);
    }

    /// <summary>
    /// Generate code from a PDF file using Claude
    /// </summary>
    public async Task<string> GenerateCodeFromPdf(
        string pdfFilePath,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfFilePath))
            throw new ArgumentException("PDF file path cannot be empty", nameof(pdfFilePath));

        if (!File.Exists(pdfFilePath))
            throw new FileNotFoundException("PDF file not found", pdfFilePath);

        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        _logger.LogInformation(
            "Starting code generation from PDF: {PdfPath}", 
            Path.GetFileName(pdfFilePath));

        try
        {
            // Read and convert PDF to base64
            byte[] pdfBytes = await File.ReadAllBytesAsync(pdfFilePath, cancellationToken);
            string base64Pdf = Convert.ToBase64String(pdfBytes);

            _logger.LogDebug(
                "PDF converted to base64. Size: {SizeKB} KB", 
                pdfBytes.Length / 1024);

            // Build request
            var request = new ClaudeRequest
            {
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                Messages = new List<ClaudeMessage>
                {
                    new ClaudeMessage
                    {
                        Role = "user",
                        Content = new List<ClaudeContent>
                        {
                            new ClaudeContent
                            {
                                Type = "document",
                                Source = new ClaudeSource
                                {
                                    Type = "base64",
                                    MediaType = "application/pdf",
                                    Data = base64Pdf
                                }
                            },
                            new ClaudeContent
                            {
                                Type = "text",
                                Text = prompt
                            }
                        }
                    }
                }
            };

            // Serialize request
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending request to Claude API...");

            // Send request with retry logic
            var response = await SendWithRetryAsync(
                () => _httpClient.PostAsync("/v1/messages", content, cancellationToken),
                cancellationToken);

            // Read response
            string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Claude API returned error. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    responseJson);

                throw new ClaudeApiException(
                    $"Claude API request failed with status {response.StatusCode}",
                    response.StatusCode,
                    responseJson);
            }

            _logger.LogDebug("Received response from Claude API");

            // Deserialize response
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(
                responseJson, 
                jsonOptions);

            if (claudeResponse?.Content == null || !claudeResponse.Content.Any())
            {
                throw new ClaudeApiException(
                    "Claude API returned empty response",
                    response.StatusCode,
                    responseJson);
            }

            // Extract text from response
            var generatedCode = ExtractTextFromResponse(claudeResponse);

            _logger.LogInformation(
                "Code generation completed. Generated {Length} characters",
                generatedCode.Length);

            return generatedCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling Claude API");
            throw new ClaudeApiException("Network error while calling Claude API", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Claude API request timed out");
            throw new ClaudeApiException("Claude API request timed out", ex);
        }
        catch (Exception ex) when (ex is not ClaudeApiException)
        {
            _logger.LogError(ex, "Unexpected error during code generation");
            throw new ClaudeApiException("Unexpected error during code generation", ex);
        }
    }

    /// <summary>
    /// Test the API connection
    /// </summary>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing Claude API connection...");

            var request = new ClaudeRequest
            {
                Model = _settings.Model,
                MaxTokens = 10,
                Messages = new List<ClaudeMessage>
                {
                    new ClaudeMessage
                    {
                        Role = "user",
                        Content = new List<ClaudeContent>
                        {
                            new ClaudeContent
                            {
                                Type = "text",
                                Text = "Hello"
                            }
                        }
                    }
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "/v1/messages", 
                content, 
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Claude API connection test successful");
                return true;
            }

            _logger.LogWarning(
                "Claude API connection test failed with status: {StatusCode}",
                response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude API connection test failed");
            return false;
        }
    }

    private string ExtractTextFromResponse(ClaudeResponse response)
    {
        var textBlocks = response.Content
            .Where(c => c.Type == "text" && !string.IsNullOrWhiteSpace(c.Text))
            .Select(c => c.Text)
            .ToList();

        if (!textBlocks.Any())
        {
            throw new ClaudeApiException("No text content found in Claude response");
        }

        return string.Join("\n\n", textBlocks);
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<Task<HttpResponseMessage>> requestFunc,
        CancellationToken cancellationToken)
    {
        int maxRetries = _settings.MaxRetries;
        int retryDelayMs = _settings.RetryDelayMilliseconds;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await requestFunc();

                // Don't retry on success or client errors (4xx)
                if (response.IsSuccessStatusCode || 
                    (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }

                // Retry on server errors (5xx) or rate limiting (429)
                if (attempt < maxRetries)
                {
                    _logger.LogWarning(
                        "Request failed with status {StatusCode}. Retry attempt {Attempt} of {MaxRetries}",
                        response.StatusCode,
                        attempt + 1,
                        maxRetries);

                    await Task.Delay(retryDelayMs * (attempt + 1), cancellationToken);
                    continue;
                }

                return response;
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                _logger.LogWarning(
                    "Network error. Retry attempt {Attempt} of {MaxRetries}",
                    attempt + 1,
                    maxRetries);

                await Task.Delay(retryDelayMs * (attempt + 1), cancellationToken);
            }
        }

        throw new ClaudeApiException("Max retries exceeded");
    }
}

#region Request/Response Models

public class ClaudeRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public List<ClaudeMessage> Messages { get; set; } = new();
}

public class ClaudeMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public List<ClaudeContent> Content { get; set; } = new();
}

public class ClaudeContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("source")]
    public ClaudeSource? Source { get; set; }
}

public class ClaudeSource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("media_type")]
    public string MediaType { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public class ClaudeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public List<ClaudeContentResponse> Content { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }

    [JsonPropertyName("usage")]
    public ClaudeUsage? Usage { get; set; }
}

public class ClaudeContentResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class ClaudeUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
}

#endregion
```

**5.3 Create AnthropicSettings:**

Create `ExternalServices/AnthropicSettings.cs`:

```csharp
namespace FormGenAI.Infrastructure.ExternalServices;

public class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; set; } = 4096;
    public string ApiBaseUrl { get; set; } = "https://api.anthropic.com";
    public int TimeoutMinutes { get; set; } = 5;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
```

**5.4 Create ClaudeApiException:**

Create `ExternalServices/ClaudeApiException.cs`:

```csharp
using System.Net;

namespace FormGenAI.Infrastructure.ExternalServices;

public class ClaudeApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ResponseBody { get; }

    public ClaudeApiException(string message) : base(message)
    {
    }

    public ClaudeApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public ClaudeApiException(
        string message, 
        HttpStatusCode statusCode, 
        string responseBody) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
```

**5.5 Update appsettings.json:**

Update `FormGenAI.Web/appsettings.json` to add Anthropic configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "FormGenAI.Infrastructure.ExternalServices": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormGenAI;Username=postgres;Password=your_password"
  },
  "Anthropic": {
    "ApiKey": "YOUR_ANTHROPIC_API_KEY_HERE",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 4096,
    "ApiBaseUrl": "https://api.anthropic.com",
    "TimeoutMinutes": 5,
    "MaxRetries": 3,
    "RetryDelayMilliseconds": 1000
  },
  "AllowedHosts": "*"
}
```

**Important:** Replace `YOUR_ANTHROPIC_API_KEY_HERE` with your actual API key from https://console.anthropic.com/

**5.6 Update appsettings.Development.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information",
      "FormGenAI.Infrastructure.ExternalServices": "Trace"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormGenAI_Dev;Username=postgres;Password=your_password"
  },
  "Anthropic": {
    "MaxTokens": 8000
  }
}
```

**5.7 Update DependencyInjection:**

Update `Infrastructure/DependencyInjection.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.Infrastructure.Data;
using FormGenAI.Infrastructure.Repositories.FormContext;
using FormGenAI.Infrastructure.ExternalServices;

namespace FormGenAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        // Repositories - Form Context
        services.AddScoped<IFormRepository, FormRepository>();

        // Configure Anthropic settings
        services.Configure<AnthropicSettings>(
            configuration.GetSection("Anthropic"));

        // Validate Anthropic configuration
        services.AddOptions<AnthropicSettings>()
            .Validate(settings =>
            {
                if (string.IsNullOrWhiteSpace(settings.ApiKey))
                    return false;
                if (string.IsNullOrWhiteSpace(settings.Model))
                    return false;
                if (settings.MaxTokens <= 0)
                    return false;
                return true;
            }, "Anthropic configuration is invalid. Please check your API key and settings.");

        // Register HttpClient with ClaudeApiClient
        services.AddHttpClient<ClaudeApiClient>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var settings = serviceProvider
                    .GetRequiredService<IOptions<AnthropicSettings>>()
                    .Value;
                
                client.BaseAddress = new Uri(settings.ApiBaseUrl);
                client.Timeout = TimeSpan.FromMinutes(settings.TimeoutMinutes);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(10)); // Avoid socket exhaustion

        // Register as scoped service
        services.AddScoped<ClaudeApiClient>();

        return services;
    }
}
```

**5.8 Update Program.cs to test connection:**

Update `FormGenAI.Web/Program.cs`:

```csharp
using FormGenAI.Infrastructure;
using FormGenAI.UseCases;
using FormGenAI.Infrastructure.Data;
using FormGenAI.Infrastructure.ExternalServices;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FormGenAI application");

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "FormGenAI API",
            Version = "v1",
            Description = "AI-Powered Form Code Generation System"
        });
    });

    // Add Infrastructure services
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add UseCase services
    builder.Services.AddUseCaseServices();

    var app = builder.Build();

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Apply migrations on startup (Development only)
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        
        // Apply database migrations
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied");
        
        // Test Claude API connection
        var claudeClient = scope.ServiceProvider.GetRequiredService<ClaudeApiClient>();
        
        try
        {
            var connectionOk = await claudeClient.TestConnectionAsync();
            if (connectionOk)
            {
                app.Logger.LogInformation("✓ Claude API connection verified");
            }
            else
            {
                app.Logger.LogWarning("⚠ Claude API connection test failed");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "✗ Claude API connection test error");
        }
    }

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

**5.9 Test the Claude API Integration:**

Run the application:

```bash
cd src/FormGenAI.Web
dotnet run
```

You should see in the logs:
```
✓ Claude API connection verified
```

If you see an error, check:
- [ ] API key is correct in appsettings.json
- [ ] API key has not expired
- [ ] You have API credits
- [ ] Internet connection is working

### Checkpoint 5: Verification

- [ ] Application starts without errors
- [ ] Claude API connection test passes
- [ ] ClaudeApiClient is registered in DI container
- [ ] Configuration validation works

**Git Commit:**
```bash
git add .
git commit -m "Phase 5: Integrated Claude Sonnet 4 API with connection testing"
```

---

## Continuing the Document

Due to length constraints, I'll provide you with a complete downloadable file. Would you like me to:

1. **Create a complete downloadable Markdown file** with all remaining phases (6-9)?
2. **Generate this as a PDF** for easier reading?
3. **Create separate documents** for each remaining phase?

The remaining phases to document are:
- Phase 6: Import Context (Perfect! I'll create the complete downloadable Markdown file with all phases. Let me save this as a single comprehensive document:

```markdown
# FormGenAI Migration Guide - Complete Edition
## Integrating DDD Bounded Contexts into Ardalis Clean Architecture

**Version:** 1.0  
**Date:** December 2024  
**Author:** Architecture Team  
**Target Solution:** FormGenAI  
**Document Type:** Complete Migration Guide (Phases 1-9)

---

[Previous content from Phases 1-5 remains the same]

---

## Phase 6: Import Context (Week 4-5)

### Step 6: Create PDF Import and AI Extraction Context

This phase implements the Import Context which handles PDF uploads and uses Claude to extract form fields.

**6.1 Create folder structure:**

```bash
cd src/FormGenAI.Core
mkdir -p ImportContext/Aggregates
mkdir -p ImportContext/ValueObjects
mkdir -p ImportContext/Services
mkdir -p ImportContext/Events
mkdir -p ImportContext/Interfaces
```

**6.2 Create Value Objects:**

Create `ImportContext/ValueObjects/ExtractionStatus.cs`:

```csharp
namespace FormGenAI.Core.ImportContext.ValueObjects;

/// <summary>
/// Status of PDF extraction process
/// </summary>
public enum ExtractionStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

Create `ImportContext/ValueObjects/ApprovalStatus.cs`:

```csharp
namespace FormGenAI.Core.ImportContext.ValueObjects;

/// <summary>
/// Status of form candidate approval
/// </summary>
public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
```

**6.3 Create Domain Events:**

Create `ImportContext/Events/FormCandidateExtractedEvent.cs`:

```csharp
using FormGenAI.SharedKernel.Base;

namespace FormGenAI.Core.ImportContext.Events;

/// <summary>
/// Event raised when a form candidate is extracted from PDF
/// </summary>
public record FormCandidateExtractedEvent(
    Guid CandidateId,
    Guid BatchId,
    string FileName,
    bool Success,
    string? Error
) : DomainEventBase;
```

Create `ImportContext/Events/FormCandidateApprovedEvent.cs`:

```csharp
using FormGenAI.SharedKernel.Base;

namespace FormGenAI.Core.ImportContext.Events;

/// <summary>
/// Event raised when a form candidate is approved
/// </summary>
public record FormCandidateApprovedEvent(
    Guid CandidateId,
    Guid BatchId,
    string ExtractedJson,
    string ApprovedBy,
    DateTime ApprovedAt
) : DomainEventBase;
```

**6.4 Create Aggregates:**

Create `ImportContext/Aggregates/ImportBatch.cs`:

```csharp
using Ardalis.GuardClauses;
using FormGenAI.SharedKernel.Base;
using FormGenAI.SharedKernel.Interfaces;
using FormGenAI.Core.ImportContext.ValueObjects;

namespace FormGenAI.Core.ImportContext.Aggregates;

/// <summary>
/// ImportBatch aggregate root - manages a batch of PDF imports
/// </summary>
public class ImportBatch : EntityBase, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Status { get; private set; } = "Pending";
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? CompletedAt { get; private set; }
    
    private readonly List<string> _uploadedFiles = new();
    public IReadOnlyCollection<string> UploadedFiles => _uploadedFiles.AsReadOnly();
    
    private readonly List<ImportedFormCandidate> _candidates = new();
    public IReadOnlyCollection<ImportedFormCandidate> Candidates => _candidates.AsReadOnly();

    // Private constructor for EF Core
    private ImportBatch() { }

    /// <summary>
    /// Factory method to create a new import batch
    /// </summary>
    public static ImportBatch Create(List<string> fileNames, string createdBy)
    {
        Guard.Against.NullOrEmpty(fileNames, nameof(fileNames));
        Guard.Against.NullOrEmpty(createdBy, nameof(createdBy));

        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        batch._uploadedFiles.AddRange(fileNames);

        return batch;
    }

    /// <summary>
    /// Add a candidate to this batch
    /// </summary>
    public void AddCandidate(ImportedFormCandidate candidate)
    {
        Guard.Against.Null(candidate, nameof(candidate));
        _candidates.Add(candidate);
    }

    /// <summary>
    /// Mark batch as processing
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = "Processing";
    }

    /// <summary>
    /// Mark batch as completed
    /// </summary>
    public void Complete()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark batch as failed
    /// </summary>
    public void Fail()
    {
        Status = "Failed";
        CompletedAt = DateTime.UtcNow;
    }
}
```

Create `ImportContext/Aggregates/ImportedFormCandidate.cs`:

```csharp
using Ardalis.GuardClauses;
using FormGenAI.SharedKernel.Base;
using FormGenAI.Core.ImportContext.ValueObjects;
using FormGenAI.Core.ImportContext.Events;

namespace FormGenAI.Core.ImportContext.Aggregates;

/// <summary>
/// Represents a form extracted from a PDF awaiting approval
/// </summary>
public class ImportedFormCandidate : EntityBase
{
    public Guid Id { get; private set; }
    public Guid BatchId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string? ExtractedJson { get; private set; }
    public ExtractionStatus ExtractionStatus { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    
    private readonly List<string> _validationErrors = new();
    public IReadOnlyCollection<string> ValidationErrors => _validationErrors.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    // Private constructor for EF Core
    private ImportedFormCandidate() { }

    /// <summary>
    /// Factory method to create a new candidate
    /// </summary>
    public static ImportedFormCandidate Create(Guid batchId, string fileName)
    {
        Guard.Against.Default(batchId, nameof(batchId));
        Guard.Against.NullOrEmpty(fileName, nameof(fileName));

        return new ImportedFormCandidate
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            OriginalFileName = fileName,
            ExtractionStatus = ExtractionStatus.Pending,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mark extraction as started
    /// </summary>
    public void StartExtraction()
    {
        ExtractionStatus = ExtractionStatus.Processing;
    }

    /// <summary>
    /// Complete extraction successfully
    /// </summary>
    public void CompleteExtraction(string extractedJson)
    {
        Guard.Against.NullOrEmpty(extractedJson, nameof(extractedJson));
        
        ExtractedJson = extractedJson;
        ExtractionStatus = ExtractionStatus.Completed;

        RegisterDomainEvent(new FormCandidateExtractedEvent(
            Id,
            BatchId,
            OriginalFileName,
            true,
            null
        ));
    }

    /// <summary>
    /// Mark extraction as failed
    /// </summary>
    public void FailExtraction(string error)
    {
        Guard.Against.NullOrEmpty(error, nameof(error));
        
        ExtractionStatus = ExtractionStatus.Failed;
        _validationErrors.Add(error);

        RegisterDomainEvent(new FormCandidateExtractedEvent(
            Id,
            BatchId,
            OriginalFileName,
            false,
            error
        ));
    }

    /// <summary>
    /// Approve this candidate
    /// </summary>
    public void Approve(string approvedBy)
    {
        Guard.Against.NullOrEmpty(approvedBy, nameof(approvedBy));
        
        if (ExtractionStatus != ExtractionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot approve candidate with incomplete extraction");
        }

        ApprovalStatus = ApprovalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;

        RegisterDomainEvent(new FormCandidateApprovedEvent(
            Id,
            BatchId,
            ExtractedJson!,
            approvedBy,
            ApprovedAt.Value
        ));
    }

    /// <summary>
    /// Reject this candidate
    /// </summary>
    public void Reject(string rejectedBy, string reason)
    {
        Guard.Against.NullOrEmpty(rejectedBy, nameof(rejectedBy));
        Guard.Against.NullOrEmpty(reason, nameof(reason));

        ApprovalStatus = ApprovalStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectedBy = rejectedBy;
        RejectionReason = reason;
    }
}
```

**6.5 Create Domain Service:**

Create `ImportContext/Services/PdfExtractionService.cs`:

```csharp
using FormGenAI.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;

namespace FormGenAI.Core.ImportContext.Services;

/// <summary>
/// Domain service for extracting form data from PDFs
/// </summary>
public class PdfExtractionService
{
    private readonly ClaudeApiClient _claudeClient;
    private readonly ILogger<PdfExtractionService> _logger;

    public PdfExtractionService(
        ClaudeApiClient claudeClient,
        ILogger<PdfExtractionService> logger)
    {
        _claudeClient = claudeClient;
        _logger = logger;
    }

    /// <summary>
    /// Extract form fields from a PDF file
    /// </summary>
    public async Task<string> ExtractFormFields(
        string pdfFilePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting form fields from: {FileName}", Path.GetFileName(pdfFilePath));

        var prompt = @"
Analyze this PDF form and extract all form fields into a JSON structure.

For each field, identify:
- name: The field name (camelCase)
- label: The human-readable label
- type: The field type (text, number, date, checkbox, radio, select, textarea, email, phone)
- required: Whether the field is required (true/false)
- placeholder: Any placeholder text
- options: For select/radio fields, list all options
- validation: Any validation rules (min, max, pattern, etc.)

Return ONLY valid JSON in this format:
{
  ""formName"": ""Form Title"",
  ""fields"": [
    {
      ""name"": ""firstName"",
      ""label"": ""First Name"",
      ""type"": ""text"",
      ""required"": true,
      ""placeholder"": ""Enter first name""
    }
  ]
}

Do not include any explanations, markdown, or additional text. Return ONLY the JSON.
";

        try
        {
            var result = await _claudeClient.GenerateCodeFromPdf(
                pdfFilePath,
                prompt,
                cancellationToken);

            // Remove any markdown code fences if present
            result = result.Replace("```json", "").Replace("```", "").Trim();

            _logger.LogInformation("Successfully extracted form fields");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract form fields from: {FileName}", Path.GetFileName(pdfFilePath));
            throw;
        }
    }
}
```

**6.6 Create Repository Interface:**

Create `ImportContext/Interfaces/IImportBatchRepository.cs`:

```csharp
using FormGenAI.SharedKernel.Interfaces;
using FormGenAI.Core.ImportContext.Aggregates;

namespace FormGenAI.Core.ImportContext.Interfaces;

public interface IImportBatchRepository : IRepository<ImportBatch>
{
    Task<ImportBatch?> GetByIdWithCandidatesAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ImportBatch>> GetRecentBatchesAsync(
        int count,
        CancellationToken cancellationToken = default);
}
```

**6.7 Create EF Core Configurations:**

Create `Infrastructure/Data/ImportContext/ImportBatchConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormGenAI.Core.ImportContext.Aggregates;
using System.Text.Json;

namespace FormGenAI.Infrastructure.Data.ImportContext;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(b => b.CreatedAt)
            .IsRequired();
        
        builder.Property(b => b.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(b => b.CompletedAt);
        
        // Store uploaded files as JSON
        builder.Property("_uploadedFiles")
            .HasColumnName("UploadedFiles")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
            );
        
        // Candidates relationship
        builder.HasMany(typeof(ImportedFormCandidate))
            .WithOne()
            .HasForeignKey("BatchId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(b => b.DomainEvents);
        
        builder.HasIndex(b => b.CreatedAt);
        builder.HasIndex(b => b.Status);
    }
}
```

Create `Infrastructure/Data/ImportContext/ImportedFormCandidateConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormGenAI.Core.ImportContext.Aggregates;
using FormGenAI.Core.ImportContext.ValueObjects;
using System.Text.Json;

namespace FormGenAI.Infrastructure.Data.ImportContext;

public class ImportedFormCandidateConfiguration : IEntityTypeConfiguration<ImportedFormCandidate>
{
    public void Configure(EntityTypeBuilder<ImportedFormCandidate> builder)
    {
        builder.ToTable("ImportedFormCandidates");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.BatchId)
            .IsRequired();
        
        builder.Property(c => c.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(c => c.ExtractedJson)
            .HasColumnType("jsonb");
        
        builder.Property(c => c.ExtractionStatus)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(c => c.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>();
        
        // Store validation errors as JSON
        builder.Property("_validationErrors")
            .HasColumnName("ValidationErrors")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
            );
        
        builder.Property(c => c.CreatedAt)
            .IsRequired();
        
        builder.Property(c => c.ApprovedAt);
        
        builder.Property(c => c.ApprovedBy)
            .HasMaxLength(100);
        
        builder.Property(c => c.RejectedAt);
        
        builder.Property(c => c.RejectedBy)
            .HasMaxLength(100);
        
        builder.Property(c => c.RejectionReason)
            .HasMaxLength(1000);
        
        builder.Ignore(c => c.DomainEvents);
        
        builder.HasIndex(c => c.BatchId);
        builder.HasIndex(c => c.ApprovalStatus);
    }
}
```

**6.8 Update AppDbContext:**

Update `Infrastructure/Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using FormGenAI.Core.FormContext.Aggregates;
using FormGenAI.Core.ImportContext.Aggregates;
using System.Reflection;

namespace FormGenAI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Form Context
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormRevision> FormRevisions => Set<FormRevision>();

    // Import Context
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportedFormCandidate> ImportedFormCandidates => Set<ImportedFormCandidate>();

    // TODO: Code Generation Context (add in Phase 7)
    // public DbSet<CodeGenerationJob> CodeGenerationJobs => Set<CodeGenerationJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: In Phase 8, add domain event dispatching here
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

**6.9 Create Repository Implementation:**

Create `Infrastructure/Repositories/ImportContext/ImportBatchRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using FormGenAI.Core.ImportContext.Aggregates;
using FormGenAI.Core.ImportContext.Interfaces;
using FormGenAI.Infrastructure.Data;

namespace FormGenAI.Infrastructure.Repositories.ImportContext;

public class ImportBatchRepository : IImportBatchRepository
{
    private readonly AppDbContext _context;

    public ImportBatchRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ImportBatch?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<ImportBatch?> GetByIdWithCandidatesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .Include("_candidates")
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ImportBatch>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ImportBatch>> GetRecentBatchesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportBatches
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportBatch> AddAsync(
        ImportBatch entity,
        CancellationToken cancellationToken = default)
    {
        _context.ImportBatches.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(
        ImportBatch entity,
        CancellationToken cancellationToken = default)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        ImportBatch entity,
        CancellationToken cancellationToken = default)
    {
        _context.ImportBatches.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**6.10 Register in DependencyInjection:**

Update `Infrastructure/DependencyInjection.cs`:

```csharp
// Add to AddInfrastructureServices method:

// Repositories - Import Context
services.AddScoped<IImportBatchRepository, ImportBatchRepository>();

// Domain Services
services.AddScoped<PdfExtractionService>();
```

**6.11 Create Migration:**

```bash
cd src/FormGenAI.Infrastructure

dotnet ef migrations add AddImportContext \
  --startup-project ../FormGenAI.Web \
  --context AppDbContext

dotnet ef database update \
  --startup-project ../FormGenAI.Web \
  --context AppDbContext
```

**6.12 Create Use Cases:**

Create `UseCases/ImportContext/Commands/UploadPdfBatchCommand.cs`:

```csharp
using MediatR;

namespace FormGenAI.UseCases.ImportContext.Commands;

public record UploadPdfBatchCommand(
    List<string> FileNames,
    string UploadedBy
) : IRequest<Guid>; // Returns BatchId
```

Create `UseCases/ImportContext/Commands/UploadPdfBatchCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormGenAI.Core.ImportContext.Aggregates;
using FormGenAI.Core.ImportContext.Interfaces;

namespace FormGenAI.UseCases.ImportContext.Commands;

public class UploadPdfBatchCommandHandler 
    : IRequestHandler<UploadPdfBatchCommand, Guid>
{
    private readonly IImportBatchRepository _repository;
    private readonly ILogger<UploadPdfBatchCommandHandler> _logger;

    public UploadPdfBatchCommandHandler(
        IImportBatchRepository repository,
        ILogger<UploadPdfBatchCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        UploadPdfBatchCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating import batch with {Count} files",
            request.FileNames.Count);

        var batch = ImportBatch.Create(request.FileNames, request.UploadedBy);
        
        await _repository.AddAsync(batch, cancellationToken);
        
        _logger.LogInformation("Import batch created: {BatchId}", batch.Id);
        
        return batch.Id;
    }
}
```

Create `UseCases/ImportContext/Commands/ProcessImportBatchCommand.cs`:

```csharp
using MediatR;

namespace FormGenAI.UseCases.ImportContext.Commands;

public record ProcessImportBatchCommand(
    Guid BatchId,
    Dictionary<string, string> FilePaths // FileName -> FilePath mapping
) : IRequest<Unit>;
```

Create `UseCases/ImportContext/Commands/ProcessImportBatchCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormGenAI.Core.ImportContext.Aggregates;
using FormGenAI.Core.ImportContext.Interfaces;
using FormGenAI.Core.ImportContext.Services;

namespace FormGenAI.UseCases.ImportContext.Commands;

public class ProcessImportBatchCommandHandler 
    : IRequestHandler<ProcessImportBatchCommand, Unit>
{
    private readonly IImportBatchRepository _repository;
    private readonly PdfExtractionService _extractionService;
    private readonly ILogger<ProcessImportBatchCommandHandler> _logger;

    public ProcessImportBatchCommandHandler(
        IImportBatchRepository repository,
        PdfExtractionService extractionService,
        ILogger<ProcessImportBatchCommandHandler> logger)
    {
        _repository = repository;
        _extractionService = extractionService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ProcessImportBatchCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing import batch: {BatchId}", request.BatchId);

        var batch = await _repository.GetByIdWithCandidatesAsync(
            request.BatchId,
            cancellationToken);

        if (batch == null)
        {
            throw new InvalidOperationException($"Batch not found: {request.BatchId}");
        }

        batch.MarkAsProcessing();

        foreach (var fileName in batch.UploadedFiles)
        {
            if (!request.FilePaths.TryGetValue(fileName, out var filePath))
            {
                _logger.LogWarning("File path not found for: {FileName}", fileName);
                continue;
            }

            var candidate = ImportedFormCandidate.Create(batch.Id, fileName);
            batch.AddCandidate(candidate);

            try
            {
                candidate.StartExtraction();
                
                var extractedJson = await _extractionService.ExtractFormFields(
                    filePath,
                    cancellationToken);
                
                candidate.CompleteExtraction(extractedJson);
                
                _logger.LogInformation(
                    "Successfully extracted fields from: {FileName}",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to extract fields from: {FileName}",
                    fileName);
                
                candidate.FailExtraction(ex.Message);
            }
        }

        batch.Complete();
        await _repository.UpdateAsync(batch, cancellationToken);

        _logger.LogInformation("Completed processing batch: {BatchId}", request.BatchId);

        return Unit.Value;
    }
}
```

Create `UseCases/ImportContext/Commands/ApproveFormCandidateCommand.cs`:

```csharp
using MediatR;

namespace FormGenAI.UseCases.ImportContext.Commands;

public record ApproveFormCandidateCommand(
    Guid CandidateId,
    string ApprovedBy
) : IRequest<Unit>;
```

Create `UseCases/ImportContext/Commands/ApproveFormCandidateCommandHandler.cs`:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormGenAI.Core.ImportContext.Interfaces;

namespace FormGenAI.UseCases.ImportContext.Commands;

public class ApproveFormCandidateCommandHandler 
    : IRequestHandler<ApproveFormCandidateCommand, Unit>
{
    private readonly IImportBatchRepository _repository;
    private readonly ILogger<ApproveFormCandidateCommandHandler> _logger;

    public ApproveFormCandidateCommandHandler(
        IImportBatchRepository repository,
        ILogger<ApproveFormCandidateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ApproveFormCandidateCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving candidate: {CandidateId}", request.CandidateId);

        // Find the candidate across all batches
        var batches = await _repository.GetAllAsync(cancellationToken);
        
        foreach (var batch in batches)
        {
            var batchWithCandidates = await _repository.GetByIdWithCandidatesAsync(
                batch.Id,
                cancellationToken);
            
            var candidate = batchWithCandidates?.Candidates
                .FirstOrDefault(c => c.Id == request.CandidateId);
            
            if (candidate != null)
            {
                candidate.Approve(request.ApprovedBy);
                await _repository.UpdateAsync(batchWithCandidates!, cancellationToken);
                
                _logger.LogInformation(
                    "Candidate approved: {CandidateId}",
                    request.CandidateId);
                
                return Unit.Value;
            }
        }

        throw new InvalidOperationException($"Candidate not found: {request.CandidateId}");
    }
}
```

**6.13 Create API Controller:**

Create `Web/Controllers/ImportContext/ImportsController.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FormGenAI.UseCases.ImportContext.Commands;

namespace FormGenAI.Web.Controllers.ImportContext;

[ApiController]
[Route("api/[controller]")]
public class ImportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ImportsController(
        IMediator mediator,
        ILogger<ImportsController> logger,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Upload PDF files for processing
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<Guid>> UploadPdfs(
        [FromForm] List<IFormFile> files,
        [FromForm] string uploadedBy)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files uploaded");
        }

        // Save files to temp location
        var uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadPath);

        var fileNames = new List<string>();
        var filePaths = new Dictionary<string, string>();

        foreach (var file in files)
        {
            if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".pdf")
            {
                var fileName = Path.