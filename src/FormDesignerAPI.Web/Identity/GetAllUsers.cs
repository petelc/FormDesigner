using FormDesignerAPI.UseCases.Identity.GetAllUsers;


namespace FormDesignerAPI.Web.Identity;

public class GetAllUsers(IMediator _mediator) : FastEndpoints.Endpoint<GetAllUsersRequest, GetAllUsersResponse>
{
    public override void Configure()
    {
        Get(GetAllUsersRequest.Route);
        AllowAnonymous();
        Description(b => b
            .Produces<GetAllUsersResponse>()
            .WithTags("Identity")
            .WithSummary("Get All Users")
            .WithDescription("Gets a list of all users."));
    }

    public override async Task HandleAsync(GetAllUsersRequest req, CancellationToken ct)
    {
        var users = await _mediator.Send(new GetAllUsersCommand(), ct);
        await SendAsync(new GetAllUsersResponse { Users = users }, 200, ct);
    }
}
