namespace FormDesignerAPI.Core.FormAggregate.Events;

/// <summary>
///    Represents the event that is published when a form is deleted.
/// The DeletedFormService is used to dispatch this event.
/// </summary>
/// <param name="formId"></param>
internal sealed class FormDeletedEvent(Guid formId) : DomainEventBase
{
    public Guid FormId { get; init; } = formId;

}
