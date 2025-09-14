namespace FormDesignerAPI.Core.FormAggregate;

public class Form : EntityBase, IAggregateRoot
{
    // TODO: Add value objects for FormNumber, FormTitle, Division, Version, ConfigurationPath
    // ? How does a value object encapsulate its data and enforce invariants?
    public Form(string formNumber)
    {
        UpdateFormNumber(formNumber);
    }

    public Form(string formNumber, string formTitle)
    {
        UpdateFormNumber(formNumber);
        UpdateFormTitle(formTitle);
    }

    public Form(string formNumber, string formTitle, string division, Owner owner, string? version = null, DateTime? createdDate = null, DateTime? revisedDate = null, string? configurationPath = null)
    {
        UpdateFormNumber(formNumber);
        UpdateFormTitle(formTitle);
        UpdateDivision(division);
        SetOwner(owner.Name, owner.Email);
        if (version is not null)
        {
            UpdateVersion(version);
        }

        if (createdDate is not null)
        {
            SetCreatedDate(createdDate.Value);
        }

        if (revisedDate is not null)
        {
            SetRevisedDate(revisedDate.Value);
        }

        if (configurationPath is not null)
        {
            SetConfigurationPath(configurationPath);
        }
    }

    public string FormNumber { get; private set; } = default!;
    public string FormTitle { get; private set; } = default!;
    public string? Division { get; private set; } = default!;
    public Owner? Owner { get; private set; } = default!;
    public string? Version { get; private set; } = default!;
    public DateTime? CreatedDate { get; private set; }
    public DateTime? RevisedDate { get; private set; }
    public string? ConfigurationPath { get; set; }

    public FormStatus Status { get; private set; } = FormStatus.NotSet;

    public Form UpdateFormNumber(string newFormNumber)
    {
        FormNumber = Guard.Against.NullOrEmpty(newFormNumber, nameof(newFormNumber));
        return this;
    }

    public Form UpdateFormTitle(string title)
    {
        FormTitle = Guard.Against.NullOrEmpty(title, nameof(title));
        return this;
    }

    public Form UpdateDivision(string division)
    {
        Division = Guard.Against.NullOrEmpty(division, nameof(division));
        return this;
    }

    public Form UpdateVersion(string version)
    {
        Version = Guard.Against.NullOrEmpty(version, nameof(version));
        return this;
    }

    public Form SetOwner(string name, string email)
    {
        Owner = new Owner(name, email);
        return this;
    }

    public Form SetCreatedDate(DateTime createdDate)
    {
        CreatedDate = createdDate;
        return this;
    }

    public Form SetRevisedDate(DateTime revisedDate)
    {
        RevisedDate = revisedDate;
        return this;
    }

    public Form SetConfigurationPath(string? configurationPath)
    {
        ConfigurationPath = configurationPath;
        return this;
    }

    public Form UpdateDetails(string newFormNumber, string newFormTitle, string newDivision, string newOwner, string newVersion, DateTime newRevisionDate, string newConfigurationPath)
    {
        UpdateFormNumber(newFormNumber);
        UpdateFormTitle(newFormTitle);
        UpdateDivision(newDivision);
        SetOwner(newOwner, string.Empty); // Email is not provided in this context
        UpdateVersion(newVersion);
        SetConfigurationPath(newConfigurationPath);
        //SetCreatedDate(DateTime.UtcNow);
        SetRevisedDate(newRevisionDate);
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
