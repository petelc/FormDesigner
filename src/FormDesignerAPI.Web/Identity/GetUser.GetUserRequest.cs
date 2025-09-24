namespace FormDesignerAPI.Web.Identity;

public class GetUserRequest
{
    public const string Route = "/Identity/GetUser";
    public required string UserId { get; set; }
}
