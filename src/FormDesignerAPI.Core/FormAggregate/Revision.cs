namespace FormDesignerAPI.Core.FormAggregate;

public class Revision : EntityBase<Guid>
{
    public Guid RevisionId { get; set; }

    // Revision components: Major.Minor.Patch
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    public DateTime RevisionDate { get; set; }
    public DateTime? ReleasedDate { get; set; }
    public FormStatus Status { get; internal set; } = FormStatus.NotSet; // internal set allows Form aggregate to modify it

    // Each revision has exactly one form definition
    public FormDefinition FormDefinition { get; private set; } = default!;

    public Guid FormId { get; set; }

    // Constructor for EF Core
    protected Revision()
    {
    }

    private Revision(int major, int minor, int patch, FormDefinition formDefinition)
    {
        RevisionId = Guid.NewGuid();
        Major = Guard.Against.NegativeOrZero(major, nameof(major));
        Minor = Guard.Against.NegativeOrZero(minor, nameof(minor));
        Patch = Guard.Against.NegativeOrZero(patch, nameof(patch));
        FormDefinition = Guard.Against.Null(formDefinition, nameof(formDefinition));
        RevisionDate = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    public void UpdateRevision(int major, int minor, int patch, FormDefinition formDefinition)
    {
        Major = Guard.Against.NegativeOrZero(major, nameof(major));
        Minor = Guard.Against.NegativeOrZero(minor, nameof(minor));
        Patch = Guard.Against.NegativeOrZero(patch, nameof(patch));
        FormDefinition = formDefinition;
    }

    public Revision CreateRevision(int major, int minor, int patch, FormDefinition formDefinition)
    {
        return new Revision(major, minor, patch, formDefinition);
    }

    public static Revision Create(int major, int minor, int patch, FormDefinition formDefinition)
    {
        return new Revision(major, minor, patch, formDefinition);
    }

    public Revision PublishRevision(DateTime releasedDate)
    {
        ReleasedDate = releasedDate;
        Status = FormStatus.Published;
        return this;
    }

    /// <summary>
    /// Archives this revision
    /// </summary>
    /// <returns>The revision instance for method chaining</returns>
    public Revision Archive()
    {
        Status = FormStatus.Archived;
        return this;
    }

    /// <summary>
    /// Sets this revision back to draft status
    /// </summary>
    /// <returns>The revision instance for method chaining</returns>
    public Revision SetToDraft()
    {
        Status = FormStatus.Draft;
        ReleasedDate = null; // Clear release date when reverting to draft
        return this;
    }

    /// <summary>
    /// Checks if this revision is currently published
    /// </summary>
    public bool IsPublished => Status == FormStatus.Published;

    /// <summary>
    /// Checks if this revision is archived
    /// </summary>
    public bool IsArchived => Status == FormStatus.Archived;

    /// <summary>
    /// Checks if this revision is in draft status
    /// </summary>
    public bool IsDraft => Status == FormStatus.Draft;
}
