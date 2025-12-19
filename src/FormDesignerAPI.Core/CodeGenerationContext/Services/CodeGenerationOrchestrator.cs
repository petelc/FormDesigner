// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/Services/CodeGenerationOrchestrator.cs

using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Core.CodeGenerationContext.Services;

/// <summary>
/// Orchestrates the entire code generation process using Scriban templates
/// </summary>
public class CodeGenerationOrchestrator
{
  private readonly ScribanTemplateEngine _templateEngine;
  private readonly TemplateRepository _templateRepository;
  private readonly CodeArtifactOrganizer _organizer;
  private readonly ZipPackager _zipPackager;
  private readonly ILogger<CodeGenerationOrchestrator> _logger;

  public CodeGenerationOrchestrator(
    ScribanTemplateEngine templateEngine,
    TemplateRepository templateRepository,
    CodeArtifactOrganizer organizer,
    ZipPackager zipPackager,
    ILogger<CodeGenerationOrchestrator> logger)
  {
    _templateEngine = templateEngine;
    _templateRepository = templateRepository;
    _organizer = organizer;
    _zipPackager = zipPackager;
    _logger = logger;
  }

  /// <summary>
  /// Generate all code artifacts for a form
  /// </summary>
  public async Task<CodeGenerationJob> GenerateAsync(
    Guid formId,
    Guid revisionId,
    FormDefinition formDefinition,
    GenerationOptions options,
    string requestedBy,
    CancellationToken cancellationToken = default)
  {
    // Create job
    var job = CodeGenerationJob.Create(
      formId,
      revisionId,
      GenerationVersion.Parse("1.0.0"),
      options,
      requestedBy
    );

    try
    {
      job.MarkAsProcessing();
      
      _logger.LogInformation(
        "Starting code generation job: {JobId} for form: {FormId}",
        job.Id,
        formId);

      // Extract entity name from form definition or options
      var entityName = ExtractEntityName(formDefinition, options);
      
      _logger.LogDebug("Entity name: {EntityName}", entityName);

      // Create template model (data passed to all templates)
      var model = CreateTemplateModel(formDefinition, entityName, options);

      // Generate artifacts
      var artifacts = new List<GeneratedArtifact>();

      // Generate C# artifacts
      if (options.IncludeCSharpModels)
      {
        _logger.LogDebug("Generating C# artifacts...");
        artifacts.AddRange(await GenerateCSharpArtifacts(model, entityName, cancellationToken));
      }

      // Generate SQL artifacts
      if (options.IncludeSqlSchema)
      {
        _logger.LogDebug("Generating SQL artifacts...");
        artifacts.AddRange(await GenerateSqlArtifacts(model, entityName, cancellationToken));
      }

      // Generate React artifacts
      if (options.IncludeReactComponents)
      {
        _logger.LogDebug("Generating React artifacts...");
        artifacts.AddRange(await GenerateReactArtifacts(model, entityName, cancellationToken));
      }

      // Add all artifacts to job
      foreach (var artifact in artifacts)
      {
        job.AddArtifact(artifact);
      }

      _logger.LogInformation(
        "Generated {Count} artifacts totaling {Size} bytes",
        artifacts.Count,
        artifacts.Sum(a => a.SizeBytes));

      // Organize into folder structure
      var organized = await _organizer.OrganizeArtifacts(
        artifacts,
        options.ProjectName);

      _logger.LogDebug("Artifacts organized at: {Path}", organized.RootPath);

      // Create ZIP file
      var zipPath = await _zipPackager.CreateZipFile(
        organized.RootPath,
        Path.GetDirectoryName(organized.RootPath)!);

      var zipSize = new FileInfo(zipPath).Length;
      
      _logger.LogInformation(
        "Created ZIP file: {Path} ({Size} bytes)",
        zipPath,
        zipSize);

      // Set output paths on job
      job.SetOutputPaths(organized.RootPath, zipPath, zipSize);

      // Complete job
      job.Complete();

      _logger.LogInformation(
        "Code generation completed successfully: {JobId}, Duration: {Duration}ms",
        job.Id,
        job.ProcessingDuration?.TotalMilliseconds);

      return job;
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Code generation failed: {JobId}",
        job.Id);
      
      job.Fail(ex);
      throw;
    }
  }

  // ==========================================================================
  // PRIVATE HELPER METHODS
  // ==========================================================================

  /// <summary>
  /// Generate C# artifacts (Entity, Repository, Controller, etc.)
  /// </summary>
  private async Task<List<GeneratedArtifact>> GenerateCSharpArtifacts(
    object model,
    string entityName,
    CancellationToken cancellationToken)
  {
    var artifacts = new List<GeneratedArtifact>();

    // Templates to generate (in order)
    var csharpTemplates = new[]
    {
      "CSharpEntity",
      "CSharpInterface",
      "CSharpRepository",
      "CSharpController",
      "CSharpDto"
    };

    foreach (var templateName in csharpTemplates)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var artifact = await GenerateFromTemplate(
        templateName,
        model,
        entityName,
        cancellationToken);

      artifacts.Add(artifact);
    }

    return artifacts;
  }

  /// <summary>
  /// Generate SQL artifacts (CREATE TABLE, Stored Procedures)
  /// </summary>
  private async Task<List<GeneratedArtifact>> GenerateSqlArtifacts(
    object model,
    string entityName,
    CancellationToken cancellationToken)
  {
    var artifacts = new List<GeneratedArtifact>();

    var sqlTemplates = new[]
    {
      "SqlCreateTable",
      "SqlStoredProcs"
    };

    foreach (var templateName in sqlTemplates)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var artifact = await GenerateFromTemplate(
        templateName,
        model,
        entityName,
        cancellationToken);

      artifacts.Add(artifact);
    }

    return artifacts;
  }

  /// <summary>
  /// Generate React artifacts (Components, Validation)
  /// </summary>
  private async Task<List<GeneratedArtifact>> GenerateReactArtifacts(
    object model,
    string entityName,
    CancellationToken cancellationToken)
  {
    var artifacts = new List<GeneratedArtifact>();

    var reactTemplates = new[]
    {
      "ReactComponent",
      "ReactValidation"
    };

    foreach (var templateName in reactTemplates)
    {
      cancellationToken.ThrowIfCancellationRequested();

      // Check if template exists (ReactValidation is optional)
      if (!_templateRepository.TemplateExists(templateName))
      {
        _logger.LogDebug("Template not found, skipping: {Template}", templateName);
        continue;
      }

      var artifact = await GenerateFromTemplate(
        templateName,
        model,
        entityName,
        cancellationToken);

      artifacts.Add(artifact);
    }

    return artifacts;
  }

  /// <summary>
  /// Generate a single artifact from a template
  /// </summary>
  private async Task<GeneratedArtifact> GenerateFromTemplate(
    string templateName,
    object model,
    string entityName,
    CancellationToken cancellationToken)
  {
    _logger.LogDebug("Generating artifact from template: {Template}", templateName);

    // Get template metadata
    var metadata = _templateRepository.GetTemplate(templateName);

    // Render template
    var content = await _templateEngine.RenderFromFileAsync(
      metadata.TemplateFilePath,
      model);

    // Generate filename
    var fileName = metadata.GenerateFileName(entityName);

    // Create artifact
    var artifact = new GeneratedArtifact(
      metadata.ArtifactType,
      fileName,
      content);

    _logger.LogDebug(
      "Generated artifact: {FileName} ({Size} bytes)",
      fileName,
      artifact.SizeBytes);

    return artifact;
  }

  /// <summary>
  /// Extract entity name from form definition or use project name
  /// </summary>
  private string ExtractEntityName(FormDefinition definition, GenerationOptions options)
  {
    // Try to extract from form definition (you might have a "name" field)
    // For now, use project name and sanitize it
    var entityName = options.ProjectName.Replace(" ", "");
    
    // Remove special characters
    entityName = new string(entityName.Where(c => char.IsLetterOrDigit(c)).ToArray());
    
    // Ensure it starts with a letter
    if (!char.IsLetter(entityName[0]))
    {
      entityName = "Entity" + entityName;
    }

    return entityName;
  }

  /// <summary>
  /// Create the model object passed to all templates
  /// </summary>
  private object CreateTemplateModel(
    FormDefinition definition,
    string entityName,
    GenerationOptions options)
  {
    // Deduplicate field names by appending numeric suffixes
    var deduplicatedFields = DeduplicateFieldNames(definition.Fields);

    _logger.LogInformation(
      "Creating template model with {FieldCount} fields for entity {EntityName}",
      deduplicatedFields.Count,
      entityName);

    var mappedFields = deduplicatedFields.Select(f => new Dictionary<string, object?>
    {
      // Original field info
      ["name"] = f.UniqueName,
      ["type"] = f.Field.Type,
      ["required"] = f.Field.Required,
      ["label"] = f.Field.Label ?? ToPascalCase(f.UniqueName),
      ["placeholder"] = f.Field.Placeholder,
      ["default_value"] = f.Field.DefaultValue,
      ["min_length"] = f.Field.MinLength,
      ["max_length"] = f.Field.MaxLength,
      ["pattern"] = f.Field.Pattern,
      ["options"] = f.Field.Options,
      
      // Type mappings for different languages
      ["csharp_type"] = MapToCSharpType(f.Field.Type, f.Field.Required),
      ["sql_type"] = MapToSqlType(f.Field.Type, f.Field.MaxLength),
      ["typescript_type"] = MapToTypeScriptType(f.Field.Type),
      
      // Naming variations
      ["name_pascal"] = ToPascalCase(f.UniqueName),
      ["name_camel"] = ToCamelCase(f.UniqueName),
      ["name_snake"] = ToSnakeCase(f.UniqueName),
      
      // Track if this was renamed
      ["was_renamed"] = f.WasRenamed,
      ["original_name"] = f.OriginalName
    }).ToList();

    // Log first few fields for debugging
    foreach (var field in mappedFields.Take(3))
    {
      _logger.LogInformation(
        "Sample field: Name={Name}, Type={Type}, CSharpType={CSharpType}, NamePascal={NamePascal}, Label={Label}",
        field["name"],
        field["type"],
        field["csharp_type"],
        field["name_pascal"],
        field["label"]);
    }

    return new Dictionary<string, object?>
    {
      // Entity information
      ["entity_name"] = entityName,
      ["entity_name_plural"] = Pluralize(entityName),
      ["entity_name_camel"] = ToCamelCase(entityName),
      
      // Project configuration
      ["namespace"] = options.Namespace,
      ["project_name"] = options.ProjectName,
      ["author"] = options.Author,
      
      // Form fields mapped to template-friendly format
      ["fields"] = mappedFields,
      
      // Generation metadata
      ["generated_date"] = DateTime.UtcNow,
      ["generated_date_formatted"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
      ["version"] = "1.0.0",
      
      // Database configuration
      ["database_type"] = options.DatabaseType,
      
      // Test configuration
      ["test_framework"] = options.TestFramework,
      ["use_fluent_assertions"] = options.UseFluentAssertions,
      
      // Additional settings
      ["custom_settings"] = options.CustomSettings
    };
  }

  /// <summary>
  /// Deduplicate field names by appending numeric suffixes for duplicates
  /// </summary>
  private List<(FormField Field, string UniqueName, string OriginalName, bool WasRenamed)> DeduplicateFieldNames(
    IReadOnlyList<FormField> fields)
  {
    var result = new List<(FormField Field, string UniqueName, string OriginalName, bool WasRenamed)>();
    var nameCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var field in fields)
    {
      var originalName = field.Name;
      var baseName = originalName;
      
      // Track how many times we've seen this name
      if (!nameCount.ContainsKey(baseName))
      {
        nameCount[baseName] = 0;
      }
      nameCount[baseName]++;

      string uniqueName;
      bool wasRenamed = false;

      // If this is the first occurrence, use the original name
      if (nameCount[baseName] == 1)
      {
        uniqueName = baseName;
      }
      else
      {
        // For duplicates, append a number
        uniqueName = $"{baseName}{nameCount[baseName]}";
        wasRenamed = true;
        
        _logger.LogWarning(
          "Duplicate field name detected: '{OriginalName}' renamed to '{UniqueName}'",
          originalName,
          uniqueName);
      }

      // Ensure the unique name is actually unique (in case of name conflicts with appended numbers)
      var finalUniqueName = uniqueName;
      var counter = 1;
      while (usedNames.Contains(finalUniqueName))
      {
        counter++;
        finalUniqueName = $"{baseName}{nameCount[baseName]}{counter}";
        wasRenamed = true;
      }

      usedNames.Add(finalUniqueName);
      result.Add((field, finalUniqueName, originalName, wasRenamed));
    }

    // Log summary of duplicates
    var duplicateCount = result.Count(r => r.WasRenamed);
    if (duplicateCount > 0)
    {
      _logger.LogWarning(
        "Found {Count} duplicate field names that were renamed with numeric suffixes",
        duplicateCount);
    }

    return result;
  }

  // ==========================================================================
  // TYPE MAPPING FUNCTIONS
  // ==========================================================================

  /// <summary>
  /// Map form field type to C# type
  /// </summary>
  private string MapToCSharpType(string fieldType, bool required)
  {
    var csharpType = fieldType.ToLower() switch
    {
      "text" or "email" or "url" or "tel" => "string",
      "textarea" => "string",
      "number" => "int",
      "decimal" or "currency" => "decimal",
      "date" or "datetime" => "DateTime",
      "time" => "TimeSpan",
      "boolean" or "checkbox" => "bool",
      "select" or "radio" => "string",
      "file" => "string", // Store file path
      _ => "string"
    };

    // Add nullable suffix if not required (except for string which is already nullable)
    if (!required && csharpType != "string")
    {
      csharpType += "?";
    }

    return csharpType;
  }

  /// <summary>
  /// Map form field type to SQL type
  /// </summary>
  private string MapToSqlType(string fieldType, int? maxLength)
  {
    return fieldType.ToLower() switch
    {
      "text" or "email" or "url" or "tel" => 
        maxLength.HasValue ? $"NVARCHAR({maxLength})" : "NVARCHAR(255)",
      "textarea" => "NVARCHAR(MAX)",
      "number" => "INT",
      "decimal" or "currency" => "DECIMAL(18,2)",
      "date" => "DATE",
      "datetime" => "DATETIME",
      "time" => "TIME",
      "boolean" or "checkbox" => "BIT",
      "select" or "radio" => "NVARCHAR(100)",
      "file" => "NVARCHAR(500)",
      _ => "NVARCHAR(255)"
    };
  }

  /// <summary>
  /// Map form field type to TypeScript type
  /// </summary>
  private string MapToTypeScriptType(string fieldType)
  {
    return fieldType.ToLower() switch
    {
      "text" or "email" or "url" or "tel" or "textarea" => "string",
      "number" or "decimal" or "currency" => "number",
      "date" or "datetime" => "Date",
      "time" => "string",
      "boolean" or "checkbox" => "boolean",
      "select" or "radio" => "string",
      "file" => "File",
      _ => "string"
    };
  }

  // ==========================================================================
  // STRING MANIPULATION HELPERS
  // ==========================================================================

  private string ToPascalCase(string input)
  {
    if (string.IsNullOrWhiteSpace(input)) return input;
    var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    return string.Join("", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
  }

  private string ToCamelCase(string input)
  {
    var pascal = ToPascalCase(input);
    return char.ToLower(pascal[0]) + pascal.Substring(1);
  }

  private string ToSnakeCase(string input)
  {
    if (string.IsNullOrWhiteSpace(input)) return input;
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

  private string Pluralize(string input)
  {
    if (string.IsNullOrWhiteSpace(input)) return input;
    if (input.EndsWith("y")) return input.Substring(0, input.Length - 1) + "ies";
    if (input.EndsWith("s")) return input + "es";
    return input + "s";
  }
}

