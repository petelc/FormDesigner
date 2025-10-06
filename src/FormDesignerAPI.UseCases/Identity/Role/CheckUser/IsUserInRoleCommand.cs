namespace FormDesignerAPI.UseCases.Identity.Role.CheckUser;

public record IsUserInRoleCommand(string UserId, string Role) : FastEndpoints.ICommand<Ardalis.Result.Result<bool>>, MediatR.IRequest<Ardalis.Result.Result<bool>>;
