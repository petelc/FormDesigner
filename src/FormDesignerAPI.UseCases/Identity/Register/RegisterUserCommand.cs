using Ardalis.SharedKernel;
using Ardalis.Result;

namespace FormDesignerAPI.UseCases.Identity.Register;

public record RegisterUserCommand(string UserName, string Password) : ICommand<Result<string>>;