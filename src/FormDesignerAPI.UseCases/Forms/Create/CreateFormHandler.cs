using Ardalis.Result;
using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;
using MediatR;

namespace FormDesignerAPI.UseCases.Forms.Create;

/// <summary>
/// Handler for creating a new form using the FormBuilder pattern.
/// Implements both FastEndpoints.ICommandHandler and MediatR.IRequestHandler for form creation.
/// </summary>
public class CreateFormHandler(IRepository<Form> _repository)
  : FastEndpoints.ICommandHandler<CreateFormCommand, Result<Guid>>, IRequestHandler<CreateFormCommand, Result<Guid>>
{
  public async Task<Result<Guid>> ExecuteAsync(CreateFormCommand request, CancellationToken cancellationToken)
  {
    return await Handle(request, cancellationToken);
  }

  public async Task<Result<Guid>> Handle(CreateFormCommand request, CancellationToken cancellationToken)
  {
    // Create a new form using the FormBuilder pattern
    var formBuilder = Form.CreateBuilder(request.FormNumber);

    // Configure the form with optional properties
    if (!string.IsNullOrEmpty(request.FormTitle))
    {
      formBuilder.WithTitle(request.FormTitle);
    }

    if (!string.IsNullOrEmpty(request.Division))
    {
      formBuilder.WithDivision(request.Division);
    }

    if (request.Owner is not null)
    {
      formBuilder.WithOwner(request.Owner);
    }

    // Set creation date
    var createdDate = request.CreatedDate ?? DateTime.UtcNow;
    formBuilder.WithCreatedDate(createdDate);

    // Set revision date if provided, otherwise use creation date
    var revisedDate = request.RevisedDate ?? createdDate;
    formBuilder.WithRevisedDate(revisedDate);

    // Build the form aggregate
    var newForm = formBuilder.Build();

    // Persist the form
    var createdItem = await _repository.AddAsync(newForm, cancellationToken);

    return Result.Success(createdItem.FormId);
  }
}

