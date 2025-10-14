using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.GetAllUsers;

public class GetAllUsersHandler : FastEndpoints.ICommandHandler<GetAllUsersCommand, Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>>, MediatR.IRequestHandler<GetAllUsersCommand, Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>>
{
    private readonly IIdentityService identityService;

    public GetAllUsersHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>> ExecuteAsync(GetAllUsersCommand request, CancellationToken ct)
    {
        return await HandleAsync(request, ct);
    }

    public async Task<Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>> HandleAsync(GetAllUsersCommand request, CancellationToken ct)
    {
        var result = await identityService.GetAllUsersAsync();
        if (result == null || result.Value == null || result.Value.Count == 0)
        {
            return Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>.Error("No users found.");
        }
        return Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>.Success(result.Value);
    }

    public async Task<Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>> Handle(GetAllUsersCommand request, CancellationToken cancellationToken)
    {
        return await HandleAsync(request, cancellationToken);
    }


}
