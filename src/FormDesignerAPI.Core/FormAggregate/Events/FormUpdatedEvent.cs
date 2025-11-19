using System;

namespace FormDesignerAPI.Core.FormAggregate.Events;

public class FormUpdatedEvent(Form form) : DomainEventBase
{
    public Guid FormId { get; init; } = form.Id;
    public string FormNumber { get; init; } = form.FormNumber;
    public string FormTitle { get; init; } = form.FormTitle;
    public string? Division { get; init; } = form.Division;
    public string? Owner { get; init; } = form.Owner?.Name;
    public List<Version> Version { get; init; } = form.GetPublishedVersion() != null
        ? new List<Version> { form.GetPublishedVersion()! }
        : new List<Version>();
    public DateTime? CreatedDate { get; init; } = form.CreatedDate;
    public DateTime? RevisedDate { get; init; } = form.RevisedDate;

}
