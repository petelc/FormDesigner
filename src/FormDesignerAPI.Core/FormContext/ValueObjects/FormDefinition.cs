using System.Text.Json;
using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Represents the structure and fields of a form
/// Immutable value object
/// </summary>
public class FormDefinition : ValueObject
{
    public string Schema { get; init; } = string.Empty;
    public IReadOnlyList<FormField> Fields { get; init; } = new List<FormField>();

    // Private constructor
    private FormDefinition() { }

    /// <summary>
    /// Create a form definition from a JSON schema
    /// </summary>
    public static FormDefinition From(string jsonSchema)
    {
        if (string.IsNullOrWhiteSpace(jsonSchema))
            throw new ArgumentException("JSON schema cannot be empty", nameof(jsonSchema));

        var fields = ParseFields(jsonSchema);

        return new FormDefinition
        {
            Schema = jsonSchema,
            Fields = fields
        };
    }

    /// <summary>
    /// Create a form definition from a list of fields
    /// </summary>
    public static FormDefinition FromFields(List<FormField> fields)
    {
        if (fields == null || fields.Count == 0)
            throw new ArgumentException("Fields list cannot be empty", nameof(fields));

        // Validate all fields
        if (fields.Any(f => !f.IsValid()))
            throw new ArgumentException("One or more fields are invalid");

        var schema = GenerateSchema(fields);

        return new FormDefinition
        {
            Schema = schema,
            Fields = fields.AsReadOnly()
        };
    }

    private static List<FormField> ParseFields(string jsonSchema)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonSchema);
            var root = doc.RootElement;

            var fields = new List<FormField>();

            // Check if there's a "fields" array
            if (root.TryGetProperty("fields", out var fieldsArray))
            {
                foreach (var fieldElement in fieldsArray.EnumerateArray())
                {
                    var field = ParseField(fieldElement);
                    if (field != null)
                        fields.Add(field);
                }
            }

            return fields;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON schema: {ex.Message}", nameof(jsonSchema));
        }
    }

    private static FormField? ParseField(JsonElement element)
    {
        try
        {
            var name = element.GetProperty("name").GetString() ?? string.Empty;
            var type = element.GetProperty("type").GetString() ?? string.Empty;

            var field = new FormField
            {
                Name = name,
                Type = type,
                Required = element.TryGetProperty("required", out var req) && req.GetBoolean(),
                Label = element.TryGetProperty("label", out var lbl) ? lbl.GetString() : null,
                Placeholder = element.TryGetProperty("placeholder", out var ph) ? ph.GetString() : null,
                DefaultValue = element.TryGetProperty("defaultValue", out var dv) ? dv.GetString() : null,
                MinLength = element.TryGetProperty("minLength", out var min) ? min.GetInt32() : null,
                MaxLength = element.TryGetProperty("maxLength", out var max) ? max.GetInt32() : null,
                Pattern = element.TryGetProperty("pattern", out var pat) ? pat.GetString() : null
            };

            // Parse options for select/radio fields
            if (element.TryGetProperty("options", out var opts))
            {
                var options = new List<string>();
                foreach (var opt in opts.EnumerateArray())
                {
                    var optValue = opt.GetString();
                    if (!string.IsNullOrEmpty(optValue))
                        options.Add(optValue);
                }
                field = field with { Options = options };
            }

            return field;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSchema(List<FormField> fields)
    {
        var schema = new
        {
            fields = fields.Select(f => new
            {
                name = f.Name,
                type = f.Type,
                required = f.Required,
                label = f.Label,
                placeholder = f.Placeholder,
                defaultValue = f.DefaultValue,
                minLength = f.MinLength,
                maxLength = f.MaxLength,
                pattern = f.Pattern,
                options = f.Options
            })
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Get a specific field by name
    /// </summary>
    public FormField? GetField(string fieldName)
    {
        return Fields.FirstOrDefault(f =>
          f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if definition has a specific field
    /// </summary>
    public bool HasField(string fieldName)
    {
        return Fields.Any(f =>
          f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Schema;
        foreach (var field in Fields)
        {
            yield return field;
        }
    }
}