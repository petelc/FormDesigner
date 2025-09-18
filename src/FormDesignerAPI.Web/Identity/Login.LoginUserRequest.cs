using System;

namespace FormDesignerAPI.Web.Identity;

public class LoginUserRequest
{
    public const string Route = "/Identity/Login";

    public string? UserName { get; set; }
    public string? Password { get; set; }
}
