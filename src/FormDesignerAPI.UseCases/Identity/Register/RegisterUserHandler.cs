
using Ardalis.SharedKernel;
using System.Threading.Tasks;
using FormDesignerAPI.UseCases.Interfaces;
using FastEndpoints;

namespace FormDesignerAPI.UseCases.Identity.Register;

public class RegisterUserHandler : FastEndpoints.ICommandHandler<RegisterUserCommand, Ardalis.Result.Result<string>>
{
    private readonly IIdentityService identityService;

    public RegisterUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<string>> ExecuteAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        string userName = request.UserName ?? throw new ArgumentNullException(nameof(request.UserName));
        string password = request.Password ?? throw new ArgumentNullException(nameof(request.Password));

        var (result, userId) = await identityService.CreateUserAsync(userName, password);
        if (result.IsSuccess)
        {
            return Ardalis.Result.Result<string>.Success(userId);
        }
        return Ardalis.Result.Result<string>.Error(string.Join("; ", result.Errors));
    }
}

