using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.Web.Forms;

public record class FormRecord(
    int Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? Version,
    FormStatus Status,
    DateTime CreatedDate,
    DateTime RevisedDate,
    string? ConfigurationPath
    );
