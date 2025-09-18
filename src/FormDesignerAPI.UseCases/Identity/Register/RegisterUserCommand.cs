using Ardalis.SharedKernel;

namespace FormDesignerAPI.UseCases.Identity.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
/// <param name="UserName">The username for the new user.</param>
/// <param name="Password">The password for the new user.</param>
public record RegisterUserCommand(string UserName, string Password) : FastEndpoints.ICommand<Ardalis.Result.Result<string>>;