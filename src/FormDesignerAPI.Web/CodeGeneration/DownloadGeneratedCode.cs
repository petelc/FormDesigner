using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;

namespace FormDesignerAPI.Web.CodeGeneration;

/// <summary>
/// Download generated code artifacts as a ZIP file
/// </summary>
public class DownloadGeneratedCode(ICodeGenerationJobRepository _jobRepository)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get(DownloadRequest.Route);
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Download generated code artifacts";
            s.Description = "Downloads the ZIP file containing all generated code artifacts";
            s.Responses[200] = "ZIP file download";
            s.Responses[404] = "Job not found or not completed";
            s.Responses[400] = "Job not completed or failed";
        });
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var jobIdStr = Route<string>("JobId");
        if (!Guid.TryParse(jobIdStr, out var jobId))
        {
            ThrowError("Invalid job ID");
            return;
        }

        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (job == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        if (job.Status != JobStatus.Completed)
        {
            ThrowError($"Job is not completed. Current status: {job.Status}");
            return;
        }

        if (string.IsNullOrEmpty(job.ZipFilePath) || !File.Exists(job.ZipFilePath))
        {
            ThrowError("Generated ZIP file not found");
            return;
        }

        // Read the ZIP file
        var fileBytes = await File.ReadAllBytesAsync(job.ZipFilePath, cancellationToken);

        // Generate a friendly filename
        var fileName = $"generated-code-{job.Id}.zip";

        // Send the file
        await SendBytesAsync(
            fileBytes,
            fileName,
            contentType: "application/zip",
            cancellation: cancellationToken);
    }
}
