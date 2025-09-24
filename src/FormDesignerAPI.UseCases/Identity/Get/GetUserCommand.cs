namespace FormDesignerAPI.UseCases.Identity.Get;

public record GetUserCommand(string UserId) : FastEndpoints.ICommand<Ardalis.Result.Result<string?>>;

