namespace FormDesignerAPI.UseCases.Identity.Role.GetUserRoles;

public record class GetUserRolesCommand(string UserId) : FastEndpoints.ICommand<Ardalis.Result.Result<List<string>>>, MediatR.IRequest<Ardalis.Result.Result<List<string>>>;

