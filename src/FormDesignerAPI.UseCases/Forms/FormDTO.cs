namespace FormDesignerAPI.UseCases.Forms;

public record FormDTO
(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? Revision,
    DateTime CreatedDate,
    DateTime RevisedDate
);
