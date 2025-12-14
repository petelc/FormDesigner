namespace FormDesignerAPI.UseCases.FormContext;

/// <summary>
/// DTO for FormContext.Form
/// </summary>
public class FormContextDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FormDefinitionDTO Definition { get; set; } = null!;
    public OriginMetadataDTO Origin { get; set; } = null!;
    public bool IsActive { get; set; }
    public int CurrentVersion { get; set; }
    public int FieldCount { get; set; }
}

/// <summary>
/// DTO for FormDefinition value object
/// </summary>
public class FormDefinitionDTO
{
    public string Schema { get; set; } = string.Empty;
    public List<FormFieldDTO> Fields { get; set; } = new();
}

/// <summary>
/// DTO for FormField
/// </summary>
public class FormFieldDTO
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public List<string>? Options { get; set; }
}

/// <summary>
/// DTO for OriginMetadata
/// </summary>
public class OriginMetadataDTO
{
    public string Type { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for FormRevision
/// </summary>
public class FormRevisionDTO
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    public FormDefinitionDTO Definition { get; set; } = null!;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
