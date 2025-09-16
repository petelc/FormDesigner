namespace FormDesignerAPI.Core.FormAggregate;

public record FormUpdateDto
(
    int Id,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? Version,
    //DateTime? CreatedDate,
    DateTime? RevisedDate,
    string? ConfigurationPath
);
