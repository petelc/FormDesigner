using Ardalis.Result;
using FormDesignerAPI.UseCases.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
      UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
      IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
      IAuthorizationService authorizationService,
        ILogger<IdentityService> logger
      )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public async Task<Result<string?>> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName };
        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<Result> LoginAsync(string userName, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed for user {UserName}", userName);
            return Result.Unauthorized();
        }

        _logger.LogInformation("User {UserName} logged in successfully", userName);
        return Result.Success();
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out successfully");
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        //if (user == null) return false;
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<Result> AddUserToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.NotFound();

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.ToApplicationResult();
    }

    public async Task<Result> RemoveUserFromRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.NotFound();

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.ToApplicationResult();
    }

    public async Task<Result> RemoveUserFromAllRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        return result.ToApplicationResult();
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }


    //** TODO: I should also remove a deleted user from any roles they are in
    /*and possibly clean up any related data depending on the application's requirements.
    /* This is a basic implementation and might need to be expanded based on specific needs.
    */
    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.NotFound();

        var result = await _userManager.DeleteAsync(user);
        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);
        return result.ToApplicationResult();
    }
}
