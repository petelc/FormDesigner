using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Role.GetUserRoles;

public class GetUserRolesHandler : FastEndpoints.ICommandHandler<GetUserRolesCommand, Ardalis.Result.Result<List<string>>>, MediatR.IRequestHandler<GetUserRolesCommand, Ardalis.Result.Result<List<string>>>
{
    private readonly IIdentityService identityService;

    public GetUserRolesHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<List<string>>> ExecuteAsync(GetUserRolesCommand request, CancellationToken cancellationToken)
    {
        return await Handle(request, cancellationToken);
    }

    public async Task<Ardalis.Result.Result<List<string>>> Handle(GetUserRolesCommand request, CancellationToken cancellationToken)
    {
        var roles = await identityService.GetUserRolesAsync(request.UserId);
        return Ardalis.Result.Result.Success(roles);
    }

}
