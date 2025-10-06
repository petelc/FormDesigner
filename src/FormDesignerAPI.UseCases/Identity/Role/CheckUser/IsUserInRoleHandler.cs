using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Role.CheckUser;

public class IsUserInRoleHandler : FastEndpoints.ICommandHandler<IsUserInRoleCommand, Ardalis.Result.Result<bool>>, MediatR.IRequestHandler<IsUserInRoleCommand, Ardalis.Result.Result<bool>>
{
    private readonly IIdentityService identityService;

    public IsUserInRoleHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result<bool>> ExecuteAsync(IsUserInRoleCommand request, CancellationToken cancellationToken)
    {
        return await Handle(request, cancellationToken);
    }

    public async Task<Ardalis.Result.Result<bool>> Handle(IsUserInRoleCommand request, CancellationToken cancellationToken)
    {
        var isInRole = await identityService.IsInRoleAsync(request.UserId, request.Role);
        return Ardalis.Result.Result.Success(isInRole);
    }

}
