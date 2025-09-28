namespace FormDesignerAPI.UseCases.Identity.Delete;

public record class DeleteUserCommand(string UserId) : FastEndpoints.ICommand<Ardalis.Result.Result>, MediatR.IRequest<Ardalis.Result.Result>;
