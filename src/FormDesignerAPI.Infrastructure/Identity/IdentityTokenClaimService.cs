using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FormDesignerAPI.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FormDesignerAPI.Infrastructure.Identity;

public class IdentityTokenClaimService : ITokenClaimService
{
    private readonly UserManager<ApplicationUser> userManager;
    public IdentityTokenClaimService(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<string> GetTokenAsync(string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // TODO: Replace with your actual key
        var key = System.Text.Encoding.ASCII.GetBytes("YourSuperSecretKeyHere");
        var user = await userManager.FindByNameAsync(userName);
        // TODO: Create user not found exception
        if (user == null) throw new UserNotFoundException(userName);
        // TODO: figure out the method parameters for this
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName) };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims.ToArray()),
            Expires = DateTime.UtcNow.AddHours(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
