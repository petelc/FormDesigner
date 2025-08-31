using System;
using System.Buffers;

namespace FormDesignerAPI.Core.FormAggregate;

public class Form : EntityBase, IAggregateRoot
{
    public Form(string formNumber)
    {
        UpdateFormNumber(formNumber);
        Status = FormStatus.NotSet;
    }

    public string FormNumber { get; private set; } = default!;
    public string FormTitle { get; private set; } = default!;
    public string Division { get; private set; } = default!;
    public Owner Owner { get; private set; } = default!;
    public string Version { get; private set; } = default!;
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime RevisedDate { get; private set; } = DateTime.UtcNow;
    public string? ConfigurationPath { get; set; }

    public FormStatus Status { get; private set; } = FormStatus.NotSet;

    public Form UpdateFormNumber(string newFormNumber)
    {
        FormNumber = Guard.Against.NullOrEmpty(newFormNumber, nameof(newFormNumber));
        return this;
    }

    public Form SetTitle(string title)
    {
        FormTitle = Guard.Against.NullOrEmpty(title, nameof(title));
        return this;
    }

    public Form SetOwner(string name, string email)
    {
        Owner = new Owner(name, email);
        return this;
    }

}

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
