using Ardalis.Result;
using FormDesignerAPI.Core.Entities;
using FormDesignerAPI.Core.Interfaces;
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

    #region User Management
    public async Task<Result<string?>> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<Result<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        return Result<List<UserDto>>.Success(users.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Division = u.Division,
            JobTitle = u.JobTitle,
            Supervisor = u.Supervisor,
            PhoneNumber = u.PhoneNumber,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToList());
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName, Email = userName };
        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<Result> UpdateUserProfileAsync(string userId, string firstName, string lastName, string division, string jobTitle, string supervisor, string? profileImageUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.NotFound();

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Division = division;
        user.JobTitle = jobTitle;
        user.Supervisor = supervisor;
        user.ProfileImageUrl = profileImageUrl;

        var result = await _userManager.UpdateAsync(user);
        return result.ToApplicationResult();
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
    #endregion

    #region User Roles
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
    #endregion

    /// <summary>
    /// Check if a user meets the requirements of a specific policy
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    #region DeleteUser
    //* TODO: I should also remove a deleted user from any roles they are in and possibly 
    //* clean up any related data depending on the application's requirements.
    //* This is a basic implementation and might need to be expanded based on specific needs. 

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


    #endregion
}
