# Overview: FormDesignerAPI Migration Guide

## Executive Summary

This migration guide transforms your existing Ardalis CleanArchitecture template into a comprehensive Domain-Driven Design (DDD) solution with AI-powered code generation capabilities.

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
| **Domain-Driven Design** | Clear business domain separation | High maintainability |
| **Three Bounded Contexts** | Independent evolution | Scalable architecture |
| **Ardalis CleanArchitecture** | Proven pattern | Reduced learning curve |
| **PostgreSQL + JSONB** | Flexible schema storage | Dynamic form support |
| **Event-Driven Communication** | Decoupled contexts | Loose coupling |
| **Claude Sonnet 4 API** | Best-in-class AI | High-quality generation |

## Prerequisites

### Required Software Versions
```bash
# Verify your environment
dotnet --version        # Must be 8.0+
node --version          # Must be 20.x+
docker --version        # Latest stable
git --version           # Latest stable
psql --version         # PostgreSQL 15+

# Install global tools
dotnet tool install --global dotnet-ef
dotnet ef --version    # Verify EF Core tools
```

### Environment Setup

**1. PostgreSQL Database (Using Docker):**
```bash
# Start PostgreSQL container
docker run --name FormDesignerAPI-db \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -e POSTGRES_DB=FormDesignerAPI \
  -p 5432:5432 \
  -v FormDesignerAPI-data:/var/lib/postgresql/data \
  -d postgres:15

# Verify it's running
docker ps | grep FormDesignerAPI-db
```

**2. Anthropic API Key:**

1. Navigate to https://console.anthropic.com/
2. Sign up or log in
3. Go to **Settings** → **API Keys**
4. Click **Create Key**
5. Copy the key (starts with `sk-ant-`)
6. Store it securely

**3. Development Environment:**

Choose one:
- Visual Studio 2022 (Community or higher)
- JetBrains Rider
- VS Code with C# extension

### Backup Strategy

#### Create backup OUTSIDE your project directory

```bash

# Create backup
mkdir -p ~/Development/Backups/FormDesignerAPI-Backups
# Option 1: Compressed archive (recommended)
tar -czf ~/Development/Backups/FormDesignerAPI-Backups/FormDesignerAPI-backup-$(date +%Y%m%d).tar.gz \
  --exclude='.git' \
  --exclude='node_modules' \
  --exclude='bin' \
  --exclude='obj' \
  .

# Option 2: Git tag (best for Git projects)
git tag -a v0.0.0-pre-migration -m "State before DDD migration"

# Create feature branch
git checkout -b feature/ddd-migration

# Commit current state
git add .
git commit -m "Checkpoint: Before DDD migration ($(date +%Y-%m-%d))"
```

## Architecture Overview

### Target State: DDD with Bounded Contexts

```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.Core/              ← Add Traxs.SharedKernel package
│   │   ├── FormContext/
│   │   │   ├── Aggregates/
│   │   │   │   └── Form.cs                ← Inherits EntityBase<Guid>
│   │   │   ├── ValueObjects/
│   │   │   │   └── FormDefinition.cs      ← Inherits ValueObject
│   │   │   ├── Events/
│   │   │   │   └── FormCreatedEvent.cs    ← Inherits DomainEventBase
│   │   │   └── Interfaces/
│   │   │       └── IFormRepository.cs     ← Inherits IRepository<Form>
│   │   ├── ImportContext/
│   │   └── CodeGenerationContext/
│   │
│   ├── FormDesignerAPI.Infrastructure/    ← Add Traxs.SharedKernel package
│   │   └── Repositories/
│   │       └── FormRepository.cs          ← Implements IFormRepository
│   │
│   ├── FormDesignerAPI.UseCases/          ← Add Traxs.SharedKernel package
│   └── FormDesignerAPI.Web/               ← Add Traxs.SharedKernel package
```


```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.Core/              # Phase 2 Skipping Phase 1
│   ├── FormDesignerAPI.UseCases/          # Phase 4
│   ├── FormDesignerAPI.Infrastructure/    # Phase 3
│   └── FormDesignerAPI.Web/               # Phase 4
└── tests/
    ├── FormDesignerAPI.UnitTests/
    ├── FormDesignerAPI.IntegrationTests/
    └── FormDesignerAPI.FunctionalTests/
```

### Bounded Context Map
```
┌──────────────────────┐
│   FORM CONTEXT       │
│  → FormCreated       │
└──────┬───────────────┘
       │
   ┌───┴────┬──────────┐
   │        │          │
   ▼        ▼          ▼
┌──────┐ ┌──────┐  ┌──────┐
│IMPORT│ │CODGEN│  │FUTURE│
└──────┘ └──────┘  └──────┘
```

## Implementation Timeline

| Week | Phase | Focus |
|------|-------|-------|
| 1 | 1-2 | Domain |
| 2 | 3-4 | Infrastructure + API |
| 3-4 | 5-6 | AI Integration |
| 5-7 | 7 | Code Generation |
| 7-8 | 8-9 | Integration + Testing |

## Success Criteria

- [ ] Can upload PDF and extract form
- [ ] Can generate working application code
- [ ] Generated code compiles
- [ ] All tests pass
- [ ] Documentation complete

## Next Steps

Skipping Phase 1. Proceed to **[02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)** to begin.

---

**Document Version:** 1.0.0  
**Last Updated:** December 2024
