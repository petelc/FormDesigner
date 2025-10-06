using FormDesignerAPI.UseCases.Identity.Role.AddUser;

namespace FormDesignerAPI.Web.Identity;

public class AddUserToRole(IMediator mediator) : FastEndpoints.Endpoint<AddUserToRoleRequest, AddUserToRoleResponse>
{
    public override void Configure()
    {
        Post(AddUserToRoleRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddUserToRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AddUserToRoleCommand(request.UserId, request.RoleName));

        Response = new AddUserToRoleResponse
        {
            Success = result.IsSuccess,
            Message = result.IsSuccess ? "User added to role successfully." : string.Join("; ", result.Errors)
        };
    }
}
