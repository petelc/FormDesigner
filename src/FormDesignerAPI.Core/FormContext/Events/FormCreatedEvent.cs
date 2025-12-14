using Traxs.SharedKernel;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a new form is created
/// </summary>
public class FormCreatedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public string Name { get; init; }
  public OriginMetadata Origin { get; init; }
  public string CreatedBy { get; init; }

  public FormCreatedEvent(
    Guid formId,
    string name,
    OriginMetadata origin,
    string createdBy)
  {
    FormId = formId;
    Name = name;
    Origin = origin;
    CreatedBy = createdBy;
  }
}