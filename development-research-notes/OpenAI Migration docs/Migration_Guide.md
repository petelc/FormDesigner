# Migration Guide: Integrating Azure AI Code Generation into Clean Architecture

## Overview

This guide walks you through the step-by-step process of migrating the Azure AI code generation features into your existing FormDesignerAPI Clean Architecture solution.

---

## Pre-Migration Checklist

Before starting, ensure you have:

- [ ] Access to Azure Document Intelligence resource
- [ ] Access to Azure OpenAI resource
- [ ] API keys for both services
- [ ] Visual Studio 2022 or VS Code
- [ ] .NET 8 SDK installed
- [ ] Backup of your current solution

---

## Migration Phases

### Phase 1: Prepare the Solution (15 minutes)

#### Step 1.1: Install Required NuGet Packages

Open a terminal in your solution root and run:

```bash
# Navigate to Infrastructure project
cd src/FormDesignerAPI.Infrastructure

# Add Azure AI packages
dotnet add package Azure.AI.FormRecognizer --version 4.1.0
dotnet add package Azure.AI.OpenAI --version 1.0.0-beta.12
dotnet add package itext7 --version 8.0.2

# Navigate to Core project (no new packages needed)
cd ../FormDesignerAPI.Core

# Navigate to UseCases project (no new packages needed)
cd ../FormDesignerAPI.UseCases

# Navigate to Web project (no new packages needed)
cd ../FormDesignerAPI.Web
```

#### Step 1.2: Update Project References

Verify that your project references are correct:

**FormDesignerAPI.Infrastructure.csproj** should reference:
```xml
<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
</ItemGroup>
```

**FormDesignerAPI.UseCases.csproj** should reference:
```xml
<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
</ItemGroup>
```

**FormDesignerAPI.Web.csproj** should reference:
```xml
<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
  <ProjectReference Include="..\FormDesignerAPI.UseCases\FormDesignerAPI.UseCases.csproj" />
  <ProjectReference Include="..\FormDesignerAPI.Infrastructure\FormDesignerAPI.Infrastructure.csproj" />
</ItemGroup>
```

#### Step 1.3: Create Directory Structure

Create the following folders:

```
src/FormDesignerAPI.Core/
├── Entities/
├── Enums/
└── Interfaces/

src/FormDesignerAPI.UseCases/
├── FormCodeGeneration/
└── DTOs/

src/FormDesignerAPI.Infrastructure/
├── AI/
│   ├── AzureDocumentIntelligence/
│   ├── AzureOpenAI/
│   └── Pipeline/
└── Configuration/

tests/FormDesignerAPI.UnitTests/
└── FormCodeGeneration/
```

PowerShell script to create folders:
```powershell
# Run from solution root
$folders = @(
    "src/FormDesignerAPI.Core/Entities",
    "src/FormDesignerAPI.Core/Enums",
    "src/FormDesignerAPI.Core/Interfaces",
    "src/FormDesignerAPI.UseCases/FormCodeGeneration",
    "src/FormDesignerAPI.UseCases/DTOs",
    "src/FormDesignerAPI.Infrastructure/AI/AzureDocumentIntelligence",
    "src/FormDesignerAPI.Infrastructure/AI/AzureOpenAI",
    "src/FormDesignerAPI.Infrastructure/AI/Pipeline",
    "src/FormDesignerAPI.Infrastructure/Configuration",
    "tests/FormDesignerAPI.UnitTests/FormCodeGeneration"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Path $folder -Force
}
```

---

### Phase 2: Core Layer Migration (20 minutes)

#### Step 2.1: Create Enums

**File: `src/FormDesignerAPI.Core/Enums/PdfFormType.cs`**

```csharp
namespace FormDesignerAPI.Core.Enums;

/// <summary>
/// Represents the type of PDF form
/// </summary>
public enum PdfFormType
{
    /// <summary>
    /// Static PDF with embedded form fields (AcroForm)
    /// </summary>
    AcroForm,
    
    /// <summary>
    /// XML-based dynamic forms (XFA)
    /// </summary>
    XFA,
    
    /// <summary>
    /// Forms with both AcroForm and XFA
    /// </summary>
    Hybrid,
    
    /// <summary>
    /// PDFs without form fields
    /// </summary>
    Static
}
```

#### Step 2.2: Create Domain Entities

**File: `src/FormDesignerAPI.Core/Entities/FormField.cs`**

```csharp
namespace FormDesignerAPI.Core.Entities;

/// <summary>
/// Represents a single field extracted from a form
/// </summary>
public class FormField
{
    /// <summary>
    /// Internal field name (from PDF metadata)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable label (displayed on form)
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// Field type (text, date, number, email, checkbox, etc.)
    /// </summary>
    public string FieldType { get; set; } = "text";
    
    /// <summary>
    /// Whether this field is required
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Maximum length constraint (for text fields)
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// Default value
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Options for dropdown/radio fields
    /// </summary>
    public List<string>? Options { get; set; }
    
    /// <summary>
    /// Validation pattern (regex)
    /// </summary>
    public string? ValidationPattern { get; set; }
    
    /// <summary>
    /// XFA-specific properties (calculations, scripts, etc.)
    /// </summary>
    public Dictionary<string, string>? XFAProperties { get; set; }
    
    /// <summary>
    /// Position on page
    /// </summary>
    public FieldPosition? Position { get; set; }
    
    /// <summary>
    /// Bounding box coordinates
    /// </summary>
    public IReadOnlyList<System.Drawing.PointF>? BoundingBox { get; set; }
}

/// <summary>
/// Represents the position of a field on a page
/// </summary>
public class FieldPosition
{
    public int Page { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}
```

**File: `src/FormDesignerAPI.Core/Entities/FormTable.cs`**

```csharp
namespace FormDesignerAPI.Core.Entities;

/// <summary>
/// Represents a table extracted from a form
/// </summary>
public class FormTable
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<TableCell> Cells { get; set; } = new();
}

/// <summary>
/// Represents a cell in a table
/// </summary>
public class TableCell
{
    public string Content { get; set; } = string.Empty;
    public int RowIndex { get; set; }
    public int ColumnIndex { get; set; }
    public bool IsHeader { get; set; }
}
```

**File: `src/FormDesignerAPI.Core/Entities/FormStructure.cs`**

```csharp
using FormDesignerAPI.Core.Enums;

namespace FormDesignerAPI.Core.Entities;

/// <summary>
/// Represents the complete structure of a PDF form
/// </summary>
public class FormStructure
{
    /// <summary>
    /// Type of PDF form (AcroForm, XFA, Hybrid, Static)
    /// </summary>
    public PdfFormType FormType { get; set; }
    
    /// <summary>
    /// List of all fields in the form
    /// </summary>
    public List<FormField> Fields { get; set; } = new();
    
    /// <summary>
    /// List of all tables in the form
    /// </summary>
    public List<FormTable> Tables { get; set; } = new();
    
    /// <summary>
    /// Warnings about complex features that may need manual review
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
```

**File: `src/FormDesignerAPI.Core/Entities/GeneratedCode.cs`**

```csharp
namespace FormDesignerAPI.Core.Entities;

/// <summary>
/// Represents code files generated from a form
/// </summary>
public class GeneratedCode
{
    /// <summary>
    /// C# model class code
    /// </summary>
    public string ModelCode { get; set; } = string.Empty;
    
    /// <summary>
    /// C# controller class code
    /// </summary>
    public string ControllerCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Razor view code
    /// </summary>
    public string ViewCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity Framework migration code
    /// </summary>
    public string MigrationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Suggested namespace for the generated code
    /// </summary>
    public string SuggestedNamespace { get; set; } = string.Empty;
    
    /// <summary>
    /// Analysis of the form
    /// </summary>
    public FormAnalysis FormAnalysis { get; set; } = new();
}

/// <summary>
/// Analysis metadata about the form
/// </summary>
public class FormAnalysis
{
    /// <summary>
    /// What the AI detected this form is used for
    /// </summary>
    public string DetectedPurpose { get; set; } = string.Empty;
    
    /// <summary>
    /// Complexity level (Simple, Medium, Complex)
    /// </summary>
    public string Complexity { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the form has repeating data (tables, multiple entries)
    /// </summary>
    public bool HasRepeatingData { get; set; }
    
    /// <summary>
    /// Recommendations for implementation
    /// </summary>
    public string RecommendedApproach { get; set; } = string.Empty;
}
```

#### Step 2.3: Create Interfaces

**File: `src/FormDesignerAPI.Core/Interfaces/IFormExtractor.cs`**

```csharp
using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Enums;

namespace FormDesignerAPI.Core.Interfaces;

/// <summary>
/// Service for extracting structure from PDF forms
/// </summary>
public interface IFormExtractor
{
    /// <summary>
    /// Detects the type of PDF form (AcroForm, XFA, Hybrid, Static)
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <returns>The detected form type</returns>
    PdfFormType DetectFormType(string pdfPath);
    
    /// <summary>
    /// Extracts the complete structure from a PDF form
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The extracted form structure</returns>
    Task<FormStructure> ExtractFormStructureAsync(
        string pdfPath,
        CancellationToken cancellationToken = default);
}
```

**File: `src/FormDesignerAPI.Core/Interfaces/ICodeGenerator.cs`**

```csharp
using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.Core.Interfaces;

/// <summary>
/// Service for generating code from form structures using AI
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Generates code files from a form structure
    /// </summary>
    /// <param name="formStructure">The extracted form structure</param>
    /// <param name="formName">Name to use for generated classes</param>
    /// <param name="formPurpose">Optional description of form purpose</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated code files</returns>
    Task<GeneratedCode> GenerateCodeFromFormAsync(
        FormStructure formStructure,
        string formName,
        string? formPurpose = null,
        CancellationToken cancellationToken = default);
}
```

**File: `src/FormDesignerAPI.Core/Interfaces/IFormToCodePipeline.cs`**

```csharp
using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.Core.Interfaces;

/// <summary>
/// Orchestrates the complete form-to-code pipeline
/// </summary>
public interface IFormToCodePipeline
{
    /// <summary>
    /// Processes a single form through the complete pipeline
    /// </summary>
    /// <param name="pdfPath">Path to the PDF file</param>
    /// <param name="formName">Name for the generated code</param>
    /// <param name="outputDirectory">Directory to save generated files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated code</returns>
    Task<GeneratedCode> ProcessFormAsync(
        string pdfPath,
        string formName,
        string outputDirectory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes multiple forms in batch
    /// </summary>
    /// <param name="pdfDirectory">Directory containing PDF files</param>
    /// <param name="outputDirectory">Directory to save generated files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch processing results</returns>
    Task<BatchProcessResult> ProcessBatchAsync(
        string pdfDirectory,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Results from batch processing multiple forms
/// </summary>
public class BatchProcessResult
{
    public int TotalForms { get; set; }
    public int SuccessfulForms { get; set; }
    public int FailedForms { get; set; }
    public List<FormProcessResult> Results { get; set; } = new();
}

/// <summary>
/// Result from processing a single form in a batch
/// </summary>
public class FormProcessResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
}
```

**Build Core Project**
```bash
cd src/FormDesignerAPI.Core
dotnet build
```

Verify no errors before proceeding.

---

### Phase 3: Use Cases Layer Migration (25 minutes)

#### Step 3.1: Create DTOs

**File: `src/FormDesignerAPI.UseCases/DTOs/FormAnalysisResult.cs`**

```csharp
namespace FormDesignerAPI.UseCases.DTOs;

/// <summary>
/// Result of analyzing a PDF form
/// </summary>
public class FormAnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public int TableCount { get; set; }
    public List<FieldSummary> Fields { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Summary information about a form field
/// </summary>
public class FieldSummary
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool HasOptions { get; set; }
    public bool HasValidation { get; set; }
}
```

**File: `src/FormDesignerAPI.UseCases/DTOs/CodeGenerationResult.cs`**

```csharp
namespace FormDesignerAPI.UseCases.DTOs;

/// <summary>
/// Result of generating code from a form
/// </summary>
public class CodeGenerationResult
{
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public GeneratedFiles? GeneratedFiles { get; set; }
    public GenerationAnalysis? Analysis { get; set; }
    public string SuggestedNamespace { get; set; } = string.Empty;
}

/// <summary>
/// Container for generated code files
/// </summary>
public class GeneratedFiles
{
    public string ModelCode { get; set; } = string.Empty;
    public string ControllerCode { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string MigrationCode { get; set; } = string.Empty;
}

/// <summary>
/// Analysis of the generated code
/// </summary>
public class GenerationAnalysis
{
    public string DetectedPurpose { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public bool HasRepeatingData { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
}
```

**File: `src/FormDesignerAPI.UseCases/DTOs/BatchProcessingResult.cs`**

```csharp
namespace FormDesignerAPI.UseCases.DTOs;

/// <summary>
/// Result of batch processing multiple forms
/// </summary>
public class BatchProcessingResult
{
    public int TotalForms { get; set; }
    public int SuccessfulForms { get; set; }
    public int FailedForms { get; set; }
    public List<BatchFormResult> Results { get; set; } = new();
}

/// <summary>
/// Result of processing a single form in a batch
/// </summary>
public class BatchFormResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
}
```

#### Step 3.2: Create Use Cases

**File: `src/FormDesignerAPI.UseCases/FormCodeGeneration/AnalyzeFormUseCase.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.UseCases.DTOs;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.UseCases.FormCodeGeneration;

/// <summary>
/// Use case for analyzing a PDF form
/// </summary>
public class AnalyzeFormUseCase
{
    private readonly IFormExtractor _formExtractor;
    private readonly ILogger<AnalyzeFormUseCase> _logger;

    public AnalyzeFormUseCase(
        IFormExtractor formExtractor,
        ILogger<AnalyzeFormUseCase> logger)
    {
        _formExtractor = formExtractor;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a PDF form to extract its structure
    /// </summary>
    public async Task<FormAnalysisResult> ExecuteAsync(
        string pdfPath,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting form analysis: {FileName}", fileName);

        // Detect form type
        var formType = _formExtractor.DetectFormType(pdfPath);
        _logger.LogInformation("Detected form type: {FormType}", formType);

        // Extract structure
        var formStructure = await _formExtractor.ExtractFormStructureAsync(
            pdfPath,
            cancellationToken);

        // Map to DTO
        var result = new FormAnalysisResult
        {
            FileName = fileName,
            FormType = formType.ToString(),
            FieldCount = formStructure.Fields.Count,
            TableCount = formStructure.Tables.Count,
            Fields = formStructure.Fields.Select(f => new FieldSummary
            {
                Name = f.Name,
                Label = f.Label,
                Type = f.FieldType,
                IsRequired = f.IsRequired,
                HasOptions = f.Options?.Any() ?? false,
                HasValidation = !string.IsNullOrEmpty(f.ValidationPattern)
            }).ToList(),
            Warnings = formStructure.Warnings
        };

        _logger.LogInformation(
            "Analysis complete: {FieldCount} fields, {TableCount} tables, {WarningCount} warnings",
            result.FieldCount,
            result.TableCount,
            result.Warnings.Count);

        return result;
    }
}
```

**File: `src/FormDesignerAPI.UseCases/FormCodeGeneration/GenerateCodeUseCase.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.UseCases.DTOs;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.UseCases.FormCodeGeneration;

/// <summary>
/// Use case for generating code from a PDF form
/// </summary>
public class GenerateCodeUseCase
{
    private readonly IFormExtractor _formExtractor;
    private readonly ICodeGenerator _codeGenerator;
    private readonly ILogger<GenerateCodeUseCase> _logger;

    public GenerateCodeUseCase(
        IFormExtractor formExtractor,
        ICodeGenerator codeGenerator,
        ILogger<GenerateCodeUseCase> logger)
    {
        _formExtractor = formExtractor;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Generates code files from a PDF form
    /// </summary>
    public async Task<CodeGenerationResult> ExecuteAsync(
        string pdfPath,
        string formName,
        string? formPurpose = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting code generation for form: {FormName}", formName);

        try
        {
            // Extract form structure
            var formStructure = await _formExtractor.ExtractFormStructureAsync(
                pdfPath,
                cancellationToken);
            
            _logger.LogInformation(
                "Extracted {FieldCount} fields from {FormType} form",
                formStructure.Fields.Count,
                formStructure.FormType);

            // Generate code using AI
            var generatedCode = await _codeGenerator.GenerateCodeFromFormAsync(
                formStructure,
                formName,
                formPurpose,
                cancellationToken);

            // Map to DTO
            var result = new CodeGenerationResult
            {
                FormName = formName,
                Success = true,
                GeneratedFiles = new GeneratedFiles
                {
                    ModelCode = generatedCode.ModelCode,
                    ControllerCode = generatedCode.ControllerCode,
                    ViewCode = generatedCode.ViewCode,
                    MigrationCode = generatedCode.MigrationCode
                },
                Analysis = new GenerationAnalysis
                {
                    DetectedPurpose = generatedCode.FormAnalysis.DetectedPurpose,
                    Complexity = generatedCode.FormAnalysis.Complexity,
                    HasRepeatingData = generatedCode.FormAnalysis.HasRepeatingData,
                    RecommendedApproach = generatedCode.FormAnalysis.RecommendedApproach
                },
                SuggestedNamespace = generatedCode.SuggestedNamespace
            };

            _logger.LogInformation("Code generation successful for {FormName}", formName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate code for form {FormName}", formName);
            throw;
        }
    }
}
```

**File: `src/FormDesignerAPI.UseCases/FormCodeGeneration/BatchProcessUseCase.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.UseCases.DTOs;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.UseCases.FormCodeGeneration;

/// <summary>
/// Use case for batch processing multiple PDF forms
/// </summary>
public class BatchProcessUseCase
{
    private readonly IFormToCodePipeline _pipeline;
    private readonly ILogger<BatchProcessUseCase> _logger;

    public BatchProcessUseCase(
        IFormToCodePipeline pipeline,
        ILogger<BatchProcessUseCase> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    /// <summary>
    /// Processes multiple PDF forms in batch
    /// </summary>
    public async Task<BatchProcessingResult> ExecuteAsync(
        List<(string FilePath, string FileName)> pdfFiles,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting batch processing of {Count} forms", pdfFiles.Count);

        var results = new List<BatchFormResult>();

        foreach (var (filePath, fileName) in pdfFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Batch processing cancelled by user");
                break;
            }

            try
            {
                var formName = Path.GetFileNameWithoutExtension(fileName);
                
                _logger.LogDebug("Processing form: {FormName}", formName);
                
                var generatedCode = await _pipeline.ProcessFormAsync(
                    filePath,
                    formName,
                    outputDirectory,
                    cancellationToken);

                results.Add(new BatchFormResult
                {
                    FileName = fileName,
                    FormName = formName,
                    Success = true,
                    OutputPath = Path.Combine(outputDirectory, formName)
                });

                _logger.LogInformation("Successfully processed: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process form: {FileName}", fileName);
                
                results.Add(new BatchFormResult
                {
                    FileName = fileName,
                    FormName = Path.GetFileNameWithoutExtension(fileName),
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        var summary = new BatchProcessingResult
        {
            TotalForms = results.Count,
            SuccessfulForms = results.Count(r => r.Success),
            FailedForms = results.Count(r => !r.Success),
            Results = results
        };

        _logger.LogInformation(
            "Batch processing complete: {Successful}/{Total} successful",
            summary.SuccessfulForms,
            summary.TotalForms);

        return summary;
    }
}
```

**Build UseCases Project**
```bash
cd src/FormDesignerAPI.UseCases
dotnet build
```

---

### Phase 4: Infrastructure Layer Migration (40 minutes)

This is the most complex phase as it contains the Azure AI implementations.

#### Step 4.1: Configuration Classes

**File: `src/FormDesignerAPI.Infrastructure/Configuration/AzureAIOptions.cs`**

```csharp
namespace FormDesignerAPI.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Azure AI services
/// </summary>
public class AzureAIOptions
{
    public const string SectionName = "Azure:AI";

    public DocumentIntelligenceOptions DocumentIntelligence { get; set; } = new();
    public OpenAIOptions OpenAI { get; set; } = new();
    public FormGenerationOptions FormGeneration { get; set; } = new();
}

/// <summary>
/// Azure Document Intelligence configuration
/// </summary>
public class DocumentIntelligenceOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Azure OpenAI configuration
/// </summary>
public class OpenAIOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4";
    public float Temperature { get; set; } = 0.3f;
    public int MaxTokens { get; set; } = 4000;
}

/// <summary>
/// Form generation configuration
/// </summary>
public class FormGenerationOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public int MaxConcurrentProcessing { get; set; } = 5;
}
```

#### Step 4.2: Create Infrastructure Helpers

**File: `src/FormDesignerAPI.Infrastructure/AI/AzureDocumentIntelligence/FormTypeDetector.cs`**

```csharp
using iText.Kernel.Pdf;
using iText.Forms;
using FormDesignerAPI.Core.Enums;

namespace FormDesignerAPI.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Detects the type of PDF form
/// </summary>
public class FormTypeDetector
{
    /// <summary>
    /// Analyzes a PDF to determine its form type
    /// </summary>
    public PdfFormType DetectFormType(string pdfPath)
    {
        using var pdfDoc = new PdfDocument(new PdfReader(pdfPath));
        var acroForm = PdfAcroForm.GetAcroForm(pdfDoc, false);

        bool hasAcroForm = acroForm != null && acroForm.GetAllFormFields().Count > 0;
        bool hasXFA = acroForm?.GetXfaForm()?.IsXfaPresent() ?? false;

        return (hasXFA, hasAcroForm) switch
        {
            (true, true) => PdfFormType.Hybrid,
            (true, false) => PdfFormType.XFA,
            (false, true) => PdfFormType.AcroForm,
            _ => PdfFormType.Static
        };
    }
}
```

#### Step 4.3: Main Infrastructure Implementation

Due to the length of the full implementations, I'll provide the key structure. You'll use the code from the earlier examples to fill in the implementation details.

**File: `src/FormDesignerAPI.Infrastructure/AI/AzureDocumentIntelligence/EnhancedFormExtractor.cs`**

```csharp
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Enums;
using FormDesignerAPI.Core.Interfaces;
using Microsoft.Extensions.Options;
using FormDesignerAPI.Infrastructure.Configuration;

namespace FormDesignerAPI.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Implementation of IFormExtractor using Azure Document Intelligence
/// </summary>
public class EnhancedFormExtractor : IFormExtractor
{
    private readonly DocumentAnalysisClient _client;
    private readonly FormTypeDetector _formTypeDetector;
    private readonly AzureAIOptions _options;

    public EnhancedFormExtractor(
        IOptions<AzureAIOptions> options,
        FormTypeDetector formTypeDetector)
    {
        _options = options.Value;
        _formTypeDetector = formTypeDetector;
        
        _client = new DocumentAnalysisClient(
            new Uri(_options.DocumentIntelligence.Endpoint),
            new AzureKeyCredential(_options.DocumentIntelligence.ApiKey)
        );
    }

    public PdfFormType DetectFormType(string pdfPath)
    {
        return _formTypeDetector.DetectFormType(pdfPath);
    }

    public async Task<FormStructure> ExtractFormStructureAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        var formType = DetectFormType(pdfPath);
        
        // Use different extraction strategies based on form type
        return formType switch
        {
            PdfFormType.AcroForm => await ExtractAcroFormAsync(pdfPath, cancellationToken),
            PdfFormType.XFA => await ExtractXFAFormAsync(pdfPath, cancellationToken),
            PdfFormType.Hybrid => await ExtractHybridFormAsync(pdfPath, cancellationToken),
            PdfFormType.Static => await ExtractStaticFormAsync(pdfPath, cancellationToken),
            _ => throw new NotSupportedException($"Form type {formType} not supported")
        };
    }
    
    // TODO: Implement extraction methods using code from earlier examples
    private async Task<FormStructure> ExtractAcroFormAsync(string pdfPath, CancellationToken ct)
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
    
    private async Task<FormStructure> ExtractXFAFormAsync(string pdfPath, CancellationToken ct)
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
    
    private async Task<FormStructure> ExtractHybridFormAsync(string pdfPath, CancellationToken ct)
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
    
    private async Task<FormStructure> ExtractStaticFormAsync(string pdfPath, CancellationToken ct)
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
}
```

**File: `src/FormDesignerAPI.Infrastructure/AI/AzureOpenAI/CodeGenerator.cs`**

```csharp
using Azure;
using Azure.AI.OpenAI;
using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Interfaces;
using Microsoft.Extensions.Options;
using FormDesignerAPI.Infrastructure.Configuration;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.AI.AzureOpenAI;

/// <summary>
/// Implementation of ICodeGenerator using Azure OpenAI
/// </summary>
public class CodeGenerator : ICodeGenerator
{
    private readonly OpenAIClient _client;
    private readonly AzureAIOptions _options;

    public CodeGenerator(IOptions<AzureAIOptions> options)
    {
        _options = options.Value;
        
        _client = new OpenAIClient(
            new Uri(_options.OpenAI.Endpoint),
            new AzureKeyCredential(_options.OpenAI.ApiKey)
        );
    }

    public async Task<GeneratedCode> GenerateCodeFromFormAsync(
        FormStructure formStructure,
        string formName,
        string? formPurpose = null,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(formStructure, formName, formPurpose);
        
        var chatOptions = new ChatCompletionsOptions
        {
            DeploymentName = _options.OpenAI.DeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            },
            Temperature = _options.OpenAI.Temperature,
            MaxTokens = _options.OpenAI.MaxTokens,
            ResponseFormat = ChatCompletionsResponseFormat.JsonObject
        };
        
        var response = await _client.GetChatCompletionsAsync(chatOptions, cancellationToken);
        var content = response.Value.Choices[0].Message.Content;
        
        var generatedCode = JsonSerializer.Deserialize<GeneratedCode>(content)
            ?? throw new InvalidOperationException("Failed to deserialize generated code");
        
        return generatedCode;
    }
    
    // TODO: Implement prompt building methods using code from earlier examples
    private string BuildSystemPrompt()
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
    
    private string BuildUserPrompt(FormStructure structure, string name, string? purpose)
    {
        // Implementation goes here
        throw new NotImplementedException();
    }
}
```

**File: `src/FormDesignerAPI.Infrastructure/AI/Pipeline/EnhancedFormToCodePipeline.cs`**

```csharp
using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FormDesignerAPI.Infrastructure.AI.Pipeline;

/// <summary>
/// Implementation of IFormToCodePipeline
/// </summary>
public class EnhancedFormToCodePipeline : IFormToCodePipeline
{
    private readonly IFormExtractor _formExtractor;
    private readonly ICodeGenerator _codeGenerator;
    private readonly ILogger<EnhancedFormToCodePipeline> _logger;

    public EnhancedFormToCodePipeline(
        IFormExtractor formExtractor,
        ICodeGenerator codeGenerator,
        ILogger<EnhancedFormToCodePipeline> logger)
    {
        _formExtractor = formExtractor;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<GeneratedCode> ProcessFormAsync(
        string pdfPath,
        string formName,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing form: {FormName}", formName);

        // Extract structure
        var formStructure = await _formExtractor.ExtractFormStructureAsync(pdfPath, cancellationToken);
        
        // Generate code
        var generatedCode = await _codeGenerator.GenerateCodeFromFormAsync(
            formStructure,
            formName,
            null,
            cancellationToken);
        
        // Save files
        await SaveGeneratedFilesAsync(generatedCode, formName, outputDirectory);
        
        return generatedCode;
    }

    public async Task<BatchProcessResult> ProcessBatchAsync(
        string pdfDirectory,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement batch processing
        throw new NotImplementedException();
    }
    
    private async Task SaveGeneratedFilesAsync(GeneratedCode code, string formName, string outputDir)
    {
        var basePath = Path.Combine(outputDir, formName);
        Directory.CreateDirectory(basePath);
        
        await File.WriteAllTextAsync(
            Path.Combine(basePath, $"{formName}Model.cs"),
            code.ModelCode);
        
        await File.WriteAllTextAsync(
            Path.Combine(basePath, $"{formName}Controller.cs"),
            code.ControllerCode);
        
        // TODO: Save other files
    }
}
```

#### Step 4.4: Dependency Injection Setup

**File: `src/FormDesignerAPI.Infrastructure/Configuration/InfrastructureServiceExtensions.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Infrastructure.AI.AzureDocumentIntelligence;
using FormDesignerAPI.Infrastructure.AI.AzureOpenAI;
using FormDesignerAPI.Infrastructure.AI.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FormDesignerAPI.Infrastructure.Configuration;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Azure AI options
        services.Configure<AzureAIOptions>(
            configuration.GetSection(AzureAIOptions.SectionName));

        // Register helper services
        services.AddScoped<FormTypeDetector>();

        // Register core services
        services.AddScoped<IFormExtractor, EnhancedFormExtractor>();
        services.AddScoped<ICodeGenerator, CodeGenerator>();
        services.AddScoped<IFormToCodePipeline, EnhancedFormToCodePipeline>();

        return services;
    }
}
```

**Build Infrastructure Project**
```bash
cd src/FormDesignerAPI.Infrastructure
dotnet build
```

---

### Phase 5: Web Layer Migration (20 minutes)

#### Step 5.1: Create Controller

**File: `src/FormDesignerAPI.Web/Controllers/FormCodeGenerationController.cs`**

See the full controller implementation from the refactoring guide. Key points:
- Inject the use cases (not services directly)
- Handle file uploads
- Manage temp files properly
- Return appropriate HTTP status codes

#### Step 5.2: Register Services

**File: `src/FormDesignerAPI.Web/Extensions/WebServiceExtensions.cs`**

```csharp
using FormDesignerAPI.UseCases.FormCodeGeneration;

namespace FormDesignerAPI.Web.Extensions;

public static class WebServiceExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        // Register use cases
        services.AddScoped<AnalyzeFormUseCase>();
        services.AddScoped<GenerateCodeUseCase>();
        services.AddScoped<BatchProcessUseCase>();

        return services;
    }
}
```

#### Step 5.3: Update Program.cs

**File: `src/FormDesignerAPI.Web/Program.cs`**

Add the following lines:

```csharp
using FormDesignerAPI.Infrastructure.Configuration;
using FormDesignerAPI.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Existing services...
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// NEW: Add Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// NEW: Add Web services (Use Cases)
builder.Services.AddWebServices();

var app = builder.Build();

// Rest of Program.cs remains the same...
```

#### Step 5.4: Update appsettings.json

**File: `src/FormDesignerAPI.Web/appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-KEY-HERE"
      },
      "OpenAI": {
        "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
        "ApiKey": "YOUR-KEY-HERE",
        "DeploymentName": "gpt-4",
        "Temperature": 0.3,
        "MaxTokens": 4000
      },
      "FormGeneration": {
        "OutputDirectory": "C:\\GeneratedCode",
        "MaxConcurrentProcessing": 5
      }
    }
  }
}
```

**⚠️ IMPORTANT:** Add appsettings.Development.json to .gitignore to protect API keys!

---

### Phase 6: Testing & Verification (30 minutes)

#### Step 6.1: Build Entire Solution

```bash
# From solution root
dotnet build
```

Fix any compilation errors before proceeding.

#### Step 6.2: Run the Application

```bash
cd src/FormDesignerAPI.Web
dotnet run
```

Navigate to: `https://localhost:5001/swagger`

#### Step 6.3: Test Each Endpoint

**Test 1: Analyze Form**
```bash
curl -X POST "https://localhost:5001/api/formcodegeneration/analyze" \
  -H "Content-Type: multipart/form-data" \
  -F "pdfFile=@/path/to/test-form.pdf"
```

**Test 2: Generate Code**
```bash
curl -X POST "https://localhost:5001/api/formcodegeneration/generate" \
  -H "Content-Type: multipart/form-data" \
  -F "pdfFile=@/path/to/test-form.pdf" \
  -F "formName=TestForm" \
  -F "formPurpose=Patient intake form"
```

#### Step 6.4: Verify Generated Code

Check the output directory specified in appsettings.json for generated files.

---

## Troubleshooting

### Common Issues

**Issue 1: Azure Authentication Errors**
```
Error: Unauthorized (401)
```
**Solution:** Verify your API keys in appsettings.json

**Issue 2: PDF Parsing Errors**
```
Error: Unable to read PDF
```
**Solution:** Ensure itext7 is properly installed and PDF is not corrupted

**Issue 3: JSON Deserialization Errors**
```
Error: Failed to deserialize generated code
```
**Solution:** Check Azure OpenAI prompt and ensure it returns valid JSON

**Issue 4: File Permission Errors**
```
Error: Access denied to output directory
```
**Solution:** Ensure the application has write permissions to the output directory

---

## Post-Migration Checklist

- [ ] All projects build without errors
- [ ] Application runs successfully
- [ ] Swagger UI is accessible
- [ ] Can analyze a test PDF form
- [ ] Can generate code from a test PDF form
- [ ] Generated code is valid C#
- [ ] Batch processing works for multiple files
- [ ] Logging is working correctly
- [ ] Error handling works as expected

---

## Next Steps

1. **Add Unit Tests** - Test use cases in isolation
2. **Add Integration Tests** - Test the full pipeline
3. **Performance Optimization** - Add caching, parallel processing
4. **Enhanced Error Handling** - Add retry policies, circuit breakers
5. **Monitoring** - Add Application Insights or similar
6. **Documentation** - Add XML comments and API documentation

---

## Additional Resources

- [Azure Document Intelligence Docs](https://learn.microsoft.com/azure/ai-services/document-intelligence/)
- [Azure OpenAI Docs](https://learn.microsoft.com/azure/ai-services/openai/)
- [Clean Architecture Guide](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

---

## Getting Help

If you encounter issues during migration:

1. Check the build output for specific error messages
2. Verify all NuGet packages are properly restored
3. Ensure Azure credentials are correct
4. Review the logs for detailed error information
5. Test each layer independently to isolate issues

Good luck with your migration! 🚀
