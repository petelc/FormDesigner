namespace FormDesignerAPI.Web.CodeGeneration;

public class DownloadRequest
{
    public const string Route = "/api/code-generation/{JobId}/download";

    public static string BuildRoute(Guid jobId) => Route.Replace("{JobId}", jobId.ToString());

    public Guid JobId { get; set; }
}
