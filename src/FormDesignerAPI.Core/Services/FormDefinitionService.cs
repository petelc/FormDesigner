using System.Text.Json;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.Services;

public class FormDefinitionService(ILogger<FormDefinitionService> logger) : IFormDefinitionService
{
    public Task<Result> GenerateFormDefinitionAsync(int formId, string json, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Form Definition for Form {formId}", formId);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var flat = new Dictionary<string, List<JsonElement>>();
            FlattenJsonAdvanced(doc.RootElement, "", flat);

            // Infer types for root object
            //var columnDefs = flat.Select(kvp => $"[{kvp.Key}] {InferSqlTypeAdvanced(kvp.Value)}").ToArray(); // I do not need to infer types here
            // This needs to be more generic in a real implementation
            // string createTableSql = $"CREATE TABLE [User] ({string.Join(", ", columnDefs)});";
            // Console.WriteLine("Root Table:");
            // Console.WriteLine(createTableSql);

            // Handle arrays of objects (e.g., "children")
            // This needs to be more generic in a real implementation
            if (doc.RootElement.TryGetProperty("children", out var childrenElements) && childrenElements.ValueKind == JsonValueKind.Array)
            {
                var childrenColumns = new Dictionary<string, List<JsonElement>>();
                // Loop over the child array
                foreach (var child in childrenElements.EnumerateArray())
                {

                    foreach (var prop in child.EnumerateObject())
                    {
                        if (!childrenColumns.ContainsKey(prop.Name))
                            childrenColumns[prop.Name] = new List<JsonElement>();
                        childrenColumns[prop.Name].Add(prop.Value);
                    }
                }
                // Add a foreign key column to link to the parent table
                // This assumes the parent table is called "User" and has a primary key "Id"
                // this needs to be more generic in a real implementation
                childrenColumns["UserId"] = new List<JsonElement>();
                var childrenColumnDefs = childrenColumns.Select(kvp =>
                    $"[{kvp.Key}] {(kvp.Key == "UserId" ? "NVARCHAR(MAX)" : InferSqlTypeAdvanced(kvp.Value))}"
                ).ToArray();
                string createChildrenTableSql = $"CREATE TABLE [Children] ({string.Join(", ", childrenColumnDefs)});";
                Console.WriteLine("\nChildren Table:"); // Fixed: Was \\n instead of \n
                Console.WriteLine(createChildrenTableSql);
                // this is where the sql file is created and saved to disk
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating form definition for Form {formId}", formId);
            return Task.FromResult(Result.Error($"Failed to generate form definition: {ex.Message}"));
        }
    }

    // Flattens JSON, collects all values for each key for type inference
    static void FlattenJsonAdvanced(JsonElement element, string prefix, Dictionary<string, List<JsonElement>> flat)
    {
        foreach (var prop in element.EnumerateObject())
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonAdvanced(prop.Value, key, flat);
            }
            else if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                // If array of primitives, store as JSON string
                if (prop.Value.EnumerateArray().All(e => e.ValueKind != JsonValueKind.Object))
                {
                    if (!flat.ContainsKey(key))
                        flat[key] = new List<JsonElement>();
                    flat[key].Add(prop.Value);
                }
                // If array of objects, handle separately (see above)
            }
            else
            {
                if (!flat.ContainsKey(key))
                    flat[key] = new List<JsonElement>();
                flat[key].Add(prop.Value);
            }
        }
    }

    // Infers type from all values for a key
    private string InferSqlTypeAdvanced(List<JsonElement> values)
    {
        bool hasString = false, hasInt = false, hasFloat = false, hasBool = false, hasNull = false;

        foreach (var v in values)
        {
            switch (v.ValueKind)
            {
                case JsonValueKind.String:
                    hasString = true;
                    break;
                case JsonValueKind.Number:
                    if (v.TryGetInt32(out _)) hasInt = true;
                    else hasFloat = true;
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    hasBool = true;
                    break;
                case JsonValueKind.Null:
                    hasNull = true;
                    break;
                default:
                    hasString = true; // fallback for objects/arrays
                    break;
            }
        }
        if (hasBool && !hasString && !hasInt && !hasFloat) return "BIT";
        if (hasString) return "NVARCHAR(MAX)";
        if (hasFloat) return "FLOAT";
        if (hasInt) return "INT";
        if (hasBool) return "BIT";
        if (hasNull) return "NVARCHAR(MAX)";
        return "NVARCHAR(MAX)";
    }
}

