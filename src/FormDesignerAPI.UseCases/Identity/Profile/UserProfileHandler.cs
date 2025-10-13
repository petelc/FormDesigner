using FormDesignerAPI.Core.Interfaces;
using MediatR;

namespace FormDesignerAPI.UseCases.Identity.Profile;

public class UserProfileHandler : IRequestHandler<UserProfileCommand, Ardalis.Result.Result>
{
    private readonly IIdentityService identityService;

    public UserProfileHandler(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    public async Task<Ardalis.Result.Result> Handle(UserProfileCommand request, CancellationToken cancellationToken)
    {
        return await identityService.UpdateUserProfileAsync(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.Division,
            request.JobTitle,
            request.Supervisor,
            request.ProfileImageUrl
        );
    }
}
