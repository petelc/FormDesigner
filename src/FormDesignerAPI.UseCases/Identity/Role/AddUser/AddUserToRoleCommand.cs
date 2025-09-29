namespace FormDesignerAPI.UseCases.Identity.Role.AddUser;

public record AddUserToRoleCommand
    (string UserId, string RoleName) : FastEndpoints.ICommand<Ardalis.Result.Result>, MediatR.IRequest<Ardalis.Result.Result>;
