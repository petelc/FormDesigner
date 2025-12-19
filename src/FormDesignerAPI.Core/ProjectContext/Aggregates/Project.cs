using Ardalis.GuardClauses;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;

namespace FormDesignerAPI.Core.ProjectContext.Aggregates;

/// <summary>
/// Project aggregate root - manages project details, forms, and code generation
/// </summary>
public class Project : EntityBase<Guid>, IAggregateRoot
{
  // Basic Properties
  public string Name { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public ProjectStatus Status { get; private set; }
  public ProjectFilter Filter { get; private set; } = null!;

  // Related entities
  private readonly List<Guid> _formIds = new();
  public IReadOnlyCollection<Guid> FormIds => _formIds.AsReadOnly();

  private readonly List<Guid> _codeGenerationJobIds = new();
  public IReadOnlyCollection<Guid> CodeGenerationJobIds => _codeGenerationJobIds.AsReadOnly();

  // Metadata
  public string CreatedBy { get; private set; } = string.Empty;
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public string? UpdatedBy { get; private set; }

  // Private constructor for EF Core
  private Project() { }

  /// <summary>
  /// Factory method to create a new project
  /// </summary>
  public static Project Create(
    string name,
    string description,
    ProjectFilter filter,
    string createdBy)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
    Guard.Against.Null(filter, nameof(filter));

    var project = new Project
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = description ?? string.Empty,
      Filter = filter,
      Status = ProjectStatus.DRAFT,
      CreatedBy = createdBy,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    return project;
  }

  /// <summary>
  /// Update project details
  /// </summary>
  public void Update(
    string name,
    string description,
    ProjectFilter filter,
    string updatedBy)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.NullOrWhiteSpace(updatedBy, nameof(updatedBy));
    Guard.Against.Null(filter, nameof(filter));

    Name = name;
    Description = description ?? string.Empty;
    Filter = filter;
    UpdatedBy = updatedBy;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Add a form to the project
  /// </summary>
  public void AddForm(Guid formId)
  {
    Guard.Against.Default(formId, nameof(formId));

    if (!_formIds.Contains(formId))
    {
      _formIds.Add(formId);
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Remove a form from the project
  /// </summary>
  public void RemoveForm(Guid formId)
  {
    if (_formIds.Remove(formId))
    {
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Add a code generation job to the project
  /// </summary>
  public void AddCodeGenerationJob(Guid jobId)
  {
    Guard.Against.Default(jobId, nameof(jobId));

    if (!_codeGenerationJobIds.Contains(jobId))
    {
      _codeGenerationJobIds.Add(jobId);
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Update project status
  /// </summary>
  public void UpdateStatus(ProjectStatus status, string updatedBy)
  {
    Guard.Against.NullOrWhiteSpace(updatedBy, nameof(updatedBy));

    Status = status;
    UpdatedBy = updatedBy;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Mark project as PDF uploaded
  /// </summary>
  public void MarkPdfUploaded(string updatedBy)
  {
    UpdateStatus(ProjectStatus.PDF_UPLOADED, updatedBy);
  }

  /// <summary>
  /// Mark project as analyzing
  /// </summary>
  public void MarkAnalyzing(string updatedBy)
  {
    UpdateStatus(ProjectStatus.ANALYZING, updatedBy);
  }

  /// <summary>
  /// Mark project analysis as complete
  /// </summary>
  public void MarkAnalysisComplete(string updatedBy)
  {
    UpdateStatus(ProjectStatus.ANALYSING_COMPLETE, updatedBy);
  }

  /// <summary>
  /// Mark project structure as reviewed
  /// </summary>
  public void MarkStructureReviewed(string updatedBy)
  {
    UpdateStatus(ProjectStatus.STRUCTURE_REVIEWED, updatedBy);
  }

  /// <summary>
  /// Mark project as generating code
  /// </summary>
  public void MarkGeneratingCode(string updatedBy)
  {
    UpdateStatus(ProjectStatus.GENERATING_CODE, updatedBy);
  }

  /// <summary>
  /// Mark code generation as complete
  /// </summary>
  public void MarkCodeGenerated(string updatedBy)
  {
    UpdateStatus(ProjectStatus.CODE_GENERATED, updatedBy);
  }

  /// <summary>
  /// Mark project as completed
  /// </summary>
  public void MarkCompleted(string updatedBy)
  {
    UpdateStatus(ProjectStatus.COMPLETED, updatedBy);
  }

  /// <summary>
  /// Mark project as failed
  /// </summary>
  public void MarkFailed(string updatedBy)
  {
    UpdateStatus(ProjectStatus.FAILED, updatedBy);
  }
}
