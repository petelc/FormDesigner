namespace FormDesignerAPI.Web.Identity;

public class IsUserInRoleRequest
{
    public const string Route = "/Identity/IsUserInRole/{UserId}/{Role}";
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
