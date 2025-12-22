using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Services;
using FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;
using FormDesignerAPI.UseCases.ProjectContext.GetById;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.Web.ProjectContext;

/// <summary>
/// Generate code artifacts for all forms in a project
/// </summary>
/// <remarks>
/// Takes a project ID and generation options, generates code artifacts (C#, SQL, React) 
/// for all forms in the project, and returns the generated files organized by category.
/// Also provides a download URL for the zip file containing all generated code.
/// </remarks>
public class GenerateProjectCode(
    IMediator _mediator,
    IFormRepository _formRepository,
    ICodeGenerationJobRepository _jobRepository,
    CodeGenerationOrchestrator _orchestrator,
    IUser _currentUser,
    ILogger<GenerateProjectCode> _logger)
    : Endpoint<GenerateProjectCodeRequest, GenerateProjectCodeResponse>
{
    public override void Configure()
    {
        Post(GenerateProjectCodeRequest.Route);

        Summary(s =>
        {
            s.Summary = "Generate code artifacts for all forms in a project";
            s.Description = "Generates C#, SQL, React, tests, and documentation for all forms in the project";
            s.Responses[200] = "Code generation completed successfully";
            s.Responses[404] = "Project not found";
            s.Responses[400] = "Invalid request or project has no forms";
        });
    }

    public override async Task HandleAsync(
        GenerateProjectCodeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating code for project: {ProjectId}", request.ProjectId);

        // Get the project
        var projectQuery = new GetProjectByIdQuery(request.ProjectId);
        var project = await _mediator.Send(projectQuery, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project not found: {ProjectId}", request.ProjectId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (project.Value.FormIds.Count == 0)
        {
            _logger.LogWarning("Project {ProjectId} has no forms", request.ProjectId);
            AddError("Project has no forms to generate code from");
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var filesStructure = new GeneratedFilesStructure();
        var formsGenerated = 0;
        var requestedBy = _currentUser.Id ?? "system";
        var jobIds = new List<Guid>();

        // Generate code for each form
        foreach (var formId in project.Value.FormIds)
        {
            var form = await _formRepository.GetByIdWithRevisionsAsync(formId, cancellationToken);

            if (form == null)
            {
                _logger.LogWarning("Form {FormId} not found in project {ProjectId}", formId, request.ProjectId);
                continue;
            }

            var currentRevision = form.CurrentRevision;
            var formDefinition = currentRevision.Definition;

            _logger.LogInformation(
                "Generating code for form: {FormId}, Name: {FormName}, Fields: {FieldCount}",
                form.Id,
                form.Name,
                formDefinition.Fields.Count);

            // Generate code using the orchestrator
            var job = await _orchestrator.GenerateAsync(
                form.Id,
                currentRevision.Id,
                formDefinition,
                request.Options,
                requestedBy,
                cancellationToken);

            // Save the job to enable zip download
            await _jobRepository.AddAsync(job, cancellationToken);
            await _jobRepository.SaveChangesAsync(cancellationToken);
            jobIds.Add(job.Id);

            // Organize artifacts into file structure
            foreach (var artifact in job.Artifacts)
            {
                var fileName = Path.GetFileName(artifact.FilePath);

                var file = new GeneratedFile
                {
                    FileName = fileName,
                    FilePath = artifact.FilePath,
                    Content = artifact.Content,
                    Language = DetermineLanguage(artifact.Type)
                };

                // Categorize by artifact type
                switch (artifact.Type)
                {
                    case ArtifactType.CSharpEntity:
                    case ArtifactType.CSharpInterface:
                    case ArtifactType.CSharpDto:
                    case ArtifactType.CSharpAutoMapper:
                    case ArtifactType.CSharpValidation:
                    case ArtifactType.CSharpRepository:
                    case ArtifactType.CSharpController:
                        filesStructure.Backend.Add(file);
                        break;

                    case ArtifactType.SqlCreateTable:
                    case ArtifactType.SqlStoredProcedures:
                        filesStructure.Sql.Add(file);
                        break;

                    case ArtifactType.ReactComponent:
                    case ArtifactType.ReactValidation:
                        filesStructure.Frontend.Add(file);
                        break;

                    case ArtifactType.CSharpUnitTests:
                    case ArtifactType.CSharpIntegrationTests:
                        filesStructure.Tests.Add(file);
                        break;

                    case ArtifactType.GitHubActions:
                    case ArtifactType.AzurePipeline:
                    case ArtifactType.Dockerfile:
                        filesStructure.Docs.Add(file);
                        break;
                }
            }

            formsGenerated++;
        }

        _logger.LogInformation(
            "Code generation completed for project {ProjectId}. Forms processed: {FormsGenerated}, Total files: {FileCount}",
            request.ProjectId,
            formsGenerated,
            filesStructure.Backend.Count + filesStructure.Frontend.Count + filesStructure.Sql.Count +
            filesStructure.Tests.Count + filesStructure.Docs.Count);

        Response = new GenerateProjectCodeResponse
        {
            ProjectId = project.Value.Id,
            ProjectName = project.Value.Name,
            FormsGenerated = formsGenerated,
            Files = filesStructure,
            DownloadUrls = jobIds.Select(jobId => $"/api/code-generation/{jobId}/download").ToList(),
            Message = $"Successfully generated code for {formsGenerated} form(s) in project '{project.Value.Name}'. Use download URLs to get zip files for each form."
        };

        await SendOkAsync(Response, cancellationToken);
    }

    private static string DetermineLanguage(ArtifactType artifactType)
    {
        return artifactType switch
        {
            ArtifactType.CSharpEntity => "csharp",
            ArtifactType.CSharpInterface => "csharp",
            ArtifactType.CSharpDto => "csharp",
            ArtifactType.CSharpAutoMapper => "csharp",
            ArtifactType.CSharpValidation => "csharp",
            ArtifactType.CSharpRepository => "csharp",
            ArtifactType.CSharpController => "csharp",
            ArtifactType.CSharpUnitTests => "csharp",
            ArtifactType.CSharpIntegrationTests => "csharp",
            ArtifactType.SqlCreateTable => "sql",
            ArtifactType.SqlStoredProcedures => "sql",
            ArtifactType.ReactComponent => "typescript",
            ArtifactType.ReactValidation => "typescript",
            ArtifactType.GitHubActions => "yaml",
            ArtifactType.AzurePipeline => "yaml",
            ArtifactType.Dockerfile => "dockerfile",
            _ => "text"
        };
    }
}
