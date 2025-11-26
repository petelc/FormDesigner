namespace FormDesignerAPI.Web.Forms;

public record class FormRecord(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? Revision,
    DateTime CreatedDate,
    DateTime RevisedDate
    );
