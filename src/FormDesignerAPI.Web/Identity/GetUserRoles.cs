using FormDesignerAPI.UseCases.Identity.Role.GetUserRoles;

namespace FormDesignerAPI.Web.Identity;

public class GetUserRoles(IMediator _mediator) : Endpoint<GetUserRolesRequest, GetUserRolesResponse>
{
    public override void Configure()
    {
        Get(GetUserRolesRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetUserRolesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserRolesCommand(req.UserId), ct);

        Response = new GetUserRolesResponse
        {
            Roles = result.Value != null ? result.Value.ToList() : new List<string>()
        };
    }

}

