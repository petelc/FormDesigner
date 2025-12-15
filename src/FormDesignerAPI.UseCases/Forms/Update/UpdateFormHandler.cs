using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Update;

/// <summary>
/// Handler for updating an existing form
/// </summary>
public class UpdateFormHandler(IRepository<Form> repository)
    : IRequestHandler<UpdateFormCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFormCommand request,
        CancellationToken cancellationToken)
    {
        var form = await repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        form.Update(
            request.FormNumber,
            request.FormTitle,
            request.Division,
            request.Owner,
            request.Version,
            request.RevisedDate,
            request.ConfigurationPath
        );

        await repository.UpdateAsync(form, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
