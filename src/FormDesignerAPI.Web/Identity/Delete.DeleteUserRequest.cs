namespace FormDesignerAPI.Web.Identity;

public class DeleteUserRequest
{
    public const string Route = "/Identity/DeleteUser/{UserId}";
    public string UserId { get; set; } = string.Empty;
}
