using FormDesignerAPI.UseCases.Interfaces;
using MediatR;

namespace FormDesignerAPI.UseCases.Identity.Get;

public class GetUserHandler : FastEndpoints.ICommandHandler<GetUserCommand, Ardalis.Result.Result<string?>>, IRequestHandler<GetUserCommand, Ardalis.Result.Result<string?>>
{
    private readonly IIdentityService identityService;

    public GetUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<string?>> ExecuteAsync(GetUserCommand request, CancellationToken ct)
    {
        return await HandleAsync(request, ct);
    }

    public async Task<Ardalis.Result.Result<string?>> HandleAsync(GetUserCommand request, CancellationToken ct)
    {
        var result = await identityService.GetUserNameAsync(request.UserId);
        if (result == null)
        {
            return Ardalis.Result.Result<string?>.Error("User not found.");
        }
        return Ardalis.Result.Result<string?>.Success(result);
    }

    public async Task<Ardalis.Result.Result<string?>> Handle(GetUserCommand request, CancellationToken cancellationToken)
    {
        return await HandleAsync(request, cancellationToken);
    }
}
