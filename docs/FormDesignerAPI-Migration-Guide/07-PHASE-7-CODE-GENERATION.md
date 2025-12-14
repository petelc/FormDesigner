# Phase 7: Code Generation Context

**Duration:** TBD  
**Complexity:** Medium-High  
**Prerequisites:** Previous phases complete



## Overview

Using the Scriban template engine take in the json string created during the import AI service and using provided templates generate the necessary Code files.

# Scriban Code Generation - Complete Setup Guide

## 📦 What You're Building

A template-based code generation system that generates consistent, standards-compliant code from form definitions.

**Key Benefits:**
- ✅ 100% consistent with YOUR coding standards
- ✅ Millisecond generation (vs seconds with AI)
- ✅ Zero API costs
- ✅ Works offline
- ✅ Fully version controlled
- ✅ Easy to customize

---

## 📋 Prerequisites

- .NET 8.0 SDK
- FormDesignerAPI solution (with Traxs.SharedKernel already installed)
- Basic understanding of Scriban templating (optional - templates are provided)

---

## 🚀 Installation Steps

### Step 1: Install Scriban Package

```bash
cd src/FormDesignerAPI.Core
dotnet add package Scriban
cd ../..
```

### Step 2: Create Directory Structure

```bash
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Aggregates
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/ValueObjects
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Services
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Events
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Templates/CSharp
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Templates/Sql
mkdir -p src/FormDesignerAPI.Core/CodeGenerationContext/Templates/React
```

### Step 3: Copy All Files

Download all artifacts (1-22) and copy them to the specified locations in each file's header comment.

**File Checklist:**

#### ValueObjects/ (5 files)
- [ ] ArtifactType.cs
- [ ] GenerationVersion.cs
- [ ] GeneratedArtifact.cs
- [ ] GenerationOptions.cs
- [ ] TemplateMetadata.cs

#### Events/ (4 files)
- [ ] CodeGenerationJobCreatedEvent.cs
- [ ] CodeGenerationJobProcessingEvent.cs
- [ ] CodeArtifactsGeneratedEvent.cs
- [ ] CodeGenerationFailedEvent.cs

#### Aggregates/ (1 file)
- [ ] CodeGenerationJob.cs

#### Services/ (5 files)
- [ ] ScribanTemplateEngine.cs
- [ ] TemplateRepository.cs
- [ ] CodeGenerationOrchestrator.cs
- [ ] CodeArtifactOrganizer.cs
- [ ] ZipPackager.cs

#### Templates/ (8 .sbn files)
- [ ] CSharp/Entity.sbn
- [ ] CSharp/Repository.sbn
- [ ] CSharp/Interface.sbn
- [ ] CSharp/Controller.sbn
- [ ] CSharp/Dto.sbn
- [ ] Sql/CreateTable.sbn
- [ ] Sql/StoredProcs.sbn
- [ ] React/FormComponent.sbn
- [ ] React/ValidationSchema.sbn

### Step 4: Build and Verify

```bash
cd src/FormDesignerAPI.Core
dotnet build
```

Should build without errors.

---

## 🎯 Usage Example

Here's how to use the code generation system:

```csharp
// 1. Create a form definition (from PDF extraction)
var formDefinition = FormDefinition.From(extractedJsonSchema);

// 2. Configure generation options
var options = GenerationOptions.FullStack(
    projectName: "PatientManagement",
    author: "Your Team"
);

// 3. Generate code
var orchestrator = new CodeGenerationOrchestrator(
    templateEngine,
    templateRepository,
    organizer,
    zipPackager,
    logger
);

var job = await orchestrator.GenerateAsync(
    formId: formId,
    revisionId: revisionId,
    formDefinition: formDefinition,
    options: options,
    requestedBy: "admin@company.com",
    cancellationToken: CancellationToken.None
);

// 4. Download the ZIP file
var zipPath = job.ZipFilePath; // Path to generated ZIP
```

---

## 🎨 Customizing Templates

### Example: Modify Entity Template

Edit `Templates/CSharp/Entity.sbn`:

```scriban
{{- # Add your company's copyright notice -}}
//
// Copyright © {{ now | date.year }} Your Company
// All rights reserved.
//

using System;
using Traxs.SharedKernel;
using {{ Namespace }}.Core.Interfaces;

namespace {{ Namespace }}.Core.Entities;

/// <summary>
/// {{ EntityName }} entity
/// Generated: {{ GeneratedDateFormatted }}
/// </summary>
public class {{ EntityName }} : EntityBase<Guid>, IAggregateRoot
{
    // ... rest of template
}
```

### Available Variables in Templates

All templates have access to:

```javascript
{
  EntityName: "User",
  EntityNamePlural: "Users",
  EntityNameCamel: "user",
  Namespace: "YourApp",
  ProjectName: "Your Project",
  Author: "Your Name",
  GeneratedDate: DateTime.UtcNow,
  GeneratedDateFormatted: "2024-12-11 20:00:00",
  
  Fields: [
    {
      Name: "email",
      Type: "email",
      Required: true,
      Label: "Email Address",
      CSharpType: "string",
      SqlType: "NVARCHAR(255)",
      TypeScriptType: "string",
      NamePascal: "Email",
      NameCamel: "email",
      NameSnake: "email"
    },
    // ... more fields
  ]
}
```

### Scriban Functions Available

```scriban
{{- # String manipulation -}}
{{ "user_name" | to_pascal_case }}     {{- # UserName -}}
{{ "UserName" | to_camel_case }}       {{- # userName -}}
{{ "UserName" | to_snake_case }}       {{- # user_name -}}
{{ "User" | pluralize }}               {{- # Users -}}
{{ "Users" | singularize }}            {{- # User -}}

{{- # Date formatting -}}
{{ now | date.to_string '%Y-%m-%d' }}  {{- # 2024-12-11 -}}

{{- # Conditionals -}}
{{- if field.Required -}}
    [Required]
{{- end -}}

{{- # Loops -}}
{{- for field in Fields -}}
    public {{ field.CSharpType }} {{ field.NamePascal }} { get; set; }
{{- end -}}
```

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│  CodeGenerationOrchestrator                         │
│  (Main entry point)                                 │
└────────┬────────────────────────────────────────────┘
         │
         ├──> ScribanTemplateEngine
         │    (Renders templates with data)
         │
         ├──> TemplateRepository
         │    (Manages template metadata)
         │
         ├──> CodeArtifactOrganizer
         │    (Organizes files into folder structure)
         │
         └──> ZipPackager
              (Creates ZIP file)
```

---

## 🧪 Testing Your Setup

Create a simple test:

```csharp
[Fact]
public async Task ShouldGenerateCode()
{
    // Arrange
    var fields = new List<FormField>
    {
        new FormField 
        { 
            Name = "firstName", 
            Type = "text", 
            Required = true,
            Label = "First Name"
        },
        new FormField 
        { 
            Name = "email", 
            Type = "email", 
            Required = true,
            Label = "Email"
        }
    };
    
    var definition = FormDefinition.FromFields(fields);
    var options = GenerationOptions.Minimal("TestProject", "TestAuthor");
    
    // Act
    var job = await _orchestrator.GenerateAsync(
        Guid.NewGuid(),
        Guid.NewGuid(),
        definition,
        options,
        "tester@test.com"
    );
    
    // Assert
    Assert.Equal(JobStatus.Completed, job.Status);
    Assert.True(job.Artifacts.Count > 0);
    Assert.NotNull(job.ZipFilePath);
    Assert.True(File.Exists(job.ZipFilePath));
}
```

---

## 📊 Generated Output Structure

```
GeneratedProject/
├── CSharp/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   └── User.cs
│   │   └── Interfaces/
│   │       └── IUserRepository.cs
│   ├── Infrastructure/
│   │   └── Repositories/
│   │       └── UserRepository.cs
│   ├── Application/
│   │   └── DTOs/
│   │       └── UserDtos.cs
│   └── Web/
│       └── Controllers/
│           └── UserController.cs
├── SQL/
│   ├── Tables/
│   │   └── CreateUserTable.sql
│   └── StoredProcedures/
│       └── UserStoredProcedures.sql
├── React/
│   └── Components/
│       ├── UserForm.tsx
│       └── UserValidation.ts
├── README.md
└── .gitignore
```

---

## 🔧 Configuration

### Configure Template Path

In your DI setup:

```csharp
services.AddScoped<TemplateRepository>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<TemplateRepository>>();
    var templatePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "CodeGenerationContext",
        "Templates"
    );
    return new TemplateRepository(templatePath, logger);
});
```

### Configure Output Path

```csharp
services.AddScoped<CodeArtifactOrganizer>(sp =>
{
    var outputPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "GeneratedCode"
    );
    return new CodeArtifactOrganizer(outputPath);
});
```

---

## 🐛 Troubleshooting

### Issue: Template file not found

**Solution:** Check that template files are copied to output directory.

In your .csproj:

```xml
<ItemGroup>
  <None Update="CodeGenerationContext\Templates\**\*.sbn">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Issue: Template syntax error

**Solution:** Use Scriban online tester to validate syntax:
https://github.com/scriban/scriban

### Issue: Generated code doesn't compile

**Solution:** 
1. Check your template logic
2. Verify field mappings (CSharpType, SqlType, etc.)
3. Test with simple form first

---

## 📚 Next Steps

1. ✅ Complete installation
2. ✅ Run test generation
3. ✅ Customize templates to match your standards
4. ✅ Integrate with your Import Context
5. ✅ Add more templates (AutoMapper, FluentValidation, etc.)

---

## 🎓 Learn More

- [Scriban Documentation](https://github.com/scriban/scriban/blob/master/doc/language.md)
- [Scriban Online Tester](https://scriban.github.io/scriban/)
- [Template Examples](https://github.com/scriban/scriban/tree/master/src/Scriban.Tests/TestFiles)

---

**Questions?** Review the code comments in each file for detailed explanations.

**Version:** 1.0.0  
**Last Updated:** December 2024

## Next Steps

Continue to next phase.
