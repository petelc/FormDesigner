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

[Include all the value object code from our conversation - OriginType, OriginMetadata, FormField, FormDefinition]

---

## Step 4: Create Domain Events

[Include all the domain event code - FormCreatedEvent, FormRevisionCreatedEvent, FormRenamedEvent]

---

## Step 5: Create Aggregate Root and Entities

[Include FormRevision and Form aggregate code]

---

## Step 6: Create Repository Interface

[Include IFormRepository code]

---

## Step 7: Create Specifications

[Include specification code]

---

## Step 8: Create Unit Tests

[Include unit test code]

---

## Verification Checklist

- [ ] All code compiles
- [ ] Unit tests pass
- [ ] No infrastructure dependencies
- [ ] Domain events raised correctly

---

## Git Commit
```bash
git add .
git commit -m "Phase 2: Implemented Form Context domain model with Traxs.SharedKernel"
```

---

**Phase 2 Complete!** ✅

Proceed to Phase 3: Infrastructure
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