namespace FormDesignerAPI.UseCases.FormContext.Get;

/// <summary>
/// Query to get a form by ID
/// </summary>
public record GetFormQuery(Guid FormId) : IRequest<Result<FormContextDTO>>;
