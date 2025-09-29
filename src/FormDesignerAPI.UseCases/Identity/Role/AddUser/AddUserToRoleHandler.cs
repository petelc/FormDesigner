using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Role.AddUser;

public class AddUserToRoleHandler : FastEndpoints.ICommandHandler<AddUserToRoleCommand, Ardalis.Result.Result>, MediatR.IRequestHandler<AddUserToRoleCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService identityService;

    public AddUserToRoleHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result> ExecuteAsync(AddUserToRoleCommand request, CancellationToken cancellationToken)
    {
        return await Handle(request, cancellationToken);
    }

    public async Task<Ardalis.Result.Result> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.AddUserToRoleAsync(request.UserId, request.RoleName);
        if (result.IsSuccess)
        {
            return Ardalis.Result.Result.Success();
        }
        return Ardalis.Result.Result.Error(string.Join("; ", result.Errors));
    }


}
