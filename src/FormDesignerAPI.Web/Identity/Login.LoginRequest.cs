using FormDesignerAPI.Web.Common;

namespace FormDesignerAPI.Web.Identity;

public class LoginRequest : BaseRequest
{

    public string? UserName { get; set; }
    public string? Password { get; set; }
}
