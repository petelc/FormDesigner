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
