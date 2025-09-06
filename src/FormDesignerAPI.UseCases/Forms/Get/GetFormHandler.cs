using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.FormAggregate.Specifications;

namespace FormDesignerAPI.UseCases.Forms.Get;

/// <summary>
/// Queries don't necessarily need to use repository methods, but they can if it's convenient
/// </summary>
/// </summary>
public class GetFormHandler
(IReadRepository<Form> _repository)
  : IQueryHandler<GetFormQuery, Result<FormDTO>>
{
    public async Task<Result<FormDTO>> Handle(GetFormQuery request, CancellationToken cancellationToken)
    {
        var spec = new FormByIdSpec(request.FormId);
        var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        if (entity == null) return Result<FormDTO>.NotFound();



        var dto = new FormDTO(
            entity.Id,
            entity.FormNumber,
            entity.FormTitle ?? "",
            entity.Division ?? "",
            entity.Owner!.Name ?? "",
            entity.Version ?? "",
            entity.Status,
            entity.CreatedDate,
            entity.RevisedDate,
            entity.ConfigurationPath ?? ""
        );
        return Result<FormDTO>.Success(dto);
    }
}
