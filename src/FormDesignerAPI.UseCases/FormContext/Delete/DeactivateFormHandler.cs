using FormDesignerAPI.Core.FormContext.Interfaces;

namespace FormDesignerAPI.UseCases.FormContext.Delete;

/// <summary>
/// Handler for deactivating a form
/// </summary>
public class DeactivateFormHandler : IRequestHandler<DeactivateFormCommand, Result>
{
    private readonly IFormRepository _repository;

    public DeactivateFormHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        DeactivateFormCommand request,
        CancellationToken cancellationToken)
    {
        var form = await _repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        form.Deactivate();

        await _repository.UpdateAsync(form, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
