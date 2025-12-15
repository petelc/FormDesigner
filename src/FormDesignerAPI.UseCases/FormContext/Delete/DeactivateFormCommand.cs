namespace FormDesignerAPI.UseCases.FormContext.Delete;

/// <summary>
/// Command to deactivate a form (soft delete)
/// </summary>
public record DeactivateFormCommand(Guid FormId) : IRequest<Result>;
