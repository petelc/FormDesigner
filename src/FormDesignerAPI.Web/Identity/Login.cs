using FormDesignerAPI.UseCases.Identity.Login;

namespace FormDesignerAPI.Web.Identity;

public class Login(IMediator mediator) : Endpoint<LoginUserRequest, LoginUserResponse>
{
    public override void Configure()
    {
        Post(LoginUserRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginUserRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginUserCommand(req.UserName!, req.Password!), ct);

        if (result != null && result.Equals(Ardalis.Result.Result.Success()))
        {
            await SendAsync(new LoginUserResponse { Success = true }, cancellation: ct);
        }
    }
}
