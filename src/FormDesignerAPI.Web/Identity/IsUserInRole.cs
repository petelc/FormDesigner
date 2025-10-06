using FormDesignerAPI.UseCases.Identity.Role.CheckUser;

namespace FormDesignerAPI.Web.Identity;

public class IsUserInRole(IMediator _mediator) : Endpoint<IsUserInRoleRequest, IsUserInRoleResponse>
{
    public override void Configure()
    {
        Get(IsUserInRoleRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(IsUserInRoleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new IsUserInRoleCommand(req.UserId, req.Role), ct);

        Response = new IsUserInRoleResponse
        {
            IsInRole = result
        };
    }

}
