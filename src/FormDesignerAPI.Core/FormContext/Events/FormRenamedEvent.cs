using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.Events;

/// <summary>
/// Domain event raised when a form is renamed
/// </summary>
public class FormRenamedEvent : DomainEventBase
{
  public Guid FormId { get; init; }
  public string OldName { get; init; }
  public string NewName { get; init; }
  public string RenamedBy { get; init; }

  public FormRenamedEvent(
    Guid formId,
    string oldName,
    string newName,
    string renamedBy)
  {
    FormId = formId;
    OldName = oldName;
    NewName = newName;
    RenamedBy = renamedBy;
  }
}