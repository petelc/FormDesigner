using Traxs.SharedKernel;
using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;

namespace FormDesignerAPI.Core.CodeGenerationContext.Interfaces;

/// <summary>
/// Repository interface for CodeGenerationJob aggregate
/// </summary>
public interface ICodeGenerationJobRepository : IRepository<CodeGenerationJob>
{
    /// <summary>
    /// Get job with all artifacts eagerly loaded
    /// </summary>
    Task<CodeGenerationJob?> GetByIdWithArtifactsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all jobs for a specific form definition
    /// </summary>
    Task<List<CodeGenerationJob>> GetJobsByFormDefinitionIdAsync(
        Guid formDefinitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent jobs (limited to specific number)
    /// </summary>
    Task<List<CodeGenerationJob>> GetRecentJobsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}
