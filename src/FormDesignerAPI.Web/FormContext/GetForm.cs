using FormDesignerAPI.UseCases.FormContext;
using FormDesignerAPI.UseCases.FormContext.Get;

namespace FormDesignerAPI.Web.FormContext;

/// <summary>
/// Get a form by ID
/// </summary>
public class GetForm(IMediator _mediator)
    : Endpoint<GetFormRequest, FormContextDTO>
{
    public override void Configure()
    {
        Get(GetFormRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get a form by ID";
            s.Description = "Retrieves a form with its full definition and metadata";
            s.Responses[200] = "Form found";
            s.Responses[404] = "Form not found";
        });
    }

    public override async Task HandleAsync(
        GetFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetFormQuery(request.Id),
            cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            Response = result.Value;
            await SendOkAsync(Response, cancellationToken);
        }
        else
        {
            await SendNotFoundAsync(cancellationToken);
        }
    }
}
