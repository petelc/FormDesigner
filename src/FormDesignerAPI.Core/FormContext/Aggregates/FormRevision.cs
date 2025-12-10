using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// FormRevision entity - represents a version of a form
/// NOT an aggregate root - managed by Form aggregate
/// </summary>
public class FormRevision : EntityBase<Guid>
{
    public Guid FormId { get; private set; }
    public int Version { get; private set; }
    public FormDefinition Definition { get; private set; } = null!;
    public string Notes { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Private constructor for EF Core
    private FormRevision() { }

    /// <summary>
    /// Factory method to create a new revision
    /// Internal - only Form aggregate can create revisions
    /// </summary>
    internal static FormRevision Create(
      Guid formId,
      FormDefinition definition,
      string notes,
      string createdBy,
      int version)
    {
        Guard.Against.Default(formId, nameof(formId));
        Guard.Against.Null(definition, nameof(definition));
        Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
        Guard.Against.NegativeOrZero(version, nameof(version));

        return new FormRevision
        {
            Id = Guid.NewGuid(),
            FormId = formId,
            Version = version,
            Definition = definition,
            Notes = notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Check if this revision has breaking changes from another
    /// </summary>
    public bool HasBreakingChangesFrom(FormRevision other)
    {
        // Field removed
        var thisFieldNames = Definition.Fields.Select(f => f.Name).ToHashSet();
        var otherFieldNames = other.Definition.Fields.Select(f => f.Name).ToHashSet();

        if (otherFieldNames.Except(thisFieldNames).Any())
            return true;

        // Required field added
        var newRequiredFields = Definition.Fields
          .Where(f => f.Required)
          .Select(f => f.Name)
          .Except(other.Definition.Fields.Where(f => f.Required).Select(f => f.Name));

        if (newRequiredFields.Any())
            return true;

        // Field type changed
        foreach (var fieldName in thisFieldNames.Intersect(otherFieldNames))
        {
            var thisField = Definition.GetField(fieldName);
            var otherField = other.Definition.GetField(fieldName);

            if (thisField?.Type != otherField?.Type)
                return true;
        }

        return false;
    }
}