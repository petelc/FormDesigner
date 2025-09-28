namespace FormDesignerAPI.Web.Identity;

public class UserInfo
{
    public static readonly UserInfo Anonymous = new UserInfo();
    public bool IsAuthenticated { get; init; }
    public string NameClaimType { get; init; } = string.Empty;
    public string RoleClaimType { get; init; } = string.Empty;
    public IEnumerable<ClaimValue> Claims { get; init; } = new List<ClaimValue>();
}
