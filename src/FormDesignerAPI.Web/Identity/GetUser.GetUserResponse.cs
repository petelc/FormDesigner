namespace FormDesignerAPI.Web.Identity;

public class GetUserResponse()
{
    public string? UserName { get; set; }
    public string? Error { get; set; }
    public bool Success { get; set; }
}
