namespace FormDesignerAPI.UseCases.Identity.Login;

public record LoginUserCommand(string UserName, string Password) : FastEndpoints.ICommand<Ardalis.Result.Result>;

