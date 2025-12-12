# Infrastructure Layer Implementation Guide

## Overview

The Infrastructure layer implements external dependencies: Azure Document Intelligence, Azure OpenAI, Entity Framework Core persistence, and event publishing.

---

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

Let us continue with the Code Generation Request configurations and repositories in the next part.

Continue? [Infrastructure Implementation (part 2)](Infrastructure_Implementation_Part2.md)


