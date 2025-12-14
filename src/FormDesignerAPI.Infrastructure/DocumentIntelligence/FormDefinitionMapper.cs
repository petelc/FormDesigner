using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Infrastructure.DocumentIntelligence;

/// <summary>
/// Maps extracted form structure from Document Intelligence to FormDefinition value object
/// </summary>
public class FormDefinitionMapper
{
    private readonly ILogger<FormDefinitionMapper> _logger;

    public FormDefinitionMapper(ILogger<FormDefinitionMapper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Convert ExtractedFormStructure to FormDefinition
    /// </summary>
    public FormDefinition MapToFormDefinition(ExtractedFormStructure extracted)
    {
        _logger.LogInformation(
            "Mapping {FieldCount} extracted fields to FormDefinition",
            extracted.Fields.Count);

        var fields = extracted.Fields.Select(MapField).ToList();

        _logger.LogInformation(
            "Successfully mapped {Count} fields",
            fields.Count);

        return FormDefinition.FromFields(fields);
    }

    /// <summary>
    /// Map a single extracted field to FormField
    /// </summary>
    private FormField MapField(ExtractedField extracted)
    {
        return new FormField
        {
            Name = SanitizeFieldName(extracted.Name),
            Type = MapFieldType(extracted.Type),
            Label = extracted.Label ?? extracted.Name,
            Required = extracted.IsRequired,
            MaxLength = extracted.MaxLength,
            Pattern = extracted.ValidationPattern,
            Options = extracted.Options,
            Placeholder = GeneratePlaceholder(extracted)
        };
    }

    /// <summary>
    /// Sanitize field name to be a valid C# identifier
    /// </summary>
    private string SanitizeFieldName(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return "Field" + Guid.NewGuid().ToString("N").Substring(0, 8);

        // Remove invalid characters
        var sanitized = new string(fieldName
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());

        // Ensure it starts with a letter
        if (!char.IsLetter(sanitized.FirstOrDefault()))
            sanitized = "Field" + sanitized;

        // Convert to PascalCase
        return ToPascalCase(sanitized);
    }

    /// <summary>
    /// Map extracted field type to HTML input type
    /// </summary>
    private string MapFieldType(string extractedType)
    {
        return extractedType.ToLowerInvariant() switch
        {
            "text" => "text",
            "email" => "email",
            "tel" or "phone" => "tel",
            "number" => "number",
            "date" => "date",
            "datetime" => "datetime-local",
            "time" => "time",
            "url" => "url",
            "password" => "password",
            "textarea" => "textarea",
            "select" or "dropdown" => "select",
            "radio" => "radio",
            "checkbox" => "checkbox",
            "file" => "file",
            _ => "text" // Default to text
        };
    }

    /// <summary>
    /// Generate a placeholder hint for the field
    /// </summary>
    private string? GeneratePlaceholder(ExtractedField field)
    {
        return field.Type.ToLowerInvariant() switch
        {
            "email" => "user@example.com",
            "tel" or "phone" => "(555) 123-4567",
            "date" => "MM/DD/YYYY",
            "url" => "https://example.com",
            "number" => "Enter a number",
            _ => null
        };
    }

    /// <summary>
    /// Convert string to PascalCase
    /// </summary>
    private string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Split on non-alphanumeric characters
        var words = System.Text.RegularExpressions.Regex.Split(input, @"[^a-zA-Z0-9]+")
            .Where(w => !string.IsNullOrWhiteSpace(w));

        return string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }

    /// <summary>
    /// Extract tables as repeating fields (advanced feature)
    /// </summary>
    public List<FormField> MapTablesAsRepeatingFields(List<ExtractedTable> tables)
    {
        var fields = new List<FormField>();

        foreach (var table in tables)
        {
            // Get headers from first row
            var headers = table.Cells
                .Where(c => c.IsHeader && c.RowIndex == 0)
                .OrderBy(c => c.ColumnIndex)
                .Select(c => c.Content)
                .ToList();

            if (!headers.Any())
                continue;

            // Create a field for each column
            for (int i = 0; i < headers.Count; i++)
            {
                fields.Add(new FormField
                {
                    Name = SanitizeFieldName($"{headers[i]}_{i}"),
                    Label = headers[i],
                    Type = "text",
                    Required = false,
                    MaxLength = 200
                });
            }
        }

        return fields;
    }
}
