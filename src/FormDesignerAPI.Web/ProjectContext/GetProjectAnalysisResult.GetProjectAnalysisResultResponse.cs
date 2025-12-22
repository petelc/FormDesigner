using FormDesignerAPI.UseCases.Commands.AnalyzeProject;

namespace FormDesignerAPI.Web.ProjectContext;

public record GetProjectAnalysisResultResponse
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalForms { get; init; }
    public List<FormAnalysisDto> Forms { get; init; } = new();
}
