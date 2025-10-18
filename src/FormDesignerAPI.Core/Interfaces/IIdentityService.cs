using Ardalis.Result;
using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.Core.Interfaces;

public interface IIdentityService
{
    Task<Result<string?>> GetUserNameAsync(string userId);
    Task<Result<List<UserDto>>> GetAllUsersAsync();
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    Task<Result> UpdateUserProfileAsync(string userId, string firstName, string lastName, string division, string jobTitle, string supervisor, string? profileImageUrl);
    Task<Result> LoginAsync(string userName, string password);
    Task LogoutAsync();
    Task<Result> DeleteUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<Result> AddUserToRoleAsync(string userId, string role);
    Task<Result> RemoveUserFromRoleAsync(string userId, string role);
    Task<Result> RemoveUserFromAllRolesAsync(string userId);
}
