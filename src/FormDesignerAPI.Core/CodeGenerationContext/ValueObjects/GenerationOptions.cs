namespace FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

/// <summary>
/// Configuration options for code generation
/// </summary>
public record GenerationOptions
{
  // What to generate
  public bool IncludeCSharpModels { get; init; } = true;
  public bool IncludeSqlSchema { get; init; } = true;
  public bool IncludeReactComponents { get; init; } = true;
  public bool IncludeTests { get; init; } = false;
  public bool GenerateIntegrationTests { get; init; } = false;
  
  // Project configuration
  public string Namespace { get; init; } = "GeneratedApp";
  public string ProjectName { get; init; } = "GeneratedProject";
  public string Author { get; init; } = "CodeGenerator";
  
  // Test framework options
  public string TestFramework { get; init; } = "xUnit";
  public bool UseFluentAssertions { get; init; } = true;
  
  // Database options
  public string DatabaseType { get; init; } = "SqlServer"; // PostgreSQL, SqlServer, MySQL
  
  // Additional customization
  public Dictionary<string, string> AdditionalImports { get; init; } = new();
  public Dictionary<string, object> CustomSettings { get; init; } = new();

  /// <summary>
  /// Create default options with project name
  /// </summary>
  public static GenerationOptions Default(string projectName, string author)
  {
    return new GenerationOptions
    {
      ProjectName = projectName,
      Namespace = projectName.Replace(" ", ""),
      Author = author
    };
  }

  /// <summary>
  /// Create minimal options (C# only, no tests)
  /// </summary>
  public static GenerationOptions Minimal(string projectName, string author)
  {
    return new GenerationOptions
    {
      ProjectName = projectName,
      Namespace = projectName.Replace(" ", ""),
      Author = author,
      IncludeCSharpModels = true,
      IncludeSqlSchema = false,
      IncludeReactComponents = false,
      IncludeTests = false
    };
  }

  /// <summary>
  /// Create full-stack options (everything enabled)
  /// </summary>
  public static GenerationOptions FullStack(string projectName, string author)
  {
    return new GenerationOptions
    {
      ProjectName = projectName,
      Namespace = projectName.Replace(" ", ""),
      Author = author,
      IncludeCSharpModels = true,
      IncludeSqlSchema = true,
      IncludeReactComponents = true,
      IncludeTests = true,
      GenerateIntegrationTests = true
    };
  }
}
