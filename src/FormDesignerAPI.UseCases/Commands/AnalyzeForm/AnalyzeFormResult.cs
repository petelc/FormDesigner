namespace FormDesignerAPI.UseCases.Commands.AnalyzeForm;

/// <summary>
/// Result of form analysis
/// </summary>
public class AnalyzeFormResult
{
    public Guid FormId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FormType { get; init; } = string.Empty;
    public int FieldCount { get; init; }
    public int TableCount { get; init; }
    public List<FieldSummaryDto> Fields { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool RequiresManualReview { get; init; }
    public DateTime AnalyzedAt { get; init; }
}

public class FieldSummaryDto
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public bool HasOptions { get; init; }
    public bool HasValidation { get; init; }
    public int? MaxLength { get; init; }
}