# Phase 4: Form Context - Infrastructure

**Duration:** TBD  
**Complexity:** Medium-High  
**Prerequisites:** Previous phases complete

## Overview

The Infrastructure layer implements external dependencies: Azure Document Intelligence, Azure OpenAI, Entity Framework Core persistence, and event publishing.

## Objectives

- [ ] Create Azure AI folder structure
- [ ] Create Form Type Detector
- [ ] Create AcroForm Parser
- [ ] Create XFAForm Parser
- [ ] Create Azure Form Extractor
- [ ] Create Persistence folder structure
- [ ] Update/Refactor DB Context
- [ ] Create Form Configuration
- [ ] Create Form Field Configuration
- [ ] Create Form Table Configuration
- [ ] Create Form Repository
- [ ] Create Options folder structure
- [ ] Create Azure AI Options **this is optional, only needed if using AI for Code Generation**
- [ ] Create Persistence Options
- [ ] Create Events folder structure
- [ ] Create Event Publisher **Requires Interface in Use Cases layer**
- [ ] **Create other needed events for form context**
- [ ] Create Configuration folder structure
- [ ] Create Infrastructure Service Extensions
- [ ] Update appsettings **in the web layer**
- [ ] Run Database Migrations
- [ ] Write unit tests
- [ ] Verify domain layer has no infrastructure dependencies

## Steps

## Table of Contents

1. [Azure Document Intelligence](#azure-document-intelligence)
2. [Azure OpenAI Code Generation](#azure-openai-code-generation)
3. [Entity Framework Core](#entity-framework-core)
4. [Repositories](#repositories)
5. [Event Publishing](#event-publishing)
6. [Configuration](#configuration)

---

## Azure Document Intelligence

### FormTypeDetector.cs

Detects the type of PDF form (AcroForm, XFA, Hybrid, Static).

```csharp
using iText.Forms;
using iText.Kernel.Pdf;

namespace FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Detects PDF form type using iText7
/// </summary>
public class FormTypeDetector
{
    public async Task<string> DetectFormTypeAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            using var pdfReader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(pdfReader);

            var acroForm = PdfAcroForm.GetAcroForm(pdfDoc, false);
            var hasAcroForm = acroForm != null && acroForm.GetAllFormFields().Count > 0;

            var catalog = pdfDoc.GetCatalog();
            var hasXFA = catalog.GetPdfObject().GetAsDictionary(PdfName.AcroForm)
                ?.ContainsKey(PdfName.XFA) ?? false;

            if (hasAcroForm && hasXFA)
                return "Hybrid";
            else if (hasXFA)
                return "XFA";
            else if (hasAcroForm)
                return "AcroForm";
            else
                return "Static";
        });
    }
}
```

---

### AcroFormParser.cs

Extracts structure from AcroForm PDFs.

```csharp
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Parses AcroForm PDF fields
/// </summary>
public class AcroFormParser
{
    public async Task<List<ExtractedField>> ParseFieldsAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            var fields = new List<ExtractedField>();

            using var pdfReader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(pdfReader);

            var acroForm = PdfAcroForm.GetAcroForm(pdfDoc, false);
            if (acroForm == null)
                return fields;

            foreach (var fieldEntry in acroForm.GetAllFormFields())
            {
                var fieldName = fieldEntry.Key;
                var field = fieldEntry.Value;

                var extractedField = new ExtractedField
                {
                    Name = SanitizeFieldName(fieldName),
                    Label = GetFieldLabel(field, fieldName),
                    Type = DetermineFieldType(field),
                    IsRequired = IsFieldRequired(field),
                    MaxLength = GetMaxLength(field),
                    DefaultValue = GetDefaultValue(field),
                    Options = GetFieldOptions(field),
                    ValidationPattern = InferValidationPattern(fieldName, field)
                };

                fields.Add(extractedField);
            }

            return fields;
        });
    }

    private string SanitizeFieldName(string fieldName)
    {
        // Remove special characters, replace spaces with underscores
        var sanitized = new string(fieldName
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray());

        // Ensure it starts with a letter
        if (!char.IsLetter(sanitized[0]))
            sanitized = "Field_" + sanitized;

        return sanitized;
    }

    private string GetFieldLabel(PdfFormField field, string fieldName)
    {
        // Try to get alternate name (often used for labels)
        var alternateName = field.GetAlternativeName();
        if (!string.IsNullOrEmpty(alternateName))
            return alternateName;

        // Try to get partial name
        var partialName = field.GetPartialName();
        if (!string.IsNullOrEmpty(partialName))
            return FormatLabel(partialName);

        return FormatLabel(fieldName);
    }

    private string FormatLabel(string name)
    {
        // Convert camelCase or snake_case to Title Case
        var result = System.Text.RegularExpressions.Regex.Replace(
            name,
            "([a-z])([A-Z])",
            "$1 $2");

        result = result.Replace('_', ' ');

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
            result.ToLower());
    }

    private string DetermineFieldType(PdfFormField field)
    {
        var fieldType = field.GetFormType();

        return fieldType switch
        {
            PdfName.Tx => DetermineTextFieldType(field),
            PdfName.Ch => "select",
            PdfName.Btn => IsCheckbox(field) ? "checkbox" : "radio",
            _ => "text"
        };
    }

    private string DetermineTextFieldType(PdfFormField field)
    {
        var fieldName = field.GetPartialName()?.ToLower() ?? "";
        var maxLen = GetMaxLength(field);

        // Check field name for hints
        if (fieldName.Contains("email"))
            return "email";
        if (fieldName.Contains("phone") || fieldName.Contains("tel"))
            return "phone";
        if (fieldName.Contains("date"))
            return "date";
        if (fieldName.Contains("number") || fieldName.Contains("amount"))
            return "number";

        // Multi-line text fields
        if (field.GetPdfObject().GetAsNumber(PdfName.Ff)?.IntValue() == 4096)
            return "textarea";

        // Large max length suggests textarea
        if (maxLen.HasValue && maxLen > 200)
            return "textarea";

        return "text";
    }

    private bool IsCheckbox(PdfFormField field)
    {
        if (field is PdfButtonFormField buttonField)
        {
            return !buttonField.IsPushButton();
        }
        return false;
    }

    private bool IsFieldRequired(PdfFormField field)
    {
        var flags = field.GetFieldFlags();
        return (flags & PdfFormField.FF_REQUIRED) != 0;
    }

    private int? GetMaxLength(PdfFormField field)
    {
        if (field is PdfTextFormField textField)
        {
            var maxLen = textField.GetMaxLen();
            return maxLen > 0 ? maxLen : null;
        }
        return null;
    }

    private string? GetDefaultValue(PdfFormField field)
    {
        var value = field.GetValueAsString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private List<string>? GetFieldOptions(PdfFormField field)
    {
        if (field is PdfChoiceFormField choiceField)
        {
            var options = choiceField.GetOptions();
            return options?
                .Select(opt => opt.GetAsString(0)?.GetValue())
                .Where(s => !string.IsNullOrEmpty(s))
                .Cast<string>()
                .ToList();
        }
        return null;
    }

    private string? InferValidationPattern(string fieldName, PdfFormField field)
    {
        var lowerName = fieldName.ToLower();

        if (lowerName.Contains("email"))
            return @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (lowerName.Contains("phone") || lowerName.Contains("tel"))
            return @"^\d{3}-\d{3}-\d{4}$";

        if (lowerName.Contains("zip") || lowerName.Contains("postal"))
            return @"^\d{5}(-\d{4})?$";

        if (lowerName.Contains("ssn") || lowerName.Contains("social"))
            return @"^\d{3}-\d{2}-\d{4}$";

        return null;
    }
}
```

---

### XFAFormParser.cs

Extracts structure from XFA PDFs using Document Intelligence.

```csharp
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Parses XFA forms using Azure Document Intelligence
/// </summary>
public class XFAFormParser
{
    private readonly DocumentAnalysisClient _client;
    private readonly AcroFormParser _acroFormParser;

    public XFAFormParser(DocumentAnalysisClient client, AcroFormParser acroFormParser)
    {
        _client = client;
        _acroFormParser = acroFormParser;
    }

    public async Task<List<ExtractedField>> ParseFieldsAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        // For XFA, we need to correlate Document Intelligence results with field metadata
        var fields = new List<ExtractedField>();

        await using var stream = File.OpenRead(pdfPath);

        // Use prebuilt-document model to extract key-value pairs
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-document",
            stream,
            cancellationToken: cancellationToken);

        var result = operation.Value;

        // Try to get AcroForm fields as well (for hybrid forms)
        var acroFields = await _acroFormParser.ParseFieldsAsync(pdfPath);
        var acroFieldMap = acroFields.ToDictionary(f => f.Label, f => f);

        // Process key-value pairs from Document Intelligence
        foreach (var kvp in result.KeyValuePairs)
        {
            if (kvp.Key?.Content == null)
                continue;

            var label = kvp.Key.Content.Trim();
            var value = kvp.Value?.Content?.Trim();

            // Try to match with AcroForm field
            if (acroFieldMap.TryGetValue(label, out var acroField))
            {
                fields.Add(acroField);
                continue;
            }

            // Create field from Document Intelligence data
            var field = new ExtractedField
            {
                Name = SanitizeFieldName(label),
                Label = label,
                Type = InferFieldType(label, value),
                IsRequired = false, // Can't determine from Document Intelligence
                DefaultValue = value
            };

            fields.Add(field);
        }

        return fields;
    }

    private string SanitizeFieldName(string label)
    {
        var sanitized = new string(label
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray());

        if (!char.IsLetter(sanitized[0]))
            sanitized = "Field_" + sanitized;

        return sanitized;
    }

    private string InferFieldType(string label, string? value)
    {
        var lowerLabel = label.ToLower();

        if (lowerLabel.Contains("email"))
            return "email";
        if (lowerLabel.Contains("phone") || lowerLabel.Contains("tel"))
            return "phone";
        if (lowerLabel.Contains("date"))
            return "date";
        if (lowerLabel.Contains("address") || lowerLabel.Contains("street"))
            return "textarea";

        // Try to infer from value
        if (!string.IsNullOrEmpty(value))
        {
            if (DateTime.TryParse(value, out _))
                return "date";
            if (decimal.TryParse(value, out _))
                return "number";
        }

        return "text";
    }
}
```

---

### AzureFormExtractor.cs

Main implementation of IFormExtractor interface.

```csharp
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FormCodeGenerator.Application.Interfaces;
using FormCodeGenerator.Domain.ValueObjects;
using FormCodeGenerator.Infrastructure.Options;

namespace FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;

/// <summary>
/// Extracts form structure using Azure Document Intelligence and iText7
/// </summary>
public class AzureFormExtractor : IFormExtractor
{
    private readonly DocumentAnalysisClient _client;
    private readonly FormTypeDetector _formTypeDetector;
    private readonly AcroFormParser _acroFormParser;
    private readonly XFAFormParser _xfaFormParser;
    private readonly ILogger<AzureFormExtractor> _logger;

    public AzureFormExtractor(
        IOptions<AzureAIOptions> options,
        ILogger<AzureFormExtractor> logger)
    {
        var config = options.Value.DocumentIntelligence;
        
        _client = new DocumentAnalysisClient(
            new Uri(config.Endpoint),
            new AzureKeyCredential(config.ApiKey));

        _formTypeDetector = new FormTypeDetector();
        _acroFormParser = new AcroFormParser();
        _xfaFormParser = new XFAFormParser(_client, _acroFormParser);
        _logger = logger;
    }

    public async Task<string> DetectFormTypeAsync(
        string pdfPath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting form type for: {PdfPath}", pdfPath);

        var formType = await _formTypeDetector.DetectFormTypeAsync(pdfPath);

        _logger.LogInformation("Detected form type: {FormType}", formType);

        return formType;
    }

    public async Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        FormType formType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Extracting form structure: {PdfPath}, Type: {FormType}",
            pdfPath,
            formType.Value);

        var structure = new ExtractedFormStructure();

        try
        {
            // Extract fields based on form type
            structure.Fields = formType.Value switch
            {
                "AcroForm" => await _acroFormParser.ParseFieldsAsync(pdfPath),
                "XFA" => await _xfaFormParser.ParseFieldsAsync(pdfPath, cancellationToken),
                "Hybrid" => await ParseHybridFormAsync(pdfPath, cancellationToken),
                "Static" => await ParseStaticFormAsync(pdfPath, cancellationToken),
                _ => new List<ExtractedField>()
            };

            // Extract tables using Document Intelligence
            structure.Tables = await ExtractTablesAsync(pdfPath, cancellationToken);

            // Add warnings
            if (formType == FormType.XFA)
            {
                structure.Warnings.Add(
                    "XFA forms may have dynamic behavior that cannot be fully captured. Manual review recommended.");
            }

            if (structure.Fields.Any(f => f.ValidationPattern == null && f.IsRequired))
            {
                structure.Warnings.Add(
                    "Some required fields don't have validation patterns. Consider adding validation.");
            }

            _logger.LogInformation(
                "Extraction complete: {FieldCount} fields, {TableCount} tables",
                structure.Fields.Count,
                structure.Tables.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting form structure");
            throw;
        }

        return structure;
    }

    private async Task<List<ExtractedField>> ParseHybridFormAsync(
        string pdfPath,
        CancellationToken cancellationToken)
    {
        // For hybrid forms, try both approaches and merge
        var acroFields = await _acroFormParser.ParseFieldsAsync(pdfPath);
        var xfaFields = await _xfaFormParser.ParseFieldsAsync(pdfPath, cancellationToken);

        // Merge, preferring AcroForm fields (they have better metadata)
        var merged = new Dictionary<string, ExtractedField>();

        foreach (var field in acroFields)
        {
            merged[field.Name] = field;
        }

        foreach (var field in xfaFields)
        {
            if (!merged.ContainsKey(field.Name))
            {
                merged[field.Name] = field;
            }
        }

        return merged.Values.ToList();
    }

    private async Task<List<ExtractedField>> ParseStaticFormAsync(
        string pdfPath,
        CancellationToken cancellationToken)
    {
        // For static PDFs, use Document Intelligence to find key-value pairs
        await using var stream = File.OpenRead(pdfPath);

        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-document",
            stream,
            cancellationToken: cancellationToken);

        var result = operation.Value;
        var fields = new List<ExtractedField>();

        foreach (var kvp in result.KeyValuePairs)
        {
            if (kvp.Key?.Content == null)
                continue;

            var label = kvp.Key.Content.Trim();
            var value = kvp.Value?.Content?.Trim();

            var field = new ExtractedField
            {
                Name = SanitizeFieldName(label),
                Label = label,
                Type = InferFieldType(label, value),
                IsRequired = false,
                DefaultValue = value
            };

            fields.Add(field);
        }

        return fields;
    }

    private async Task<List<ExtractedTable>> ExtractTablesAsync(
        string pdfPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(pdfPath);

        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-document",
            stream,
            cancellationToken: cancellationToken);

        var result = operation.Value;
        var tables = new List<ExtractedTable>();

        foreach (var table in result.Tables)
        {
            var extractedTable = new ExtractedTable
            {
                RowCount = table.RowCount,
                ColumnCount = table.ColumnCount,
                Cells = new List<ExtractedCell>()
            };

            foreach (var cell in table.Cells)
            {
                extractedTable.Cells.Add(new ExtractedCell
                {
                    Content = cell.Content,
                    RowIndex = cell.RowIndex,
                    ColumnIndex = cell.ColumnIndex,
                    IsHeader = cell.Kind == DocumentTableCellKind.ColumnHeader ||
                               cell.Kind == DocumentTableCellKind.RowHeader
                });
            }

            tables.Add(extractedTable);
        }

        return tables;
    }

    private string SanitizeFieldName(string name)
    {
        var sanitized = new string(name
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray());

        if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]))
            sanitized = "Field_" + sanitized;

        return sanitized;
    }

    private string InferFieldType(string label, string? value)
    {
        var lowerLabel = label.ToLower();

        if (lowerLabel.Contains("email"))
            return "email";
        if (lowerLabel.Contains("phone") || lowerLabel.Contains("tel"))
            return "phone";
        if (lowerLabel.Contains("date"))
            return "date";

        if (!string.IsNullOrEmpty(value))
        {
            if (DateTime.TryParse(value, out _))
                return "date";
            if (decimal.TryParse(value, out _))
                return "number";
        }

        return "text";
    }
}
```

---

## Azure OpenAI Code Generation

### PromptBuilder.cs

Builds prompts for Azure OpenAI.

```csharp
using System.Text;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;

namespace FormCodeGenerator.Infrastructure.AI.AzureOpenAI;

/// <summary>
/// Builds prompts for code generation
/// </summary>
public class PromptBuilder
{
    public string BuildCodeGenerationPrompt(
        Form form,
        string formName,
        string targetNamespace,
        string? formPurpose)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert C# developer. Generate production-ready ASP.NET Core MVC code for the following form.");
        sb.AppendLine();

        // Form metadata
        sb.AppendLine("## Form Information");
        sb.AppendLine($"- Name: {formName}");
        sb.AppendLine($"- Type: {form.FormType.Value}");
        sb.AppendLine($"- Field Count: {form.Fields.Count}");
        if (!string.IsNullOrEmpty(formPurpose))
        {
            sb.AppendLine($"- Purpose: {formPurpose}");
        }
        sb.AppendLine();

        // Fields
        sb.AppendLine("## Form Fields");
        foreach (var field in form.Fields)
        {
            sb.AppendLine($"- {field.Label}:");
            sb.AppendLine($"  - Name: {field.Name}");
            sb.AppendLine($"  - Type: {field.FieldType.Value}");
            sb.AppendLine($"  - Required: {field.IsRequired}");
            
            if (field.MaxLength.HasValue)
                sb.AppendLine($"  - Max Length: {field.MaxLength}");
            
            if (field.Options.Any())
                sb.AppendLine($"  - Options: {string.Join(", ", field.Options)}");
            
            if (field.ValidationRule != null)
                sb.AppendLine($"  - Validation: {field.ValidationRule.Pattern}");
        }
        sb.AppendLine();

        // Tables
        if (form.Tables.Any())
        {
            sb.AppendLine("## Form Tables");
            foreach (var table in form.Tables)
            {
                sb.AppendLine($"- Table: {table.RowCount} rows x {table.ColumnCount} columns");
            }
            sb.AppendLine();
        }

        // Requirements
        sb.AppendLine("## Generation Requirements");
        sb.AppendLine();
        
        sb.AppendLine("Generate the following files:");
        sb.AppendLine();
        
        sb.AppendLine("### 1. Model Class");
        sb.AppendLine($"- Namespace: {targetNamespace}.Models");
        sb.AppendLine($"- Class name: {formName}");
        sb.AppendLine("- Use data annotations for validation");
        sb.AppendLine("- Use appropriate C# types (string, int, DateTime, bool, etc.)");
        sb.AppendLine("- Add [Required], [MaxLength], [EmailAddress], etc. as needed");
        sb.AppendLine("- Use PascalCase for property names");
        sb.AppendLine();

        sb.AppendLine("### 2. Controller Class");
        sb.AppendLine($"- Namespace: {targetNamespace}.Controllers");
        sb.AppendLine($"- Class name: {formName}Controller");
        sb.AppendLine("- Inherit from Controller");
        sb.AppendLine("- Implement GET action for displaying form");
        sb.AppendLine("- Implement POST action for submitting form");
        sb.AppendLine("- Add ModelState validation");
        sb.AppendLine("- Return RedirectToAction on success");
        sb.AppendLine();

        sb.AppendLine("### 3. Razor View");
        sb.AppendLine($"- Model: {formName}");
        sb.AppendLine("- Use Bootstrap 5 for styling");
        sb.AppendLine("- Create proper form layout");
        sb.AppendLine("- Add validation summary");
        sb.AppendLine("- Use asp-for tag helpers");
        sb.AppendLine("- Use asp-validation-for for field validation");
        sb.AppendLine("- Make it responsive");
        sb.AppendLine();

        sb.AppendLine("### 4. EF Core Migration");
        sb.AppendLine($"- Table name: {formName}s");
        sb.AppendLine("- Include all properties from model");
        sb.AppendLine("- Use appropriate SQL types");
        sb.AppendLine("- Add primary key (Id)");
        sb.AppendLine("- Add timestamp fields (CreatedAt, UpdatedAt)");
        sb.AppendLine();

        sb.AppendLine("## Output Format");
        sb.AppendLine("Return ONLY a JSON object with this exact structure:");
        sb.AppendLine("```json");
        sb.AppendLine("{");
        sb.AppendLine("  \"ModelCode\": \"<complete C# model code>\",");
        sb.AppendLine("  \"ControllerCode\": \"<complete C# controller code>\",");
        sb.AppendLine("  \"ViewCode\": \"<complete Razor view code>\",");
        sb.AppendLine("  \"MigrationCode\": \"<complete EF Core migration code>\",");
        sb.AppendLine("  \"Analysis\": {");
        sb.AppendLine("    \"DetectedPurpose\": \"<inferred purpose of the form>\",");
        sb.AppendLine("    \"Complexity\": \"Simple|Medium|Complex\",");
        sb.AppendLine("    \"HasRepeatingData\": true|false,");
        sb.AppendLine("    \"RecommendedApproach\": \"<architecture recommendation>\"");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("Important:");
        sb.AppendLine("- Do NOT include markdown code fences (```csharp, etc.) in the code strings");
        sb.AppendLine("- All code should be properly indented and formatted");
        sb.AppendLine("- Include all necessary using statements");
        sb.AppendLine("- Follow C# naming conventions");
        sb.AppendLine("- Add helpful comments where appropriate");

        return sb.ToString();
    }
}
```

---

### AzureCodeGenerator.cs

Implements ICodeGenerator using Azure OpenAI.

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using FormCodeGenerator.Application.Interfaces;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.ValueObjects;
using FormCodeGenerator.Infrastructure.Options;

namespace FormCodeGenerator.Infrastructure.AI.AzureOpenAI;

/// <summary>
/// Generates code using Azure OpenAI
/// </summary>
public class AzureCodeGenerator : ICodeGenerator
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly PromptBuilder _promptBuilder;
    private readonly ILogger<AzureCodeGenerator> _logger;

    public AzureCodeGenerator(
        IOptions<AzureAIOptions> options,
        ILogger<AzureCodeGenerator> logger)
    {
        var config = options.Value.OpenAI;

        _client = new OpenAIClient(
            new Uri(config.Endpoint),
            new AzureKeyCredential(config.ApiKey));

        _deploymentName = config.DeploymentName;
        _promptBuilder = new PromptBuilder();
        _logger = logger;
    }

    public async Task<GeneratedCodeOutput> GenerateCodeAsync(
        Form form,
        string formName,
        CodeNamespace targetNamespace,
        string? formPurpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating code for form: {FormName}, Namespace: {Namespace}",
            formName,
            targetNamespace.Value);

        try
        {
            // Build prompt
            var prompt = _promptBuilder.BuildCodeGenerationPrompt(
                form,
                formName,
                targetNamespace.Value,
                formPurpose);

            _logger.LogDebug("Prompt length: {Length} characters", prompt.Length);

            // Call Azure OpenAI
            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(
                        "You are an expert C# developer specializing in ASP.NET Core MVC. " +
                        "Generate clean, production-ready code following best practices."),
                    new ChatRequestUserMessage(prompt)
                },
                Temperature = 0.3f, // Lower temperature for consistent code generation
                MaxTokens = 4000,
                ResponseFormat = ChatCompletionsResponseFormat.JsonObject
            };

            var response = await _client.GetChatCompletionsAsync(
                chatCompletionsOptions,
                cancellationToken);

            var completionContent = response.Value.Choices[0].Message.Content;

            _logger.LogDebug("Received response, length: {Length}", completionContent.Length);

            // Parse JSON response
            var jsonResponse = JsonSerializer.Deserialize<CodeGenerationResponse>(
                completionContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (jsonResponse == null)
            {
                throw new InvalidOperationException("Failed to parse OpenAI response");
            }

            // Validate generated code
            ValidateGeneratedCode(jsonResponse);

            var output = new GeneratedCodeOutput
            {
                ModelCode = jsonResponse.ModelCode,
                ControllerCode = jsonResponse.ControllerCode,
                ViewCode = jsonResponse.ViewCode,
                MigrationCode = jsonResponse.MigrationCode,
                Analysis = new GeneratedCodeAnalysis
                {
                    DetectedPurpose = jsonResponse.Analysis?.DetectedPurpose ?? "Unknown",
                    Complexity = jsonResponse.Analysis?.Complexity ?? "Medium",
                    HasRepeatingData = jsonResponse.Analysis?.HasRepeatingData ?? false,
                    RecommendedApproach = jsonResponse.Analysis?.RecommendedApproach ?? "Standard MVC"
                }
            };

            _logger.LogInformation("Code generation completed successfully");

            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code");
            throw;
        }
    }

    private void ValidateGeneratedCode(CodeGenerationResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.ModelCode))
            throw new InvalidOperationException("Model code is empty");

        if (string.IsNullOrWhiteSpace(response.ControllerCode))
            throw new InvalidOperationException("Controller code is empty");

        if (string.IsNullOrWhiteSpace(response.ViewCode))
            throw new InvalidOperationException("View code is empty");

        if (string.IsNullOrWhiteSpace(response.MigrationCode))
            throw new InvalidOperationException("Migration code is empty");

        // Basic validation - check for common patterns
        if (!response.ModelCode.Contains("class "))
            throw new InvalidOperationException("Model code doesn't contain a class definition");

        if (!response.ControllerCode.Contains("Controller"))
            throw new InvalidOperationException("Controller code doesn't contain Controller class");

        if (!response.ViewCode.Contains("@model"))
            throw new InvalidOperationException("View code doesn't contain @model directive");
    }
}

/// <summary>
/// Response from Azure OpenAI
/// </summary>
internal class CodeGenerationResponse
{
    public string ModelCode { get; set; } = string.Empty;
    public string ControllerCode { get; set; } = string.Empty;
    public string ViewCode { get; set; } = string.Empty;
    public string MigrationCode { get; set; } = string.Empty;
    public AnalysisResponse? Analysis { get; set; }
}

internal class AnalysisResponse
{
    public string DetectedPurpose { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public bool HasRepeatingData { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
}
```

---

## Entity Framework Core

### ApplicationDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormTable> FormTables => Set<FormTable>();
    public DbSet<CodeGenerationRequest> CodeGenerationRequests => Set<CodeGenerationRequest>();
    public DbSet<GeneratedCode> GeneratedCodes => Set<GeneratedCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events before saving
        var domainEvents = ChangeTracker.Entries<Form>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear domain events
        foreach (var entity in ChangeTracker.Entries<Form>().Select(e => e.Entity))
        {
            entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Domain events would be published here by an event dispatcher
        // For now, we just clear them

        return result;
    }
}
```

---

### Entity Configurations

#### FormConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Form entity
/// </summary>
public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.UploadedAt)
            .IsRequired();

        builder.Property(f => f.AnalyzedAt);

        // Value object - FormType
        builder.OwnsOne(f => f.FormType, ft =>
        {
            ft.Property(t => t.Value)
                .HasColumnName("FormType")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Collection - Fields
        builder.HasMany<FormField>("_fields")
            .WithOne()
            .HasForeignKey("FormId")
            .OnDelete(DeleteBehavior.Cascade);

        // Collection - Tables
        builder.HasMany<FormTable>("_tables")
            .WithOne()
            .HasForeignKey("FormId")
            .OnDelete(DeleteBehavior.Cascade);

        // Collection - Warnings (stored as JSON)
        builder.Property<List<string>>("_warnings")
            .HasColumnName("Warnings")
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

        // Ignore domain events
        builder.Ignore(f => f.DomainEvents);

        // Index
        builder.HasIndex(f => f.FileName);
    }
}
```

#### FormFieldConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for FormField entity
/// </summary>
public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("FormFields");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.Label)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.IsRequired)
            .IsRequired();

        builder.Property(f => f.MaxLength);

        builder.Property(f => f.DefaultValue)
            .HasMaxLength(1000);

        // Value object - FieldType
        builder.OwnsOne(f => f.FieldType, ft =>
        {
            ft.Property(t => t.Value)
                .HasColumnName("FieldType")
                .HasMaxLength(50)
                .IsRequired();

            ft.Property(t => t.CSharpType)
                .HasColumnName("CSharpType")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Collection - Options (stored as JSON)
        builder.Property(f => f.Options)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

        // Value object - ValidationRule (optional)
        builder.OwnsOne(f => f.ValidationRule, vr =>
        {
            vr.Property(r => r.Pattern)
                .HasColumnName("ValidationPattern")
                .HasMaxLength(500);

            vr.Property(r => r.ErrorMessage)
                .HasColumnName("ValidationErrorMessage")
                .HasMaxLength(500);

            vr.Property(r => r.IsRequired)
                .HasColumnName("ValidationIsRequired");

            vr.Property(r => r.MaxLength)
                .HasColumnName("ValidationMaxLength");

            vr.Property(r => r.MinLength)
                .HasColumnName("ValidationMinLength");
        });

        // Value object - FieldPosition (optional)
        builder.OwnsOne(f => f.Position, p =>
        {
            p.Property(pos => pos.Page)
                .HasColumnName("PositionPage");

            p.Property(pos => pos.X)
                .HasColumnName("PositionX");

            p.Property(pos => pos.Y)
                .HasColumnName("PositionY");
        });

        // Dictionary - XFAProperties (stored as JSON)
        builder.Property(f => f.XFAProperties)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        // Index
        builder.HasIndex(f => f.Name);
    }
}
```

#### FormTableConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for FormTable entity
/// </summary>
public class FormTableConfiguration : IEntityTypeConfiguration<FormTable>
{
    public void Configure(EntityTypeBuilder<FormTable> builder)
    {
        builder.ToTable("FormTables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.RowCount)
            .IsRequired();

        builder.Property(t => t.ColumnCount)
            .IsRequired();

        // Collection - Cells (stored as JSON for simplicity)
        builder.Property(t => t.Cells)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<TableCell>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<TableCell>());
    }
}
```
---

## Repositories, CodeGeneration Configuration, Events, and DI Setup

---

## Entity Configurations (Continued)

### CodeGenerationRequestConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for CodeGenerationRequest entity
/// </summary>
public class CodeGenerationRequestConfiguration : IEntityTypeConfiguration<CodeGenerationRequest>
{
    public void Configure(EntityTypeBuilder<CodeGenerationRequest> builder)
    {
        builder.ToTable("CodeGenerationRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FormId)
            .IsRequired();

        builder.Property(r => r.FormName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.FormPurpose)
            .HasMaxLength(1000);

        builder.Property(r => r.RequestedAt)
            .IsRequired();

        builder.Property(r => r.CompletedAt);

        builder.Property(r => r.FailedAt);

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(2000);

        // Enum - Status
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Value object - CodeNamespace
        builder.OwnsOne(r => r.TargetNamespace, ns =>
        {
            ns.Property(n => n.Value)
                .HasColumnName("TargetNamespace")
                .HasMaxLength(500)
                .IsRequired();
        });

        // Value object - CodeAnalysis (optional)
        builder.OwnsOne(r => r.Analysis, a =>
        {
            a.Property(x => x.DetectedPurpose)
                .HasColumnName("AnalysisDetectedPurpose")
                .HasMaxLength(500);

            a.Property(x => x.Complexity)
                .HasColumnName("AnalysisComplexity")
                .HasMaxLength(50);

            a.Property(x => x.HasRepeatingData)
                .HasColumnName("AnalysisHasRepeatingData");

            a.Property(x => x.RecommendedApproach)
                .HasColumnName("AnalysisRecommendedApproach")
                .HasMaxLength(1000);
        });

        // Collection - GeneratedCodes
        builder.HasMany(r => r.GeneratedCodes)
            .WithOne()
            .HasForeignKey("RequestId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);

        // Indexes
        builder.HasIndex(r => r.FormId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.RequestedAt);
    }
}
```

### GeneratedCodeConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for GeneratedCode entity
/// </summary>
public class GeneratedCodeConfiguration : IEntityTypeConfiguration<GeneratedCode>
{
    public void Configure(EntityTypeBuilder<GeneratedCode> builder)
    {
        builder.ToTable("GeneratedCodes");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.CodeType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(g => g.Language)
            .IsRequired()
            .HasMaxLength(50);

        // Index
        builder.HasIndex(g => g.CodeType);
    }
}
```

---

## Repositories

### FormRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using FormCodeGenerator.Domain.Aggregates.FormAggregate;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for Form aggregate
/// </summary>
public class FormRepository : IFormRepository
{
    private readonly ApplicationDbContext _context;

    public FormRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Form?> GetByFileNameAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .FirstOrDefaultAsync(f => f.FileName == fileName, cancellationToken);
    }

    public async Task<List<Form>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Forms
            .Include("_fields")
            .Include("_tables")
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Form form, CancellationToken cancellationToken = default)
    {
        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Form form, CancellationToken cancellationToken = default)
    {
        _context.Forms.Update(form);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var form = await GetByIdAsync(id, cancellationToken);
        if (form != null)
        {
            _context.Forms.Remove(form);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### CodeGenerationRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;
using FormCodeGenerator.Domain.Interfaces;

namespace FormCodeGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CodeGenerationRequest aggregate
/// </summary>
public class CodeGenerationRepository : ICodeGenerationRepository
{
    private readonly ApplicationDbContext _context;

    public CodeGenerationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CodeGenerationRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetByFormIdAsync(
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .Where(r => r.FormId == formId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CodeGenerationRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CodeGenerationRequests
            .Include(r => r.GeneratedCodes)
            .Where(r => r.Status == CodeGenerationStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CodeGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _context.CodeGenerationRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        CodeGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        _context.CodeGenerationRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await GetByIdAsync(id, cancellationToken);
        if (request != null)
        {
            _context.CodeGenerationRequests.Remove(request);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

---

## Missing Domain Code

Before we can use the repositories, we need to add the missing CodeGenerationRequest aggregate. Let me create that:

### CodeGenerationRequest.cs (Domain Layer)

```csharp
using FormCodeGenerator.Domain.Events;
using FormCodeGenerator.Domain.ValueObjects;

namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// CodeGenerationRequest aggregate root
/// </summary>
public class CodeGenerationRequest
{
    public Guid Id { get; private set; }
    public Guid FormId { get; private set; }
    public string FormName { get; private set; }
    public string? FormPurpose { get; private set; }
    public CodeNamespace TargetNamespace { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public CodeGenerationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<GeneratedCode> _generatedCodes;
    public IReadOnlyList<GeneratedCode> GeneratedCodes => _generatedCodes.AsReadOnly();

    public CodeAnalysis? Analysis { get; private set; }

    // Domain events
    private readonly List<object> _domainEvents;
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private CodeGenerationRequest()
    {
        // Required by EF Core
        FormName = string.Empty;
        TargetNamespace = new CodeNamespace("MyApp");
        _generatedCodes = new List<GeneratedCode>();
        _domainEvents = new List<object>();
        Status = CodeGenerationStatus.Pending;
    }

    public CodeGenerationRequest(
        Guid formId,
        string formName,
        string? formPurpose,
        CodeNamespace targetNamespace)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty", nameof(formId));

        if (string.IsNullOrWhiteSpace(formName))
            throw new ArgumentException("Form name cannot be empty", nameof(formName));

        Id = Guid.NewGuid();
        FormId = formId;
        FormName = formName;
        FormPurpose = formPurpose;
        TargetNamespace = targetNamespace ?? throw new ArgumentNullException(nameof(targetNamespace));
        RequestedAt = DateTime.UtcNow;
        Status = CodeGenerationStatus.Pending;
        
        _generatedCodes = new List<GeneratedCode>();
        _domainEvents = new List<object>();
    }

    public void AddGeneratedCode(string codeType, string code, string language)
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Cannot add code to a completed or failed request");

        var generatedCode = new GeneratedCode(codeType, code, language);
        _generatedCodes.Add(generatedCode);
    }

    public void SetAnalysis(CodeAnalysis analysis)
    {
        Analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
    }

    public void MarkAsCompleted()
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Request is not in pending state");

        if (!_generatedCodes.Any())
            throw new InvalidOperationException("Cannot complete request without generated code");

        Status = CodeGenerationStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        // Raise domain event
        _domainEvents.Add(new CodeGeneratedEvent(
            Id,
            FormId,
            FormName,
            TargetNamespace.Value,
            _generatedCodes.Count));
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status != CodeGenerationStatus.Pending)
            throw new InvalidOperationException("Request is not in pending state");

        Status = CodeGenerationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### GeneratedCode.cs (Entity)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Generated code entity
/// </summary>
public class GeneratedCode
{
    public Guid Id { get; private set; }
    public string CodeType { get; private set; }
    public string Code { get; private set; }
    public string Language { get; private set; }

    private GeneratedCode()
    {
        // Required by EF Core
        CodeType = string.Empty;
        Code = string.Empty;
        Language = string.Empty;
    }

    public GeneratedCode(string codeType, string code, string language)
    {
        if (string.IsNullOrWhiteSpace(codeType))
            throw new ArgumentException("Code type cannot be empty", nameof(codeType));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be empty", nameof(language));

        Id = Guid.NewGuid();
        CodeType = codeType;
        Code = code;
        Language = language;
    }
}
```

### CodeGenerationStatus.cs (Enum)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Status of code generation request
/// </summary>
public enum CodeGenerationStatus
{
    Pending,
    Completed,
    Failed
}
```

### CodeAnalysis.cs (Value Object)

```csharp
namespace FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

/// <summary>
/// Analysis of generated code
/// </summary>
public sealed class CodeAnalysis : IEquatable<CodeAnalysis>
{
    public string DetectedPurpose { get; }
    public string Complexity { get; }
    public bool HasRepeatingData { get; }
    public string RecommendedApproach { get; }

    public CodeAnalysis(
        string detectedPurpose,
        string complexity,
        bool hasRepeatingData,
        string recommendedApproach)
    {
        if (string.IsNullOrWhiteSpace(detectedPurpose))
            throw new ArgumentException("Detected purpose cannot be empty", nameof(detectedPurpose));

        if (string.IsNullOrWhiteSpace(complexity))
            throw new ArgumentException("Complexity cannot be empty", nameof(complexity));

        if (string.IsNullOrWhiteSpace(recommendedApproach))
            throw new ArgumentException("Recommended approach cannot be empty", nameof(recommendedApproach));

        DetectedPurpose = detectedPurpose;
        Complexity = complexity;
        HasRepeatingData = hasRepeatingData;
        RecommendedApproach = recommendedApproach;
    }

    public bool Equals(CodeAnalysis? other)
    {
        if (other is null) return false;
        return DetectedPurpose == other.DetectedPurpose &&
               Complexity == other.Complexity &&
               HasRepeatingData == other.HasRepeatingData &&
               RecommendedApproach == other.RecommendedApproach;
    }

    public override bool Equals(object? obj) => Equals(obj as CodeAnalysis);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(DetectedPurpose, Complexity, HasRepeatingData, RecommendedApproach);
    }
}
```

### ICodeGenerationRepository.cs (Domain Interface)

```csharp
using FormCodeGenerator.Domain.Aggregates.CodeGenerationAggregate;

namespace FormCodeGenerator.Domain.Interfaces;

/// <summary>
/// Repository for CodeGenerationRequest aggregate
/// </summary>
public interface ICodeGenerationRepository
{
    Task<CodeGenerationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetByFormIdAsync(Guid formId, CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<CodeGenerationRequest>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CodeGenerationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(CodeGenerationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

---

## Configuration Options

### AzureAIOptions.cs

```csharp
namespace FormCodeGenerator.Infrastructure.Options;

/// <summary>
/// Azure AI service configuration options
/// </summary>
public class AzureAIOptions
{
    public const string SectionName = "Azure:AI";

    public DocumentIntelligenceOptions DocumentIntelligence { get; set; } = new();
    public OpenAIOptions OpenAI { get; set; } = new();
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
```

### PersistenceOptions.cs

```csharp
namespace FormCodeGenerator.Infrastructure.Options;

/// <summary>
/// Persistence configuration options
/// </summary>
public class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}
```

---

## Event Publishing

### IEventPublisher.cs (Application Interface)

```csharp
namespace FormCodeGenerator.Application.Interfaces;

/// <summary>
/// Publishes domain events
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
```

### EventPublisher.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Application.Interfaces;

namespace FormCodeGenerator.Infrastructure.Events;

/// <summary>
/// Simple in-memory event publisher
/// For production, replace with Azure Service Bus, RabbitMQ, etc.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventType = @event.GetType().Name;
        
        _logger.LogInformation("Publishing event: {EventType}", eventType);
        
        // In production, publish to message queue/event bus
        // For now, just log it
        _logger.LogDebug("Event data: {@Event}", @event);

        return Task.CompletedTask;
    }
}
```

### Event Handlers (Examples)

#### FormAnalyzedEventHandler.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Events;

namespace FormCodeGenerator.Infrastructure.Events.EventHandlers;

/// <summary>
/// Handles FormAnalyzedEvent
/// </summary>
public class FormAnalyzedEventHandler
{
    private readonly ILogger<FormAnalyzedEventHandler> _logger;

    public FormAnalyzedEventHandler(ILogger<FormAnalyzedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FormAnalyzedEvent @event)
    {
        _logger.LogInformation(
            "Form analyzed: {FileName} (ID: {FormId}), {FieldCount} fields, {TableCount} tables",
            @event.FileName,
            @event.FormId,
            @event.FieldCount,
            @event.TableCount);

        // Could trigger notifications, analytics, etc.

        return Task.CompletedTask;
    }
}
```

#### CodeGeneratedEventHandler.cs

```csharp
using Microsoft.Extensions.Logging;
using FormCodeGenerator.Domain.Events;

namespace FormCodeGenerator.Infrastructure.Events.EventHandlers;

/// <summary>
/// Handles CodeGeneratedEvent
/// </summary>
public class CodeGeneratedEventHandler
{
    private readonly ILogger<CodeGeneratedEventHandler> _logger;

    public CodeGeneratedEventHandler(ILogger<CodeGeneratedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CodeGeneratedEvent @event)
    {
        _logger.LogInformation(
            "Code generated: {FormName} (Request: {RequestId}), {FileCount} files",
            @event.FormName,
            @event.RequestId,
            @event.GeneratedFilesCount);

        // Could send notifications, save to blob storage, etc.

        return Task.CompletedTask;
    }
}
```

---

## Dependency Injection Setup

### InfrastructureServiceExtensions.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Application.Interfaces;
using FormCodeGenerator.Domain.Interfaces;
using FormCodeGenerator.Infrastructure.AI.AzureDocumentIntelligence;
using FormCodeGenerator.Infrastructure.AI.AzureOpenAI;
using FormCodeGenerator.Infrastructure.Events;
using FormCodeGenerator.Infrastructure.Events.EventHandlers;
using FormCodeGenerator.Infrastructure.Options;
using FormCodeGenerator.Infrastructure.Persistence;
using FormCodeGenerator.Infrastructure.Persistence.Repositories;

namespace FormCodeGenerator.Infrastructure.Configuration;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration options
        services.Configure<AzureAIOptions>(
            configuration.GetSection(AzureAIOptions.SectionName));

        services.Configure<PersistenceOptions>(
            configuration.GetSection(PersistenceOptions.SectionName));

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            // Enable sensitive data logging only in development
            if (configuration.GetValue<bool>("Persistence:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Repositories
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<ICodeGenerationRepository, CodeGenerationRepository>();

        // Azure AI Services
        services.AddScoped<IFormExtractor, AzureFormExtractor>();
        services.AddScoped<ICodeGenerator, AzureCodeGenerator>();

        // Event Publishing
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<FormAnalyzedEventHandler>();
        services.AddSingleton<CodeGeneratedEventHandler>();

        return services;
    }

    public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
}
```

---

## appsettings.json Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "FormCodeGenerator": "Debug"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FormCodeGeneratorDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },

  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-DOCUMENT-INTELLIGENCE-KEY"
      },
      "OpenAI": {
        "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
        "ApiKey": "YOUR-OPENAI-KEY",
        "DeploymentName": "gpt-4",
        "Temperature": 0.3,
        "MaxTokens": 4000
      }
    }
  },

  "Persistence": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false
  }
}
```

---

## Database Migrations

### Creating Initial Migration

```bash
# From solution root
dotnet ef migrations add InitialCreate \
  --project src/FormCodeGenerator.Infrastructure \
  --startup-project src/FormCodeGenerator.API \
  --output-dir Persistence/Migrations
```

### Applying Migrations

```bash
# Apply to database
dotnet ef database update \
  --project src/FormCodeGenerator.Infrastructure \
  --startup-project src/FormCodeGenerator.API

# Or programmatically in Program.cs:
# await app.Services.MigrateDatabase();
```

### Migration File Example

The generated migration will create:
- Forms table
- FormFields table
- FormTables table
- CodeGenerationRequests table
- GeneratedCodes table
- All necessary indexes and foreign keys

---

## Testing Infrastructure

### Integration Test Example

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormCodeGenerator.Infrastructure.Persistence;
using FormCodeGenerator.Infrastructure.Configuration;

namespace FormCodeGenerator.Infrastructure.IntegrationTests;

public class InfrastructureTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public ApplicationDbContext DbContext { get; }

    public InfrastructureTestFixture()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        // Add infrastructure services
        services.AddInfrastructureServices(configuration);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
```

---

## Summary

The Infrastructure layer provides:

✅ **Azure Document Intelligence** - PDF form extraction  
✅ **Azure OpenAI** - Code generation  
✅ **Entity Framework Core** - Persistence  
✅ **Repositories** - Data access  
✅ **Event Publishing** - Domain events  
✅ **Configuration** - Options pattern  
✅ **Dependency Injection** - Service registration  


## Verification

- [ ] All tests pass
- [ ] Code compiles

## Next Steps

[Continue to next phase.](05-PHASE-5-FORM-API.md)
