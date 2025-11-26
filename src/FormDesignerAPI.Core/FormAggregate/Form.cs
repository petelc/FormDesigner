namespace FormDesignerAPI.Core.FormAggregate;


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
    // public Revision? CurrentRevision { get; private set; }  // Not tracked by EF Core to avoid circular dependency
    public Guid? CurrentRevisionId { get; private set; }

    // Collection of revisions - each form can have multiple revisions
    private readonly List<Revision> _revisions = new();
    public IReadOnlyCollection<Revision> Revisions => _revisions.AsReadOnly();

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

    public Form AddRevision(Revision revision)
    {
        Guard.Against.Null(revision, nameof(revision));

        // Set revision status to Draft by default if not already set
        if (revision.Status == FormStatus.NotSet)
        {
            revision.Status = FormStatus.Draft;
        }

        _revisions.Add(revision);

        // Update form status if this is the first revision
        if (_revisions.Count == 1 && Status == FormStatus.NotSet)
        {
            Status = FormStatus.Draft;
            // Set CurrentRevisionId to the first revision added
            CurrentRevisionId = revision.RevisionId;
        }

        return this;
    }

    public Revision? GetCurrentRevision()
    {
        return _revisions.OrderByDescending(r => r.RevisionDate).FirstOrDefault();
    }

    public Revision? GetPublishedRevision()
    {
        return _revisions.FirstOrDefault(r => r.Status == FormStatus.Published);
    }

    /// <summary>
    /// Publishes a revision, ensuring only one revision can be published at a time.
    /// If another revision is currently published, it will be archived.
    /// </summary>
    /// <param name="revisionToPublish">The revision to publish</param>
    /// <param name="publishDate">The date to publish the revision (optional, defaults to UtcNow)</param>
    /// <returns>The form instance for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when the revision doesn't belong to this form</exception>
    public Form PublishRevision(Revision revisionToPublish, DateTime? publishDate = null)
    {
        Guard.Against.Null(revisionToPublish, nameof(revisionToPublish));

        // Ensure the revision belongs to this form
        if (!_revisions.Contains(revisionToPublish))
        {
            throw new InvalidOperationException("Cannot publish a revision that doesn't belong to this form.");
        }

        var releaseDate = publishDate ?? DateTime.UtcNow;

        // Archive the currently published revision (if any)
        var currentlyPublished = GetPublishedRevision();
        if (currentlyPublished != null && currentlyPublished != revisionToPublish)
        {
            currentlyPublished.Status = FormStatus.Archived;
        }

        // Publish the new revision
        revisionToPublish.PublishRevision(releaseDate);

        // Update the form's overall status to Published
        Status = FormStatus.Published;
        SetRevisedDate(releaseDate);

        // Set this revision as the current revision
        CurrentRevisionId = revisionToPublish.RevisionId; return this;
    }

    /// <summary>
    /// Archives the currently published revision, setting the form status to Draft
    /// </summary>
    /// <returns>The form instance for method chaining</returns>
    public Form ArchivePublishedRevision()
    {
        var publishedRevision = GetPublishedRevision();
        if (publishedRevision != null)
        {
            publishedRevision.Status = FormStatus.Archived;

            // If no other revisions are published, set form status to Draft
            if (!_revisions.Any(r => r.Status == FormStatus.Published))
            {
                Status = FormStatus.Draft;
            }
        }

        return this;
    }

    /// <summary>
    /// Gets all revisions with a specific status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>Collection of revisions with the specified status</returns>
    public IEnumerable<Revision> GetRevisionsByStatus(FormStatus status)
    {
        return _revisions.Where(r => r.Status == status);
    }

    /// <summary>
    /// Checks if the form can have a new revision published
    /// </summary>
    /// <returns>True if a new revision can be published</returns>
    public bool CanPublishRevision()
    {
        // You can add business rules here, e.g.:
        // - Must have at least one revision
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

    public Form UpdateDetails(string newFormNumber, string newFormTitle, string newDivision, string newOwnerName, string newOwnerEmail, Revision newRevision, DateTime newRevisionDate)
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
