using System;

namespace FormDesignerAPI.Web.Identity;

public class AddUserToRoleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
