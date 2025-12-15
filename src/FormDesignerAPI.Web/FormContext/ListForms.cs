using FormDesignerAPI.UseCases.FormContext;
using FormDesignerAPI.UseCases.FormContext.List;

namespace FormDesignerAPI.Web.FormContext;

/// <summary>
/// List all forms with optional filtering
/// </summary>
public class ListForms(IMediator _mediator)
    : Endpoint<ListFormsRequest, IEnumerable<FormContextDTO>>
{
    public override void Configure()
    {
        Get(ListFormsRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all forms";
            s.Description = "Retrieves all forms with optional filtering by active status and search term";
            s.Responses[200] = "List of forms";
        });
    }

    public override async Task HandleAsync(
        ListFormsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListFormsQuery(request.ActiveOnly, request.SearchTerm),
            cancellationToken);

        if (result.IsSuccess)
        {
            Response = result.Value!;
            await SendOkAsync(Response, cancellationToken);
        }
        else
        {
            await SendNoContentAsync(cancellationToken);
        }
    }
}
