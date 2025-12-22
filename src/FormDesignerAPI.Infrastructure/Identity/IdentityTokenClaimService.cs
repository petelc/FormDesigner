using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FormDesignerAPI.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;


namespace FormDesignerAPI.Infrastructure.Identity;


public class IdentityTokenClaimService : ITokenClaimService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<IdentityTokenClaimService> _logger;

    public IdentityTokenClaimService(UserManager<ApplicationUser> userManager, IOptions<AuthSettings> authOptions, ILogger<IdentityTokenClaimService> logger)
    {
        this.userManager = userManager;
        _authSettings = authOptions.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_authSettings.JWT_SECRET_KEY))
        {
            throw new ArgumentNullException(nameof(_authSettings.JWT_SECRET_KEY), "JWT_SECRET_KEY is not configured");
        }
    }

    public async Task<string> GetTokenAsync(string userName)
    {
        _logger.LogInformation("Starting token generation for user: {UserName}", userName);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes(_authSettings.JWT_SECRET_KEY);
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserName}", userName);
            throw new UserNotFoundException(userName);
        }

        _logger.LogInformation("User found: {UserName}, Id: {UserId}", userName, user.Id);

        var roles = await userManager.GetRolesAsync(user);
        _logger.LogInformation("User {UserName} has {RoleCount} roles", userName, roles.Count);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_authSettings.JWT_EXPIRATION_HOURS),
            Issuer = _authSettings.JWT_ISSUER,
            Audience = _authSettings.JWT_AUDIENCE,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Token generated successfully for user: {UserName}, Token length: {TokenLength}",
            userName, tokenString.Length);

        return tokenString;
    }
}
