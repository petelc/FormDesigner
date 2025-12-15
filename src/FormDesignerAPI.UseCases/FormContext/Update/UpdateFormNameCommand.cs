namespace FormDesignerAPI.UseCases.FormContext.Update;

/// <summary>
/// Command to update/rename a form
/// </summary>
public record UpdateFormNameCommand(
    Guid FormId,
    string NewName,
    string UpdatedBy
) : IRequest<Result>;
