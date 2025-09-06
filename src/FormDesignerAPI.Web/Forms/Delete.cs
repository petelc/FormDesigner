using FormDesignerAPI.UseCases.Forms.Delete;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Delete a Form.
/// </summary>
/// <remarks>
/// Delete a Form by providing a valid integer id.
/// </remarks>
public class Delete(IMediator _mediator)
  : Endpoint<DeleteFormRequest>
{
    public override void Configure()
    {
        Delete(DeleteFormRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
      DeleteFormRequest request,
      CancellationToken cancellationToken)
    {
        var command = new DeleteFormCommand(request.FormId);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (result.IsSuccess)
        {
            await SendNoContentAsync(cancellationToken);
        }
        ;
    }
}
