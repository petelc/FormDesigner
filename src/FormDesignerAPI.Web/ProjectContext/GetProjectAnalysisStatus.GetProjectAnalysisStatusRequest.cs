namespace FormDesignerAPI.Web.ProjectContext;

public record GetProjectAnalysisStatusRequest
{
    public const string Route = "/api/projects/{projectId}/analysis-status";

    [BindFrom("projectId")]
    public Guid ProjectId { get; init; }
}
