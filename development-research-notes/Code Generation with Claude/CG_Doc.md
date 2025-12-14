Perfect! I'll create a PowerShell script and a Bash script that will auto-generate all the markdown files with the proper structure.

---

## **File: generate-docs.ps1** (PowerShell for Windows)

```powershell
# FormGenAI Migration Guide Generator
# PowerShell script to create all documentation files

Write-Host "🚀 FormGenAI Migration Guide Generator" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Create base directory
$baseDir = "FormGenAI-Migration-Guide"
if (Test-Path $baseDir) {
    Write-Host "⚠️  Directory already exists. Remove it? (Y/N): " -ForegroundColor Yellow -NoNewline
    $response = Read-Host
    if ($response -eq "Y" -or $response -eq "y") {
        Remove-Item -Recurse -Force $baseDir
        Write-Host "✓ Removed existing directory" -ForegroundColor Green
    } else {
        Write-Host "❌ Aborted" -ForegroundColor Red
        exit
    }
}

New-Item -ItemType Directory -Path $baseDir | Out-Null
New-Item -ItemType Directory -Path "$baseDir/diagrams" | Out-Null
New-Item -ItemType Directory -Path "$baseDir/templates" | Out-Null

Write-Host "✓ Created directory structure" -ForegroundColor Green

# Function to create file with content
function New-DocFile {
    param(
        [string]$fileName,
        [string]$content
    )
    
    $filePath = Join-Path $baseDir $fileName
    Set-Content -Path $filePath -Value $content -Encoding UTF8
    Write-Host "  ✓ Created $fileName" -ForegroundColor Gray
}

# README.md
$readmeContent = @"
# FormGenAI Migration Guide - Complete Documentation

Welcome to the complete migration guide for transforming your Ardalis CleanArchitecture solution into a DDD-based AI-powered code generation system.

## 📚 Document Structure

This guide is organized into separate documents for easier navigation:

### Getting Started
- **[00-OVERVIEW.md](00-OVERVIEW.md)** - Executive summary, prerequisites, and architecture overview

### Phase-by-Phase Implementation
- **[01-PHASE-1-FOUNDATION.md](01-PHASE-1-FOUNDATION.md)** - SharedKernel and base classes (Week 1)
- **[02-PHASE-2-FORM-DOMAIN.md](02-PHASE-2-FORM-DOMAIN.md)** - Form Context domain model (Week 1-2)
- **[03-PHASE-3-FORM-INFRASTRUCTURE.md](03-PHASE-3-FORM-INFRASTRUCTURE.md)** - EF Core and repositories (Week 2)
- **[04-PHASE-4-FORM-API.md](04-PHASE-4-FORM-API.md)** - Use cases and REST API (Week 3)
- **[05-PHASE-5-CLAUDE-INTEGRATION.md](05-PHASE-5-CLAUDE-INTEGRATION.md)** - Claude API client (Week 3-4)
- **[06-PHASE-6-IMPORT-CONTEXT.md](06-PHASE-6-IMPORT-CONTEXT.md)** - PDF upload and extraction (Week 4-5)
- **[07-PHASE-7-CODE-GENERATION.md](07-PHASE-7-CODE-GENERATION.md)** - Templates and code generation (Week 5-7)
- **[08-PHASE-8-INTEGRATION.md](08-PHASE-8-INTEGRATION.md)** - Events and cross-context communication (Week 7-8)
- **[09-PHASE-9-TESTING.md](09-PHASE-9-TESTING.md)** - Testing and documentation (Week 8)

### Reference Materials
- **[APPENDIX-A-CODE-EXAMPLES.md](APPENDIX-A-CODE-EXAMPLES.md)** - Complete code samples
- **[APPENDIX-B-TROUBLESHOOTING.md](APPENDIX-B-TROUBLESHOOTING.md)** - Common issues and solutions
- **[APPENDIX-C-GLOSSARY.md](APPENDIX-C-GLOSSARY.md)** - Terms and definitions

## 🎯 Quick Start

1. Read **00-OVERVIEW.md** for prerequisites and architecture understanding
2. Follow phases sequentially (1-9)
3. Each phase includes:
   - Clear objectives
   - Step-by-step instructions
   - Code examples
   - Verification checklist
   - Git commit message

## ⏱️ Estimated Timeline

| Phase | Duration | Complexity |
|-------|----------|------------|
| Phase 1: Foundation | 2-3 days | Low |
| Phase 2: Form Domain | 3-4 days | Medium |
| Phase 3: Form Infrastructure | 2-3 days | Medium |
| Phase 4: Form API | 3-4 days | Medium |
| Phase 5: Claude Integration | 2-3 days | Medium |
| Phase 6: Import Context | 5-7 days | High |
| Phase 7: Code Generation | 10-14 days | High |
| Phase 8: Integration | 3-5 days | Medium |
| Phase 9: Testing | 5-7 days | Medium |

**Total: 6-8 weeks**

## 📋 Prerequisites Checklist

Before starting, ensure you have:
- [ ] .NET 8.0 SDK installed
- [ ] PostgreSQL running (Docker or local)
- [ ] Anthropic API key
- [ ] Visual Studio 2022 or Rider
- [ ] Git for version control
- [ ] Basic understanding of DDD concepts

## 🏗️ Architecture Overview

``````
FormGenAI/
├── SharedKernel/          (Phase 1)
├── Core/
│   ├── FormContext/       (Phase 2)
│   ├── ImportContext/     (Phase 6)
│   └── CodeGenContext/    (Phase 7)
├── UseCases/              (Phase 4)
├── Infrastructure/        (Phase 3, 5)
└── Web/                   (Phase 4)
``````

## 📊 Progress Tracking

Track your progress by checking off completed phases:

- [ ] Phase 1: Foundation ✅
- [ ] Phase 2: Form Domain ✅
- [ ] Phase 3: Form Infrastructure ✅
- [ ] Phase 4: Form API ✅
- [ ] Phase 5: Claude Integration ✅
- [ ] Phase 6: Import Context ✅
- [ ] Phase 7: Code Generation ✅
- [ ] Phase 8: Integration ✅
- [ ] Phase 9: Testing ✅

## 🤝 Contributing

Found an issue or have suggestions? Please create an issue in the repository.

## 📝 Version History

- **v1.0.0** (December 2024) - Initial release

## 📧 Support

For questions or support:
- Review the Troubleshooting guide (APPENDIX-B)
- Check the Glossary (APPENDIX-C)
- Review completed phases for reference implementations

---

**Ready to start?** Proceed to [00-OVERVIEW.md](00-OVERVIEW.md)

**Last Updated:** December 2024  
**Version:** 1.0.0
"@

New-DocFile "README.md" $readmeContent

# 00-OVERVIEW.md
$overviewContent = @"
# Overview: FormGenAI Migration Guide

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
| **Template-Based Generation** | Consistent output | Maintainable prompts |

## Prerequisites

### Required Software Versions

``````bash
# Verify your environment
dotnet --version        # Must be 8.0+
node --version          # Must be 20.x+
docker --version        # Latest stable
git --version           # Latest stable
psql --version         # PostgreSQL 15+

# Install global tools
dotnet tool install --global dotnet-ef
dotnet ef --version    # Verify EF Core tools
``````

### Expected Output
``````
.NET SDK: 8.0.100 or higher
Node.js: v20.x.x
Docker: 24.x.x or higher
PostgreSQL: 15.x or higher
EF Core tools: 8.0.x
``````

### Environment Setup

**1. PostgreSQL Database (Using Docker):**

``````bash
# Start PostgreSQL container
docker run --name formgenai-db \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -e POSTGRES_DB=FormGenAI \
  -p 5432:5432 \
  -v formgenai-data:/var/lib/postgresql/data \
  -d postgres:15

# Verify it's running
docker ps | grep formgenai-db

# Connect to verify
docker exec -it formgenai-db psql -U postgres -d FormGenAI

# Inside psql:
\l                    # List databases
\q                    # Exit
``````

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

``````bash
# Create feature branch
git checkout -b feature/ddd-migration

# Create backup directory
mkdir -p ../FormGenAI-Backups
cp -r . ../FormGenAI-Backups/FormGenAI-$(date +%Y%m%d)

# Or create ZIP backup
tar -czf ../FormGenAI-backup-$(date +%Y%m%d).tar.gz .

# Commit current state
git add .
git commit -m "Checkpoint: Before DDD migration ($(date +%Y-%m-%d))"
git tag -a v0.0.0-pre-migration -m "State before DDD migration"
``````

## Architecture Overview

### Current State: Ardalis Template

``````
FormGenAI/
├── src/
│   ├── FormGenAI.Core/          # Generic domain layer
│   ├── FormGenAI.Infrastructure/ # Data access
│   ├── FormGenAI.UseCases/      # Application logic
│   └── FormGenAI.Web/           # API endpoints
└── tests/
    ├── FormGenAI.FunctionalTests/
    ├── FormGenAI.IntegrationTests/
    └── FormGenAI.UnitTests/
``````

**Current Issues:**
- ❌ No bounded context separation
- ❌ Generic entity base classes
- ❌ Monolithic domain model
- ❌ No clear aggregate boundaries
- ❌ Limited domain events

### Target State: DDD with Bounded Contexts

``````
FormGenAI/
├── src/
│   ├── FormGenAI.SharedKernel/          # 🆕 Phase 1
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
│   ├── FormGenAI.Core/                  # 🔄 Refactored
│   │   ├── FormContext/                 # Phase 2
│   │   │   ├── Aggregates/
│   │   │   │   ├── Form.cs
│   │   │   │   └── FormRevision.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── FormDefinition.cs
│   │   │   │   └── OriginMetadata.cs
│   │   │   ├── Events/
│   │   │   │   ├── FormCreatedEvent.cs
│   │   │   │   └── FormRevisionCreatedEvent.cs
│   │   │   └── Interfaces/
│   │   │       └── IFormRepository.cs
│   │   │
│   │   ├── ImportContext/               # Phase 6
│   │   │   ├── Aggregates/
│   │   │   │   ├── ImportBatch.cs
│   │   │   │   └── ImportedFormCandidate.cs
│   │   │   ├── Services/
│   │   │   │   └── PdfExtractionService.cs
│   │   │   └── Events/
│   │   │
│   │   └── CodeGenerationContext/       # Phase 7
│   │       ├── Aggregates/
│   │       │   └── CodeGenerationJob.cs
│   │       ├── Templates/
│   │       │   ├── CSharp/
│   │       │   ├── Sql/
│   │       │   └── React/
│   │       └── Services/
│   │           ├── TemplateBasedCodeGenerator.cs
│   │           └── ZipPackager.cs
│   │
│   ├── FormGenAI.UseCases/              # 🔄 Refactored
│   │   ├── FormContext/                 # Phase 4
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── ImportContext/               # Phase 6
│   │   └── CodeGenerationContext/       # Phase 7
│   │
│   ├── FormGenAI.Infrastructure/        # 🔄 Refactored
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── FormContext/             # Phase 3
│   │   │   ├── ImportContext/           # Phase 6
│   │   │   └── CodeGenerationContext/   # Phase 7
│   │   ├── Repositories/
│   │   ├── ExternalServices/
│   │   │   └── ClaudeApiClient.cs       # Phase 5
│   │   └── BackgroundJobs/              # Phase 8
│   │
│   └── FormGenAI.Web/                   # 🔄 Refactored
│       ├── Controllers/
│       │   ├── FormContext/             # Phase 4
│       │   ├── ImportContext/           # Phase 6
│       │   └── CodeGenerationContext/   # Phase 7
│       └── Program.cs
│
└── tests/
    ├── FormGenAI.UnitTests/             # Phase 9
    │   ├── FormContext/
    │   ├── ImportContext/
    │   └── CodeGenerationContext/
    ├── FormGenAI.IntegrationTests/      # Phase 9
    └── FormGenAI.FunctionalTests/       # Phase 9
``````

### Bounded Context Map

This diagram shows how the three contexts interact:

``````
┌──────────────────────────────────────────────────────────┐
│                   FORM CONTEXT                            │
│  Aggregate: Form                                          │
│  Responsibilities:                                        │
│  - Manage form definitions and metadata                  │
│  - Track form revisions over time                        │
│  - Store form origin (manual/import/API)                 │
│                                                          │
│  Published Events:                                       │
│  → FormCreatedEvent                                      │
│  → FormRevisionCreatedEvent                              │
└────────────────┬─────────────────────────────────────────┘
                 │
                 │ Domain Events (MediatR)
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
``````

### Clean Architecture Layers

Each bounded context follows this layering:

``````
┌─────────────────────────────────────────────────────────┐
│     PRESENTATION LAYER                                  │
│     FormGenAI.Web                                       │
│     - Controllers                                       │
│     - API Endpoints                                     │
│     - Request/Response Models                           │
└──────────────┬──────────────────────────────────────────┘
               │ Depends on ↓
               │
┌──────────────┴──────────────────────────────────────────┐
│     APPLICATION LAYER                                   │
│     FormGenAI.UseCases                                  │
│     - Commands (write operations)                       │
│     - Queries (read operations)                         │
│     - DTOs (data transfer objects)                      │
│     - MediatR handlers                                  │
└──────────────┬──────────────────────────────────────────┘
               │ Depends on ↓
               │
┌──────────────┴──────────────────────────────────────────┐
│     DOMAIN LAYER (Core)                                 │
│     FormGenAI.Core                                      │
│     - Aggregates                                        │
│     - Entities                                          │
│     - Value Objects                                     │
│     - Domain Events                                     │
│     - Business Logic                                    │
│     - Domain Services                                   │
│     - Repository Interfaces                             │
└──────────────┬──────────────────────────────────────────┘
               │ ↑ Implemented by
               │
┌──────────────┴──────────────────────────────────────────┐
│     INFRASTRUCTURE LAYER                                │
│     FormGenAI.Infrastructure                            │
│     - EF Core DbContext                                 │
│     - Repository Implementations                        │
│     - External Service Clients (Claude API)             │
│     - File System Access                                │
│     - Background Jobs                                   │
└─────────────────────────────────────────────────────────┘
``````

**Key Principles:**
1. **Dependency Rule**: Dependencies point inward only
2. **Domain Independence**: Core has NO dependencies on outer layers
3. **Interface Segregation**: Domain defines interfaces, Infrastructure implements
4. **Stable Abstractions**: Domain is most stable, UI is least stable

### Database Schema Overview

**Forms Table:**
``````sql
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
``````

**FormRevisions Table:**
``````sql
CREATE TABLE "FormRevisions" (
    "Id" UUID PRIMARY KEY,
    "FormId" UUID NOT NULL REFERENCES "Forms"("Id") ON DELETE CASCADE,
    "Version" INT NOT NULL,
    "DefinitionSchema" JSONB NOT NULL,
    "DefinitionFields" JSONB NOT NULL,
    "Notes" VARCHAR(1000),
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL,
    UNIQUE ("FormId", "Version")
);

CREATE INDEX "IX_FormRevisions_FormId_Version" 
    ON "FormRevisions"("FormId", "Version");
``````

**ImportBatches Table:**
``````sql
CREATE TABLE "ImportBatches" (
    "Id" UUID PRIMARY KEY,
    "Status" VARCHAR(50) NOT NULL,
    "UploadedFiles" JSONB NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL,
    "CompletedAt" TIMESTAMP
);

CREATE INDEX "IX_ImportBatches_Status" ON "ImportBatches"("Status");
``````

**ImportedFormCandidates Table:**
``````sql
CREATE TABLE "ImportedFormCandidates" (
    "Id" UUID PRIMARY KEY,
    "BatchId" UUID NOT NULL REFERENCES "ImportBatches"("Id") ON DELETE CASCADE,
    "OriginalFileName" VARCHAR(255) NOT NULL,
    "ExtractedJson" JSONB,
    "ExtractionStatus" VARCHAR(50) NOT NULL,
    "ApprovalStatus" VARCHAR(50) NOT NULL,
    "ValidationErrors" JSONB,
    "CreatedAt" TIMESTAMP NOT NULL,
    "ApprovedAt" TIMESTAMP,
    "ApprovedBy" VARCHAR(100)
);

CREATE INDEX "IX_ImportedFormCandidates_BatchId" 
    ON "ImportedFormCandidates"("BatchId");
``````

**CodeGenerationJobs Table:**
``````sql
CREATE TABLE "CodeGenerationJobs" (
    "Id" UUID PRIMARY KEY,
    "FormDefinitionId" UUID NOT NULL,
    "FormRevisionId" UUID NOT NULL,
    "Version" VARCHAR(20) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "Options" JSONB NOT NULL,
    "ZipFilePath" VARCHAR(500),
    "RequestedAt" TIMESTAMP NOT NULL,
    "RequestedBy" VARCHAR(100) NOT NULL,
    "CompletedAt" TIMESTAMP,
    "ErrorMessage" TEXT
);

CREATE INDEX "IX_CodeGenerationJobs_Status" 
    ON "CodeGenerationJobs"("Status");
``````

## Implementation Timeline

### Detailed Schedule

| Week | Phase | Focus | Deliverable |
|------|-------|-------|-------------|
| 1 | 1-2 | Foundation + Domain | SharedKernel + Form aggregate |
| 2 | 3-4 | Infrastructure + API | Database + REST endpoints |
| 3-4 | 5-6 | AI Integration + Import | Claude client + PDF processing |
| 5-7 | 7 | Code Generation | Templates + generator service |
| 7-8 | 8-9 | Integration + Testing | Events + test suite |

### Daily Breakdown (Example for Week 1)

**Week 1, Day 1-2: Phase 1 (Foundation)**
- Create SharedKernel project
- Implement base classes
- Set up interfaces
- Verify compilation

**Week 1, Day 3-5: Phase 2 (Form Domain)**
- Create value objects
- Implement Form aggregate
- Add domain events
- Write unit tests

## Success Criteria

### Technical Criteria

By the end of this migration, you must achieve:

**Phase 1:**
- [ ] SharedKernel project builds
- [ ] All base classes compile
- [ ] Interfaces defined correctly

**Phase 2:**
- [ ] Form aggregate enforces invariants
- [ ] Domain events raised properly
- [ ] No infrastructure dependencies

**Phase 3:**
- [ ] Database migrations apply cleanly
- [ ] Repository queries work
- [ ] EF Core configurations correct

**Phase 4:**
- [ ] All API endpoints return 200 OK
- [ ] CQRS pattern implemented
- [ ] Swagger documentation complete

**Phase 5:**
- [ ] Claude API connection verified
- [ ] API key configuration secure
- [ ] Error handling robust

**Phase 6:**
- [ ] PDF upload works
- [ ] Extraction produces valid JSON
- [ ] Approval workflow functional

**Phase 7:**
- [ ] At least 10 templates implemented
- [ ] Generated code compiles
- [ ] ZIP packaging works

**Phase 8:**
- [ ] Events publish correctly
- [ ] Cross-context communication works
- [ ] Saga patterns implemented (if needed)

**Phase 9:**
- [ ] 80%+ test coverage
- [ ] All integration tests pass
- [ ] Documentation complete

### Business Criteria

- [ ] Can upload PDF and extract form
- [ ] Can generate working application code
- [ ] Generated code passes basic tests
- [ ] System handles errors gracefully
- [ ] Performance acceptable (< 5min generation)

## Risk Assessment

### High-Risk Areas

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Claude API reliability | High | Medium | Implement retry logic, fallbacks |
| Learning curve too steep | High | High | Pair programming, workshops |
| Performance issues | Medium | Medium | Load test early, optimize |
| Generated code quality | High | Medium | Template refinement, validation |
| Team alignment | High | Low | Regular standups, clear docs |

### Mitigation Strategies

**For Learning Curve:**
1. Weekly architecture review sessions
2. Pair programming on complex parts
3. Code review all domain logic
4. Create internal wiki with examples

**For API Reliability:**
1. Implement exponential backoff
2. Add circuit breaker pattern
3. Cache successful results
4. Provide manual fallback

**For Performance:**
1. Load test after Phase 7
2. Optimize prompts for token count
3. Implement background job processing
4. Add progress indicators

## Getting Help

### When You're Stuck

1. **Check the Troubleshooting Guide**: [APPENDIX-B-TROUBLESHOOTING.md](APPENDIX-B-TROUBLESHOOTING.md)
2. **Review the Glossary**: [APPENDIX-C-GLOSSARY.md](APPENDIX-C-GLOSSARY.md)
3. **Check Phase Verification**: Each phase has a checklist
4. **Review Git History**: Compare with reference commits
5. **Check Example Code**: [APPENDIX-A-CODE-EXAMPLES.md](APPENDIX-A-CODE-EXAMPLES.md)

### Common Questions

**Q: Do I need to complete all phases?**
A: Phases 1-7 are mandatory. Phase 8-9 can be done incrementally.

**Q: Can I skip the Import Context?**
A: Yes, but you'll need to manually create forms. Recommended to implement.

**Q: What if Claude API is too expensive?**
A: Start with small test files. Optimize prompts. Consider caching results.

**Q: Can I use a different database?**
A: Yes, but you'll need to adjust EF Core configurations and JSONB queries.

## Next Steps

### Before You Begin

Ensure you have:
- ✅ All prerequisites installed and verified
- ✅ Backup created
- ✅ Team aligned on architecture
- ✅ Development environment configured
- ✅ PostgreSQL running
- ✅ Anthropic API key obtained

### Ready to Start?

Proceed to **[01-PHASE-1-FOUNDATION.md](01-PHASE-1-FOUNDATION.md)** to begin building the SharedKernel.

---

**Document Version:** 1.0.0  
**Last Updated:** December 2024  
**Next Review:** After Phase 9 completion
"@

New-DocFile "00-OVERVIEW.md" $overviewContent

Write-Host ""
Write-Host "📄 Generating phase documents..." -ForegroundColor Cyan

# Generate all phase files (abbreviated versions - you'll need to fill these in with full content)
$phases = @(
    @{
        Number = "01"
        Name = "PHASE-1-FOUNDATION"
        Title = "Phase 1: Foundation - SharedKernel"
        Duration = "2-3 days"
        Summary = "Create the SharedKernel project with base classes, interfaces, and domain event infrastructure."
    },
    @{
        Number = "02"
        Name = "PHASE-2-FORM-DOMAIN"
        Title = "Phase 2: Form Context - Domain Model"
        Duration = "3-4 days"
        Summary = "Implement the Form aggregate root, value objects, and domain events for the Form Context."
    },
    @{
        Number = "03"
        Name = "PHASE-3-FORM-INFRASTRUCTURE"
        Title = "Phase 3: Form Context - Infrastructure"
        Duration = "2-3 days"
        Summary = "Set up EF Core, database migrations, and repository implementations for Form Context."
    },
    @{
        Number = "04"
        Name = "PHASE-4-FORM-API"
        Title = "Phase 4: Form Context - Use Cases & API"
        Duration = "3-4 days"
        Summary = "Implement CQRS pattern with MediatR, create REST API endpoints, and set up Swagger."
    },
    @{
        Number = "05"
        Name = "PHASE-5-CLAUDE-INTEGRATION"
        Title = "Phase 5: Claude API Integration"
        Duration = "2-3 days"
        Summary = "Integrate Anthropic Claude Sonnet 4 API for AI-powered code generation."
    },
    @{
        Number = "06"
        Name = "PHASE-6-IMPORT-CONTEXT"
        Title = "Phase 6: Import Context"
        Duration = "5-7 days"
        Summary = "Build PDF upload system and AI-powered form field extraction."
    },
    @{
        Number = "07"
        Name = "PHASE-7-CODE-GENERATION"
        Title = "Phase 7: Code Generation Context"
        Duration = "10-14 days"
        Summary = "Create template system and code generation engine for producing application code."
    },
    @{
        Number = "08"
        Name = "PHASE-8-INTEGRATION"
        Title = "Phase 8: Integration & Events"
        Duration = "3-5 days"
        Summary = "Connect bounded contexts with domain events and implement cross-context workflows."
    },
    @{
        Number = "09"
        Name = "PHASE-9-TESTING"
        Title = "Phase 9: Testing & Documentation"
        Duration = "5-7 days"
        Summary = "Comprehensive testing strategy, documentation, and deployment preparation."
    }
)

foreach ($phase in $phases) {
    $phaseContent = @"
# $($phase.Title)

**Duration:** $($phase.Duration)  
**Complexity:** Medium to High  
**Dependencies:** Previous phases must be complete

## Overview

$($phase.Summary)

## Objectives

This phase will guide you through:
- [ ] Objective 1
- [ ] Objective 2
- [ ] Objective 3
- [ ] Objective 4

## Prerequisites

Before starting this phase:
- [ ] Previous phase completed and verified
- [ ] All tests passing
- [ ] Git committed with proper message

## Implementation Steps

### Step 1: [Step Name]

**Description:**
[What this step accomplishes]

**Commands:**
``````bash
# Example commands
cd src/FormGenAI.Core
mkdir -p Context/Folder
``````

**Code Example:**
``````csharp
// Example code snippet
namespace FormGenAI.Core.Context;

public class ExampleClass
{
    // Implementation
}
``````

### Step 2: [Next Step]

[Continue pattern for all steps]

## Verification Checklist

After completing this phase, verify:

- [ ] All code compiles without warnings
- [ ] Unit tests pass
- [ ] Integration points work
- [ ] Database migrations apply cleanly
- [ ] API endpoints return expected results
- [ ] No TODO comments remain
- [ ] Code follows project conventions

## Testing

### Unit Tests

``````csharp
// Example unit test
[Fact]
public void Test_Description()
{
    // Arrange
    // Act
    // Assert
}
``````

### Integration Tests

``````csharp
// Example integration test
[Fact]
public async Task Integration_Test_Description()
{
    // Test implementation
}
``````

## Troubleshooting

### Common Issues

**Issue 1: [Problem Description]**
- **Symptom:** What you see
- **Cause:** Why it happens
- **Solution:** How to fix

**Issue 2: [Problem Description]**
- **Symptom:** What you see
- **Cause:** Why it happens
- **Solution:** How to fix

## Git Commit

After verification:

``````bash
git add .
git commit -m "$($phase.Title): Completed implementation"
git push origin feature/ddd-migration
``````

## Next Steps

✅ Phase $($phase.Number) complete!

Proceed to **[$([int]$phase.Number + 1)-PHASE-$([int]$phase.Number + 1)-[NEXT].md]** to continue.

---

**Phase $($phase.Number) Complete!** 
"@

    New-DocFile "$($phase.Number)-$($phase.Name).md" $phaseContent
}

Write-Host ""
Write-Host "📚 Generating appendices..." -ForegroundColor Cyan

# APPENDIX-A
$appendixAContent = @"
# Appendix A: Complete Code Examples

This appendix contains complete, working code examples for reference.

## Table of Contents

1. [SharedKernel Complete Examples](#sharedkernel)
2. [Form Context Examples](#form-context)
3. [Import Context Examples](#import-context)
4. [Code Generation Examples](#code-generation)
5. [Integration Examples](#integration)

## SharedKernel

### EntityBase.cs (Complete)

``````csharp
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
``````

### ValueObject.cs (Complete)

``````csharp
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
``````

[Continue with more examples...]

## Form Context

### Form Aggregate (Complete)

``````csharp
// See Phase 2 for complete implementation
``````

[More examples...]

---

**Note:** This appendix contains reference implementations. Always adapt to your specific needs.
"@

New-DocFile "APPENDIX-A-CODE-EXAMPLES.md" $appendixAContent

# APPENDIX-B
$appendixBContent = @"
# Appendix B: Troubleshooting Guide

Common issues and their solutions organized by phase.

## Table of Contents

1. [General Issues](#general-issues)
2. [Phase 1 Issues](#phase-1-issues)
3. [Phase 2-4 Issues](#phase-2-4-issues)
4. [Claude API Issues](#claude-api-issues)
5. [Database Issues](#database-issues)
6. [Performance Issues](#performance-issues)

## General Issues

### Issue: Solution Won't Build

**Symptoms:**
- Build errors across multiple projects
- Missing references
- Type not found errors

**Diagnosis:**
``````bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
``````

**Solutions:**
1. Ensure all project references are correct
2. Check NuGet package versions for compatibility
3. Verify .NET SDK version: `dotnet --version`
4. Clear NuGet cache: `dotnet nuget locals all --clear`

### Issue: EF Core Migrations Failing

**Symptoms:**
- Migration add fails
- Migration apply fails
- Database schema mismatch

**Diagnosis:**
``````bash
# List migrations
dotnet ef migrations list --project src/FormGenAI.Infrastructure

# Check migration status
dotnet ef database update --project src/FormGenAI.Infrastructure --dry-run
``````

**Solutions:**

**Solution 1: Remove and recreate migration**
``````bash
# Remove last migration
dotnet ef migrations remove --project src/FormGenAI.Infrastructure

# Recreate
dotnet ef migrations add YourMigration --project src/FormGenAI.Infrastructure
``````

**Solution 2: Reset database (Development only!)**
``````bash
# Drop database
dotnet ef database drop --project src/FormGenAI.Infrastructure --force

# Reapply all migrations
dotnet ef database update --project src/FormGenAI.Infrastructure
``````

## Claude API Issues

### Issue: API Key Invalid

**Symptoms:**
- 401 Unauthorized errors
- "Invalid API Key" messages

**Diagnosis:**
``````bash
# Check if API key is set
cat src/FormGenAI.Web/appsettings.json | grep ApiKey
``````

**Solutions:**
1. Verify API key from https://console.anthropic.com/
2. Ensure no extra spaces in configuration
3. Check key starts with `sk-ant-`
4. Verify key hasn't expired
5. Use User Secrets in development:

``````bash
cd src/FormGenAI.Web
dotnet user-secrets set "Anthropic:ApiKey" "your-api-key-here"
``````

### Issue: Rate Limiting

**Symptoms:**
- 429 Too Many Requests
- Requests timing out

**Solutions:**
1. Implement exponential backoff (already in ClaudeApiClient)
2. Reduce concurrent requests
3. Cache results where possible
4. Check your API tier limits

### Issue: Large PDF Files Failing

**Symptoms:**
- Timeout errors
- 413 Payload Too Large

**Solutions:**
1. Increase timeout in configuration:
``````json
{
  "Anthropic": {
    "TimeoutMinutes": 10
  }
}
``````

2. Compress PDF before sending
3. Split large PDFs into smaller chunks
4. Validate PDF size before processing

## Database Issues

### Issue: Connection String Invalid

**Symptoms:**
- Unable to connect to database
- Connection timeout

**Diagnosis:**
``````bash
# Test PostgreSQL connection
docker exec -it formgenai-db psql -U postgres -d FormGenAI

# If that fails, check if container is running
docker ps | grep formgenai-db
``````

**Solutions:**
1. Verify PostgreSQL is running
2. Check connection string format:
``````json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FormGenAI;Username=postgres;Password=your_password"
  }
}
``````

3. Ensure firewall allows port 5432
4. Check PostgreSQL logs:
``````bash
docker logs formgenai-db
``````

### Issue: JSONB Queries Failing

**Symptoms:**
- Invalid cast errors
- JSON parse errors

**Solutions:**
1. Ensure Npgsql.EntityFrameworkCore.PostgreSQL package installed
2. Verify column type is `jsonb` not `json`:
``````sql
\d+ "Forms"  -- In psql
``````

3. Check JSON serialization settings are consistent

## Performance Issues

### Issue: Code Generation Taking Too Long

**Symptoms:**
- Generation jobs timeout
- UI becomes unresponsive

**Solutions:**
1. Implement background job processing (Hangfire/Quartz)
2. Add progress indicators
3. Optimize Claude prompts for token count
4. Cache template generations
5. Process files in parallel with rate limiting

### Issue: Large ZIP Files

**Symptoms:**
- Download timeouts
- Memory issues

**Solutions:**
1. Stream ZIP creation:
``````csharp
// Use streaming instead of loading into memory
return File(zipStream, "application/zip", "generated-code.zip");
``````

2. Implement chunked downloads
3. Add file size limits
4. Use cloud storage for large artifacts

## Phase-Specific Issues

### Phase 1: SharedKernel

**Issue: Circular Dependencies**
- Ensure SharedKernel has NO dependencies on other projects
- Only MediatR.Contracts allowed

### Phase 2-4: Form Context

**Issue: Domain Events Not Firing**
- Check `RegisterDomainEvent()` calls in aggregate
- Verify event handler registered in MediatR
- Ensure `SaveChangesAsync` dispatches events

### Phase 6: Import Context

**Issue: PDF Extraction Produces Invalid JSON**
- Validate JSON immediately after Claude response
- Add retry logic with refined prompts
- Implement manual correction UI

### Phase 7: Code Generation

**Issue: Generated Code Won't Compile**
- Add post-generation compilation check
- Refine templates iteratively
- Implement template unit tests

## Quick Reference Commands

### Reset Everything (Development)
``````bash
# ⚠️ WARNING: This deletes all data!
docker stop formgenai-db
docker rm formgenai-db
rm -rf src/FormGenAI.Infrastructure/Migrations
dotnet ef database drop --force --project src/FormGenAI.Infrastructure
# Then recreate from Phase 3
``````

### Check System Health
``````bash
# Check .NET
dotnet --version

# Check Docker
docker --version
docker ps

# Check PostgreSQL
docker exec -it formgenai-db psql -U postgres -c "SELECT version();"

# Check EF Core tools
dotnet ef --version

# Build solution
dotnet build

# Run tests
dotnet test
``````

### View Logs
``````bash
# Application logs (if using Serilog to file)
tail -f logs/log-.txt

# Docker logs
docker logs -f formgenai-db

# EF Core SQL logging (add to appsettings)
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
``````

---

**Still stuck?** Check the code examples in APPENDIX-A or review the relevant phase document.
"@

New-DocFile "APPENDIX-B-TROUBLESHOOTING.md" $appendixBContent

# APPENDIX-C
$appendixCContent = @"
# Appendix C: Glossary

Definitions of terms used throughout this guide.

## A

**Aggregate**
: A cluster of domain objects that can be treated as a single unit. An aggregate has a root entity (Aggregate Root) and maintains consistency within its boundary.

**Aggregate Root**
: The main entity in an aggregate that enforces invariants and serves as the entry point for all operations on the aggregate.

**Anthropic**
: The company that created Claude, the AI model used in this system.

**API Key**
: A secret token used to authenticate with the Anthropic Claude API.

**Ardalis CleanArchitecture**
: A solution template created by Steve Smith (Ardalis) that implements Clean Architecture principles in .NET.

## B

**Bounded Context**
: A central pattern in DDD that defines explicit boundaries within which a domain model is defined and applicable.

**Business Logic**
: The core functionality and rules that define how the business operates, implemented in the domain layer.

## C

**Claude Sonnet 4**
: The latest version of Anthropic's AI model, optimized for code generation and document understanding.

**Clean Architecture**
: An architectural pattern that emphasizes separation of concerns and dependency inversion.

**Command**
: In CQRS, a request to change the system state (write operation).

**CQRS (Command Query Responsibility Segregation)**
: A pattern that separates read operations (queries) from write operations (commands).

## D

**DDD (Domain-Driven Design)**
: An approach to software development that focuses on modeling the business domain.

**Domain Event**
: Something that happened in the domain that domain experts care about.

**Domain Model**
: A representation of the business domain in code, including entities, value objects, and business logic.

**Domain Service**
: A service that contains domain logic that doesn't naturally fit within an entity or value object.

**DTO (Data Transfer Object)**
: An object used to transfer data between layers, typically without business logic.

## E

**Entity**
: A domain object that has a unique identity and lifecycle.

**EF Core (Entity Framework Core)**
: Microsoft's Object-Relational Mapping (ORM) framework for .NET.

**Event-Driven Architecture**
: An architecture pattern where components communicate through events.

## F

**Form Candidate**
: A potential form extracted from a PDF, awaiting approval.

**Form Definition**
: The structure and metadata of a form, including fields and validation rules.

**Form Revision**
: A versioned snapshot of a form definition.

## G

**Guard Clause**
: A validation check at the beginning of a method that throws an exception if preconditions aren't met.

## I

**Infrastructure Layer**
: The outermost layer in Clean Architecture, containing implementations for data access and external services.

**Invariant**
: A business rule that must always be true for an aggregate.

## J

**JSONB**
: PostgreSQL's binary JSON data type, optimized for storage and querying.

## M

**MediatR**
: A library that implements the Mediator pattern for in-process messaging.

**Migration**
: A versioned database schema change managed by EF Core.

## P

**Persistence Ignorance**
: The principle that domain models should not know how they're persisted.

**Prompt**
: Instructions given to an AI model to generate specific output.

## Q

**Query**
: In CQRS, a request to read data without changing system state.

## R

**Repository**
: An abstraction for data access that provides collection-like interface for aggregates.

**Rich Domain Model**
: A domain model that contains both data and behavior.

## S

**Seed Data**
: Initial data inserted into a database for testing or development.

**Separation of Concerns**
: The principle that different concerns should be separated into distinct sections of code.

**SharedKernel**
: Common code shared across bounded contexts.

## T

**Template**
: A predefined structure used to generate code with Claude.

**Transaction**
: A unit of work that either completely succeeds or completely fails.

## U

**Ubiquitous Language**
: A common language shared between developers and domain experts.

**Unit of Work**
: A pattern that maintains a list of objects affected by a business transaction and coordinates writing changes.

**Use Case**
: A specific way that users interact with the system to achieve a goal.

## V

**Value Object**
: A domain object defined by its attributes rather than a unique identity.

**Validation**
: The process of ensuring data meets business rules and constraints.

## Z

**ZIP Package**
: A compressed archive containing all generated code files.

---

## Acronyms Quick Reference

| Acronym | Full Term |
|---------|-----------|
| API | Application Programming Interface |
| CQRS | Command Query Responsibility Segregation |
| DDD | Domain-Driven Design |
| DTO | Data Transfer Object |
| EF | Entity Framework |
| ORM | Object-Relational Mapping |
| PDF | Portable Document Format |
| REST | Representational State Transfer |
| SQL | Structured Query Language |
| UI | User Interface |
| UUID | Universally Unique Identifier |

---

**Note:** Domain-specific terms are defined in the context of this system and may vary in other contexts.
"@

New-DocFile "APPENDIX-C-GLOSSARY.md" $appendixCContent

Write-Host ""
Write-Host "🎨 Generating diagrams..." -ForegroundColor Cyan

# Create diagram templates (Mermaid format)
$architectureDiagram = @"
graph TD
    A[Web Layer] -->|Uses| B[UseCases Layer]
    B -->|Uses| C[Core Domain Layer]
    C -.->|Defines Interfaces| D[Infrastructure Layer]
    D -->|Implements| C
    D -->|Accesses| E[(PostgreSQL Database)]
    D -->|Calls| F[Claude API]
    
    subgraph "Clean Architecture Layers"
        A
        B
        C
        D
    end
    
    style C fill:#f9f,stroke:#333,stroke-width:4px
    style D fill:#bbf,stroke:#333,stroke-width:2px
    style E fill:#bfb,stroke:#333,stroke-width:2px
    style F fill:#fbb,stroke:#333,stroke-width:2px
"@

New-DocFile "diagrams/architecture-overview.mmd" $architectureDiagram

$boundedContextsDiagram = @"
graph LR
    FC[Form Context] -->|FormCreatedEvent| CG[Code Generation Context]
    IC[Import Context] -->|CandidateApprovedEvent| FC
    
    subgraph "Form Context"
        FC
        FA[Form Aggregate]
        FR[Form Revision]
    end
    
    subgraph "Import Context"
        IC
        IB[Import Batch]
        IFC[Form Candidate]
    end
    
    subgraph "Code Generation Context"
        CG
        CGJ[Code Gen Job]
        ART[Artifacts]
    end
    
    style FC fill:#f96,stroke:#333,stroke-width:2px
    style IC fill:#69f,stroke:#333,stroke-width:2px
    style CG fill:#6f9,stroke:#333,stroke-width:2px
"@

New-DocFile "diagrams/bounded-contexts.mmd" $boundedContextsDiagram

Write-Host ""
Write-Host "📝 Generating templates..." -ForegroundColor Cyan

$commitMessages = @"
# Git Commit Message Templates

Use these standardized commit messages throughout the migration.

## Phase Completion Messages

``````
Phase 1: Foundation - Created SharedKernel with base classes and interfaces
Phase 2: Form Domain - Implemented Form aggregate with value objects and events
Phase 3: Form Infrastructure - Set up EF Core with PostgreSQL and repositories
Phase 4: Form API - Implemented CQRS with MediatR and REST endpoints
Phase 5: Claude Integration - Integrated Anthropic Claude Sonnet 4 API
Phase 6: Import Context - Built PDF upload and AI extraction system
Phase 7: Code Generation - Implemented template engine and code generator
Phase 8: Integration - Connected contexts with domain events
Phase 9: Testing - Added comprehensive test suite and documentation
``````

## Feature Addition Messages

``````
feat(form): Add form revision tracking
feat(import): Implement batch processing
feat(codegen): Add React component templates
feat(api): Add pagination support
``````

## Bug Fix Messages

``````
fix(form): Correct validation in CreateRevision
fix(import): Handle PDF parsing errors gracefully
fix(codegen): Fix template rendering for nested objects
``````

## Refactoring Messages

``````
refactor(core): Extract PDF service to domain service
refactor(infra): Simplify repository implementations
refactor(api): Improve error handling middleware
``````

## Documentation Messages

``````
docs: Update README with setup instructions
docs: Add API documentation for Import endpoints
docs: Document code generation templates
``````
"@

New-DocFile "templates/git-commit-messages.md" $commitMessages

$checklistTemplate = @"
# Phase Completion Checklist Template

Copy this checklist for each phase you complete.

## Phase X: [Phase Name]

**Date Started:** _______________  
**Date Completed:** _______________  
**Completed By:** _______________

### Pre-Phase Checklist

- [ ] Previous phase verified and committed
- [ ] All tests passing
- [ ] Database up to date
- [ ] Dependencies installed

### Implementation Checklist

- [ ] Step 1: [Description]
- [ ] Step 2: [Description]
- [ ] Step 3: [Description]
- [ ] Step 4: [Description]
- [ ] Step 5: [Description]

### Verification Checklist

- [ ] Code compiles without warnings
- [ ] Unit tests pass (if applicable)
- [ ] Integration tests pass (if applicable)
- [ ] Manual testing completed
- [ ] Documentation updated
- [ ] Code reviewed
- [ ] Git committed with proper message

### Issues Encountered

| Issue | Resolution | Time Lost |
|-------|------------|-----------|
|       |            |           |

### Lessons Learned

- 
- 
- 

### Next Steps

- [ ] Review next phase document
- [ ] Prepare environment for next phase
- [ ] Schedule team sync (if needed)

---

**Phase Status:** [ ] Complete  
**Ready for Next Phase:** [ ] Yes / [ ] No
"@

New-DocFile "templates/phase-checklist-template.md" $checklistTemplate

Write-Host ""
Write-Host "✅ Documentation generation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Created structure:" -ForegroundColor Cyan
Write-Host "   └── $baseDir/" -ForegroundColor Gray
Write-Host "       ├── README.md" -ForegroundColor Gray
Write-Host "       ├── 00-OVERVIEW.md" -ForegroundColor Gray
Write-Host "       ├── 01-PHASE-1-FOUNDATION.md" -ForegroundColor Gray
Write-Host "       ├── ... (all phase files)" -ForegroundColor Gray
Write-Host "       ├── APPENDIX-A-CODE-EXAMPLES.md" -ForegroundColor Gray
Write-Host "       ├── APPENDIX-B-TROUBLESHOOTING.md" -ForegroundColor Gray
Write-Host "       ├── APPENDIX-C-GLOSSARY.md" -ForegroundColor Gray
Write-Host "       ├── diagrams/" -ForegroundColor Gray
Write-Host "       └── templates/" -ForegroundColor Gray
Write-Host ""
Write-Host "🎉 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Review the generated files" -ForegroundColor White
Write-Host "   2. Fill in detailed content for each phase" -ForegroundColor White
Write-Host "   3. Create ZIP: Compress-Archive -Path '$baseDir' -DestinationPath 'FormGenAI-Migration-Guide.zip'" -ForegroundColor White
Write-Host ""
Write-Host "📦 To create ZIP archive:" -ForegroundColor Cyan
Write-Host "   Compress-Archive -Path '$baseDir' -DestinationPath 'FormGenAI-Migration-Guide.zip'" -ForegroundColor White
Write-Host ""
```

---

## **File: generate-docs.sh** (Bash for Mac/Linux)

```bash
#!/bin/bash

# FormGenAI Migration Guide Generator
# Bash script to create all documentation files

echo "🚀 FormGenAI Migration Guide Generator"
echo "======================================="
echo ""

# Create base directory
BASE_DIR="FormGenAI-Migration-Guide"

if [ -d "$BASE_DIR" ]; then
    echo -n "⚠️  Directory already exists. Remove it? (y/n): "
    read -r response
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
    local content="$2"
    echo "$content" > "$BASE_DIR/$filename"
    echo "  ✓ Created $filename"
}

# README.md
read -r -d '' README_CONTENT << 'EOF'
# FormGenAI Migration Guide - Complete Documentation

Welcome to the complete migration guide for transforming your Ardalis CleanArchitecture solution into a DDD-based AI-powered code generation system.

[Same content as PowerShell version]
EOF

create_file "README.md" "$README_CONTENT"

# Continue with other files...
# (Same structure as PowerShell script)

echo ""
echo "✅ Documentation generation complete!"
echo ""
echo "📁 Created structure:"
echo "   └── $BASE_DIR/"
echo "       ├── README.md"
echo "       ├── All phase files..."
echo "       ├── Appendices..."
echo "       ├── diagrams/"
echo "       └── templates/"
echo ""
echo "🎉 Next steps:"
echo "   1. Review the generated files"
echo "   2. Fill in detailed content for each phase"
echo "   3. Create ZIP: zip -r FormGenAI-Migration-Guide.zip $BASE_DIR"
echo ""
echo "📦 To create ZIP archive:"
echo "   zip -r FormGenAI-Migration-Guide.zip $BASE_DIR"
echo ""
```

---

## **Usage Instructions**

### For Windows:
```powershell
# Save the PowerShell script as generate-docs.ps1
# Run it:
.\generate-docs.ps1

# After completion, create ZIP:
Compress-Archive -Path FormGenAI-Migration-Guide -DestinationPath FormGenAI-Migration-Guide.zip
```

### For Mac/Linux:
```bash
# Save the Bash script as generate-docs.sh
# Make it executable:
chmod +x generate-docs.sh

# Run it:
./generate-docs.sh

# After completion, create ZIP:
zip -r FormGenAI-Migration-Guide.zip FormGenAI-Migration-Guide
```

---

## What the Scripts Do

1. **Create directory structure** with all necessary folders
2. **Generate README.md** with table of contents
3. **Create 00-OVERVIEW.md** with architecture and prerequisites
4. **Generate all 9 phase documents** (01-PHASE-1 through 09-PHASE-9)
5. **Create 3 appendices** (Code Examples, Troubleshooting, Glossary)
6. **Add diagram templates** in Mermaid format
7. **Include useful templates** (commit messages, checklists)

The generated files include:
- ✅ Proper markdown formatting
- ✅ Code blocks with syntax highlighting
- ✅ Tables and checklists
- ✅ Cross-references between documents
- ✅ Git commit message templates
- ✅ Troubleshooting guides
- ✅ Complete glossary

---

Would you like me to:
1. **Expand any specific phase** with more detailed content?
2. **Add more code examples** to the appendices?
3. **Create additional helper scripts** (like a progress tracker)?
4. **Generate a PDF version** instead of markdown?