using FormDesignerAPI.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Text;

public class HtmlFormElementParser : IHtmlFormElementParser
{
    public Dictionary<string, string> ExtractFormElements(string htmlContent)
    {
        var formElements = new Dictionary<string, string>();

        // Regex patterns for different HTML form elements
        var patterns = new[]
        {
            @"<input[^>]+name\s*=\s*[""']([^""']+)[""'][^>]*type\s*=\s*[""']([^""']+)[""'][^>]*>",
            @"<input[^>]+type\s*=\s*[""']([^""']+)[""'][^>]*name\s*=\s*[""']([^""']+)[""'][^>]*>",
            @"<textarea[^>]+name\s*=\s*[""']([^""']+)[""'][^>]*>",
            @"<select[^>]+name\s*=\s*[""']([^""']+)[""'][^>]*>"
        };

        // Pattern for input with name and type (name first)
        var inputPattern1 = new Regex(patterns[0], RegexOptions.IgnoreCase);
        foreach (Match match in inputPattern1.Matches(htmlContent))
        {
            string name = match.Groups[1].Value;
            string type = match.Groups[2].Value;
            formElements[name] = MapHtmlTypeToSqlType(type);
        }

        // Pattern for input with type and name (type first)
        var inputPattern2 = new Regex(patterns[1], RegexOptions.IgnoreCase);
        foreach (Match match in inputPattern2.Matches(htmlContent))
        {
            string type = match.Groups[1].Value;
            string name = match.Groups[2].Value;
            if (!formElements.ContainsKey(name))
            {
                formElements[name] = MapHtmlTypeToSqlType(type);
            }
        }

        // Pattern for textarea
        var textareaPattern = new Regex(patterns[2], RegexOptions.IgnoreCase);
        foreach (Match match in textareaPattern.Matches(htmlContent))
        {
            string name = match.Groups[1].Value;
            formElements[name] = "NVARCHAR(MAX)";
        }

        // Pattern for select
        var selectPattern = new Regex(patterns[3], RegexOptions.IgnoreCase);
        foreach (Match match in selectPattern.Matches(htmlContent))
        {
            string name = match.Groups[1].Value;
            formElements[name] = "NVARCHAR(MAX)";
        }

        return formElements;
    }

    public string GenerateCreateTableSql(string tableName, Dictionary<string, string> columns)
    {
        if (!columns.Any())
            return string.Empty;

        var columnDefs = columns.Select(kvp => $"[{kvp.Key}] {kvp.Value}").ToArray();
        return $"CREATE TABLE [{tableName}] ({string.Join(", ", columnDefs)});";
    }

    public string GenerateCSharpModel(string className, Dictionary<string, string> columns)
    {
        if (!columns.Any())
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        foreach (var kvp in columns)
        {
            string csharpType = MapSqlTypeToCSharpType(kvp.Value);
            string propertyName = ToPascalCase(kvp.Key);
            sb.AppendLine($"    public {csharpType} {propertyName} {{ get; set; }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string MapHtmlTypeToSqlType(string htmlType)
    {
        return htmlType.ToLower() switch
        {
            "text" => "NVARCHAR(255)",
            "email" => "NVARCHAR(255)",
            "password" => "NVARCHAR(255)",
            "number" => "INT",
            "decimal" => "DECIMAL(18,2)",
            "date" => "DATE",
            "datetime-local" => "DATETIME2",
            "time" => "TIME",
            "checkbox" => "BIT",
            "radio" => "NVARCHAR(100)",
            "hidden" => "NVARCHAR(255)",
            "tel" => "NVARCHAR(20)",
            "url" => "NVARCHAR(500)",
            _ => "NVARCHAR(255)"
        };
    }

    private string MapSqlTypeToCSharpType(string sqlType)
    {
        return sqlType.ToUpper() switch
        {
            var t when t.StartsWith("NVARCHAR") => "string",
            var t when t.StartsWith("VARCHAR") => "string",
            "INT" => "int",
            "BIT" => "bool",
            "DECIMAL(18,2)" => "decimal",
            "DATE" => "DateTime",
            "DATETIME2" => "DateTime",
            "TIME" => "TimeSpan",
            _ => "string"
        };
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input[1..];
    }
}