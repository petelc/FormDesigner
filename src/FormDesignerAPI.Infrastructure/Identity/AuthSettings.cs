namespace FormDesignerAPI.Infrastructure.Identity;

public class AuthSettings
{
    public string? AUTH_KEY { get; set; }
    public string JWT_SECRET_KEY { get; set; } = string.Empty;
    public string JWT_ISSUER { get; set; } = "FormDesignerAPI";
    public string JWT_AUDIENCE { get; set; } = "FormDesignerAPIClients";
    public int JWT_EXPIRATION_HOURS { get; set; } = 7;
}
