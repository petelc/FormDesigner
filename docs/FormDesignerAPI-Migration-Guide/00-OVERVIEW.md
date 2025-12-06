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
```bash
# Create feature branch
git checkout -b feature/ddd-migration

# Create backup
mkdir -p ../FormDesignerAPI-Backups
cp -r . ../FormDesignerAPI-Backups/FormDesignerAPI-$(date +%Y%m%d)

# Commit current state
git add .
git commit -m "Checkpoint: Before DDD migration"
```

## Architecture Overview

### Target State: DDD with Bounded Contexts
```
FormDesignerAPI/
├── SharedKernel/           # Phase 1
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
| 1 | 1-2 | Foundation + Domain |
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

Proceed to **[01-PHASE-1-FOUNDATION.md](01-PHASE-1-FOUNDATION.md)** to begin.

---

**Document Version:** 1.0.0  
**Last Updated:** December 2024
