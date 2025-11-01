using FormDesignerAPI.Core.Interfaces;
using MediatR;

namespace FormDesignerAPI.UseCases.Identity.Login;

public class LoginUserHandler : FastEndpoints.ICommandHandler<LoginUserCommand, Ardalis.Result.Result>, IRequestHandler<LoginUserCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService identityService;

    public LoginUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public Task<Result> ExecuteAsync(LoginUserCommand request, CancellationToken ct)
    {
        return Handle(request, ct);
    }

    public Task<Result> Handle(LoginUserCommand request, CancellationToken ct)
    {
        return identityService.LoginAsync(request.UserName, request.Password);
    }
}
