using FormDesignerAPI.UseCases.Commands.AnalyzeProject;

namespace FormDesignerAPI.Web.ProjectContext;

public record AnalyzeProjectResponse
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int FormsToAnalyze { get; init; }
    public bool AnalysisStarted { get; init; }
    public List<string> Messages { get; init; } = new();
    public List<FormAnalysisDto> Forms { get; init; } = new();
}
