using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Delete;

/// <summary>
/// Handler for deleting a form
/// </summary>
public class DeleteFormHandler(IRepository<Form> repository)
    : IRequestHandler<DeleteFormCommand, Result>
{
    public async Task<Result> Handle(
        DeleteFormCommand request,
        CancellationToken cancellationToken)
    {
        var form = await repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        await repository.DeleteAsync(form, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
