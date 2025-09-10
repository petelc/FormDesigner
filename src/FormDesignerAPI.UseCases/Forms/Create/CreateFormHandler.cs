using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Create;

public class CreateFormHandler(IRepository<Form> _repository)
  : ICommandHandler<CreateFormCommand, Result<int>>
{
  public async Task<Result<int>> Handle(CreateFormCommand request,
    CancellationToken cancellationToken)
  {
    var newForm = new Form(request.FormNumber);
    if (!string.IsNullOrEmpty(request.FormTitle))
    {
      newForm.UpdateFormTitle(request.FormTitle);
    }

    if (!string.IsNullOrEmpty(request.Division))
    {
      newForm.UpdateDivision(request.Division);
    }


    //var newOwner = new Owner(request.Owner!.Name, request.Owner.Email);
    if (request.Owner is not null)
    {
      newForm.SetOwner(request.Owner!.Name, request.Owner.Email);
    }

    if (!string.IsNullOrEmpty(request.Version))
    {
      newForm.UpdateVersion(request.Version);
    }

    if (!string.IsNullOrEmpty(request.ConfigurationPath))
    {
      newForm.SetConfigurationPath(request.ConfigurationPath);
    }

    var createdItem = await _repository.AddAsync(newForm, cancellationToken);

    return createdItem.Id;
  }
}
