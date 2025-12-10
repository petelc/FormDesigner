using Traxs.SharedKernel;

namespace FormDesignerAPI.Core.FormContext.ValueObjects;

/// <summary>
/// Tracks how and when a form was created
/// Immutable value object
/// </summary>
public class OriginMetadata : ValueObject
{
    public OriginType Type { get; init; }
    public string? ReferenceId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;

    // Private constructor - use factory methods
    private OriginMetadata() { }

    /// <summary>
    /// Create origin metadata for a manually created form
    /// </summary>
    public static OriginMetadata Manual(string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        return new OriginMetadata
        {
            Type = OriginType.Manual,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Create origin metadata for an imported form
    /// </summary>
    public static OriginMetadata Import(string candidateId, string approvedBy)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
            throw new ArgumentException("CandidateId cannot be empty", nameof(candidateId));
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("ApprovedBy cannot be empty", nameof(approvedBy));

        return new OriginMetadata
        {
            Type = OriginType.Import,
            ReferenceId = candidateId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = approvedBy
        };
    }

    /// <summary>
    /// Create origin metadata for an API-created form
    /// </summary>
    public static OriginMetadata Api(string createdBy, string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        return new OriginMetadata
        {
            Type = OriginType.API,
            ReferenceId = externalId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Create origin metadata for a template-based form
    /// </summary>
    public static OriginMetadata Template(string templateId, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("TemplateId cannot be empty", nameof(templateId));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        return new OriginMetadata
        {
            Type = OriginType.Template,
            ReferenceId = templateId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return ReferenceId ?? string.Empty;
        yield return CreatedAt;
        yield return CreatedBy;
    }
}

/// <summary>
/// Represents a single field in a form definition
/// Using record for immutability
/// </summary>
public record FormField
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string? Label { get; init; }
    public string? Placeholder { get; init; }
    public string? DefaultValue { get; init; }
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public string? Pattern { get; init; }
    public Dictionary<string, object>? ValidationRules { get; init; }
    public List<string>? Options { get; init; } // For select/radio fields

    /// <summary>
    /// Validate that the field definition is complete
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (string.IsNullOrWhiteSpace(Type)) return false;

        // If it's a select/radio, must have options
        if ((Type == "select" || Type == "radio") &&
            (Options == null || Options.Count == 0))
            return false;

        return true;
    }
}