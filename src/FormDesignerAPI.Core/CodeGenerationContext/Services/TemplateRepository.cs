// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/Services/TemplateRepository.cs

using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Core.CodeGenerationContext.Services;

/// <summary>
/// Manages registration and retrieval of Scriban templates
/// </summary>
public class TemplateRepository
{
  private readonly string _templateBasePath;
  private readonly ILogger<TemplateRepository> _logger;
  private readonly Dictionary<string, TemplateMetadata> _templates;

  public TemplateRepository(
    string templateBasePath,
    ILogger<TemplateRepository> logger)
  {
    _templateBasePath = templateBasePath;
    _logger = logger;
    _templates = new Dictionary<string, TemplateMetadata>();
    
    RegisterTemplates();
  }

  /// <summary>
  /// Register all available templates
  /// </summary>
  private void RegisterTemplates()
  {
    _logger.LogInformation("Registering templates from: {Path}", _templateBasePath);

    // =========================================================================
    // C# TEMPLATES
    // =========================================================================

    Register(TemplateMetadata.Create(
      "CSharpEntity",
      ArtifactType.CSharpEntity,
      Path.Combine(_templateBasePath, "CSharp", "Entity.sbn"),
      "{EntityName}.cs",
      "CSharp",
      "Domain entity class with properties and factory methods",
      priority: 1
    ));

    Register(TemplateMetadata.Create(
      "CSharpInterface",
      ArtifactType.CSharpInterface,
      Path.Combine(_templateBasePath, "CSharp", "Interface.sbn"),
      "I{EntityName}Repository.cs",
      "CSharp",
      "Repository interface for the entity",
      priority: 2
    ));

    Register(TemplateMetadata.Create(
      "CSharpRepository",
      ArtifactType.CSharpRepository,
      Path.Combine(_templateBasePath, "CSharp", "Repository.sbn"),
      "{EntityName}Repository.cs",
      "CSharp",
      "EF Core repository implementation",
      priority: 3
    ));

    Register(TemplateMetadata.Create(
      "CSharpController",
      ArtifactType.CSharpController,
      Path.Combine(_templateBasePath, "CSharp", "Controller.sbn"),
      "{EntityName}Controller.cs",
      "CSharp",
      "ASP.NET Core API controller with CRUD endpoints",
      priority: 4
    ));

    Register(TemplateMetadata.Create(
      "CSharpDto",
      ArtifactType.CSharpDto,
      Path.Combine(_templateBasePath, "CSharp", "Dto.sbn"),
      "{EntityName}Dtos.cs",
      "CSharp",
      "Data transfer objects for API requests/responses",
      priority: 5
    ));

    // =========================================================================
    // SQL TEMPLATES
    // =========================================================================

    Register(TemplateMetadata.Create(
      "SqlCreateTable",
      ArtifactType.SqlCreateTable,
      Path.Combine(_templateBasePath, "Sql", "CreateTable.sbn"),
      "Create{EntityName}Table.sql",
      "SQL",
      "CREATE TABLE script with indexes",
      priority: 10
    ));

    Register(TemplateMetadata.Create(
      "SqlStoredProcs",
      ArtifactType.SqlStoredProcedures,
      Path.Combine(_templateBasePath, "Sql", "StoredProcs.sbn"),
      "{EntityName}StoredProcedures.sql",
      "SQL",
      "CRUD stored procedures",
      priority: 11
    ));

    // =========================================================================
    // REACT TEMPLATES
    // =========================================================================

    Register(TemplateMetadata.Create(
      "ReactComponent",
      ArtifactType.ReactComponent,
      Path.Combine(_templateBasePath, "React", "FormComponent.sbn"),
      "{EntityName}Form.tsx",
      "React",
      "React form component with TypeScript",
      priority: 20
    ));

    Register(TemplateMetadata.Create(
      "ReactValidation",
      ArtifactType.ReactValidation,
      Path.Combine(_templateBasePath, "React", "ValidationSchema.sbn"),
      "{EntityName}Validation.ts",
      "React",
      "Yup validation schema for form",
      priority: 21
    ));

    _logger.LogInformation("Registered {Count} templates", _templates.Count);
  }

  /// <summary>
  /// Register a single template
  /// </summary>
  private void Register(TemplateMetadata metadata)
  {
    if (_templates.ContainsKey(metadata.TemplateName))
    {
      _logger.LogWarning(
        "Template already registered: {TemplateName}. Overwriting.",
        metadata.TemplateName);
    }

    _templates[metadata.TemplateName] = metadata;
    
    _logger.LogDebug(
      "Registered template: {Name} ({Category})",
      metadata.TemplateName,
      metadata.Category);
  }

  /// <summary>
  /// Get a template by name
  /// </summary>
  public TemplateMetadata GetTemplate(string templateName)
  {
    if (!_templates.TryGetValue(templateName, out var metadata))
    {
      throw new InvalidOperationException(
        $"Template not found: {templateName}. " +
        $"Available templates: {string.Join(", ", _templates.Keys)}");
    }

    return metadata;
  }

  /// <summary>
  /// Get all templates in a category
  /// </summary>
  public IEnumerable<TemplateMetadata> GetTemplatesByCategory(string category)
  {
    return _templates.Values
      .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
      .OrderBy(t => t.Priority);
  }

  /// <summary>
  /// Get all templates
  /// </summary>
  public IEnumerable<TemplateMetadata> GetAllTemplates()
  {
    return _templates.Values.OrderBy(t => t.Priority);
  }

  /// <summary>
  /// Check if a template exists
  /// </summary>
  public bool TemplateExists(string templateName)
  {
    return _templates.ContainsKey(templateName);
  }

  /// <summary>
  /// Get template names by category
  /// </summary>
  public IEnumerable<string> GetTemplateNames(string? category = null)
  {
    if (string.IsNullOrWhiteSpace(category))
      return _templates.Keys;

    return _templates.Values
      .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
      .Select(t => t.TemplateName);
  }

  /// <summary>
  /// Validate that all template files exist
  /// </summary>
  public (bool IsValid, List<string> MissingTemplates) ValidateTemplates()
  {
    var missingTemplates = new List<string>();

    foreach (var template in _templates.Values)
    {
      if (!File.Exists(template.TemplateFilePath))
      {
        missingTemplates.Add(template.TemplateFilePath);
        _logger.LogWarning(
          "Template file not found: {Path}",
          template.TemplateFilePath);
      }
    }

    var isValid = missingTemplates.Count == 0;

    if (isValid)
    {
      _logger.LogInformation("All {Count} templates validated successfully", 
        _templates.Count);
    }
    else
    {
      _logger.LogError(
        "Template validation failed. Missing {Count} template files",
        missingTemplates.Count);
    }

    return (isValid, missingTemplates);
  }

  /// <summary>
  /// Get statistics about registered templates
  /// </summary>
  public TemplateStatistics GetStatistics()
  {
    var byCategory = _templates.Values
      .GroupBy(t => t.Category)
      .ToDictionary(g => g.Key, g => g.Count());

    return new TemplateStatistics
    {
      TotalTemplates = _templates.Count,
      TemplatesByCategory = byCategory,
      Categories = byCategory.Keys.ToList()
    };
  }
}

/// <summary>
/// Statistics about registered templates
/// </summary>
public record TemplateStatistics
{
  public int TotalTemplates { get; init; }
  public Dictionary<string, int> TemplatesByCategory { get; init; } = new();
  public List<string> Categories { get; init; } = new();
}