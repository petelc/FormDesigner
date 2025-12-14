namespace FormDesignerAPI.Web.FormContext;

public class UploadPdfRequest
{
    public const string Route = "/api/forms/upload-pdf";
    public IFormFile? PdfFile { get; set; }
}
