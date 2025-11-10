namespace FormDesignerAPI.Core.FormAggregate;

public class Version : EntityBase<Guid>
{
    public Guid VersionId { get; set; }

    // Version components: Major.Minor.Patch
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    public DateTime VersionDate { get; set; }
    public DateTime? ReleasedDate { get; set; }
    public FormStatus Status { get; set; } = FormStatus.NotSet;
    public FormDefinition? FormDefinition { get; set; }

    private Version(int major, int minor, int patch)
    {
        VersionId = Guid.NewGuid();
        Major = Guard.Against.NegativeOrZero(major, nameof(major));
        Minor = Guard.Against.NegativeOrZero(minor, nameof(minor));
        Patch = Guard.Against.NegativeOrZero(patch, nameof(patch));
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

    public Version CreateVersion(int major, int minor, int patch)
    {
        return new Version(major, minor, patch);
    }

    public Version PublishVersion(DateTime releasedDate)
    {
        ReleasedDate = releasedDate;
        Status = FormStatus.Published;
        return this;
    }
}