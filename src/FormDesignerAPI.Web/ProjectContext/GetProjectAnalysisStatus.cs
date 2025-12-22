using FormDesignerAPI.UseCases.ProjectContext;
using FormDesignerAPI.UseCases.ProjectContext.GetById;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Get the analysis status of a project
/// </summary>
/// <remarks>
/// Returns the current status of the project including form count, 
/// code generation job status, and overall completion state.
/// </remarks>
public class GetProjectAnalysisStatus(IMediator _mediator, IUser _currentUser, ILogger<GetProjectAnalysisStatus> _logger)
    : Endpoint<GetProjectAnalysisStatusRequest, GetProjectAnalysisStatusResponse>
{
    public override void Configure()
    {
        Get(GetProjectAnalysisStatusRequest.Route);

        Summary(s =>
        {
            s.Summary = "Get project analysis status";
            s.Description = "Retrieves the current analysis status of a project including forms and code generation jobs";
            s.Responses[200] = "Analysis status retrieved successfully";
            s.Responses[401] = "Unauthorized - authentication required";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
        GetProjectAnalysisStatusRequest request,
        CancellationToken cancellationToken)
    {
        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Getting analysis status for project {ProjectId}", request.ProjectId);

        // Get the project
        var projectQuery = new GetProjectByIdQuery(request.ProjectId);
        var projectResult = await _mediator.Send(projectQuery, cancellationToken);

        if (!projectResult.IsSuccess || projectResult.Value == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var project = projectResult.Value;

        // Build status response
        var messages = new List<string>();
        var isComplete = false;
        var hasErrors = false;

        // Determine completion and error states based on status
        switch (project.Status.ToString())
        {
            case "COMPLETED":
                isComplete = true;
                messages.Add("Project analysis and code generation completed successfully");
                break;
            case "FAILED":
                hasErrors = true;
                messages.Add("Project analysis or code generation failed");
                break;
            case "CODE_GENERATED":
                messages.Add("Code generation completed, awaiting final review");
                break;
            case "GENERATING_CODE":
                messages.Add("Code generation in progress");
                break;
            case "STRUCTURE_REVIEWED":
                messages.Add("Structure reviewed, ready for code generation");
                break;
            case "ANALYSING_COMPLETE":
                messages.Add("Analysis complete, awaiting structure review");
                break;
            case "ANALYZING":
                messages.Add("Form analysis in progress");
                break;
            case "PDF_UPLOADED":
                messages.Add("PDF uploaded, awaiting analysis");
                break;
            case "DRAFT":
                messages.Add("Project in draft state");
                break;
        }

        // Count job statuses if we have a detailed project
        var failedJobCount = 0;
        var completedJobCount = 0;

        if (project is ProjectDetailDTO detailedProject)
        {
            completedJobCount = detailedProject.CompletedJobs;
            failedJobCount = detailedProject.CodeGenerationJobs.Count(j => j.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));
        }

        Response = new GetProjectAnalysisStatusResponse
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = project.Status,
            TotalForms = project.FormIds.Count,
            TotalCodeGenerationJobs = project.CodeGenerationJobIds.Count,
            CompletedJobs = completedJobCount,
            FailedJobs = failedJobCount,
            LastUpdated = project.UpdatedAt,
            IsComplete = isComplete,
            HasErrors = hasErrors,
            Messages = messages
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
