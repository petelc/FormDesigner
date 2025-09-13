using System;

namespace FormDesignerAPI.Core.FormAggregate.Events;

public class FormUpdatedEvent(Form form) : DomainEventBase
{
    public int FormId { get; init; } = form.Id;
    public string FormNumber { get; init; } = form.FormNumber;
    public string FormTitle { get; init; } = form.FormTitle;
    public string? Division { get; init; } = form.Division;
    public string? Owner { get; init; } = form.Owner?.Name;
    public string? Version { get; init; } = form.Version;
    public DateTime CreatedDate { get; init; } = form.CreatedDate;
    public DateTime RevisedDate { get; init; } = form.RevisedDate;
    public string? ConfigurationPath { get; init; } = form.ConfigurationPath;
}
