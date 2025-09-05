using Ardalis.Result;
using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;

    public IdentityService(
      UserManager<ApplicationUser> userManager,
      IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
      IAuthorizationService authorizationService
      )
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName };
        var result = await _userManager.CreateAsync(user, password);
        return (result.ToArdalisResult(), user.Id);
    }

    public Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteUserAsync(string userId)
    {
        throw new NotImplementedException();
    }
}
