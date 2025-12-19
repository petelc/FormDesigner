using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Services;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Web.CodeGeneration;

/// <summary>
/// Generate code artifacts from a form definition
/// </summary>
/// <remarks>
/// Takes a form ID and generation options, generates code artifacts (C#, SQL, React),
/// and returns a job ID that can be used to track status and download the generated code.
/// </remarks>
public class GenerateCode(
    IFormRepository _formRepository,
    ICodeGenerationJobRepository _jobRepository,
    CodeGenerationOrchestrator _orchestrator,
    ILogger<GenerateCode> _logger)
    : Endpoint<GenerateCodeRequest, GenerateCodeResponse>
{
    public override void Configure()
    {
        Post(GenerateCodeRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Generate code artifacts from a form";
            s.Description = "Generates C#, SQL, and React code from a form definition";
            s.Responses[200] = "Code generation job created successfully";
            s.Responses[404] = "Form not found";
            s.Responses[400] = "Invalid request";
        });
    }

    public override async Task HandleAsync(
        GenerateCodeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating code for form: {FormId}", request.FormId);

        // Retrieve the form with its revisions
        var form = await _formRepository.GetByIdWithRevisionsAsync(
            request.FormId,
            cancellationToken);

        if (form == null)
        {
            _logger.LogWarning("Form not found: {FormId}", request.FormId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        // Get the current revision (most recent)
        var currentRevision = form.CurrentRevision;

        // Get the form definition from the current revision
        var formDefinition = currentRevision.Definition;

        _logger.LogInformation(
            "Form loaded: {FormId}, Revision: {RevisionId}, Fields count: {FieldCount}",
            form.Id,
            currentRevision.Id,
            formDefinition.Fields.Count);

        if (formDefinition.Fields.Count == 0)
        {
            _logger.LogWarning(
                "Form definition has no fields! Schema length: {SchemaLength}",
                formDefinition.Schema?.Length ?? 0);
        }
        else
        {
            // Log first few field names for debugging
            var firstFields = formDefinition.Fields.Take(3).Select(f => f.Name);
            _logger.LogDebug(
                "First fields: {Fields}",
                string.Join(", ", firstFields));
        }

        // Get the current user (for now, use a placeholder)
        var requestedBy = "system"; // TODO: Replace with actual user from authentication

        // Generate code using the orchestrator
        var job = await _orchestrator.GenerateAsync(
            form.Id,
            currentRevision.Id,
            formDefinition,
            request.Options,
            requestedBy,
            cancellationToken);

        // Save the job
        await _jobRepository.AddAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Return response
        Response = new GenerateCodeResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            RequestedAt = job.RequestedAt,
            Message = "Code generation completed successfully",
            DownloadUrl = job.Status == Core.CodeGenerationContext.Aggregates.JobStatus.Completed
                ? $"/api/code-generation/{job.Id}/download"
                : null
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
