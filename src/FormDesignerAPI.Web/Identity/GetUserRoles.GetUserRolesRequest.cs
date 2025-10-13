namespace FormDesignerAPI.Web.Identity;

public class GetUserRolesRequest
{
    public const string Route = "/Identity/GetUserRoles/{UserId}";
    public required string UserId { get; set; }
}
