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
    var createdItem = await _repository.AddAsync(newForm, cancellationToken);

    return createdItem.Id;
  }
}
