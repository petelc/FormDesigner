namespace FormDesignerAPI.UseCases.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }
}
