namespace FormDesignerAPI.Core.FormAggregate;

public record FormUpdateDto
(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? OwnerEmail,
    Revision Revision,
    DateTime? RevisedDate
);
