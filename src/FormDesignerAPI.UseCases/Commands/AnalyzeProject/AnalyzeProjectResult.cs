using FormDesignerAPI.UseCases.Commands.AnalyzeForm;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeProject;

/// <summary>
/// Result of analyzing a project
/// </summary>
public class AnalyzeProjectResult
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int FormsToAnalyze { get; init; }
    public bool AnalysisStarted { get; init; }
    public List<string> Messages { get; init; } = new();
    public List<FormAnalysisDto> Forms { get; init; } = new();
}

/// <summary>
/// Form analysis details within a project
/// </summary>
public class FormAnalysisDto
{
    public Guid FormId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FormType { get; init; } = string.Empty;
    public int FieldCount { get; init; }
    public int TableCount { get; init; }
    public List<FieldSummaryDto> Fields { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool RequiresManualReview { get; init; }
    public DateTime? AnalyzedAt { get; init; }
}
