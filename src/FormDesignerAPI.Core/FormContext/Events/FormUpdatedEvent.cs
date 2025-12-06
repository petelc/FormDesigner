using FormDesignerAPI.Core.FormContext.Aggregates;

namespace FormDesignerAPI.Core.FormContext.Events;

public class FormUpdatedEvent(Form form) : DomainEventBase
{
    public Guid FormId { get; init; } = form.Id;
    public string FormNumber { get; init; } = form.FormNumber;
    public string FormTitle { get; init; } = form.FormTitle;
    public string? Division { get; init; } = form.Division;
    public string? Owner { get; init; } = form.Owner?.Name;
    // public string? Revision { get; init; } = form.Revision;
    public DateTime? CreatedDate { get; init; } = form.CreatedDate;
    public DateTime? RevisedDate { get; init; } = form.RevisedDate;
}
