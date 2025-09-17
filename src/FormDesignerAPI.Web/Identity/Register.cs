using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.UseCases.Identity.Register;

namespace FormDesignerAPI.Web.Identity;

/// <summary>
/// Register a new user
/// </summary>
/// <remarks>
/// Registers a new user given a username, email, and password.
/// </remarks>
public class Register(IMediator mediator)
{
    public async Task<RegisterUserResponse> Handle(RegisterUserRequest request)
    {
        // var user = new ApplicationUser
        // {
        //     UserName = request.UserName,
        //     Email = request.Email,
        //     Password = request.Password
        // };

        var result = await mediator.Send(new RegisterUserCommand(user));

        return new RegisterUserResponse
        {
            Success = result.IsSuccess,
            Error = result.Error
        };
    }
}
