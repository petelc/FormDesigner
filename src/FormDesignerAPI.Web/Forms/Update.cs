using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.UseCases.Forms.Get;
using FormDesignerAPI.UseCases.Forms.Update;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Update an existing Form.
/// </summary>
/// <remarks>
/// Update an existing Form by providing a fully defined replacement set of values.
/// See: https://stackoverflow.com/questions/60761955/rest-update-best-practice-put-collection-id-without-id-in-body-vs-put-collecti
/// </remarks>
public class Update(IMediator _mediator)
  : Endpoint<UpdateFormRequest, UpdateFormResponse>
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
        var result = await _mediator.Send(new UpdateFormCommand(
            request.Id,
            request.FormNumber!,
            request.FormTitle!,
            request.Division!,
            request.Owner!,
            request.Version!,
            request.RevisedDate ?? DateTime.UtcNow,
            request.ConfigurationPath!
            ), cancellationToken);

        if (result.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var query = new GetFormQuery(request.Id);

        var queryResult = await _mediator.Send(query, cancellationToken);

        if (queryResult.Status == ResultStatus.NotFound)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (queryResult.IsSuccess)
        {
            var dto = queryResult.Value;
            Response = new UpdateFormResponse(new FormRecord(
                dto.Id,
                dto.FormNumber,
                dto.FormTitle,
                dto.Division,
                dto.Owner,
                dto.Version,
                dto.CreatedDate,
                dto.RevisedDate,
                dto.ConfigurationPath
            ));
            return;
        }
    }
}


