namespace FormDesignerAPI.Web.ProjectContext;

public class UploadPdfToProjectRequest
{
    public const string Route = "/api/projects/{projectId}/upload-pdf";

    public Guid ProjectId { get; set; }
    public IFormFile? PdfFile { get; set; }
}
