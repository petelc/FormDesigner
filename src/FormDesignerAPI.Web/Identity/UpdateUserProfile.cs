using FormDesignerAPI.UseCases.Identity.Profile;

namespace FormDesignerAPI.Web.Identity;

public class UpdateUserProfile(IMediator _mediator) : Endpoint<UpdateUserProfileRequest, UpdateUserProfileResponse>
{
    public override void Configure()
    {
        Post(UpdateUserProfileRequest.Route);
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateUserProfileRequest req, CancellationToken ct)
    {
        var response = new UpdateUserProfileResponse();
        var command = new UpdateUserProfileCommand(
            req.UserId ?? string.Empty,
            req.FirstName ?? string.Empty,
            req.LastName ?? string.Empty,
            req.Division ?? string.Empty,
            req.JobTitle ?? string.Empty,
            req.Supervisor ?? string.Empty,
            req.ProfileImageUrl ?? string.Empty
        );

        var result = await _mediator.Send(command, ct);

        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
        {
            response.Success = false;
            response.Error = "User not found.";
            await SendAsync(response, cancellation: ct);
            return;
        }

        if (result.IsSuccess)
        {
            response.Success = true;
            await SendAsync(response, cancellation: ct);
            return;
        }

    }
}
