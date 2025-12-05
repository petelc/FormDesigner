using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

public class FormRevision : EntityBase<Guid>
{
    public Guid VersionId { get; set; }

    // Version components: Major.Minor.Patch
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    public DateTime VersionDate { get; set; }
    public DateTime? ReleasedDate { get; set; }
    public FormStatus Status { get; internal set; } = FormStatus.NotSet; // internal set allows Form aggregate to modify it

    // Each version has exactly one form definition
    public FormDefinition FormDefinition { get; private set; } = default!;

    private FormRevision(int major, int minor, int patch, FormDefinition formDefinition)
    {
        VersionId = Guid.NewGuid();
        Major = Guard.Against.NegativeOrZero(major, nameof(major));
        Minor = Guard.Against.NegativeOrZero(minor, nameof(minor));
        Patch = Guard.Against.NegativeOrZero(patch, nameof(patch));
        FormDefinition = Guard.Against.Null(formDefinition, nameof(formDefinition));
        VersionDate = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    public void UpdateVersion(int major, int minor, int patch, FormDefinition formDefinition)
    {
        Major = Guard.Against.NegativeOrZero(major, nameof(major));
        Minor = Guard.Against.NegativeOrZero(minor, nameof(minor));
        Patch = Guard.Against.NegativeOrZero(patch, nameof(patch));
        FormDefinition = formDefinition;
    }

    public FormRevision CreateVersion(int major, int minor, int patch, FormDefinition formDefinition)
    {
        return new FormRevision(major, minor, patch, formDefinition);
    }

    public static FormRevision Create(int major, int minor, int patch, FormDefinition formDefinition)
    {
        return new FormRevision(major, minor, patch, formDefinition);
    }

    public FormRevision PublishVersion(DateTime releasedDate)
    {
        ReleasedDate = releasedDate;
        Status = FormStatus.Published;
        return this;
    }

    /// <summary>
    /// Archives this version
    /// </summary>
    /// <returns>The version instance for method chaining</returns>
    public FormRevision Archive()
    {
        Status = FormStatus.Archived;
        return this;
    }

    /// <summary>
    /// Sets this version back to draft status
    /// </summary>
    /// <returns>The version instance for method chaining</returns>
    public FormRevision SetToDraft()
    {
        Status = FormStatus.Draft;
        ReleasedDate = null; // Clear release date when reverting to draft
        return this;
    }

    /// <summary>
    /// Checks if this version is currently published
    /// </summary>
    public bool IsPublished => Status == FormStatus.Published;

    /// <summary>
    /// Checks if this version is archived
    /// </summary>
    public bool IsArchived => Status == FormStatus.Archived;

    /// <summary>
    /// Checks if this version is in draft status
    /// </summary>
    public bool IsDraft => Status == FormStatus.Draft;
}