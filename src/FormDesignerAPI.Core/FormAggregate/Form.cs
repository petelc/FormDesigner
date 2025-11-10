namespace FormDesignerAPI.Core.FormAggregate;


/// <summary>
/// Represents a form entity within the Form Designer API.
/// This representation is base properties of a form and does not include the form's fields or layout.
/// </summary>
public class Form : EntityBase<Guid>, IAggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class with the specified form number.
    /// </summary>
    /// <param name="formNumber"></param>
    public Form(string formNumber)
    {
        UpdateFormNumber(formNumber);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class with the specified form number and title.
    /// </summary>
    /// <param name="formNumber"></param>
    /// <param name="formTitle"></param>
    public Form(string formNumber, string formTitle)
    {
        UpdateFormNumber(formNumber);
        UpdateFormTitle(formTitle);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class with the specified properties.
    /// </summary>
    /// <param name="formNumber"></param>
    /// <param name="formTitle"></param>
    /// <param name="division"></param>
    /// <param name="owner"></param>
    /// <param name="version"></param>
    /// <param name="createdDate"></param>
    /// <param name="revisedDate"></param>
    /// <param name="configurationPath"></param>
    public Form(string formNumber, string formTitle, string division, Owner owner, Version? version = null, DateTime? createdDate = null, DateTime? revisedDate = null, string? configurationPath = null)
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

    public Guid FormId { get; private set; } = Guid.NewGuid();
    public string FormNumber { get; private set; } = default!;
    public string FormTitle { get; private set; } = default!;
    public string? Division { get; private set; } = default!;
    public Owner? Owner { get; private set; } = default!; // Value Object representing the owner of the form
    public Version? Version { get; private set; } = default!;
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

    public Form UpdateVersion(Version version)
    {
        Version = Guard.Against.Null(version, nameof(version));
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

    public Form UpdateDetails(string newFormNumber, string newFormTitle, string newDivision, string newOwner, Version newVersion, DateTime newRevisionDate, string newConfigurationPath)
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
