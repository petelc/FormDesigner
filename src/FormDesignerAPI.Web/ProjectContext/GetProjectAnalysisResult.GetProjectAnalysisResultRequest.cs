namespace FormDesignerAPI.Web.ProjectContext;

public record GetProjectAnalysisResultRequest
{
    public const string Route = "/api/projects/{projectId}/analysis-result";
    public Guid ProjectId { get; init; }
}
