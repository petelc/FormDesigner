

namespace FormDesignerAPI.UseCases.Interfaces;

public interface IIdentityService
{
    Task<Result<string?>> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    Task<Result> LoginAsync(string userName, string password);
    Task LogoutAsync();
    Task<Result> DeleteUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<Result> AddUserToRoleAsync(string userId, string role);
    Task<Result> RemoveUserFromRoleAsync(string userId, string role);
    Task<Result> RemoveUserFromAllRolesAsync(string userId);
}
