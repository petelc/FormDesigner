namespace FormDesignerAPI.Web.Identity;

public class GetUserRequest
{
    public const string Route = "/Identity/GetUser/{UserId}";
    public string UserId { get; set; } = string.Empty;
}
