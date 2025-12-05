namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// Value object representing the owner of a form
/// </summary>
public class Owner(string name, string email) : ValueObject
{
    public string Name { get; private set; } = name;
    public string Email { get; private set; } = email;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Email;
    }
}