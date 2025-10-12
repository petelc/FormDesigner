namespace FormDesignerAPI.UseCases.Identity.Profile;

public record UserProfileCommand(string UserId, string FirstName, string LastName, string Division, string JobTitle, string Supervisor, string? ProfileImageUrl) : FastEndpoints.ICommand<Ardalis.Result.Result>, MediatR.IRequest<Ardalis.Result.Result>;

