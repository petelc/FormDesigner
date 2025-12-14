using FormDesignerAPI.Core.FormContext.Interfaces;

namespace FormDesignerAPI.UseCases.FormContext.Update;

/// <summary>
/// Handler for updating form name
/// </summary>
public class UpdateFormNameHandler : IRequestHandler<UpdateFormNameCommand, Result>
{
    private readonly IFormRepository _repository;

    public UpdateFormNameHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        UpdateFormNameCommand request,
        CancellationToken cancellationToken)
    {
        var form = await _repository.GetByIdAsync(request.FormId, cancellationToken);

        if (form == null)
        {
            return Result.NotFound($"Form with ID {request.FormId} not found");
        }

        form.Rename(request.NewName, request.UpdatedBy);

        await _repository.UpdateAsync(form, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
