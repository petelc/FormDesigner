using FormDesignerAPI.UseCases.Commands.AnalyzeForm;

namespace FormDesignerAPI.Web.FormContext;

/// <summary>
/// Upload and analyze a PDF form
/// </summary>
/// <remarks>
/// Uploads a PDF form, extracts its structure using Document Intelligence,
/// and creates a Form aggregate in the system.
/// </remarks>
public class UploadPdf(IMediator _mediator)
    : Endpoint<UploadPdfRequest, UploadPdfResponse>
{
    public override void Configure()
    {
        Post(UploadPdfRequest.Route);
        AllowAnonymous();
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Upload and analyze a PDF form";
            s.Description = "Uploads a PDF form, extracts its structure, and creates a Form in the system";
            s.Responses[200] = "Form successfully analyzed and created";
            s.Responses[400] = "Invalid PDF or validation error";
        });
    }

    public override async Task HandleAsync(
        UploadPdfRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PdfFile == null)
        {
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        // Open the uploaded PDF file as a stream
        await using var pdfStream = request.PdfFile.OpenReadStream();

        // Send command to analyze the PDF
        var command = new AnalyzeFormCommand(pdfStream, request.PdfFile.FileName);
        var result = await _mediator.Send(command, cancellationToken);

        // Map result to response
        Response = new UploadPdfResponse
        {
            FormId = result.FormId,
            FileName = result.FileName,
            FormType = result.FormType,
            FieldCount = result.FieldCount,
            TableCount = result.TableCount,
            Warnings = result.Warnings,
            RequiresManualReview = result.RequiresManualReview
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
