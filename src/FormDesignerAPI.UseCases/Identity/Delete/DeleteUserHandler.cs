using FormDesignerAPI.Core.Interfaces;

namespace FormDesignerAPI.UseCases.Identity.Delete;

public class DeleteUserHandler : FastEndpoints.ICommandHandler<DeleteUserCommand, Ardalis.Result.Result>, MediatR.IRequestHandler<DeleteUserCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService identityService;

    public DeleteUserHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result> ExecuteAsync(DeleteUserCommand request, CancellationToken ct)
    {
        return await HandleAsync(request, ct);
    }

    public async Task<Ardalis.Result.Result> HandleAsync(DeleteUserCommand request, CancellationToken ct)
    {
        var result = await identityService.DeleteUserAsync(request.UserId);
        if (!result.IsSuccess)
        {
            return Ardalis.Result.Result.Error(string.Join(", ", result.Errors));
        }
        return Ardalis.Result.Result.Success();
    }

    public async Task<Ardalis.Result.Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return await HandleAsync(request, cancellationToken);
    }


}
