namespace FormDesignerAPI.Web.ProjectContext;

public record GetProjectAnalysisStatusResponse
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalForms { get; init; }
    public int TotalCodeGenerationJobs { get; init; }
    public int CompletedJobs { get; init; }
    public int FailedJobs { get; init; }
    public DateTime? LastUpdated { get; init; }
    public bool IsComplete { get; init; }
    public bool HasErrors { get; init; }
    public List<string> Messages { get; init; } = new();
}
