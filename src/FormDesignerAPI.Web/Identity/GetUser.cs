using FormDesignerAPI.UseCases.Identity.Get;

namespace FormDesignerAPI.Web.Identity;

public class GetUser(IMediator mediator) : FastEndpoints.Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get(GetUserRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserCommand(req.UserId), ct);
        if (result != null && result.Equals(Ardalis.Result.Result.Success()))
        {
            await SendAsync(new GetUserResponse { Success = true }, cancellation: ct);
        }
    }
}
