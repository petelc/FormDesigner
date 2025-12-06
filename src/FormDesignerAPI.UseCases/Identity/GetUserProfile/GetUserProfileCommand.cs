using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.UseCases.Identity.GetUserProfile;

public record class GetUserProfileCommand(string UserId) : FastEndpoints.ICommand<Ardalis.Result.Result<UserDto?>>, MediatR.IRequest<Ardalis.Result.Result<UserDto?>>;
