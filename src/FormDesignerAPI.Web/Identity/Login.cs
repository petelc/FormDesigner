using FormDesignerAPI.Core.Interfaces;
using FormDesignerAPI.Infrastructure.Identity;
using FormDesignerAPI.UseCases.Identity.Login;
using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Web.Identity;

/// <summary>
/// Login a user
/// </summary>
public class Login(SignInManager<ApplicationUser> signInManager, ITokenClaimService tokenClaimService, ILogger<Login> logger) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/Identity/login");
        AllowAnonymous();
        Description(d =>
            d.WithSummary("Login a user.")
            .WithDescription("Logs in a user given a username and password.")
            .WithName("LoginUser")
            .WithTags("Identity"));
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = new LoginResponse(request.CorrelationId);

        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
        {
            AddError("Username and password must not be null or empty.");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        logger.LogInformation("Attempting login for user: {UserName}", request.UserName);
        var result = await signInManager.PasswordSignInAsync(request.UserName, request.Password, isPersistent: false, lockoutOnFailure: false);

        logger.LogInformation("Login attempt result for {UserName}: Succeeded={Succeeded}, IsLockedOut={IsLockedOut}, IsNotAllowed={IsNotAllowed}, RequiresTwoFactor={RequiresTwoFactor}",
            request.UserName, result.Succeeded, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);

        response.Result = result.Succeeded;
        response.IsLockedOut = result.IsLockedOut;
        response.IsNotAllowed = result.IsNotAllowed;
        response.RequiresTwoFactor = result.RequiresTwoFactor;
        response.UserName = request.UserName;

        if (result.Succeeded)
        {
            try
            {
                logger.LogInformation("Generating token for user: {UserName}", request.UserName);
                response.Token = await tokenClaimService.GetTokenAsync(request.UserName);
                logger.LogInformation("Token generated successfully for user: {UserName}. Token length: {TokenLength}",
                    request.UserName, response.Token?.Length ?? 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate token for user: {UserName}", request.UserName);
                AddError($"Failed to generate authentication token: {ex.Message}");
                await SendErrorsAsync(cancellation: cancellationToken);
                return;
            }
        }

        await SendAsync(response, cancellation: cancellationToken);
    }
}
