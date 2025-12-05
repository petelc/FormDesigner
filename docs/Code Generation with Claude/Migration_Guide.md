# FormGenAI Migration Guide
## Integrating DDD Bounded Contexts into Ardalis Clean Architecture

**Version:** 1.0  
**Date:** December 2024  
**Author:** Architecture Team  
**Target Solution:** FormGenAI

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
<!-- FormGenAI.SharedKernel -->
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />

<!-- FormGenAI.Core -->
<PackageReference Include="Ardalis.GuardClauses" Version="4.5.0" />

<!-- FormGenAI.Infrastructure -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- FormGenAI.UseCases -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.0" />

<!-- FormGenAI.UnitTests -->
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

### 2.3 Environment Setup

**PostgreSQL Database (Using Docker):**
```bash
# Start PostgreSQL container
docker run --name formgenai-db \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=FormGenAI \
  -p 5432:5432 \
  -d postgres:15

# Verify it's running
docker ps

# Connect to verify
docker exec -it formgenai-db psql -U postgres -d FormGenAI
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
cp -r FormGenAI FormGenAI.backup

# Commit current state
git add .
git commit -m "Checkpoint before DDD migration"
```

---

## 3. Understanding the Architecture

### 3.1 Current Ardalis Structure

Your solution currently follows this structure:
```
FormGenAI/
├── src/
│   ├── FormGenAI.Core/          # Domain entities & interfaces
│   ├── FormGenAI.Infrastructure/ # Data access & external services
│   ├── FormGenAI.UseCases/      # Application logic
│   └── FormGenAI.Web/           # API endpoints
└── tests/
    ├── FormGenAI.FunctionalTests/
    ├── FormGenAI.IntegrationTests/
    └── FormGenAI.UnitTests/
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
FormGenAI/
├── src/
│   ├── FormGenAI.SharedKernel/        # NEW - Shared abstractions
│   ├── FormGenAI.Core/                # REFACTOR - Organize by context
│   │   ├── FormContext/
│   │   ├── ImportContext/
│   │   └── CodeGenerationContext/
│   ├── FormGenAI.UseCases/            # REFACTOR - Organize by context
│   ├── FormGenAI.Infrastructure/      # REFACTOR - Organize by context
│   └── FormGenAI.Web/                 # REFACTOR - Organize by context
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
│     Presentation Layer              │ ← FormGenAI.Web
│     (Controllers, APIs)             │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Application Layer               │ ← FormGenAI.UseCases
│   (Use Cases, Commands, Queries)   │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Domain Layer                    │ ← FormGenAI.Core
│  (Aggregates, Entities, Events)    │
└──────────────┬──────────────────────┘
               │
┌──────────────┴──────────────────────┐
│     Infrastructure Layer            │ ← FormGenAI.Infrastructure
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
FormGenAI/
├── src/
│   ├── FormGenAI.SharedKernel/
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
│   ├── FormGenAI.Core/
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
│   ├── FormGenAI.UseCases/
│   │   ├── FormContext/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── ImportContext/
│   │   └── CodeGenerationContext/
│   │
│   ├── FormGenAI.Infrastructure/
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
│   └── FormGenAI.Web/
│       ├── Controllers/
│       │   ├── FormContext/
│       │   ├── ImportContext/
│       │   └── CodeGenerationContext/
│       ├── Configuration/
│       ├── appsettings.json
│       └── Program.cs
│
└── tests/
    ├── FormGenAI.UnitTests/
    ├── FormGenAI.IntegrationTests/
    └── FormGenAI.FunctionalTests/
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
dotnet new classlib -n FormGenAI.SharedKernel
dotnet sln ../FormGenAI.sln add FormGenAI.SharedKernel/FormGenAI.SharedKernel.csproj
```

**1.2 Add NuGet packages:**
```bash
cd FormGenAI.SharedKernel
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
using FormGenAI.SharedKernel.Interfaces;

namespace FormGenAI.SharedKernel.Base;

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
namespace FormGenAI.SharedKernel.Interfaces;

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

namespace FormGenAI.SharedKernel.Interfaces;

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
using FormGenAI.SharedKernel.Interfaces;

namespace FormGenAI.SharedKernel.Base;

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
namespace FormGenAI.SharedKernel.Base;

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
namespace FormGenAI.SharedKernel.Interfaces;

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
namespace FormGenAI.SharedKernel.Results;

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
cd ../FormGenAI.Core
dotnet add reference ../FormGenAI.SharedKernel/FormGenAI.SharedKernel.csproj
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
cd src/FormGenAI.Core
mkdir -p FormContext/Aggregates
mkdir -p FormContext/ValueObjects
mkdir -p FormContext/Events
mkdir -p FormContext/Interfaces
```

**2.2 Create OriginType enum:**

Create `FormContext/ValueObjects/OriginType.cs`:
```csharp
namespace FormGenAI.Core.FormContext.ValueObjects;

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
namespace FormGenAI.Core.FormContext.ValueObjects;

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

namespace FormGenAI.Core.FormContext.ValueObjects;

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
using FormGenAI.SharedKernel.Base;
using FormGenAI.Core.FormContext.ValueObjects;

namespace FormGenAI.Core.FormContext.Events;

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
using FormGenAI.SharedKernel.Base;

namespace FormGenAI.Core.FormContext.Events;

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
using FormGenAI.SharedKernel.Base;
using FormGenAI.SharedKernel.Interfaces;
using FormGenAI.Core.FormContext.ValueObjects;
using FormGenAI.Core.FormContext.Events;

namespace FormGenAI.Core.FormContext.Aggregates;

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
using FormGenAI.SharedKernel.Interfaces;
using FormGenAI.Core.FormContext.Aggregates;

namespace FormGenAI.Core.FormContext.Interfaces;

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
cd src/FormGenAI.Infrastructure
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
using FormGenAI.Core.FormContext.Aggregates;
using FormGenAI.Core.FormContext.ValueObjects;
using System.Text.Json;

namespace FormGenAI.Infrastructure.Data.FormContext;

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
using FormGenAI.Core.FormContext.Aggregates;
using FormGenAI.Core.FormContext.ValueObjects;
using System.Text.Json;

namespace FormGenAI.Infrastructure.Data.FormContext;

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
using FormGenAI.Core.FormContext.Aggregates;
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
using FormGenAI.Core.FormContext.Aggregates;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.Core.FormContext.ValueObjects;
using FormGenAI.Infrastructure.Data;

namespace FormGenAI.Infrastructure.Repositories.FormContext;

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

Update `FormGenAI.Web/appsettings.json`:
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
    "DefaultConnection": "Host=localhost;Database=FormGenAI;Username=postgres;Password=your_password"
  },
  "AllowedHosts": "*"
}
```

Update `FormGenAI.Web/appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormGenAI_Dev;Username=postgres;Password=your_password"
  }
}
```

**3.7 Create DependencyInjection:**

Create `Infrastructure/DependencyInjection.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.Infrastructure.Data;
using FormGenAI.Infrastructure.Repositories.FormContext;

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

        // TODO: Add repositories for other contexts as you implement them
        // services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        // services.AddScoped<ICodeGenerationJobRepository, CodeGenerationJobRepository>();

        return services;
    }
}
```

**3.8 Update Program.cs:**

Update `FormGenAI.Web/Program.cs`:
```csharp
using FormGenAI.Infrastructure;
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
cd src/FormGenAI.Infrastructure

# Create migration
dotnet ef migrations add InitialCreate \
  --startup-project ../FormGenAI.Web \
  --context AppDbContext

# Review the migration file in Migrations/ folder
# Then apply it:

dotnet ef database update \
  --startup-project ../FormGenAI.Web \
  --context AppDbContext
```

**3.10 Verify Database:**
```bash
# Connect to PostgreSQL
psql -U postgres -d FormGenAI

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
cd src/FormGenAI.UseCases
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
using FormGenAI.Core.FormContext.Aggregates;

namespace FormGenAI.UseCases.FormContext.DTOs;

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
using FormGenAI.Core.FormContext.Aggregates;

namespace FormGenAI.UseCases.FormContext.DTOs;

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
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Commands;

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
using FormGenAI.Core.FormContext.Aggregates;
using FormGenAI.Core.FormContext.ValueObjects;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Commands;

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
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Commands;

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
using FormGenAI.Core.FormContext.ValueObjects;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Commands;

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
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Queries;

/// <summary>
/// Query to get a form by ID
/// </summary>
public record GetFormByIdQuery(Guid FormId) : IRequest<FormDetailDto?>;
```

Create `FormContext/Queries/GetFormByIdQueryHandler.cs`:
```csharp
using MediatR;
using FormGenAI.Core.FormContext.Interfaces;
using FormGenAI.UseCases.FormContext.DTOs;

namespace FormGenAI.UseCases.FormContext.Queries;

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
using FormGenAI.UseCases.FormContext.DT
