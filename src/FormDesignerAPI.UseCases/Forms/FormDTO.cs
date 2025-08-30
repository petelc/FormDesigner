namespace FormDesignerAPI.UseCases.Forms;

public record FormDTO
(
    Guid Id,
    string FormNumber,
    string FormTitle,
    string Division,
    string Owner,
    string Version,
    DateTime CreatedDate,
    DateTime RevisedDate,
    string? ConfigurationPath
);
