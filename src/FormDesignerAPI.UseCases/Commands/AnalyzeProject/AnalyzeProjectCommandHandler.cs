using FormDesignerAPI.Core.ProjectContext.Interfaces;
using FormDesignerAPI.Core.ProjectContext.ValueObjects;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.UseCases.Commands.AnalyzeForm;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeProject;

/// <summary>
/// Handler for AnalyzeProjectCommand
/// </summary>
public class AnalyzeProjectCommandHandler : IRequestHandler<AnalyzeProjectCommand, AnalyzeProjectResult>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IFormRepository _formRepository;
    private readonly ILogger<AnalyzeProjectCommandHandler> _logger;

    public AnalyzeProjectCommandHandler(
        IProjectRepository projectRepository,
        IFormRepository formRepository,
        ILogger<AnalyzeProjectCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _formRepository = formRepository;
        _logger = logger;
    }

    public async Task<AnalyzeProjectResult> Handle(
        AnalyzeProjectCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting analysis for project {ProjectId} by user {UserId}",
            request.ProjectId, request.UserId);

        // Get the project
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found", request.ProjectId);
            throw new InvalidOperationException($"Project {request.ProjectId} not found");
        }

        var messages = new List<string>();

        // Validate project state
        if (project.Status == ProjectStatus.DRAFT)
        {
            _logger.LogWarning("Cannot analyze project {ProjectId} in DRAFT status", request.ProjectId);
            throw new InvalidOperationException("Cannot analyze project in DRAFT status. Please upload a PDF first.");
        }

        if (project.Status == ProjectStatus.ANALYZING)
        {
            _logger.LogWarning("Project {ProjectId} is already being analyzed", request.ProjectId);
            throw new InvalidOperationException("Project is already being analyzed");
        }

        if (project.FormIds.Count == 0)
        {
            _logger.LogWarning("Project {ProjectId} has no forms to analyze", request.ProjectId);
            throw new InvalidOperationException("No forms found in project. Please upload a PDF first.");
        }

        // Mark project as analyzing
        project.MarkAnalyzing(request.UserId);
        messages.Add($"Analysis started for {project.FormIds.Count} form(s)");
        messages.Add("Project status updated to ANALYZING");

        // Since forms are already analyzed during PDF upload via AnalyzeFormCommand,
        // we can immediately mark the analysis as complete
        project.MarkAnalysisComplete(request.UserId);
        messages.Add("All forms have been analyzed");
        messages.Add("Project status updated to ANALYSING_COMPLETE");

        await _projectRepository.UpdateAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} analysis completed with {FormCount} forms",
            request.ProjectId, project.FormIds.Count);

        // Retrieve all forms and their analysis details
        var formAnalysisResults = new List<FormAnalysisDto>();
        foreach (var formId in project.FormIds)
        {
            var form = await _formRepository.GetByIdWithRevisionsAsync(formId, cancellationToken);
            if (form != null)
            {
                formAnalysisResults.Add(new FormAnalysisDto
                {
                    FormId = form.Id,
                    FileName = form.Origin.ReferenceId ?? form.Name,
                    FormType = "PDF Form", // Form type is not stored in the aggregate, only in analysis result
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
                    Warnings = new List<string>(), // Could be stored in Form if needed
                    RequiresManualReview = false, // Could be stored in Form if needed
                    AnalyzedAt = form.CurrentRevision.CreatedAt
                });
            }
        }

        _logger.LogInformation("Retrieved analysis details for {FormCount} forms in project {ProjectId}",
            formAnalysisResults.Count, request.ProjectId);

        // TODO: If you need to re-analyze forms or perform additional processing,
        // implement a background job queue here to process each form asynchronously

        return new AnalyzeProjectResult
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = project.Status.ToString(),
            FormsToAnalyze = project.FormIds.Count,
            AnalysisStarted = true,
            Messages = messages,
            Forms = formAnalysisResults
        };
    }
}
