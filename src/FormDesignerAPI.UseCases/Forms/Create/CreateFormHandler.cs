using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Handler for creating a new form
/// </summary>
public class CreateFormHandler(IRepository<Form> repository)
    : IRequestHandler<CreateFormCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateFormCommand request,
        CancellationToken cancellationToken)
    {
        var form = Form.Create(
            request.FormNumber,
            request.FormTitle,
            request.Division,
            request.Owner ?? new Owner("Unknown", "unknown@example.com"),
            request.Version,
            request.CreatedDate,
            request.RevisedDate,
            request.ConfigurationPath
        );

        var createdForm = await repository.AddAsync(form, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success(createdForm.Id);
    }
}
