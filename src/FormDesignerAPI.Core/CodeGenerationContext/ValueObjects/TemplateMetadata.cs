// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/ValueObjects/TemplateMetadata.cs

using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

/// <summary>
/// Metadata about a Scriban template
/// </summary>
public class TemplateMetadata : ValueObject
{
  public string TemplateName { get; init; } = string.Empty;
  public ArtifactType ArtifactType { get; init; }
  public string TemplateFilePath { get; init; } = string.Empty;
  public string OutputFilePattern { get; init; } = string.Empty; // e.g., "{EntityName}Repository.cs"
  public string Category { get; init; } = string.Empty; // CSharp, SQL, React
  public string Description { get; init; } = string.Empty;
  public int Priority { get; init; } = 0; // Generation order (lower = first)
  
  private TemplateMetadata() { }

  /// <summary>
  /// Create template metadata
  /// </summary>
  public static TemplateMetadata Create(
    string name,
    ArtifactType type,
    string filePath,
    string outputPattern,
    string category,
    string description = "",
    int priority = 0)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Template name is required", nameof(name));
    
    if (string.IsNullOrWhiteSpace(filePath))
      throw new ArgumentException("Template file path is required", nameof(filePath));

    if (string.IsNullOrWhiteSpace(outputPattern))
      throw new ArgumentException("Output file pattern is required", nameof(outputPattern));

    return new TemplateMetadata
    {
      TemplateName = name,
      ArtifactType = type,
      TemplateFilePath = filePath,
      OutputFilePattern = outputPattern,
      Category = category,
      Description = description,
      Priority = priority
    };
  }

  /// <summary>
  /// Generate output filename from pattern
  /// </summary>
  public string GenerateFileName(string entityName)
  {
    return OutputFilePattern.Replace("{EntityName}", entityName);
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return TemplateName;
    yield return ArtifactType;
    yield return TemplateFilePath;
  }
}