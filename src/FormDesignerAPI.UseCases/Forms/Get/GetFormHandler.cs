using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Get;

/// <summary>
/// Handler for getting a form by ID
/// </summary>
public class GetFormHandler(IRepository<Form> repository)
    : IRequestHandler<GetFormQuery, Result<FormDTO>>
{
    public async Task<Result<FormDTO>> Handle(
        GetFormQuery request,
        CancellationToken cancellationToken)
    {
        var form = await repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        var dto = new FormDTO
        {
            Id = form.Id,
            FormNumber = form.FormNumber,
            FormTitle = form.FormTitle,
            Division = form.Division,
            Owner = form.Owner?.Name,
            Version = form.Version,
            CreatedDate = form.CreatedDate,
            RevisedDate = form.RevisedDate,
            ConfigurationPath = form.ConfigurationPath
        };

        return Result.Success(dto);
    }
}
