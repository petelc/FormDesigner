using Ardalis.SharedKernel;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.FormAggregate.Specifications;

namespace FormDesignerAPI.UseCases.Forms.Get;

/// <summary>
/// Handler for retrieving a form by its ID.
/// Maps the Form aggregate to a FormDTO for the query response.
/// </summary>
public class GetFormHandler
(IReadRepository<Form> _repository)
  : IQueryHandler<GetFormQuery, Result<FormDTO>>
{
    public async Task<Result<FormDTO>> Handle(GetFormQuery request, CancellationToken cancellationToken)
    {
        var spec = new FormByIdSpec(request.FormId);
        var form = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (form == null)
        {
            return Result<FormDTO>.NotFound();
        }

        // Map Form aggregate to FormDTO
        var currentRevision = form.GetCurrentRevision();

        var dto = new FormDTO(
            form.Id,
            form.FormNumber,
            form.FormTitle ?? string.Empty,
            form.Division,
            form.Owner?.Name,
            currentRevision?.ToString(),
            form.CreatedDate ?? DateTime.UtcNow,
            form.RevisedDate ?? DateTime.UtcNow
        );

        return Result<FormDTO>.Success(dto);
    }
}

