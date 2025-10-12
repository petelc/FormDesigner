using System;
using FormDesignerAPI.UseCases.Identity.Logout;

namespace FormDesignerAPI.Web.Identity;

public class Logout(IMediator mediator) : EndpointWithoutRequest
{

    public override void Configure()
    {
        Post(LogoutUserRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await mediator.Send(new LogoutUserCommand(), ct);
        await SendNoContentAsync(cancellation: ct);
    }
}
