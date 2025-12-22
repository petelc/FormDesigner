using FormDesignerAPI.Core.ProjectContext.ValueObjects;
using FormDesignerAPI.UseCases.Commands.AnalyzeProject;
using FormDesignerAPI.UseCases.ProjectContext.GetById;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Trigger analysis for a project
/// </summary>
/// <remarks>
/// Initiates the analysis process for all forms in the project.
/// Updates the project status to ANALYZING.
/// </remarks>
public class AnalyzeProject(IMediator _mediator, IUser _currentUser, ILogger<AnalyzeProject> _logger)
    : Endpoint<AnalyzeProjectRequest, AnalyzeProjectResponse>
{
    public override void Configure()
    {
        Post(AnalyzeProjectRequest.Route);

        Summary(s =>
        {
            s.Summary = "Trigger project analysis";
            s.Description = "Initiates the analysis process for all forms in the project";
            s.Responses[200] = "Analysis started successfully";
            s.Responses[400] = "Invalid request or project not ready for analysis";
            s.Responses[401] = "Unauthorized - authentication required";
            s.Responses[404] = "Project not found";
        });
    }

    public override async Task HandleAsync(
        AnalyzeProjectRequest request,
        CancellationToken cancellationToken)
    {
        // Check if user is authenticated
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.Id))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Starting analysis for project {ProjectId} by user {UserId}",
            request.ProjectId, _currentUser.Id);

        // Get the project
        var projectQuery = new GetProjectByIdQuery(request.ProjectId);
        var project = await _mediator.Send(projectQuery, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var messages = new List<string>();

        // Validate project is in correct state for analysis
        if (project.Value.Status == ProjectStatus.DRAFT.ToString())
        {
            AddError("Cannot analyze project in DRAFT status. Please upload a PDF first.");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        if (project.Value.Status == ProjectStatus.ANALYZING.ToString())
        {
            AddError("Project is already being analyzed");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        if (project.Value.FormIds?.Count == 0 || project.Value.FormIds == null)
        {
            AddError("No forms found in project. Please upload a PDF first.");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        // Send command to start analysis
        var command = new AnalyzeProjectCommand(request.ProjectId, _currentUser.Id!);

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Analysis initiated for project {ProjectId} with {FormCount} forms",
                request.ProjectId, result.FormsToAnalyze);

            Response = new AnalyzeProjectResponse
            {
                ProjectId = result.ProjectId,
                ProjectName = result.ProjectName,
                Status = result.Status,
                FormsToAnalyze = result.FormsToAnalyze,
                AnalysisStarted = result.AnalysisStarted,
                Messages = result.Messages,
                Forms = result.Forms
            };

            await SendOkAsync(Response, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to start analysis for project {ProjectId}", request.ProjectId);
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: cancellationToken);
        }
    }
}
