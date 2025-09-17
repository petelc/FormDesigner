using FormDesignerAPI.UseCases.Interfaces;
using Ardalis.Result;

namespace FormDesignerAPI.UseCases.Identity.Register;

public class RegisterUserHandler(IIdentityService identityService) : ICommandHandler<RegisterUserCommand, Result<string>>
{

    public Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        string userName = request.UserName ?? throw new ArgumentNullException(nameof(request.UserName));

        string password = request.Password ?? throw new ArgumentNullException(nameof(request.Password));

        return identityService.CreateUserAsync(userName, password);
    }

}
