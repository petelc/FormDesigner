using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.UseCases.Commands.AnalyzeForm;
using FormDesignerAPI.UseCases.Commands.AnalyzeProject;
using FormDesignerAPI.UseCases.ProjectContext.GetById;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Get analysis results for a project
/// </summary>
/// <remarks>
/// Retrieves the document intelligence analysis results for all forms in the project.
/// </remarks>
public class GetProjectAnalysisResult(
    IMediator _mediator,
    IFormRepository _formRepository,
    ILogger<GetProjectAnalysisResult> _logger)
    : Endpoint<GetProjectAnalysisResultRequest, GetProjectAnalysisResultResponse>
{
    public override void Configure()
    {
        Get(GetProjectAnalysisResultRequest.Route);

        Summary(s =>
        {
            s.Summary = "Get project analysis results";
            s.Description = "Retrieves the document intelligence analysis results for all forms in the project";
            s.Responses[200] = "Analysis results retrieved successfully";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
        GetProjectAnalysisResultRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting analysis results for project {ProjectId}", request.ProjectId);

        // Get the project
        var projectQuery = new GetProjectByIdQuery(request.ProjectId);
        var project = await _mediator.Send(projectQuery, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        // Retrieve all forms and their analysis details
        var formAnalysisResults = new List<FormAnalysisDto>();
        foreach (var formId in project.Value.FormIds)
        {
            var form = await _formRepository.GetByIdWithRevisionsAsync(formId, cancellationToken);
            if (form != null)
            {
                formAnalysisResults.Add(new FormAnalysisDto
                {
                    FormId = form.Id,
                    FileName = form.Origin.ReferenceId ?? form.Name,
                    FormType = "PDF Form",
                    FieldCount = form.FieldCount,
                    TableCount = form.Definition?.Fields?.Count(f => f.Type == "table") ?? 0,
                    Fields = form.Definition?.Fields?.Select(f => new FieldSummaryDto
                    {
                        Name = f.Name,
                        Label = f.Label ?? f.Name,
                        Type = f.Type,
                        IsRequired = f.Required,
                        HasOptions = f.Options?.Any() ?? false,
                        HasValidation = !string.IsNullOrEmpty(f.Pattern),
                        MaxLength = f.MaxLength
                    }).ToList() ?? new List<FieldSummaryDto>(),
                    Warnings = new List<string>(),
                    RequiresManualReview = false,
                    AnalyzedAt = form.CurrentRevision.CreatedAt
                });
            }
        }

        _logger.LogInformation("Retrieved analysis results for {FormCount} forms in project {ProjectId}",
            formAnalysisResults.Count, request.ProjectId);

        Response = new GetProjectAnalysisResultResponse
        {
            ProjectId = project.Value.Id,
            ProjectName = project.Value.Name,
            Status = project.Value.Status.ToString(),
            TotalForms = project.Value.FormIds.Count,
            Forms = formAnalysisResults
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
