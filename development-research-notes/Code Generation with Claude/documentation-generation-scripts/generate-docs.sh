#!/bin/bash

# FormDesignerAPI Migration Guide Generator
# Bash script to create all documentation files

echo "🚀 FormDesignerAPI Migration Guide Generator"
echo "============================================="
echo ""

# Create base directory
BASE_DIR="../../../docs/FormDesignerAPI-Migration-Guide"

if [ -d "$BASE_DIR" ]; then
    read -p "⚠️  Directory already exists. Remove it? (y/N): " response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        rm -rf "$BASE_DIR"
        echo "✓ Removed existing directory"
    else
        echo "❌ Aborted"
        exit 1
    fi
fi

mkdir -p "$BASE_DIR"/{diagrams,templates}
echo "✓ Created directory structure"

# Function to create file with content
create_file() {
    local filename="$1"
    local filepath="$BASE_DIR/$filename"
    shift
    printf "%s\n" "$@" > "$filepath"
    echo "  ✓ Created $filename"
}

# README.md
cat > "$BASE_DIR/README.md" << 'EOF'
# FormDesignerAPI Migration Guide - Complete Documentation

Welcome to the complete migration guide for transforming your Ardalis CleanArchitecture solution into a DDD-based AI-powered code generation system.

## 📚 Document Structure

This guide is organized into separate documents for easier navigation:

### Getting Started
- **[00-OVERVIEW.md](00-OVERVIEW.md)** - Executive summary, prerequisites, and architecture overview

### Phase-by-Phase Implementation
- **[01-PHASE-1-SKIP.md](01-PHASE-1-SKIP.md)** - Why we skip Phase 1 (using Traxs.SharedKernel)
- **[02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)** - Form Context domain model (START HERE)
- **[03-PHASE-3-FORM-INFRASTRUCTURE.md](03-PHASE-3-FORM-INFRASTRUCTURE.md)** - EF Core and repositories
- **[04-PHASE-4-FORM-API.md](04-PHASE-4-FORM-API.md)** - Use cases and REST API
- **[05-PHASE-5-CLAUDE-INTEGRATION.md](05-PHASE-5-CLAUDE-INTEGRATION.md)** - Claude API client
- **[06-PHASE-6-IMPORT-CONTEXT.md](06-PHASE-6-IMPORT-CONTEXT.md)** - PDF upload and extraction
- **[07-PHASE-7-CODE-GENERATION.md](07-PHASE-7-CODE-GENERATION.md)** - Templates and code generation
- **[08-PHASE-8-INTEGRATION.md](08-PHASE-8-INTEGRATION.md)** - Events and cross-context communication
- **[09-PHASE-9-TESTING.md](09-PHASE-9-TESTING.md)** - Testing and documentation

### Reference Materials
- **[APPENDIX-A-CODE-EXAMPLES.md](APPENDIX-A-CODE-EXAMPLES.md)** - Complete code samples
- **[APPENDIX-B-TROUBLESHOOTING.md](APPENDIX-B-TROUBLESHOOTING.md)** - Common issues and solutions
- **[APPENDIX-C-GLOSSARY.md](APPENDIX-C-GLOSSARY.md)** - Terms and definitions

## 🎯 Quick Start

**IMPORTANT:** This guide uses your existing `Traxs.SharedKernel` package, so Phase 1 is skipped!

1. Read **00-OVERVIEW.md** for prerequisites and architecture understanding
2. Read **01-PHASE-1-SKIP.md** to understand why we skip Phase 1
3. **Start with 02-PHASE-2-FORM-DOMAIN.md** to begin implementation
4. Follow phases sequentially (2-9)

Each phase includes:
- Clear objectives
- Step-by-step instructions
- Complete code examples
- Verification checklist
- Git commit message

## ⏱️ Estimated Timeline

| Phase | Duration | Complexity | Notes |
|-------|----------|------------|-------|
| Phase 1: Foundation | SKIP | N/A | Using Traxs.SharedKernel |
| Phase 2: Form Domain | 3-4 days | Medium | **START HERE** |
| Phase 3: Form Infrastructure | 2-3 days | Medium | |
| Phase 4: Form API | 3-4 days | Medium | |
| Phase 5: Claude Integration | 2-3 days | Medium | |
| Phase 6: Import Context | 5-7 days | High | |
| Phase 7: Code Generation | 10-14 days | High | |
| Phase 8: Integration | 3-5 days | Medium | |
| Phase 9: Testing | 5-7 days | Medium | |

**Total: 5-7 weeks** (1 week less due to skipping Phase 1!)

## 📋 Prerequisites Checklist

Before starting, ensure you have:
- [ ] .NET 9.0 SDK installed
- [ ] PostgreSQL running (Docker or local)
- [ ] Anthropic API key
- [ ] Visual Studio 2022 or Rider
- [ ] Git for version control
- [ ] Traxs.SharedKernel package (v0.1.1+)
- [ ] Basic understanding of DDD concepts

## 🏗️ Architecture Overview
```
FormDesignerAPI/
├── Traxs.SharedKernel (NuGet Package) ← Already have this!
├── Core/
│   ├── FormContext/       (Phase 2)
│   ├── ImportContext/     (Phase 6)
│   └── CodeGenContext/    (Phase 7)
├── UseCases/              (Phase 4)
├── Infrastructure/        (Phase 3, 5)
└── Web/                   (Phase 4)
```

## 📊 Progress Tracking

Track your progress by checking off completed phases:

- [x] Phase 1: Foundation ✅ (SKIPPED - Using Traxs.SharedKernel)
- [ ] Phase 2: Form Domain (START HERE)
- [ ] Phase 3: Form Infrastructure
- [ ] Phase 4: Form API
- [ ] Phase 5: Claude Integration
- [ ] Phase 6: Import Context
- [ ] Phase 7: Code Generation
- [ ] Phase 8: Integration
- [ ] Phase 9: Testing

## 🎁 Benefits of Using Traxs.SharedKernel

Your existing package provides:
- ✅ EntityBase with domain event support
- ✅ IAggregateRoot marker interface
- ✅ IDomainEvent and DomainEventBase
- ✅ ValueObject base class
- ✅ IRepository<T> with Ardalis.Specification
- ✅ MediatR integration for events
- ✅ LoggingBehavior for pipelines
- ✅ Support for Guid, int, and strongly-typed IDs

This saves you ~2-3 days of work!

---

**Ready to start?** Proceed to [00-OVERVIEW.md](00-OVERVIEW.md), then [02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)

**Last Updated:** December 2024  
**Version:** 2.0.0 (Updated for Traxs.SharedKernel)
EOF

echo "  ✓ Created README.md"

echo ""
echo "📄 Generating overview document..."

# 00-OVERVIEW.md
cat > "$BASE_DIR/00-OVERVIEW.md" << 'EOF'
# Overview: FormDesignerAPI Migration Guide

## Executive Summary

This migration guide transforms your existing Ardalis CleanArchitecture template into a comprehensive Domain-Driven Design (DDD) solution with AI-powered code generation capabilities.

**Important:** This guide is customized for your project which already has the `Traxs.SharedKernel` package, so we skip Phase 1!

### What You'll Build

An AI-powered system that:
1. ✅ **Accepts PDF form uploads** 
2. ✅ **Extracts form fields** using Claude Sonnet 4
3. ✅ **Generates complete application code** including:
   - C# domain models, repositories, controllers
   - SQL schemas and stored procedures
   - React components with Bootstrap
   - Unit and integration tests
   - CI/CD pipelines (GitHub Actions, Azure Pipelines)

### System Capabilities

After completing this migration, your system will:
- Process PDF forms and extract structured data
- Generate production-ready code in multiple languages
- Support versioning of form definitions
- Track code generation jobs and artifacts
- Provide REST APIs for all operations
- Include comprehensive test coverage

## Key Architectural Decisions

| Decision | Rationale | Impact |
|----------|-----------|--------|
| **Use Traxs.SharedKernel** | Already implemented, well-tested | Saves 2-3 days |
| **Domain-Driven Design** | Clear business domain separation | High maintainability |
| **Three Bounded Contexts** | Independent evolution | Scalable architecture |
| **Ardalis CleanArchitecture** | Proven pattern | Reduced learning curve |
| **PostgreSQL + JSONB** | Flexible schema storage | Dynamic form support |
| **Event-Driven Communication** | Decoupled contexts | Loose coupling |
| **Claude Sonnet 4 API** | Best-in-class AI | High-quality generation |
| **Ardalis.Specification** | Powerful query pattern | Already in Traxs.SharedKernel |

## Prerequisites

### Required Software Versions
```bash
# Verify your environment
dotnet --version        # Must be 9.0+ (your Traxs.SharedKernel uses 9.0)
node --version          # Must be 20.x+
docker --version        # Latest stable
git --version           # Latest stable
psql --version         # PostgreSQL 15+

# Install global tools
dotnet tool install --global dotnet-ef
dotnet ef --version    # Verify EF Core tools
```

### Expected Output
```
.NET SDK: 9.0.100 or higher
Node.js: v20.x.x
Docker: 24.x.x or higher
PostgreSQL: 15.x or higher
EF Core tools: 9.0.x
```

### Environment Setup

**1. PostgreSQL Database (Using Docker):**
```bash
# Start PostgreSQL container
docker run --name formdesigner-db \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -e POSTGRES_DB=FormDesignerAPI \
  -p 5432:5432 \
  -v formdesigner-data:/var/lib/postgresql/data \
  -d postgres:15

# Verify it's running
docker ps | grep formdesigner-db

# Connect to verify
docker exec -it formdesigner-db psql -U postgres -d FormDesignerAPI

# Inside psql:
\l                    # List databases
\q                    # Exit
```

**2. Anthropic API Key:**

Step-by-step:
1. Navigate to https://console.anthropic.com/
2. Sign up or log in
3. Go to **Settings** → **API Keys**
4. Click **Create Key**
5. Copy the key (starts with `sk-ant-`)
6. Store it securely (we'll add to configuration in Phase 5)

**Important:** Keep your API key secret. Never commit it to source control.

**3. Development Environment:**

Choose one:
- **Visual Studio 2022** (Community or higher)
- **JetBrains Rider** 
- **VS Code** with C# extension

### Backup Strategy

Before starting the migration:
```bash
# Create feature branch
git checkout -b feature/ddd-migration

# Create backup OUTSIDE your project directory
mkdir -p ~/FormDesignerAPI-Backups

# Option 1: Compressed archive (recommended)
tar -czf ~/FormDesignerAPI-Backups/FormDesignerAPI-backup-$(date +%Y%m%d).tar.gz \
  --exclude='.git' \
  --exclude='node_modules' \
  --exclude='bin' \
  --exclude='obj' \
  .

# Option 2: Git tag (best for Git projects)
git tag -a v0.0.0-pre-migration -m "State before DDD migration"

# Commit current state
git add .
git commit -m "Checkpoint: Before DDD migration ($(date +%Y-%m-%d))"
```

## Architecture Overview

### Target State: DDD with Bounded Contexts
```
FormDesignerAPI/
├── Traxs.SharedKernel (NuGet)  ← Already have this!
├── Core/
│   ├── FormContext/        # Phase 2
│   ├── ImportContext/      # Phase 6
│   └── CodeGenContext/     # Phase 7
├── UseCases/               # Phase 4
├── Infrastructure/         # Phase 3, 5
└── Web/                    # Phase 4
```

### Bounded Context Map
```
┌──────────────────────────────────────────────────────────┐
│                   FORM CONTEXT                            │
│  Aggregate: Form                                          │
│  Uses: Traxs.SharedKernel                                │
│  Responsibilities:                                        │
│  - Manage form definitions and metadata                  │
│  - Track form revisions over time                        │
│  - Store form origin (manual/import/API)                 │
│                                                          │
│  Published Events:                                       │
│  → FormCreatedEvent (DomainEventBase)                   │
│  → FormRevisionCreatedEvent (DomainEventBase)           │
└────────────────┬─────────────────────────────────────────┘
                 │
                 │ Domain Events (MediatR from Traxs.SharedKernel)
                 │
        ┌────────┴────────┬────────────────────┐
        │                 │                    │
        ▼                 ▼                    ▼
┌──────────────┐  ┌──────────────────┐  ┌──────────────┐
│   IMPORT     │  │   CODE GEN       │  │   FUTURE     │
│   CONTEXT    │  │   CONTEXT        │  │   CONTEXTS   │
│              │  │                  │  │              │
│ Aggregate:   │  │ Aggregate:       │  │ - Analytics  │
│ ImportBatch  │  │ CodeGenJob       │  │ - Deployment │
│              │  │                  │  │ - Versioning │
│ Subscribes:  │  │ Subscribes:      │  └──────────────┘
│ (none)       │  │ → FormCreated    │
│              │  │                  │
│ Publishes:   │  │ Publishes:       │
│ → Candidate  │  │ → Artifacts      │
│   Approved   │  │   Generated      │
└──────┬───────┘  └──────────────────┘
       │
       │ FormCandidateApprovedEvent
       │
       └────> Triggers Form Creation in FORM CONTEXT
```

### Clean Architecture Layers (Using Traxs.SharedKernel)
```
┌─────────────────────────────────────────────────────────┐
│     PRESENTATION LAYER                                  │
│     FormDesignerAPI.Web                                 │
│     - Controllers                                       │
│     - API Endpoints                                     │
└──────────────┬──────────────────────────────────────────┘
               │ Depends on ↓
┌──────────────┴──────────────────────────────────────────┐
│     APPLICATION LAYER                                   │
│     FormDesignerAPI.UseCases                            │
│     - Commands (write operations)                       │
│     - Queries (read operations)                         │
│     - DTOs                                              │
│     - MediatR handlers (from Traxs.SharedKernel)        │
└──────────────┬──────────────────────────────────────────┘
               │ Depends on ↓
┌──────────────┴──────────────────────────────────────────┐
│     DOMAIN LAYER (Core)                                 │
│     FormDesignerAPI.Core                                │
│     - Aggregates (EntityBase<Guid>)                     │
│     - Value Objects (ValueObject)                       │
│     - Domain Events (DomainEventBase)                   │
│     - Repository Interfaces (IRepository<T>)            │
│     Uses: Traxs.SharedKernel ✅                         │
└──────────────┬──────────────────────────────────────────┘
               │ ↑ Implemented by
┌──────────────┴──────────────────────────────────────────┐
│     INFRASTRUCTURE LAYER                                │
│     FormDesignerAPI.Infrastructure                      │
│     - EF Core DbContext                                 │
│     - Repository Implementations                        │
│     - External Service Clients (Claude API)             │
│     - MediatorDomainEventDispatcher                     │
└─────────────────────────────────────────────────────────┘
```

## Implementation Timeline

| Week | Phase | Focus | Deliverable |
|------|-------|-------|-------------|
| 1 | 2 | Form Domain | Aggregates + Events |
| 1-2 | 3 | Infrastructure | Database + Repos |
| 2 | 4 | API | REST endpoints |
| 3 | 5 | Claude | AI integration |
| 3-4 | 6 | Import | PDF processing |
| 5-7 | 7 | CodeGen | Templates |
| 7-8 | 8-9 | Final | Events + Tests |

## Success Criteria

- [ ] Using Traxs.SharedKernel successfully
- [ ] Can upload PDF and extract form
- [ ] Can generate working application code
- [ ] Generated code compiles
- [ ] All tests pass
- [ ] Documentation complete

## Next Steps

Proceed to **[01-PHASE-1-SKIP.md](01-PHASE-1-SKIP.md)** to understand why we skip Phase 1, then start with **[02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)**.

---

**Document Version:** 2.0.0  
**Last Updated:** December 2024  
**Customized for:** Traxs.SharedKernel
EOF

echo "  ✓ Created 00-OVERVIEW.md"

echo ""
echo "📄 Creating Phase 1 (SKIP) document..."

# 01-PHASE-1-SKIP.md
cat > "$BASE_DIR/01-PHASE-1-SKIP.md" << 'EOF'
# Phase 1: Foundation (SKIPPED - Using Traxs.SharedKernel)

**Status:** ✅ COMPLETE (Already have Traxs.SharedKernel)  
**Time Saved:** 2-3 days

---

## Why We Skip This Phase

The original migration guide includes Phase 1 where you would create a `FormDesignerAPI.SharedKernel` project with:
- EntityBase
- IAggregateRoot
- IDomainEvent
- ValueObject
- IRepository<T>
- Result<T>

**You already have all of this in `Traxs.SharedKernel`!** ✨

## What Your Traxs.SharedKernel Provides

Your package (v0.1.1) includes:

| Component | Description | Status |
|-----------|-------------|--------|
| `EntityBase<TId>` | Base class for entities with Guid/int/custom IDs | ✅ Better than guide |
| `HasDomainEventsBase` | Domain event management | ✅ Perfect |
| `IAggregateRoot` | Marker interface for aggregate roots | ✅ Perfect |
| `IDomainEvent` | Interface for domain events | ✅ MediatR integrated |
| `DomainEventBase` | Base implementation for events | ✅ Perfect |
| `ValueObject` | Base class for value objects | ✅ Optimized |
| `IRepository<T>` | Repository interface | ✅ With Ardalis.Specification! |
| `IReadRepository<T>` | Read-only repository | ✅ Bonus feature |
| `MediatorDomainEventDispatcher` | Event dispatching | ✅ Production-ready |
| `LoggingBehavior` | Pipeline logging | ✅ Bonus feature |

## Advantages Over the Guide

Your `Traxs.SharedKernel` is actually **better** than what the guide would have you build:

1. **Ardalis.Specification Support** 
   - More powerful than basic repository pattern
   - Allows complex queries without leaking infrastructure concerns

2. **Multiple ID Type Support**
   - `EntityBase` (int IDs)
   - `EntityBase<TId>` (any struct: Guid, long, etc.)
   - `EntityBase<T, TId>` (for strongly-typed IDs with Vogen)

3. **Production-Ready Event Dispatching**
   - `MediatorDomainEventDispatcher` already implemented
   - Integrated with MediatR

4. **Logging Pipeline Behavior**
   - `LoggingBehavior<TRequest, TResponse>`
   - Automatic request/response logging

5. **Well-Tested**
   - You have unit tests already
   - Published as NuGet package
   - Version controlled

## What You Need to Do

### Step 1: Verify Package Installation

Check that Traxs.SharedKernel is available:
```bash
# Check if published to your GitHub Packages
dotnet nuget list source

# You should see your github source
```

### Step 2: Add to Your Projects
```bash
cd src/FormDesignerAPI.Core
dotnet add package Traxs.SharedKernel

cd ../FormDesignerAPI.Infrastructure
dotnet add package Traxs.SharedKernel

cd ../FormDesignerAPI.UseCases
dotnet add package Traxs.SharedKernel

cd ../FormDesignerAPI.Web
dotnet add package Traxs.SharedKernel
```

### Step 3: Verify Installation
```bash
cd src/FormDesignerAPI.Core
dotnet list package
```

You should see:
```
Traxs.SharedKernel    0.1.1
```

## Key Differences in Implementation

Throughout the remaining phases, use these patterns:

### Entity Base Class

**Original Guide:**
```csharp
public class Form : EntityBase, IAggregateRoot
```

**With Traxs.SharedKernel (Use This):**
```csharp
public class Form : EntityBase<Guid>, IAggregateRoot
```

### Namespaces

**Original Guide:**
```csharp
using FormDesignerAPI.SharedKernel.Base;
using FormDesignerAPI.SharedKernel.Interfaces;
```

**With Traxs.SharedKernel (Use This):**
```csharp
using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Ardalis.Specification;
```

### Repository Pattern

Your Traxs.SharedKernel uses Ardalis.Specification, which means you get powerful querying:
```csharp
// Basic operations (from IRepository<T>)
var form = await _repository.GetByIdAsync(id);
var allForms = await _repository.ListAsync();

// Specification pattern (powerful!)
var spec = new GetFormsByOriginTypeSpec(OriginType.Import);
var importedForms = await _repository.ListAsync(spec);

// Paging support
var spec = new GetFormsSpec();
spec.Query.Skip(10).Take(20);
var pagedForms = await _repository.ListAsync(spec);
```

## Verification Checklist

Before proceeding to Phase 2, verify:

- [ ] Traxs.SharedKernel package is published to your GitHub Packages
- [ ] You can restore packages from your GitHub feed
- [ ] Package version is 0.1.1 or higher
- [ ] You understand how to use `EntityBase<Guid>`
- [ ] You understand the Specification pattern

## Next Steps

**You're ready for Phase 2!**

Proceed to **[02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)** to start building the Form Context domain model.

---

**Phase 1: SKIPPED ✅**

Time saved: 2-3 days  
You can thank yourself for building Traxs.SharedKernel ahead of time! 🎉
EOF

echo "  ✓ Created 01-PHASE-1-SKIP.md"

echo ""
echo "📄 Creating Phase 2 (Form Domain) document..."

# 02-PHASE-2-FORM-DOMAIN.md - This will be the complete version we created earlier
cat > "$BASE_DIR/02-PHASE-2-FORM-DOMAIN.md" << 'PHASE2EOF'
# Phase 2: Form Context - Domain Model (Using Traxs.SharedKernel)

**Duration:** 3-4 days  
**Complexity:** Medium  
**Prerequisites:** Traxs.SharedKernel package available

---

## Overview

In this phase, you'll create the Form Context domain model using your `Traxs.SharedKernel` package. You'll build aggregates, value objects, and domain events following DDD principles.

## Objectives

- [ ] Add Traxs.SharedKernel to Core project
- [ ] Create Form Context folder structure
- [ ] Implement value objects (OriginType, OriginMetadata, FormDefinition)
- [ ] Create Form aggregate root
- [ ] Create FormRevision entity
- [ ] Define domain events
- [ ] Create repository interface
- [ ] Write unit tests
- [ ] Verify domain layer has no infrastructure dependencies

---

## Step 1: Add Traxs.SharedKernel Package

### 1.1 Add package reference
```bash
cd src/FormDesignerAPI.Core
dotnet add package Traxs.SharedKernel
```

### 1.2 Verify installation
```bash
dotnet list package
```

You should see:
```
Traxs.SharedKernel    0.1.1
```

---

## Step 2: Create Folder Structure
```bash
cd src/FormDesignerAPI.Core

# Create Form Context structure
mkdir -p FormContext/Aggregates
mkdir -p FormContext/ValueObjects
mkdir -p FormContext/Events
mkdir -p FormContext/Interfaces
mkdir -p FormContext/Specifications
```

---

## Step 3: Create Value Objects

3.1 Create OriginType Enum
File: FormContext/ValueObjects/OriginType.cs

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

3.2 Create OriginMetadata Value Object
File: FormContext/ValueObjects/OriginMetadata.cs

```csharp
using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Tracks how and when a form was created
/// Immutable value object
/// </summary>
public class OriginMetadata : ValueObject
{
  public OriginType Type { get; init; }
  public string? ReferenceId { get; init; }
  public DateTime CreatedAt { get; init; }
  public string CreatedBy { get; init; } = string.Empty;

  // Private constructor - use factory methods
  private OriginMetadata() { }

  /// <summary>
  /// Create origin metadata for a manually created form
  /// </summary>
  public static OriginMetadata Manual(string createdBy)
  {
    if (string.IsNullOrWhiteSpace(createdBy))
      throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

    return new OriginMetadata
    {
      Type = OriginType.Manual,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = createdBy
    };
  }

  /// <summary>
  /// Create origin metadata for an imported form
  /// </summary>
  public static OriginMetadata Import(string candidateId, string approvedBy)
  {
    if (string.IsNullOrWhiteSpace(candidateId))
      throw new ArgumentException("CandidateId cannot be empty", nameof(candidateId));
    if (string.IsNullOrWhiteSpace(approvedBy))
      throw new ArgumentException("ApprovedBy cannot be empty", nameof(approvedBy));

    return new OriginMetadata
    {
      Type = OriginType.Import,
      ReferenceId = candidateId,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = approvedBy
    };
  }

  /// <summary>
  /// Create origin metadata for an API-created form
  /// </summary>
  public static OriginMetadata Api(string createdBy, string? externalId = null)
  {
    if (string.IsNullOrWhiteSpace(createdBy))
      throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

    return new OriginMetadata
    {
      Type = OriginType.API,
      ReferenceId = externalId,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = createdBy
    };
  }

  /// <summary>
  /// Create origin metadata for a template-based form
  /// </summary>
  public static OriginMetadata Template(string templateId, string createdBy)
  {
    if (string.IsNullOrWhiteSpace(templateId))
      throw new ArgumentException("TemplateId cannot be empty", nameof(templateId));
    if (string.IsNullOrWhiteSpace(createdBy))
      throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

    return new OriginMetadata
    {
      Type = OriginType.Template,
      ReferenceId = templateId,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = createdBy
    };
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Type;
    yield return ReferenceId ?? string.Empty;
    yield return CreatedAt;
    yield return CreatedBy;
  }
}
3.3 Create FormField Record
File: FormContext/ValueObjects/FormField.cs
```csharp
namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents a single field in a form definition
/// Using record for immutability
/// </summary>
public record FormField
{
  public string Name { get; init; } = string.Empty;
  public string Type { get; init; } = string.Empty;
  public bool Required { get; init; }
  public string? Label { get; init; }
  public string? Placeholder { get; init; }
  public string? DefaultValue { get; init; }
  public int? MinLength { get; init; }
  public int? MaxLength { get; init; }
  public string? Pattern { get; init; }
  public Dictionary<string, object>? ValidationRules { get; init; }
  public List<string>? Options { get; init; } // For select/radio fields

  /// <summary>
  /// Validate that the field definition is complete
  /// </summary>
  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(Name)) return false;
    if (string.IsNullOrWhiteSpace(Type)) return false;
    
    // If it's a select/radio, must have options
    if ((Type == "select" || Type == "radio") && 
        (Options == null || Options.Count == 0))
      return false;

    return true;
  }
}
```
3.4 Create FormDefinition Value Object
File: FormContext/ValueObjects/FormDefinition.cs
```csharp
using System.Text.Json;
using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents the structure and fields of a form
/// Immutable value object
/// </summary>
public class FormDefinition : ValueObject
{
  public string Schema { get; init; } = string.Empty;
  public IReadOnlyList<FormField> Fields { get; init; } = new List<FormField>();

  // Private constructor
  private FormDefinition() { }

  /// <summary>
  /// Create a form definition from a JSON schema
  /// </summary>
  public static FormDefinition From(string jsonSchema)
  {
    if (string.IsNullOrWhiteSpace(jsonSchema))
      throw new ArgumentException("JSON schema cannot be empty", nameof(jsonSchema));

    var fields = ParseFields(jsonSchema);

    return new FormDefinition
    {
      Schema = jsonSchema,
      Fields = fields
    };
  }

  /// <summary>
  /// Create a form definition from a list of fields
  /// </summary>
  public static FormDefinition FromFields(List<FormField> fields)
  {
    if (fields == null || fields.Count == 0)
      throw new ArgumentException("Fields list cannot be empty", nameof(fields));

    // Validate all fields
    if (fields.Any(f => !f.IsValid()))
      throw new ArgumentException("One or more fields are invalid");

    var schema = GenerateSchema(fields);

    return new FormDefinition
    {
      Schema = schema,
      Fields = fields.AsReadOnly()
    };
  }

  private static List<FormField> ParseFields(string jsonSchema)
  {
    try
    {
      var doc = JsonDocument.Parse(jsonSchema);
      var root = doc.RootElement;

      var fields = new List<FormField>();

      // Check if there's a "fields" array
      if (root.TryGetProperty("fields", out var fieldsArray))
      {
        foreach (var fieldElement in fieldsArray.EnumerateArray())
        {
          var field = ParseField(fieldElement);
          if (field != null)
            fields.Add(field);
        }
      }

      return fields;
    }
    catch (JsonException ex)
    {
      throw new ArgumentException($"Invalid JSON schema: {ex.Message}", nameof(jsonSchema));
    }
  }

  private static FormField? ParseField(JsonElement element)
  {
    try
    {
      var name = element.GetProperty("name").GetString() ?? string.Empty;
      var type = element.GetProperty("type").GetString() ?? string.Empty;

      var field = new FormField
      {
        Name = name,
        Type = type,
        Required = element.TryGetProperty("required", out var req) && req.GetBoolean(),
        Label = element.TryGetProperty("label", out var lbl) ? lbl.GetString() : null,
        Placeholder = element.TryGetProperty("placeholder", out var ph) ? ph.GetString() : null,
        DefaultValue = element.TryGetProperty("defaultValue", out var dv) ? dv.GetString() : null,
        MinLength = element.TryGetProperty("minLength", out var min) ? min.GetInt32() : null,
        MaxLength = element.TryGetProperty("maxLength", out var max) ? max.GetInt32() : null,
        Pattern = element.TryGetProperty("pattern", out var pat) ? pat.GetString() : null
      };

      // Parse options for select/radio fields
      if (element.TryGetProperty("options", out var opts))
      {
        var options = new List<string>();
        foreach (var opt in opts.EnumerateArray())
        {
          var optValue = opt.GetString();
          if (!string.IsNullOrEmpty(optValue))
            options.Add(optValue);
        }
        field = field with { Options = options };
      }

      return field;
    }
    catch
    {
      return null;
    }
  }

  private static string GenerateSchema(List<FormField> fields)
  {
    var schema = new
    {
      fields = fields.Select(f => new
      {
        name = f.Name,
        type = f.Type,
        required = f.Required,
        label = f.Label,
        placeholder = f.Placeholder,
        defaultValue = f.DefaultValue,
        minLength = f.MinLength,
        maxLength = f.MaxLength,
        pattern = f.Pattern,
        options = f.Options
      })
    };

    return JsonSerializer.Serialize(schema, new JsonSerializerOptions
    {
      WriteIndented = true,
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    });
  }

  /// <summary>
  /// Get a specific field by name
  /// </summary>
  public FormField? GetField(string fieldName)
  {
    return Fields.FirstOrDefault(f => 
      f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>
  /// Check if definition has a specific field
  /// </summary>
  public bool HasField(string fieldName)
  {
    return Fields.Any(f => 
      f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Schema;
    foreach (var field in Fields)
    {
      yield return field;
    }
  }
}
```

---

## Step 4: Create Domain Events

4.1 Create FormCreatedEvent
File: FormContext/Events/FormCreatedEvent.cs

```csharp
using Traxs.SharedKernel;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form is created
/// </summary>
public record FormCreatedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public string Name { get; init; }
  public OriginMetadata Origin { get; init; }
  public string CreatedBy { get; init; }

  public FormCreatedEvent(
    Guid formId,
    string name,
    OriginMetadata origin,
    string createdBy)
  {
    FormId = formId;
    Name = name;
    Origin = origin;
    CreatedBy = createdBy;
  }
}
```

4.2 Create FormRevisionCreatedEvent
File: FormContext/Events/FormRevisionCreatedEvent.cs

```csharp
using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form revision is created
/// </summary>
public record FormRevisionCreatedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public Guid RevisionId { get; init; }
  public int Version { get; init; }
  public string CreatedBy { get; init; }

  public FormRevisionCreatedEvent(
    Guid formId,
    Guid revisionId,
    int version,
    string createdBy)
  {
    FormId = formId;
    RevisionId = revisionId;
    Version = version;
    CreatedBy = createdBy;
  }
}
```

4.3 Create FormRenamedEvent
File: FormContext/Events/FormRenamedEvent.cs

```csharp
using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a form is renamed
/// </summary>
public record FormRenamedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public string OldName { get; init; }
  public string NewName { get; init; }
  public string RenamedBy { get; init; }

  public FormRenamedEvent(
    Guid formId,
    string oldName,
    string newName,
    string renamedBy)
  {
    FormId = formId;
    OldName = oldName;
    NewName = newName;
    RenamedBy = renamedBy;
  }
}
```

---

## Step 5: Create Aggregate Root and Entities

5.1 Create FormRevision Entity
File: FormContext/Aggregates/FormRevision.cs

```csharp
using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// FormRevision entity - represents a version of a form
/// NOT an aggregate root - managed by Form aggregate
/// </summary>
public class FormRevision : EntityBase<Guid>
{
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
  /// Internal - only Form aggregate can create revisions
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
    Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
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

  /// <summary>
  /// Check if this revision has breaking changes from another
  /// </summary>
  public bool HasBreakingChangesFrom(FormRevision other)
  {
    // Field removed
    var thisFieldNames = Definition.Fields.Select(f => f.Name).ToHashSet();
    var otherFieldNames = other.Definition.Fields.Select(f => f.Name).ToHashSet();
    
    if (otherFieldNames.Except(thisFieldNames).Any())
      return true;

    // Required field added
    var newRequiredFields = Definition.Fields
      .Where(f => f.Required)
      .Select(f => f.Name)
      .Except(other.Definition.Fields.Where(f => f.Required).Select(f => f.Name));
    
    if (newRequiredFields.Any())
      return true;

    // Field type changed
    foreach (var fieldName in thisFieldNames.Intersect(otherFieldNames))
    {
      var thisField = Definition.GetField(fieldName);
      var otherField = other.Definition.GetField(fieldName);
      
      if (thisField?.Type != otherField?.Type)
        return true;
    }

    return false;
  }
}
```

5.2 Create Form Aggregate Root
File: FormContext/Aggregates/Form.cs

```csharp
using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Events;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// Form aggregate root - manages form definitions and their revisions
/// </summary>
public class Form : EntityBase<Guid>, IAggregateRoot
{
  // Properties
  public string Name { get; private set; } = string.Empty;
  public FormDefinition Definition { get; private set; } = null!;
  public OriginMetadata Origin { get; private set; } = null!;
  public bool IsActive { get; private set; } = true;
  
  private readonly List<FormRevision> _revisions = new();
  public IReadOnlyCollection<FormRevision> Revisions => _revisions.AsReadOnly();
  
  /// <summary>
  /// Get the most recent revision
  /// </summary>
  public FormRevision CurrentRevision => 
    _revisions.OrderByDescending(r => r.Version).First();

  /// <summary>
  /// Get the current version number
  /// </summary>
  public int CurrentVersion => 
    _revisions.Any() ? _revisions.Max(r => r.Version) : 0;

  /// <summary>
  /// Total number of fields in current definition
  /// </summary>
  public int FieldCount => Definition.Fields.Count;

  // Private constructor for EF Core
  private Form() { }
  
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
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.Null(definition, nameof(definition));
    Guard.Against.Null(origin, nameof(origin));
    Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
    
    // Validate definition has fields
    if (definition.Fields.Count == 0)
      throw new ArgumentException("Form must have at least one field", nameof(definition));

    var form = new Form
    {
      Id = Guid.NewGuid(),
      Name = name,
      Definition = definition,
      Origin = origin,
      IsActive = true
    };
    
    // Create initial revision (version 1)
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
    Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));

    // Validate form is active
    if (!IsActive)
      throw new InvalidOperationException("Cannot create revision for inactive form");

    // Validate definition has fields
    if (newDefinition.Fields.Count == 0)
      throw new ArgumentException("Form revision must have at least one field");

    var nextVersion = CurrentVersion + 1;
    
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
  public void Rename(string newName, string renamedBy)
  {
    Guard.Against.NullOrWhiteSpace(newName, nameof(newName));
    Guard.Against.NullOrWhiteSpace(renamedBy, nameof(renamedBy));

    if (newName == Name)
      return; // No change

    var oldName = Name;
    Name = newName;

    RegisterDomainEvent(new FormRenamedEvent(
      Id,
      oldName,
      newName,
      renamedBy
    ));
  }

  /// <summary>
  /// Deactivate the form
  /// </summary>
  public void Deactivate()
  {
    IsActive = false;
  }

  /// <summary>
  /// Reactivate the form
  /// </summary>
  public void Activate()
  {
    IsActive = true;
  }

  /// <summary>
  /// Get a specific revision by version number
  /// </summary>
  public FormRevision? GetRevision(int version)
  {
    return _revisions.FirstOrDefault(r => r.Version == version);
  }

  /// <summary>
  /// Check if a field exists in the current definition
  /// </summary>
  public bool HasField(string fieldName)
  {
    return Definition.HasField(fieldName);
  }
}
```

---

## Step 6: Create Repository Interface

6.1 Create IFormRepository
File: FormContext/Interfaces/IFormRepository.cs

```csharp
using Traxs.SharedKernel;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Interfaces;

/// <summary>
/// Repository interface for Form aggregate
/// Inherits from Traxs.SharedKernel IRepository which uses Ardalis.Specification
/// </summary>
public interface IFormRepository : IRepository<Form>
{
  /// <summary>
  /// Get form with all its revisions eagerly loaded
  /// </summary>
  Task<Form?> GetByIdWithRevisionsAsync(
    Guid id, 
    CancellationToken cancellationToken = default);
  
  /// <summary>
  /// Get form by name
  /// </summary>
  Task<Form?> GetByNameAsync(
    string name,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a form with the given name already exists
  /// </summary>
  Task<bool> ExistsWithNameAsync(
    string name,
    CancellationToken cancellationToken = default);
}
```

---

## Step 7: Create Specifications

Specifications are a powerful pattern from Ardalis.Specification that your Traxs.SharedKernel supports.

7.1 Create GetFormsByOriginTypeSpec
File: FormContext/Specifications/GetFormsByOriginTypeSpec.cs

```csharp
using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to get forms by origin type
/// </summary>
public class GetFormsByOriginTypeSpec : Specification<Form>
{
  public GetFormsByOriginTypeSpec(OriginType originType)
  {
    Query
      .Where(f => f.Origin.Type == originType)
      .OrderByDescending(f => f.Origin.CreatedAt);
  }
}
```

7.2 Create GetActiveFormsSpec
File: FormContext/Specifications/GetActiveFormsSpec.cs

```csharp
using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to get all active forms
/// </summary>
public class GetActiveFormsSpec : Specification<Form>
{
  public GetActiveFormsSpec()
  {
    Query
      .Where(f => f.IsActive)
      .OrderByDescending(f => f.Origin.CreatedAt);
  }
}
```

7.3 Create SearchFormsByNameSpec
File: FormContext/Specifications/SearchFormsByNameSpec.cs

```csharp
using Ardalis.Specification;
using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Specifications;

/// <summary>
/// Specification to search forms by name (case-insensitive)
/// </summary>
public class SearchFormsByNameSpec : Specification<Form>
{
  public SearchFormsByNameSpec(string searchTerm)
  {
    Query
      .Where(f => f.Name.ToLower().Contains(searchTerm.ToLower()))
      .OrderBy(f => f.Name);
  }
}
```

---

## Step 8: Create Unit Tests

8.1 Build the project
bash
cd src/FormDesignerAPI.Core
dotnet build
```

### 8.2 Verify no errors

You should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
### 8.3 Verify dependencies

```bash
dotnet list package
```

Should show:
```
Project 'FormDesignerAPI.Core' has the following package references
   [net9.0]: 
   Top-level Package               Requested   Resolved
   > Ardalis.GuardClauses          5.0.0       5.0.0
   > Traxs.SharedKernel            0.1.1       0.1.1

```

### Step 9: Create Unit Tests

#### 9.1 Add test project reference

```bash
cd tests/FormDesignerAPI.UnitTests
dotnet add reference ../../src/FormDesignerAPI.Core/FormDesignerAPI.Core.csproj
dotnet add package Traxs.SharedKernel
```

#### 9.2 Create Form Tests

File: tests/FormDesignerAPI.UnitTests/FormContext/FormTests.cs

```csharp
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Events;
using Xunit;
using FluentAssertions;

namespace FormDesignerAPI.UnitTests.FormContext;

public class FormTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateForm()
    {
        // Arrange
        var name = "Patient Intake Form";
        var fields = new List<FormField>
        {
        new FormField { Name = "firstName", Type = "text", Required = true, Label = "First Name" },
        new FormField { Name = "lastName", Type = "text", Required = true, Label = "Last Name" }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");

        // Act
        var form = Form.Create(name, definition, origin, "admin@test.com");

        // Assert
        form.Should().NotBeNull();
        form.Id.Should().NotBeEmpty();
        form.Name.Should().Be(name);
        form.Definition.Should().Be(definition);
        form.Origin.Should().Be(origin);
        form.IsActive.Should().BeTrue();
        form.Revisions.Should().HaveCount(1);
        form.CurrentVersion.Should().Be(1);
        form.FieldCount.Should().Be(2);
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseFormCreatedEvent()
    {
        // Arrange
        var name = "Test Form";
        var fields = new List<FormField>
        {
        new FormField { Name = "field1", Type = "text", Required = true }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");

        // Act
        var form = Form.Create(name, definition, origin, "admin@test.com");

        // Assert
        form.DomainEvents.Should().HaveCount(1);
        var domainEvent = form.DomainEvents.First();
        domainEvent.Should().BeOfType<FormCreatedEvent>();
        
        var formCreatedEvent = (FormCreatedEvent)domainEvent;
        formCreatedEvent.FormId.Should().Be(form.Id);
        formCreatedEvent.Name.Should().Be(name);
        formCreatedEvent.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var fields = new List<FormField>
        {
        new FormField { Name = "field1", Type = "text", Required = true }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
        Form.Create("", definition, origin, "admin@test.com"));
    }
        
    [Fact]
    public void CreateRevision_WithValidData_ShouldCreateNewRevision()
    {
        // Arrange
        var form = CreateTestForm();
        var newFields = new List<FormField>
        {
            new FormField { Name = "firstName", Type = "text", Required = true },
            new FormField { Name = "email", Type = "email", Required = true, Label = "Email" }
        };
        var newDefinition = FormDefinition.FromFields(newFields);
        // Act
        form.CreateRevision(newDefinition, "Added email field", "admin@test.com");

        // Assert
        form.Revisions.Should().HaveCount(2);
        form.CurrentVersion.Should().Be(2);
        form.Definition.Should().Be(newDefinition);
        form.DomainEvents.Should().HaveCount(2); // FormCreated + FormRevisionCreated
    }

    [Fact]
    public void Rename_WithNewName_ShouldUpdateNameAndRaiseEvent()
    {
        // Arrange
        var form = CreateTestForm();
        var oldName = form.Name;
        var newName = "Updated Form Name";
        // Act
        form.Rename(newName, "admin@test.com");

        // Assert
        form.Name.Should().Be(newName);
        form.DomainEvents.Should().Contain(e => e is FormRenamedEvent);

        var renameEvent = form.DomainEvents.OfType<FormRenamedEvent>().First();
        renameEvent.OldName.Should().Be(oldName);
        renameEvent.NewName.Should().Be(newName);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var form = CreateTestForm();
        // Act
        form.Deactivate();

        // Assert
        form.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CreateRevision_OnInactiveForm_ShouldThrowException()
    {
        // Arrange
        var form = CreateTestForm();
        form.Deactivate();
        var newFields = new List<FormField>
        {
            new FormField { Name = "field1", Type = "text", Required = true }
        };
        var newDefinition = FormDefinition.FromFields(newFields);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
        form.CreateRevision(newDefinition, "test", "admin@test.com"));
    }

    // Helper method
    private Form CreateTestForm()
    {
        var fields = new List<FormField>
        {
            new FormField { Name = "firstName", Type = "text", Required = true, Label = "First Name" }
        };
        var definition = FormDefinition.FromFields(fields);
        var origin = OriginMetadata.Manual("admin@test.com");
        return Form.Create("Test Form", definition, origin, "admin@test.com");
    }
}
```

### 9.3 Create OriginMetadata Tests

**File:** `tests/FormDesignerAPI.UnitTests/FormContext/OriginMetadataTests.cs`

```csharp
using FormDesignerAPI.Core.FormContext.ValueObjects;
using Xunit;
using FluentAssertions;

namespace FormDesignerAPI.UnitTests.FormContext;

public class OriginMetadataTests
{
  [Fact]
  public void Manual_WithValidCreatedBy_ShouldCreateOrigin()
  {
    // Act
    var origin = OriginMetadata.Manual("admin@test.com");

    // Assert
    origin.Type.Should().Be(OriginType.Manual);
    origin.CreatedBy.Should().Be("admin@test.com");
    origin.ReferenceId.Should().BeNull();
    origin.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void Import_WithValidData_ShouldCreateOrigin()
  {
    // Act
    var origin = OriginMetadata.Import("candidate-123", "approver@test.com");

    // Assert
    origin.Type.Should().Be(OriginType.Import);
    origin.ReferenceId.Should().Be("candidate-123");
    origin.CreatedBy.Should().Be("approver@test.com");
  }

  [Fact]
  public void Manual_WithEmptyCreatedBy_ShouldThrowException()
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(() => OriginMetadata.Manual(""));
  }

  [Fact]
  public void Equals_WithSameValues_ShouldBeEqual()
  {
    // Arrange
    var origin1 = OriginMetadata.Manual("admin@test.com");
    System.Threading.Thread.Sleep(10); // Ensure different timestamp
    var origin2 = OriginMetadata.Manual("admin@test.com");

    // Assert - Should NOT be equal due to different timestamps
    origin1.Should().NotBe(origin2);
  }
}
```

### 9.4 Run tests

```bash
cd tests/FormDesignerAPI.UnitTests
dotnet test
```

---

## Verification Checklist

- [ ] Traxs.SharedKernel package added to Core project
- [ ] All value objects compile without errors
- [ ] Form aggregate compiles without errors
- [ ] FormRevision entity compiles without errors
- [ ] All domain events compile without errors
- [ ] Repository interface compiles without errors
- [ ] Specifications compile without errors
- [ ] No infrastructure dependencies in Core project
- [ ] Unit tests pass
- [ ] Domain events are raised correctly
- [ ] Aggregate enforces invariants (guard clauses work)

---

## Common Issues and Solutions

### Issue 1: "EntityBase<Guid> not found"

**Solution:** Make sure you're using `EntityBase<Guid>` not just `EntityBase`. Your Traxs.SharedKernel supports both.

### Issue 2: "RegisterDomainEvent is protected"

**Solution:** This is correct. `RegisterDomainEvent` is `protected` and should only be called from within the entity itself.

### Issue 3: "IRepository<T> not found"

**Solution:** Ensure you have `using Traxs.SharedKernel;` at the top of your file.

---

## Git Commit

```bash
git add .
git commit -m "Phase 2: Implemented Form Context domain model with Traxs.SharedKernel

- Created value objects (OriginMetadata, FormDefinition, FormField)
- Implemented Form aggregate root with Guid IDs
- Added FormRevision entity
- Created domain events (FormCreated, FormRevisionCreated, FormRenamed)
- Defined IFormRepository interface
- Added Ardalis.Specification-based specifications
- Wrote comprehensive unit tests
- All tests passing"
```

---

**Phase 2 Complete!** ✅

Proceed to **[03-PHASE-3-FORM-INFRASTRUCTURE.md](03-PHASE-3-FORM-INFRASTRUCTURE.md)**
PHASE2EOF

echo "  ✓ Created 02-PHASE-2-FORM-DOMAIN.md"

echo ""
echo "📄 Creating remaining phase placeholders..."

# Create placeholder files for phases 3-9
for i in {3..9}; do
    phase_num=$(printf "%02d" $i)
    
    case $i in
        3) phase_name="PHASE-3-FORM-INFRASTRUCTURE"; title="Phase 3: Form Context - Infrastructure" ;;
        4) phase_name="PHASE-4-FORM-API"; title="Phase 4: Form Context - Use Cases & API" ;;
        5) phase_name="PHASE-5-CLAUDE-INTEGRATION"; title="Phase 5: Claude API Integration" ;;
        6) phase_name="PHASE-6-IMPORT-CONTEXT"; title="Phase 6: Import Context" ;;
        7) phase_name="PHASE-7-CODE-GENERATION"; title="Phase 7: Code Generation Context" ;;
        8) phase_name="PHASE-8-INTEGRATION"; title="Phase 8: Integration & Events" ;;
        9) phase_name="PHASE-9-TESTING"; title="Phase 9: Testing & Documentation" ;;
    esac
    
    cat > "$BASE_DIR/${phase_num}-${phase_name}.md" << EOF
# $title

**Duration:** TBD  
**Complexity:** Medium-High  
**Prerequisites:** Previous phases complete

## Overview

[To be implemented]

## Objectives

- [ ] Objective 1
- [ ] Objective 2

## Steps

[Detailed steps to follow]

## Verification

- [ ] All tests pass
- [ ] Code compiles

## Next Steps

Continue to next phase.
EOF
    
    echo "  ✓ Created ${phase_num}-${phase_name}.md"
done

echo ""
echo "📚 Generating appendices..."

# APPENDIX-A
cat > "$BASE_DIR/APPENDIX-A-CODE-EXAMPLES.md" << 'EOF'
# Appendix A: Complete Code Examples

## Using Traxs.SharedKernel

### Entity with Guid ID
```csharp
using Traxs.SharedKernel;

public class MyEntity : EntityBase<Guid>, IAggregateRoot
{
    public string Name { get; private set; }
    
    private MyEntity() { }
    
    public static MyEntity Create(string name)
    {
        var entity = new MyEntity 
        { 
            Id = Guid.NewGuid(),
            Name = name 
        };
        
        entity.RegisterDomainEvent(new MyEntityCreatedEvent(entity.Id));
        return entity;
    }
}
```

### Domain Event
```csharp
using Traxs.SharedKernel;

public record MyEntityCreatedEvent(Guid EntityId) : DomainEventBase;
```

### Repository Interface
```csharp
using Traxs.SharedKernel;

public interface IMyRepository : IRepository<MyEntity>
{
    Task<MyEntity?> GetByNameAsync(string name);
}
```

### Specification
```csharp
using Ardalis.Specification;

public class GetActiveEntitiesSpec : Specification<MyEntity>
{
    public GetActiveEntitiesSpec()
    {
        Query.Where(e => e.IsActive)
             .OrderBy(e => e.Name);
    }
}
```

[Additional examples...]
EOF

echo "  ✓ Created APPENDIX-A-CODE-EXAMPLES.md"

# APPENDIX-B
cat > "$BASE_DIR/APPENDIX-B-TROUBLESHOOTING.md" << 'EOF'
# Appendix B: Troubleshooting Guide

## Traxs.SharedKernel Issues

### Error: "Package 'Traxs.SharedKernel' not found"

**Cause:** Package not available in configured sources

**Solution:**
```bash
# Check your NuGet sources
dotnet nuget list source

# Add GitHub Packages source if missing
dotnet nuget add source \
  --name github \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_PAT \
  --store-password-in-clear-text \
  "https://nuget.pkg.github.com/petelc/index.json"
```

### Error: "EntityBase<Guid> not found"

**Cause:** Wrong base class used

**Solution:** Use `EntityBase<Guid>` not just `EntityBase`
```csharp
// Wrong
public class Form : EntityBase, IAggregateRoot
// Correct
public class Form : EntityBase<Guid>, IAggregateRoot
[Additional troubleshooting...]
EOF

echo "  ✓ Created APPENDIX-B-TROUBLESHOOTING.md"

# APPENDIX-C
cat > "$BASE_DIR/APPENDIX-C-GLOSSARY.md" << 'EOF'
# Appendix C: Glossary

## DDD Terms

**Aggregate Root**  
The main entity that enforces invariants and controls access to other entities.

**Entity**  
An object with a unique identity (ID).

**Value Object**  
An immutable object defined by its attributes, not identity.

**Domain Event**  
Something that happened in the domain.

**Specification Pattern**  
Encapsulates query logic in reusable, composable objects.

## Project-Specific

**Traxs.SharedKernel**  
Your custom shared kernel package based on Ardalis.SharedKernel.

**FormDesignerAPI**  
Your project name (replaces FormGenAI from guide).

[Additional terms...]
EOF

echo "  ✓ Created APPENDIX-C-GLOSSARY.md"

echo ""
echo "✅ Documentation generation complete!"
echo ""
echo "📁 Files created in: $BASE_DIR"
echo ""
echo "📦 To create a ZIP file:"
echo "   zip -r FormDesignerAPI-Migration-Guide.zip $BASE_DIR"
echo ""
echo "🚀 Next steps:"
echo "   1. cd $BASE_DIR"
echo "   2. Review README.md"
echo "   3. Read 00-OVERVIEW.md"
echo "   4. Read 01-PHASE-1-SKIP.md"
echo "   5. START with 02-PHASE-2-FORM-DOMAIN.md"
echo ""
echo "⚡ Note: Phase 1 is skipped because you're using Traxs.SharedKernel!"
echo ""