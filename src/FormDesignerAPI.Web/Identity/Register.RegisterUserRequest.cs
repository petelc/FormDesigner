namespace FormDesignerAPI.Web.Identity;

public class RegisterUserRequest
{
    public const string Route = "/Identity/Register";

    public string? UserName { get; set; }
    public string? Password { get; set; }
}
