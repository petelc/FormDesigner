namespace FormDesignerAPI.Web.CodeGeneration;

public class GetJobStatusResponse
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
    public string? ErrorMessage { get; set; }
    public int ArtifactCount { get; set; }
    public long? ZipFileSizeBytes { get; set; }
    public string? DownloadUrl { get; set; }
}
