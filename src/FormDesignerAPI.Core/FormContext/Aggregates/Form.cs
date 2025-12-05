namespace FormDesignerAPI.Core.FormContext.Aggregates;


/// <summary>
/// Represents a form entity within the Form Designer API.
/// This representation is base properties of a form domain model. Each form can have multiple versions and each version can have its own form definition.
/// </summary>
public class Form : EntityBase<Guid>, IAggregateRoot
{
    /// <summary>
    /// Private constructor - use Form.CreateBuilder() to create new instances
    /// </summary>
    internal Form(string formNumber)
    {
        FormId = Guid.NewGuid();
        FormNumber = Guard.Against.NullOrEmpty(formNumber, nameof(formNumber));
        Status = FormStatus.NotSet;
    }

    /// <summary>
    /// Creates a new Form builder for fluent construction
    /// </summary>
    /// <param name="formNumber">The required form number</param>
    /// <returns>A FormBuilder instance</returns>
    public static FormBuilder CreateBuilder(string formNumber)
    {
        return new FormBuilder(formNumber);
    }

    public Guid FormId { get; private set; } = Guid.NewGuid();
    public string FormNumber { get; private set; } = default!;
    public string FormTitle { get; private set; } = default!;
    public string? Division { get; private set; } = default!;
    public Owner? Owner { get; private set; } = default!; // Value Object representing the owner of the form
    public DateTime? CreatedDate { get; private set; }
    public DateTime? RevisedDate { get; private set; }

    // Collection of versions - each form can have multiple versions
    private readonly List<FormRevision> _revisions = new();
    public IReadOnlyCollection<FormRevision> Revisions => _revisions.AsReadOnly();

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

    public Form AddRevision(FormRevision revision)
    {
        Guard.Against.Null(revision, nameof(revision));

        // Set version status to Draft by default if not already set
        if (revision.Status == FormStatus.NotSet)
        {
            revision.Status = FormStatus.Draft;
        }

        _revisions.Add(revision);

        // Update form status if this is the first version
        if (_revisions.Count == 1 && Status == FormStatus.NotSet)
        {
            Status = FormStatus.Draft;
        }

        return this;
    }

    public FormRevision? GetCurrentRevision()
    {
        return _revisions.OrderByDescending(v => v.VersionDate).FirstOrDefault();
    }

    public FormRevision? GetPublishedRevision()
    {
        return _revisions.FirstOrDefault(v => v.Status == FormStatus.Published);
    }

    /// <summary>
    /// Publishes a version, ensuring only one version can be published at a time.
    /// If another version is currently published, it will be archived.
    /// </summary>
    /// <param name="versionToPublish">The version to publish</param>
    /// <param name="publishDate">The date to publish the version (optional, defaults to UtcNow)</param>
    /// <returns>The form instance for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when the version doesn't belong to this form</exception>
    public Form PublishVersion(FormRevision versionToPublish, DateTime? publishDate = null)
    {
        Guard.Against.Null(versionToPublish, nameof(versionToPublish));

        // Ensure the version belongs to this form
        if (!_revisions.Contains(versionToPublish))
        {
            throw new InvalidOperationException("Cannot publish a version that doesn't belong to this form.");
        }

        var releaseDate = publishDate ?? DateTime.UtcNow;

        // Archive the currently published version (if any)
        var currentlyPublished = GetPublishedRevision();
        if (currentlyPublished != null && currentlyPublished != versionToPublish)
        {
            currentlyPublished.Status = FormStatus.Archived;
        }

        // Publish the new version
        versionToPublish.PublishVersion(releaseDate);

        // Update the form's overall status to Published
        Status = FormStatus.Published;
        SetRevisedDate(releaseDate);

        return this;
    }

    /// <summary>
    /// Archives the currently published version, setting the form status to Draft
    /// </summary>
    /// <returns>The form instance for method chaining</returns>
    public Form ArchivePublishedVersion()
    {
        var publishedRevision = GetPublishedRevision();
        if (publishedRevision != null)
        {
            publishedRevision.Status = FormStatus.Archived;

            // If no other revisions are published, set form status to Draft
            if (!_revisions.Any(v => v.Status == FormStatus.Published))
            {
                Status = FormStatus.Draft;
            }
        }

        return this;
    }

    /// <summary>
    /// Gets all versions with a specific status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>Collection of versions with the specified status</returns>
    public IEnumerable<FormRevision> GetRevisionsByStatus(FormStatus status)
    {
        return _revisions.Where(v => v.Status == status);
    }

    /// <summary>
    /// Checks if the form can have a new version published
    /// </summary>
    /// <returns>True if a new version can be published</returns>
    public bool CanPublishRevision()
    {
        // You can add business rules here, e.g.:
        // - Must have at least one version
        // - Form must not be archived
        // - etc.
        return _revisions.Any() && Status != FormStatus.Archived;
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

    public Form UpdateDetails(string newFormNumber, string newFormTitle, string newDivision, string newOwnerName, string newOwnerEmail, FormRevision newRevision, DateTime newRevisionDate)
    {
        UpdateFormNumber(newFormNumber);
        UpdateFormTitle(newFormTitle);
        UpdateDivision(newDivision);
        SetOwner(newOwnerName, newOwnerEmail);
        AddRevision(newRevision);
        SetRevisedDate(newRevisionDate);
        return this;
    }

}
