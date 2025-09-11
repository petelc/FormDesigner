using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.UseCases.Forms.Create;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// Create a new Form
/// </summary>
/// <remarks>
/// Creates a new Form given a Form Number and Form Title.
/// </remarks>
public class Create(IMediator _mediator)
  : Endpoint<CreateFormRequest, CreateFormResponse>
{
    public override void Configure()
    {
        Post(CreateFormRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            // XML Docs are used by default but are overridden by these properties:
            //s.Summary = "Create a new Form.";
            //s.Description = "Create a new Form. A valid Form Number and Form Title are required.";
            s.ExampleRequest = new CreateFormRequest { FormNumber = "Form Number", FormTitle = "Form Title", Division = "Division", Owner = new Owner("Owner Name", "owner@example.com"), Version = "1.0", ConfigurationPath = "/path/to/config" };
        });
    }

    public override async Task HandleAsync(
        CreateFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateFormCommand(request.FormNumber!,
        request.FormTitle!, request.Division, request.Owner, request.Version, request.ConfigurationPath), cancellationToken);

        if (result.IsSuccess)
        {
            Response = new CreateFormResponse(result.Value, request.FormNumber!,
                request.FormTitle!, request.Division, request.Owner, request.Version, request.ConfigurationPath);
            return;
        }
    }
}
