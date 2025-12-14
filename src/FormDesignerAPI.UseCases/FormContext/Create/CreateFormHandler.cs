using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.UseCases.FormContext.Create;

/// <summary>
/// Handler for creating a new form
/// </summary>
public class CreateFormHandler : IRequestHandler<CreateFormCommand, Result<Guid>>
{
    private readonly IFormRepository _repository;

    public CreateFormHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(
        CreateFormCommand request,
        CancellationToken cancellationToken)
    {
        // Create origin metadata for manual form creation
        var origin = OriginMetadata.Manual(request.CreatedBy);

        // Create the form aggregate
        var form = Form.Create(
            request.Name,
            request.Definition,
            origin,
            request.CreatedBy);

        // Save to repository
        await _repository.AddAsync(form, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(form.Id);
    }
}
