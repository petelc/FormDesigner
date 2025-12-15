namespace FormDesignerAPI.Web.CodeGeneration;

public class GetJobStatusRequest
{
    public const string Route = "/api/code-generation/{JobId}/status";

    public static string BuildRoute(Guid jobId) => Route.Replace("{JobId}", jobId.ToString());

    public Guid JobId { get; set; }
}
