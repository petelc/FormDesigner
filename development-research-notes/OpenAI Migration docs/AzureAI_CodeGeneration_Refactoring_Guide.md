# Azure AI Code Generation - Clean Architecture Refactoring Guide

## Overview
This guide shows how to integrate the Azure AI-powered form-to-code generation features into your existing Clean Architecture solution.

## Solution Structure

```
FormDesignerAPI/
├── src/
│   ├── FormDesignerAPI.Core/              # Domain layer
│   │   ├── Entities/
│   │   │   ├── FormStructure.cs           # NEW
│   │   │   ├── FormField.cs               # NEW
│   │   │   ├── FormTable.cs               # NEW
│   │   │   └── GeneratedCode.cs           # NEW
│   │   ├── Enums/
│   │   │   └── PdfFormType.cs             # NEW
│   │   └── Interfaces/
│   │       ├── IFormExtractor.cs          # NEW
│   │       ├── ICodeGenerator.cs          # NEW
│   │       └── IFormToCodePipeline.cs     # NEW
│   │
│   ├── FormDesignerAPI.UseCases/          # Application layer
│   │   ├── FormCodeGeneration/            # NEW folder
│   │   │   ├── AnalyzeFormUseCase.cs
│   │   │   ├── GenerateCodeUseCase.cs
│   │   │   ├── GenerateAndDownloadUseCase.cs
│   │   │   └── BatchProcessUseCase.cs
│   │   └── DTOs/                          # NEW folder
│   │       ├── FormAnalysisResult.cs
│   │       ├── CodeGenerationResult.cs
│   │       └── BatchProcessingResult.cs
│   │
│   ├── FormDesignerAPI.Infrastructure/    # Infrastructure layer
│   │   ├── AI/                            # NEW folder
│   │   │   ├── AzureDocumentIntelligence/
│   │   │   │   ├── EnhancedFormExtractor.cs
│   │   │   │   ├── AcroFormExtractor.cs
│   │   │   │   ├── XFAFormExtractor.cs
│   │   │   │   └── FormTypeDetector.cs
│   │   │   ├── AzureOpenAI/
│   │   │   │   ├── CodeGenerator.cs
│   │   │   │   └── PromptBuilder.cs
│   │   │   └── Pipeline/
│   │   │       └── EnhancedFormToCodePipeline.cs
│   │   └── Configuration/
│   │       ├── AzureAIOptions.cs          # NEW
│   │       └── InfrastructureServiceExtensions.cs
│   │
│   └── FormDesignerAPI.Web/               # Presentation layer
│       ├── Controllers/
│       │   └── FormCodeGenerationController.cs  # NEW
│       ├── Extensions/
│       │   └── WebServiceExtensions.cs
│       └── appsettings.json               # Update with Azure configs
│
└── tests/
    └── FormDesignerAPI.UnitTests/
        └── FormCodeGeneration/            # NEW folder
            ├── AnalyzeFormUseCaseTests.cs
            ├── GenerateCodeUseCaseTests.cs
            └── FormExtractorTests.cs
```

---

## Implementation Steps

### STEP 1: Update FormDesignerAPI.Core

Add new domain entities and interfaces.

#### 1.1 Create Enums

**File: `src/FormDesignerAPI.Core/Enums/PdfFormType.cs`**

```csharp
namespace FormDesignerAPI.Core.Enums;

/// <summary>
/// Represents the type of PDF form
/// </summary>
public enum PdfFormType
{
    /// <summary>
    /// Static PDF with embedded form fields
    /// </summary>
    AcroForm,
    
    /// <summary>
    /// XML-based dynamic forms
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

#### 1.2 Create Domain Entities

**File: `src/FormDesignerAPI.Core/Entities/FormField.cs`**

```csharp
namespace FormDesignerAPI.Core.Entities;

/// <summary>
/// Represents a single field in a form
/// </summary>
public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
    public string? ValidationPattern { get; set; }
    public Dictionary<string, string>? XFAProperties { get; set; }
    public FieldPosition? Position { get; set; }
    public IReadOnlyList<System.Drawing.PointF>? BoundingBox { get; set; }
}

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
/// Represents a table in a form
/// </summary>
public class FormTable
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<TableCell> Cells { get; set; } = new();
}

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
    public PdfFormType FormType { get; set; }
    public List<FormField> Fields { get; set; } = new();
    public List<FormTable> Tables { get; set; } = new();
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
    public string ModelCode { get; set; } = string.Empty;
    public string ControllerCode { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string MigrationCode { get; set; } = string.Empty;
    public string SuggestedNamespace { get; set; } = string.Empty;
    public FormAnalysis FormAnalysis { get; set; } = new();
}

public class FormAnalysis
{
    public string DetectedPurpose { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public bool HasRepeatingData { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
}
```

#### 1.3 Create Interfaces

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
    /// Detects the type of PDF form
    /// </summary>
    PdfFormType DetectFormType(string pdfPath);
    
    /// <summary>
    /// Extracts the complete structure from a PDF form
    /// </summary>
    Task<FormStructure> ExtractFormStructureAsync(string pdfPath, CancellationToken cancellationToken = default);
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
    Task<GeneratedCode> ProcessFormAsync(
        string pdfPath,
        string formName,
        string outputDirectory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes multiple forms in batch
    /// </summary>
    Task<BatchProcessResult> ProcessBatchAsync(
        string pdfDirectory,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}

public class BatchProcessResult
{
    public int TotalForms { get; set; }
    public int SuccessfulForms { get; set; }
    public int FailedForms { get; set; }
    public List<FormProcessResult> Results { get; set; } = new();
}

public class FormProcessResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
}
```

---

### STEP 2: Update FormDesignerAPI.UseCases

Create use cases that orchestrate the business logic.

#### 2.1 Create DTOs

**File: `src/FormDesignerAPI.UseCases/DTOs/FormAnalysisResult.cs`**

```csharp
namespace FormDesignerAPI.UseCases.DTOs;

public class FormAnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public int TableCount { get; set; }
    public List<FieldSummary> Fields { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

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

public class CodeGenerationResult
{
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public GeneratedFiles? GeneratedFiles { get; set; }
    public GenerationAnalysis? Analysis { get; set; }
    public string SuggestedNamespace { get; set; } = string.Empty;
}

public class GeneratedFiles
{
    public string ModelCode { get; set; } = string.Empty;
    public string ControllerCode { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string MigrationCode { get; set; } = string.Empty;
}

public class GenerationAnalysis
{
    public string DetectedPurpose { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public bool HasRepeatingData { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
}
```

#### 2.2 Create Use Cases

**File: `src/FormDesignerAPI.UseCases/FormCodeGeneration/AnalyzeFormUseCase.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.UseCases.DTOs;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.UseCases.FormCodeGeneration;

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

    public async Task<FormAnalysisResult> ExecuteAsync(
        string pdfPath,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing form: {FileName}", fileName);

        // Detect form type
        var formType = _formExtractor.DetectFormType(pdfPath);
        _logger.LogInformation("Detected form type: {FormType}", formType);

        // Extract structure
        var formStructure = await _formExtractor.ExtractFormStructureAsync(pdfPath, cancellationToken);

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
            "Analysis complete: {FieldCount} fields, {TableCount} tables",
            result.FieldCount,
            result.TableCount);

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

    public async Task<CodeGenerationResult> ExecuteAsync(
        string pdfPath,
        string formName,
        string? formPurpose = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating code for form: {FormName}", formName);

        try
        {
            // Extract form structure
            var formStructure = await _formExtractor.ExtractFormStructureAsync(pdfPath, cancellationToken);
            
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
                _logger.LogWarning("Batch processing cancelled");
                break;
            }

            try
            {
                var formName = Path.GetFileNameWithoutExtension(fileName);
                
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

public class BatchProcessingResult
{
    public int TotalForms { get; set; }
    public int SuccessfulForms { get; set; }
    public int FailedForms { get; set; }
    public List<BatchFormResult> Results { get; set; } = new();
}

public class BatchFormResult
{
    public string FileName { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public string? Error { get; set; }
}
```

---

### STEP 3: Update FormDesignerAPI.Infrastructure

Implement the Azure AI services.

#### 3.1 Configuration

**File: `src/FormDesignerAPI.Infrastructure/Configuration/AzureAIOptions.cs`**

```csharp
namespace FormDesignerAPI.Infrastructure.Configuration;

public class AzureAIOptions
{
    public const string SectionName = "Azure:AI";

    public DocumentIntelligenceOptions DocumentIntelligence { get; set; } = new();
    public OpenAIOptions OpenAI { get; set; } = new();
    public FormGenerationOptions FormGeneration { get; set; } = new();
}

public class DocumentIntelligenceOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class OpenAIOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4";
    public float Temperature { get; set; } = 0.3f;
    public int MaxTokens { get; set; } = 4000;
}

public class FormGenerationOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public int MaxConcurrentProcessing { get; set; } = 5;
}
```

#### 3.2 Service Implementations

**File: `src/FormDesignerAPI.Infrastructure/AI/AzureDocumentIntelligence/FormTypeDetector.cs`**

```csharp
using iText.Kernel.Pdf;
using iText.Forms;
using FormDesignerAPI.Core.Enums;

namespace FormDesignerAPI.Infrastructure.AI.AzureDocumentIntelligence;

public class FormTypeDetector
{
    public PdfFormType DetectFormType(string pdfPath)
    {
        using var pdfDoc = new PdfDocument(new PdfReader(pdfPath));
        var acroForm = PdfAcroForm.GetAcroForm(pdfDoc, false);

        bool hasAcroForm = acroForm != null && acroForm.GetAllFormFields().Count > 0;
        bool hasXFA = acroForm?.GetXfaForm()?.IsXfaPresent() ?? false;

        if (hasXFA && hasAcroForm)
            return PdfFormType.Hybrid;
        else if (hasXFA)
            return PdfFormType.XFA;
        else if (hasAcroForm)
            return PdfFormType.AcroForm;
        else
            return PdfFormType.Static;
    }
}
```

*Note: Continue this pattern for the other infrastructure files. Due to length constraints, I'll provide the key structure and you can expand each file based on the earlier code examples.*

#### 3.3 Dependency Injection

**File: `src/FormDesignerAPI.Infrastructure/Configuration/InfrastructureServiceExtensions.cs`**

```csharp
using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Infrastructure.AI.AzureDocumentIntelligence;
using FormDesignerAPI.Infrastructure.AI.AzureOpenAI;
using FormDesignerAPI.Infrastructure.AI.Pipeline;
using FormDesignerAPI.Infrastructure.Configuration;
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

        // Register form extraction services
        services.AddScoped<FormTypeDetector>();
        services.AddScoped<IFormExtractor, EnhancedFormExtractor>();

        // Register code generation services
        services.AddScoped<ICodeGenerator, CodeGenerator>();

        // Register pipeline orchestrator
        services.AddScoped<IFormToCodePipeline, EnhancedFormToCodePipeline>();

        return services;
    }
}
```

---

### STEP 4: Update FormDesignerAPI.Web

Add the API endpoints.

**File: `src/FormDesignerAPI.Web/Controllers/FormCodeGenerationController.cs`**

```csharp
using FormDesignerAPI.UseCases.FormCodeGeneration;
using FormDesignerAPI.UseCases.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FormDesignerAPI.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormCodeGenerationController : ControllerBase
{
    private readonly AnalyzeFormUseCase _analyzeFormUseCase;
    private readonly GenerateCodeUseCase _generateCodeUseCase;
    private readonly BatchProcessUseCase _batchProcessUseCase;
    private readonly ILogger<FormCodeGenerationController> _logger;

    public FormCodeGenerationController(
        AnalyzeFormUseCase analyzeFormUseCase,
        GenerateCodeUseCase generateCodeUseCase,
        BatchProcessUseCase batchProcessUseCase,
        ILogger<FormCodeGenerationController> logger)
    {
        _analyzeFormUseCase = analyzeFormUseCase;
        _generateCodeUseCase = generateCodeUseCase;
        _batchProcessUseCase = batchProcessUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Analyze a PDF form to detect its type and extract structure
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(FormAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeForm(
        IFormFile pdfFile,
        CancellationToken cancellationToken)
    {
        if (!IsValidPdfFile(pdfFile))
        {
            return BadRequest("Invalid PDF file");
        }

        var tempPath = await SaveTempFileAsync(pdfFile, cancellationToken);

        try
        {
            var result = await _analyzeFormUseCase.ExecuteAsync(
                tempPath,
                pdfFile.FileName,
                cancellationToken);

            return Ok(result);
        }
        finally
        {
            CleanupTempFile(tempPath);
        }
    }

    /// <summary>
    /// Generate code files from a PDF form
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(CodeGenerationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCode(
        IFormFile pdfFile,
        [FromForm] string formName,
        [FromForm] string? formPurpose = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidPdfFile(pdfFile) || string.IsNullOrWhiteSpace(formName))
        {
            return BadRequest("Invalid input");
        }

        var tempPath = await SaveTempFileAsync(pdfFile, cancellationToken);

        try
        {
            var result = await _generateCodeUseCase.ExecuteAsync(
                tempPath,
                formName,
                formPurpose,
                cancellationToken);

            return Ok(result);
        }
        finally
        {
            CleanupTempFile(tempPath);
        }
    }

    /// <summary>
    /// Batch process multiple PDF forms
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchProcessingResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchGenerate(
        List<IFormFile> pdfFiles,
        CancellationToken cancellationToken)
    {
        if (pdfFiles == null || !pdfFiles.Any())
        {
            return BadRequest("No PDF files provided");
        }

        var tempFiles = new List<(string FilePath, string FileName)>();

        try
        {
            // Save all files temporarily
            foreach (var file in pdfFiles)
            {
                if (IsValidPdfFile(file))
                {
                    var tempPath = await SaveTempFileAsync(file, cancellationToken);
                    tempFiles.Add((tempPath, file.FileName));
                }
            }

            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);

            var result = await _batchProcessUseCase.ExecuteAsync(
                tempFiles,
                outputDir,
                cancellationToken);

            return Ok(result);
        }
        finally
        {
            // Cleanup temp files
            foreach (var (filePath, _) in tempFiles)
            {
                CleanupTempFile(filePath);
            }
        }
    }

    private bool IsValidPdfFile(IFormFile? file)
    {
        return file != null
            && file.Length > 0
            && file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> SaveTempFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var tempPath = Path.GetTempFileName();
        using var stream = new FileStream(tempPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);
        return tempPath;
    }

    private void CleanupTempFile(string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup temp file: {Path}", path);
        }
    }
}
```

**File: `src/FormDesignerAPI.Web/Extensions/WebServiceExtensions.cs`**

```csharp
using FormDesignerAPI.UseCases.FormCodeGeneration;
using Microsoft.Extensions.DependencyInjection;

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

**File: `src/FormDesignerAPI.Web/Program.cs`** (Update)

```csharp
using FormDesignerAPI.Infrastructure.Configuration;
using FormDesignerAPI.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure services (Azure AI, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Web services (Use Cases, etc.)
builder.Services.AddWebServices();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

### STEP 5: Update appsettings.json

**File: `src/FormDesignerAPI.Web/appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "FormDesignerAPI": "Debug"
    }
  },
  "AllowedHosts": "*",
  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "https://your-doc-intel.cognitiveservices.azure.com/",
        "ApiKey": "your-key-here"
      },
      "OpenAI": {
        "Endpoint": "https://your-openai.openai.azure.com/",
        "ApiKey": "your-key-here",
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

---

### STEP 6: Update Project References

Update each `.csproj` file to add necessary NuGet packages:

**`FormDesignerAPI.Infrastructure.csproj`**

```xml
<ItemGroup>
  <PackageReference Include="Azure.AI.FormRecognizer" Version="4.1.0" />
  <PackageReference Include="Azure.AI.OpenAI" Version="1.0.0-beta.12" />
  <PackageReference Include="itext7" Version="8.0.2" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
</ItemGroup>
```

**`FormDesignerAPI.UseCases.csproj`**

```xml
<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
</ItemGroup>
```

**`FormDesignerAPI.Web.csproj`**

```xml
<ItemGroup>
  <ProjectReference Include="..\FormDesignerAPI.Core\FormDesignerAPI.Core.csproj" />
  <ProjectReference Include="..\FormDesignerAPI.UseCases\FormDesignerAPI.UseCases.csproj" />
  <ProjectReference Include="..\FormDesignerAPI.Infrastructure\FormDesignerAPI.Infrastructure.csproj" />
</ItemGroup>
```

---

## Benefits of This Clean Architecture Approach

✅ **Separation of Concerns** - Each layer has a clear responsibility
✅ **Testability** - Use cases can be unit tested independently
✅ **Maintainability** - Changes to Azure APIs only affect Infrastructure layer
✅ **Scalability** - Easy to add new form types or AI providers
✅ **Dependency Inversion** - Core doesn't depend on external services
✅ **Reusability** - Use cases can be called from multiple controllers or background jobs

## Next Steps

1. Copy the entity/interface files to FormDesignerAPI.Core
2. Copy the use case files to FormDesignerAPI.UseCases
3. Copy the infrastructure implementation files to FormDesignerAPI.Infrastructure
4. Update the Web project with the new controller
5. Update appsettings.json with your Azure credentials
6. Run and test!

Would you like me to generate any specific implementation files in full detail?
