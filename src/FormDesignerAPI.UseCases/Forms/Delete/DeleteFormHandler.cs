using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.UseCases.Forms.Delete;

public class DeleteFormHandler(IDeleteFormService service) : ICommandHandler<DeleteFormCommand, Result>
{
    // This Approach: Keep Domain Events in the Domain Model / Core project; this becomes a pass-through
    public async Task<Result> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        return await service.DeleteFormAsync(request.FormId);
    }
}
