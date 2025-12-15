using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;

namespace FormDesignerAPI.Web.CodeGeneration;

/// <summary>
/// Get the status of a code generation job
/// </summary>
public class GetJobStatus(ICodeGenerationJobRepository _jobRepository)
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
        var job = await _jobRepository.GetByIdWithArtifactsAsync(
            request.JobId,
            cancellationToken);

        if (job == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        Response = new GetJobStatusResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            RequestedAt = job.RequestedAt,
            CompletedAt = job.CompletedAt,
            ProcessingDuration = job.ProcessingDuration,
            ErrorMessage = job.ErrorMessage,
            ArtifactCount = job.Artifacts.Count,
            ZipFileSizeBytes = job.ZipFileSizeBytes,
            DownloadUrl = job.Status == JobStatus.Completed
                ? $"/api/code-generation/{job.Id}/download"
                : null
        };

        await SendOkAsync(Response, cancellationToken);
    }
}
