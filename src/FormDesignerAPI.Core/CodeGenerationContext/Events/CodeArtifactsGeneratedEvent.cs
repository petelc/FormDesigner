using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.CodeGenerationContext.Events;

/// <summary>
/// Event raised when code artifacts are successfully generated
/// </summary>
public class CodeArtifactsGeneratedEvent : DomainEventBase
{
  public Guid JobId { get; init; }
  public string Version { get; init; }
  public string? ZipFilePath { get; init; }

  public CodeArtifactsGeneratedEvent(Guid jobId, string version, string? zipFilePath)
  {
    JobId = jobId;
    Version = version;
    ZipFilePath = zipFilePath;
  }
}