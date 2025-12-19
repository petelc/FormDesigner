using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Web.CodeGeneration;

/// <summary>
/// Get the status of a code generation job
/// </summary>
public class GetJobStatus(
    ICodeGenerationJobRepository _jobRepository,
    ILogger<GetJobStatus> _logger)
    : Endpoint<GetJobStatusRequest, GetJobStatusResponse>
{
    public override void Configure()
    {
        Get(GetJobStatusRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get code generation job status";
            s.Description = "Returns the current status of a code generation job";
            s.Responses[200] = "Job status retrieved successfully";
            s.Responses[404] = "Job not found";
        });
    }

    public override async Task HandleAsync(
        GetJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting job status for JobId: {JobId}", request.JobId);

        var job = await _jobRepository.GetByIdAsync(
            request.JobId,
            cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Job not found with JobId: {JobId}", request.JobId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Job found: {JobId}, Status: {Status}", job.Id, job.Status);

        Response = new GetJobStatusResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            RequestedAt = job.RequestedAt,
            CompletedAt = job.CompletedAt,
            ProcessingDuration = job.ProcessingDuration,
            ErrorMessage = job.ErrorMessage,
            ArtifactCount = job.ArtifactCount,
            ZipFileSizeBytes = job.ZipFileSizeBytes,
            DownloadUrl = job.Status == JobStatus.Completed
                ? $"/api/code-generation/{job.Id}/download"
                : null
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
