using FormDesignerAPI.UseCases.Identity.Register;

namespace FormDesignerAPI.Web.Identity;

/// <summary>
/// Register a new user
/// </summary>
/// <remarks>
/// Registers a new user given a username, email, and password.
/// </remarks>
public class Register(IMediator mediator) : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    public override void Configure()
    {
        Post(RegisterUserRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserRequest request, CancellationToken ct)
    {
        var user = request.UserName ?? throw new ArgumentNullException(nameof(request.UserName));
        var password = request.Password ?? throw new ArgumentNullException(nameof(request.Password));

        var result = await mediator.Send(new RegisterUserCommand(user, password)) as Ardalis.Result.Result<string>;

        Response = new RegisterUserResponse
        {
            Success = result != null && result.IsSuccess,
            Error = result != null && result.IsSuccess ? null : result?.Errors.FirstOrDefault()
        };
    }
}
