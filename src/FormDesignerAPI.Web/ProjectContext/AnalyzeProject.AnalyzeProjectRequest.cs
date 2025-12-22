namespace FormDesignerAPI.Web.ProjectContext;

public record AnalyzeProjectRequest
{
    public const string Route = "/api/projects/{projectId}/analyze";

    [BindFrom("projectId")]
    public Guid ProjectId { get; init; }
}
