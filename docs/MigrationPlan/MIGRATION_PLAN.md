# FormContext Migration & Document Intelligence Implementation Plan

## Overview
This plan outlines the migration from the simple FormAggregate CRUD model to the advanced FormContext DDD model, integrated with Azure Document Intelligence for PDF form extraction and templated code generation.

## Current State
- ✅ FormAggregate (simple CRUD) with complete use cases
- ✅ Basic CRUD endpoints working
- ⚠️ FormContext domain model exists but not connected
- ⚠️ Document Intelligence interfaces defined but not implemented
- ⚠️ Code generation infrastructure exists but not wired up

## Phase 1: Database Migration (FormContext Schema)

### 1.1 Update Database Schema
**Goal:** Support FormContext aggregates with revisions and JSON-based field definitions

**Tasks:**
- [ ] Create new migration for FormContext tables
- [ ] Add `Forms_v2` table with FormContext schema
- [ ] Add `FormRevisions` table for version history
- [ ] Add indexes for common queries (by origin type, by active status)
- [ ] Create data migration script to copy FormAggregate → FormContext

**Schema Changes:**
```sql
-- Forms table (FormContext)
CREATE TABLE Forms (
    Id TEXT PRIMARY KEY,  -- Guid
    Name TEXT NOT NULL,
    Definition TEXT NOT NULL,  -- JSON: FormDefinition
    Origin_Type INTEGER NOT NULL,  -- OriginType enum
    Origin_CreatedBy TEXT NOT NULL,
    Origin_CreatedAt TEXT NOT NULL,
    Origin_ReferenceId TEXT,
    IsActive INTEGER NOT NULL,
    CreatedDate TEXT NOT NULL,
    UpdatedDate TEXT NOT NULL
);

-- FormRevisions table
CREATE TABLE FormRevisions (
    Id TEXT PRIMARY KEY,  -- Guid
    FormId TEXT NOT NULL,
    Version INTEGER NOT NULL,
    Definition TEXT NOT NULL,  -- JSON: FormDefinition snapshot
    Notes TEXT,
    CreatedBy TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (FormId) REFERENCES Forms(Id)
);
```

### 1.2 Update EF Core Configuration
**Files to modify:**
- `Infrastructure/Data/Config/FormConfiguration.cs` - Switch to FormContext
- `Infrastructure/Data/AppDbContext.cs` - Add FormRevisions DbSet
- Create `Infrastructure/Data/Config/FormRevisionConfiguration.cs`

## Phase 2: Azure Document Intelligence Integration

### 2.1 Setup Azure Resources
**Prerequisites:**
- [ ] Create Azure Document Intelligence resource
- [ ] Store connection string and endpoint in appsettings
- [ ] Add NuGet package: `Azure.AI.FormRecognizer` (latest version)

**Configuration:**
```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://<resource-name>.cognitiveservices.azure.com/",
    "Key": "<api-key>",
    "ModelId": "prebuilt-document"  // or custom trained model
  }
}
```

### 2.2 Implement FormExtractor Service
**Create:** `Infrastructure/DocumentIntelligence/FormExtractorService.cs`

**Responsibilities:**
- Extract form fields from PDF using Document Intelligence
- Map Azure results to `ExtractedFormStructure`
- Detect field types (text, number, date, checkbox, dropdown)
- Extract tables and repeating sections
- Generate validation patterns from field analysis
- Detect required fields based on labels/structure

**Key Methods:**
```csharp
public class FormExtractorService : IFormExtractor
{
    private readonly DocumentAnalysisClient _client;

    // Analyze PDF and extract fields
    public async Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        string formType,
        CancellationToken ct)
    {
        // 1. Use Document Intelligence to analyze PDF
        // 2. Extract key-value pairs as form fields
        // 3. Extract tables as structured data
        // 4. Infer field types from content
        // 5. Generate ExtractedFormStructure
    }

    // Auto-detect form type/purpose
    public async Task<string> DetectFormTypeAsync(string pdfPath, CancellationToken ct)
    {
        // Analyze document and classify
        // Return: "survey", "registration", "application", etc.
    }
}
```

**Field Type Detection Logic:**
```csharp
private string InferFieldType(string fieldName, string? fieldValue)
{
    // Email pattern
    if (fieldName.Contains("email", StringComparison.OrdinalIgnoreCase))
        return "email";

    // Phone pattern
    if (fieldName.Contains("phone", StringComparison.OrdinalIgnoreCase))
        return "tel";

    // Date pattern
    if (DateTime.TryParse(fieldValue, out _))
        return "date";

    // Number pattern
    if (decimal.TryParse(fieldValue, out _))
        return "number";

    // Checkbox (yes/no, true/false)
    if (fieldValue is "yes" or "no" or "true" or "false")
        return "checkbox";

    // Default to text
    return "text";
}
```

### 2.3 Mapping Document Intelligence → FormDefinition
**Create:** `Infrastructure/DocumentIntelligence/FormDefinitionMapper.cs`

**Responsibilities:**
- Convert `ExtractedFormStructure` → `FormDefinition` value object
- Build JSON schema from extracted fields
- Set field validation rules
- Handle nested/repeating sections

**Example Mapping:**
```csharp
public FormDefinition MapToFormDefinition(ExtractedFormStructure extracted)
{
    var fields = extracted.Fields.Select(f => new FormField
    {
        Name = SanitizeFieldName(f.Name),
        Type = f.Type,
        Label = f.Label,
        Required = f.IsRequired,
        MaxLength = f.MaxLength,
        DefaultValue = f.DefaultValue,
        Options = f.Options,
        Pattern = f.ValidationPattern
    }).ToList();

    return FormDefinition.FromFields(fields);
}
```

### 2.4 Re-enable AnalyzeFormCommandHandler
**File:** `UseCases/Commands/AnalyzeForm/AnalyzeFormCommandHandler.cs.disabled`

**Updates needed:**
- Change from FormContext.Form to FormAggregate.Form temporarily
- Or migrate to use FormContext.Form properly
- Wire up FormExtractorService
- Store extracted form with origin tracking

**Workflow:**
```
1. Receive PDF upload (AnalyzeFormCommand)
2. Save PDF to temp storage
3. Call FormExtractor.ExtractFormStructureAsync()
4. Create Form aggregate with extracted definition
5. Create initial FormRevision
6. Set Origin to "Import" with PDF reference
7. Save to database
8. Return AnalyzeFormResult with form ID
```

## Phase 3: Use Cases Migration to FormContext

### 3.1 Create New FormContext Use Cases
**Location:** `UseCases/FormContext/` (separate from old Forms)

**New Commands/Queries:**
- [ ] `CreateFormFromDefinitionCommand` - Create form with JSON definition
- [ ] `CreateFormFromPdfCommand` - Upload PDF → extract → create form
- [ ] `GetFormByIdQuery` - Get form with current revision
- [ ] `GetFormRevisionsQuery` - List all revisions for a form
- [ ] `CreateFormRevisionCommand` - Add new revision to existing form
- [ ] `UpdateFormNameCommand` - Rename form
- [ ] `ActivateFormCommand` / `DeactivateFormCommand`
- [ ] `ListFormsQuery` - List all forms with filters
- [ ] `SearchFormsByFieldQuery` - Search by field names/types

### 3.2 Create FormContext DTOs
```csharp
public class FormContextDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public FormDefinitionDTO Definition { get; set; }
    public OriginMetadataDTO Origin { get; set; }
    public bool IsActive { get; set; }
    public int CurrentVersion { get; set; }
    public int FieldCount { get; set; }
}

public class FormDefinitionDTO
{
    public List<FormFieldDTO> Fields { get; set; }
    public string Schema { get; set; }
}

public class FormRevisionDTO
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public FormDefinitionDTO Definition { get; set; }
    public string Notes { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Phase 4: Code Generation Implementation

### 4.1 Install Scriban Template Engine
**Already included in project** - `CodeGenerationContext/Services/ScribanTemplateEngine.cs`

**NuGet Package:** `Scriban` (verify it's referenced)

### 4.2 Create Code Templates
**Location:** `Infrastructure/CodeGeneration/Templates/`

**Templates to create:**
- [ ] `ModelTemplate.sbn` - C# model/entity class
- [ ] `ControllerTemplate.sbn` - ASP.NET Core API controller
- [ ] `ViewTemplate.sbn` - Razor view for form display
- [ ] `ValidationTemplate.sbn` - FluentValidation validator class
- [ ] `DTOTemplate.sbn` - Data transfer objects

**Example ModelTemplate.sbn:**
```scriban
using System;
using System.ComponentModel.DataAnnotations;

namespace {{ namespace }}
{
    /// <summary>
    /// {{ form.name }} - Generated from form definition
    /// </summary>
    public class {{ model_name }}
    {
        {{~ for field in form.definition.fields ~}}

        /// <summary>
        /// {{ field.label }}
        /// </summary>
        {{~ if field.required ~}}
        [Required]
        {{~ end ~}}
        {{~ if field.max_length ~}}
        [MaxLength({{ field.max_length }})]
        {{~ end ~}}
        {{~ if field.pattern ~}}
        [RegularExpression("{{ field.pattern }}")]
        {{~ end ~}}
        public {{ get_csharp_type field.type }} {{ field.name | string.capitalize }} { get; set; }
        {{~ end ~}}
    }
}
```

### 4.3 Implement CodeGeneratorService
**Create:** `Infrastructure/CodeGeneration/CodeGeneratorService.cs`

**Responsibilities:**
- Load templates from embedded resources or file system
- Render templates using FormContext.Form data
- Generate multiple artifacts (Model, Controller, View, etc.)
- Package as ZIP for download

**Implementation:**
```csharp
public class CodeGeneratorService : ICodeGenerator
{
    private readonly ScribanTemplateEngine _templateEngine;
    private readonly TemplateRepository _templateRepository;
    private readonly ZipPackager _zipPackager;

    public async Task<GeneratedCodeOutput> GenerateCodeAsync(
        Form form,
        string formName,
        string targetNamespace,
        string? formPurpose,
        CancellationToken ct)
    {
        // 1. Load templates
        var modelTemplate = await _templateRepository.LoadTemplateAsync("Model");
        var controllerTemplate = await _templateRepository.LoadTemplateAsync("Controller");
        var viewTemplate = await _templateRepository.LoadTemplateAsync("View");

        // 2. Prepare template context
        var context = new
        {
            form_name = formName,
            @namespace = targetNamespace,
            form = new
            {
                name = form.Name,
                definition = new
                {
                    fields = form.Definition.Fields.Select(f => new
                    {
                        name = f.Name,
                        type = f.Type,
                        label = f.Label,
                        required = f.Required,
                        max_length = f.MaxLength,
                        pattern = f.Pattern,
                        options = f.Options
                    })
                }
            }
        };

        // 3. Render templates
        var modelCode = await _templateEngine.RenderAsync(modelTemplate, context);
        var controllerCode = await _templateEngine.RenderAsync(controllerTemplate, context);
        var viewCode = await _templateEngine.RenderAsync(viewTemplate, context);

        // 4. Analyze complexity
        var analysis = AnalyzeFormComplexity(form);

        return new GeneratedCodeOutput
        {
            ModelCode = modelCode,
            ControllerCode = controllerCode,
            ViewCode = viewCode,
            Analysis = analysis
        };
    }

    private GeneratedCodeAnalysis AnalyzeFormComplexity(Form form)
    {
        var fieldCount = form.FieldCount;
        var hasComplexFields = form.Definition.Fields
            .Any(f => f.Type == "table" || f.Options?.Count > 10);

        return new GeneratedCodeAnalysis
        {
            DetectedPurpose = InferFormPurpose(form),
            Complexity = fieldCount switch
            {
                <= 5 => "Simple",
                <= 15 => "Medium",
                _ => "Complex"
            },
            HasRepeatingData = hasComplexFields,
            RecommendedApproach = hasComplexFields
                ? "Consider using a collection property for repeating sections"
                : "Standard model-based approach recommended"
        };
    }
}
```

### 4.4 Wire Up Code Generation Command
**Create:** `UseCases/CodeGeneration/GenerateCodeCommand.cs`

```csharp
public record GenerateCodeCommand(
    Guid FormId,
    string TargetNamespace,
    string OutputFormat  // "zip", "single-file", etc.
) : IRequest<Result<GeneratedCodeOutput>>;

public class GenerateCodeHandler : IRequestHandler<GenerateCodeCommand, Result<GeneratedCodeOutput>>
{
    private readonly IFormRepository _formRepository;
    private readonly ICodeGenerator _codeGenerator;

    public async Task<Result<GeneratedCodeOutput>> Handle(
        GenerateCodeCommand request,
        CancellationToken ct)
    {
        // 1. Get form by ID
        var form = await _formRepository.GetByIdAsync(request.FormId, ct);
        if (form == null)
            return Result.NotFound("Form not found");

        // 2. Generate code
        var output = await _codeGenerator.GenerateCodeAsync(
            form,
            form.Name,
            request.TargetNamespace,
            null,
            ct);

        return Result.Success(output);
    }
}
```

## Phase 5: API Endpoints for New Workflow

### 5.1 Create FormContext Endpoints
**Location:** `Web/FormContext/`

**New Endpoints:**
- [ ] `POST /api/forms/upload-pdf` - Upload PDF for analysis
- [ ] `GET /api/forms/{id}` - Get form with definition
- [ ] `GET /api/forms/{id}/revisions` - Get form revisions
- [ ] `POST /api/forms/{id}/revisions` - Create new revision
- [ ] `POST /api/forms/{id}/generate-code` - Generate code from form
- [ ] `GET /api/forms/{id}/download-code` - Download generated code as ZIP

### 5.2 Upload PDF Endpoint Example
```csharp
public class UploadPdfForAnalysis : Endpoint<UploadPdfRequest, UploadPdfResponse>
{
    private readonly IMediator _mediator;

    public override void Configure()
    {
        Post("/api/forms/upload-pdf");
        AllowFileUploads();
    }

    public override async Task HandleAsync(
        UploadPdfRequest request,
        CancellationToken ct)
    {
        // Send to AnalyzeFormCommand
        var command = new AnalyzeFormCommand(
            request.PdfFile.OpenReadStream(),
            request.PdfFile.FileName);

        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(new UploadPdfResponse
            {
                FormId = result.Value.FormId,
                FieldCount = result.Value.FieldCount,
                Warnings = result.Value.Warnings
            }, ct);
        }
    }
}
```

## Phase 6: Testing & Validation

### 6.1 Integration Tests
- [ ] Test PDF upload → extraction → form creation
- [ ] Test form revision creation
- [ ] Test code generation from various form types
- [ ] Test generated code compiles successfully

### 6.2 Sample PDFs for Testing
- [ ] Simple contact form (5-10 fields)
- [ ] Complex application form (20+ fields)
- [ ] Form with tables/repeating sections
- [ ] Multi-page form

## Phase 7: Migration Cutover

### 7.1 Gradual Migration Strategy
**Option A: Parallel Running**
- Keep both FormAggregate and FormContext endpoints
- Migrate users gradually
- Eventually deprecate FormAggregate

**Option B: Big Bang Migration**
- Migrate all data at once
- Switch all endpoints to FormContext
- Remove FormAggregate code

### 7.2 Data Migration Script
```csharp
public class MigrateFormsToFormContext
{
    public async Task MigrateAsync()
    {
        var oldForms = await _oldFormRepository.ListAllAsync();

        foreach (var oldForm in oldForms)
        {
            // Create FormDefinition from old form
            var fields = new List<FormField>
            {
                new FormField
                {
                    Name = "FormNumber",
                    Type = "text",
                    Label = "Form Number",
                    Required = true,
                    DefaultValue = oldForm.FormNumber
                },
                // ... map other fields
            };

            var definition = FormDefinition.FromFields(fields);

            // Create new FormContext Form
            var newForm = Form.Create(
                oldForm.FormTitle,
                definition,
                OriginMetadata.FromManual(oldForm.Owner?.Name ?? "System", Guid.NewGuid()),
                "migration-script"
            );

            await _newFormRepository.AddAsync(newForm);
        }
    }
}
```

## Timeline Estimate

**Phase 1 (Database):** 1-2 days
**Phase 2 (Document Intelligence):** 3-5 days
**Phase 3 (Use Cases):** 2-3 days
**Phase 4 (Code Generation):** 4-6 days
**Phase 5 (API Endpoints):** 2-3 days
**Phase 6 (Testing):** 2-3 days
**Phase 7 (Migration):** 1-2 days

**Total:** ~15-24 days

## Success Criteria

✅ PDF forms can be uploaded and automatically extracted
✅ Extracted forms are stored with version history
✅ Code can be generated from form definitions
✅ Generated code is syntactically correct and follows conventions
✅ Users can download generated code as ZIP package
✅ All original FormAggregate functionality is preserved

## Next Steps

1. **Immediate:** Set up Azure Document Intelligence resource
2. **Week 1:** Implement database migration and FormExtractorService
3. **Week 2:** Build out code generation with templates
4. **Week 3:** Create new API endpoints and test end-to-end
5. **Week 4:** Migrate existing data and deprecate old endpoints

---

**Questions to Resolve:**
- Which Azure Document Intelligence model to use? (prebuilt-document vs custom)
- Code generation target: .NET version? Framework (ASP.NET Core MVC, Razor Pages, Blazor)?
- Should generated code include unit tests?
- ZIP structure: Single project or multi-project solution?
