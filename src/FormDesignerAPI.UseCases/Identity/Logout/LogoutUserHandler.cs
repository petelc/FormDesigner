using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Logout;

public class LogoutUserHandler : FastEndpoints.ICommandHandler<LogoutUserCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService _identityService;

    public LogoutUserHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Ardalis.Result.Result> ExecuteAsync(LogoutUserCommand command, CancellationToken cancellationToken)
    {
        await _identityService.LogoutAsync();
        return Ardalis.Result.Result.Success();
    }
}
