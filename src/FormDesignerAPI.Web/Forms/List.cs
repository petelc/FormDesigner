using System;
using FormDesignerAPI.UseCases.Forms;
using FormDesignerAPI.UseCases.Forms.List;

namespace FormDesignerAPI.Web.Forms;

/// <summary>
/// List all Forms
/// </summary>
/// <remarks>
/// List all forms - returns a FormListResponse containing the Forms.
/// </remarks>
public class List(IMediator _mediator) : EndpointWithoutRequest<FormListResponse>
{
    public override void Configure()
    {
        Get("/Forms");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        Result<IEnumerable<FormDTO>> result = await _mediator.Send(new ListFormsQuery(null, null), cancellationToken);

        if (result.IsSuccess)
        {
            Response = new FormListResponse
            {
                Forms = result.Value.Select(f => new FormRecord(
                    f.Id,
                    f.FormNumber,
                    f.FormTitle,
                    f.Division ?? string.Empty,
                    f.Owner ?? string.Empty,
                    f.Version,
                    f.CreatedDate,
                    f.RevisedDate
                    )).ToList()
            };
        }
    }

}
