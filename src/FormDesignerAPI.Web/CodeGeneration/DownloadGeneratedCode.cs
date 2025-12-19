using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;
using Microsoft.Extensions.Logging;

namespace FormDesignerAPI.Web.CodeGeneration;

/// <summary>
/// Download generated code artifacts as a ZIP file
/// </summary>
public class DownloadGeneratedCode(
    ICodeGenerationJobRepository _jobRepository,
    ILogger<DownloadGeneratedCode> _logger)
    : Endpoint<DownloadRequest>
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

    public override async Task HandleAsync(DownloadRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Download request for JobId: {JobId}", request.JobId);

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Job not found with JobId: {JobId}", request.JobId);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        _logger.LogInformation("Job found: {JobId}, Status: {Status}, ZipPath: {ZipPath}", 
            job.Id, job.Status, job.ZipFilePath);

        if (job.Status != JobStatus.Completed)
        {
            ThrowError($"Job is not completed. Current status: {job.Status}");
            return;
        }

        if (string.IsNullOrEmpty(job.ZipFilePath) || !File.Exists(job.ZipFilePath))
        {
            _logger.LogError("ZIP file not found at path: {ZipPath}", job.ZipFilePath);
            ThrowError("Generated ZIP file not found");
            return;
        }

        _logger.LogInformation("Reading ZIP file: {ZipPath}", job.ZipFilePath);

        // Read the ZIP file
        var fileBytes = await File.ReadAllBytesAsync(job.ZipFilePath, cancellationToken);

        // Generate a friendly filename
        var fileName = $"generated-code-{job.Id}.zip";

        _logger.LogInformation("Sending ZIP file: {FileName}, Size: {Size} bytes", fileName, fileBytes.Length);

        // Send the file
        await SendBytesAsync(
            fileBytes,
            fileName,
            contentType: "application/zip",
            cancellation: cancellationToken);
    }
}
