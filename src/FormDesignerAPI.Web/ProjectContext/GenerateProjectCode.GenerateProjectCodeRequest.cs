using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

namespace FormDesignerAPI.Web.ProjectContext;

public class GenerateProjectCodeRequest
{
    public const string Route = "/api/projects/{ProjectId}/generate-code";

    public static string BuildRoute(Guid projectId) => Route.Replace("{ProjectId}", projectId.ToString());

    public Guid ProjectId { get; set; }
    public GenerationOptions Options { get; set; } = new();
}
