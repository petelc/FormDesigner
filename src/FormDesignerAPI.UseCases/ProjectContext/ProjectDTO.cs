using FormDesignerAPI.Core.ProjectContext.ValueObjects;

namespace FormDesignerAPI.UseCases.ProjectContext;

/// <summary>
/// DTO for Project aggregate
/// </summary>
public record ProjectDTO
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public ProjectFilterDTO Filter { get; init; } = null!;
  public List<Guid> FormIds { get; init; } = new();
  public List<Guid> CodeGenerationJobIds { get; init; } = new();
  public string CreatedBy { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
  public DateTime UpdatedAt { get; init; }
  public string? UpdatedBy { get; init; }
}

/// <summary>
/// DTO for ProjectFilter
/// </summary>
public record ProjectFilterDTO
{
  public string? FormType { get; init; }
  public List<string> Tags { get; init; } = new();
  public bool ActiveOnly { get; init; } = true;
  public DateTime? FromDate { get; init; }
  public DateTime? ToDate { get; init; }
  public Dictionary<string, object> CustomFilters { get; init; } = new();
}

/// <summary>
/// Detailed project DTO with related entities
/// </summary>
public record ProjectDetailDTO : ProjectDTO
{
  public List<FormSummaryDTO> Forms { get; init; } = new();
  public List<CodeGenerationJobSummaryDTO> CodeGenerationJobs { get; init; } = new();
  public int TotalForms => Forms.Count;
  public int TotalJobs => CodeGenerationJobs.Count;
  public int CompletedJobs => CodeGenerationJobs.Count(j => j.Status == "Completed");
}

/// <summary>
/// Summary DTO for Form (used in Project details)
/// </summary>
public record FormSummaryDTO
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public int FieldCount { get; init; }
  public bool IsActive { get; init; }
  public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Summary DTO for CodeGenerationJob (used in Project details)
/// </summary>
public record CodeGenerationJobSummaryDTO
{
  public Guid Id { get; init; }
  public string Status { get; init; } = string.Empty;
  public DateTime RequestedAt { get; init; }
  public DateTime? CompletedAt { get; init; }
  public string? ZipFilePath { get; init; }
  public long? ZipFileSizeBytes { get; init; }
}
