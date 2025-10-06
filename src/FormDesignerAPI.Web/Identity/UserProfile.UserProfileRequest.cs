namespace FormDesignerAPI.Web.Identity;

public class UserProfileRequest
{
    public const string Route = "/Identity/UserProfile";

    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Division { get; set; }
    public string? JobTitle { get; set; }
    public string? Supervisor { get; set; }
    public string? ProfileImageUrl { get; set; }
}
