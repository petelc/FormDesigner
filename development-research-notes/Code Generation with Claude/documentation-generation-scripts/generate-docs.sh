#!/bin/bash

# FormDesignerAPI Migration Guide Generator
# Bash script to create all documentation files

echo "🚀 FormDesignerAPI Migration Guide Generator"
echo "======================================="
echo ""

# Create base directory
BASE_DIR="FormDesignerAPI-Migration-Guide"

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
create_file "README.md" \
'# FormDesignerAPI Migration Guide - Complete Documentation

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
```
FormDesignerAPI/
├── SharedKernel/          (Phase 1)
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

- [ ] Phase 1: Foundation ✅
- [ ] Phase 2: Form Domain ✅
- [ ] Phase 3: Form Infrastructure ✅
- [ ] Phase 4: Form API ✅
- [ ] Phase 5: Claude Integration ✅
- [ ] Phase 6: Import Context ✅
- [ ] Phase 7: Code Generation ✅
- [ ] Phase 8: Integration ✅
- [ ] Phase 9: Testing ✅

---

**Ready to start?** Proceed to [00-OVERVIEW.md](00-OVERVIEW.md)

**Last Updated:** December 2024  
**Version:** 1.0.0'

echo ""
echo "📄 Generating overview document..."

# 00-OVERVIEW.md
cat > "$BASE_DIR/00-OVERVIEW.md" << 'EOF'
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
EOF

echo "  ✓ Created 00-OVERVIEW.md"

echo ""
echo "📄 Generating phase documents..."

# Generate phase files
for i in {1..9}; do
    phase_num=$(printf "%02d" $i)
    
    case $i in
        1) phase_name="PHASE-1-FOUNDATION"; title="Phase 1: Foundation - SharedKernel" ;;
        2) phase_name="PHASE-2-FORM-DOMAIN"; title="Phase 2: Form Context - Domain Model" ;;
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

**Duration:** 3-5 days  
**Complexity:** Medium  
**Prerequisites:** Previous phases complete

## Overview

This phase implements [description].

## Objectives

- [ ] Objective 1
- [ ] Objective 2
- [ ] Objective 3

## Step-by-Step Implementation

### Step 1: Setup

[Instructions here]

### Step 2: Implementation

[Instructions here]

## Verification Checklist

- [ ] All code compiles
- [ ] Tests pass
- [ ] Documentation updated

## Git Commit

\`\`\`bash
git add .
git commit -m "$title complete"
\`\`\`

## Next Steps

Continue to the next phase.

---

**Phase $phase_num Complete!**
EOF
    
    echo "  ✓ Created ${phase_num}-${phase_name}.md"
done

echo ""
echo "📚 Generating appendices..."

# APPENDIX-A
cat > "$BASE_DIR/APPENDIX-A-CODE-EXAMPLES.md" << 'EOF'
# Appendix A: Complete Code Examples

## SharedKernel Examples

### EntityBase.cs
```csharp
using FormDesignerAPI.SharedKernel.Interfaces;

namespace FormDesignerAPI.SharedKernel.Base;

public abstract class EntityBase
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents.AsReadOnly();

    protected void RegisterDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

[Additional examples...]
EOF

echo "  ✓ Created APPENDIX-A-CODE-EXAMPLES.md"

# APPENDIX-B
cat > "$BASE_DIR/APPENDIX-B-TROUBLESHOOTING.md" << 'EOF'
# Appendix B: Troubleshooting Guide

## Build Errors

### Error: "The type or namespace name could not be found"

**Solution:**
```bash
dotnet restore
dotnet build
```

## Database Errors

### Error: "Password authentication failed"

**Solution:**
Check your connection string in appsettings.json

[Additional troubleshooting...]
EOF

echo "  ✓ Created APPENDIX-B-TROUBLESHOOTING.md"

# APPENDIX-C
cat > "$BASE_DIR/APPENDIX-C-GLOSSARY.md" << 'EOF'
# Appendix C: Glossary

**Aggregate Root**  
The main entity in an aggregate that enforces invariants.

**Bounded Context**  
A logical boundary within which a domain model is defined.

**Domain Event**  
A record of something that happened in the domain.

[Additional terms...]
EOF

echo "  ✓ Created APPENDIX-C-GLOSSARY.md"

echo ""
echo "📊 Creating diagram files..."

# Create diagrams
cat > "$BASE_DIR/diagrams/architecture-overview.mmd" << 'EOF'
graph TD
    A[FormDesignerAPI] --> B[SharedKernel]
    A --> C[Core]
    A --> D[UseCases]
    A --> E[Infrastructure]
    A --> F[Web]
EOF

echo "  ✓ Created architecture-overview.mmd"

cat > "$BASE_DIR/diagrams/bounded-contexts.mmd" << 'EOF'
graph LR
    FC[Form Context] -->|FormCreated| CG[Code Gen]
    IC[Import Context] -->|CandidateApproved| FC
EOF

echo "  ✓ Created bounded-contexts.mmd"

echo ""
echo "📝 Creating template files..."

cat > "$BASE_DIR/templates/git-commit-messages.md" << 'EOF'
# Git Commit Message Templates

## Phase Completion
```
Phase X: [Phase Name] complete

- Implemented [feature 1]
- Added [feature 2]
```
EOF

echo "  ✓ Created git-commit-messages.md"

cat > "$BASE_DIR/templates/phase-checklist-template.md" << 'EOF'
# Phase Checklist Template

## Before Starting
- [ ] Previous phase complete
- [ ] Tests passing

## Implementation
- [ ] Task 1
- [ ] Task 2

## Completion
- [ ] Code reviewed
- [ ] Committed
EOF

echo "  ✓ Created phase-checklist-template.md"

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
echo "   3. Start with 01-PHASE-1-FOUNDATION.md"
echo ""