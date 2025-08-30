namespace FormDesignerAPI.Core.FormAggregate.Events;

/// <summary>
///    Represents the event that is published when a form is deleted.
/// The DeletedFormService is used to dispatch this event.
/// </summary>
/// <param name="formId"></param>
internal sealed class FormDeletedEvent(int formId) : DomainEventBase
{
    public int FormId { get; init; } = formId;

}
