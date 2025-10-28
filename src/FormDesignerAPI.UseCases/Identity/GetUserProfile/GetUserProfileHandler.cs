using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Interfaces;
using MediatR;

namespace FormDesignerAPI.UseCases.Identity.GetUserProfile;

public class GetUserProfileHandler : FastEndpoints.ICommandHandler<GetUserProfileCommand, Ardalis.Result.Result<UserDto?>>, IRequestHandler<GetUserProfileCommand, Ardalis.Result.Result<UserDto?>>
{
    private readonly IIdentityService _identityService;

    public GetUserProfileHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result<UserDto?>> ExecuteAsync(GetUserProfileCommand request, CancellationToken ct)
    {
        return HandleAsync(request, ct);
    }

    public async Task<Ardalis.Result.Result<UserDto?>> HandleAsync(GetUserProfileCommand request, CancellationToken ct)
    {
        var result = await _identityService.GetUserProfileAsync(request.UserId);

        if (result == null)
        {
            return Ardalis.Result.Result<UserDto?>.Error("User not found.");
        }
        return Ardalis.Result.Result<UserDto?>.Success(result);
    }

    public async Task<Ardalis.Result.Result<UserDto?>> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        return await HandleAsync(request, cancellationToken);
    }
}
