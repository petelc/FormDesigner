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
