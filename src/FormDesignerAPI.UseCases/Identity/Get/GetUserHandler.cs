using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Get;

public class GetUserHandler : FastEndpoints.ICommandHandler<GetUserCommand, Ardalis.Result.Result<string?>>
{
    private readonly IIdentityService identityService;

    public GetUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<string?>> ExecuteAsync(GetUserCommand request, CancellationToken ct)
    {
        var result = await identityService.GetUserNameAsync(request.UserId);
        if (result == null)
        {
            return Ardalis.Result.Result<string?>.Error("User not found.");
        }
        return Ardalis.Result.Result<string?>.Success(result);
    }
}
