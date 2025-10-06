namespace FormDesignerAPI.Web.Identity;

public class AddUserToRoleRequest
{
    public const string Route = "/Identity/AddUserToRole/{UserId}/{RoleName}";
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
