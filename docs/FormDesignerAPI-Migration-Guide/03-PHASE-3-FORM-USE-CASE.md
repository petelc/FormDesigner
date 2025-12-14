# Phase 3: USE CASES

**Duration:** TBD  
**Complexity:** Medium-High  
**Prerequisites:** Previous phases complete

## Overview

The Application layer implements CQRS (Command Query Responsibility Segregation) using MediatR. It orchestrates domain logic and coordinates with infrastructure services without containing business rules.

## Objectives

1. [Commands](#commands)
   - AnalyzeFormCommand
   - GenerateCodeCommand
   - ProcessBatchCommand
2. [Queries](#queries)
   - GetFormAnalysisQuery
   - GetGeneratedCodeQuery
   - GetBatchStatusQuery
3. [DTOs](#dtos)
4. [Behaviors](#behaviors)
5. [Mappings](#mappings)
6. [Validators](#validators)

## Steps

---

## Commands

Commands represent write operations that change system state.

### 1. AnalyzeFormCommand

Analyzes a PDF form and extracts its structure.

#### AnalyzeFormCommand.cs
```csharp
using MediatR;

namespace FormCodeGenerator.Application.Commands.AnalyzeForm;

/// <summary>
/// Command to analyze a PDF form
/// </summary>
public record AnalyzeFormCommand(
    Stream PdfStream,
    string FileName
) : IRequest<AnalyzeFormResult>;
```

#### AnalyzeFormResult.cs
```csharp
namespace FormCodeGenerator.Application.Commands.AnalyzeForm;

/// <summary>
/// Result of form analysis
/// </summary>
public class AnalyzeFormResult
{
    public Guid FormId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FormType { get; init; } = string.Empty;
    public int FieldCount { get; init; }
    public int TableCount { get; init; }
    public List<FieldSummaryDto> Fields { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool RequiresManualReview { get; init; }
    public DateTime AnalyzedAt { get; init; }
}

public class FieldSummaryDto
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool HasOptions { get; init; }
    public bool HasValidation { get; init; }
    public int? MaxLength { get; init; }
}
```

#### AnalyzeFormCommandHandler.cs
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.Interfaces;
using FormCodeGenerator.Domain.ValueObjects;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Application.Commands.AnalyzeForm;

/// <summary>
/// Handler for AnalyzeFormCommand
/// </summary>
public class AnalyzeFormCommandHandler : IRequestHandler<AnalyzeFormCommand, AnalyzeFormResult>
{
    private readonly IFormExtractor _formExtractor;
    private readonly IFormRepository _formRepository;
    private readonly ILogger<AnalyzeFormCommandHandler> _logger;

    public AnalyzeFormCommandHandler(
        IFormExtractor formExtractor,
        IFormRepository formRepository,
        ILogger<AnalyzeFormCommandHandler> logger)
    {
        _formExtractor = formExtractor;
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<AnalyzeFormResult> Handle(
        AnalyzeFormCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analyzing form: {FileName}", request.FileName);

        // Save PDF to temp location for processing
        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await request.PdfStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Detect form type
            var formTypeValue = await _formExtractor.DetectFormTypeAsync(tempPath, cancellationToken);
            var formType = FormType.FromString(formTypeValue);

            _logger.LogInformation("Detected form type: {FormType}", formType.Value);

            // Create Form aggregate
            var form = new Form(request.FileName, formType);

            // Extract structure
            var extractedStructure = await _formExtractor.ExtractFormStructureAsync(
                tempPath,
                formType,
                cancellationToken);

            // Add fields to aggregate
            foreach (var fieldData in extractedStructure.Fields)
            {
                var fieldType = FieldType.FromString(fieldData.Type);
                var field = new FormField(fieldData.Name, fieldData.Label, fieldType, fieldData.IsRequired);

                if (fieldData.MaxLength.HasValue)
                    field.SetMaxLength(fieldData.MaxLength.Value);

                if (!string.IsNullOrEmpty(fieldData.DefaultValue))
                    field.SetDefaultValue(fieldData.DefaultValue);

                if (fieldData.Options != null)
                {
                    foreach (var option in fieldData.Options)
                        field.AddOption(option);
                }

                if (!string.IsNullOrEmpty(fieldData.ValidationPattern))
                {
                    var validationRule = new ValidationRule(
                        fieldData.ValidationPattern,
                        "Validation failed",
                        fieldData.IsRequired);
                    field.SetValidationRule(validationRule);
                }

                form.AddField(field);
            }

            // Add tables to aggregate
            foreach (var tableData in extractedStructure.Tables)
            {
                var table = new FormTable(tableData.RowCount, tableData.ColumnCount);

                foreach (var cellData in tableData.Cells)
                {
                    var cell = new TableCell(
                        cellData.Content,
                        cellData.RowIndex,
                        cellData.ColumnIndex,
                        cellData.IsHeader);
                    table.AddCell(cell);
                }

                form.AddTable(table);
            }

            // Add warnings
            foreach (var warning in extractedStructure.Warnings)
            {
                form.AddWarning(warning);
            }

            // Mark as analyzed (raises domain event)
            form.MarkAsAnalyzed();

            // Persist to repository
            await _formRepository.AddAsync(form, cancellationToken);

            _logger.LogInformation(
                "Form analysis complete: {FormId}, {FieldCount} fields, {TableCount} tables",
                form.Id,
                form.Fields.Count,
                form.Tables.Count);

            // Map to result DTO
            return new AnalyzeFormResult
            {
                FormId = form.Id,
                FileName = form.FileName,
                FormType = form.FormType.Value,
                FieldCount = form.Fields.Count,
                TableCount = form.Tables.Count,
                Fields = form.Fields.Select(f => new FieldSummaryDto
                {
                    Name = f.Name,
                    Label = f.Label,
                    Type = f.FieldType.Value,
                    IsRequired = f.IsRequired,
                    HasOptions = f.Options.Any(),
                    HasValidation = f.ValidationRule != null,
                    MaxLength = f.MaxLength
                }).ToList(),
                Warnings = form.Warnings.ToList(),
                RequiresManualReview = form.RequiresManualReview(),
                AnalyzedAt = form.AnalyzedAt!.Value
            };
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
```

#### AnalyzeFormCommandValidator.cs
```csharp
using FluentValidation;

namespace FormCodeGenerator.Application.Commands.AnalyzeForm;

/// <summary>
/// Validator for AnalyzeFormCommand
/// </summary>
public class AnalyzeFormCommandValidator : AbstractValidator<AnalyzeFormCommand>
{
    public AnalyzeFormCommandValidator()
    {
        RuleFor(x => x.PdfStream)
            .NotNull()
            .WithMessage("PDF stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(BeAPdfFile)
            .WithMessage("File must be a PDF (.pdf extension)");

        RuleFor(x => x.PdfStream)
            .Must(HaveValidSize)
            .When(x => x.PdfStream != null)
            .WithMessage("PDF file size must be between 1 KB and 50 MB");
    }

    private bool BeAPdfFile(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private bool HaveValidSize(Stream stream)
    {
        const long minSize = 1024; // 1 KB
        const long maxSize = 50 * 1024 * 1024; // 50 MB

        return stream.Length >= minSize && stream.Length <= maxSize;
    }
}
```

---

### 2. GenerateCodeCommand

Generates C# code from an analyzed form.

#### GenerateCodeCommand.cs
```csharp
using MediatR;

namespace FormCodeGenerator.Application.Commands.GenerateCode;

/// <summary>
/// Command to generate code from a form
/// </summary>
public record GenerateCodeCommand(
    Guid FormId,
    string FormName,
    string? FormPurpose = null,
    string? TargetNamespace = null
) : IRequest<GenerateCodeResult>;
```

#### GenerateCodeResult.cs
```csharp
namespace FormCodeGenerator.Application.Commands.GenerateCode;

/// <summary>
/// Result of code generation
/// </summary>
public class GenerateCodeResult
{
    public Guid RequestId { get; init; }
    public Guid FormId { get; init; }
    public string FormName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public GeneratedFilesDto GeneratedFiles { get; init; } = new();
    public CodeAnalysisDto Analysis { get; init; } = new();
    public string SuggestedNamespace { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
}

public class GeneratedFilesDto
{
    public string ModelCode { get; init; } = string.Empty;
    public string ControllerCode { get; init; } = string.Empty;
    public string ViewCode { get; init; } = string.Empty;
    public string MigrationCode { get; init; } = string.Empty;
}

public class CodeAnalysisDto
{
    public string DetectedPurpose { get; init; } = string.Empty;
    public string Complexity { get; init; } = string.Empty;
    public bool HasRepeatingData { get; init; }
    public string RecommendedApproach { get; init; } = string.Empty;
}
```

#### GenerateCodeCommandHandler.cs
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;
using FormCodeGenerator.Domain.Interfaces;
using FormCodeGenerator.Domain.ValueObjects;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Application.Commands.GenerateCode;

/// <summary>
/// Handler for GenerateCodeCommand
/// </summary>
public class GenerateCodeCommandHandler : IRequestHandler<GenerateCodeCommand, GenerateCodeResult>
{
    private readonly IFormRepository _formRepository;
    private readonly ICodeGenerationRepository _codeGenerationRepository;
    private readonly ICodeGenerator _codeGenerator;
    private readonly ILogger<GenerateCodeCommandHandler> _logger;

    public GenerateCodeCommandHandler(
        IFormRepository formRepository,
        ICodeGenerationRepository codeGenerationRepository,
        ICodeGenerator codeGenerator,
        ILogger<GenerateCodeCommandHandler> logger)
    {
        _formRepository = formRepository;
        _codeGenerationRepository = codeGenerationRepository;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<GenerateCodeResult> Handle(
        GenerateCodeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating code for form: {FormId}, FormName: {FormName}",
            request.FormId,
            request.FormName);

        // Get the form aggregate
        var form = await _formRepository.GetByIdAsync(request.FormId, cancellationToken);
        if (form == null)
        {
            throw new InvalidOperationException($"Form not found: {request.FormId}");
        }

        if (!form.AnalyzedAt.HasValue)
        {
            throw new InvalidOperationException("Form must be analyzed before code generation");
        }

        // Determine namespace
        var targetNamespace = string.IsNullOrEmpty(request.TargetNamespace)
            ? new CodeNamespace($"MyApp.{request.FormName}")
            : new CodeNamespace(request.TargetNamespace);

        // Create code generation request aggregate
        var codeGenRequest = new CodeGenerationRequest(
            form.Id,
            request.FormName,
            request.FormPurpose,
            targetNamespace);

        try
        {
            // Generate code using infrastructure service
            var generatedCode = await _codeGenerator.GenerateCodeAsync(
                form,
                request.FormName,
                targetNamespace,
                request.FormPurpose,
                cancellationToken);

            // Add generated code to aggregate
            codeGenRequest.AddGeneratedCode(
                "Model",
                generatedCode.ModelCode,
                "C#");

            codeGenRequest.AddGeneratedCode(
                "Controller",
                generatedCode.ControllerCode,
                "C#");

            codeGenRequest.AddGeneratedCode(
                "View",
                generatedCode.ViewCode,
                "Razor");

            codeGenRequest.AddGeneratedCode(
                "Migration",
                generatedCode.MigrationCode,
                "C#");

            // Set analysis
            var analysis = new CodeAnalysis(
                generatedCode.Analysis.DetectedPurpose,
                generatedCode.Analysis.Complexity,
                generatedCode.Analysis.HasRepeatingData,
                generatedCode.Analysis.RecommendedApproach);

            codeGenRequest.SetAnalysis(analysis);

            // Mark as completed (raises domain event)
            codeGenRequest.MarkAsCompleted();

            // Persist
            await _codeGenerationRepository.AddAsync(codeGenRequest, cancellationToken);

            _logger.LogInformation(
                "Code generation complete: RequestId={RequestId}, FormId={FormId}",
                codeGenRequest.Id,
                form.Id);

            // Map to result DTO
            return new GenerateCodeResult
            {
                RequestId = codeGenRequest.Id,
                FormId = form.Id,
                FormName = request.FormName,
                Success = true,
                GeneratedFiles = new GeneratedFilesDto
                {
                    ModelCode = generatedCode.ModelCode,
                    ControllerCode = generatedCode.ControllerCode,
                    ViewCode = generatedCode.ViewCode,
                    MigrationCode = generatedCode.MigrationCode
                },
                Analysis = new CodeAnalysisDto
                {
                    DetectedPurpose = analysis.DetectedPurpose,
                    Complexity = analysis.Complexity,
                    HasRepeatingData = analysis.HasRepeatingData,
                    RecommendedApproach = analysis.RecommendedApproach
                },
                SuggestedNamespace = targetNamespace.Value,
                GeneratedAt = codeGenRequest.CompletedAt!.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code generation failed for form {FormId}", form.Id);
            
            codeGenRequest.MarkAsFailed(ex.Message);
            await _codeGenerationRepository.AddAsync(codeGenRequest, cancellationToken);
            
            throw;
        }
    }
}
```

#### GenerateCodeCommandValidator.cs

```csharp
using FluentValidation;

namespace FormCodeGenerator.Application.Commands.GenerateCode;

/// <summary>
/// Validator for GenerateCodeCommand
/// </summary>
public class GenerateCodeCommandValidator : AbstractValidator<GenerateCodeCommand>
{
    public GenerateCodeCommandValidator()
    {
        RuleFor(x => x.FormId)
            .NotEmpty()
            .WithMessage("Form ID is required");

        RuleFor(x => x.FormName)
            .NotEmpty()
            .WithMessage("Form name is required")
            .MaximumLength(100)
            .WithMessage("Form name must not exceed 100 characters")
            .Must(BeValidCSharpIdentifier)
            .WithMessage("Form name must be a valid C# identifier");

        RuleFor(x => x.TargetNamespace)
            .Must(BeValidNamespace)
            .When(x => !string.IsNullOrEmpty(x.TargetNamespace))
            .WithMessage("Target namespace must be a valid C# namespace");
    }

    private bool BeValidCSharpIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (!char.IsLetter(name[0]) && name[0] != '_') return false;
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private bool BeValidNamespace(string? ns)
    {
        if (string.IsNullOrWhiteSpace(ns)) return false;

        var parts = ns.Split('.');
        return parts.All(part =>
            !string.IsNullOrWhiteSpace(part) &&
            char.IsLetter(part[0]) &&
            part.All(c => char.IsLetterOrDigit(c) || c == '_'));
    }
}
```

---

### 3. ProcessBatchCommand

Processes multiple forms in batch.

#### ProcessBatchCommand.cs
```csharp
using MediatR;

namespace FormCodeGenerator.Application.Commands.ProcessBatch;

/// <summary>
/// Command to process multiple forms in batch
/// </summary>
public record ProcessBatchCommand(
    List<BatchFormInput> Forms,
    string? TargetNamespace = null
) : IRequest<ProcessBatchResult>;

public record BatchFormInput(
    Stream PdfStream,
    string FileName,
    string FormName);
```

#### ProcessBatchResult.cs
```csharp
namespace FormCodeGenerator.Application.Commands.ProcessBatch;

/// <summary>
/// Result of batch processing
/// </summary>
public class ProcessBatchResult
{
    public Guid BatchId { get; init; }
    public int TotalForms { get; init; }
    public int SuccessfulForms { get; init; }
    public int FailedForms { get; init; }
    public List<BatchFormResultDto> Results { get; init; } = new();
    public TimeSpan ProcessingTime { get; init; }
    public DateTime CompletedAt { get; init; }
}

public class BatchFormResultDto
{
    public string FileName { get; init; } = string.Empty;
    public string FormName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public Guid? FormId { get; init; }
    public Guid? RequestId { get; init; }
    public string? Error { get; init; }
}
```

#### ProcessBatchCommandHandler.cs

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FormCodeGenerator.Application.Commands.ProcessBatch;

/// <summary>
/// Handler for ProcessBatchCommand
/// </summary>
public class ProcessBatchCommandHandler : IRequestHandler<ProcessBatchCommand, ProcessBatchResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessBatchCommandHandler> _logger;

    public ProcessBatchCommandHandler(
        IMediator mediator,
        ILogger<ProcessBatchCommandHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ProcessBatchResult> Handle(
        ProcessBatchCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting batch processing of {Count} forms", request.Forms.Count);

        var batchId = Guid.NewGuid();
        var results = new List<BatchFormResultDto>();
        var stopwatch = Stopwatch.StartNew();

        foreach (var formInput in request.Forms)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Batch processing cancelled");
                break;
            }

            try
            {
                _logger.LogDebug("Processing form: {FileName}", formInput.FileName);

                // Step 1: Analyze form
                var analyzeCommand = new AnalyzeForm.AnalyzeFormCommand(
                    formInput.PdfStream,
                    formInput.FileName);

                var analyzeResult = await _mediator.Send(analyzeCommand, cancellationToken);

                // Step 2: Generate code
                var generateCommand = new GenerateCode.GenerateCodeCommand(
                    analyzeResult.FormId,
                    formInput.FormName,
                    null,
                    request.TargetNamespace);

                var generateResult = await _mediator.Send(generateCommand, cancellationToken);

                results.Add(new BatchFormResultDto
                {
                    FileName = formInput.FileName,
                    FormName = formInput.FormName,
                    Success = true,
                    FormId = analyzeResult.FormId,
                    RequestId = generateResult.RequestId
                });

                _logger.LogInformation("✓ Successfully processed: {FileName}", formInput.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Failed to process: {FileName}", formInput.FileName);

                results.Add(new BatchFormResultDto
                {
                    FileName = formInput.FileName,
                    FormName = formInput.FormName,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        stopwatch.Stop();

        var batchResult = new ProcessBatchResult
        {
            BatchId = batchId,
            TotalForms = results.Count,
            SuccessfulForms = results.Count(r => r.Success),
            FailedForms = results.Count(r => !r.Success),
            Results = results,
            ProcessingTime = stopwatch.Elapsed,
            CompletedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Batch processing complete: {Successful}/{Total} successful in {Duration}",
            batchResult.SuccessfulForms,
            batchResult.TotalForms,
            batchResult.ProcessingTime);

        return batchResult;
    }
}
```

#### ProcessBatchCommandValidator.cs

```csharp
using FluentValidation;

namespace FormCodeGenerator.Application.Commands.ProcessBatch;

/// <summary>
/// Validator for ProcessBatchCommand
/// </summary>
public class ProcessBatchCommandValidator : AbstractValidator<ProcessBatchCommand>
{
    public ProcessBatchCommandValidator()
    {
        RuleFor(x => x.Forms)
            .NotEmpty()
            .WithMessage("At least one form is required")
            .Must(HaveValidCount)
            .WithMessage("Batch size must be between 1 and 100 forms");

        RuleForEach(x => x.Forms)
            .ChildRules(form =>
            {
                form.RuleFor(f => f.PdfStream)
                    .NotNull()
                    .WithMessage("PDF stream is required");

                form.RuleFor(f => f.FileName)
                    .NotEmpty()
                    .WithMessage("File name is required")
                    .Must(name => name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("File must be a PDF");

                form.RuleFor(f => f.FormName)
                    .NotEmpty()
                    .WithMessage("Form name is required")
                    .MaximumLength(100)
                    .WithMessage("Form name must not exceed 100 characters");
            });
    }

    private bool HaveValidCount(List<BatchFormInput> forms)
    {
        return forms.Count >= 1 && forms.Count <= 100;
    }
}
```

---

## Queries

Queries represent read operations that don't change system state.

### 1. GetFormAnalysisQuery

Retrieves form analysis details.

#### GetFormAnalysisQuery.cs
```csharp
using MediatR;

namespace FormCodeGenerator.Application.Queries.GetFormAnalysis;

/// <summary>
/// Query to get form analysis
/// </summary>
public record GetFormAnalysisQuery(Guid FormId) : IRequest<FormAnalysisDto?>;
```

#### FormAnalysisDto.cs
```csharp
namespace FormCodeGenerator.Application.Queries.GetFormAnalysis;

/// <summary>
/// Form analysis data transfer object
/// </summary>
public class FormAnalysisDto
{
    public Guid FormId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FormType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public DateTime? AnalyzedAt { get; init; }
    public int FieldCount { get; init; }
    public int TableCount { get; init; }
    public List<FormFieldDto> Fields { get; init; } = new();
    public List<FormTableDto> Tables { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool RequiresManualReview { get; init; }
}

public class FormFieldDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }
    public List<string> Options { get; init; } = new();
    public bool HasValidation { get; init; }
    public bool HasXFAProperties { get; init; }
}

public class FormTableDto
{
    public Guid Id { get; init; }
    public int RowCount { get; init; }
    public int ColumnCount { get; init; }
    public int CellCount { get; init; }
}
```

#### GetFormAnalysisQueryHandler.cs
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Application.Queries.GetFormAnalysis;

/// <summary>
/// Handler for GetFormAnalysisQuery
/// </summary>
public class GetFormAnalysisQueryHandler : IRequestHandler<GetFormAnalysisQuery, FormAnalysisDto?>
{
    private readonly IFormRepository _formRepository;
    private readonly ILogger<GetFormAnalysisQueryHandler> _logger;

    public GetFormAnalysisQueryHandler(
        IFormRepository formRepository,
        ILogger<GetFormAnalysisQueryHandler> logger)
    {
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<FormAnalysisDto?> Handle(
        GetFormAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving form analysis: {FormId}", request.FormId);

        var form = await _formRepository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            _logger.LogWarning("Form not found: {FormId}", request.FormId);
            return null;
        }

        return new FormAnalysisDto
        {
            FormId = form.Id,
            FileName = form.FileName,
            FormType = form.FormType.Value,
            UploadedAt = form.UploadedAt,
            AnalyzedAt = form.AnalyzedAt,
            FieldCount = form.Fields.Count,
            TableCount = form.Tables.Count,
            Fields = form.Fields.Select(f => new FormFieldDto
            {
                Id = f.Id,
                Name = f.Name,
                Label = f.Label,
                Type = f.FieldType.Value,
                IsRequired = f.IsRequired,
                MaxLength = f.MaxLength,
                DefaultValue = f.DefaultValue,
                Options = f.Options.ToList(),
                HasValidation = f.ValidationRule != null,
                HasXFAProperties = f.XFAProperties.Any()
            }).ToList(),
            Tables = form.Tables.Select(t => new FormTableDto
            {
                Id = t.Id,
                RowCount = t.RowCount,
                ColumnCount = t.ColumnCount,
                CellCount = t.Cells.Count
            }).ToList(),
            Warnings = form.Warnings.ToList(),
            RequiresManualReview = form.RequiresManualReview()
        };
    }
}
```

---

### 2. GetGeneratedCodeQuery

Retrieves generated code.

#### GetGeneratedCodeQuery.cs
```csharp
using MediatR;

namespace FormCodeGenerator.Application.Queries.GetGeneratedCode;

/// <summary>
/// Query to get generated code
/// </summary>
public record GetGeneratedCodeQuery(Guid RequestId) : IRequest<GeneratedCodeDto?>;
```

#### GeneratedCodeDto.cs
```csharp
namespace FormCodeGenerator.Application.Queries.GetGeneratedCode;

/// <summary>
/// Generated code data transfer object
/// </summary>
public class GeneratedCodeDto
{
    public Guid RequestId { get; init; }
    public Guid FormId { get; init; }
    public string FormName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<CodeFileDto> CodeFiles { get; init; } = new();
    public string SuggestedNamespace { get; init; } = string.Empty;
    public CodeAnalysisInfoDto? Analysis { get; init; }
}

public class CodeFileDto
{
    public Guid Id { get; init; }
    public string CodeType { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public int LineCount { get; init; }
}

public class CodeAnalysisInfoDto
{
    public string DetectedPurpose { get; init; } = string.Empty;
    public string Complexity { get; init; } = string.Empty;
    public bool HasRepeatingData { get; init; }
    public string RecommendedApproach { get; init; } = string.Empty;
}
```

#### GetGeneratedCodeQueryHandler.cs
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Application.Queries.GetGeneratedCode;

/// <summary>
/// Handler for GetGeneratedCodeQuery
/// </summary>
public class GetGeneratedCodeQueryHandler : IRequestHandler<GetGeneratedCodeQuery, GeneratedCodeDto?>
{
    private readonly ICodeGenerationRepository _codeGenerationRepository;
    private readonly ILogger<GetGeneratedCodeQueryHandler> _logger;

    public GetGeneratedCodeQueryHandler(
        ICodeGenerationRepository codeGenerationRepository,
        ILogger<GetGeneratedCodeQueryHandler> logger)
    {
        _codeGenerationRepository = codeGenerationRepository;
        _logger = logger;
    }

    public async Task<GeneratedCodeDto?> Handle(
        GetGeneratedCodeQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving generated code: {RequestId}", request.RequestId);

        var codeGenRequest = await _codeGenerationRepository.GetByIdAsync(
            request.RequestId,
            cancellationToken);

        if (codeGenRequest == null)
        {
            _logger.LogWarning("Code generation request not found: {RequestId}", request.RequestId);
            return null;
        }

        return new GeneratedCodeDto
        {
            RequestId = codeGenRequest.Id,
            FormId = codeGenRequest.FormId,
            FormName = codeGenRequest.FormName,
            Status = codeGenRequest.Status.ToString(),
            RequestedAt = codeGenRequest.RequestedAt,
            CompletedAt = codeGenRequest.CompletedAt,
            CodeFiles = codeGenRequest.GeneratedCodes.Select(gc => new CodeFileDto
            {
                Id = gc.Id,
                CodeType = gc.CodeType,
                Code = gc.Code,
                Language = gc.Language,
                LineCount = gc.Code.Split('\n').Length
            }).ToList(),
            SuggestedNamespace = codeGenRequest.TargetNamespace.Value,
            Analysis = codeGenRequest.Analysis != null ? new CodeAnalysisInfoDto
            {
                DetectedPurpose = codeGenRequest.Analysis.DetectedPurpose,
                Complexity = codeGenRequest.Analysis.Complexity,
                HasRepeatingData = codeGenRequest.Analysis.HasRepeatingData,
                RecommendedApproach = codeGenRequest.Analysis.RecommendedApproach
            } : null
        };
    }
}
```

---

## Behaviors (Pipeline Behaviors)

Behaviors wrap the request handling pipeline for cross-cutting concerns.

### LoggingBehavior.cs
```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FormCodeGenerator.Application.Behaviors;

/// <summary>
/// Logs all requests and responses with timing
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

### ValidationBehavior.cs

```csharp
using FluentValidation;
using MediatR;

namespace FormCodeGenerator.Application.Behaviors;

/// <summary>
/// Validates requests before they reach the handler
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### PerformanceBehavior.cs

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FormCodeGenerator.Application.Behaviors;

/// <summary>
/// Logs warning if request takes longer than threshold
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        // Log warning if request took longer than 3 seconds
        if (elapsedMilliseconds > 3000)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds)",
                requestName,
                elapsedMilliseconds);
        }

        return response;
    }
}
```

---

## Application Interfaces

### IFormExtractor.cs
```csharp
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.ValueObjects;

namespace FormCodeGenerator.Application.Interfaces;

/// <summary>
/// Infrastructure service for extracting form structure
/// </summary>
public interface IFormExtractor
{
    Task<string> DetectFormTypeAsync(
        string pdfPath,
        CancellationToken cancellationToken = default);

    Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        FormType formType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Extracted form structure (infrastructure DTO)
/// </summary>
public class ExtractedFormStructure
{
    public List<ExtractedField> Fields { get; init; } = new();
    public List<ExtractedTable> Tables { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}

public class ExtractedField
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }
    public List<string>? Options { get; init; }
    public string? ValidationPattern { get; init; }
}

public class ExtractedTable
{
    public int RowCount { get; init; }
    public int ColumnCount { get; init; }
    public List<ExtractedCell> Cells { get; init; } = new();
}

public class ExtractedCell
{
    public string Content { get; init; } = string.Empty;
    public int RowIndex { get; init; }
    public int ColumnIndex { get; init; }
    public bool IsHeader { get; init; }
}
```

### ICodeGenerator.cs

```csharp
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.ValueObjects;

namespace FormCodeGenerator.Application.Interfaces;

/// <summary>
/// Infrastructure service for generating code
/// </summary>
public interface ICodeGenerator
{
    Task<GeneratedCodeOutput> GenerateCodeAsync(
        Form form,
        string formName,
        CodeNamespace targetNamespace,
        string? formPurpose,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Generated code output (infrastructure DTO)
/// </summary>
public class GeneratedCodeOutput
{
    public string ModelCode { get; init; } = string.Empty;
    public string ControllerCode { get; init; } = string.Empty;
    public string ViewCode { get; init; } = string.Empty;
    public string MigrationCode { get; init; } = string.Empty;
    public GeneratedCodeAnalysis Analysis { get; init; } = new();
}

public class GeneratedCodeAnalysis
{
    public string DetectedPurpose { get; init; } = string.Empty;
    public string Complexity { get; init; } = string.Empty;
    public bool HasRepeatingData { get; init; }
    public string RecommendedApproach { get; init; } = string.Empty;
}
```

---

## Dependency Injection Setup

### ApplicationServiceExtensions.cs
```csharp
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Application.Behaviors;
using System.Reflection;

namespace FormCodeGenerator.Application.Configuration;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}
```

---

## Verification

- [ ] All tests pass
- [ ] Code compiles

## Next Steps

Continue to next phase. [FORM INFRASTRUCTURE](04-PHASE-4-FORM-INFRASTRUCTURE.md)
