using FormDesignerAPI.Core.Entities;

namespace FormDesignerAPI.UseCases.Identity.GetAllUsers;

public record class GetAllUsersCommand : FastEndpoints.ICommand<Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>>, MediatR.IRequest<Ardalis.Result.Result<System.Collections.Generic.List<UserDto>>>;
