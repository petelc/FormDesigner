using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Infrastructure.DocumentIntelligence;

/// <summary>
/// Real Azure Document Intelligence implementation for extracting form structure from PDFs
/// </summary>
public class AzureFormExtractorService : IFormExtractor
{
    private readonly DocumentAnalysisClient _client;
    private readonly ILogger<AzureFormExtractorService> _logger;
    private readonly string _modelId;

    public AzureFormExtractorService(
        IConfiguration configuration,
        ILogger<AzureFormExtractorService> logger)
    {
        _logger = logger;

        var endpoint = configuration["AzureDocumentIntelligence:Endpoint"]
            ?? throw new InvalidOperationException("Azure Document Intelligence Endpoint not configured");
        var key = configuration["AzureDocumentIntelligence:Key"]
            ?? throw new InvalidOperationException("Azure Document Intelligence Key not configured");
        _modelId = configuration["AzureDocumentIntelligence:Model"] ?? "prebuilt-document";

        var credential = new AzureKeyCredential(key);
        _client = new DocumentAnalysisClient(new Uri(endpoint), credential);

        _logger.LogInformation(
            "AzureFormExtractorService initialized with endpoint: {Endpoint}, model: {Model}",
            endpoint,
            _modelId);
    }

    public async Task<string> DetectFormTypeAsync(string pdfPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting form type for: {PdfPath}", pdfPath);

        try
        {
            // Use Document Intelligence to analyze the document
            await using var stream = File.OpenRead(pdfPath);

            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _modelId,
                stream,
                cancellationToken: cancellationToken);

            var result = operation.Value;

            // Analyze content to determine form type
            var formType = InferFormType(result);

            _logger.LogInformation("Detected form type: {FormType}", formType);
            return formType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting form type for {PdfPath}", pdfPath);
            return "generic"; // Fallback to generic
        }
    }

    public async Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        string formType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Extracting form structure from {PdfPath} as {FormType}",
            pdfPath,
            formType);

        try
        {
            await using var stream = File.OpenRead(pdfPath);

            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _modelId,
                stream,
                cancellationToken: cancellationToken);

            var result = operation.Value;

            // Extract fields from key-value pairs
            var fields = ExtractFields(result);

            // Extract tables
            var tables = ExtractTables(result);

            // Generate warnings
            var warnings = new List<string>();
            if (result.Pages.Count > 1)
            {
                warnings.Add($"Multi-page document detected ({result.Pages.Count} pages). Only fields from all pages will be extracted.");
            }

            _logger.LogInformation(
                "Extracted {FieldCount} fields and {TableCount} tables from {PdfPath}",
                fields.Count,
                tables.Count,
                pdfPath);

            return new ExtractedFormStructure
            {
                Fields = fields,
                Tables = tables,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting form structure from {PdfPath}", pdfPath);
            throw;
        }
    }

    private string InferFormType(AnalyzeResult result)
    {
        // Analyze document content to infer form type
        var allText = string.Join(" ", result.Pages.SelectMany(p => p.Lines.Select(l => l.Content)));
        var lowerText = allText.ToLowerInvariant();

        if (lowerText.Contains("application") && (lowerText.Contains("employment") || lowerText.Contains("job")))
            return "application";

        if (lowerText.Contains("registration") || lowerText.Contains("register"))
            return "registration";

        if (lowerText.Contains("contact") && lowerText.Contains("information"))
            return "contact";

        if (lowerText.Contains("survey") || lowerText.Contains("questionnaire"))
            return "survey";

        return "generic";
    }

    private List<ExtractedField> ExtractFields(AnalyzeResult result)
    {
        var fields = new List<ExtractedField>();

        // Extract from key-value pairs
        foreach (var kvp in result.KeyValuePairs)
        {
            if (kvp.Key?.Content == null) continue;

            var fieldName = SanitizeFieldName(kvp.Key.Content);
            var fieldType = InferFieldType(kvp.Key.Content, kvp.Value?.Content);

            fields.Add(new ExtractedField
            {
                Name = fieldName,
                Label = kvp.Key.Content,
                Type = fieldType,
                IsRequired = false, // Azure doesn't detect required fields
                DefaultValue = kvp.Value?.Content
            });
        }

        // If no key-value pairs, extract from form fields (if using form model)
        if (result.Documents?.Any() == true)
        {
            foreach (var document in result.Documents)
            {
                foreach (var field in document.Fields)
                {
                    var fieldName = SanitizeFieldName(field.Key);
                    var fieldValue = field.Value;

                    fields.Add(new ExtractedField
                    {
                        Name = fieldName,
                        Label = field.Key,
                        Type = MapDocumentFieldType(fieldValue),
                        IsRequired = false,
                        DefaultValue = fieldValue.Content
                    });
                }
            }
        }

        return fields;
    }

    private List<ExtractedTable> ExtractTables(AnalyzeResult result)
    {
        var tables = new List<ExtractedTable>();

        foreach (var table in result.Tables)
        {
            var extractedTable = new ExtractedTable
            {
                RowCount = table.RowCount,
                ColumnCount = table.ColumnCount,
                Cells = table.Cells.Select(cell => new ExtractedCell
                {
                    Content = cell.Content ?? string.Empty,
                    RowIndex = cell.RowIndex,
                    ColumnIndex = cell.ColumnIndex,
                    IsHeader = cell.Kind == DocumentTableCellKind.ColumnHeader ||
                               cell.Kind == DocumentTableCellKind.RowHeader
                }).ToList()
            };

            tables.Add(extractedTable);
        }

        return tables;
    }

    private string SanitizeFieldName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Field" + Guid.NewGuid().ToString("N").Substring(0, 8);

        // Remove special characters, keep only alphanumeric and underscores
        var sanitized = new string(input
            .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == ' ')
            .ToArray());

        // Convert to PascalCase
        var words = sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var pascalCase = string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + w.Substring(1).ToLower()));

        return string.IsNullOrEmpty(pascalCase) ? "Field" : pascalCase;
    }

    private string InferFieldType(string key, string? value)
    {
        var lowerKey = key.ToLowerInvariant();

        if (lowerKey.Contains("email"))
            return "email";
        if (lowerKey.Contains("phone") || lowerKey.Contains("tel"))
            return "tel";
        if (lowerKey.Contains("date"))
            return "date";
        if (lowerKey.Contains("time"))
            return "time";
        if (lowerKey.Contains("url") || lowerKey.Contains("website"))
            return "url";
        if (lowerKey.Contains("password"))
            return "password";
        if (lowerKey.Contains("number") || lowerKey.Contains("age") || lowerKey.Contains("quantity"))
            return "number";
        if (lowerKey.Contains("address") || lowerKey.Contains("description") || lowerKey.Contains("comments"))
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

    private string MapDocumentFieldType(DocumentField field)
    {
        return field.FieldType switch
        {
            DocumentFieldType.String => "text",
            DocumentFieldType.Date => "date",
            DocumentFieldType.Time => "time",
            DocumentFieldType.PhoneNumber => "tel",
            DocumentFieldType.Double or DocumentFieldType.Int64 => "number",
            DocumentFieldType.SelectionMark => "checkbox",
            DocumentFieldType.Signature => "text", // Signatures can't be input fields
            _ => "text"
        };
    }
}
