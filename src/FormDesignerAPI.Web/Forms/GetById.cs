using FormDesignerAPI.UseCases.Forms.Get;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Get a Form by integer ID.
/// </summary>
/// <remarks>
/// Takes a positive integer ID and returns a matching Form record.
/// </remarks>
public class GetById(IMediator _mediator)
  : Endpoint<GetFormByIdRequest, FormRecord>
{
    public override void Configure()
    {
        Get(GetFormByIdRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetFormByIdRequest request,
      CancellationToken cancellationToken)
    {
        var query = new GetFormQuery(request.FormId);

        var result = await _mediator.Send(query, cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (result.IsSuccess)
        {
            Response = new FormRecord(
                result.Value.Id,
                result.Value.FormNumber,
                result.Value.FormTitle,
                result.Value.Division,
                result.Value.Owner,
                result.Value.Revision,
                result.Value.CreatedDate,
                result.Value.RevisedDate
                );
        }
    }
}

