using Ardalis.Result;

namespace FormDesignerAPI.UseCases.Forms.Get;

/// <summary>
/// Query to get a form by ID
/// </summary>
public record GetFormQuery(int FormId) : IRequest<Result<FormDTO>>;
