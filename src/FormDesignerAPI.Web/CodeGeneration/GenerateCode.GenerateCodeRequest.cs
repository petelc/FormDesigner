using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

namespace FormDesignerAPI.Web.CodeGeneration;

public class GenerateCodeRequest
{
    public const string Route = "/api/forms/{FormId}/generate-code";

    public static string BuildRoute(Guid formId) => Route.Replace("{FormId}", formId.ToString());

    public Guid FormId { get; set; }
    public GenerationOptions Options { get; set; } = new();
}
