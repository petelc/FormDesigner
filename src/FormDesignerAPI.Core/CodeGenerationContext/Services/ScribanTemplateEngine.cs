// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/Services/ScribanTemplateEngine.cs

using Scriban;
using Scriban.Runtime;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Core.CodeGenerationContext.Services;

/// <summary>
/// Service to render Scriban templates with caching and custom functions
/// </summary>
public class ScribanTemplateEngine
{
  private readonly ILogger<ScribanTemplateEngine> _logger;
  private readonly Dictionary<string, Template> _compiledTemplates;

  public ScribanTemplateEngine(ILogger<ScribanTemplateEngine> logger)
  {
    _logger = logger;
    _compiledTemplates = new Dictionary<string, Template>();
  }

  /// <summary>
  /// Render a template with the provided model
  /// </summary>
  public async Task<string> RenderAsync(
    string templateContent,
    object model,
    string templateName = "unknown")
  {
    try
    {
      _logger.LogDebug("Rendering template: {TemplateName}", templateName);

      // Parse and compile template (with caching)
      var template = GetOrCompileTemplate(templateContent, templateName);

      // Create script object with model
      var scriptObject = new ScriptObject();
      scriptObject.Import(model, renamer: member => member.Name);

      // Add custom functions
      AddCustomFunctions(scriptObject);

      // Create template context
      var context = new TemplateContext();
      context.PushGlobal(scriptObject);

      // Render
      var result = await template.RenderAsync(context);

      _logger.LogDebug(
        "Template rendered successfully: {TemplateName}, Size: {Size} bytes",
        templateName,
        result.Length);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error rendering template: {TemplateName}", templateName);
      throw new InvalidOperationException(
        $"Template rendering failed for '{templateName}': {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Render from a file path
  /// </summary>
  public async Task<string> RenderFromFileAsync(
    string templateFilePath,
    object model)
  {
    if (!File.Exists(templateFilePath))
    {
      throw new FileNotFoundException(
        $"Template file not found: {templateFilePath}");
    }

    var templateContent = await File.ReadAllTextAsync(templateFilePath);
    var templateName = Path.GetFileName(templateFilePath);

    return await RenderAsync(templateContent, model, templateName);
  }

  /// <summary>
  /// Get or compile template (with caching for performance)
  /// </summary>
  private Template GetOrCompileTemplate(string templateContent, string templateName)
  {
    // Check cache
    if (_compiledTemplates.TryGetValue(templateName, out var cachedTemplate))
    {
      _logger.LogTrace("Using cached template: {TemplateName}", templateName);
      return cachedTemplate;
    }

    // Parse and compile
    _logger.LogDebug("Compiling template: {TemplateName}", templateName);
    var template = Template.Parse(templateContent);

    if (template.HasErrors)
    {
      var errors = string.Join(", ", template.Messages.Select(m => m.Message));
      throw new InvalidOperationException(
        $"Template '{templateName}' has syntax errors: {errors}");
    }

    // Cache it
    _compiledTemplates[templateName] = template;

    return template;
  }

  /// <summary>
  /// Add custom functions available in all templates
  /// </summary>
  private void AddCustomFunctions(ScriptObject scriptObject)
  {
    // String manipulation functions
    scriptObject.Import(nameof(ToPascalCase), new Func<string, string>(ToPascalCase));
    scriptObject.Import(nameof(ToCamelCase), new Func<string, string>(ToCamelCase));
    scriptObject.Import(nameof(ToSnakeCase), new Func<string, string>(ToSnakeCase));
    scriptObject.Import(nameof(Pluralize), new Func<string, string>(Pluralize));
    scriptObject.Import(nameof(Singularize), new Func<string, string>(Singularize));

    // Current date/time
    scriptObject["now"] = DateTime.UtcNow;
    scriptObject["today"] = DateTime.UtcNow.Date;
  }

  // ============================================================================
  // CUSTOM HELPER FUNCTIONS (Available in all templates)
  // ============================================================================

  /// <summary>
  /// Convert to PascalCase (e.g., "user_name" -> "UserName")
  /// </summary>
  private static string ToPascalCase(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return input;

    // Handle snake_case and kebab-case
    var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

    return string.Join("", words.Select(w =>
      char.ToUpper(w[0]) + w.Substring(1).ToLower()));
  }

  /// <summary>
  /// Convert to camelCase (e.g., "user_name" -> "userName")
  /// </summary>
  private static string ToCamelCase(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return input;

    var pascal = ToPascalCase(input);
    return char.ToLower(pascal[0]) + pascal.Substring(1);
  }

  /// <summary>
  /// Convert to snake_case (e.g., "UserName" -> "user_name")
  /// </summary>
  private static string ToSnakeCase(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return input;

    var result = new System.Text.StringBuilder();
    result.Append(char.ToLower(input[0]));

    for (int i = 1; i < input.Length; i++)
    {
      if (char.IsUpper(input[i]))
      {
        result.Append('_');
        result.Append(char.ToLower(input[i]));
      }
      else
      {
        result.Append(input[i]);
      }
    }

    return result.ToString();
  }

  /// <summary>
  /// Simple pluralization (e.g., "User" -> "Users")
  /// Note: This is basic - for production, consider using Humanizer library
  /// </summary>
  private static string Pluralize(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return input;

    // Handle common cases
    if (input.EndsWith("y", StringComparison.OrdinalIgnoreCase))
      return input.Substring(0, input.Length - 1) + "ies";

    if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
      return input + "es";

    return input + "s";
  }

  /// <summary>
  /// Simple singularization (e.g., "Users" -> "User")
  /// </summary>
  private static string Singularize(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
      return input;

    if (input.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
      return input.Substring(0, input.Length - 3) + "y";

    if (input.EndsWith("ses", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("xes", StringComparison.OrdinalIgnoreCase) ||
        input.EndsWith("zes", StringComparison.OrdinalIgnoreCase))
      return input.Substring(0, input.Length - 2);

    if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase))
      return input.Substring(0, input.Length - 1);

    return input;
  }

  /// <summary>
  /// Clear template cache (useful for development/testing)
  /// </summary>
  public void ClearCache()
  {
    _logger.LogInformation("Clearing template cache ({Count} templates)",
      _compiledTemplates.Count);
    _compiledTemplates.Clear();
  }

  /// <summary>
  /// Get count of cached templates
  /// </summary>
  public int GetCachedTemplateCount()
  {
    return _compiledTemplates.Count;
  }
}