using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Events;

namespace FormDesignerAPI.Core.FormContext.Aggregates;

/// <summary>
/// Form aggregate root - manages form definitions and their revisions
/// </summary>
public class Form : EntityBase<Guid>, IAggregateRoot
{
    // Properties
    public string Name { get; private set; } = string.Empty;
    public FormDefinition Definition { get; private set; } = null!;
    public OriginMetadata Origin { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private readonly List<FormRevision> _revisions = new();
    public IReadOnlyCollection<FormRevision> Revisions => _revisions.AsReadOnly();

    /// <summary>
    /// Get the most recent revision
    /// </summary>
    public FormRevision CurrentRevision =>
      _revisions.OrderByDescending(r => r.Version).First();

    /// <summary>
    /// Get the current version number
    /// </summary>
    public int CurrentVersion =>
      _revisions.Any() ? _revisions.Max(r => r.Version) : 0;

    /// <summary>
    /// Total number of fields in current definition
    /// </summary>
    public int FieldCount => Definition.Fields.Count;

    // Private constructor for EF Core
    private Form() { }

    /// <summary>
    /// Factory method to create a new form
    /// </summary>
    public static Form Create(
      string name,
      FormDefinition definition,
      OriginMetadata origin,
      string createdBy)
    {
        // Guard clauses
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(definition, nameof(definition));
        Guard.Against.Null(origin, nameof(origin));
        Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));

        // Validate definition has fields
        if (definition.Fields.Count == 0)
            throw new ArgumentException("Form must have at least one field", nameof(definition));

        var form = new Form
        {
            Id = Guid.NewGuid(),
            Name = name,
            Definition = definition,
            Origin = origin,
            IsActive = true
        };

        // Create initial revision (version 1)
        var initialRevision = FormRevision.Create(
          form.Id,
          definition,
          "Initial version",
          createdBy,
          1
        );

        form._revisions.Add(initialRevision);

        // Raise domain event
        form.RegisterDomainEvent(new FormCreatedEvent(
          form.Id,
          form.Name,
          origin,
          createdBy
        ));

        return form;
    }

    /// <summary>
    /// Create a new revision of this form
    /// </summary>
    public void CreateRevision(
      FormDefinition newDefinition,
      string notes,
      string createdBy)
    {
        Guard.Against.Null(newDefinition, nameof(newDefinition));
        Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));

        // Validate form is active
        if (!IsActive)
            throw new InvalidOperationException("Cannot create revision for inactive form");

        // Validate definition has fields
        if (newDefinition.Fields.Count == 0)
            throw new ArgumentException("Form revision must have at least one field");

        var nextVersion = CurrentVersion + 1;

        var revision = FormRevision.Create(
          Id,
          newDefinition,
          notes,
          createdBy,
          nextVersion
        );

        _revisions.Add(revision);
        Definition = newDefinition;

        RegisterDomainEvent(new FormRevisionCreatedEvent(
          Id,
          revision.Id,
          revision.Version,
          createdBy
        ));
    }

    /// <summary>
    /// Update the form name
    /// </summary>
    public void Rename(string newName, string renamedBy)
    {
        Guard.Against.NullOrWhiteSpace(newName, nameof(newName));
        Guard.Against.NullOrWhiteSpace(renamedBy, nameof(renamedBy));

        if (newName == Name)
            return; // No change

        var oldName = Name;
        Name = newName;

        RegisterDomainEvent(new FormRenamedEvent(
          Id,
          oldName,
          newName,
          renamedBy
        ));
    }

    /// <summary>
    /// Deactivate the form
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate the form
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Get a specific revision by version number
    /// </summary>
    public FormRevision? GetRevision(int version)
    {
        return _revisions.FirstOrDefault(r => r.Version == version);
    }

    /// <summary>
    /// Check if a field exists in the current definition
    /// </summary>
    public bool HasField(string fieldName)
    {
        return Definition.HasField(fieldName);
    }
}