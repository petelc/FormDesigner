namespace FormDesignerAPI.UseCases.Forms.Delete;

/// <summary>
/// Command to delete a form
/// </summary>
public record DeleteFormCommand(int FormId) : IRequest<Result>;
