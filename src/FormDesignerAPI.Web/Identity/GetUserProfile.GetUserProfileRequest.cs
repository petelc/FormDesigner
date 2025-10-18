namespace FormDesignerAPI.Web.Identity;

public class GetUserProfileRequest
{
    public const string Route = "/Identity/GetUserProfile";

    public string UserId { get; set; } = string.Empty;
}
