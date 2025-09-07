using FormDesignerAPI.Core.FormAggregate;

namespace FormDesignerAPI.UseCases.Forms.Update;

public class UpdateFormHandler(IRepository<Form> _repository)
  : ICommandHandler<UpdateFormCommand, Result<FormDTO>>
{
  public async Task<Result<FormDTO>> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
  {
    var existingForm = await _repository.GetByIdAsync(request.FormId, cancellationToken);
    if (existingForm == null)
    {
      return Result.NotFound();
    }

    existingForm.UpdateDetails(request.newFormNumber, request.newFormTitle, request.newDivision, request.newOwner, request.newVersion, request.newConfigurationPath);

    await _repository.UpdateAsync(existingForm, cancellationToken);

    return new FormDTO(
      existingForm.Id,
      existingForm.FormNumber,
      existingForm.FormTitle,
      existingForm.Division! ?? "",
      existingForm.Owner!.Name ?? "",
      existingForm.Version! ?? "",
      existingForm.Status,
      existingForm.CreatedDate,
      existingForm.RevisedDate,
      existingForm.ConfigurationPath
      );
  }

}
