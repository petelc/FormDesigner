using FormDesignerAPI.UseCases.Forms.Get;
using FormDesignerAPI.UseCases.Forms.Update;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Update an existing Form.
/// </summary>
/// <remarks>
/// Update an existing Form by providing a fully defined replacement set of values.
/// </remarks>
public class Update(IMediator _mediator)
  : Endpoint<UpdateFormRequest>
{
    public override void Configure()
    {
        Put(UpdateFormRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(
      UpdateFormRequest request,
      CancellationToken cancellationToken)
    {
        var command = new UpdateFormCommand(
            request.FormId,
            request.FormNumber ?? string.Empty,
            request.FormTitle ?? string.Empty,
            request.Division ?? string.Empty,
            request.Owner ?? string.Empty,
            request.Version ?? string.Empty,
            request.RevisedDate ?? DateTime.UtcNow,
            request.ConfigurationPath ?? string.Empty
        );
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (result.IsSuccess)
        {
            var getQuery = new GetFormQuery(request.FormId);
            var getResult = await _mediator.Send(getQuery, cancellationToken);

            if (getResult.IsSuccess)
            {
                await SendOkAsync(getResult.Value, cancellationToken);
                return;
            }
        }
    }
}


