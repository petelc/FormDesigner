using FormDesignerAPI.UseCases.Identity.Get;

namespace FormDesignerAPI.Web.Identity;

public class GetUser(IMediator mediator) : FastEndpoints.Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get(GetUserRequest.Route);
        AllowAnonymous();
        // Bind UserId from route
        Summary(s =>
        {
            s.Summary = "Get a user by userId.";
            s.Description = "Returns the username for the given userId.";
        });
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserCommand(req.UserId), ct);
        if (result != null && result.IsSuccess)
        {
            await SendAsync(new GetUserResponse { UserName = result.Value, Success = true }, cancellation: ct);
        }
        else
        {
            await SendAsync(new GetUserResponse { Error = result?.Errors?.FirstOrDefault(), Success = false }, cancellation: ct);
        }
    }
}
