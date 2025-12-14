using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form revision is created
/// </summary>
public class FormRevisionCreatedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public Guid RevisionId { get; init; }
  public int Version { get; init; }
  public string CreatedBy { get; init; }

  public FormRevisionCreatedEvent(
    Guid formId,
    Guid revisionId,
    int version,
    string createdBy)
  {
    FormId = formId;
    RevisionId = revisionId;
    Version = version;
    CreatedBy = createdBy;
  }
}