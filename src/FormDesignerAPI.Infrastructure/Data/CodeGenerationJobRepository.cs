using FormDesignerAPI.Core.CodeGenerationContext.Aggregates;
using FormDesignerAPI.Core.CodeGenerationContext.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FormDesignerAPI.Infrastructure.Data;

/// <summary>
/// Repository implementation for CodeGenerationJob aggregate
/// </summary>
public class CodeGenerationJobRepository : EfRepository<CodeGenerationJob>, ICodeGenerationJobRepository
{
    private readonly AppDbContext _dbContext;

    public CodeGenerationJobRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

  public Task<CodeGenerationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return _dbContext.CodeGenerationJobs
        .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
  }

  /// <summary>
  /// Get all jobs for a specific form definition
  /// </summary>
  public async Task<List<CodeGenerationJob>> GetJobsByFormDefinitionIdAsync(
        Guid formDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CodeGenerationJobs
            .Where(j => j.FormDefinitionId == formDefinitionId)
            .OrderByDescending(j => j.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get recent jobs (limited to specific number)
    /// </summary>
    public async Task<List<CodeGenerationJob>> GetRecentJobsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CodeGenerationJobs
            .OrderByDescending(j => j.RequestedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
