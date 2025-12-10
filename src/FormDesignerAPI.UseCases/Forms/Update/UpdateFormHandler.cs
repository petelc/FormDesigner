// using FormDesignerAPI.Core.FormAggregate;
// using FormDesignerAPI.Core.Interfaces;
// using MediatR;


// namespace FormDesignerAPI.UseCases.Forms.Update;

// public class UpdateFormHandler(IFormUpdateService service)
//   : IRequestHandler<UpdateFormCommand, Result>
// {
//   public async Task<Result> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
//   {
//     var form = new FormUpdateDto
//     (
//       request.FormId,
//       request.newFormNumber,
//       request.newFormTitle,
//       request.newDivision,
//       request.newOwner,
//       request.newVersion,
//       request.RevisedDate,
//       request.newConfigurationPath
//     );
//     return await service.UpdateFormAsync(form.Id, form, cancellationToken);
//   }

// }
