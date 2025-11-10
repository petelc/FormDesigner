using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.Web.Identity;

public class GetUserProfileResponse
{
    public UserDto User { get; set; } = new();
    public string? Error { get; set; }
    public bool Success { get; set; }
}
