using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms;

public record FormDTO
(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    Core.FormAggregate.Version? Version,
    DateTime CreatedDate,
    DateTime RevisedDate
);
