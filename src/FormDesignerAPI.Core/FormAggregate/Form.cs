using Traxs.SharedKernel;
using Ardalis.GuardClauses;

namespace FormDesignerAPI.Core.FormAggregate;

/// <summary>
/// Form aggregate root - represents a form configuration
/// </summary>
public class Form : EntityBase<int>, IAggregateRoot
{
    public string FormNumber { get; private set; } = string.Empty;
    public string FormTitle { get; private set; } = string.Empty;
    public string? Division { get; private set; }
    public Owner Owner { get; private set; } = null!;
    public string? Version { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime RevisedDate { get; private set; }
    public string? ConfigurationPath { get; private set; }
    public FormStatus Status { get; private set; }

    // Private constructor for EF Core
    private Form() { }

    /// <summary>
    /// Factory method to create a new form
    /// </summary>
    public static Form Create(
        string formNumber,
        string formTitle,
        string? division,
        Owner owner,
        string? version,
        DateTime? createdDate,
        DateTime? revisedDate,
        string? configurationPath)
    {
        Guard.Against.NullOrWhiteSpace(formNumber, nameof(formNumber));
        Guard.Against.NullOrWhiteSpace(formTitle, nameof(formTitle));
        Guard.Against.Null(owner, nameof(owner));

        var form = new Form
        {
            FormNumber = formNumber,
            FormTitle = formTitle,
            Division = division,
            Owner = owner,
            Version = version,
            CreatedDate = createdDate ?? DateTime.UtcNow,
            RevisedDate = revisedDate ?? DateTime.UtcNow,
            ConfigurationPath = configurationPath,
            Status = FormStatus.Draft
        };

        return form;
    }

    /// <summary>
    /// Update form details
    /// </summary>
    public void Update(
        string formNumber,
        string formTitle,
        string? division,
        string? owner,
        string? version,
        DateTime revisedDate,
        string? configurationPath)
    {
        Guard.Against.NullOrWhiteSpace(formNumber, nameof(formNumber));
        Guard.Against.NullOrWhiteSpace(formTitle, nameof(formTitle));

        FormNumber = formNumber;
        FormTitle = formTitle;
        Division = division;
        Version = version;
        RevisedDate = revisedDate;
        ConfigurationPath = configurationPath;
    }

    /// <summary>
    /// Activate the form
    /// </summary>
    public void Activate()
    {
        Status = FormStatus.Active;
    }

    /// <summary>
    /// Archive the form
    /// </summary>
    public void Archive()
    {
        Status = FormStatus.Archived;
    }

    /// <summary>
    /// Mark form as deprecated
    /// </summary>
    public void Deprecate()
    {
        Status = FormStatus.Deprecated;
    }
}
