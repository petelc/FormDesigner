using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Web.Forms;

public record class FormRecord(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    Core.FormAggregate.Version? Version,
    DateTime CreatedDate,
    DateTime RevisedDate
    );
