using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.UseCases.Identity.Login;
using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Web.Identity;

/// <summary>
/// Login a user
/// </summary>
public class Login(SignInManager<ApplicationUser> signInManager, ITokenClaimService tokenClaimService) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/Identity/Login");
        AllowAnonymous();
        Description(d =>
            d.WithSummary("Login a user.")
            .WithDescription("Logs in a user given a username and password.")
            .WithName("LoginUser")
            .WithTags("Identity"));
    }

    public override async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = new LoginResponse(request.CorrelationId);

        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Username and password must not be null or empty.");
        }

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        var result = await signInManager.PasswordSignInAsync(request.UserName, request.Password, isPersistent: false, lockoutOnFailure: false);

        response.Result = result.Succeeded;
        response.IsLockedOut = result.IsLockedOut;
        response.IsNotAllowed = result.IsNotAllowed;
        response.RequiresTwoFactor = result.RequiresTwoFactor;
        response.UserName = request.UserName;

        if (result.Succeeded)
        {
            response.Token = await tokenClaimService.GetTokenAsync(request.UserName);
        }

        return response;

    }
}
