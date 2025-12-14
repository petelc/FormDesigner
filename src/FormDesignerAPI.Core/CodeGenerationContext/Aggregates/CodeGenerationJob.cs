using Ardalis.GuardClauses;
using Traxs.SharedKernel;
using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using FormDesignerAPI.Core.CodeGenerationContext.Events;

namespace FormDesignerAPI.Core.CodeGenerationContext.Aggregates;

/// <summary>
/// CodeGenerationJob aggregate root - manages code generation process
/// </summary>
public class CodeGenerationJob : EntityBase<Guid>, IAggregateRoot
{
  public Guid FormDefinitionId { get; private set; }
  public Guid FormRevisionId { get; private set; }
  public GenerationVersion Version { get; private set; } = null!;
  public JobStatus Status { get; private set; }

  private readonly List<GeneratedArtifact> _artifacts = new();
  public IReadOnlyCollection<GeneratedArtifact> Artifacts => _artifacts.AsReadOnly();

  public GenerationOptions Options { get; private set; } = null!;
  public string? OutputFolderPath { get; private set; }
  public string? ZipFilePath { get; private set; }
  public long? ZipFileSizeBytes { get; private set; }

  public DateTime RequestedAt { get; private set; }
  public string RequestedBy { get; private set; } = string.Empty;
  public DateTime? CompletedAt { get; private set; }
  public TimeSpan? ProcessingDuration { get; private set; }
  public string? ErrorMessage { get; private set; }

  // Private constructor for EF Core
  private CodeGenerationJob() { }

  /// <summary>
  /// Factory method to create a new generation job
  /// </summary>
  public static CodeGenerationJob Create(
    Guid formDefinitionId,
    Guid formRevisionId,
    GenerationVersion version,
    GenerationOptions options,
    string requestedBy)
  {
    Guard.Against.Default(formDefinitionId, nameof(formDefinitionId));
    Guard.Against.Default(formRevisionId, nameof(formRevisionId));
    Guard.Against.Null(version, nameof(version));
    Guard.Against.Null(options, nameof(options));
    Guard.Against.NullOrEmpty(requestedBy, nameof(requestedBy));

    var job = new CodeGenerationJob
    {
      Id = Guid.NewGuid(),
      FormDefinitionId = formDefinitionId,
      FormRevisionId = formRevisionId,
      Version = version,
      Status = JobStatus.Pending,
      Options = options,
      RequestedAt = DateTime.UtcNow,
      RequestedBy = requestedBy
    };

    job.RegisterDomainEvent(new CodeGenerationJobCreatedEvent(job.Id));

    return job;
  }

  /// <summary>
  /// Add a generated artifact to this job
  /// </summary>
  public void AddArtifact(GeneratedArtifact artifact)
  {
    Guard.Against.Null(artifact, nameof(artifact));

    if (Status != JobStatus.Processing)
      throw new InvalidOperationException("Can only add artifacts to processing jobs");

    _artifacts.Add(artifact);
  }

  /// <summary>
  /// Mark job as processing
  /// </summary>
  public void MarkAsProcessing()
  {
    if (Status != JobStatus.Pending)
      throw new InvalidOperationException("Can only start processing pending jobs");

    Status = JobStatus.Processing;
    RegisterDomainEvent(new CodeGenerationJobProcessingEvent(Id));
  }

  /// <summary>
  /// Set output paths after generation
  /// </summary>
  public void SetOutputPaths(string outputFolder, string zipFile, long zipSize)
  {
    Guard.Against.NullOrEmpty(outputFolder, nameof(outputFolder));
    Guard.Against.NullOrEmpty(zipFile, nameof(zipFile));
    Guard.Against.NegativeOrZero(zipSize, nameof(zipSize));

    OutputFolderPath = outputFolder;
    ZipFilePath = zipFile;
    ZipFileSizeBytes = zipSize;
  }

  /// <summary>
  /// Complete the job successfully
  /// </summary>
  public void Complete()
  {
    if (Status != JobStatus.Processing)
      throw new InvalidOperationException("Can only complete processing jobs");

    Status = JobStatus.Completed;
    CompletedAt = DateTime.UtcNow;
    ProcessingDuration = CompletedAt.Value - RequestedAt;

    RegisterDomainEvent(new CodeArtifactsGeneratedEvent(
      Id,
      Version.ToString(),
      ZipFilePath));
  }

  /// <summary>
  /// Mark job as failed
  /// </summary>
  public void Fail(Exception ex)
  {
    Guard.Against.Null(ex, nameof(ex));

    Status = JobStatus.Failed;
    CompletedAt = DateTime.UtcNow;
    ProcessingDuration = CompletedAt.Value - RequestedAt;
    ErrorMessage = ex.Message;

    RegisterDomainEvent(new CodeGenerationFailedEvent(Id, ex.Message));
  }

  /// <summary>
  /// Get total size of all generated artifacts
  /// </summary>
  public long GetTotalArtifactSize()
  {
    return _artifacts.Sum(a => a.SizeBytes);
  }

  /// <summary>
  /// Get artifacts by type
  /// </summary>
  public IEnumerable<GeneratedArtifact> GetArtifactsByType(ArtifactType type)
  {
    return _artifacts.Where(a => a.Type == type);
  }
}

/// <summary>
/// Status of a code generation job
/// </summary>
public enum JobStatus
{
  Pending,
  Processing,
  Completed,
  Failed
}