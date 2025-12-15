namespace FormDesignerAPI.UseCases.Interfaces;

/// <summary>
/// Infrastructure service for extracting form structure
/// </summary>
public interface IFormExtractor
{
    Task<string> DetectFormTypeAsync(
        string pdfPath,
        CancellationToken cancellationToken = default);

    Task<ExtractedFormStructure> ExtractFormStructureAsync(
        string pdfPath,
        string formType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Extracted form structure (infrastructure DTO)
/// </summary>
public class ExtractedFormStructure
{
    public List<ExtractedField> Fields { get; init; } = new();
    public List<ExtractedTable> Tables { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}

public class ExtractedField
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public int? MaxLength { get; init; }
    public string? DefaultValue { get; init; }
    public List<string>? Options { get; init; }
    public string? ValidationPattern { get; init; }
}

public class ExtractedTable
{
    public int RowCount { get; init; }
    public int ColumnCount { get; init; }
    public List<ExtractedCell> Cells { get; init; } = new();
}

public class ExtractedCell
{
    public string Content { get; init; } = string.Empty;
    public int RowIndex { get; init; }
    public int ColumnIndex { get; init; }
    public bool IsHeader { get; init; }
}