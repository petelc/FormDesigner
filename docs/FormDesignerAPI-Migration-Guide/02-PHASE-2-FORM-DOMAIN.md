# Phase 2: Form Context - Domain Model

**Duration:** 3-5 days  
**Complexity:** Medium  
**Prerequisites:** Previous phases complete

## Overview

In this phase, you'll create the Form Context domain model using Traxs.SharedKernel package. You'll build aggregates, value objects, and domain events following DDD principles.

## Objectives

 -[] Add Traxs.SharedKernel to Core project
 -[] Create Form Context folder structure
 -[] Implement value objects (OriginType, OriginMetadata, FormDefinition)
 -[] Create Form aggregate root
 -[] Create FormRevision entity
 -[] Define domain events
 -[] Create repository interface
 -[] Verify domain layer has no infrastructure dependencies

## Step-by-Step Implementation

### Step 1: Add Traxs.SharedKernel Package

**1.1 Add package reference**

```bash
cd src/FormDesignerAPI.Core
dotnet add package Traxs.SharedKernel
```

**1.2 Verify Installation**

```bash
dotnet list package
```
You should see:

```bash
Traxs.SharedKernel    0.1.1
```

### Step 2: Create Folder Structure

```bash
cd src/FormDesignerAPI.Core

# Create Form Context structure
mkdir -p FormContext/Aggregates
mkdir -p FormContext/ValueObjects
mkdir -p FormContext/Events
mkdir -p FormContext/Interfaces
mkdir -p FormContext/Specifications
```

Your structure should look like:

```bash
FormDesignerAPI.Core/
└── FormContext/
    ├── Aggregates/
    ├── ValueObjects/
    ├── Events/
    ├── Interfaces/
    └── Specifications/
```

### Step 3: Create Value Objects

**3.1 Create OriginType Enum**

> File: FormContext/ValueObjects/OriginType.cs

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

**3.2 Create OriginMetadata Value Object**

> File: FormContext/ValueObjects/OriginMetadata.cs


## Verification Checklist

- [ ] All code compiles
- [ ] Tests pass
- [ ] Documentation updated

## Git Commit

```bash
git add .
git commit -m "Phase 2: Form Context - Domain Model complete"
```

## Next Steps

Continue to the next phase.

---

**Phase 02 Complete!**
