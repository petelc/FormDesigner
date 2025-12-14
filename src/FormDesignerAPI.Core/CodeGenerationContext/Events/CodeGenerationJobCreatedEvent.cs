using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.CodeGenerationContext.Events;

/// <summary>
/// Event raised when a new code generation job is created
/// </summary>
public class CodeGenerationJobCreatedEvent : DomainEventBase
{
	public Guid JobId { get; init; }
	
	public CodeGenerationJobCreatedEvent(Guid jobId)
	{
		JobId = jobId;
	}
}