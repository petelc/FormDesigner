using FormDesignerAPI.UseCases.Identity.Delete;

namespace FormDesignerAPI.Web.Identity;

public class Delete(IMediator mediator) : FastEndpoints.Endpoint<DeleteUserRequest, DeleteUserResponse>
{
    public override void Configure()
    {
        Delete(DeleteUserRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteUserCommand(req.UserId), ct);


        await SendAsync(new DeleteUserResponse { UserId = req.UserId, Success = true }, cancellation: ct);
    }
}
