namespace FormDesignerAPI.UseCases.Forms.Update;

/// <summary>
/// Command to update an existing form
/// </summary>
public record UpdateFormCommand(
    int FormId,
    string FormNumber,
    string FormTitle,
    string? Division,
    string? Owner,
    string? Version,
    DateTime RevisedDate,
    string? ConfigurationPath
) : IRequest<Result>;
