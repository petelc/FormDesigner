using System;

namespace FormDesignerAPI.Web.Identity;

public class LoginUserResponse()
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
