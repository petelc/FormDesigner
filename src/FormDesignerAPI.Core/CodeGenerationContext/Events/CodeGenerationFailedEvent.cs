using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.CodeGenerationContext.Events;

/// <summary>
/// Event raised when code generation fails
/// </summary>
public class CodeGenerationFailedEvent : DomainEventBase
{
  public Guid JobId { get; init; }
  public string ErrorMessage { get; init; }

  public CodeGenerationFailedEvent(Guid jobId, string errorMessage)
  {
    JobId = jobId;
    ErrorMessage = errorMessage;
  }
}