using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormAggregate;

/// <summary>
/// Value object representing a form owner
/// </summary>
public class Owner : ValueObject
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public Owner(string name, string email)
    {
        Name = name;
        Email = email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Email;
    }

    public override string ToString() => $"{Name} <{Email}>";
}
