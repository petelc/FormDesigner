namespace FormDesignerAPI.UseCases.Forms;

public record FormDTO
(
    int Id,
    string FormNumber,
    string FormTitle,
    string Division,
    string Owner,
    string Version,
    DateTime CreatedDate,
    DateTime RevisedDate,
    string? ConfigurationPath
);
