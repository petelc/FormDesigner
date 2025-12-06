using FormDesignerAPI.UseCases.Identity.GetUserProfile;

namespace FormDesignerAPI.Web.Identity;

public class GetUserProfile(IMediator _mediator) : FastEndpoints.Endpoint<GetUserProfileRequest, GetUserProfileResponse>
{
    public override void Configure()
    {
        Get(GetUserProfileRequest.Route);
        AllowAnonymous();
        Description(b => b
            .Produces<GetUserProfileResponse>()
            .WithTags("Identity")
            .WithSummary("Get User Profile")
            .WithDescription("Gets the profile of a specific user by their UserId."));
    }
    public override async Task HandleAsync(GetUserProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserProfileCommand(request.UserId), cancellationToken);

        if (result != null && result.IsSuccess)
        {
            await SendAsync(new GetUserProfileResponse { User = result.Value! }, cancellation: cancellationToken);
        }
        else
        {
            await SendAsync(new GetUserProfileResponse { Error = result?.Errors?.FirstOrDefault(), Success = false }, cancellation: cancellationToken);
            return;
        }
    }
}
