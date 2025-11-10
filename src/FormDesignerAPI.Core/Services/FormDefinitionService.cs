using System.Text.Json;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.Core.Services;

public class FormDefinitionService(
    ILogger<FormDefinitionService> logger,
    IHtmlFormElementParser htmlParser) : IFormDefinitionService
{
    public Task<Result> GenerateFormDefinitionAsync(int formId, string json, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Form Definition for Form {formId}", formId);

        try
        {
            using var doc = JsonDocument.Parse(json);

            // Handle arrays of objects (e.g., "children")
            if (doc.RootElement.TryGetProperty("children", out var childrenElements) && childrenElements.ValueKind == JsonValueKind.Array)
            {
                var allFormElements = new Dictionary<string, string>();

                // Loop over the child array
                foreach (var child in childrenElements.EnumerateArray())
                {
                    if (child.TryGetProperty("content", out var contentElement))
                    {
                        string content = contentElement.GetString() ?? string.Empty;
                        var formElements = htmlParser.ExtractFormElements(content);

                        // Merge form elements (avoid duplicates)
                        foreach (var kvp in formElements)
                        {
                            if (!allFormElements.ContainsKey(kvp.Key))
                            {
                                allFormElements[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }

                // Generate SQL CREATE TABLE
                string createTableSql = htmlParser.GenerateCreateTableSql("FormData", allFormElements);
                Console.WriteLine("SQL CREATE TABLE:");
                Console.WriteLine(createTableSql);
                Console.WriteLine();

                // Generate C# Model
                string csharpModel = htmlParser.GenerateCSharpModel("FormDataModel", allFormElements);
                Console.WriteLine("C# Model:");
                Console.WriteLine(csharpModel);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating form definition for Form {formId}", formId);
            return Task.FromResult(Result.Error($"Failed to generate form definition: {ex.Message}"));
        }
    }

    // ...existing methods remain unchanged...
}