using Ardalis.SharedKernel;
using FormDesignerAPI.UseCases.Forms;

namespace FormDesignerAPI.UseCases.Forms.Get;

/// <summary>
/// Query to retrieve a form by its ID.
/// </summary>
/// <param name="FormId">The unique identifier of the form to retrieve</param>
public record GetFormQuery(Guid FormId) : IQuery<Result<FormDTO>>;
