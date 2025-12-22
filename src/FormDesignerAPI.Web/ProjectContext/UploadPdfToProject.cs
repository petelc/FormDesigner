using FormDesignerAPI.UseCases.Commands.AnalyzeForm;
using FormDesignerAPI.UseCases.ProjectContext.AddPdf;
using FormDesignerAPI.UseCases.ProjectContext.GetById;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Upload and analyze a PDF form for a specific project
/// </summary>
/// <remarks>
/// Uploads a PDF form, extracts its structure using Document Intelligence,
/// creates a Form aggregate, and associates it with the specified project.
/// </remarks>
public class UploadPdfToProject(IMediator _mediator, IUser _currentUser, ILogger<UploadPdfToProject> _logger)
    : Endpoint<UploadPdfToProjectRequest, UploadPdfToProjectResponse>
{
    public override void Configure()
    {
        Post(UploadPdfToProjectRequest.Route);
        AllowFileUploads();
        Options(x => x.Accepts<UploadPdfToProjectRequest>("multipart/form-data"));
        Summary(s =>
        {
            s.Summary = "Upload and analyze a PDF form for a project";
            s.Description = "Uploads a PDF form, extracts its structure, creates a Form, and associates it with the project";
            s.Responses[200] = "Form successfully analyzed, created, and added to project";
            s.Responses[400] = "Invalid PDF or validation error";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
        UploadPdfToProjectRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PdfFile == null)
        {
            AddError("PDF file is required");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        // Verify the project exists
        _logger.LogInformation("Verifying project {ProjectId} exists", request.ProjectId);
        var projectQuery = new GetProjectByIdQuery(request.ProjectId);
        var projectResult = await _mediator.Send(projectQuery, cancellationToken);

        if (projectResult == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Uploading PDF {FileName} to project {ProjectId}",
            request.PdfFile.FileName, request.ProjectId);

        // Open the uploaded PDF file as a stream
        await using var pdfStream = request.PdfFile.OpenReadStream();

        // Send command to analyze the PDF
        var command = new AnalyzeFormCommand(pdfStream, request.PdfFile.FileName);
        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Form {FormId} created from PDF {FileName} for project {ProjectId}",
            result.FormId, result.FileName, request.ProjectId);

        // Associate the form with the project and update status to PDF_UPLOADED
        await _mediator.Send(new AddFormToProjectCommand(request.ProjectId, result.FormId, _currentUser.Id!), cancellationToken);

        // Map result to response
        Response = new UploadPdfToProjectResponse
        {
            ProjectId = request.ProjectId,
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
