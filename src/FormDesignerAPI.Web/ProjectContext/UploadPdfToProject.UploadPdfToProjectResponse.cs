namespace FormDesignerAPI.Web.ProjectContext;

public class UploadPdfToProjectResponse
{
    public Guid ProjectId { get; set; }
    public Guid FormId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public int TableCount { get; set; }
    public List<string> Warnings { get; set; } = new();
    public bool RequiresManualReview { get; set; }
}
