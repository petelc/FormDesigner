namespace FormDesignerAPI.Web.CodeGeneration;

public class GenerateCodeResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
}
