using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.CodeGenerationContext.Events;

// <summary>
/// Event raised when a code generation job starts processing
/// </summary>
public class CodeGenerationJobProcessingEvent : DomainEventBase
{
	public Guid JobId { get; }

	public CodeGenerationJobProcessingEvent(Guid jobId)
	{
		JobId = jobId;
	}
}

