using System;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Login;

public class LoginUserHandler : FastEndpoints.ICommandHandler<LoginUserCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService identityService;

    public LoginUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public Task<Result> ExecuteAsync(LoginUserCommand request, CancellationToken ct)
    {
        return identityService.LoginAsync(request.UserName, request.Password);
    }
}
